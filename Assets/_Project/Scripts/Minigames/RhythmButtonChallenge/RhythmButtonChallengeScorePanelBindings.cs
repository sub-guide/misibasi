using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// 슬롯 총점 TMP와 점수 팝업 Animator. Inspector에서만 연결.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RhythmButtonChallengeScorePanelBindings : MonoBehaviour
    {
        public TMP_Text ScoreText;
        public Animator PopupSuccess;
        public Animator PopupFail;
        public Animator PopupBonus;
    }
}
