using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool _devGodModeEnabled;
#endif

        /// <summary>Editor·Development Build에서 Backspace. 켜면 본게임 타이머가 멈춘다.</summary>
        bool IsDevGodModeActive()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return _devGodModeEnabled;
#else
            return false;
#endif
        }

        void TickDevGodModeToggle()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!DevGodModeTogglePressed())
                return;

            _devGodModeEnabled = !_devGodModeEnabled;

            if (_devGodModeEnabled && _tier2Active && _aliveMask[0] && _slots != null)
                ActivateAllDjTargets(0);

            Debug.Log(
                $"[OiiaMinigameModule] Dev God Mode (1P: all buttons highlighted, any key = hit, timer paused): " +
                $"{(_devGodModeEnabled ? "ON" : "OFF")}",
                this);
#endif
        }

        static bool DevGodModeTogglePressed()
        {
            if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
                return true;

            return UnityEngine.Input.GetKeyDown(KeyCode.Backspace);
        }

        void ActivateAllDjTargets(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            EnsureDjActiveArray(ref sr);

            for (var k = 0; k < DjPadButtonCount; k++)
                sr.DjActive[k] = true;

            ApplyDjPadHighlights(slotIndex);
        }
    }
}
