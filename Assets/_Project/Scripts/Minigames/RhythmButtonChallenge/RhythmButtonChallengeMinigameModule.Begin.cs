using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            ResolveBoardAndScoreBindings();

            _ctx = context;
            gameObject.SetActive(true);
            _running = true;
            _completing = false;

            _sessionSeed = Random.Range(int.MinValue, int.MaxValue);
            _slots = new SlotRuntime[SlotCount];
            _aliveMask = new bool[SlotCount];

            ForEachSlot(i =>
            {
                bool play = _ctx.Slots[i].State == SlotState.PLAYING;
                _aliveMask[i] = play;

                ref SlotRuntime sr = ref _slots[i];
                sr.ScoreSum = 0;
                sr.BeatJudgments = new RbcJudgment[BeatsPerSegment];
                sr.BeatJudged = new bool[BeatsPerSegment];
                sr.ExtraInputCountOnBeat = 0;
            });

            ApplyAudioPitch(Phase1Pitch);
            HideSpeedUpOverlay();
            ClearAllJudgmentImages();
            ClearBoardIcons();

            _phaseNumber = 1;
            _stageIndex = 1;
            BeginPhaseIntro();
            FlushAllUi();
        }

        void BeginPhaseIntro()
        {
            _flowState = RbcFlowState.PhaseIntro;
            _segmentKind = RbcSegmentKind.PhaseIntro;
            _beatIndex = 0;
            _segmentAudioStarted = false;
            _inputBeatWindow = default;
        }

        void BeginStageReveal(int stageIndex)
        {
            _flowState = RbcFlowState.StageReveal;
            _stageIndex = stageIndex;
            _segmentKind = RbcSegmentKind.StageReveal;
            _beatIndex = 0;
            _segmentAudioStarted = false;
            _inputBeatWindow = default;

            GeneratePatternForStage(stageIndex);
            ClearAllJudgmentImages();
        }

        void BeginStageInput()
        {
            _flowState = RbcFlowState.StageInput;
            _segmentKind = RbcSegmentKind.StageInput;
            _beatIndex = 0;
            _segmentAudioStarted = false;
            _inputBeatWindow = default;

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                ref SlotRuntime sr = ref _slots[i];
                for (var b = 0; b < BeatsPerSegment; b++)
                {
                    sr.BeatJudgments[b] = RbcJudgment.None;
                    sr.BeatJudged[b] = false;
                }

                sr.ExtraInputCountOnBeat = 0;
            });
        }
    }
}
