using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void TickSlotGameplay(int i, float dt)
        {
            ref SlotRuntime sr = ref _slots[i];

            ReadBalanceInput(i, out float left, out float right);
            if (IsDevGodModeSlot(i) && !IsExclusiveShoulderInput(left, right))
                StepDevGodIdleReturn(ref sr, dt);
            else
                StepShoulderControl(ref sr, dt, left, right);
            ApplyPallbearerPoses(i, ref sr);
            UpdateExtremeSeesawShoulderColliders(i, ref sr);
            AccruePassiveScore(i, ref sr, dt);
            HandleFailFloorContact(i, ref sr);
            TickShoulderIgnore(i, ref sr, dt);
            ApplyShoulderDepenetration(i, ref sr);
            TickCenterBalanceCameraFx(i, ref sr, dt);
        }

        void AccruePassiveScore(int i, ref SlotRuntime sr, float dt)
        {
            if (_ctx.IsPractice)
                return;

            float mul = _scoreMultiplier;
            float gain = SurvivalScorePerSecond * mul * dt;
            if (IsCenterBalanceActive(i, ref sr))
                gain += centerBonusScorePerSec * mul * dt;

            sr.ScoreExact += gain;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }

        bool IsCenterBalanceActive(int i, ref SlotRuntime sr)
        {
            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            if (Mathf.Abs(x - 0.5f) > centerZoneThreshold)
                return false;

            if (sr.ShoulderIgnoreRemain > 0f)
                return false;

            CoffinDanceSlotBindings bind = GetBindings(i);
            return bind != null && bind.IsCoffinTouchingAnyEnabledShoulder();
        }
    }
}
