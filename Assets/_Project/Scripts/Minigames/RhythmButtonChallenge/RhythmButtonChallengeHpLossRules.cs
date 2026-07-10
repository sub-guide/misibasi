using System.Collections.Generic;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// Rhythm Button Challenge 본게임 종료 시 HP 1 감소 대상 판정.
    /// (1) 총점 &lt; 500000 · (2) 참가 2명 이상이면 하위 50%.
    /// 500000점 정확히 달성 시 HP 감소 없음 (저점수 규칙 미해당).
    /// </summary>
    public static class RhythmButtonChallengeHpLossRules
    {
        public const int DefaultLowScoreThreshold = 500000;

        public static void FillHpLost(
            int[] finalScore,
            bool[] participated,
            bool[] hpLostOut,
            int lowScoreThreshold = DefaultLowScoreThreshold)
        {
            if (hpLostOut == null)
                return;

            for (var i = 0; i < hpLostOut.Length; i++)
                hpLostOut[i] = false;

            if (finalScore == null || participated == null)
                return;

            var active = new List<int>(4);
            for (var i = 0; i < participated.Length && i < finalScore.Length; i++)
            {
                if (participated[i])
                    active.Add(i);
            }

            int count = active.Count;
            if (count == 0)
                return;

            for (var a = 0; a < active.Count; a++)
            {
                int slot = active[a];
                if (finalScore[slot] < lowScoreThreshold)
                    hpLostOut[slot] = true;
            }

            if (count < 2)
                return;

            int bottomCount = count / 2;
            active.Sort((a, b) =>
            {
                int scoreCmp = finalScore[a].CompareTo(finalScore[b]);
                return scoreCmp != 0 ? scoreCmp : a.CompareTo(b);
            });

            for (var k = 0; k < bottomCount; k++)
                hpLostOut[active[k]] = true;
        }
    }
}
