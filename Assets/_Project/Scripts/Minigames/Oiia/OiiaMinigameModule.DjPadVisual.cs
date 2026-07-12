using MiniParty.UI.ControllerButtons;
using UnityEngine;
using UnityEngine.Serialization;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("OIIA L/R 디제잉 박스 보정")]
        [Tooltip("L 강조 전용. 예: ShoulderButtons/Color/LHighlighted(black).png")]
        [SerializeField] Sprite oiiaLHighlightedBlack;

        [Tooltip("R 강조 전용. 예: ShoulderButtons/Color/RHighlighted(black).png")]
        [SerializeField] Sprite oiiaRHighlightedBlack;

        [Tooltip("Highlighted 스프라이트를 그릴 때만 L/R Image 흰색. Pressed/Held/Idle은 틴트 유지.")]
        [FormerlySerializedAs("oiiaLrForceWhiteIconColor")]
        [SerializeField] bool oiiaLrWhiteIconColorOnlyWhenHighlighted = true;

        [Header("OIIA ABXY Unpressed 명도 (이 미니게임만)")]
        [Tooltip("OIIA DjBox A/B/X/Y Idle(Unpressed)에만 명도 적용. L/R·D-Pad·다른 씬에는 영향 없음.")]
        [SerializeField] bool oiiaDjPadDimUnpressed = true;

        [Tooltip("ABXY Unpressed RGB 명도(0~255).")]
        [SerializeField] [Range(0, 255)] int oiiaDjPadUnpressedBrightness = 100;

        void ApplyOiiaLrDjBoxVisualOverrides(SlotUiBindings b)
        {
            if (b?.DjPadButtons == null)
                return;

            ApplyOiiaFaceUnpressedBrightness(b);

            ConfigureOiiaShoulderVisual(
                b.DjPadButtons[(int)OiiaDjPadButtonId.L],
                oiiaLHighlightedBlack);

            ConfigureOiiaShoulderVisual(
                b.DjPadButtons[(int)OiiaDjPadButtonId.R],
                oiiaRHighlightedBlack);
        }

        void ApplyOiiaFaceUnpressedBrightness(SlotUiBindings b)
        {
            ConfigureOiiaUnpressedBrightness(b.DjPadButtons[(int)OiiaDjPadButtonId.A]);
            ConfigureOiiaUnpressedBrightness(b.DjPadButtons[(int)OiiaDjPadButtonId.B]);
            ConfigureOiiaUnpressedBrightness(b.DjPadButtons[(int)OiiaDjPadButtonId.X]);
            ConfigureOiiaUnpressedBrightness(b.DjPadButtons[(int)OiiaDjPadButtonId.Y]);
        }

        void ConfigureOiiaUnpressedBrightness(SnesControllerButtonVisual visual)
        {
            if (visual == null)
                return;

            visual.ConfigureUnpressedBrightness(
                oiiaDjPadDimUnpressed,
                oiiaDjPadUnpressedBrightness);
        }

        void ConfigureOiiaShoulderVisual(SnesControllerButtonVisual visual, Sprite highlightedBlack)
        {
            if (visual == null)
                return;

            if (highlightedBlack != null)
                visual.SetHighlightedSpriteOverride(highlightedBlack);

            visual.ConfigureWhiteIconOnlyWhenShowingHighlighted(
                oiiaLrWhiteIconColorOnlyWhenHighlighted);
        }
    }
}
