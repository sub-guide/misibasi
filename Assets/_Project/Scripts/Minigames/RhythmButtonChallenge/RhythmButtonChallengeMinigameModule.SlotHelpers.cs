using System;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        static void ForEachSlot(Action<int> action)
        {
            for (var i = 0; i < SlotCount; i++)
                action(i);
        }

        static int ApplyScoreDeltaNonNegative(int current, int delta) =>
            UnityEngine.Mathf.Max(0, current + delta);

        static bool IsGoodJudgment(RbcJudgment j) =>
            j is RbcJudgment.Perfect or RbcJudgment.Fast or RbcJudgment.Slow;

        int ScoreDeltaForJudgment(RbcJudgment j) =>
            j switch
            {
                RbcJudgment.Perfect => ScorePerfect,
                RbcJudgment.Fast => ScoreFast,
                RbcJudgment.Slow => ScoreSlow,
                RbcJudgment.Miss => ScoreMiss,
                RbcJudgment.Wrong => ScoreWrong,
                _ => 0
            };
    }
}
