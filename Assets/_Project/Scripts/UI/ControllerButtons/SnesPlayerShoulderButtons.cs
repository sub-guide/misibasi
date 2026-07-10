using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// 플레이어(0~3) 숄더 L/R 시각 드라이버.
    /// <see cref="SlotGamepad"/> · <see cref="BoothUsbGamepadLayout.ShoulderL"/> · <see cref="BoothUsbGamepadLayout.ShoulderR"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SnesPlayerShoulderButtons : MonoBehaviour
    {
        public const int PlayerCount = 4;

        [Header("플레이어 (게임 세션 유지)")]
        [Tooltip("0=1P … 3=4P. PartySession.Slots[i] · SlotGamepad.Get(i)와 동일.")]
        [SerializeField] [Range(0, PlayerCount - 1)] int playerIndex;

        [Header("Shoulder Visual")]
        [SerializeField] SnesControllerButtonVisual buttonL;
        [SerializeField] SnesControllerButtonVisual buttonR;

        [Header("입력 동기화")]
        [Tooltip("매 프레임 이 플레이어 패드로 SetHeld 동기화.")]
        [SerializeField] bool syncHeldFromPadEveryFrame = true;

        [Header("애니 타이밍 (초 · L/R 공통)")]
        [Tooltip("체크 시 아래 값을 BtnL/R Visual에 자동 반영(OnValidate).")]
        [SerializeField] bool applyAnimationSettingsToChildren = true;

        [SerializeField] [Min(0.001f)] float secondsPerSprite = 0.1f;
        [SerializeField] int pressFrameCount = -1;
        [SerializeField] float heldScale = 1f;

        [Header("2D (무애니)")]
        [SerializeField] bool instantHoldVisual;

        public int PlayerIndex => playerIndex;

        public SnesControllerButtonVisual ButtonL => buttonL;
        public SnesControllerButtonVisual ButtonR => buttonR;

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

        public SnesControllerButtonVisual GetVisual(SnesShoulderButtonId id) =>
            id switch
            {
                SnesShoulderButtonId.L => buttonL,
                SnesShoulderButtonId.R => buttonR,
                _ => null
            };

        public void TickHeldFromPad()
        {
            Joystick pad = SlotGamepad.Get(playerIndex);
            ApplyHeld(buttonL, IsShoulderHeld(playerIndex, pad, SnesShoulderButtonId.L));
            ApplyHeld(buttonR, IsShoulderHeld(playerIndex, pad, SnesShoulderButtonId.R));
        }

        public void SetHighlighted(SnesShoulderButtonId id, bool highlighted)
        {
            SnesControllerButtonVisual v = GetVisual(id);
            if (v != null)
                v.SetHighlighted(highlighted);
        }

        public void SetHighlightedOnly(SnesShoulderButtonId? id)
        {
            SetHighlighted(SnesShoulderButtonId.L, id == SnesShoulderButtonId.L);
            SetHighlighted(SnesShoulderButtonId.R, id == SnesShoulderButtonId.R);
        }

        public void ClearAllHighlights() => SetHighlightedOnly(null);

        public void ResetAllVisuals()
        {
            ClearAllHighlights();
            ApplyHeld(buttonL, false);
            ApplyHeld(buttonR, false);
        }

        public void AutoWireFromChildren()
        {
            if (buttonL == null)
                buttonL = FindChildVisual("BtnL", "Button_L", "L", "Shoulder_L");
            if (buttonR == null)
                buttonR = FindChildVisual("BtnR", "Button_R", "R", "Shoulder_R");
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
            PushTo(buttonL);
            PushTo(buttonR);
        }

        void PushTo(SnesControllerButtonVisual visual)
        {
            if (visual == null)
                return;

            visual.ApplyAnimationSettings(secondsPerSprite, pressFrameCount, heldScale, instantHoldVisual);
        }

        static void ApplyHeld(SnesControllerButtonVisual visual, bool held)
        {
            if (visual == null)
                return;

            visual.SetHeld(held);
        }

        static bool IsShoulderHeld(int playerIndex, Joystick pad, SnesShoulderButtonId id)
        {
            string path = id switch
            {
                SnesShoulderButtonId.L => BoothUsbGamepadLayout.ShoulderL,
                SnesShoulderButtonId.R => BoothUsbGamepadLayout.ShoulderR,
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
