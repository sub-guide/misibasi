using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        void EnsureStartDonationSprite()
        {
            if (!useStartDonationSprite || centerPopupImage == null || centerPopupImage.sprite != null)
                return;

            Sprite sprite = LoadEditorSprite(FlounderDonationUiConstants.StartDonationSpritePath);
            if (sprite == null)
                sprite = LoadEditorSprite(FlounderDonationUiConstants.StartDonationSpriteFallbackPath);

            if (sprite != null)
                centerPopupImage.sprite = sprite;
        }

        static Sprite LoadEditorSprite(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            return null;
#endif
        }

        void EnsureGameplayStarBalloonSprites()
        {
            int tierCount = FlounderDonationUiConstants.TierAmountWon.Length;
            if (gameplayStarBalloonSprites == null || gameplayStarBalloonSprites.Length != tierCount)
                gameplayStarBalloonSprites = new Sprite[tierCount];

            for (var i = 0; i < tierCount; i++)
            {
                if (gameplayStarBalloonSprites[i] != null)
                    continue;

                gameplayStarBalloonSprites[i] =
                    LoadTierStarBalloonSprite(FlounderDonationUiConstants.TierAmountWon[i]);
            }
        }

        static Sprite LoadTierStarBalloonSprite(int amountWon)
        {
            string path = amountWon switch
            {
                1000 => "Assets/Minigames/Flounder/Sprites/StarBalloon_1000.png",
                3000 => "Assets/Minigames/Flounder/Sprites/StarBalloon_3000.png",
                5000 => "Assets/Minigames/Flounder/Sprites/StarBalloon_5000.png",
                10000 => "Assets/Minigames/Flounder/Sprites/StarBalloon_10000.png",
                30000 => "Assets/Minigames/Flounder/Sprites/StarBalloon_30000.png",
                50000 => "Assets/Minigames/Flounder/Sprites/StarBalloon_50000.png",
                _ => null
            };

            if (string.IsNullOrEmpty(path))
                return null;

            return LoadEditorSprite(path);
        }

        Sprite ResolveStarBalloonSprite(int amountWon)
        {
            EnsureGameplayStarBalloonSprites();

            int[] tiers = FlounderDonationUiConstants.TierAmountWon;
            if (gameplayStarBalloonSprites != null)
            {
                for (var i = 0; i < tiers.Length && i < gameplayStarBalloonSprites.Length; i++)
                {
                    if (tiers[i] == amountWon && gameplayStarBalloonSprites[i] != null)
                        return gameplayStarBalloonSprites[i];
                }
            }

            Sprite fallback = LoadTierStarBalloonSprite(amountWon);
            if (fallback != null)
                return fallback;

            return gameplayBannerImage != null ? gameplayBannerImage.sprite : null;
        }
    }
}
