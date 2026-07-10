namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public const float MainRoundDurationSeconds = 60f;

        public const int TargetSuccessScore = 100_000;

        const float Tier1GaugeDrainSeconds = 10f;

        const float Tier2GaugeDrainSeconds = 7f;

        const float Tier3GaugeDrainSeconds = 5f;

        const int Tier1ScorePerChar = 300;

        const int Tier2ScorePerChar = 500;

        const int Tier3ScorePerChar = 800;

        const int Tier1LoopBonus = 1_000;

        const int Tier2LoopBonus = 2_000;

        const int Tier3LoopBonus = 4_000;

        const int HpLowScoreThresholdDefault = 8_000;

        int HpLowScoreThreshold => HpLowScoreThresholdDefault;

        static int ResolveGameplayTier(int consecutiveLoopSuccesses)
        {
            if (consecutiveLoopSuccesses >= 3)
                return 3;

            if (consecutiveLoopSuccesses >= 2)
                return 2;

            return 1;
        }

        int ResolveGameplayTier(ref SlotRuntime sr) =>
            ResolveGameplayTier(sr.ConsecutiveLoopSuccesses);

        float GaugeDrainSecondsForTier(int tier)
        {
            if (tier >= 3)
                return Tier3GaugeDrainSeconds;

            if (tier >= 2)
                return Tier2GaugeDrainSeconds;

            return Tier1GaugeDrainSeconds;
        }

        int ScorePerCorrectStepForTier(int tier)
        {
            if (tier >= 3)
                return Tier3ScorePerChar;

            if (tier >= 2)
                return Tier2ScorePerChar;

            return Tier1ScorePerChar;
        }

        int LoopBonusForTier(int tier)
        {
            if (tier >= 3)
                return Tier3LoopBonus;

            if (tier >= 2)
                return Tier2LoopBonus;

            return Tier1LoopBonus;
        }
    }
}
