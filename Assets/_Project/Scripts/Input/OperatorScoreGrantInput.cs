using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Input
{
    /// <summary>
    /// 게임 중 운영자 키보드 1~4. 누른 번호의 플레이어에게 그 게임의 성공 1회분 점수를 준다.
    /// 메인 숫자열과 키패드 모두. 개발자 1P 키보드 디버그가 켜져 있으면 키패드 4는 패드 B라서 제외.
    /// </summary>
    public static class OperatorScoreGrantInput
    {
        public static bool WasPressed(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > 3)
                return false;

            if (AlphaPressed(slotIndex))
                return true;

            if (DeveloperKeyboardGamepadDebug.IsActive && slotIndex == 3)
                return false;

            return NumpadPressed(slotIndex);
        }

        static bool AlphaPressed(int slotIndex)
        {
            (Key key, KeyCode legacy) = slotIndex switch
            {
                0 => (Key.Digit1, KeyCode.Alpha1),
                1 => (Key.Digit2, KeyCode.Alpha2),
                2 => (Key.Digit3, KeyCode.Alpha3),
                _ => (Key.Digit4, KeyCode.Alpha4)
            };

            return Down(key, legacy);
        }

        static bool NumpadPressed(int slotIndex)
        {
            (Key key, KeyCode legacy) = slotIndex switch
            {
                0 => (Key.Numpad1, KeyCode.Keypad1),
                1 => (Key.Numpad2, KeyCode.Keypad2),
                2 => (Key.Numpad3, KeyCode.Keypad3),
                _ => (Key.Numpad4, KeyCode.Keypad4)
            };

            return Down(key, legacy);
        }

        static bool Down(Key key, KeyCode legacy)
        {
            Keyboard kb = Keyboard.current;
            bool fromSystem = kb != null && kb[key].wasPressedThisFrame;
            bool fromLegacy = UnityEngine.Input.GetKeyDown(legacy);
            return fromSystem || fromLegacy;
        }
    }
}
