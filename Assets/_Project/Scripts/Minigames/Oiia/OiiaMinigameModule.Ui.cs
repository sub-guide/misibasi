using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>이 시간(초) 이하로 남으면 FEVER! 펄스.</summary>
        const float FeverEndPulseWindowSeconds = 1f;

        const float FeverPulseSpeed = 14f;

        const float FeverPulseAlphaMin = 0.35f;

        const float FeverPulseScaleMax = 1.12f;

        /// <summary>콤보 라벨(`COMBO`) 상대 크기(%). 숫자는 기본 fontSize.</summary>
        const float ComboLabelRelativeSizePercent = 55f;

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
            FlushWaitingUi(i, ui);
            FlushPanelBackgroundUi(i, ui, ref sr);
        }

        void FlushHudUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            // 소형 디스플레이: Score ↔ FEVER! 상호 배타 (한 종류만).
            bool fever = sr.FeverRemaining > 0f;
            FlushHudDisplayExclusive(ui, ref sr, fever);
            FlushComboHud(ui, ref sr);
            FlushFeverGauge(ui, ref sr);

            if (ui.PracticeReadyText == null)
                return;

            bool show = _ctx.IsPractice && _aliveMask[i] && _practiceReady[i];
            ui.PracticeReadyText.gameObject.SetActive(show);
            if (show)
                ui.PracticeReadyText.text = "READY";
        }

        /// <summary>
        /// HudDisplay — 피버 중 FEVER!만, 아니면 Score만.
        /// </summary>
        void FlushHudDisplayExclusive(SlotUiBindings ui, ref SlotRuntime sr, bool fever)
        {
            if (ui.HudScoreText != null)
            {
                ui.HudScoreText.gameObject.SetActive(!fever);
                if (!fever)
                    ui.HudScoreText.text = _ctx.IsPractice ? "-" : $"{sr.ScoreSum}";
            }

            if (ui.HudFeverText == null)
                return;

            ui.HudFeverText.gameObject.SetActive(fever);
            if (!fever)
            {
                ResetFeverHudVisual(ui.HudFeverText);
                return;
            }

            ui.HudFeverText.text = "FEVER!";

            bool pulse = sr.FeverRemaining <= FeverEndPulseWindowSeconds;
            if (!pulse)
            {
                ResetFeverHudVisual(ui.HudFeverText);
                return;
            }

            float t = Mathf.Abs(Mathf.Sin(Time.unscaledTime * FeverPulseSpeed));
            float alpha = Mathf.Lerp(FeverPulseAlphaMin, 1f, t);
            Color c = ui.HudFeverText.color;
            c.a = alpha;
            ui.HudFeverText.color = c;

            float scale = Mathf.Lerp(1f, FeverPulseScaleMax, t);
            ui.HudFeverText.rectTransform.localScale = new Vector3(scale, scale, 1f);
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
        /// DjBox 독립 피버 게이지 ×2. 비피버: FeverCharge 충전 · 피버: 남은 시간 소모 · 종료 시 0.
        /// </summary>
        static void FlushFeverGauge(SlotUiBindings ui, ref SlotRuntime sr)
        {
            float fill;
            if (sr.FeverRemaining > 0f)
                fill = sr.FeverRemaining / FeverDurationSeconds;
            else
                fill = sr.FeverCharge / (float)FeverComboThreshold;

            fill = Mathf.Clamp01(fill);
            SetFeverGaugeFill(ui.FeverGaugeImage, fill);
            SetFeverGaugeFill(ui.FeverGaugeImageB, fill);
        }

        static void SetFeverGaugeFill(Image gauge, float fill)
        {
            if (gauge != null)
                gauge.fillAmount = fill;
        }

        static void ResetFeverHudVisual(TMP_Text fever)
        {
            if (fever == null)
                return;

            Color c = fever.color;
            c.a = 1f;
            fever.color = c;
            fever.rectTransform.localScale = Vector3.one;
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
