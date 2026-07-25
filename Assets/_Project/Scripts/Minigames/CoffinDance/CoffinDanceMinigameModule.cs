using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// GAME 03 관짝춤 (Coffin Dance). 어깨 지지 순수 충돌 + ←/→ 무릎 자세 + A 자유 점프.
    /// 입력: ←/→ 어깨 승강, A(<c>button2</c>) 점프 — <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class CoffinDanceMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "coffin_dance";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
