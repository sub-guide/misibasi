using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        void EnsureGameplayHudRects()
        {
            if (gameplayHudCountdown == null)
            {
                Transform countdown = transform.Find("Countdown");
                if (countdown != null)
                    gameplayHudCountdown = countdown as RectTransform;
            }

            if (gameplayHudLiveUi != null)
                return;

            Transform canvasRoot = transform.parent;
            if (canvasRoot == null)
                return;

            Transform live = canvasRoot.Find("LiveUI");
            if (live != null)
                gameplayHudLiveUi = live as RectTransform;
        }

        void PositionGameplayAlertOverSlot(int slotIndex)
        {
            if (gameplayAlertRoot == null)
                return;

            FlounderPlayerSlotView slot = FindGameplaySlot(slotIndex);
            RectTransform labelRect = slot != null ? slot.SlotLabelRect : null;
            var alertRect = gameplayAlertRoot.transform as RectTransform;
            var rootRect = transform as RectTransform;
            if (labelRect == null || alertRect == null || rootRect == null)
                return;

            EnsureGameplayHudRects();
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            Transform home = _gameplayPanelHomeParent != null ? _gameplayPanelHomeParent : transform;
            if (alertRect.parent != home)
                alertRect.SetParent(home, false);

            alertRect.anchorMin = alertRect.anchorMax = new Vector2(0.5f, 0.5f);
            alertRect.pivot = new Vector2(0.5f, 0f);
            alertRect.localRotation = Quaternion.identity;
            alertRect.localScale = Vector3.one;

            float scale = slotStarBalloonScale;
            float scaleMin = Mathf.Min(slotStarBalloonScaleMin, slotStarBalloonScale);
            float ceilingWorldY = GetActiveHudBottomWorldY() - gameplayHudClearance;
            GetRectTopCenterWorld(labelRect, out float labelCenterX, out float labelTopY);

            for (var attempt = 0; attempt < 10; attempt++)
            {
                ApplySlotBalloonScale(scale);
                if (gameplayMessageText != null && gameplayMessageText.gameObject.activeSelf)
                    gameplayMessageText.ForceMeshUpdate();

                float popupHeight = MeasurePopupWorldHeight(alertRect);
                float bottomWorldY = labelTopY + gameplayAlertAboveLabelGap;
                float topWorldY = bottomWorldY + popupHeight;

                if (float.IsFinite(ceilingWorldY) && topWorldY > ceilingWorldY)
                {
                    float available = ceilingWorldY - bottomWorldY;
                    if (available <= 4f)
                    {
                        scale = scaleMin;
                        continue;
                    }

                    float shrink = available / Mathf.Max(popupHeight, 1f);
                    scale = Mathf.Clamp(scale * shrink, scaleMin, slotStarBalloonScale);
                    continue;
                }

                break;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                new Vector3(labelCenterX, labelTopY + gameplayAlertAboveLabelGap, 0f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screenPoint, uiCamera, out Vector2 local);
            alertRect.anchoredPosition = local;
            AlignGameplayAlertAboveSlotLabel(labelRect, alertRect);
        }

        void AlignGameplayAlertAboveSlotLabel(RectTransform labelRect, RectTransform alertRect)
        {
            GetRectTopCenterWorld(labelRect, out float labelCenterX, out float labelTopY);
            GetAlertWorldBounds(alertRect, out float alertMinY, out _, out float alertCenterX);
            float deltaY = labelTopY + gameplayAlertAboveLabelGap - alertMinY;
            float deltaX = labelCenterX - alertCenterX;
            alertRect.position += new Vector3(deltaX, deltaY, 0f);
        }

        static void GetAlertWorldBounds(RectTransform alertRect, out float minY, out float maxY, out float centerX)
        {
            minY = float.PositiveInfinity;
            maxY = float.NegativeInfinity;
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;

            foreach (Transform child in alertRect.GetComponentsInChildren<Transform>(true))
            {
                if (child is not RectTransform childRect)
                    continue;

                var corners = new Vector3[4];
                childRect.GetWorldCorners(corners);
                minY = Mathf.Min(minY, corners[0].y);
                maxY = Mathf.Max(maxY, corners[1].y);
                minX = Mathf.Min(minX, corners[0].x);
                maxX = Mathf.Max(maxX, corners[2].x);
            }

            centerX = minX <= maxX ? (minX + maxX) * 0.5f : alertRect.position.x;
            if (float.IsPositiveInfinity(minY))
                minY = alertRect.position.y;
            if (float.IsNegativeInfinity(maxY))
                maxY = alertRect.position.y;
        }

        static void GetRectTopCenterWorld(RectTransform rt, out float centerX, out float topY)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            centerX = (corners[0].x + corners[2].x) * 0.5f;
            topY = corners[1].y;
        }

        float GetActiveHudBottomWorldY()
        {
            float bottom = float.PositiveInfinity;
            var any = false;

            if (gameplayHudLiveUi != null && gameplayHudLiveUi.gameObject.activeInHierarchy)
            {
                var corners = new Vector3[4];
                gameplayHudLiveUi.GetWorldCorners(corners);
                bottom = Mathf.Min(bottom, corners[0].y);
                any = true;
            }

            if (gameplayHudCountdown != null && gameplayHudCountdown.gameObject.activeInHierarchy)
            {
                var corners = new Vector3[4];
                gameplayHudCountdown.GetWorldCorners(corners);
                bottom = Mathf.Min(bottom, corners[0].y);
                any = true;
            }

            return any ? bottom : float.PositiveInfinity;
        }

        float MeasurePopupWorldHeight(RectTransform alertRect)
        {
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            foreach (Transform child in alertRect.GetComponentsInChildren<Transform>(true))
            {
                if (child is not RectTransform childRect)
                    continue;

                AccumulateWorldVerticalBounds(childRect, ref minY, ref maxY);
            }

            if (maxY <= minY)
                return alertRect.rect.height;

            return maxY - minY;
        }

        static void AccumulateWorldVerticalBounds(RectTransform rt, ref float minY, ref float maxY)
        {
            if (rt == null)
                return;

            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            minY = Mathf.Min(minY, corners[0].y);
            maxY = Mathf.Max(maxY, corners[1].y);
        }

        void ApplySlotBalloonScale(float scale)
        {
            if (gameplayAlertRoot == null || gameplayBannerImage == null || gameplayBannerImage.sprite == null)
                return;

            var rt = gameplayAlertRoot.transform as RectTransform;
            if (rt == null)
                return;

            gameplayBannerImage.preserveAspect = true;
            gameplayBannerImage.SetNativeSize();

            float clamped = Mathf.Clamp(scale, slotStarBalloonScaleMin, slotStarBalloonScale);
            rt.sizeDelta = gameplayBannerImage.rectTransform.sizeDelta * clamped;
        }
    }
}
