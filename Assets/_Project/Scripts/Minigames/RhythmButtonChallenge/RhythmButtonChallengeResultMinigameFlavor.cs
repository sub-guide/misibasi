using MiniParty.Minigames;
using MiniParty.Result;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>Rhythm Button Challenge 전용 Result Intro 훅. 현재는 ID 매칭만.</summary>
    public sealed class RhythmButtonChallengeResultMinigameFlavor : IResultMinigameFlavor
    {
        public bool TryApplyIntro(
            string minigameId,
            ResultSlotView[] slotViews,
            bool[] playedMask,
            MinigameSessionReport report,
            bool practice)
        {
            if (!string.Equals(minigameId, RhythmButtonChallengeMinigameModule.BuiltInId, System.StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}
