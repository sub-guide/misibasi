using MiniParty.Input;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        static bool WasDjPadPressed(int slotIndex, Joystick pad, OiiaDjPadButtonId id) =>
            id switch
            {
                OiiaDjPadButtonId.A => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceA),
                OiiaDjPadButtonId.B => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceB),
                OiiaDjPadButtonId.X => BoothUsbSlotInput.PrimaryTriggerWasPressed(slotIndex, pad),
                OiiaDjPadButtonId.Y => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceY),
                OiiaDjPadButtonId.L => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.ShoulderL),
                OiiaDjPadButtonId.R => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.ShoulderR),
                OiiaDjPadButtonId.Up => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.StickUp),
                OiiaDjPadButtonId.Down => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.StickDown),
                OiiaDjPadButtonId.Left => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.StickLeft),
                OiiaDjPadButtonId.Right => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.StickRight),
                _ => false
            };

        /// <summary>이번 프레임에 눌린 10키를 <paramref name="pressed"/>에 기록. 하나라도 눌리면 true.</summary>
        static bool CollectDjPadPressedThisFrame(int slotIndex, Joystick pad, bool[] pressed)
        {
            var any = false;
            for (var k = 0; k < DjPadButtonCount; k++)
            {
                bool p = WasDjPadPressed(slotIndex, pad, (OiiaDjPadButtonId)k);
                pressed[k] = p;
                if (p)
                    any = true;
            }

            return any;
        }
    }
}
