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

        [Tooltip("입력 시각에 더해 판정 박을 floor 로 계산(초). 클록 _beatIndex 와 일치할 때만 판정.")]
        [SerializeField] float inputTimingBiasSeconds = 0.03f;

        [Header("점수")]
        [SerializeField] int scoreSuccess = 10000;
        [SerializeField] int scoreFail = -10000;
        [SerializeField] int scoreEightBeatBonus = 30000;

        [Header("HUD")]
        [SerializeField] TMP_Text phaseLabel;

        [Tooltip("1P~4P. 비참가 슬롯은 숨김.")]
        [SerializeField] RectTransform[] playerSlots;

        [Tooltip("P1~P4 RhythmButtonChallengeScorePanelBindings.")]
        [SerializeField] RhythmButtonChallengeScorePanelBindings[] scorePanels;

        [Tooltip("점수 팝업 표시 시간(초). ScoreEffect 클립과 맞출 것.")]
        [SerializeField] float scorePopupVisibleSeconds = 0.5f;
    }
}
