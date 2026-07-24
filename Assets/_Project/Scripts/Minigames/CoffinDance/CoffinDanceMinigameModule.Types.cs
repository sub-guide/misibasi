using UnityEngine;

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

        enum CdJumpPromptState
        {
            Idle,
            Prompting
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
            public float ThetaRad;
            public float Omega;
            public float ScoreExact;
            public int ScoreSum;
            public bool Eliminated;
            public float StumbleTimer;
            public bool InStumble;
            public float JumpLockoutRemain;
            public bool JumpSucceededThisPrompt;
            public int JumpPressesThisPrompt;
            public float JumpVisualT;
            public Vector3 CoffinBaseLocalPos;
            public bool HasCoffinBase;
        }
    }
}
