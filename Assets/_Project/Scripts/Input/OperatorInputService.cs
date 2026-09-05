using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Input
{
    /// <summary>
    /// 운영자 키보드: ↑ / ↓ 로 메뉴 이동, Space 셔플, Enter 로 확정.
    /// 새 Input System + 구 UnityEngine.Input 둘 다 본다(EventSystem 때문에 한쪽만 먹는 경우 완충).
    /// </summary>
    public sealed class OperatorInputService
    {
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

        public bool MenuUp
        {
            get
            {
                Keyboard k = ResolveKeyboard();

                bool fromSystem =
                    k != null &&
                    k.upArrowKey.wasPressedThisFrame;

                bool fromLegacy =
                    UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);

                return fromSystem || fromLegacy;
            }
        }

        public bool MenuDown
        {
            get
            {
                Keyboard k = ResolveKeyboard();

                bool fromSystem =
                    k != null &&
                    k.downArrowKey.wasPressedThisFrame;

                bool fromLegacy =
                    UnityEngine.Input.GetKeyDown(KeyCode.DownArrow);

                return fromSystem || fromLegacy;
            }
        }

        public bool Shuffle
        {
            get
            {
                Keyboard k = ResolveKeyboard();

                bool fromSystem =
                    k != null &&
                    k.spaceKey.wasPressedThisFrame;

                bool fromLegacy =
                    UnityEngine.Input.GetKeyDown(KeyCode.Space);

                return fromSystem || fromLegacy;
            }
        }

        public bool Confirm
        {
            get
            {
                Keyboard k = ResolveKeyboard();

                bool fromSystem =
                    k != null &&
                    (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame);

                bool fromLegacy =
                    UnityEngine.Input.GetKeyDown(KeyCode.Return) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter);

                return fromSystem || fromLegacy;
            }
        }
    }
}
