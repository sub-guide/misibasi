namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public const int SlotCount = 4;
        public const int BeatsPerSegment = 8;
        public const int StagesPerPhase = 5;

        const int ScorePerfect = 10000;
        const int ScoreFast = 5000;
        const int ScoreSlow = 5000;
        const int ScoreMiss = -10000;
        const int ScoreWrong = -10000;
        const int ScoreExtraInputPenalty = -2000;
        const int ScoreEightBeatBonus = 30000;

        const float JudgmentPerfectHalfWindowMs = 50f;
        const float JudgmentEarlyMinMs = -120f;
        const float JudgmentEarlyMaxMs = -50f;
        const float JudgmentLateMinMs = 50f;
        const float JudgmentLateMaxMs = 120f;

        const float Phase1Pitch = 1f;
        const float Phase2Pitch = 2f;
        const float DefaultSpeedUpDisplaySeconds = 2f;
    }
}
