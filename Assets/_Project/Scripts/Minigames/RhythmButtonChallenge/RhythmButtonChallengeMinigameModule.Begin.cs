using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            _ctx = context;
            gameObject.SetActive(true);
            _running = true;
            _completing = false;

            _sessionSeed = Random.Range(int.MinValue, int.MaxValue);
            _slots = new SlotRuntime[SlotCount];
            _aliveMask = new bool[SlotCount];

            ForEachSlot(i =>
            {
                _aliveMask[i] = _ctx.Slots[i].State == SlotState.PLAYING;
                _slots[i].ScoreSum = 0;
            });

            _stageIndex = 1;
            ResolveBoard();
            StartSegment(RbcSegmentKind.PhaseIntro);
        }
    }
}
