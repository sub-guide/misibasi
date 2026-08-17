using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 프리팹 루트. 관·운구인·스코어 UI를 Module에 연결한다.
    /// Pallbearers[0..2]=좌 · [3..5]=우. Pose는 각 루트에서 자동 탐색.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceSlotBindings : MonoBehaviour
    {
        [Header("3D")]
        [Tooltip("연출용 부모(운구인 포함). Yaw만 적용. 관 Z 기울기는 Rigidbody.")]
        public Transform TiltRoot;

        [Tooltip("관 Cube + CoffinDanceCoffinBody(Rigidbody).")]
        public Transform Coffin;

        [Tooltip("관 물리 컴포넌트. 비우면 Coffin에서 GetComponent.")]
        public CoffinDanceCoffinBody CoffinBody;

        [Tooltip("운구인 루트 6개. [0..2]=좌 · [3..5]=우.")]
        public Transform[] Pallbearers = new Transform[6];

        public Camera SlotCamera;

        [Header("UI")]
        public TMP_Text ScoreText;
        public TMP_Text PracticeReadyText;
        public TMP_Text EliminatedText;
        public TMP_Text PlayerLabelText;

        public CoffinDanceCoffinBody ResolveCoffinBody()
        {
            if (CoffinBody != null)
                return CoffinBody;

            if (Coffin != null)
                CoffinBody = Coffin.GetComponent<CoffinDanceCoffinBody>();

            return CoffinBody;
        }

        public void PrepareAllPoses()
        {
            for (var i = 0; i < 6; i++)
                GetOrAddPoseAt(i)?.PrepareForGameplay();
        }

        public void ApplySideExtension(bool leftSide, float extension)
        {
            int start = leftSide ? 0 : 3;
            for (var i = start; i < start + 3; i++)
            {
                CoffinDancePallbearerPose pose = GetOrAddPoseAt(i);
                if (pose == null)
                    continue;

                pose.SetExtension(extension);
            }
        }

        public void SoftResetAllPallbearers()
        {
            ForEachPose(p =>
            {
                p.SoftResetTransform();
                p.ResetPoseAnim();
            });
        }

        public void SetCoffinShoulderCollisionsIgnored(bool ignored)
        {
            CoffinDanceCoffinBody body = ResolveCoffinBody();
            Collider coffinCol = body != null ? body.GetComponent<Collider>() : null;
            if (coffinCol == null || Pallbearers == null)
                return;

            ForEachShoulderSphere(includeDisabled: true, sphere =>
            {
                Physics.IgnoreCollision(coffinCol, sphere, ignored);
            });
        }

        public void SetSideShoulderCollidersEnabled(bool leftSide, bool enabled)
        {
            if (Pallbearers == null)
                return;

            int start = leftSide ? 0 : 3;
            int end = start + 3;
            for (var i = start; i < end && i < Pallbearers.Length; i++)
            {
                Transform root = Pallbearers[i];
                if (root == null)
                    continue;

                SphereCollider[] spheres = root.GetComponentsInChildren<SphereCollider>(true);
                for (var s = 0; s < spheres.Length; s++)
                {
                    if (spheres[s] != null)
                        spheres[s].enabled = enabled;
                }
            }
        }

        public void ApplyUpwardShoulderDepenetration(float maxY)
        {
            CoffinDanceCoffinBody body = ResolveCoffinBody();
            if (body == null || maxY <= 0f)
                return;

            Collider coffinCol = body.GetComponent<Collider>();
            Rigidbody rb = body.Body;
            if (coffinCol == null || rb == null || rb.isKinematic)
                return;

            Vector3 coffinPos = rb.position;
            Quaternion coffinRot = rb.rotation;
            float lift = 0f;

            ForEachShoulderSphere(includeDisabled: false, sphere =>
            {
                Transform st = sphere.transform;
                if (!Physics.ComputePenetration(
                        coffinCol, coffinPos, coffinRot,
                        sphere, st.position, st.rotation,
                        out Vector3 direction, out float distance))
                    return;

                float y = direction.y * distance;
                if (y > lift)
                    lift = y;
            });

            if (lift <= 0f)
                return;

            body.LiftWorldY(Mathf.Min(lift, maxY));
        }

        public bool IsCoffinTouchingAnyEnabledShoulder()
        {
            CoffinDanceCoffinBody body = ResolveCoffinBody();
            return body != null && body.IsTouchingShoulder;
        }

        void ForEachShoulderSphere(bool includeDisabled, System.Action<SphereCollider> action)
        {
            if (Pallbearers == null || action == null)
                return;

            for (var i = 0; i < Pallbearers.Length; i++)
            {
                Transform root = Pallbearers[i];
                if (root == null)
                    continue;

                SphereCollider[] spheres = root.GetComponentsInChildren<SphereCollider>(true);
                for (var s = 0; s < spheres.Length; s++)
                {
                    SphereCollider sphere = spheres[s];
                    if (sphere == null)
                        continue;
                    if (!includeDisabled && !sphere.enabled)
                        continue;

                    action(sphere);
                }
            }
        }

        void ForEachPose(System.Action<CoffinDancePallbearerPose> action)
        {
            for (var i = 0; i < 6; i++)
            {
                CoffinDancePallbearerPose pose = GetOrAddPoseAt(i);
                if (pose != null)
                    action(pose);
            }
        }

        CoffinDancePallbearerPose GetOrAddPoseAt(int pallbearerIndex)
        {
            if (Pallbearers == null || pallbearerIndex < 0 || pallbearerIndex >= Pallbearers.Length)
                return null;

            return GetOrAddPose(Pallbearers[pallbearerIndex]);
        }

        static CoffinDancePallbearerPose GetOrAddPose(Transform root)
        {
            if (root == null)
                return null;

            var pose = root.GetComponent<CoffinDancePallbearerPose>();
            if (pose == null)
                pose = root.gameObject.AddComponent<CoffinDancePallbearerPose>();
            return pose;
        }
    }
}
