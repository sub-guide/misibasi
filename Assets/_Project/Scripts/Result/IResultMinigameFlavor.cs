using MiniParty.Minigames;

namespace MiniParty.Result
{
    /// <summary>미니게임별 Result Intro 장식(선택). <see cref="ResultFlowController"/> 가 호출한다.</summary>
    public interface IResultMinigameFlavor
    {
        bool TryApplyIntro(
            string minigameId,
            ResultSlotView[] slotViews,
            bool[] playedMask,
            MinigameSessionReport report,
            bool practice);
    }
}
