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

            TickJumpState(i, ref sr, dt);

            bool controlEnabled = !sr.JumpActive && sr.JumpLockoutRemain <= 0f;
            float left = 0f;
            float right = 0f;

            if (controlEnabled)
            {
                ReadBalanceInput(i, out left, out right);

                if (WasJumpPressed(i))
                    BeginFreeJump(ref sr);
            }

            if (!sr.JumpActive)
                StepShoulderControl(ref sr, dt, left, right);

            ApplyPallbearerPoses(i, ref sr);
            ApplyPresentationYaw(i);
            AccruePassiveScore(i, ref sr, dt);

            if (CheckFailFloorAndMaybeEliminate(i, ref sr))
                return;
        }

        void TickJumpState(int i, ref SlotRuntime sr, float dt)
        {
            if (!sr.JumpActive)
                return;

            sr.JumpElapsed += dt;
            sr.JumpLockoutRemain = Mathf.Max(0f, jumpLockoutSeconds - sr.JumpElapsed);

            if (sr.JumpElapsed < jumpLockoutSeconds)
                return;

            // 착지
            sr.JumpActive = false;
            sr.JumpElapsed = jumpLockoutSeconds;
            sr.JumpLockoutRemain = 0f;
            ApplyLandingImpulse(i);
        }

        void BeginFreeJump(ref SlotRuntime sr)
        {
            if (sr.JumpActive)
                return;

            sr.JumpActive = true;
            sr.JumpElapsed = 0f;
            sr.JumpLockoutRemain = jumpLockoutSeconds;
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
            sr.JumpActive = false;
            sr.JumpLockoutRemain = 0f;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
            _aliveMask[i] = false;
            SetEliminatedUi(i, true);
            HideJumpPrompt(i);

            CoffinDanceSlotBindings bind = GetBindings(i);
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
