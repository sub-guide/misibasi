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

            /// <summary>플레이어 Hold 바이어스. 키를 떼도 유지(미세 도치·풀은 별도).</summary>
            public float SeesawBias;

            /// <summary>실제 어깨에 반영되는 x. Y_L=x · Y_R=1-x.</summary>
            public float SeesawXCurrent;

            /// <summary>좌/우 단일 홀드 누적 시간. 미입력·동시 입력 시 0.</summary>
            public float HoldTimer;
        }
    }
}
