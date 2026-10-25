using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        static bool EscapePressed()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;

            return UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
