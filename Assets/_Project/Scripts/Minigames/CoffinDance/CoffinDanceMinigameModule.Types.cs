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
            public float JumpLockoutRemain;
            public bool JumpActive;
            public float JumpElapsed;

            /// <summary>플레이어 Hold 바이어스. 키를 떼도 유지.</summary>
            public float SeesawBias;

            /// <summary>실제 어깨에 반영되는 x (Rate Limiter 결과). Y_L=x · Y_R=1-x.</summary>
            public float SeesawXCurrent;
        }
    }
}
