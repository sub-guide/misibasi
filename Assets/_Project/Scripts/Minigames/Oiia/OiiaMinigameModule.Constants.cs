namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public const int SlotCount = 4;

        /// <summary>디제잉 레이브 SNES 10키 수. <see cref="OiiaDjPadButtonId"/> 와 동일.</summary>
        public const int DjPadButtonCount = 10;

        const float PracticeGaugeDrainSeconds = 10f;

        const float GaugeEmptyThreshold = 0.01f;

        const float FailFlashDurationSeconds = 0.25f;

        const float TierBumpBlurDurationSeconds = 0.5f;

        const float MainRoundMinSeconds = 1f;

        const float TypoBlinkSpeed = 12f;

        const float GuideButtonHoldScale = 0.87f;

        const float GuideButtonIdleBrightness = 0.2f;

        const float GuideButtonHoldBrightness = 1f;

        const float NeonShockwaveMinScale = 2f;

        const float NeonShockwaveMaxScale = 2.5f;

        const float NeonShockwaveDuration = 0.22f;

        /// <summary>고양이 Canvas sort(base+슬롯)보다 위. BurstText = base + SlotCount + offset + 슬롯.</summary>
        const int BurstTextDrawSortOrderOffset = 1;
    }
}
