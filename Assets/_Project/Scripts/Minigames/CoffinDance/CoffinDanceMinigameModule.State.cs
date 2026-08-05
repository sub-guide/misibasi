using MiniParty.Input;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        MinigameContext _ctx;

        SlotRuntime[] _slots;
        bool _running;
        bool _completing;

        /// <summary>세션 참가(탈락해도 true 유지).</summary>
        readonly bool[] _participatedMask = new bool[SlotCount];

        /// <summary>현재 플레이 가능(탈락 시 false).</summary>
        readonly bool[] _aliveMask = new bool[SlotCount];

        readonly bool[] _practiceReady = new bool[SlotCount];

        readonly OperatorInputService _operatorInput = new();

        float _elapsedMainTime;
        float _remainingMainTime;
        float _endDelayRemain;
        CdFlowState _flowState;

        CdPhase _phase;
        float _scoreMultiplier = 1f;
    }
}
