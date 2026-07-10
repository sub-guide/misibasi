using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// 플레이어(0~3 = 1P~4P) 전용 Face Y/X/A/B 시각 드라이버.
    /// Prefab은 Face 종류만, 인스턴스마다 <see cref="playerIndex"/>로 패드를 묶는다.
    /// PartySession 슬롯·<see cref="SlotGamepad"/>와 동일 인덱스.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SnesPlayerFaceButtons : MonoBehaviour
    {
        public const int PlayerCount = 4;

        [Header("플레이어 (게임 세션 유지)")]
        [Tooltip("0=1P … 3=4P. PartySession.Slots[i] · SlotGamepad.Get(i)와 동일.")]
        [SerializeField] [Range(0, PlayerCount - 1)] int playerIndex;

        [Header("Face Visual")]
        [SerializeField] SnesControllerButtonVisual buttonY;
        [SerializeField] SnesControllerButtonVisual buttonX;
        [SerializeField] SnesControllerButtonVisual buttonA;
        [SerializeField] SnesControllerButtonVisual buttonB;

        [Header("입력 동기화")]
        [Tooltip("매 프레임 이 플레이어 패드로 SetHeld 동기화. 미니게임이 TickHeldFromPad를 직접 부르면 false.")]
        [SerializeField] bool syncHeldFromPadEveryFrame = true;

        [Header("애니 타이밍 (초 · 4버튼 공통)")]
        [Tooltip("체크 시 아래 값을 BtnY/X/A/B Visual에 자동 반영(OnValidate).")]
        [SerializeField] bool applyAnimationSettingsToChildren = true;

        [SerializeField] [Min(0.001f)] float secondsPerSprite = 0.1f;
        [SerializeField] int pressFrameCount = -1;
        [SerializeField] float heldScale = 1f;

        [Header("2D (무애니)")]
        [Tooltip("자식 Visual에 즉시 Idle↔Held 전달. SpriteSet pressFrames 비움과 함께 사용.")]
        [SerializeField] bool instantHoldVisual;

        public float SecondsPerSprite => secondsPerSprite;

        public int PlayerIndex => playerIndex;

        public SnesControllerButtonVisual ButtonY => buttonY;
        public SnesControllerButtonVisual ButtonX => buttonX;
        public SnesControllerButtonVisual ButtonA => buttonA;
        public SnesControllerButtonVisual ButtonB => buttonB;

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

        /// <summary>Inspector / 부모에서 플레이어 번호 주입 (0~3).</summary>
        public void SetPlayerIndex(int index)
        {
            playerIndex = Mathf.Clamp(index, 0, PlayerCount - 1);
        }

        public SnesControllerButtonVisual GetVisual(SnesFaceButtonId id) =>
            id switch
            {
                SnesFaceButtonId.Y => buttonY,
                SnesFaceButtonId.X => buttonX,
                SnesFaceButtonId.A => buttonA,
                SnesFaceButtonId.B => buttonB,
                _ => null
            };

        /// <summary>이 플레이어 패드의 Face 홀드 상태를 네 Visual에 반영.</summary>
        public void TickHeldFromPad()
        {
            Joystick pad = SlotGamepad.Get(playerIndex);
            ApplyHeld(buttonY, IsFaceHeld(playerIndex, pad, SnesFaceButtonId.Y));
            ApplyHeld(buttonX, IsFaceHeld(playerIndex, pad, SnesFaceButtonId.X));
            ApplyHeld(buttonA, IsFaceHeld(playerIndex, pad, SnesFaceButtonId.A));
            ApplyHeld(buttonB, IsFaceHeld(playerIndex, pad, SnesFaceButtonId.B));
        }

        public void SetHighlighted(SnesFaceButtonId id, bool highlighted)
        {
            SnesControllerButtonVisual v = GetVisual(id);
            if (v != null)
                v.SetHighlighted(highlighted);
        }

        /// <summary>하나만 강조. 나머지는 해제. <paramref name="id"/>가 null이면 전부 해제.</summary>
        public void SetHighlightedOnly(SnesFaceButtonId? id)
        {
            SetHighlighted(SnesFaceButtonId.Y, id == SnesFaceButtonId.Y);
            SetHighlighted(SnesFaceButtonId.X, id == SnesFaceButtonId.X);
            SetHighlighted(SnesFaceButtonId.A, id == SnesFaceButtonId.A);
            SetHighlighted(SnesFaceButtonId.B, id == SnesFaceButtonId.B);
        }

        public void ClearAllHighlights() => SetHighlightedOnly(null);

        public void ResetAllVisuals()
        {
            ClearAllHighlights();
            ApplyHeld(buttonY, false);
            ApplyHeld(buttonX, false);
            ApplyHeld(buttonA, false);
            ApplyHeld(buttonB, false);
        }

        /// <summary>
        /// 자식 이름 BtnY / BtnX / BtnA / BtnB (또는 Face_Y 등)에서 Visual 자동 연결.
        /// </summary>
        public void AutoWireFromChildren()
        {
            if (buttonY == null)
                buttonY = FindChildVisual("BtnY", "Face_Y", "Y");
            if (buttonX == null)
                buttonX = FindChildVisual("BtnX", "Face_X", "X");
            if (buttonA == null)
                buttonA = FindChildVisual("BtnA", "Face_A", "A");
            if (buttonB == null)
                buttonB = FindChildVisual("BtnB", "Face_B", "B");
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
            PushTo(buttonY);
            PushTo(buttonX);
            PushTo(buttonA);
            PushTo(buttonB);
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

        static bool IsFaceHeld(int playerIndex, Joystick pad, SnesFaceButtonId id)
        {
            if (!SlotGamepad.HasInput(playerIndex))
                return false;

            return id switch
            {
                SnesFaceButtonId.X => BoothUsbSlotInput.PrimaryTriggerIsHeld(playerIndex, pad),
                SnesFaceButtonId.Y => BoothUsbSlotInput.IsPathHeld(playerIndex, pad, BoothUsbGamepadLayout.FaceY),
                SnesFaceButtonId.A => BoothUsbSlotInput.IsPathHeld(playerIndex, pad, BoothUsbGamepadLayout.FaceA),
                SnesFaceButtonId.B => BoothUsbSlotInput.IsPathHeld(playerIndex, pad, BoothUsbGamepadLayout.FaceB),
                _ => false
            };
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

            // 한 단계 더 깊은 자식 (ControllerGuide/BtnY 등)
            for (var i = 0; i < names.Length; i++)
            {
                SnesControllerButtonVisual deep = FindDeepChildVisual(transform, names[i]);
                if (deep != null)
                    return deep;
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
