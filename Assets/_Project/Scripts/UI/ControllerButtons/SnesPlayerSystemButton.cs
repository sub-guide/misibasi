using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// 플레이어(0~3) System Start 또는 Select 단일 버튼 시각 드라이버.
    /// <b>버튼 1개 = Prefab 1개</b> (<c>Button_Start</c> · <c>Button_Select</c>). ABXY·LR처럼 묶음 Prefab 없음.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SnesPlayerSystemButton : MonoBehaviour
    {
        public const int PlayerCount = 4;

        [Header("플레이어 (게임 세션 유지)")]
        [Tooltip("0=1P … 3=4P. PartySession.Slots[i] · SlotGamepad.Get(i)와 동일.")]
        [SerializeField] [Range(0, PlayerCount - 1)] int playerIndex;

        [Header("System 버튼")]
        [Tooltip("이 Prefab이 Start인지 Select인지. Button_Start / Button_Select 각각 고정.")]
        [SerializeField] SnesSystemButtonId systemButtonId = SnesSystemButtonId.Start;

        [Header("Visual")]
        [SerializeField] SnesControllerButtonVisual buttonVisual;

        [Header("입력 동기화")]
        [Tooltip("매 프레임 이 플레이어 패드로 SetHeld 동기화.")]
        [SerializeField] bool syncHeldFromPadEveryFrame = true;

        [Header("애니 타이밍 (초)")]
        [Tooltip("체크 시 아래 값을 Visual에 자동 반영(OnValidate).")]
        [SerializeField] bool applyAnimationSettingsToVisual = true;

        [SerializeField] [Min(0.001f)] float secondsPerSprite = 0.1f;
        [SerializeField] int pressFrameCount = -1;
        [SerializeField] float heldScale = 1f;

        [Header("2D (무애니)")]
        [SerializeField] bool instantHoldVisual;

        public int PlayerIndex => playerIndex;

        public SnesSystemButtonId SystemButtonId => systemButtonId;

        public SnesControllerButtonVisual ButtonVisual => buttonVisual;

        void Awake()
        {
            if (applyAnimationSettingsToVisual)
                PushAnimationSettingsToVisual();
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

        /// <summary>이 플레이어 패드의 Start/Select 홀드 상태를 Visual에 반영.</summary>
        public void TickHeldFromPad()
        {
            Joystick pad = SlotGamepad.Get(playerIndex);
            ApplyHeld(IsSystemHeld(playerIndex, pad, systemButtonId));
        }

        public void SetHighlighted(bool highlighted)
        {
            if (buttonVisual != null)
                buttonVisual.SetHighlighted(highlighted);
        }

        public void ResetVisual()
        {
            SetHighlighted(false);
            ApplyHeld(false);
        }

        public void AutoWireVisual()
        {
            if (buttonVisual != null)
                return;

            buttonVisual = GetComponent<SnesControllerButtonVisual>();
            if (buttonVisual != null)
                return;

            buttonVisual = FindChildVisual("BtnStart", "BtnSelect", "Button_Start", "Button_Select", "Start", "Select");
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            playerIndex = Mathf.Clamp(playerIndex, 0, PlayerCount - 1);
            AutoWireVisual();
            if (applyAnimationSettingsToVisual)
                PushAnimationSettingsToVisual();
        }
#endif

        void PushAnimationSettingsToVisual()
        {
            if (buttonVisual == null)
                return;

            buttonVisual.ApplyAnimationSettings(secondsPerSprite, pressFrameCount, heldScale, instantHoldVisual);
        }

        void ApplyHeld(bool held)
        {
            if (buttonVisual == null)
                return;

            buttonVisual.SetHeld(held);
        }

        static bool IsSystemHeld(int playerIndex, Joystick pad, SnesSystemButtonId id)
        {
            string path = id switch
            {
                SnesSystemButtonId.Start => BoothUsbGamepadLayout.Start,
                SnesSystemButtonId.Select => BoothUsbGamepadLayout.Select,
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
