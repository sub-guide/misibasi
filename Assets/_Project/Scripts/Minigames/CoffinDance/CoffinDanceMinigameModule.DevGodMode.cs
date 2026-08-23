using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool _devGodModeEnabled;
#endif

        /// <summary>
        /// 개발 무적: 1P 시소. Backspace 토글. Editor·Development Build만.
        /// LB/RB 단독 입력은 그대로. 미입력·동시 입력이면 bias·x가 0.5로 복귀(노이즈·도치·풀 없음).
        /// </summary>
        bool IsDevGodModeSlot(int slotIndex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return _devGodModeEnabled && slotIndex == 0 && _aliveMask[slotIndex];
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
                $"[CoffinDanceMinigameModule] Dev God Mode (1P: LR works, idle returns x to 0.5): " +
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

        static bool IsExclusiveShoulderInput(float leftHeld, float rightHeld)
        {
            bool left = leftHeld > 0.5f;
            bool right = rightHeld > 0.5f;
            return left != right;
        }

        void StepDevGodIdleReturn(ref SlotRuntime sr, float dt)
        {
            sr.HoldTimer = 0f;
            float speed = Mathf.Max(0f, devGodReturnSpeed);
            sr.SeesawBias = Mathf.MoveTowards(sr.SeesawBias, 0.5f, speed * dt);
            sr.SeesawXCurrent = sr.SeesawBias;
        }
    }
}
