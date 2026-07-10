using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool _devGodModeEnabled;
#endif

        /// <summary>
        /// 개발자 무적 모드: 1P 슬롯만 <see cref="OiiaPhysicalButton.A"/> 입력을 항상 정답 처리.
        /// Editor·Development Build 에서만 <c>Backspace</c> 토글.
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
                $"[OiiaMinigameModule] Dev God Mode (1P A=always correct, other inputs ignored, main timer paused): " +
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

        void TickGameplayDevGodMode1P(int slotIndex, Joystick pad)
        {
            UpdateGuideHoldFeedbackDevGodMode1P(slotIndex, pad);

            if (!WasPhysicalPressed(slotIndex, pad, OiiaPhysicalButton.A))
                return;

            OnCorrectInput(slotIndex);
        }

        void UpdateGuideHoldFeedbackDevGodMode1P(int slotIndex, Joystick pad)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.ControllerGuideRoot == null)
                return;

            if (!_aliveMask[slotIndex] || IsSlotEmptyForUi(slotIndex))
                return;

            EnsureGuideNeonCaptured(slotIndex, b);

            Image[] buttons = GuideButtonsArray(b);
            int aIndex = GuideButtonIndexForPhysical(OiiaPhysicalButton.A);

            for (var k = 0; k < GuideButtonsPerSlot; k++)
            {
                Image btn = buttons[k];
                if (btn == null)
                    continue;

                bool held = k == aIndex && IsPhysicalHeld(slotIndex, pad, OiiaPhysicalButton.A);
                Vector3 restScale = _guideButtonRestScale[slotIndex][k];

                if (held)
                {
                    btn.rectTransform.localScale = restScale * GuideButtonHoldScale;
                    ApplyGuideButtonBrightness(btn, GuideButtonHoldBrightness);
                }
                else
                {
                    btn.rectTransform.localScale = restScale;
                    ApplyGuideButtonBrightness(btn, GuideButtonIdleBrightness);
                }
            }
        }
    }
}
