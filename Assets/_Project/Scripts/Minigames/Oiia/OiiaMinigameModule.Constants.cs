using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public const int SlotCount = 4;

        /// <summary>디제잉 레이브 SNES 10키 수. <see cref="OiiaDjPadButtonId"/> 와 동일.</summary>
        public const int DjPadButtonCount = 10;

        const float FailFlashDurationSeconds = 0.25f;

        const float TierBumpBlurDurationSeconds = 0.5f;

        const float MainRoundMinSeconds = 1f;
    }
}
