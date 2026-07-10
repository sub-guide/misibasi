using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        void TickGameplayInput()
        {
            if (_flowState != RbcFlowState.StageInput || !_inputBeatWindow.Active)
                return;

            int beat = _inputBeatWindow.BeatIndex;
            RbcButton expected = _currentPattern[beat];

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                Joystick pad = SlotGamepad.Get(i);
                RbcButton? pressed = ReadAnyGameplayButtonPressed(i, pad);
                if (pressed == null)
                    return;

                ref SlotRuntime sr = ref _slots[i];

                if (!sr.BeatJudged[beat])
                {
                    RbcJudgment judgment;
                    if (pressed.Value != expected)
                    {
                        judgment = RbcJudgment.Wrong;
                    }
                    else
                    {
                        float deltaMs = (float)((Time.unscaledTimeAsDouble - _inputBeatWindow.StartTime) * 1000.0);
                        judgment = ResolveTimingJudgment(deltaMs);
                    }

                    ApplyJudgment(i, beat, judgment);
                    return;
                }

                sr.ExtraInputCountOnBeat++;
                sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, ScoreExtraInputPenalty);
                SetJudgmentImage(i, beat, RbcJudgment.Wrong);
            });
        }

        void FinalizeInputBeat(int beatIndex)
        {
            if (beatIndex < 0 || beatIndex >= BeatsPerSegment)
                return;

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                ref SlotRuntime sr = ref _slots[i];
                if (sr.BeatJudged[beatIndex])
                    return;

                ApplyJudgment(i, beatIndex, RbcJudgment.Miss);
            });

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                _slots[i].ExtraInputCountOnBeat = 0;
            });

            _inputBeatWindow = default;
        }

        RbcJudgment ResolveTimingJudgment(float deltaMs)
        {
            if (Mathf.Abs(deltaMs) <= JudgmentPerfectHalfWindowMs)
                return RbcJudgment.Perfect;

            if (deltaMs >= JudgmentEarlyMinMs && deltaMs < JudgmentEarlyMaxMs)
                return RbcJudgment.Fast;

            if (deltaMs > JudgmentLateMinMs && deltaMs <= JudgmentLateMaxMs)
                return RbcJudgment.Slow;

            return RbcJudgment.Miss;
        }

        void ApplyJudgment(int slotIndex, int beatIndex, RbcJudgment judgment)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.BeatJudged[beatIndex] = true;
            sr.BeatJudgments[beatIndex] = judgment;
            sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, ScoreDeltaForJudgment(judgment));
            SetJudgmentImage(slotIndex, beatIndex, judgment);
        }

        void ApplyEightBeatBonusIfEligible()
        {
            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                ref SlotRuntime sr = ref _slots[i];
                bool allGood = true;

                for (var b = 0; b < BeatsPerSegment; b++)
                {
                    RbcJudgment j = sr.BeatJudgments[b];
                    if (!IsGoodJudgment(j))
                    {
                        allGood = false;
                        break;
                    }
                }

                if (allGood)
                    sr.ScoreSum = ApplyScoreDeltaNonNegative(sr.ScoreSum, ScoreEightBeatBonus);
            });
        }
    }
}
