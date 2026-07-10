using MiniParty.Result;
using MiniParty.Minigames;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>Oiia 전용 Result Intro 훅. 현재는 ID 매칭만, 추후 슬롯 장식 확장.</summary>
    public sealed class OiiaResultMinigameFlavor : IResultMinigameFlavor
    {
        public bool TryApplyIntro(
            string minigameId,
            ResultSlotView[] slotViews,
            bool[] playedMask,
            MinigameSessionReport report,
            bool practice)
        {
            if (!string.Equals(minigameId, OiiaMinigameModule.BuiltInId, System.StringComparison.OrdinalIgnoreCase))
                return false;

            // 추후: Oiia 테마 색·아이콘 등
            return true;
        }
    }
}
