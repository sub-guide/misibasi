namespace MiniParty.Minigames.Flounder
{
    /// <summary>게임 중 가상 후원 1건 (슬롯 위 별풍선 알림).</summary>
    public readonly struct FlounderDonationEventData
    {
        public string DonorName { get; }
        public int TargetSlotIndex { get; }
        public int AmountWon { get; }

        public FlounderDonationEventData(string donorName, int targetSlotIndex, int amountWon)
        {
            DonorName = donorName;
            TargetSlotIndex = targetSlotIndex;
            AmountWon = amountWon;
        }
    }
}
