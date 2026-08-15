namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        public const int SlotCount = 4;

        public const float MainDurationSeconds = 60f;
        public const float CenterHoldDegrees = 10f;

        public const float SurvivalScorePerSecond = 100f;
        public const float CenterHoldScorePerSecond = 50f;

        public const float Phase1EndSeconds = 20f;
        public const float Phase2EndSeconds = 40f;
        public const float Phase3EndSeconds = 50f;

        public const float SessionEndDelaySeconds = 1f;

        public const int DefaultLowScoreThreshold = 3000;

        public const float DefaultSeesawNeutral = 0.5f;
        public const float MinExtension = 0f;
        public const float MaxExtension = 1f;

        /// <summary>←/→ 기본 탭 이동 속도.</summary>
        public const float DefaultSeesawBaseSpeed = 1.2f;

        /// <summary>홀드 조작 시 최대 가속 배율.</summary>
        public const float DefaultHoldMaxMultiplier = 3.0f;

        /// <summary>홀드 최대 가속 도달 시간(초).</summary>
        public const float DefaultHoldAccelTime = 0.2f;

        /// <summary>미입력·동시 입력 시 현재 기울기 방향 중력형 미세 도치 속도.</summary>
        public const float DefaultMicroDriftSpeed = 0.5f;

        /// <summary>중앙(0.5) 이탈 시 비선형 가속 계수.</summary>
        public const float DefaultPullCoefficient = 2.0f;

        /// <summary>고정 씰룩임 진폭 (Phase 난이도 없음 · 후속 재도입 가능).</summary>
        public const float DefaultNoiseAmp = 0.12f;

        public const float DefaultDanceSineHz = 1.2f;
    }
}
