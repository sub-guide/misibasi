namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public const int SlotCount = 4;
        public const int BeatsPerSegment = 8;
        public const int StagesPerPhase = 5;

        const float SlotOutlineFadeToIdleSeconds = 0.2f;

        static readonly int SquareIntroAnimStateHash = UnityEngine.Animator.StringToHash("Square_Intro");
        static readonly int ScorePopupAnimStateHash = UnityEngine.Animator.StringToHash("ScoreEffect");
        static readonly UnityEngine.Vector3 SquareIntroRestScale = new(1.1f, 0f, 1.1f);
    }
}
