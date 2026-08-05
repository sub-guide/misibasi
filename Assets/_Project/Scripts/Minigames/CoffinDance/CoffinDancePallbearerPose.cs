using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 운구인 자세: Animator 1D Blend (Extension 0=Crouch_Fwd · 1=Walk_Formal).
    /// Module의 SetExtension / SetJumpPhase01 API를 유지한다.
    /// Edit Mode에서는 Animator/Transform을 건드리지 않는다 (Prefab 위치 오염 방지).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDancePallbearerPose : MonoBehaviour
    {
        public const string ExtensionParam = "Extension";

        [Header("Animator")]
        [Tooltip("비우면 같은 GameObject에서 GetComponent.")]
        [SerializeField] Animator animator;

        [Header("점프")]
        [SerializeField] float jumpHeight = 0.45f;

        Vector3 _restLocalPos;
        bool _cachedRestLocalPos;

        public float Extension { get; private set; } = 1f;
        public float JumpPhase01 { get; private set; }
        public bool IsAirborne => JumpPhase01 > 0f && JumpPhase01 < 1f;

        void Awake()
        {
            EnsureAnimator();
            CacheRestLocalPos(force: true);
            if (animator != null)
                animator.applyRootMotion = false;

            Extension = 1f;
            JumpPhase01 = 0f;
            ApplyPoseVisual();
        }

        public void PrepareForGameplay()
        {
            EnsureAnimator();
            CacheRestLocalPos(force: true);
            if (animator != null)
                animator.applyRootMotion = false;

            ResetPose();
        }

        public void ResetPose()
        {
            Extension = 1f;
            JumpPhase01 = 0f;
            ApplyPoseVisual();
        }

        public void SetExtension(float extension)
        {
            Extension = Mathf.Clamp01(extension);
            ApplyPoseVisual();
        }

        public void SetJumpPhase01(float t)
        {
            JumpPhase01 = Mathf.Clamp01(t);
            ApplyPoseVisual();
        }

        void EnsureAnimator()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        void CacheRestLocalPos(bool force = false)
        {
            if (_cachedRestLocalPos && !force)
                return;

            _restLocalPos = transform.localPosition;
            _cachedRestLocalPos = true;
        }

        void ApplyPoseVisual()
        {
            // Prefab/Scene 편집 중 OnValidate·AnimationMode가 위치를 오염시키지 않도록
            if (!Application.isPlaying)
                return;

            EnsureAnimator();
            if (!_cachedRestLocalPos)
                CacheRestLocalPos();

            float blendT = Mathf.Clamp01(Extension);
            float hopY = 0f;

            if (JumpPhase01 > 0f && JumpPhase01 < 1f)
            {
                if (JumpPhase01 < 0.2f)
                {
                    float u = JumpPhase01 / 0.2f;
                    float dip = Mathf.Sin(u * Mathf.PI) * 0.85f;
                    blendT = Mathf.Lerp(Extension, 0f, dip);
                }
                else
                {
                    float u = (JumpPhase01 - 0.2f) / 0.8f;
                    hopY = Mathf.Sin(u * Mathf.PI) * jumpHeight;
                }
            }

            if (animator != null &&
                animator.isActiveAndEnabled &&
                animator.runtimeAnimatorController != null)
            {
                animator.SetFloat(ExtensionParam, blendT);
            }

            transform.localPosition = _restLocalPos + Vector3.up * hopY;
        }
    }
}
