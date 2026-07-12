using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// OIIA 미니게임 모듈 (디제잉 레이브 개편 중). partial 역할 분리.
    /// 1.5단계: 레거시 문자패턴·게이지·가이드·Burst/Shuffle 제거.
    /// 2단계: 상시 활성 타겟 3개 + SNES 10키 판정 (<see cref="OiiaDjPadButtonId"/>).
    /// </summary>
    public sealed partial class OiiaMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "oiia";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
