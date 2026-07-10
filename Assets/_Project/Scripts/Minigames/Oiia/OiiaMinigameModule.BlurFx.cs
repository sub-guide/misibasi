using MiniParty.Core;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        static readonly Color BlurFailFlashRed = new(1f, 0f, 0f, 0.45f);

        [Header("비참가(EMPTY) 슬롯")]
        [Tooltip("EMPTY 슬롯 Blur 검은 오버레이 알파(RGB는 항상 0,0,0).")]
        [Range(0f, 1f)]
        [SerializeField] float emptySlotBlurAlpha = 0.8f;

        Color BlurEmptySlotOverlayColor() => new(0f, 0f, 0f, emptySlotBlurAlpha);

        const float WaitingTextPulseSpeed = 2.2f;

        const float WaitingTextAlphaMin = 0.2f;

        const float WaitingTextAlphaMax = 1f;

        bool IsSlotEmptyForUi(int i)
        {
            return _ctx.Slots != null && i >= 0 && i < _ctx.Slots.Length && _ctx.Slots[i].State == SlotState.EMPTY;
        }

        static Color WaitingTextPulseColor()
        {
            float t = Mathf.Abs(Mathf.Sin(Time.unscaledTime * WaitingTextPulseSpeed));
            float alpha = Mathf.Lerp(WaitingTextAlphaMin, WaitingTextAlphaMax, t);
            return new Color(0.9f, 0.9f, 0.9f, alpha);
        }

        void ApplyEmptySlotBlurDrawOrder(int i, SlotUiBindings ui)
        {
            if (ui.Blur == null)
                return;

            Transform blurT = ui.Blur.transform;

            if (IsSlotEmptyForUi(i) && ui.WaitingText != null)
            {
                Transform waitingT = ui.WaitingText.transform;
                waitingT.SetAsLastSibling();
                int waitingIndex = waitingT.GetSiblingIndex();
                int blurIndex = Mathf.Max(0, waitingIndex - 1);
                if (blurT.GetSiblingIndex() != blurIndex)
                    blurT.SetSiblingIndex(blurIndex);
                if (waitingT.GetSiblingIndex() <= blurT.GetSiblingIndex())
                    waitingT.SetAsLastSibling();
                return;
            }

            if (_blurRestSiblingIndex[i] >= 0 && blurT.GetSiblingIndex() != _blurRestSiblingIndex[i])
                blurT.SetSiblingIndex(_blurRestSiblingIndex[i]);
        }

        const float BlurTierBumpWhiteBlinkSpeed = 26f;

        const float BlurTier3HueCycleSpeed = 0.22f;

        const float BlurTier3AlphaBlinkIntervalSeconds = 0.2f;

        const float BlurTier3AlphaBlinkOpaque = 0.45f;

        /// <summary>2·3티어 진입 직후(루프 완주 시) 짧은 흰색 블러 깜빡임.</summary>
        void TriggerTierBumpBlurOnLoopComplete(ref SlotRuntime sr)
        {
            int loops = sr.ConsecutiveLoopSuccesses;
            if (loops != 2 && loops != 3)
                return;

            sr.TierBumpBlurRemaining = TierBumpBlurDurationSeconds;
        }

        bool ShouldBlurTierBumpWarning(ref SlotRuntime sr, int i)
        {
            if (_ctx.IsPractice || !_aliveMask[i])
                return false;

            return sr.TierBumpBlurRemaining > 0f;
        }

        static Color BlurTierBumpWhiteBlinkColor()
        {
            float t = Mathf.Abs(Mathf.Sin(Time.unscaledTime * BlurTierBumpWhiteBlinkSpeed));
            float alpha = Mathf.Lerp(0.1f, 0.72f, t);
            return new Color(1f, 1f, 1f, alpha);
        }

        bool ShouldBlurTier3ChromaPulse(ref SlotRuntime sr, int i)
        {
            if (_ctx.IsPractice || !_aliveMask[i] || !MaintainingGameplayGauge(ref sr))
                return false;

            return sr.ConsecutiveLoopSuccesses >= 3;
        }

        static Color BlurTier3ChromaBlinkColor(int slotIndex)
        {
            float phase = Time.unscaledTime * BlurTier3HueCycleSpeed + slotIndex * 0.163f;
            float hue = phase - Mathf.Floor(phase);
            Color rgb = Color.HSVToRGB(hue, 0.82f, 1f);

            float step = Mathf.Max(0.0001f, BlurTier3AlphaBlinkIntervalSeconds);
            int slot = Mathf.FloorToInt(Time.unscaledTime / step);
            bool opaque = (slot & 1) == 0;
            rgb.a = opaque ? BlurTier3AlphaBlinkOpaque : 0f;

            return rgb;
        }
    }
}
