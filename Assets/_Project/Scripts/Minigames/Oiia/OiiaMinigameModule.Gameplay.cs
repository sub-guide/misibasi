using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        float FullGaugeDrainDurationSeconds(ref SlotRuntime sr)
        {
            if (_ctx.IsPractice)
                return PracticeGaugeDrainSeconds;

            int tier = ResolveGameplayTier(ref sr);
            return GaugeDrainSecondsForTier(tier);
        }

        static float GaugeDrainPerSecond(float secondsToEmptyFullBar)
        {
            if (secondsToEmptyFullBar <= 0.0001f)
                return 0f;

            return 1f / secondsToEmptyFullBar;
        }

        void TickMeta(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            if (sr.InputLockTimer > 0f)
                sr.InputLockTimer -= Time.deltaTime;

            if (sr.FailFlashTimer > 0f)
                sr.FailFlashTimer -= Time.deltaTime;

            if (sr.TierBumpBlurRemaining > 0f)
                sr.TierBumpBlurRemaining -= Time.deltaTime;

            TickShuffleEffect(i, Time.deltaTime);
            TickNeonShockwave(i, Time.deltaTime);
            TickBurstTextPool(i, Time.deltaTime);
        }

        void TickGameplay(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            Joystick pad = SlotGamepad.Get(i);
            if (!SlotGamepad.HasInput(i))
                return;

            if (IsDevGodModeSlot(i))
            {
                TickGameplayDevGodMode1P(i, pad);
                return;
            }

            UpdateGuideHoldFeedback(i, pad);

            if (sr.InputLockTimer > 0f)
                return;

            if (_ctx.IsPractice && _practiceReady[i])
                return;

            char expectedLetter = PatternLowerAt(sr.Cursor);

            bool pressedO = WasPhysicalPressed(i, pad, sr.MapO);
            bool pressedI = WasPhysicalPressed(i, pad, sr.MapI);
            bool pressedA = WasPhysicalPressed(i, pad, sr.MapA);

            bool b5 = BoothUsbSlotInput.WasPathPressed(i, pad, BoothUsbGamepadLayout.ShoulderL);
            bool b6 = BoothUsbSlotInput.WasPathPressed(i, pad, BoothUsbGamepadLayout.ShoulderR);
            bool b9 = BoothUsbSlotInput.WasPathPressed(i, pad, BoothUsbGamepadLayout.Select);
            bool b10 = BoothUsbSlotInput.WasPathPressed(i, pad, BoothUsbGamepadLayout.Start);

            bool anyMappedOiia = pressedO || pressedI || pressedA;

            bool anyOtherMapped = b5 || b6 || b9;
            if (!_ctx.IsPractice)
                anyOtherMapped |= b10;

            bool anyUnmappedFace = false;
            foreach (OiiaPhysicalButton btn in System.Enum.GetValues(typeof(OiiaPhysicalButton)))
            {
                if (btn == sr.MapO || btn == sr.MapI || btn == sr.MapA)
                    continue;

                if (WasPhysicalPressed(i, pad, btn))
                {
                    anyUnmappedFace = true;
                    break;
                }
            }

            bool any = anyMappedOiia || anyOtherMapped || anyUnmappedFace;

            bool correct =
                (expectedLetter == 'o' && pressedO && !pressedI && !pressedA) ||
                (expectedLetter == 'i' && pressedI && !pressedO && !pressedA) ||
                (expectedLetter == 'a' && pressedA && !pressedO && !pressedI);

            if (correct)
            {
                OnCorrectInput(i);
                return;
            }

            if (any)
            {
                OnTypo(i);
                return;
            }

            if (_ctx.IsPractice)
                return;

            float drainDuration = FullGaugeDrainDurationSeconds(ref sr);
            float drainRate = GaugeDrainPerSecond(drainDuration);
            sr.Gauge01 -= drainRate * Time.deltaTime;

            if (sr.Gauge01 <= 0f)
                OnGaugeDepleted(i);
        }

        void OnCorrectInput(int i)
        {
            ref SlotRuntime sr = ref _slots[i];
            int stepJustCompleted = sr.Cursor;
            char completedLetter = PatternLowerAt(stepJustCompleted);

            sr.InTypoState = false;

            PlayPatternStepSfx(stepJustCompleted);
            TriggerNeonShockwave(i, completedLetter);
            TriggerBurstTextOnCorrect(i, completedLetter);

            if (!_ctx.IsPractice)
            {
                PlayCatSingleCycleIfIdle(i);
                TriggerSlotUiShakeOnCorrect(i, ref sr);
            }

            sr.Cursor++;

            if (!_ctx.IsPractice)
            {
                int tier = ResolveGameplayTier(ref sr);
                sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, ScorePerCorrectStepForTier(tier));
            }

            if (sr.Cursor < _patternLower.Length)
                return;

            OnLoopComplete(i);
        }

        void OnLoopComplete(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            if (!_ctx.IsPractice)
            {
                int tierBeforeBonus = ResolveGameplayTier(ref sr);
                sr.ConsecutiveLoopSuccesses++;
                sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, LoopBonusForTier(tierBeforeBonus));
                TriggerTierBumpBlurOnLoopComplete(ref sr);
            }

            ShuffleButtonMapping(i);
            BeginShuffleEffect(i);
        }

        void OnTypo(int i)
        {
            ref SlotRuntime sr = ref _slots[i];
            sr.InTypoState = true;
            sr.FailFlashTimer = FailFlashDurationSeconds;
            PlayBuzz();
        }

        void OnGaugeDepleted(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            sr.Cursor = 0;
            sr.ConsecutiveLoopSuccesses = 0;
            sr.InTypoState = false;
            sr.ShuffleEffectTimer = 0f;
            sr.TierBumpBlurRemaining = 0f;
            sr.InputLockTimer = 0f;
            sr.FailFlashTimer = FailFlashDurationSeconds;
            sr.Gauge01 = 1f;

            AssignDefaultButtonMapping(ref sr);

            if (TryGetBinding(i, out SlotUiBindings b))
            {
                HideShuffleEffect(b);
                ClearBurstTextPoolVisual(i, b);
            }

            TriggerFailFx(i);

            if (!_ctx.IsPractice)
            {
                ResetCatMovementImmediate(i);
                StopSlotUiShake(i);
                ForceCatAnimatorIdle(i);
            }

            ResetGuideFeedbackSlot(i);
        }

        void TriggerFailFx(int i)
        {
            ref SlotRuntime sr = ref _slots[i];
            sr.FailFlashTimer = FailFlashDurationSeconds;
            PlayBuzz();
        }
    }
}
