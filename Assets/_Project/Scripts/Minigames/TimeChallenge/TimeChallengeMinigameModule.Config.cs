using MiniParty.Flow;
using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        [Header("표시")]
        [SerializeField] string displayName = "스톱워치";

        [Header("라운드")]
        [SerializeField] double round1Seconds = DefaultRound1Seconds;
        [Tooltip("2·3라운드 목표시간 최소(초). 시계 01:00.")]
        [SerializeField] double randomTargetMinSeconds = DefaultRandomTargetMinSeconds;
        [Tooltip("2·3라운드 목표시간 최대(초). 시계 07:00.")]
        [SerializeField] double randomTargetMaxSeconds = DefaultRandomTargetMaxSeconds;
        [SerializeField] double timeoutExtraSeconds = DefaultTimeoutExtraSeconds;
        [SerializeField] float countdownStepSeconds = DefaultCountdownStepSeconds;
        [SerializeField] float resultHoldSeconds = DefaultResultHoldSeconds;
        [SerializeField] float goVisibleSeconds = DefaultGoVisibleSeconds;
        [SerializeField] float briefingSeconds = DefaultBriefingSeconds;

        [Header("공통 HUD")]
        [SerializeField] TMP_Text roundLabel;
        [SerializeField] TMP_Text targetTimeLabel;
        [SerializeField] TMP_Text countdownLabel;
        [SerializeField] TMP_Text statusLabel;
        [Tooltip("라운드 안내. 글꼴은 씬의 Title/Subtitle TMP에서 지정합니다.")]
        [SerializeField] TimeChallengeRoundSting roundSting;

        [SerializeField] TMP_Text[] playerSeatLabels = new TMP_Text[SlotCount];
        [SerializeField] RectTransform roundTitle;
        [SerializeField] Camera hudCamera;
        [SerializeField] Canvas targetLabelCanvas;

        [Header("플레이어 LED 시계")]
        [Tooltip("씬에 둔 꺼진 LED 시계. 0=1P. Inspector에서 슬롯 번호 순서대로 연결.")]
        [SerializeField] TimeChallengeLedClock[] playerClocks = new TimeChallengeLedClock[SlotCount];

        [Header("책상 아래 점수판")]
        [Tooltip("씬에 둔 점수판. 0=1P. Inspector에서 슬롯 번호 순서대로 연결.")]
        [SerializeField] TimeChallengeScoreBoard[] playerScoreBoards = new TimeChallengeScoreBoard[SlotCount];
        [SerializeField] float scoreCountDuration = 0.85f;

        [Header("종료 → Result 씬")]
        [SerializeField] ScreenFader exitScreenFader;
        [SerializeField] float sessionEndHoldSeconds = 0.35f;
        [SerializeField] float exitFadeOutSeconds = 1f;
    }
}
