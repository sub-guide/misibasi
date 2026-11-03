using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        public void ShowStartDonationPopup(int amountPerPlayer, string gameTitle)
        {
            EnsureRootCoversCanvas();
            EnsureDonationUiDrawsOnTop();

            if (centerPopupRoot != null)
                centerPopupRoot.SetActive(true);

            if (centerCanvasGroup != null)
                SetCanvasAlpha(centerCanvasGroup, 0f);

            ApplyDonationTypography();
            ApplyStartDonationVisuals(amountPerPlayer, gameTitle);
            RestoreCenterPanelSceneLayout();

            FlounderGameManager manager = FindObjectOfType<FlounderGameManager>();
            manager?.PlayStartDonationBgm();

            _centerPhase = CenterPhase.FadeIn;
            _centerTimer = 0f;

            if (gameplayAlertRoot != null)
                gameplayAlertRoot.SetActive(false);

            _gameplayPhase = GameplayPhase.Hidden;
            _alertQueue.Clear();
        }

        /// <returns>중앙 후원 연출이 끝나 카운트다운을 시작해도 되면 true.</returns>
        public bool TickStartSequence(float deltaTime)
        {
            if (_centerPhase == CenterPhase.Done)
                return true;

            if (_centerPhase == CenterPhase.Idle)
                return false;

            _centerTimer += deltaTime;
            float holdSeconds = Mathf.Max(0f, centerPopupSeconds - fadeSeconds * 2f);
            bool useFade = centerCanvasGroup != null;

            switch (_centerPhase)
            {
                case CenterPhase.FadeIn:
                    if (useFade)
                        SetCanvasAlpha(centerCanvasGroup, Mathf.Clamp01(_centerTimer / fadeSeconds));
                    if (!useFade || _centerTimer >= fadeSeconds)
                    {
                        _centerPhase = CenterPhase.Hold;
                        _centerTimer = 0f;
                        SetCanvasAlpha(centerCanvasGroup, 1f);
                    }

                    break;
                case CenterPhase.Hold:
                    if (_centerTimer >= holdSeconds)
                    {
                        _centerPhase = CenterPhase.FadeOut;
                        _centerTimer = 0f;
                    }

                    break;
                case CenterPhase.FadeOut:
                    if (useFade)
                        SetCanvasAlpha(centerCanvasGroup, 1f - Mathf.Clamp01(_centerTimer / fadeSeconds));
                    if (!useFade || _centerTimer >= fadeSeconds)
                    {
                        HideCenterPopup();
                        _centerPhase = CenterPhase.Done;
                    }

                    break;
            }

            return _centerPhase == CenterPhase.Done;
        }

        public void HideCenterPopup()
        {
            if (centerPopupRoot != null)
                centerPopupRoot.SetActive(false);

            SetCanvasAlpha(centerCanvasGroup, 0f);
        }

        void ApplyStartDonationVisuals(int amountPerPlayer, string gameTitle)
        {
            EnsureStartDonationSprite();
            bool spriteMode = useStartDonationSprite && centerPopupImage != null && centerPopupImage.sprite != null;

            if (spriteMode)
            {
                centerPopupImage.enabled = true;
                ApplyCenterPanelLayout();
            }

            if (centerSpecialHeaderText != null)
                centerSpecialHeaderText.gameObject.SetActive(false);

            if (spriteMode)
            {
                if (centerAmountText != null)
                    centerAmountText.gameObject.SetActive(false);

                ApplyStartDonationTitle();
                ApplyCenterTaglineLabel(
                    string.IsNullOrWhiteSpace(centerTaglineText) ? "물 만난 광어 드가자" : centerTaglineText);

                return;
            }

            ApplyStartDonationTitle();

            if (centerAmountText != null)
            {
                centerAmountText.gameObject.SetActive(true);
                centerAmountText.text = amountPerPlayer.ToString("N0") + "원씩";
                centerAmountText.color = FlounderDonationUiConstants.CenterAmountGold;
            }

            ApplyCenterTaglineLabel(string.IsNullOrWhiteSpace(centerTaglineText) ? gameTitle : centerTaglineText);
        }

        void ApplyStartDonationTitle()
        {
            if (centerTitleText == null)
                return;

            EnsureDonationFontOn(centerTitleText);
            centerTitleText.gameObject.SetActive(true);
            centerTitleText.text = "모든 플레이어에게 후원!";
            centerTitleText.fontSize = centerStartTitleFontSize;
        }

        void ApplyCenterPanelLayout()
        {
            if (centerPopupImage == null)
                return;

            centerPopupImage.preserveAspect = true;
            centerPopupImage.color = Color.white;

            if (PreserveCenterDonationLayout)
                return;

            FitCenterPanelNativeSize();
        }

        void FitCenterPanelNativeSize()
        {
            if (centerPopupRoot == null || centerPopupImage == null || centerPopupImage.sprite == null)
                return;

            var rt = centerPopupRoot.transform as RectTransform;
            if (rt == null)
                return;

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 36f);
            centerPopupImage.SetNativeSize();

            float scale = Mathf.Clamp(centerStarBalloonScale, 0.35f, 1.35f);
            rt.sizeDelta *= scale;
        }
    }
}
