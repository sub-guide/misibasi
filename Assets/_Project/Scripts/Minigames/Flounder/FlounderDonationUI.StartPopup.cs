using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        public void ShowStartDonationPopup()
        {
            if (centerPopupRoot != null)
                centerPopupRoot.SetActive(true);

            SetCanvasAlpha(centerCanvasGroup, 0f);

            ApplyDonationTypography();
            ApplyStartDonationVisuals();

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

        void ApplyStartDonationVisuals()
        {
            if (centerPopupImage != null)
            {
                centerPopupImage.enabled = true;
                centerPopupImage.preserveAspect = true;
                centerPopupImage.color = Color.white;
            }

            if (centerTitleText != null)
            {
                centerTitleText.gameObject.SetActive(true);
                centerTitleText.text = "모든 플레이어에게 후원!";
            }

            ApplyStarBalloonCaptionLabel(centerGameTitleText, centerTaglineFont, centerTaglineText, starBalloonTaglineFontSize);
        }
    }
}
