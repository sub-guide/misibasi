using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// 플레이어(0~3) D-Pad — <see cref="SnesDpadBaseVisual"/> 중앙 + crop 팔 4개.
    /// <see cref="SlotGamepad"/> · <see cref="BoothUsbGamepadLayout"/> stick/up|down|left|right.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SnesPlayerDpadButtons : MonoBehaviour
    {
        public const int PlayerCount = 4;

        [Header("플레이어 (게임 세션 유지)")]
        [Tooltip("0=1P … 3=4P. PartySession.Slots[i] · SlotGamepad.Get(i)와 동일.")]
        [SerializeField] [Range(0, PlayerCount - 1)] int playerIndex;

        [Header("D-Pad Visual")]
        [SerializeField] SnesDpadBaseVisual dpadBase;
        [SerializeField] SnesControllerButtonVisual buttonUp;
        [SerializeField] SnesControllerButtonVisual buttonDown;
        [SerializeField] SnesControllerButtonVisual buttonLeft;
        [SerializeField] SnesControllerButtonVisual buttonRight;

        [Header("입력 동기화")]
        [Tooltip("매 프레임 이 플레이어 패드로 SetHeld 동기화.")]
        [SerializeField] bool syncHeldFromPadEveryFrame = true;

        [Header("애니 타이밍 (초 · 4방향 공통)")]
        [Tooltip("체크 시 아래 값을 BtnUp/Down/Left/Right Visual에 자동 반영(OnValidate).")]
        [SerializeField] bool applyAnimationSettingsToChildren = true;

        [SerializeField] [Min(0.001f)] float secondsPerSprite = 0.1f;
        [SerializeField] int pressFrameCount = -1;
        [SerializeField] float heldScale = 1f;

        [Header("2D (무애니)")]
        [SerializeField] bool instantHoldVisual;

        public int PlayerIndex => playerIndex;

        public SnesDpadBaseVisual DpadBase => dpadBase;

        public SnesControllerButtonVisual ButtonUp => buttonUp;
        public SnesControllerButtonVisual ButtonDown => buttonDown;
        public SnesControllerButtonVisual ButtonLeft => buttonLeft;
        public SnesControllerButtonVisual ButtonRight => buttonRight;

        void Awake()
        {
            if (applyAnimationSettingsToChildren)
                PushAnimationSettingsToChildren();
        }

        void Update()
        {
            if (!syncHeldFromPadEveryFrame)
                return;

            TickHeldFromPad();
        }

        public void SetPlayerIndex(int index)
        {
            playerIndex = Mathf.Clamp(index, 0, PlayerCount - 1);
        }

        public SnesControllerButtonVisual GetVisual(SnesDpadButtonId id) =>
            id switch
            {
                SnesDpadButtonId.Up => buttonUp,
                SnesDpadButtonId.Down => buttonDown,
                SnesDpadButtonId.Left => buttonLeft,
                SnesDpadButtonId.Right => buttonRight,
                _ => null
            };

        public void TickHeldFromPad()
        {
            Joystick pad = SlotGamepad.Get(playerIndex);
            ApplyHeld(buttonUp, IsDpadHeld(playerIndex, pad, SnesDpadButtonId.Up));
            ApplyHeld(buttonDown, IsDpadHeld(playerIndex, pad, SnesDpadButtonId.Down));
            ApplyHeld(buttonLeft, IsDpadHeld(playerIndex, pad, SnesDpadButtonId.Left));
            ApplyHeld(buttonRight, IsDpadHeld(playerIndex, pad, SnesDpadButtonId.Right));
        }

        public void SetHighlighted(SnesDpadButtonId id, bool highlighted)
        {
            SnesControllerButtonVisual v = GetVisual(id);
            if (v != null)
                v.SetHighlighted(highlighted);
        }

        public void SetHighlightedOnly(SnesDpadButtonId? id)
        {
            SetHighlighted(SnesDpadButtonId.Up, id == SnesDpadButtonId.Up);
            SetHighlighted(SnesDpadButtonId.Down, id == SnesDpadButtonId.Down);
            SetHighlighted(SnesDpadButtonId.Left, id == SnesDpadButtonId.Left);
            SetHighlighted(SnesDpadButtonId.Right, id == SnesDpadButtonId.Right);
        }

        public void ClearAllHighlights() => SetHighlightedOnly(null);

        public void ResetAllVisuals()
        {
            ClearAllHighlights();
            ApplyHeld(buttonUp, false);
            ApplyHeld(buttonDown, false);
            ApplyHeld(buttonLeft, false);
            ApplyHeld(buttonRight, false);
        }

        public void AutoWireFromChildren()
        {
            if (dpadBase == null)
                dpadBase = FindChildBase("DpadBase", "BtnBase", "Base");

            if (buttonUp == null)
                buttonUp = FindChildVisual("BtnUp", "Dpad_Up", "Up", "Button_Up");
            if (buttonDown == null)
                buttonDown = FindChildVisual("BtnDown", "Dpad_Down", "Down", "Button_Down");
            if (buttonLeft == null)
                buttonLeft = FindChildVisual("BtnLeft", "Dpad_Left", "Left", "Button_Left");
            if (buttonRight == null)
                buttonRight = FindChildVisual("BtnRight", "Dpad_Right", "Right", "Button_Right");
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            playerIndex = Mathf.Clamp(playerIndex, 0, PlayerCount - 1);
            AutoWireFromChildren();
            if (applyAnimationSettingsToChildren)
                PushAnimationSettingsToChildren();
        }
#endif

        void PushAnimationSettingsToChildren()
        {
            PushTo(buttonUp);
            PushTo(buttonDown);
            PushTo(buttonLeft);
            PushTo(buttonRight);
        }

        void PushTo(SnesControllerButtonVisual visual)
        {
            if (visual == null)
                return;

            visual.ApplyAnimationSettings(secondsPerSprite, pressFrameCount, heldScale, instantHoldVisual);
        }

        SnesDpadBaseVisual FindChildBase(params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                Transform t = transform.Find(names[i]);
                if (t == null)
                    continue;

                SnesDpadBaseVisual b = t.GetComponent<SnesDpadBaseVisual>();
                if (b != null)
                    return b;
            }

            for (var i = 0; i < names.Length; i++)
            {
                SnesDpadBaseVisual nested = FindDeepChildBase(transform, names[i]);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        static SnesDpadBaseVisual FindDeepChildBase(Transform root, string name)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == name)
                {
                    SnesDpadBaseVisual b = child.GetComponent<SnesDpadBaseVisual>();
                    if (b != null)
                        return b;
                }

                SnesDpadBaseVisual nested = FindDeepChildBase(child, name);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        static void ApplyHeld(SnesControllerButtonVisual visual, bool held)
        {
            if (visual == null)
                return;

            visual.SetHeld(held);
        }

        static bool IsDpadHeld(int playerIndex, Joystick pad, SnesDpadButtonId id)
        {
            string path = id switch
            {
                SnesDpadButtonId.Up => BoothUsbGamepadLayout.StickUp,
                SnesDpadButtonId.Down => BoothUsbGamepadLayout.StickDown,
                SnesDpadButtonId.Left => BoothUsbGamepadLayout.StickLeft,
                SnesDpadButtonId.Right => BoothUsbGamepadLayout.StickRight,
                _ => null
            };

            if (string.IsNullOrEmpty(path))
                return false;

            if (!SlotGamepad.HasInput(playerIndex))
                return false;

            return BoothUsbSlotInput.IsPathHeld(playerIndex, pad, path);
        }

        SnesControllerButtonVisual FindChildVisual(params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                Transform t = transform.Find(names[i]);
                if (t == null)
                    continue;

                SnesControllerButtonVisual v = t.GetComponent<SnesControllerButtonVisual>();
                if (v != null)
                    return v;
            }

            for (var i = 0; i < names.Length; i++)
            {
                SnesControllerButtonVisual nested = FindDeepChildVisual(transform, names[i]);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        static SnesControllerButtonVisual FindDeepChildVisual(Transform root, string name)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == name)
                {
                    SnesControllerButtonVisual v = child.GetComponent<SnesControllerButtonVisual>();
                    if (v != null)
                        return v;
                }

                SnesControllerButtonVisual nested = FindDeepChildVisual(child, name);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}
