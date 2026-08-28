using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 프리팹 루트. 관·운구인·스코어 UI를 Module에 연결한다.
    /// Pallbearers[0..2]=좌 · [3..5]=우. Pose는 각 루트에서 자동 탐색.
    /// 어깨 2점 지지는 가운데 [1]·[4] Sphere만 쓴다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceSlotBindings : MonoBehaviour
    {
        public const int MiddleLeftPallbearerIndex = 1;
        public const int MiddleRightPallbearerIndex = 4;

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

        /// <summary>
        /// 좌·우 가운데 운구인 어깨 Sphere에 강체 관 바닥이 접하도록 로컬 Y·Z를 계산한다.
        /// 메시는 변형하지 않는다.
        /// </summary>
        public bool TryComputeCoffinSupportLocalPose(out float localY, out float zDegrees)
        {
            localY = 0f;
            zDegrees = 0f;

            CoffinDanceCoffinBody body = ResolveCoffinBody();
            if (body == null)
                return false;

            if (!TryGetMiddleShoulderSphere(leftSide: true, out SphereCollider left) ||
                !TryGetMiddleShoulderSphere(leftSide: false, out SphereCollider right))
                return false;

            Transform coffin = body.transform;
            Transform parent = coffin.parent;

            Vector3 cL = left.transform.TransformPoint(left.center);
            Vector3 cR = right.transform.TransformPoint(right.center);
            if (parent != null)
            {
                cL = parent.InverseTransformPoint(cL);
                cR = parent.InverseTransformPoint(cR);
            }

            float rL = GetSphereWorldRadius(left);
            float rR = GetSphereWorldRadius(right);
            if (parent != null)
            {
                float parentScaleY = parent.lossyScale.y;
                if (Mathf.Abs(parentScaleY) > 0.0001f)
                {
                    rL /= parentScaleY;
                    rR /= parentScaleY;
                }
            }

            float r = 0.5f * (rL + rR);

            Vector2 d = new Vector2(cL.x - cR.x, cL.y - cR.y);
            if (d.sqrMagnitude < 0.00000001f)
                return false;

            Vector2 n = new Vector2(-d.y, d.x);
            if (n.y < 0f)
                n = -n;
            n.Normalize();

            if (Mathf.Abs(n.y) < 0.0001f)
                return false;

            zDegrees = Mathf.Atan2(-n.x, n.y) * Mathf.Rad2Deg;

            BoxCollider box = body.GetComponent<BoxCollider>();
            float localBottomY = -0.5f;
            float scaleY = Mathf.Abs(coffin.localScale.y);
            if (box != null)
                localBottomY = box.center.y - box.size.y * 0.5f;

            float bottomAlongUp = localBottomY * scaleY;
            Vector3 lp = coffin.localPosition;
            localY = cL.y - (bottomAlongUp - r - n.x * (cL.x - lp.x)) / n.y;
            return true;
        }

        bool TryGetMiddleShoulderSphere(bool leftSide, out SphereCollider sphere)
        {
            sphere = null;
            if (Pallbearers == null)
                return false;

            int index = leftSide ? MiddleLeftPallbearerIndex : MiddleRightPallbearerIndex;
            if (index < 0 || index >= Pallbearers.Length)
                return false;

            Transform root = Pallbearers[index];
            if (root == null)
                return false;

            SphereCollider[] spheres = root.GetComponentsInChildren<SphereCollider>(true);
            for (var s = 0; s < spheres.Length; s++)
            {
                if (spheres[s] != null)
                {
                    sphere = spheres[s];
                    return true;
                }
            }

            return false;
        }

        static float GetSphereWorldRadius(SphereCollider sphere)
        {
            Vector3 ls = sphere.transform.lossyScale;
            float m = Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.y), Mathf.Abs(ls.z));
            return sphere.radius * m;
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
