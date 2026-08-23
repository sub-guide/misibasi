namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        enum CdFlowState
        {
            Playing,
            Ending,
            Complete
        }

        enum CdPhase
        {
            Phase1 = 1,
            Phase2 = 2,
            Phase3 = 3,
            Phase4 = 4
        }

        struct SlotRuntime
        {
            public float ScoreExact;
            public int ScoreSum;
            public bool Eliminated;

            public float SeesawBias;
            public float SeesawXCurrent;
            public float HoldTimer;
            public float ShoulderIgnoreRemain;

            public float CamFovBlend;
            public float CamZ;
            public float CamZVelocity;
        }
    }
}
