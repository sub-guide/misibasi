using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Flounder
{
    /// <summary>
    /// 후원 알림 UI (중앙 시작 / 슬롯 위 게임 중). 페이드·큐 처리.
    /// </summary>
    public sealed partial class FlounderDonationUI : MonoBehaviour
    {
        [Header("중앙 시작 후원")]
        [SerializeField] GameObject centerPopupRoot;
        [SerializeField] CanvasGroup centerCanvasGroup;
        [SerializeField] Image centerPopupImage;
        [SerializeField] TextMeshProUGUI centerTitleText;
        [SerializeField] TextMeshProUGUI centerGameTitleText;
        [SerializeField] TMP_FontAsset centerTaglineFont;
        [SerializeField] float centerPopupSeconds = 7f;
        [SerializeField] float fadeSeconds = 0.3f;

        [Header("게임 중 후원 (캐릭터 슬롯 위)")]
        [SerializeField] GameObject gameplayAlertRoot;
        [SerializeField] CanvasGroup gameplayCanvasGroup;
        [SerializeField] Image gameplayBannerImage;
        [SerializeField] FlounderPlayerSlotView[] gameplaySlotAnchors;
        [Tooltip("FlounderDonationUiConstants.TierAmountWon 과 같은 순서 (1000 · 3000 · 5000 · 10000 · 30000 · 50000)")]
        [SerializeField] Sprite[] gameplayStarBalloonSprites;
        [SerializeField] [Range(0.15f, 1f)] float slotStarBalloonScale = 0.48f;
        [SerializeField] [Range(0.12f, 0.8f)] float slotStarBalloonScaleMin = 0.26f;
        [SerializeField] RectTransform gameplayHudLiveUi;
        [SerializeField] RectTransform gameplayHudCountdown;
        [SerializeField] float gameplayHudClearance = 12f;
        [Tooltip("게이지 후원 알림(별풍선+텍스트) 하단이 슬롯 라벨 상단에서 이만큼 위에 오도록 합니다.")]
        [SerializeField] float gameplayAlertAboveLabelGap = 10f;
        [SerializeField] TextMeshProUGUI gameplayMessageText;
        [SerializeField] float gameplayAlertSeconds = 2.5f;

        [Header("글꼴")]
        [SerializeField] TMP_FontAsset donationFont;
        [SerializeField] float starBalloonMessageFontSize = 45f;
        [SerializeField] float starBalloonTaglineFontSize = 80f;
        [SerializeField] float centerStartTitleFontSize = 50f;
        [SerializeField] string centerTaglineText = "물 만난 광어 드가자";

        readonly Queue<FlounderDonationEventData> _alertQueue = new Queue<FlounderDonationEventData>();

        enum CenterPhase
        {
            Idle,
            FadeIn,
            Hold,
            FadeOut,
            Done
        }

        enum GameplayPhase
        {
            Hidden,
            FadeIn,
            Hold,
            FadeOut
        }

        CenterPhase _centerPhase = CenterPhase.Idle;
        GameplayPhase _gameplayPhase = GameplayPhase.Hidden;
        float _centerTimer;
        float _gameplayTimer;

        void Awake()
        {
            ApplyDonationTypography();
            HideCenterPopup();
        }

        public void ClearFeed()
        {
            _alertQueue.Clear();
            _centerPhase = CenterPhase.Idle;
            _gameplayPhase = GameplayPhase.Hidden;
            HideCenterPopup();

            if (gameplayAlertRoot != null)
                gameplayAlertRoot.SetActive(false);
        }

        static void SetCanvasAlpha(CanvasGroup group, float alpha)
        {
            if (group == null)
                return;

            group.alpha = alpha;
        }

        static void SetRectStretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (rt == null)
                return;

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
