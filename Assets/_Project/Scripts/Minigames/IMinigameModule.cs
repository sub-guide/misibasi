using System;
using MiniParty.Core;

namespace MiniParty.Minigames
{
    public readonly struct MinigameContext
    {
        public PlayerSlotModel[] Slots { get; }
        public bool IsPractice { get; }
        public Action<MinigameSessionReport> OnComplete { get; }

        public MinigameContext(PlayerSlotModel[] slots, bool isPractice, Action<MinigameSessionReport> onComplete)
        {
            Slots = slots;
            IsPractice = isPractice;
            OnComplete = onComplete;
        }
    }

    public sealed class MinigameSessionReport
    {
        public string MinigameId;

        /// <summary>해당 세션 종료 시 HP 1 감소 대상인지 (Oiia: 저점수·하위 50% 규칙, 본게임만).</summary>
        public bool[] HpLostThisSession;

        public int[] FinalScore;

        /// <summary>1부터 시작. 동점이면 같은 값. Result 씬에서 채울 수 있다.</summary>
        public int[] Rank;

        public MinigameSessionReport(int slotCount)
        {
            HpLostThisSession = new bool[slotCount];
            FinalScore = new int[slotCount];
            Rank = new int[slotCount];
        }
    }

    public interface IMinigameModule
    {
        string Id { get; }
        string DisplayName { get; }

        void Begin(MinigameContext context);
        void Tick();
        void RequestEarlyExit();
    }
}
