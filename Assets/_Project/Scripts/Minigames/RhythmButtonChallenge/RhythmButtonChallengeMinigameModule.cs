using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// Rhythm Button Challenge 미니게임. OIIA와 동일하게 partial로 역할 분리.
    /// 부스 패드: A/B/X/Y/L/R/방향 — <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class RhythmButtonChallengeMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "rhythm_button_challenge";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
