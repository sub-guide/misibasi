using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void TickSlotGameplay(int i, float dt)
        {
            ref SlotRuntime sr = ref _slots[i];

            ReadBalanceInput(i, out float left, out float right);
            StepShoulderControl(ref sr, dt, left, right);
            ApplyPallbearerPoses(i, ref sr);
            UpdateExtremeSeesawShoulderColliders(i, ref sr);
            AccruePassiveScore(ref sr, dt);
            HandleFailFloorContact(i, ref sr);
            TickShoulderIgnore(i, ref sr, dt);
            ApplyShoulderDepenetration(i, ref sr);
        }

        void AccruePassiveScore(ref SlotRuntime sr, float dt)
        {
            if (_ctx.IsPractice)
                return;

            float mul = _scoreMultiplier;
            float gain = SurvivalScorePerSecond * mul * dt;

            // x = Clamp01(x_bias + DanceWave × noiseAmp) — StepShoulderControl에서 이미 반영
            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            bool isCenter = Mathf.Abs(x - 0.5f) <= centerZoneThreshold;
            if (isCenter)
                gain += centerBonusScorePerSec * mul * dt;

            sr.ScoreExact += gain;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }
    }
}
