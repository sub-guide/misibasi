using MiniParty.Input;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        static bool WasPressed(int slotIndex, Joystick pad, string path)
        {
            return BoothUsbSlotInput.WasPathPressed(slotIndex, pad, path);
        }

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
