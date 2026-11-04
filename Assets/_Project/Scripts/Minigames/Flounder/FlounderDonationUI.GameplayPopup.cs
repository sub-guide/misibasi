using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        public void EnqueueGameplayAlert(FlounderDonationEventData data) => _alertQueue.Enqueue(data);

        public void TickGameplayAlerts(float deltaTime)
        {
            if (_gameplayPhase == GameplayPhase.Hidden)
            {
                if (_alertQueue.Count == 0)
                    return;

                FlounderDonationEventData next = _alertQueue.Dequeue();
                if (!ShowGameplayAlert(next))
                    return;

                SetCanvasAlpha(gameplayCanvasGroup, 0f);
                _gameplayPhase = GameplayPhase.FadeIn;
                _gameplayTimer = 0f;
            }

            _gameplayTimer += deltaTime;
            float holdSeconds = Mathf.Max(0f, gameplayAlertSeconds - fadeSeconds * 2f);

            switch (_gameplayPhase)
            {
                case GameplayPhase.FadeIn:
                    SetCanvasAlpha(gameplayCanvasGroup, Mathf.Clamp01(_gameplayTimer / fadeSeconds));
                    if (_gameplayTimer >= fadeSeconds)
                    {
                        _gameplayPhase = GameplayPhase.Hold;
                        _gameplayTimer = 0f;
                        SetCanvasAlpha(gameplayCanvasGroup, 1f);
                    }

                    break;
                case GameplayPhase.Hold:
                    if (_gameplayTimer >= holdSeconds)
                    {
                        _gameplayPhase = GameplayPhase.FadeOut;
                        _gameplayTimer = 0f;
                    }

                    break;
                case GameplayPhase.FadeOut:
                    SetCanvasAlpha(gameplayCanvasGroup, 1f - Mathf.Clamp01(_gameplayTimer / fadeSeconds));
                    if (_gameplayTimer >= fadeSeconds)
                    {
                        if (gameplayAlertRoot != null)
                            gameplayAlertRoot.SetActive(false);

                        _gameplayPhase = GameplayPhase.Hidden;
                        _gameplayTimer = 0f;
                    }

                    break;
            }
        }

        /// <returns>별풍선 스프라이트가 있어 알림을 띄웠으면 true.</returns>
        bool ShowGameplayAlert(FlounderDonationEventData data)
        {
            Sprite tierSprite = ResolveStarBalloonSprite(data.AmountWon);
            if (gameplayAlertRoot == null || gameplayBannerImage == null || tierSprite == null)
                return false;

            int slot = Mathf.Clamp(data.TargetSlotIndex, 0, 3);
            string target = (slot + 1) + "P";

            ApplyDonationTypography();

            gameplayBannerImage.enabled = true;
            gameplayBannerImage.sprite = tierSprite;
            gameplayBannerImage.preserveAspect = true;
            gameplayBannerImage.color = Color.white;

            gameplayAlertRoot.SetActive(true);

            if (gameplayMessageText != null)
            {
                ApplyStarBalloonCaptionLabel(
                    gameplayMessageText,
                    donationFont,
                    $"{data.DonorName}님이 <color=#FFE066><b>{target}</b></color>에게 후원!",
                    starBalloonMessageFontSize);
                LayoutGameplayDonationMessageSingleLine(gameplayMessageText);
            }

            PositionGameplayAlertOverSlot(slot);
            gameplayAlertRoot.transform.SetAsLastSibling();
            return true;
        }

        Sprite ResolveStarBalloonSprite(int amountWon)
        {
            int[] tiers = FlounderDonationUiConstants.TierAmountWon;
            if (gameplayStarBalloonSprites == null)
                return null;

            for (var i = 0; i < tiers.Length && i < gameplayStarBalloonSprites.Length; i++)
            {
                if (tiers[i] == amountWon)
                    return gameplayStarBalloonSprites[i];
            }

            return null;
        }
    }
}
