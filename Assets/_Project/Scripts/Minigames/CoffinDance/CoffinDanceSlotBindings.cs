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
        public TMP_Text JumpPromptText;
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

        public void ApplySideExtension(bool leftSide, float extension, float jumpT)
        {
            int start = leftSide ? 0 : 3;
            for (var i = start; i < start + 3; i++)
            {
                CoffinDancePallbearerPose pose = GetOrAddPoseAt(i);
                if (pose == null)
                    continue;

                pose.SetExtension(extension);
                pose.SetJumpPhase01(jumpT);
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
