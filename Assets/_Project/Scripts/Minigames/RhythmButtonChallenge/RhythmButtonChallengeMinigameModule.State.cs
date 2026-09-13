using MiniParty.Minigames;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        MinigameContext _ctx;

        SlotRuntime[] _slots;
        bool[] _aliveMask;

        bool _running;
        bool _completing;

        RbcButton[] _currentPattern = new RbcButton[BeatsPerSegment];

        int _sessionSeed;
    }
}
