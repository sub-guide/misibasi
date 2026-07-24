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
        [Tooltip("기울기(θ)가 적용되는 루트. 관+운구인 부모 권장.")]
        public Transform TiltRoot;

        [Tooltip("관 Cube. 비우면 TiltRoot만 회전.")]
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

        [Tooltip("|θ|=StumbleLimit 일 때 fill이 0 또는 1로 치우침.")]
        public float GaugeFillAtLimit = 0f;
    }
}
