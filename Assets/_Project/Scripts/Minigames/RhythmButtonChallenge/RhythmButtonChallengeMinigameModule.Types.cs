namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public enum RbcButton
        {
            A,
            B,
            X,
            Y,
            Lb,
            Rb,
            Up,
            Down,
            Left,
            Right
        }

        enum RbcSegmentKind
        {
            PhaseIntro,
            StageReveal,
            StageInput
        }

        enum BeatJudgment
        {
            Pending,
            Success,
            Fail
        }

        struct SlotRuntime
        {
            public int ScoreSum;
            public BeatJudgment BeatState;
            public int SuccessesThisInput;
        }
    }
}
