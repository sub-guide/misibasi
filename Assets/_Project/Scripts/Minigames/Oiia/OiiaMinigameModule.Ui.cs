using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>콤보 라벨(`COMBO`) 상대 크기(%). 숫자는 기본 fontSize.</summary>
        const float ComboLabelRelativeSizePercent = 55f;

        void ApplySlotChrome(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            if (b.CatAnimator != null)
                b.CatAnimator.gameObject.SetActive(_aliveMask[i]);
        }

        void FlushUi(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            SlotRuntime sr = _slots[i];

            FlushHudUi(i, ui, ref sr);
            FlushSubPatternGuideUi(ui, ref sr);
            FlushWaitingUi(i, ui);
            FlushPanelBackgroundUi(i, ui, ref sr);
        }

        void FlushHudUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            FlushHudScore(ui, ref sr);
            FlushComboHud(ui, ref sr);
            FlushFeverGauge(ui, ref sr);

            if (ui.HudFeverText != null)
                ui.HudFeverText.gameObject.SetActive(false);

            if (ui.PracticeReadyText != null)
                ui.PracticeReadyText.gameObject.SetActive(false);
        }

        static void FlushHudScore(SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.HudScoreText == null)
                return;

            ui.HudScoreText.gameObject.SetActive(true);
            ui.HudScoreText.text = $"{sr.ScoreSum}";
        }

        /// <summary>
        /// Combo는 HudDisplay와 독립. 콤보 0 초과일 때만 숫자 강조 + 작은 `COMBO`.
        /// </summary>
        static void FlushComboHud(SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.HudComboText == null)
                return;

            bool showCombo = sr.Combo > 0;
            ui.HudComboText.gameObject.SetActive(showCombo);
            if (!showCombo)
                return;

            ui.HudComboText.richText = true;
            ui.HudComboText.text =
                $"{sr.Combo}<size={ComboLabelRelativeSizePercent}%> COMBO</size>";
        }

        /// <summary>
        /// DjBox 독립 게이지 ×2 — SubPattern 진행(matched/12). 패턴 완성 시 0으로 돌아간다.
        /// </summary>
        void FlushFeverGauge(SlotUiBindings ui, ref SlotRuntime sr)
        {
            int len = SubPatternLower.Length;
            float fill = len > 0 ? sr.SubPatternMatched / (float)len : 0f;

            fill = Mathf.Clamp01(fill);
            SetFeverGaugeFill(ui.FeverGaugeImage, fill);
            SetFeverGaugeFill(ui.FeverGaugeImageB, fill);
        }

        static void SetFeverGaugeFill(Image gauge, float fill)
        {
            if (gauge != null)
                gauge.fillAmount = fill;
        }

        void FlushWaitingUi(int i, SlotUiBindings ui)
        {
            if (ui.WaitingText == null)
                return;

            bool showWaiting = IsSlotEmptyForUi(i);
            ui.WaitingText.gameObject.SetActive(showWaiting);
            if (!showWaiting)
                return;

            ui.WaitingText.text = "WAITING";
            ui.WaitingText.color = WaitingTextPulseColor();
        }

        void FlushPanelBackgroundUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.SlotPanelBackgroundImage == null)
                return;

            ui.SlotPanelBackgroundImage.color = _slotPanelBgRestColor[i];
        }
    }
}
