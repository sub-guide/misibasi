using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 프리팹 루트. 관 Rigidbody·운구인 Pose·스코어 UI를 Module에 연결한다.
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

        [Tooltip("운구인 루트 6개 (좌 3·우 3). Pose는 자동 GetComponent.")]
        public Transform[] Pallbearers = new Transform[6];

        [Tooltip("좌측 운구인 Pose 3개. 비우면 Pallbearers[0..2]에서 탐색.")]
        public CoffinDancePallbearerPose[] LeftPallbearerPoses;

        [Tooltip("우측 운구인 Pose 3개. 비우면 Pallbearers[3..5]에서 탐색.")]
        public CoffinDancePallbearerPose[] RightPallbearerPoses;

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

        public void ResolvePallbearerPoses()
        {
            if (!Application.isPlaying)
                return;

            EnsurePoseArray(ref LeftPallbearerPoses, 3);
            EnsurePoseArray(ref RightPallbearerPoses, 3);

            for (var i = 0; i < 3; i++)
            {
                if (LeftPallbearerPoses[i] == null && Pallbearers != null && i < Pallbearers.Length)
                    LeftPallbearerPoses[i] = GetOrAddPose(Pallbearers[i]);

                if (RightPallbearerPoses[i] == null && Pallbearers != null && i + 3 < Pallbearers.Length)
                    RightPallbearerPoses[i] = GetOrAddPose(Pallbearers[i + 3]);
            }
        }

        static void EnsurePoseArray(ref CoffinDancePallbearerPose[] arr, int len)
        {
            if (arr == null || arr.Length != len)
                arr = new CoffinDancePallbearerPose[len];
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
