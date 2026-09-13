using MiniParty.Input;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        void TickInput()
        {
            if (_segmentKind != RbcSegmentKind.StageInput || !_clockStarted)
                return;

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                if (_slots[i].BeatState != BeatJudgment.Pending)
                    return;

                RbcButton? pressed = ReadAnyGameplayButtonPressed(i, SlotGamepad.Get(i));
                if (!pressed.HasValue)
                    return;

                if (pressed.Value == _currentPattern[_beatIndex])
                    ApplySuccess(i);
                else
                    ApplyFail(i);
            });
        }

        static bool WasPressed(int slotIndex, Joystick pad, string path) =>
            BoothUsbSlotInput.WasPathPressed(slotIndex, pad, path);

        RbcButton? ReadAnyGameplayButtonPressed(int slotIndex, Joystick pad)
        {
            if (!SlotGamepad.HasInput(slotIndex))
                return null;

            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceA)) return RbcButton.A;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceB)) return RbcButton.B;
            if (BoothUsbSlotInput.PrimaryTriggerWasPressed(slotIndex, pad)) return RbcButton.X;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceY)) return RbcButton.Y;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.ShoulderL)) return RbcButton.Lb;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.ShoulderR)) return RbcButton.Rb;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.StickUp)) return RbcButton.Up;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.StickDown)) return RbcButton.Down;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.StickLeft)) return RbcButton.Left;
            if (WasPressed(slotIndex, pad, BoothUsbGamepadLayout.StickRight)) return RbcButton.Right;

            return null;
        }
    }
}
