using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MiniParty.Input
{
    /// <summary>
    /// 슬롯별 부스 패드 입력 — 실제 Joystick + (1P 한정) <see cref="DeveloperKeyboardGamepadDebug"/>.
    /// </summary>
    public static class BoothUsbSlotInput
    {
        public static bool WasPathPressed(int slotIndex, Joystick pad, string pathRelativeToDevice)
        {
            DeveloperKeyboardGamepadDebug.EnsureToggleChecked();

            bool fromPad = pad != null &&
                           BoothUsbGamepadLayout.Button(pad, pathRelativeToDevice)?.wasPressedThisFrame == true;

            if (DeveloperKeyboardGamepadDebug.AppliesToSlot(slotIndex))
                return fromPad || DeveloperKeyboardGamepadDebug.WasPathPressed(pathRelativeToDevice);

            return fromPad;
        }

        public static bool IsPathHeld(int slotIndex, Joystick pad, string pathRelativeToDevice)
        {
            DeveloperKeyboardGamepadDebug.EnsureToggleChecked();

            bool fromPad = pad != null &&
                           BoothUsbGamepadLayout.Button(pad, pathRelativeToDevice)?.isPressed == true;

            if (DeveloperKeyboardGamepadDebug.AppliesToSlot(slotIndex))
                return fromPad || DeveloperKeyboardGamepadDebug.IsPathHeld(pathRelativeToDevice);

            return fromPad;
        }

        public static bool PrimaryTriggerWasPressed(int slotIndex, Joystick pad)
        {
            DeveloperKeyboardGamepadDebug.EnsureToggleChecked();

            bool fromPad = BoothUsbGamepadLayout.PrimaryTrigger(pad)?.wasPressedThisFrame == true;

            if (DeveloperKeyboardGamepadDebug.AppliesToSlot(slotIndex))
                return fromPad || DeveloperKeyboardGamepadDebug.WasPrimaryTriggerPressed();

            return fromPad;
        }

        public static bool PrimaryTriggerIsHeld(int slotIndex, Joystick pad)
        {
            DeveloperKeyboardGamepadDebug.EnsureToggleChecked();

            bool fromPad = BoothUsbGamepadLayout.PrimaryTrigger(pad)?.isPressed == true;

            if (DeveloperKeyboardGamepadDebug.AppliesToSlot(slotIndex))
                return fromPad || DeveloperKeyboardGamepadDebug.IsPrimaryTriggerHeld();

            return fromPad;
        }
    }
}
