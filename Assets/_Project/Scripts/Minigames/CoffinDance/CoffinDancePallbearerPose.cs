using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 운구인 자세: Rest ↔ Crouch 보간.
    /// lockFeetToRestPlant 시 Rest Capture 때 잡은 발 월드 위치·각도를 유지한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDancePallbearerPose : MonoBehaviour
    {
        const string HipsName = "B-hips";
        const string ThighLName = "B-thigh.L";
        const string ThighRName = "B-thigh.R";
        const string ShinLName = "B-shin.L";
        const string ShinRName = "B-shin.R";
        const string FootLName = "B-foot.L";
        const string FootRName = "B-foot.R";

        [Header("본 (비우면 이름 자동 탐색)")]
        [SerializeField] Transform hips;
        [SerializeField] Transform thighL;
        [SerializeField] Transform thighR;
        [SerializeField] Transform shinL;
        [SerializeField] Transform shinR;
        [SerializeField] Transform footL;
        [SerializeField] Transform footR;

        [Header("발 고정")]
        [Tooltip("켜면 Rest Capture 때 발 위치·각도(월드)를 유지. 앉은 자세는 hips/thigh/shin만 조절하면 됨.")]
        [SerializeField] bool lockFeetToRestPlant = true;

        [Header("점프")]
        [SerializeField] float jumpHeight = 0.45f;

        [Header("에디터 미리보기")]
        [Tooltip("0=앉은 자세 · 1=선 자세.")]
        [SerializeField] [Range(0f, 1f)] float editorPreviewExtension = 0.5f;

        [Header("Rest (선 자세)")]
        [SerializeField] bool hasRestPose;
        [SerializeField] Quaternion restHips;
        [SerializeField] Quaternion restThighL;
        [SerializeField] Quaternion restThighR;
        [SerializeField] Quaternion restShinL;
        [SerializeField] Quaternion restShinR;
        [SerializeField] Quaternion restFootL;
        [SerializeField] Quaternion restFootR;
        [SerializeField] Vector3 restRootLocalPos;

        [Header("발 플랜트 (Rest Capture 시, parent 공간)")]
        [SerializeField] bool hasFootPlant;
        [SerializeField] Vector3 plantFootLInParent;
        [SerializeField] Vector3 plantFootRInParent;
        [SerializeField] Quaternion plantFootLRotInParent;
        [SerializeField] Quaternion plantFootRRotInParent;

        [Header("Crouch (앉은 자세)")]
        [SerializeField] bool hasCrouchPose;
        [SerializeField] Quaternion crouchHips;
        [SerializeField] Quaternion crouchThighL;
        [SerializeField] Quaternion crouchThighR;
        [SerializeField] Quaternion crouchShinL;
        [SerializeField] Quaternion crouchShinR;
        [SerializeField] Quaternion crouchFootL;
        [SerializeField] Quaternion crouchFootR;
        [SerializeField] Vector3 crouchRootLocalPos;

        Transform[] _allBones;

        public float Extension { get; private set; } = 1f;
        public float JumpPhase01 { get; private set; }
        public bool IsAirborne => JumpPhase01 > 0f && JumpPhase01 < 1f;

        bool CanBlend => hasRestPose && hasCrouchPose;

#if UNITY_EDITOR
        void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            if (!CanBlend)
                return;

            ResolveBones();
            Extension = 1f;
            JumpPhase01 = 0f;
            ApplyPoseVisual();
        }

        void OnValidate()
        {
            if (Application.isPlaying)
                return;

            ResolveBones();
            if (!CanBlend)
                return;

            Extension = Mathf.Clamp01(editorPreviewExtension);
            JumpPhase01 = 0f;
            ApplyPoseVisual();
        }
#endif

        void Awake()
        {
            ResolveBones();
            Extension = 1f;
            JumpPhase01 = 0f;
            if (CanBlend)
                ApplyPoseVisual();
        }

        [ContextMenu("Capture Rest Pose (Standing)")]
        public void CaptureRestPoseContextMenu()
        {
            ResolveBones();
            CapturePose(
                out restHips, out restThighL, out restThighR,
                out restShinL, out restShinR, out restFootL, out restFootR,
                out restRootLocalPos);
            StoreFootPlantFromCurrentFeet();
            hasRestPose = true;
            editorPreviewExtension = 1f;
            Extension = 1f;
            JumpPhase01 = 0f;
            if (CanBlend)
                ApplyPoseVisual();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Capture Crouch Pose (Seated)")]
        public void CaptureCrouchPoseContextMenu()
        {
            ResolveBones();
            if (lockFeetToRestPlant && hasFootPlant)
                SnapFeetToRestPlant();

            CapturePose(
                out crouchHips, out crouchThighL, out crouchThighR,
                out crouchShinL, out crouchShinR, out crouchFootL, out crouchFootR,
                out crouchRootLocalPos);

            // 발은 Rest 플랜트 유지 — crouch에 rest 발 로컬을 넣지 않고, 스냅 후 로컬을 그대로 저장
            // (무릎이 굽은 상태에서 월드 각도 고정 → 로컬은 rest와 다를 수 있음)
            hasCrouchPose = true;
            editorPreviewExtension = 0f;
            Extension = 0f;
            JumpPhase01 = 0f;
            if (CanBlend)
                ApplyPoseVisual();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Snap Feet To Rest Plant")]
        public void SnapFeetToRestPlantContextMenu()
        {
            ResolveBones();
            SnapFeetToRestPlant();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
#endif
        }

        public void PrepareForGameplay()
        {
            ResolveBones();
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

        void ResolveBones()
        {
            _allBones = GetComponentsInChildren<Transform>(true);
            hips = FindBone(hips, HipsName);
            thighL = FindBone(thighL, ThighLName);
            thighR = FindBone(thighR, ThighRName);
            shinL = FindBone(shinL, ShinLName);
            shinR = FindBone(shinR, ShinRName);
            footL = FindBone(footL, FootLName);
            footR = FindBone(footR, FootRName);
        }

        Transform FindBone(Transform current, string boneName)
        {
            if (current != null)
                return current;

            if (_allBones == null)
                return null;

            for (var i = 0; i < _allBones.Length; i++)
            {
                Transform t = _allBones[i];
                if (t != null && t.name == boneName)
                    return t;
            }

            return null;
        }

        void CapturePose(
            out Quaternion h,
            out Quaternion tL,
            out Quaternion tR,
            out Quaternion sL,
            out Quaternion sR,
            out Quaternion fL,
            out Quaternion fR,
            out Vector3 rootLocal)
        {
            h = hips != null ? hips.localRotation : Quaternion.identity;
            tL = thighL != null ? thighL.localRotation : Quaternion.identity;
            tR = thighR != null ? thighR.localRotation : Quaternion.identity;
            sL = shinL != null ? shinL.localRotation : Quaternion.identity;
            sR = shinR != null ? shinR.localRotation : Quaternion.identity;
            fL = footL != null ? footL.localRotation : Quaternion.identity;
            fR = footR != null ? footR.localRotation : Quaternion.identity;
            rootLocal = transform.localPosition;
        }

        void StoreFootPlantFromCurrentFeet()
        {
            if (footL == null || footR == null)
            {
                hasFootPlant = false;
                return;
            }

            Transform parent = transform.parent;
            Quaternion parentRot = parent != null ? parent.rotation : Quaternion.identity;

            plantFootLInParent = parent != null
                ? parent.InverseTransformPoint(footL.position)
                : footL.position;
            plantFootRInParent = parent != null
                ? parent.InverseTransformPoint(footR.position)
                : footR.position;
            plantFootLRotInParent = Quaternion.Inverse(parentRot) * footL.rotation;
            plantFootRRotInParent = Quaternion.Inverse(parentRot) * footR.rotation;
            hasFootPlant = true;
        }

        void SnapFeetToRestPlant()
        {
            if (!hasFootPlant || footL == null || footR == null)
                return;

            Transform parent = transform.parent;
            Quaternion parentRot = parent != null ? parent.rotation : Quaternion.identity;

            // 월드 각도 고정 (localRotation=rest 가 아님 — 무릎 굽으면 로컬≠월드)
            footL.rotation = parentRot * plantFootLRotInParent;
            footR.rotation = parentRot * plantFootRRotInParent;

            Vector3 wantL = parent != null
                ? parent.TransformPoint(plantFootLInParent)
                : plantFootLInParent;
            Vector3 wantR = parent != null
                ? parent.TransformPoint(plantFootRInParent)
                : plantFootRInParent;
            Vector3 wantMid = (wantL + wantR) * 0.5f;
            Vector3 curMid = (footL.position + footR.position) * 0.5f;
            transform.position += wantMid - curMid;
        }

        void ApplyPoseVisual()
        {
            if (!CanBlend)
                return;

            float blendT = Mathf.Clamp01(Extension);
            float hopY = 0f;

            if (JumpPhase01 > 0f && JumpPhase01 < 1f)
            {
                if (JumpPhase01 < 0.2f)
                {
                    float u = JumpPhase01 / 0.2f;
                    float dip = Mathf.Sin(u * Mathf.PI) * 0.85f;
                    blendT = Mathf.Lerp(Extension, Mathf.Min(Extension, 0f), dip);
                }
                else
                {
                    float u = (JumpPhase01 - 0.2f) / 0.8f;
                    hopY = Mathf.Sin(u * Mathf.PI) * jumpHeight;
                }
            }

            // blendT 0=crouch · 1=rest
            if (hips != null)
                hips.localRotation = Quaternion.Slerp(crouchHips, restHips, blendT);
            if (thighL != null)
                thighL.localRotation = Quaternion.Slerp(crouchThighL, restThighL, blendT);
            if (thighR != null)
                thighR.localRotation = Quaternion.Slerp(crouchThighR, restThighR, blendT);
            if (shinL != null)
                shinL.localRotation = Quaternion.Slerp(crouchShinL, restShinL, blendT);
            if (shinR != null)
                shinR.localRotation = Quaternion.Slerp(crouchShinR, restShinR, blendT);

            if (lockFeetToRestPlant && hasFootPlant)
            {
                transform.localPosition = restRootLocalPos;
                SnapFeetToRestPlant();
                transform.position += Vector3.up * hopY;
                return;
            }

            if (footL != null)
                footL.localRotation = Quaternion.Slerp(crouchFootL, restFootL, blendT);
            if (footR != null)
                footR.localRotation = Quaternion.Slerp(crouchFootR, restFootR, blendT);

            Vector3 root = Vector3.Lerp(crouchRootLocalPos, restRootLocalPos, blendT);
            transform.localPosition = root + Vector3.up * hopY;
        }
    }
}
