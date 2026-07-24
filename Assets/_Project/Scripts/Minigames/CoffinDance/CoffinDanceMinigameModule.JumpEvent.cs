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
                    _gravityMul = 1f;
                    _inertiaMul = 1f;
                    _externalForceAmp = 0f;
                    _scoreMultiplier = 1f;
                    _jumpIntervalMin = 6f;
                    _jumpIntervalMax = 8f;
                    break;
                case CdPhase.Phase2:
                    _gravityMul = 1f;
                    _inertiaMul = 1.25f;
                    _externalForceAmp = phase2ExternalForce;
                    _scoreMultiplier = 1f;
                    _jumpIntervalMin = 4f;
                    _jumpIntervalMax = 6f;
                    break;
                case CdPhase.Phase3:
                    _gravityMul = phase3GravityMul;
                    _inertiaMul = 1.35f;
                    _externalForceAmp = phase2ExternalForce * 1.2f;
                    _scoreMultiplier = 1f;
                    _jumpIntervalMin = 2.5f;
                    _jumpIntervalMax = 4f;
                    break;
                default:
                    _gravityMul = phase4GravityMul;
                    _inertiaMul = phase4InertiaMul;
                    _externalForceAmp = phase2ExternalForce * 1.5f;
                    _scoreMultiplier = 2f;
                    _jumpIntervalMin = 1.5f;
                    _jumpIntervalMax = 2.5f;
                    break;
            }
        }

        void ScheduleNextJump(bool immediate)
        {
            if (_ctx.IsPractice)
            {
                // 연습: Phase1 간격으로 JUMP만 연습
                _jumpIntervalMin = 6f;
                _jumpIntervalMax = 8f;
            }

            float delay = immediate
                ? RandomRange(_jumpIntervalMin * 0.5f, _jumpIntervalMin)
                : RandomRange(_jumpIntervalMin, _jumpIntervalMax);

            float baseTime = _ctx.IsPractice ? Time.time : _elapsedMainTime;
            _nextJumpAtElapsed = baseTime + delay;
        }

        float JumpClock() => _ctx.IsPractice ? Time.time : _elapsedMainTime;

        void TickJumpPromptGlobal(float dt)
        {
            if (_flowState != CdFlowState.Playing)
                return;

            if (_jumpPromptState == CdJumpPromptState.Idle)
            {
                if (JumpClock() < _nextJumpAtElapsed)
                    return;

                if (CountAlive() == 0)
                    return;

                BeginJumpPrompt();
                return;
            }

            _jumpPromptRemain -= dt;
            if (_jumpPromptRemain > 0f)
                return;

            ResolveJumpPromptTimeout();
        }

        void BeginJumpPrompt()
        {
            _jumpIsDouble = false;
            _jumpRequiredPresses = 1;

            if (!_ctx.IsPractice && _phase >= CdPhase.Phase3)
            {
                float roll = _rng != null ? (float)_rng.NextDouble() : Random.value;
                if (roll < doubleJumpChanceFromPhase3)
                {
                    _jumpIsDouble = true;
                    _jumpRequiredPresses = 2;
                }
            }

            _jumpPromptState = CdJumpPromptState.Prompting;
            _jumpPromptRemain = jumpInputWindowSeconds;

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                ref SlotRuntime sr = ref _slots[i];
                sr.JumpSucceededThisPrompt = false;
                sr.JumpPressesThisPrompt = 0;
                ShowJumpPrompt(i, _jumpIsDouble);
            });
        }

        void TickJumpInputForSlot(int i)
        {
            ref SlotRuntime sr = ref _slots[i];
            if (!WasJumpPressed(i))
                return;

            sr.JumpPressesThisPrompt++;
            if (sr.JumpPressesThisPrompt < _jumpRequiredPresses)
                return;

            sr.JumpSucceededThisPrompt = true;
            int points = _jumpIsDouble ? JumpDoubleSuccessScore : JumpSuccessScore;
            AddScoreInstant(ref sr, points);
            sr.JumpLockoutRemain = jumpLockoutSeconds;
            HideJumpPrompt(i);

            if (AllAliveResolvedJumpPrompt())
                EndJumpPromptAndSchedule();
        }

        void ResolveJumpPromptTimeout()
        {
            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                ref SlotRuntime sr = ref _slots[i];
                if (sr.JumpSucceededThisPrompt)
                    return;

                ApplyFailTiltImpulse(ref sr);
                HideJumpPrompt(i);
            });

            EndJumpPromptAndSchedule();
        }

        void EndJumpPromptAndSchedule()
        {
            _jumpPromptState = CdJumpPromptState.Idle;
            ScheduleNextJump(immediate: false);
        }

        bool AllAliveResolvedJumpPrompt()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                    continue;

                if (!_slots[i].JumpSucceededThisPrompt)
                    return false;
            }

            return true;
        }

        float RandomRange(float min, float max)
        {
            if (_rng == null)
                return UnityEngine.Random.Range(min, max);

            return min + (float)_rng.NextDouble() * (max - min);
        }
    }
}
