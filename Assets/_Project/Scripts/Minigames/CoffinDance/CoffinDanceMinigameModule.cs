using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// GAME 03 관짝춤 (Coffin Dance). 어깨 2점 지지 스냅(낙하 제외) + LB/RB 시소.
    /// 입력: L(button5)·R(button6) 어깨 승강 — <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class CoffinDanceMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "coffin_dance";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
