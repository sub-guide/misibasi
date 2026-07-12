using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public const int SlotCount = 4;

        /// <summary>디제잉 레이브 SNES 10키 수. <see cref="OiiaDjPadButtonId"/> 와 동일.</summary>
        public const int DjPadButtonCount = 10;

        /// <summary>상시 유지하는 활성 타겟 개수.</summary>
        public const int DjActiveTargetCount = 3;

        /// <summary>SubPatternGuide 고정 문구(레거시 Sequence와 동일). 표시는 대문자 접두만.</summary>
        const string SubPatternLower = "oiiaiooiiiai";

        const float InputLockAfterMissSeconds = 0.35f;

        const float MainRoundMinSeconds = 1f;
    }
}
