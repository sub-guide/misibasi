using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        /// <summary>
        /// Phase는 HUD 라벨·Phase4 점수×2 만. 조작/노이즈 난이도는 고정(후속 재도입 가능).
        /// </summary>
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
            _scoreMultiplier = phase == CdPhase.Phase4 ? 2f : 1f;
        }
    }
}
