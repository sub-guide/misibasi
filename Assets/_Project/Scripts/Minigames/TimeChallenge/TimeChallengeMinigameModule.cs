using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>
    /// 시간 맞추기 챌린지. 숨김 타이머, A로 STOP, 3라운드 합산 점수.
    /// 입력: Face A — <see cref="MiniParty.Input.BoothUsbGamepadLayout"/>.
    /// </summary>
    public sealed partial class TimeChallengeMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "time_challenge";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
