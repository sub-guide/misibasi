using System;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    [Serializable]
    public struct FlounderDonationTier
    {
        public int amountWon;
        public int bonusScore;
        [Range(0f, 100f)] public float weightPercent;
    }

    public readonly struct FlounderDonationRoll
    {
        public int AmountWon { get; }
        public int BonusScore { get; }
        public string DonorName { get; }

        public FlounderDonationRoll(int amountWon, int bonusScore, string donorName)
        {
            AmountWon = amountWon;
            BonusScore = bonusScore;
            DonorName = donorName;
        }
    }

    /// <summary>가상 후원 금액·닉네임 결정 (DonationManager).</summary>
    public sealed class FlounderDonationManager : MonoBehaviour
    {
        [SerializeField] int startDonationAmount = 50000;
        [SerializeField] FlounderDonationTier[] gameplayTiers =
        {
            new() { amountWon = 1000, bonusScore = 100, weightPercent = 35f },
            new() { amountWon = 3000, bonusScore = 300, weightPercent = 25f },
            new() { amountWon = 5000, bonusScore = 500, weightPercent = 20f },
            new() { amountWon = 10000, bonusScore = 1000, weightPercent = 12f },
            new() { amountWon = 30000, bonusScore = 3000, weightPercent = 6f },
            new() { amountWon = 50000, bonusScore = 5000, weightPercent = 2f }
        };

        [SerializeField] string[] donorNicknames =
        {
            "광어사랑123", "광어찐팬", "파닥파닥장인", "물만난고등어", "별풍선요정",
            "큰손형님", "광어회장님", "지나가던시청자", "춤추는오징어", "오늘도출석", "엑셀방송VIP"
        };

        public int StartDonationAmount => startDonationAmount;

        public FlounderDonationRoll RollGameplayDonation()
        {
            FlounderDonationTier tier = PickTier();
            return new FlounderDonationRoll(tier.amountWon, tier.bonusScore, PickDonorName());
        }

        public string PickDonorName()
        {
            if (donorNicknames == null || donorNicknames.Length == 0)
                return "익명";

            return donorNicknames[UnityEngine.Random.Range(0, donorNicknames.Length)];
        }

        FlounderDonationTier PickTier()
        {
            if (gameplayTiers == null || gameplayTiers.Length == 0)
                return default;

            float total = 0f;
            foreach (FlounderDonationTier tier in gameplayTiers)
                total += tier.weightPercent;

            float roll = UnityEngine.Random.Range(0f, total);
            float acc = 0f;
            foreach (FlounderDonationTier tier in gameplayTiers)
            {
                acc += tier.weightPercent;
                if (roll <= acc)
                    return tier;
            }

            return gameplayTiers[gameplayTiers.Length - 1];
        }
    }
}
