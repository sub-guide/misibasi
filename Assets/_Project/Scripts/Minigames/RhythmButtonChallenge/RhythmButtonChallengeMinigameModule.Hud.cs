using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        [Header("비트클록")]
        [Tooltip("Begin 후 Intro 8박·Square 연출 시작까지 대기(초). 0이면 즉시.")]
        [SerializeField] float preIntroDelaySeconds;

        [Tooltip("한 박 길이(초). 오디오 없이 클록이 이 값으로 진행한다.")]
        [SerializeField] float beatDurationSeconds = 0.5f;

        [Header("HUD")]
        [SerializeField] TMP_Text phaseLabel;

        [Tooltip("1P~4P. 비참가 슬롯은 숨김.")]
        [SerializeField] RectTransform[] playerSlots;
    }
}
