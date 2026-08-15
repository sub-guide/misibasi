using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void TickSlotGameplay(int i, float dt)
        {
            ref SlotRuntime sr = ref _slots[i];
            if (sr.Eliminated)
                return;

            ReadBalanceInput(i, out float left, out float right);
            StepShoulderControl(ref sr, dt, left, right);
            ApplyPallbearerPoses(i, ref sr);
            AccruePassiveScore(i, ref sr, dt);

            if (CheckFailFloorAndMaybeEliminate(i, ref sr))
                return;
        }

        void AccruePassiveScore(int i, ref SlotRuntime sr, float dt)
        {
            if (_ctx.IsPractice)
                return;

            float mul = _scoreMultiplier;
            float gain = SurvivalScorePerSecond * mul * dt;

            float absDeg = Mathf.Abs(GetSlotTiltDegrees(i));
            if (absDeg <= CenterHoldDegrees)
                gain += CenterHoldScorePerSecond * mul * dt;

            sr.ScoreExact += gain;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }

        void EliminateSlot(int i, ref SlotRuntime sr)
        {
            if (sr.Eliminated)
                return;

            sr.Eliminated = true;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
            _aliveMask[i] = false;
            SetEliminatedUi(i, true);

            CoffinDanceSlotBindings bind = GetBindings(i);
            bind?.SoftResetAllPallbearers();

            CoffinDanceCoffinBody body = bind?.ResolveCoffinBody();
            if (body != null)
            {
                body.ClearFailFloorContact();
                body.SetSimulationActive(false);
            }

            if (!_ctx.IsPractice && CountAlive() == 0)
                BeginEndDelay();
        }
    }
}
