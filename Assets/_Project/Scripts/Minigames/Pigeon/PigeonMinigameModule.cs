using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    /// <summary>
    /// 비둘기야 먹자. 4인 공용 화면, D-Pad 커서 조준.
    /// 입력: 이동 stick, 쪽기 Face A — <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class PigeonMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "pigeon";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
