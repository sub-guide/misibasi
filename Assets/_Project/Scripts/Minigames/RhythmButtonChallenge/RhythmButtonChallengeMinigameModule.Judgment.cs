using MiniParty.Input;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        void OpenBeatWindow()
        {
            ForEachSlot(i =>
            {
                if (_aliveMask[i])
                    _slots[i].BeatState = BeatJudgment.Pending;
            });
        }

        void ResetInputStreaks()
        {
            ForEachSlot(i =>
            {
                if (_aliveMask[i])
                    _slots[i].SuccessesThisInput = 0;
            });
        }

        void FinalizePendingBeats()
        {
            ForEachSlot(i =>
            {
                if (_aliveMask[i] && _slots[i].BeatState == BeatJudgment.Pending)
                    ApplyFail(i);
            });
        }

        void ApplyEightBeatBonus()
        {
            ForEachSlot(i =>
            {
                if (!_aliveMask[i] || _slots[i].SuccessesThisInput != BeatsPerSegment)
                    return;

                _slots[i].ScoreSum = ApplyScoreDeltaNonNegative(_slots[i].ScoreSum, scoreEightBeatBonus);
                PlayScorePopup(i, ScorePopupKind.Bonus);
                TriggerSlotShakeOnBonus(i);
                RefreshScoreLabel(i);
            });
        }

        void TickOperatorScoreGrants()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i] || !OperatorScoreGrantInput.WasPressed(i))
                    continue;

                GrantOperatorSuccessScore(i);
            }
        }

        /// <summary>운영자 부여. 성공 점수만 더하고 박 판정·연속 성공·8박 보너스는 건드리지 않는다.</summary>
        void GrantOperatorSuccessScore(int slotIndex)
        {
            _slots[slotIndex].ScoreSum = ApplyScoreDeltaNonNegative(
                _slots[slotIndex].ScoreSum,
                scoreSuccess);
            PlayScorePopup(slotIndex, ScorePopupKind.Success);
            RefreshScoreLabel(slotIndex);
        }

        void ApplySuccess(int slotIndex)
        {
            _slots[slotIndex].BeatState = BeatJudgment.Success;
            _slots[slotIndex].SuccessesThisInput++;
            _slots[slotIndex].ScoreSum = ApplyScoreDeltaNonNegative(
                _slots[slotIndex].ScoreSum,
                scoreSuccess);
            SetSlotOutlineJudged(slotIndex, success: true);
            PlayScorePopup(slotIndex, ScorePopupKind.Success);
            TriggerSlotShakeOnSuccess(slotIndex);
            RefreshScoreLabel(slotIndex);
        }

        void ApplyFail(int slotIndex)
        {
            _slots[slotIndex].BeatState = BeatJudgment.Fail;
            _slots[slotIndex].ScoreSum = ApplyScoreDeltaNonNegative(
                _slots[slotIndex].ScoreSum,
                scoreFail);
            SetSlotOutlineJudged(slotIndex, success: false);
            PlayScorePopup(slotIndex, ScorePopupKind.Fail);
            TriggerSlotShakeOnFail(slotIndex);
            RefreshScoreLabel(slotIndex);
        }
    }
}
