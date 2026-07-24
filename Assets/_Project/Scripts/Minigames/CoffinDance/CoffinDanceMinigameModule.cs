using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// GAME 03 관짝춤 (Coffin Dance). 2점 역선풍기 균형 + JUMP 이벤트.
    /// 입력: ←/→ 복원력, A(<c>button2</c>) 점프 — <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class CoffinDanceMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "coffin_dance";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
