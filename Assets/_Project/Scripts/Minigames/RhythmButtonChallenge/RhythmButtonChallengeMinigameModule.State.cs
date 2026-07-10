using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        MinigameContext _ctx;

        SlotRuntime[] _slots;
        bool[] _aliveMask;

        bool _running;
        bool _completing;

        int _phaseNumber = 1;
        int _stageIndex = 1;
        RbcSegmentKind _segmentKind = RbcSegmentKind.PhaseIntro;
        int _beatIndex;

        RbcFlowState _flowState = RbcFlowState.PhaseIntro;

        RbcButton[] _currentPattern = new RbcButton[BeatsPerSegment];

        double _segmentStartTime;
        float _beatDurationSec;
        BeatWindow _inputBeatWindow;

        float _speedUpTimer;
        bool _segmentAudioStarted;

        int _sessionSeed;
    }
}
