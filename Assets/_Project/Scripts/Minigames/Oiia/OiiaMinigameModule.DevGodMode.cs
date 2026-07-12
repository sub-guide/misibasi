using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool _devGodModeEnabled;
#endif

        /// <summary>
        /// Dev God Mode: 1P 전 버튼 Highlight + 아무 키나 정답 · 본게임 타이머 정지.
        /// Editor·Development Build 에서만 Backspace 토글.
        /// </summary>
        bool IsDevGodModeSlot(int slotIndex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return _devGodModeEnabled && slotIndex == 0 && _aliveMask[slotIndex];
#else
            return false;
#endif
        }

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

            if (_aliveMask[0] && _slots != null)
            {
                if (_devGodModeEnabled)
                    ActivateAllDjTargets(0);
                else if (!IsFeverActive(0))
                    SeedDjActiveTargets(0);
            }

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
