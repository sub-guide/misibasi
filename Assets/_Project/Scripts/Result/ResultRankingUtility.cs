using System.Collections.Generic;

namespace MiniParty.Result
{
    /// <summary>점수·참가 마스크로 등수(동점=같은 등수, 다음 등수는 건너뜀)와 공개 그룹을 만든다.</summary>
    public static class ResultRankingUtility
    {
        public sealed class RankRevealGroup
        {
            public int Rank;
            public int[] SlotIndices;
        }

        /// <summary>1부터 시작하는 등수를 <paramref name="rankOut"/> 에 채운다. 미참가 슬롯은 0.</summary>
        public static void FillRanks(int[] scores, bool[] participated, int[] rankOut)
        {
            if (rankOut == null || rankOut.Length == 0)
                return;

            for (var i = 0; i < rankOut.Length; i++)
                rankOut[i] = 0;

            if (scores == null || participated == null)
                return;

            var entries = new List<(int index, int score)>(4);
            for (var i = 0; i < rankOut.Length && i < participated.Length; i++)
            {
                if (!participated[i])
                    continue;

                int score = i < scores.Length ? scores[i] : 0;
                entries.Add((i, score));
            }

            if (entries.Count == 0)
                return;

            entries.Sort((a, b) => b.score.CompareTo(a.score));

            int nextRank = 1;
            var iEntry = 0;
            while (iEntry < entries.Count)
            {
                int score = entries[iEntry].score;
                int groupStart = iEntry;

                while (iEntry < entries.Count && entries[iEntry].score == score)
                    iEntry++;

                for (var g = groupStart; g < iEntry; g++)
                    rankOut[entries[g].index] = nextRank;

                nextRank += iEntry - groupStart;
            }
        }

        /// <summary>등수 오름차순 공개 그룹. 동일 등수 슬롯은 한 그룹.</summary>
        public static List<RankRevealGroup> BuildRevealGroups(int[] ranks, bool[] participated)
        {
            var groups = new List<RankRevealGroup>();
            if (ranks == null || participated == null)
                return groups;

            var rankToSlots = new SortedDictionary<int, List<int>>();

            for (var i = 0; i < ranks.Length && i < participated.Length; i++)
            {
                if (!participated[i])
                    continue;

                int rank = ranks[i];
                if (rank <= 0)
                    continue;

                if (!rankToSlots.TryGetValue(rank, out List<int> list))
                {
                    list = new List<int>(2);
                    rankToSlots[rank] = list;
                }

                list.Add(i);
            }

            foreach (KeyValuePair<int, List<int>> pair in rankToSlots)
            {
                groups.Add(new RankRevealGroup
                {
                    Rank = pair.Key,
                    SlotIndices = pair.Value.ToArray()
                });
            }

            return groups;
        }
    }
}
