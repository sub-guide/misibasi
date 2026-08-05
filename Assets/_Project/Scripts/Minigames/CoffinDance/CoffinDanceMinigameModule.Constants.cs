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

        /// <summary>←/→ x_bias 초당 이동 속도.</summary>
        public const float DefaultSeesawMoveSpeed = 1.4f;

        /// <summary>고정 씰룩임 진폭 (Phase 난이도 없음 · 후속 재도입 가능).</summary>
        public const float DefaultNoiseAmp = 0.12f;

        public const float DefaultDanceSineHz = 1.2f;
    }
}
