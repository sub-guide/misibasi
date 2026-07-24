using System.Collections.Generic;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 관짝춤 본게임 HP −1 판정.
    /// · 1인: 총점 &lt; 저점수 컷(기본 3000)
    /// · 2인 이상: 하위 50%만 (저점수 컷 없음). 탈락자도 탈락 시점 점수로 순위 산정.
    /// </summary>
    public static class CoffinDanceHpLossRules
    {
        public const int DefaultLowScoreThreshold = 3000;

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

            if (count == 1)
            {
                int slot = active[0];
                if (finalScore[slot] < lowScoreThreshold)
                    hpLostOut[slot] = true;
                return;
            }

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
