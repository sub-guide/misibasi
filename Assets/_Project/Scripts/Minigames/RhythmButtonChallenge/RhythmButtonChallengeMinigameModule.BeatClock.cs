using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        void BeginPreIntroDelay()
        {
            _waitingPreIntro = true;
            _clockStarted = false;
            _preIntroDelayEndTime = Time.unscaledTimeAsDouble + preIntroDelaySeconds;
        }

        void TickPreIntroDelay()
        {
            if (!_waitingPreIntro)
                return;

            if (Time.unscaledTimeAsDouble >= _preIntroDelayEndTime)
            {
                _waitingPreIntro = false;
                StartSegment(RbcSegmentKind.PhaseIntro);
            }
        }

        void StartSegment(RbcSegmentKind kind)
        {
            _segmentKind = kind;
            _beatIndex = 0;
            _segmentStartTime = Time.unscaledTimeAsDouble;
            _clockStarted = true;
            if (kind == RbcSegmentKind.StageReveal)
                GeneratePatternForStage(_stageIndex);
            if (kind == RbcSegmentKind.StageInput)
                ResetInputStreaks();
            RefreshPhaseLabel();
            RefreshBoard();
            if (kind == RbcSegmentKind.StageInput)
                OpenBeatWindow();
            if (kind == RbcSegmentKind.PhaseIntro)
            {
                PrepareIntroSquareVisuals();
                PlayIntroSquareBeat(0);
            }
        }

        void TickBeatClock()
        {
            if (!_clockStarted)
                return;

            float duration = beatDurationSeconds;
            if (duration <= 0f)
            {
                Debug.LogError("[RhythmButtonChallengeMinigameModule] beatDurationSeconds 가 0 이하입니다.", this);
                return;
            }

            double elapsed = Time.unscaledTimeAsDouble - _segmentStartTime;
            int targetBeat = Mathf.Clamp(
                Mathf.FloorToInt((float)(elapsed / duration)),
                0,
                BeatsPerSegment - 1);
            if (_beatIndex != targetBeat)
            {
                if (_segmentKind == RbcSegmentKind.StageInput)
                    FinalizePendingBeats();
                _beatIndex = targetBeat;
                if (_segmentKind == RbcSegmentKind.StageInput)
                    OpenBeatWindow();
                if (_segmentKind == RbcSegmentKind.PhaseIntro)
                    PlayIntroSquareBeat(_beatIndex);
                RefreshBoard();
            }

            double segmentEnd = _segmentStartTime + duration * BeatsPerSegment;
            if (Time.unscaledTimeAsDouble >= segmentEnd)
                OnSegmentFinished();
        }

        void OnSegmentFinished()
        {
            _clockStarted = false;

            switch (_segmentKind)
            {
                case RbcSegmentKind.PhaseIntro:
                    _stageIndex = 1;
                    StartSegment(RbcSegmentKind.StageReveal);
                    break;

                case RbcSegmentKind.StageReveal:
                    StartSegment(RbcSegmentKind.StageInput);
                    break;

                case RbcSegmentKind.StageInput:
                    FinalizePendingBeats();
                    ApplyEightBeatBonus();
                    if (_stageIndex < StagesPerPhase)
                    {
                        _stageIndex++;
                        RefreshPhaseLabel();
                        StartSegment(RbcSegmentKind.StageReveal);
                    }
                    else
                    {
                        CompleteSession();
                    }

                    break;
            }
        }

        void RefreshPhaseLabel()
        {
            if (_segmentKind == RbcSegmentKind.PhaseIntro)
                return;

            if (phaseLabel == null)
            {
                Debug.LogError("[RhythmButtonChallengeMinigameModule] phaseLabel 을 Inspector에 연결하세요.", this);
                return;
            }

            phaseLabel.text = $"{_stageIndex}/{StagesPerPhase}";
        }
    }
}
