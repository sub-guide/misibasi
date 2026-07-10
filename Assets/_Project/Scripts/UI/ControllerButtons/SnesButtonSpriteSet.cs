using UnityEngine;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// Pixel SNES Controller Pack 버튼 스프라이트 묶음.
    /// Face: SuperFamicom A/B/X/Y · idle 필수.
    /// D-Pad 팔: DirectionalButtons/Color — 방향별 crop. idle=`*Unpressed` crop.
    /// Shoulder: ShoulderButtons/Color L/R — Face와 동일 단일 버튼(24×18).
    /// System: SystemButtons/Color Start/Select — 단일 버튼(21×20).
    /// 2D(무애니): SpriteSet에 <c>idle</c>·<c>highlighted</c>·<c>held</c>만 — <c>pressFrames</c> 비움.
    /// <see cref="SnesControllerButtonVisual"/> <c>instantHoldVisual</c> 또는 프레임 없음 → 즉시 Idle↔Held.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SnesButtonSpriteSet_",
        menuName = "Mini Party/UI/SNES Button Sprite Set")]
    public sealed class SnesButtonSpriteSet : ScriptableObject
    {
        [Header("식별")]
        [SerializeField] string displayName = "Face_A_SuperFamicom";

        [Header("스프라이트 (필수)")]
        [Tooltip("안 누른 기본. Face·Shoulder·System: *Unpressed. D-Pad 팔: *Unpressed 방향 crop.")]
        [SerializeField] Sprite idle;

        [Tooltip("강조(타겟). Face: *Highlighted. 비면 idle.")]
        [SerializeField] Sprite highlighted;

        [Header("스프라이트 (눌림)")]
        [Tooltip("*Press 시트 슬라이스 프레임(왼쪽→오른쪽). 눌림/해제 애니에 사용.")]
        [SerializeField] Sprite[] pressFrames;

        [Tooltip("누르고 있을 때 유지. Face: *Pressed / APresseed. 2D(무애니): *Pressed — pressFrames 비움.")]
        [SerializeField] Sprite held;

        public string DisplayName => displayName;

        public Sprite Idle => idle;

        public Sprite Highlighted => highlighted != null ? highlighted : idle;

        /// <summary>*Press 시트 애니 사용 여부. 2D는 보통 false(Unpressed+Pressed만).</summary>
        public bool HasPressAnimation => GetPressFrames().Length > 0;

        public Sprite Held
        {
            get
            {
                if (held != null)
                    return held;

                Sprite[] frames = GetPressFrames();
                if (frames.Length > 0)
                    return frames[frames.Length - 1];

                return idle;
            }
        }

        public bool HasIdle => idle != null;

        /// <summary>눌림 애니 프레임(좌→우). 없으면 빈 배열.</summary>
        public Sprite[] GetPressFrames()
        {
            if (pressFrames == null || pressFrames.Length == 0)
                return System.Array.Empty<Sprite>();

            var count = 0;
            for (var i = 0; i < pressFrames.Length; i++)
            {
                if (pressFrames[i] != null)
                    count++;
            }

            if (count == 0)
                return System.Array.Empty<Sprite>();

            if (count == pressFrames.Length)
                return pressFrames;

            var compact = new Sprite[count];
            var w = 0;
            for (var i = 0; i < pressFrames.Length; i++)
            {
                if (pressFrames[i] == null)
                    continue;
                compact[w++] = pressFrames[i];
            }

            return compact;
        }
    }
}
