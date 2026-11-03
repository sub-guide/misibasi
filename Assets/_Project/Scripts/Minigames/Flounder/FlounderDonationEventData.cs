namespace MiniParty.Minigames.Flounder
{
    /// <summary>가상 후원 1건 (시작·게임 중 공통).</summary>
    public struct FlounderDonationEventData
    {
        public string DonorName;
        public int TargetSlotIndex;
        public int AmountWon;
        public int BonusScore;
        public bool IsStartDonation;
        public string GameTitle;

        public static FlounderDonationEventData StartForAll(string donor, int amountPerPlayer, string gameTitle)
        {
            return new FlounderDonationEventData
            {
                DonorName = donor,
                TargetSlotIndex = -1,
                AmountWon = amountPerPlayer,
                BonusScore = 0,
                IsStartDonation = true,
                GameTitle = gameTitle
            };
        }

        public static FlounderDonationEventData Gameplay(string donor, int slotIndex, FlounderDonationRoll roll, string gameTitle)
        {
            return new FlounderDonationEventData
            {
                DonorName = donor,
                TargetSlotIndex = slotIndex,
                AmountWon = roll.AmountWon,
                BonusScore = roll.BonusScore,
                IsStartDonation = false,
                GameTitle = gameTitle
            };
        }
    }
}
