using System.Text;
using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        static readonly Color SequenceTargetNeonGreen = new(0.224f, 1f, 0.078f, 1f);

        readonly StringBuilder _patternDisplayBuilder = new(64);

        readonly float[] _sequenceTextBaseFontSize = new float[SlotCount];

        void CaptureSequenceTextMainLayout(RectTransform rt, int slotIndex)
        {
            ref SequenceTextMainLayoutSnapshot s = ref _sequenceTextMainLayout[slotIndex];
            s.AnchorMin = rt.anchorMin;
            s.AnchorMax = rt.anchorMax;
            s.Pivot = rt.pivot;
            s.AnchoredPosition = rt.anchoredPosition;
            s.SizeDelta = rt.sizeDelta;
            _sequenceTextMainLayoutCaptured[slotIndex] = true;
        }

        static void RestoreSequenceTextMainLayout(RectTransform rt, ref SequenceTextMainLayoutSnapshot s)
        {
            rt.anchorMin = s.AnchorMin;
            rt.anchorMax = s.AnchorMax;
            rt.pivot = s.Pivot;
            rt.anchoredPosition = s.AnchoredPosition;
            rt.sizeDelta = s.SizeDelta;
        }

        static void ApplyPracticeCenteredSequenceText(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        void ApplySlotChromeAndSequenceLayout(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            if (b.SequenceText != null)
            {
                RectTransform rt = b.SequenceText.rectTransform;
                if (!_sequenceTextMainLayoutCaptured[i])
                    CaptureSequenceTextMainLayout(rt, i);

                if (_ctx.IsPractice)
                    ApplyPracticeCenteredSequenceText(rt);
                else if (_sequenceTextMainLayoutCaptured[i])
                    RestoreSequenceTextMainLayout(rt, ref _sequenceTextMainLayout[i]);
            }

            if (_ctx.IsPractice)
            {
                if (b.GaugeSlider != null)
                    b.GaugeSlider.gameObject.SetActive(false);

                if (b.CatAnimator != null)
                    b.CatAnimator.gameObject.SetActive(false);
            }
            else
            {
                if (b.GaugeSlider != null)
                    b.GaugeSlider.gameObject.SetActive(true);

                if (b.CatAnimator != null)
                    b.CatAnimator.gameObject.SetActive(true);
            }

            if (b.ShuffleEffect != null)
                HideShuffleEffect(b);
        }

        string BuildPatternSequenceDisplayText(ref SlotRuntime sr)
        {
            _patternDisplayBuilder.Clear();
            int cursor = Mathf.Clamp(sr.Cursor, 0, _patternLower.Length);

            string greenHex = ColorUtility.ToHtmlStringRGB(SequenceTargetNeonGreen);

            for (var i = 0; i < _patternLower.Length; i++)
            {
                char c = char.ToUpperInvariant(_patternLower[i]);

                if (i < cursor)
                {
                    _patternDisplayBuilder.Append("<color=#555555>");
                    _patternDisplayBuilder.Append(c);
                    _patternDisplayBuilder.Append("</color>");
                }
                else if (i == cursor)
                {
                    if (sr.InTypoState)
                    {
                        bool blinkOn = Mathf.Sin(Time.unscaledTime * TypoBlinkSpeed) > 0f;
                        string red = blinkOn ? "#FF2222" : "#AA0000";
                        _patternDisplayBuilder.Append($"<color={red}><b><size=120%>");
                        _patternDisplayBuilder.Append(c);
                        _patternDisplayBuilder.Append("</size></b></color>");
                    }
                    else
                    {
                        _patternDisplayBuilder.Append($"<color=#{greenHex}><b><size=120%>");
                        _patternDisplayBuilder.Append(c);
                        _patternDisplayBuilder.Append("</size></b></color>");
                    }
                }
                else
                {
                    _patternDisplayBuilder.Append("<color=#FFFFFF>");
                    _patternDisplayBuilder.Append(c);
                    _patternDisplayBuilder.Append("</color>");
                }
            }

            return _patternDisplayBuilder.ToString();
        }

        void FlushUi(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            SlotRuntime sr = _slots[i];

            FlushGaugeUi(ui, ref sr);
            FlushSequenceTextUi(i, ui, ref sr);
            FlushScoreAndReadyUi(i, ui, ref sr);
            FlushWaitingAndBlurUi(i, ui, ref sr);
            FlushPanelBackgroundUi(i, ui, ref sr);
            FlushGuideUi(i, ui, ref sr);
        }

        void FlushGaugeUi(SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (_ctx.IsPractice || ui.GaugeSlider == null)
                return;

            ui.GaugeSlider.value = Mathf.Clamp01(sr.Gauge01);
        }

        void FlushSequenceTextUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.SequenceText == null)
                return;

            ui.SequenceText.richText = true;
            ui.SequenceText.text = BuildPatternSequenceDisplayText(ref sr);

            float baseFont = _sequenceTextBaseFontSize[i] > 0f ? _sequenceTextBaseFontSize[i] : ui.SequenceText.fontSize;
            if (_sequenceTextBaseFontSize[i] <= 0f)
                _sequenceTextBaseFontSize[i] = baseFont;

            ui.SequenceText.fontSize = baseFont;
        }

        void FlushScoreAndReadyUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.ScoreText != null)
                ui.ScoreText.text = _ctx.IsPractice ? "-" : $"{sr.ScoreSum}";

            if (ui.PracticeReadyText == null)
                return;

            bool show = _ctx.IsPractice && _aliveMask[i] && _practiceReady[i];
            ui.PracticeReadyText.gameObject.SetActive(show);
            if (show)
                ui.PracticeReadyText.text = "READY";
        }

        void FlushWaitingAndBlurUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.WaitingText != null)
            {
                bool showWaiting = IsSlotEmptyForUi(i);
                ui.WaitingText.gameObject.SetActive(showWaiting);
                if (showWaiting)
                {
                    ui.WaitingText.text = "WAITING";
                    ui.WaitingText.color = WaitingTextPulseColor();
                }
            }

            if (ui.Blur == null)
                return;

            if (IsSlotEmptyForUi(i))
                ui.Blur.color = BlurEmptySlotOverlayColor();
            else if (sr.FailFlashTimer > 0f)
                ui.Blur.color = BlurFailFlashRed;
            else if (ShouldBlurTier3ChromaPulse(ref sr, i))
                ui.Blur.color = BlurTier3ChromaBlinkColor(i);
            else if (ShouldBlurTierBumpWarning(ref sr, i))
                ui.Blur.color = BlurTierBumpWhiteBlinkColor();
            else
                ui.Blur.color = Color.clear;

            ApplyEmptySlotBlurDrawOrder(i, ui);
        }

        void FlushPanelBackgroundUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.SlotPanelBackgroundImage == null)
                return;

            bool tier2BgTransparent =
                !_ctx.IsPractice &&
                _aliveMask[i] &&
                MaintainingGameplayGauge(ref sr) &&
                ResolveGameplayTier(ref sr) >= 2;

            Color rest = _slotPanelBgRestColor[i];
            ui.SlotPanelBackgroundImage.color = tier2BgTransparent
                ? new Color(rest.r, rest.g, rest.b, 0f)
                : rest;
        }
    }
}
