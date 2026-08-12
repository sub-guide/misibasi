using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 운구인: Extension Blend + JumpStart/JumpLand 연출 + 루트 Rigidbody 점프(Y).
    /// 발 Collider·RB 제약은 에디터. 지면 = <see cref="CoffinDanceFailFloor"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDancePallbearerPose : MonoBehaviour
    {
        public const string ExtensionParam = "Extension";
        public const string StateExtensionBlend = "ExtensionBlend";
        public const string StateJumpStart = "JumpStart";
        /// <summary>Jump_Start 클립 Speed=0 · 페이드 목표(첫 프레임 고정).</summary>
        public const string StateJumpStartHold = "JumpStartHold";
        public const string StateJumpLand = "JumpLand";

        [Header("Animator")]
        [Tooltip("비우면 같은 GameObject에서 GetComponent.")]
        [SerializeField] Animator animator;

        [Header("물리")]
        [Tooltip("비우면 같은 GameObject에서 GetComponent. 제약은 에디터 설정 유지.")]
        [SerializeField] Rigidbody body;

        Vector3 _restLocalPos;
        Quaternion _restLocalRot;
        bool _cachedRest;
        bool _grounded;
        bool _leftGroundSinceJump;
        bool _landYOffsetActive;
        bool _landYLerping;
        float _landYLerpFrom;
        float _landYLerpTo;
        float _landYLerpElapsed;
        float _landYLerpDuration;

        public float Extension { get; private set; } = 1f;
        public bool IsGrounded => _grounded;
        public bool LeftGroundSinceJump => _leftGroundSinceJump;

        void Awake()
        {
            EnsureRefs();
            CacheRest(force: true);
            if (animator != null)
                animator.applyRootMotion = false;

            Extension = 1f;
            SetSimulationActive(false);
        }

        public void PrepareForGameplay()
        {
            EnsureRefs();
            // SoftReset 시 rest를 덮어쓰지 않음 — Begin 첫 캐시만 유지
            if (!_cachedRest)
                CacheRest(force: true);

            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.speed = 1f;
            }

            EnsurePhysicsConfigured();
            SoftResetPhysics();
            ResetPoseAnim();
            SetSimulationActive(true);
            _grounded = false;
            _leftGroundSinceJump = false;
        }

        public void ResetPoseAnim()
        {
            Extension = 1f;
            EnsureRefs();
            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                animator.speed = 1f;
                animator.SetFloat(ExtensionParam, Extension);
                animator.Play(StateExtensionBlend, 0, 0f);
            }
        }

        public void SetExtension(float extension)
        {
            Extension = Mathf.Clamp01(extension);
            if (animator != null &&
                animator.isActiveAndEnabled &&
                animator.runtimeAnimatorController != null)
            {
                animator.SetFloat(ExtensionParam, Extension);
            }
        }

        public void SetSimulationActive(bool active)
        {
            EnsureRefs();
            if (body == null)
                return;

            if (!active)
            {
                // kinematic이면 velocity 설정 불가 → 먼저 속도 클리어 후 kinematic
                if (!body.isKinematic)
                {
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }

                body.isKinematic = true;
            }
            else
            {
                body.isKinematic = false;
            }
        }

        /// <summary>점프 시작: 지면 이탈 감지 리셋 + 위쪽 Impulse. 제약은 건드리지 않음.</summary>
        public void BeginJumpImpulse(float impulseY)
        {
            EnsureRefs();
            EnsurePhysicsConfigured();
            _leftGroundSinceJump = false;
            _grounded = false;

            if (body == null || body.isKinematic)
                return;

            Vector3 v = body.velocity;
            v.y = 0f;
            body.velocity = v;
            body.AddForce(Vector3.up * Mathf.Max(0f, impulseY), ForceMode.VelocityChange);
        }

        /// <summary>
        /// 지상 페이드: 버튼 시점 현재 프레임 → JumpStart 첫 프레임(JumpStartHold, Speed=0).
        /// Impulse 없음. animator.speed=1로 벽시계 blendSeconds 동안 CrossFade.
        /// </summary>
        public void BeginJumpStartBlend(float blendSeconds)
        {
            EnsureRefs();
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                return;

            float blend = Mathf.Max(0f, blendSeconds);
            animator.speed = 1f;

            // 버튼 시점 프레임을 소스 시작점으로 고정한 뒤 Hold(첫 프레임)로 페이드
            AnimatorStateInfo cur = animator.GetCurrentAnimatorStateInfo(0);
            if (!animator.IsInTransition(0))
            {
                animator.Play(cur.fullPathHash, 0, cur.normalizedTime);
                animator.Update(0f);
            }

            if (blend <= 0.0001f)
            {
                animator.Play(StateJumpStartHold, 0, 0f);
                animator.Update(0f);
            }
            else
            {
                // JumpStartHold = 동일 클립 · State Speed 0 → 목표가 첫 프레임에 고정
                animator.CrossFadeInFixedTime(StateJumpStartHold, blend, 0, 0f);
            }
        }

        /// <summary>
        /// 페이드 종료 후: JumpStart를 첫 프레임부터 재생 + 물리 Impulse (동시).
        /// </summary>
        public void BeginJumpWithMotion(float impulseY, float animSpeed)
        {
            EnsureRefs();
            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                animator.speed = Mathf.Max(0.01f, animSpeed);
                animator.Play(StateJumpStart, 0, 0f);
                animator.Update(0f);
            }

            BeginJumpImpulse(impulseY);
        }

        public void PlayJumpStart(float animSpeed, float blendSeconds)
        {
            CrossFadeTo(StateJumpStart, animSpeed, blendSeconds);
        }

        public void PlayJumpLand(float animSpeed, float blendSeconds)
        {
            CrossFadeTo(StateJumpLand, animSpeed, blendSeconds);
        }

        /// <summary>
        /// Land Y 오프셋으로 점진 이동 시작. duration≤0이면 즉시 스냅.
        /// 종료 시 ClearLandYOffset은 기존처럼 rest로 즉시 복귀.
        /// </summary>
        public void BeginLandYOffsetLerp(float offset, float duration)
        {
            EnsureRefs();
            CacheRest();
            _landYLerpFrom = transform.localPosition.y;
            _landYLerpTo = _restLocalPos.y + offset;
            _landYLerpDuration = Mathf.Max(0f, duration);
            _landYLerpElapsed = 0f;
            _landYOffsetActive = true;

            if (_landYLerpDuration <= 0.0001f)
            {
                ApplyLocalY(_landYLerpTo);
                _landYLerping = false;
            }
            else
            {
                _landYLerping = true;
                ApplyLocalY(_landYLerpFrom);
            }
        }

        /// <summary>Land Y 점진 이동 틱. 끝나면 더 이상 Y를 쓰지 않음(중력 재개).</summary>
        public void TickLandYOffsetLerp(float dt)
        {
            if (!_landYLerping)
                return;

            _landYLerpElapsed += Mathf.Max(0f, dt);
            float t = Mathf.Clamp01(_landYLerpElapsed / _landYLerpDuration);
            ApplyLocalY(Mathf.Lerp(_landYLerpFrom, _landYLerpTo, t));
            if (t >= 1f)
                _landYLerping = false;
        }

        /// <summary>즉시 스냅(레거시). 점진 이동은 BeginLandYOffsetLerp 사용.</summary>
        public void SetLandYOffset(float offset)
        {
            BeginLandYOffsetLerp(offset, 0f);
        }

        /// <summary>Land 오프셋 해제 시 rest Y로 즉시 복귀. 미적용이면 무시.</summary>
        public void ClearLandYOffset()
        {
            if (!_landYOffsetActive && !_landYLerping)
                return;

            EnsureRefs();
            CacheRest();
            _landYLerping = false;
            ApplyLocalY(_restLocalPos.y);
            _landYOffsetActive = false;
        }

        public void PlayExtensionBlend(float blendSeconds)
        {
            ClearLandYOffset();
            EnsureRefs();
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                return;

            animator.speed = 1f;
            animator.SetFloat(ExtensionParam, Extension);
            CrossFadeTo(StateExtensionBlend, 1f, blendSeconds);
        }

        public bool HasAnimStateCompleted(string stateName)
        {
            EnsureRefs();
            if (animator == null || !animator.isActiveAndEnabled)
                return true;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (animator.IsInTransition(0))
                return false;

            if (!info.IsName(stateName))
                return false;

            return info.normalizedTime >= 1f;
        }

        /// <summary>현재 재생 배속을 반영한 클립 길이(초).</summary>
        public float GetPlayingStateDurationOr(string stateName, float fallback, float animSpeed)
        {
            EnsureRefs();
            float speed = Mathf.Max(0.01f, animSpeed);
            if (animator == null || animator.runtimeAnimatorController == null)
                return Mathf.Max(0.01f, fallback) / speed;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName) && info.length > 0.01f)
                return info.length / speed;

            return Mathf.Max(0.01f, fallback) / speed;
        }

        public void SoftResetPhysics()
        {
            EnsureRefs();
            EnsurePhysicsConfigured();
            CacheRest();

            if (body == null)
            {
                transform.localPosition = _restLocalPos;
                transform.localRotation = _restLocalRot;
                _landYOffsetActive = false;
                _landYLerping = false;
                return;
            }

            bool wasKinematic = body.isKinematic;
            if (!wasKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            transform.localPosition = _restLocalPos;
            transform.localRotation = _restLocalRot;
            Physics.SyncTransforms();

            if (!wasKinematic)
                body.position = transform.position;

            body.isKinematic = wasKinematic;
            _grounded = false;
            _leftGroundSinceJump = false;
            _landYOffsetActive = false;
            _landYLerping = false;
        }

        void ApplyLocalY(float localY)
        {
            Vector3 lp = transform.localPosition;
            lp.y = localY;
            transform.localPosition = lp;

            if (body == null)
                return;

            if (!body.isKinematic)
            {
                Vector3 v = body.velocity;
                v.y = 0f;
                body.velocity = v;
                body.position = transform.position;
            }
        }

        void CrossFadeTo(string stateName, float animSpeed, float blendSeconds)
        {
            EnsureRefs();
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                return;

            // CrossFadeInFixedTime 길이는 animator.speed 에 비례해 소비됨
            // → Inspector 초(벽시계)를 유지하려면 duration *= speed.
            float speed = Mathf.Max(0.01f, animSpeed);
            animator.speed = speed;
            float blend = Mathf.Max(0f, blendSeconds);
            if (blend <= 0.0001f)
            {
                animator.Play(stateName, 0, 0f);
                animator.Update(0f);
            }
            else
            {
                animator.CrossFadeInFixedTime(stateName, blend * speed, 0, 0f);
            }
        }

        void EnsurePhysicsConfigured()
        {
            EnsureRefs();
            if (body == null)
                return;

            // 제약은 에디터 값 유지. 중력·보간만 맞춤.
            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        void EnsureRefs()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (body == null)
                body = GetComponent<Rigidbody>();
        }

        void CacheRest(bool force = false)
        {
            if (_cachedRest && !force)
                return;

            _restLocalPos = transform.localPosition;
            _restLocalRot = transform.localRotation;
            _cachedRest = true;
        }

        void OnCollisionEnter(Collision collision) => UpdateGroundFromCollision(collision, entering: true);

        void OnCollisionStay(Collision collision) => UpdateGroundFromCollision(collision, entering: true);

        void OnCollisionExit(Collision collision)
        {
            if (collision == null || collision.collider == null)
                return;

            if (collision.collider.GetComponentInParent<CoffinDanceFailFloor>() == null)
                return;

            _grounded = false;
            if (!_leftGroundSinceJump)
                _leftGroundSinceJump = true;
        }

        void UpdateGroundFromCollision(Collision collision, bool entering)
        {
            if (collision == null || collision.collider == null)
                return;

            if (collision.collider.GetComponentInParent<CoffinDanceFailFloor>() == null)
                return;

            if (entering)
                _grounded = true;
        }

        void FixedUpdate()
        {
            if (!_leftGroundSinceJump && !_grounded)
                _leftGroundSinceJump = true;
        }
    }
}
