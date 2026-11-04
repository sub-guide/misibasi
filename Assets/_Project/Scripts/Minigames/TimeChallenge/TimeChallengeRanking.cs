using System.Collections.Generic;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>
    /// 라운드 오차 순위 → 점수. 동점은 같은 순위·같은 점수, 다음 순위 스킵.
    /// 미STOP은 항상 300 (마지막 라운드 ×2 없음).
    /// </summary>
    public static class TimeChallengeRanking
    {
        public static readonly int[] PlaceScores = { 1000, 700, 500, 300 };
        public const int MissScore = 300;

        public static int ScoreForPlace(int place, bool lastRoundDouble)
        {
            if (place < 1 || place > PlaceScores.Length)
                return 0;

            int score = PlaceScores[place - 1];
            return lastRoundDouble ? score * 2 : score;
        }

        public static void FillRoundScores(
            bool[] participated,
            bool[] stopped,
            double[] errors,
            bool lastRoundDouble,
            int[] roundScoreOut)
        {
            if (roundScoreOut == null)
                return;

            for (var i = 0; i < roundScoreOut.Length; i++)
                roundScoreOut[i] = 0;

            if (participated == null || stopped == null || errors == null)
                return;

            var stoppedSlots = new List<int>(4);
            for (var i = 0; i < roundScoreOut.Length && i < participated.Length; i++)
            {
                if (!participated[i])
                    continue;

                if (i < stopped.Length && stopped[i])
                    stoppedSlots.Add(i);
                else
                    roundScoreOut[i] = MissScore;
            }

            stoppedSlots.Sort((a, b) =>
            {
                int cmp = DisplayError(errors[a]).CompareTo(DisplayError(errors[b]));
                return cmp != 0 ? cmp : a.CompareTo(b);
            });

            var iEntry = 0;
            while (iEntry < stoppedSlots.Count)
            {
                int error = DisplayError(errors[stoppedSlots[iEntry]]);
                int groupStart = iEntry;
                while (iEntry < stoppedSlots.Count &&
                       DisplayError(errors[stoppedSlots[iEntry]]) == error)
                    iEntry++;

                int place = groupStart + 1;
                int score = ScoreForPlace(place, lastRoundDouble);
                if (score <= 0)
                    score = PlaceScores[System.Math.Min(place - 1, PlaceScores.Length - 1)];

                for (var g = groupStart; g < iEntry; g++)
                    roundScoreOut[stoppedSlots[g]] = score;
            }
        }

        public static void ApplyPerfectHitBonus(
            bool[] participated,
            bool[] stopped,
            double[] records,
            double targetSeconds,
            int[] roundScoreOut)
        {
            if (roundScoreOut == null || participated == null || stopped == null || records == null)
                return;

            for (var i = 0; i < roundScoreOut.Length; i++)
            {
                if (i >= participated.Length || !participated[i])
                    continue;
                if (i >= stopped.Length || !stopped[i])
                    continue;
                if (TimeChallengeLedClock.DisplayErrorCentiseconds(records[i], targetSeconds) != 0)
                    continue;

                roundScoreOut[i] *= 2;
            }
        }

        static int DisplayError(double errorSeconds)
        {
            return TimeChallengeLedClock.ToCentiseconds(errorSeconds);
        }
    }
}
