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
            AccruePassiveScore(i, ref sr, dt);
            HandleFailFloorContact(i, ref sr);
            TickShoulderIgnore(i, ref sr, dt);
            ApplyShoulderDepenetration(i, ref sr);
        }

        void AccruePassiveScore(int i, ref SlotRuntime sr, float dt)
        {
            if (_ctx.IsPractice)
                return;

            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            bool isCenter = Mathf.Abs(x - 0.5f) <= centerZoneThreshold;
            CoffinDanceSlotBindings bind = GetBindings(i);
            bool supported = sr.ShoulderIgnoreRemain <= 0f
                             && bind != null
                             && bind.IsCoffinTouchingAnyEnabledShoulder();

            float mul = _scoreMultiplier;
            float gain = SurvivalScorePerSecond * mul * dt;
            if (isCenter && supported)
                gain += centerBonusScorePerSec * mul * dt;

            sr.ScoreExact += gain;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }
    }
}
