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

            float left = 0f;
            float right = 0f;

            if (!sr.JumpActive)
            {
                ReadBalanceInput(i, out left, out right);

                if (WasJumpPressed(i))
                    BeginFreeJump(i, ref sr);
            }

            if (!sr.JumpActive)
                StepShoulderControl(ref sr, dt, left, right);

            ApplyPallbearerPoses(i, ref sr);
            AccruePassiveScore(i, ref sr, dt);

            if (CheckFailFloorAndMaybeEliminate(i, ref sr))
                return;
        }

        void TickJumpState(int i, ref SlotRuntime sr, float dt)
        {
            if (sr.JumpPhase == JumpAnimPhase.None)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);

            switch (sr.JumpPhase)
            {
                case JumpAnimPhase.BlendIn:
                    // 지상 페이드(현재→JumpStart 첫 프레임) · 종료 시 모션+Impulse 동시
                    sr.JumpPhaseTimer += dt;
                    if (sr.JumpPhaseTimer < Mathf.Max(0f, jumpAnimBlendSeconds))
                        return;

                    bind?.CommitJumpAfterBlend(jumpImpulse, jumpStartAnimSpeed);
                    sr.JumpPhase = JumpAnimPhase.Airborne;
                    sr.JumpPhaseTimer = 0f;
                    break;

                case JumpAnimPhase.Airborne:
                    // JumpStart 중에는 Land 금지. 종료 후 + 이탈·재접지 → Land
                    if (bind == null ||
                        !bind.HasJumpStartCompleted() ||
                        !bind.AreAllReadyToLand())
                        return;

                    sr.JumpPhase = JumpAnimPhase.Land;
                    sr.JumpPhaseTimer = 0f;
                    sr.JumpClipDuration = bind.EnterJumpLand(
                        jumpLandAnimSpeed,
                        jumpAnimBlendSeconds,
                        DefaultJumpLandFallbackSeconds);
                    bind.BeginLandYOffsetLerp(jumpLandYOffset, jumpLandYOffsetDuration);
                    break;

                case JumpAnimPhase.Land:
                    sr.JumpPhaseTimer += dt;
                    bind?.TickLandYOffsetLerp(dt);

                    bool landDone = sr.JumpPhaseTimer >= sr.JumpClipDuration ||
                                    (bind != null && bind.HasJumpLandCompleted());
                    if (!landDone)
                        return;

                    sr.JumpPhase = JumpAnimPhase.None;
                    sr.JumpPhaseTimer = 0f;
                    sr.JumpClipDuration = 0f;
                    bind?.EnterExtensionBlend(jumpAnimBlendSeconds); // ClearLandYOffset → rest 즉시 복귀
                    sr.LandingDriftTimer = Mathf.Max(0f, landingDriftDuration);
                    break;
            }
        }

        void BeginFreeJump(int i, ref SlotRuntime sr)
        {
            if (sr.JumpActive)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            sr.JumpPhaseTimer = 0f;
            sr.JumpClipDuration = 0f;

            float blend = Mathf.Max(0f, jumpAnimBlendSeconds);
            if (blend > 0.0001f)
            {
                // 현재 프레임 → JumpStart 첫 프레임 페이드(지상) → 끝나면 모션+점프
                sr.JumpPhase = JumpAnimPhase.BlendIn;
                bind?.BeginJumpAnim(blend);
            }
            else
            {
                sr.JumpPhase = JumpAnimPhase.Airborne;
                bind?.BeginJump(jumpImpulse, jumpStartAnimSpeed);
            }
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
            sr.JumpPhase = JumpAnimPhase.None;
            sr.JumpPhaseTimer = 0f;
            sr.JumpClipDuration = 0f;
            sr.LandingDriftTimer = 0f;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
            _aliveMask[i] = false;
            SetEliminatedUi(i, true);
            HideJumpPrompt(i);

            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind != null)
            {
                bind.SetPallbearerSimulationActive(false);
                bind.SoftResetAllPallbearers();
            }

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
