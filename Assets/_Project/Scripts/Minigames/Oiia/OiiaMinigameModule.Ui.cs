using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        void ApplySlotChrome(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            if (_ctx.IsPractice)
            {
                if (b.CatAnimator != null)
                    b.CatAnimator.gameObject.SetActive(false);
            }
            else if (b.CatAnimator != null)
            {
                b.CatAnimator.gameObject.SetActive(true);
            }
        }

        void FlushUi(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            SlotRuntime sr = _slots[i];

            FlushHudUi(i, ui, ref sr);
            FlushWaitingAndBlurUi(i, ui, ref sr);
            FlushPanelBackgroundUi(i, ui, ref sr);
        }

        void FlushHudUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.HudScoreText != null)
                ui.HudScoreText.text = _ctx.IsPractice ? "-" : $"{sr.ScoreSum}";

            if (ui.HudComboText != null)
                ui.HudComboText.text = _ctx.IsPractice ? string.Empty : $"{sr.Combo}";

            if (ui.HudFeverText != null && string.IsNullOrEmpty(ui.HudFeverText.text))
                ui.HudFeverText.gameObject.SetActive(false);

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
                MaintainingGameplaySustain(ref sr) &&
                ResolveGameplayTier(ref sr) >= 2;

            Color rest = _slotPanelBgRestColor[i];
            ui.SlotPanelBackgroundImage.color = tier2BgTransparent
                ? new Color(rest.r, rest.g, rest.b, 0f)
                : rest;
        }
    }
}
