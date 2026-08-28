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

            public bool FailFloorRecoverActive;
            public float FailFloorRecoverDuration;
            public float FailFloorRecoverElapsed;
            public float FailFloorRecoverStartY;
            public float FailFloorRecoverStartZ;
            public float FailFloorRecoverStartSeesaw;

            /// <summary>시소 최대·FailFloor가 아니고 어깨 접촉 후, 관을 가운데 어깨 2점에 붙인 상태.</summary>
            public bool CoffinShoulderAttached;

            /// <summary>시소 최대로 떨어진 뒤 FailFloor 접촉 전까지 어깨 재부착 금지.</summary>
            public bool CoffinFallLockedUntilFloor;

            public float CamFovBlend;
            public float CamZ;
            public float CamZVelocity;
        }
    }
}
