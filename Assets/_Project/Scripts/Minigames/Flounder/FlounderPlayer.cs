using System;

namespace MiniParty.Minigames.Flounder
{
    [Serializable]
    public sealed class FlounderPlayer
    {
        public int SlotIndex;
        public bool Active;
        public int MashCount;
        public int MashScore;
        public int DonationBonusScore;
        public int DonationCount;
        public int TotalDonationWon;
        public bool ReceivedGameplayDonation;

        public int FinalScore => MashScore + DonationBonusScore;

        public void ResetForRound()
        {
            MashCount = 0;
            MashScore = 0;
            DonationBonusScore = 0;
            DonationCount = 0;
            TotalDonationWon = 0;
            ReceivedGameplayDonation = false;
        }
    }
}
