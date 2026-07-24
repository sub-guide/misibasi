using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 프리팹 루트. 관/운구인 Transform·게이지·JUMP·스코어 UI를 Module에 연결한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceSlotBindings : MonoBehaviour
    {
        [Header("3D")]
        [Tooltip("기울기 연출용 부모(운구인 포함). Yaw만 적용. Z 기울기는 Coffin에만.")]
        public Transform TiltRoot;

        [Tooltip("관 Cube. 균형 기울기(Z)·점프 홉이 여기에만 적용됩니다.")]
        public Transform Coffin;

        [Tooltip("운구인 Capsule 6개 (좌 3·우 3). 선택.")]
        public Transform[] Pallbearers = new Transform[6];

        public Camera SlotCamera;

        [Header("UI")]
        public Image BalanceGaugeFill;
        public TMP_Text JumpPromptText;
        public TMP_Text ScoreText;
        public TMP_Text PracticeReadyText;
        public TMP_Text EliminatedText;
        public TMP_Text PlayerLabelText;

        [Header("게이지")]
        [Tooltip("θ=0일 때 fillAmount.")]
        public float GaugeFillAtCenter = 0.5f;

        [Tooltip("|θ|=maxTiltDegrees 일 때 fill이 0 또는 1로 치우침.")]
        public float GaugeFillAtLimit = 0f;
    }
}
