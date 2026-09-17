using System.Collections.Generic;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// Rhythm Button Challenge 본게임 종료 시 HP 1 감소 대상 판정.
    /// · 1인: 깎지 않음.
    /// · 2인 이상: 하위 50%만 (2→1, 3→1, 4→2). 저점수 컷 없음.
    /// </summary>
    public static class RhythmButtonChallengeHpLossRules
    {
        public static void FillHpLost(
            int[] finalScore,
            bool[] participated,
            bool[] hpLostOut)
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
