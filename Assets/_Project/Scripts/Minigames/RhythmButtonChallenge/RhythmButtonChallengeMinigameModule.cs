using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// Rhythm Button Challenge. 4×2 보드, 성공/실패. 부스 패드 10키.
    /// <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class RhythmButtonChallengeMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "rhythm_button_challenge";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
