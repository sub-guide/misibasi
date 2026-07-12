using MiniParty.Input;
using MiniParty.UI.ControllerButtons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        readonly bool[] _djPressedScratch = new bool[DjPadButtonCount];

        void TickMeta(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            if (sr.InputLockTimer > 0f)
                sr.InputLockTimer -= Time.deltaTime;

            TickFever(i);
        }

        /// <summary>
        /// 상시 활성 타겟 3개. 피버(30콤보/3초) 중에는 전 키 정답.
        /// 스포트라이트·글로벌 티어는 후속.
        /// </summary>
        void TickGameplay(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            if (!SlotGamepad.HasInput(i))
                return;

            Joystick pad = SlotGamepad.Get(i);

            if (sr.InputLockTimer > 0f)
                return;

            if (_ctx.IsPractice && _practiceReady[i])
                return;

            EnsureDjActiveArray(ref sr);

            if (IsAllKeysCorrectMode(i))
            {
                TickGameplayAllKeysCorrectDj(i, pad);
                return;
            }

            if (!CollectDjPadPressedThisFrame(i, pad, _djPressedScratch))
                return;

            var anyWrong = false;
            var anyCorrect = false;
            var firstCorrect = -1;

            for (var k = 0; k < DjPadButtonCount; k++)
            {
                if (!_djPressedScratch[k])
                    continue;

                if (sr.DjActive[k])
                {
                    anyCorrect = true;
                    if (firstCorrect < 0)
                        firstCorrect = k;
                }
                else
                {
                    anyWrong = true;
                }
            }

            if (anyWrong)
            {
                OnDjMiss(i);
                return;
            }

            if (anyCorrect)
                OnDjHit(i, firstCorrect);
        }

        void TickGameplayAllKeysCorrectDj(int slotIndex, Joystick pad)
        {
            if (!CollectDjPadPressedThisFrame(slotIndex, pad, _djPressedScratch))
                return;

            var firstPressed = -1;
            for (var k = 0; k < DjPadButtonCount; k++)
            {
                if (!_djPressedScratch[k])
                    continue;

                firstPressed = k;
                break;
            }

            if (firstPressed < 0)
                return;

            OnDjHit(slotIndex, firstPressed);
        }

        void OnDjHit(int i, int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= DjPadButtonCount)
                return;

            ref SlotRuntime sr = ref _slots[i];
            EnsureDjActiveArray(ref sr);

            bool allKeys = IsAllKeysCorrectMode(i);

            if (!allKeys && !sr.DjActive[buttonIndex])
                return;

            if (!allKeys)
            {
                sr.DjActive[buttonIndex] = false;
                ReplenishOneDjActive(ref sr);
                ApplyDjPadHighlights(i);
            }
            else if (!sr.DjActive[buttonIndex])
            {
                ActivateAllDjTargets(i);
            }

            sr.Combo++;

            if (sr.FeverRemaining <= 0f)
            {
                if (sr.FeverCharge < FeverComboThreshold)
                    sr.FeverCharge++;
                TryBeginFeverOnCharge(i);
            }

            if (!_ctx.IsPractice)
                sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, DjHitScore);

            if (!_ctx.IsPractice)
            {
                PlayCatSingleCycleIfIdle(i);
                TriggerSlotUiShakeOnCorrect(i, ref sr);
            }

            if (patternStepSfx != null && patternStepSfx.Length > 0)
                PlayPatternStepSfx((sr.Combo - 1) % patternStepSfx.Length);
        }

        void OnDjMiss(int i)
        {
            ref SlotRuntime sr = ref _slots[i];
            sr.Combo = 0;
            sr.FeverCharge = 0;
            sr.InputLockTimer = InputLockAfterMissSeconds;

            if (sr.FeverRemaining > 0f)
                EndFever(i);

            TriggerSpotlightMissFlash(i);
            PlayBuzz();
        }

        static void EnsureDjActiveArray(ref SlotRuntime sr)
        {
            if (sr.DjActive == null || sr.DjActive.Length != DjPadButtonCount)
                sr.DjActive = new bool[DjPadButtonCount];
        }

        void SeedDjActiveTargets(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            EnsureDjActiveArray(ref sr);

            for (var k = 0; k < DjPadButtonCount; k++)
                sr.DjActive[k] = false;

            var picked = 0;
            var guard = 0;
            while (picked < DjActiveTargetCount && guard++ < 64)
            {
                int idx = Random.Range(0, DjPadButtonCount);
                if (sr.DjActive[idx])
                    continue;

                sr.DjActive[idx] = true;
                picked++;
            }

            ApplyDjPadHighlights(slotIndex);
        }

        static void ReplenishOneDjActive(ref SlotRuntime sr)
        {
            int inactiveCount = 0;
            for (var k = 0; k < DjPadButtonCount; k++)
            {
                if (!sr.DjActive[k])
                    inactiveCount++;
            }

            if (inactiveCount <= 0)
                return;

            int pick = Random.Range(0, inactiveCount);
            for (var k = 0; k < DjPadButtonCount; k++)
            {
                if (sr.DjActive[k])
                    continue;

                if (pick == 0)
                {
                    sr.DjActive[k] = true;
                    return;
                }

                pick--;
            }
        }

        static int FindFirstDjActiveIndex(ref SlotRuntime sr)
        {
            if (sr.DjActive == null)
                return -1;

            for (var k = 0; k < sr.DjActive.Length; k++)
            {
                if (sr.DjActive[k])
                    return k;
            }

            return -1;
        }

        void ApplyDjPadHighlights(int slotIndex)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.DjPadButtons == null)
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            EnsureDjActiveArray(ref sr);

            int n = Mathf.Min(b.DjPadButtons.Length, DjPadButtonCount);
            for (var k = 0; k < n; k++)
            {
                SnesControllerButtonVisual visual = b.DjPadButtons[k];
                if (visual != null)
                    visual.SetHighlighted(sr.DjActive[k]);
            }
        }

        void SyncDjPadPlayerIndices(int slotIndex, SlotUiBindings b)
        {
            if (b.DjFaceButtons != null)
                b.DjFaceButtons.SetPlayerIndex(slotIndex);

            if (b.DjDpadButtons != null)
                b.DjDpadButtons.SetPlayerIndex(slotIndex);

            if (b.DjShoulderButtons != null)
                b.DjShoulderButtons.SetPlayerIndex(slotIndex);
        }
    }
}
