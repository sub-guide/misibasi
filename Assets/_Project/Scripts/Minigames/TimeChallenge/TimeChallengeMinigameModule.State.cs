using MiniParty.Minigames;
using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        enum Phase
        {
            Idle,
            Briefing,
            Countdown,
            Running,
            Result
        }

        MinigameContext _ctx;
        bool _running;
        bool _completing;
        Phase _phase;

        int _roundIndex;
        double _roundTargetSeconds;
        double _roundStartRealtime;
        float _phaseElapsed;

        readonly bool[] _participatedMask = new bool[SlotCount];
        readonly bool[] _clockStarted = new bool[SlotCount];
        readonly double[] _slotStartRealtime = new double[SlotCount];
        readonly bool[] _stopped = new bool[SlotCount];
        readonly double[] _records = new double[SlotCount];
        readonly double[] _errors = new double[SlotCount];
        readonly int[] _roundScores = new int[SlotCount];
        readonly int[] _totalScores = new int[SlotCount];
        bool _perfectHitThisResult;
        bool _perfectHitVideoStarted;

        bool _targetRestCached;
        Vector2 _targetRestPos;
        Vector2 _targetCloseupPos;
        Vector3 _targetRestScale;
        TextAlignmentOptions _targetRestAlign;
        float _targetAnim;
        float _targetAnimFrom;
        float _targetAnimTo;
        float _targetAnimElapsed;
        bool _targetSettling;

        readonly Color[] _playerSeatRest = new Color[SlotCount];
        readonly bool[] _playerSeatRestCached = new bool[SlotCount];
    }
}
