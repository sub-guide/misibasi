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

                _slots[i].ScoreSum = ApplyScoreDeltaNonNegative(_slots[i].ScoreSum, ScoreEightBeatBonus);
                RefreshScoreLabel(i);
            });
        }

        void ApplySuccess(int slotIndex)
        {
            _slots[slotIndex].BeatState = BeatJudgment.Success;
            _slots[slotIndex].SuccessesThisInput++;
            _slots[slotIndex].ScoreSum = ApplyScoreDeltaNonNegative(
                _slots[slotIndex].ScoreSum,
                ScoreSuccess);
            SetSlotOutlineJudged(slotIndex, success: true);
            RefreshScoreLabel(slotIndex);
        }

        void ApplyFail(int slotIndex)
        {
            _slots[slotIndex].BeatState = BeatJudgment.Fail;
            _slots[slotIndex].ScoreSum = ApplyScoreDeltaNonNegative(
                _slots[slotIndex].ScoreSum,
                ScoreFail);
            SetSlotOutlineJudged(slotIndex, success: false);
            RefreshScoreLabel(slotIndex);
        }
    }
}
