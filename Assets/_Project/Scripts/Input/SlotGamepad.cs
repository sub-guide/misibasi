using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace MiniParty.Input
{
    /// <summary>
    /// 슬롯 i → <see cref="Joystick.all"/>[i]. 부스 HID USB 패드(Gamepad 레이아웃 아님) 전제.
    /// </summary>
    public static class SlotGamepad
    {
        public static Joystick Get(int slotIndex)
        {
            if (slotIndex < 0)
                return null;

            ReadOnlyArray<Joystick> pads = Joystick.all;
            if (slotIndex >= pads.Count)
                return null;

            return pads[slotIndex];
        }

        /// <summary>실제 패드 또는 1P 키보드 디버그 모드로 입력 가능한 슬롯.</summary>
        public static bool HasInput(int slotIndex)
        {
            if (Get(slotIndex) != null)
                return true;

            DeveloperKeyboardGamepadDebug.EnsureToggleChecked();
            return DeveloperKeyboardGamepadDebug.AppliesToSlot(slotIndex);
        }

        /// <summary>사용자·기획 기준 START (디버거: Button 10). 1P 키보드 디버그 시 <c>B</c>.</summary>
        public static bool StartPressed(int slotIndex)
        {
            return BoothUsbSlotInput.WasPathPressed(slotIndex, Get(slotIndex), BoothUsbGamepadLayout.Start);
        }

        /// <summary>사용자·기획 기준 SELECT (디버거: Button 9). 1P 키보드 디버그 시 <c>V</c>.</summary>
        public static bool SelectPressed(int slotIndex)
        {
            return BoothUsbSlotInput.WasPathPressed(slotIndex, Get(slotIndex), BoothUsbGamepadLayout.Select);
        }
    }
}
