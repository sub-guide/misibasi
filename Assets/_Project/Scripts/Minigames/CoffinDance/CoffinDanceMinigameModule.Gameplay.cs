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

            if (sr.JumpLockoutRemain > 0f)
            {
                float before = sr.JumpLockoutRemain;
                sr.JumpLockoutRemain -= dt;
                if (before > 0f && sr.JumpLockoutRemain <= 0f)
                    ApplyLandingImpulse(ref sr);
            }

            bool controlEnabled = sr.JumpLockoutRemain <= 0f;
            float left = 0f;
            float right = 0f;

            if (controlEnabled)
                ReadBalanceInput(i, out left, out right);

            if (_jumpPromptState == CdJumpPromptState.Prompting &&
                !sr.JumpSucceededThisPrompt &&
                sr.JumpLockoutRemain <= 0f)
            {
                TickJumpInputForSlot(i);
            }

            StepPhysics(ref sr, dt, left, right);
            ApplyTiltVisual(i, ref sr);
            AccruePassiveScore(ref sr, dt);

            if (CheckStumbleAndMaybeEliminate(i, ref sr, dt))
                return;
        }

        void SoftResetTilt(ref SlotRuntime sr)
        {
            float sign = (_rng != null && _rng.Next(0, 2) == 0) ? -1f : 1f;
            sr.ThetaRad = sign * initialTiltDegrees * Mathf.Deg2Rad * 0.5f;
            sr.Omega = -sign * initialAngularSpeed * 0.5f;
            sr.InStumble = false;
            sr.StumbleTimer = 0f;
            sr.JumpLockoutRemain = 0f;
        }

        void AccruePassiveScore(ref SlotRuntime sr, float dt)
        {
            if (_ctx.IsPractice)
                return;

            float mul = _scoreMultiplier;
            float gain = SurvivalScorePerSecond * mul * dt;

            float absDeg = Mathf.Abs(sr.ThetaRad) * Mathf.Rad2Deg;
            if (absDeg <= CenterHoldDegrees)
                gain += CenterHoldScorePerSecond * mul * dt;

            sr.ScoreExact += gain;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }

        void AddScoreInstant(ref SlotRuntime sr, int basePoints)
        {
            if (_ctx.IsPractice)
                return;

            float add = basePoints * _scoreMultiplier;
            sr.ScoreExact += add;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }

        void EliminateSlot(int i, ref SlotRuntime sr)
        {
            if (sr.Eliminated)
                return;

            sr.Eliminated = true;
            sr.InStumble = false;
            sr.StumbleTimer = 0f;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
            _aliveMask[i] = false;
            SetEliminatedUi(i, true);
            HideJumpPrompt(i);

            if (!_ctx.IsPractice && CountAlive() == 0)
                BeginEndDelay();
        }
    }
}
