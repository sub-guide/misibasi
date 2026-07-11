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
        /// Dev God Mode: 본게임 타이머 정지. 1.5단계에서는 입력 자동 정답 없음(2단계 이후 재정의).
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
            Debug.Log(
                $"[OiiaMinigameModule] Dev God Mode (main timer paused; pad auto-correct deferred to Phase 2): " +
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
    }
}
