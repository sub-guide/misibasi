using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        void ApplyStarBalloonCaptionLabel(TextMeshProUGUI label, TMP_FontAsset font, string text, float fontSize)
        {
            if (label == null)
                return;

            ApplyFont(label, font);
            label.gameObject.SetActive(true);
            label.richText = true;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.fontStyle = FontStyles.Normal;
            label.text = text;
        }

        static void LayoutGameplayDonationMessageSingleLine(TextMeshProUGUI caption)
        {
            if (caption == null)
                return;

            SetRectStretch(caption.rectTransform, new Vector2(-0.35f, -0.24f), new Vector2(1.35f, -0.05f));
            caption.alignment = TextAlignmentOptions.Center;
            caption.enableWordWrapping = false;
            caption.overflowMode = TextOverflowModes.Overflow;
        }

        void ApplyDonationTypography()
        {
            StyleTmp(centerTitleText, donationFont, centerStartTitleFontSize);
            StyleTmp(centerGameTitleText, centerTaglineFont, starBalloonTaglineFontSize, Color.white);
            StyleTmp(gameplayMessageText, donationFont, starBalloonMessageFontSize, Color.white);
        }

        static void StyleTmp(TextMeshProUGUI tmp, TMP_FontAsset font, float size, Color? color = null)
        {
            if (tmp == null)
                return;

            ApplyFont(tmp, font);
            tmp.fontSize = size;
            if (color.HasValue)
                tmp.color = color.Value;
        }

        static void ApplyFont(TextMeshProUGUI tmp, TMP_FontAsset font)
        {
            if (font == null)
                return;

            tmp.font = font;
            if (font.material != null)
                tmp.fontSharedMaterial = font.material;
        }
    }
}
