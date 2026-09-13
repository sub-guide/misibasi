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

        int _stageIndex = 1;
        int _beatIndex;
        RbcSegmentKind _segmentKind = RbcSegmentKind.PhaseIntro;
        double _segmentStartTime;
        bool _clockStarted;

        RbcButton[] _currentPattern = new RbcButton[BeatsPerSegment];

        int _sessionSeed;
    }
}
