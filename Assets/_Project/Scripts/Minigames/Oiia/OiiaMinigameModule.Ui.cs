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
                b.CatAnimator.gameObject.SetActive(_aliveMask[i] && _tier2Active);
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
        /// 현재 티어의 <c>Gauge_L_n</c> · <c>Gauge_R_n</c>만 켜고 채운다. 12글자 가이드와는 별개다.
        /// </summary>
        void FlushFeverGauge(SlotUiBindings ui, ref SlotRuntime sr)
        {
            int tier = sr.Tier;
            if (tier < PlayerTierMin)
                tier = PlayerTierMin;
            if (tier > PlayerTierMax)
                tier = PlayerTierMax;

            float fill = Mathf.Clamp01(sr.Gauge / GaugeMaxForTier(tier));
            ApplyTierGaugePair(ui.TierGaugeLeft, tier, fill);
            ApplyTierGaugePair(ui.TierGaugeRight, tier, fill);
        }

        static void ApplyTierGaugePair(Image[] gauges, int tier, float fill)
        {
            if (gauges == null)
                return;

            for (var i = 0; i < gauges.Length; i++)
            {
                Image gauge = gauges[i];
                if (gauge == null)
                    continue;

                bool show = i == tier - 1;
                if (gauge.gameObject.activeSelf != show)
                    gauge.gameObject.SetActive(show);

                if (show)
                    gauge.fillAmount = fill;
            }
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
