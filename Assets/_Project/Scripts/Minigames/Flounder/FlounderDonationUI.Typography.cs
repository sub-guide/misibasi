using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        void LayoutStarBalloonTextAnchors()
        {
            if (PreserveCenterDonationLayout)
                return;

            if (centerAmountText != null)
                SetRectStretch(centerAmountText.rectTransform, new Vector2(0.2f, 0.68f), new Vector2(0.8f, 0.96f));

            if (centerGameTitleText != null)
                LayoutCenterTaglineBelowBanner();
        }

        void ApplyCenterTaglineLabel(string text)
        {
            ApplyStarBalloonCaptionLabel(
                centerGameTitleText,
                ResolveCenterTaglineFont(),
                text,
                starBalloonTaglineFontSize,
                preserveRectLayout: PreserveCenterDonationLayout);
        }

        void ApplyStarBalloonCaptionLabel(
            TextMeshProUGUI label,
            TMP_FontAsset font,
            string text,
            float? fontSize = null,
            bool preserveRectLayout = false)
        {
            if (label == null)
                return;

            if (font != null)
            {
                label.font = font;
                if (font.material != null)
                    label.fontSharedMaterial = font.material;
            }

            label.gameObject.SetActive(true);
            label.richText = true;
            label.fontSize = fontSize ?? starBalloonMessageFontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.fontStyle = FontStyles.Normal;
            label.text = text;

            if (!preserveRectLayout)
                LayoutDonationCaptionBelowBanner(label);
        }

        void LayoutCenterTaglineBelowBanner()
        {
            if (centerGameTitleText == null)
                return;

            LayoutDonationCaptionBelowBanner(centerGameTitleText);
        }

        static void LayoutDonationCaptionBelowBanner(TextMeshProUGUI caption)
        {
            if (caption == null)
                return;

            SetRectStretch(caption.rectTransform, new Vector2(0.02f, -0.42f), new Vector2(0.98f, -0.04f));
            caption.alignment = TextAlignmentOptions.Center;
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

        void EnsureCenterTaglineFont()
        {
            if (centerTaglineFont != null)
                return;

            centerTaglineFont = LoadEditorFont(FlounderDonationUiConstants.CenterTaglineFontPath);
        }

        TMP_FontAsset ResolveCenterTaglineFont()
        {
            EnsureCenterTaglineFont();
            if (centerTaglineFont != null)
                return centerTaglineFont;

            return ResolveDonationFont();
        }

        void EnsureCenterTaglineFontOn(TextMeshProUGUI label)
        {
            if (label == null)
                return;

            TMP_FontAsset font = ResolveCenterTaglineFont();
            if (font == null)
                return;

            label.font = font;
            if (font.material != null)
                label.fontSharedMaterial = font.material;
        }

        static TMP_FontAsset LoadEditorFont(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
#else
            return null;
#endif
        }

        TMP_FontAsset ResolveDonationFont()
        {
            if (donationFont != null)
                return donationFont;

            if (typographyReferenceSlot != null && typographyReferenceSlot.LabelFont != null)
                return typographyReferenceSlot.LabelFont;

            if (centerAmountText != null && centerAmountText.font != null)
                return centerAmountText.font;

            return null;
        }

        void EnsureDonationFontOn(TextMeshProUGUI label)
        {
            if (label == null)
                return;

            TMP_FontAsset font = ResolveDonationFont() ?? label.font;
            if (font == null)
                return;

            label.font = font;
            if (font.material != null)
                label.fontSharedMaterial = font.material;
        }

        void ApplyDonationTypography()
        {
            FlounderPlayerSlotView reference = typographyReferenceSlot;
            if (reference == null)
            {
                FlounderPlayerSlotView[] slots = FindObjectsOfType<FlounderPlayerSlotView>(true);
                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i].SlotIndex == 0)
                    {
                        reference = slots[i];
                        break;
                    }
                }
            }

            TMP_FontAsset font = ResolveDonationFont();
            float labelSize = reference != null ? reference.LabelFontSize : 60f;

            StyleTmp(centerSpecialHeaderText, font, labelSize * 0.43f, new Color(1f, 0.88f, 0.4f, 1f));
            StyleTmp(centerTitleText, font, centerStartTitleFontSize);
            StyleTmp(centerAmountText, font, labelSize * 0.85f, FlounderDonationUiConstants.CenterAmountGold);
            StyleTmp(centerGameTitleText, ResolveCenterTaglineFont(), starBalloonTaglineFontSize, Color.white);
            StyleTmp(gameplayHeaderText, font, labelSize * 0.36f, new Color(1f, 0.88f, 0.4f, 1f));
            StyleTmp(gameplayMessageText, font, starBalloonMessageFontSize, Color.white);
            StyleTmp(gameplayAmountText, font, labelSize * gameplayAmountFontRatio, FlounderDonationUiConstants.GameplayAmountGold);
            StyleTmp(gameplayGameTitleText, font, labelSize * 0.36f);
        }

        static void StyleTmp(TextMeshProUGUI tmp, TMP_FontAsset font, float size, Color? color = null)
        {
            if (tmp == null)
                return;

            if (font != null)
            {
                tmp.font = font;
                if (font.material != null)
                    tmp.fontSharedMaterial = font.material;
            }

            tmp.fontSize = size;
            if (color.HasValue)
                tmp.color = color.Value;
        }
    }
}
