using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Input
{
    /// <summary>
    /// 개발자용 키보드 디버그: <c>1</c> 토글 시 1P 부스 패드 입력을 키보드로 에뮬레이션.
    /// W/A/S/D=방향, Q=L, E=R, T=Y, Y=X(Trigger), G=B, H=A, V=Select, B=Start.
    /// </summary>
    public static class DeveloperKeyboardGamepadDebug
    {
        const int PlayerSlotIndex = 0;

        static bool _active;
        static int _lastToggleCheckFrame = -1;

        public static bool IsActive => _active;

        public static bool AppliesToSlot(int slotIndex) =>
            slotIndex == PlayerSlotIndex && _active;

        /// <summary>슬롯 입력 읽기 전에 한 프레임에 한 번 호출 — <c>1</c> 토글 감지.</summary>
        public static void EnsureToggleChecked()
        {
            if (_lastToggleCheckFrame == Time.frameCount)
                return;

            _lastToggleCheckFrame = Time.frameCount;

            if (!ToggleKeyPressedThisFrame())
                return;

            _active = !_active;
            Debug.Log($"[DeveloperKeyboardGamepadDebug] 1P keyboard debug: {(_active ? "ON" : "OFF")}");
        }

        public static bool WasPathPressed(string pathRelativeToDevice)
        {
            if (string.IsNullOrEmpty(pathRelativeToDevice))
                return false;

            if (pathRelativeToDevice == BoothUsbGamepadLayout.FaceXTriggerAlias)
                return WasPrimaryTriggerPressed();

            return pathRelativeToDevice switch
            {
                BoothUsbGamepadLayout.StickUp => KeyWasPressed(Key.W),
                BoothUsbGamepadLayout.StickDown => KeyWasPressed(Key.S),
                BoothUsbGamepadLayout.StickLeft => KeyWasPressed(Key.A),
                BoothUsbGamepadLayout.StickRight => KeyWasPressed(Key.D),
                BoothUsbGamepadLayout.ShoulderL => KeyWasPressed(Key.Q),
                BoothUsbGamepadLayout.ShoulderR => KeyWasPressed(Key.E),
                BoothUsbGamepadLayout.FaceY => KeyWasPressed(Key.T),
                BoothUsbGamepadLayout.FaceB => KeyWasPressed(Key.G),
                BoothUsbGamepadLayout.FaceA => KeyWasPressed(Key.H),
                BoothUsbGamepadLayout.Select => KeyWasPressed(Key.V),
                BoothUsbGamepadLayout.Start => KeyWasPressed(Key.B),
                _ => false
            };
        }

        public static bool IsPathHeld(string pathRelativeToDevice)
        {
            if (string.IsNullOrEmpty(pathRelativeToDevice))
                return false;

            if (pathRelativeToDevice == BoothUsbGamepadLayout.FaceXTriggerAlias)
                return IsPrimaryTriggerHeld();

            return pathRelativeToDevice switch
            {
                BoothUsbGamepadLayout.StickUp => KeyIsHeld(Key.W),
                BoothUsbGamepadLayout.StickDown => KeyIsHeld(Key.S),
                BoothUsbGamepadLayout.StickLeft => KeyIsHeld(Key.A),
                BoothUsbGamepadLayout.StickRight => KeyIsHeld(Key.D),
                BoothUsbGamepadLayout.ShoulderL => KeyIsHeld(Key.Q),
                BoothUsbGamepadLayout.ShoulderR => KeyIsHeld(Key.E),
                BoothUsbGamepadLayout.FaceY => KeyIsHeld(Key.T),
                BoothUsbGamepadLayout.FaceB => KeyIsHeld(Key.G),
                BoothUsbGamepadLayout.FaceA => KeyIsHeld(Key.H),
                BoothUsbGamepadLayout.Select => KeyIsHeld(Key.V),
                BoothUsbGamepadLayout.Start => KeyIsHeld(Key.B),
                _ => false
            };
        }

        public static bool WasPrimaryTriggerPressed() => KeyWasPressed(Key.Y);

        public static bool IsPrimaryTriggerHeld() => KeyIsHeld(Key.Y);

        static bool ToggleKeyPressedThisFrame()
        {
            Keyboard kb = ResolveKeyboard();

            bool fromSystem =
                kb != null &&
                (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame);

            bool fromLegacy =
                UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) ||
                UnityEngine.Input.GetKeyDown(KeyCode.Keypad1);

            return fromSystem || fromLegacy;
        }

        static Keyboard ResolveKeyboard()
        {
            if (Keyboard.current != null)
                return Keyboard.current;

            Keyboard kb = InputSystem.GetDevice<Keyboard>();
            if (kb != null)
                return kb;

            foreach (InputDevice d in InputSystem.devices)
            {
                if (d is Keyboard k)
                    return k;
            }

            return null;
        }

        static bool KeyWasPressed(Key key)
        {
            Keyboard kb = ResolveKeyboard();

            bool fromSystem = kb != null && kb[key].wasPressedThisFrame;
            bool fromLegacy = UnityEngine.Input.GetKeyDown(LegacyKeyCode(key));

            return fromSystem || fromLegacy;
        }

        static bool KeyIsHeld(Key key)
        {
            Keyboard kb = ResolveKeyboard();

            bool fromSystem = kb != null && kb[key].isPressed;
            bool fromLegacy = UnityEngine.Input.GetKey(LegacyKeyCode(key));

            return fromSystem || fromLegacy;
        }

        static KeyCode LegacyKeyCode(Key key) =>
            key switch
            {
                Key.W => KeyCode.W,
                Key.A => KeyCode.A,
                Key.S => KeyCode.S,
                Key.D => KeyCode.D,
                Key.Q => KeyCode.Q,
                Key.E => KeyCode.E,
                Key.T => KeyCode.T,
                Key.Y => KeyCode.Y,
                Key.G => KeyCode.G,
                Key.H => KeyCode.H,
                Key.V => KeyCode.V,
                Key.B => KeyCode.B,
                _ => KeyCode.None
            };
    }
}
