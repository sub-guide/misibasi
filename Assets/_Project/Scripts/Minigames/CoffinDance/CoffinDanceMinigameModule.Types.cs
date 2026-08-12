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

        /// <summary>점프 FSM. None이 아니면 x_bias Hold · ←/→·점프 잠금.</summary>
        enum JumpAnimPhase
        {
            None = 0,
            /// <summary>지상 · 현재 프레임→JumpStart 첫 프레임 CrossFade. 종료 후 Impulse.</summary>
            BlendIn = 1,
            /// <summary>물리 공중 · JumpStart 연출. 착지 감지 시 Land.</summary>
            Airborne = 2,
            /// <summary>지상 · JumpLand 연출. 클립 종료 시 조작 재개.</summary>
            Land = 3
        }

        struct SlotRuntime
        {
            public float ScoreExact;
            public int ScoreSum;
            public bool Eliminated;

            public JumpAnimPhase JumpPhase;
            public float JumpPhaseTimer;
            public float JumpClipDuration;

            public bool JumpActive => JumpPhase != JumpAnimPhase.None;

            public float SeesawBias;
            public float SeesawXCurrent;
            public float HoldTimer;
            public float LandingDriftTimer;
        }
    }
}
