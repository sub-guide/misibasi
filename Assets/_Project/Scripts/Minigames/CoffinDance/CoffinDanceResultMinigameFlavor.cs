using MiniParty.Minigames;
using MiniParty.Result;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>관짝춤 전용 Result Intro 훅. 현재는 ID 매칭만.</summary>
    public sealed class CoffinDanceResultMinigameFlavor : IResultMinigameFlavor
    {
        public bool TryApplyIntro(
            string minigameId,
            ResultSlotView[] slotViews,
            bool[] playedMask,
            MinigameSessionReport report,
            bool practice)
        {
            if (!string.Equals(minigameId, CoffinDanceMinigameModule.BuiltInId, System.StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}
