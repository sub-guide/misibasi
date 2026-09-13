namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public const int SlotCount = 4;
        public const int BeatsPerSegment = 8;
        public const int StagesPerPhase = 5;

        const int ScoreSuccess = 10000;
        const int ScoreFail = -10000;
        const int ScoreEightBeatBonus = 30000;
        const float SlotOutlineFadeToIdleSeconds = 0.2f;
    }
}
