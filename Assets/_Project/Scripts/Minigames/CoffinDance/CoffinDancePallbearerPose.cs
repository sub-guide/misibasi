using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 운구인: Extension Blend(앉음↔기립)만.
    /// Rigidbody 없음. 관은 어깨 SphereCollider(정적) 충돌로만 받친다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDancePallbearerPose : MonoBehaviour
    {
        public const string ExtensionParam = "Extension";
        public const string StateExtensionBlend = "ExtensionBlend";

        [Header("Animator")]
        [Tooltip("비우면 같은 GameObject에서 GetComponent.")]
        [SerializeField] Animator animator;

        Vector3 _restLocalPos;
        Quaternion _restLocalRot;
        bool _cachedRest;

        public float Extension { get; private set; } = 1f;

        void Awake()
        {
            EnsureAnimator();
            CacheRest(force: true);
            if (animator != null)
                animator.applyRootMotion = false;

            Extension = 1f;
        }

        public void PrepareForGameplay()
        {
            EnsureAnimator();
            // SoftReset 시 rest를 덮어쓰지 않음 — Begin 첫 캐시만 유지
            if (!_cachedRest)
                CacheRest(force: true);

            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.speed = 1f;
            }

            SoftResetTransform();
            ResetPoseAnim();
        }

        public void ResetPoseAnim()
        {
            Extension = 1f;
            EnsureAnimator();
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

        /// <summary>Begin 시점 rest 로컬 위치·회전으로 복귀.</summary>
        public void SoftResetTransform()
        {
            CacheRest();
            transform.localPosition = _restLocalPos;
            transform.localRotation = _restLocalRot;
            Physics.SyncTransforms();
        }

        void EnsureAnimator()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        void CacheRest(bool force = false)
        {
            if (_cachedRest && !force)
                return;

            _restLocalPos = transform.localPosition;
            _restLocalRot = transform.localRotation;
            _cachedRest = true;
        }
    }
}
