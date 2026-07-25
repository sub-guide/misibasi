using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void RefreshPhaseParameters()
        {
            float t = _ctx.IsPractice ? 0f : _elapsedMainTime;
            CdPhase phase;

            if (t < Phase1EndSeconds)
                phase = CdPhase.Phase1;
            else if (t < Phase2EndSeconds)
                phase = CdPhase.Phase2;
            else if (t < Phase3EndSeconds)
                phase = CdPhase.Phase3;
            else
                phase = CdPhase.Phase4;

            _phase = phase;

            switch (phase)
            {
                case CdPhase.Phase1:
                    _shoulderSpeedMul = 1f;
                    _scoreMultiplier = 1f;
                    break;
                case CdPhase.Phase2:
                    _shoulderSpeedMul = phase2ShoulderMul;
                    _scoreMultiplier = 1f;
                    break;
                case CdPhase.Phase3:
                    _shoulderSpeedMul = phase3ShoulderMul;
                    _scoreMultiplier = 1f;
                    break;
                default:
                    _shoulderSpeedMul = phase4ShoulderMul;
                    _scoreMultiplier = 2f;
                    break;
            }
        }
    }
}
