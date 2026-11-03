using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        public void EnqueueGameplayAlert(FlounderDonationEventData data)
        {
            _history.Add(data);
            _alertQueue.Enqueue(data);
        }

        public void TickGameplayAlerts(float deltaTime)
        {
            if (_gameplayPhase == GameplayPhase.Hidden)
            {
                if (_alertQueue.Count == 0)
                    return;

                FlounderDonationEventData next = _alertQueue.Dequeue();
                ApplyDonationTypography();
                ApplyGameplayAlertTexts(next);
                if (gameplayAlertRoot != null)
                    gameplayAlertRoot.SetActive(true);

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

                        RestoreGameplayPanelHome();
                        _gameplayPhase = GameplayPhase.Hidden;
                        _gameplayTimer = 0f;
                    }

                    break;
            }
        }

        void ApplyGameplayAlertTexts(FlounderDonationEventData data)
        {
            int slot = Mathf.Clamp(data.TargetSlotIndex, 0, 3);
            string target = (slot + 1) + "P";
            Sprite tierSprite = ResolveStarBalloonSprite(data.AmountWon);
            bool spriteMode = useGameplayDonationSprite && gameplayBannerImage != null && tierSprite != null;

            if (spriteMode)
            {
                gameplayBannerImage.enabled = true;
                gameplayBannerImage.sprite = tierSprite;
                gameplayBannerImage.preserveAspect = true;
                gameplayBannerImage.color = Color.white;
            }

            HideLegacyGameplayChrome();

            if (spriteMode)
            {
                if (gameplayAmountText != null)
                    gameplayAmountText.gameObject.SetActive(false);

                if (gameplayMessageText != null)
                {
                    ApplyStarBalloonCaptionLabel(
                        gameplayMessageText,
                        ResolveDonationFont(),
                        $"{data.DonorName}님이 <color=#FFE066><b>{target}</b></color>에게 후원!");
                    LayoutGameplayDonationMessageSingleLine(gameplayMessageText);
                }

                if (gameplayGameTitleText != null)
                    gameplayGameTitleText.gameObject.SetActive(false);

                PositionGameplayAlertOverSlot(slot);

                if (gameplayAlertRoot != null)
                    gameplayAlertRoot.transform.SetAsLastSibling();

                return;
            }

            if (gameplayAlertRoot != null)
                gameplayAlertRoot.SetActive(false);
        }

        void RemoveLegacyGameplayOverlays()
        {
            if (gameplayAlertRoot == null)
                return;

            Transform panel = gameplayAlertRoot.transform;
            foreach (string legacyName in FlounderDonationUiConstants.LegacyGameplayChildNames)
            {
                Transform legacy = panel.Find(legacyName);
                if (legacy != null)
                    Destroy(legacy.gameObject);
            }

            var outline = gameplayAlertRoot.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }

        void HideLegacyGameplayChrome()
        {
            if (gameplayAlertRoot == null)
                return;

            Transform panel = gameplayAlertRoot.transform;
            foreach (string legacyName in FlounderDonationUiConstants.LegacyGameplayChildNames)
            {
                Transform legacy = panel.Find(legacyName);
                if (legacy != null)
                    legacy.gameObject.SetActive(false);
            }

            if (gameplayHeaderText != null)
                gameplayHeaderText.gameObject.SetActive(false);
        }

        void RestoreGameplayPanelHome()
        {
            if (gameplayAlertRoot == null)
                return;

            var alertRect = gameplayAlertRoot.transform as RectTransform;
            if (alertRect == null)
                return;

            Transform home = _gameplayPanelHomeParent != null ? _gameplayPanelHomeParent : transform;
            if (alertRect.parent == home)
                return;

            alertRect.SetParent(home, false);
            alertRect.anchorMin = new Vector2(0.5f, 0.5f);
            alertRect.anchorMax = new Vector2(0.5f, 0.5f);
            alertRect.pivot = new Vector2(0.5f, 0.5f);
            alertRect.anchoredPosition = Vector2.zero;
            alertRect.localScale = Vector3.one;
        }
    }
}
