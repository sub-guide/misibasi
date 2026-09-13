using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// 플레이어 슬롯 점수. Inspector에서 ScoreText 연결.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RhythmButtonChallengeScorePanelBindings : MonoBehaviour
    {
        public TMP_Text ScoreText;
    }
}
