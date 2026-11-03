using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    static class FlounderDonationUiConstants
    {
        public const string CenterTaglineFontPath = "Assets/Fonts/NanumBarunpenB SDF.asset";
        public const string StartDonationSpritePath = "Assets/Minigames/Flounder/Sprites/StartDonation_100000.png";
        public const string StartDonationSpriteFallbackPath = "Assets/Minigames/Flounder/Sprites/StartDonationPopup.png";
        public const string StartDonationIntroBgmPath = "Assets/Audio/광어 (운동 리액션) BGM_[cut_3sec].mp3";
        public const string StartDonationTtsPath = "Assets/Audio/광어_tts.mp3";
        public const string CountdownBgmPath = "Assets/Audio/광어 (운동 리액션) BGM_[cut_30sec].mp3";

        public static readonly int[] TierAmountWon = { 1000, 3000, 5000, 10000, 30000, 50000 };

        public static readonly Color GameplayAmountGold = new Color(1f, 0.88f, 0.4f, 1f);
        public static readonly Color CenterAmountGold = new Color(1f, 0.88f, 0.4f, 1f);
        public static readonly Color StarBalloonAmountRed = new Color(0.92f, 0.15f, 0.12f, 1f);

        public static readonly string[] LegacyGameplayChildNames =
        {
            "Feed",
            "Header",
            "Footer",
            "TextBackdrop",
            "SlotBadge"
        };
    }
}
