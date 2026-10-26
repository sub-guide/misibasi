using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// OIIA 미니게임. T1 빌드업 후 T2에서 SNES 10키 연타.
    /// </summary>
    public sealed partial class OiiaMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "oiia";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
