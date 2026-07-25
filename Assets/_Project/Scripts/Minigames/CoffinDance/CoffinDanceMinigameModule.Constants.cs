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

        public const float DefaultNeutralExtension = 0.5f;
        public const float MinExtension = 0f;
        public const float MaxExtension = 1f;
    }
}
