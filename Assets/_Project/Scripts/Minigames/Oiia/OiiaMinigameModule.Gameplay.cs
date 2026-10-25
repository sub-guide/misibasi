using MiniParty.Input;
using MiniParty.UI.ControllerButtons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        readonly bool[] _djPressedScratch = new bool[DjPadButtonCount];

        /// <summary>10키 전부 정답. 같은 프레임의 첫 입력 1회만 점수.</summary>
        void TickGameplay(int i)
        {
            if (!SlotGamepad.HasInput(i))
                return;

            Joystick pad = SlotGamepad.Get(i);
            TickGameplayAllKeysCorrectDj(i, pad);
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

            if (!sr.DjActive[buttonIndex])
                ActivateAllDjTargets(i);

            sr.Combo++;
            int steppedPosition = AdvanceSubPatternOnHit(i, ref sr, out bool patternCompleted);

            sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, DjHitScore);
            if (patternCompleted)
                sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, PatternCompleteBonusScore);

            PlayCatSingleCycleIfIdle(i);
            TriggerSlotUiShakeOnCorrect(i, ref sr);

            if (steppedPosition > 0 &&
                patternStepSfx != null &&
                patternStepSfx.Length > 0)
            {
                PlayPatternStepSfx((steppedPosition - 1) % patternStepSfx.Length);
            }
        }

        static void EnsureDjActiveArray(ref SlotRuntime sr)
        {
            if (sr.DjActive == null || sr.DjActive.Length != DjPadButtonCount)
                sr.DjActive = new bool[DjPadButtonCount];
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
