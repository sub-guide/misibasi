using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        enum PourPhase
        {
            Cooldown,
            Forward,
            Spawning,
            Reverse
        }

        MinigameContext _ctx;
        bool _running;
        readonly bool[] _participatedMask = new bool[SlotCount];

        bool _pourArmed;
        PourPhase _pourPhase;
        float _pourElapsed;
        float _pourClipDuration;
        int _nextSortingOrder;

        Vector2[] _pourJitters;
        int _pourJitterCount;
        int _pourJitterIndex;
    }
}
