using MiniParty.UI.ControllerButtons;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

        void ApplyOiiaLrDjBoxVisualOverrides(SlotUiBindings b)
        {
            if (b?.DjPadButtons == null)
                return;

            ConfigureOiiaShoulderVisual(
                b.DjPadButtons[(int)OiiaDjPadButtonId.L],
                oiiaLHighlightedBlack);

            ConfigureOiiaShoulderVisual(
                b.DjPadButtons[(int)OiiaDjPadButtonId.R],
                oiiaRHighlightedBlack);
        }

        void ConfigureOiiaShoulderVisual(SnesControllerButtonVisual visual, Sprite highlightedBlack)
        {
            if (visual == null)
                return;

            if (highlightedBlack != null)
                visual.SetHighlightedSpriteOverride(highlightedBlack);

            Color rest = ReadIconColorOrWhite(visual);
            visual.ConfigureWhiteIconOnlyWhenShowingHighlighted(
                oiiaLrWhiteIconColorOnlyWhenHighlighted,
                rest);
        }

        static Color ReadIconColorOrWhite(SnesControllerButtonVisual visual)
        {
            if (visual == null)
                return Color.white;

            Image icon = visual.GetComponent<Image>();
            return icon != null ? icon.color : Color.white;
        }
    }
}
