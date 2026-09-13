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
    }
}
