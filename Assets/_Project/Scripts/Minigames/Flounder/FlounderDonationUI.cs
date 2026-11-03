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
        [SerializeField] bool useStartDonationSprite = true;
        [SerializeField] TextMeshProUGUI centerSpecialHeaderText;
        [SerializeField] TextMeshProUGUI centerTitleText;
        [SerializeField] TextMeshProUGUI centerAmountText;
        [SerializeField] TextMeshProUGUI centerGameTitleText;
        [SerializeField] TMP_FontAsset centerTaglineFont;
        [SerializeField] float centerPopupSeconds = 7f;
        [SerializeField] float fadeSeconds = 0.3f;

        [Header("게임 중 후원 (캐릭터 슬롯 위)")]
        [SerializeField] GameObject gameplayAlertRoot;
        [SerializeField] CanvasGroup gameplayCanvasGroup;
        [SerializeField] Image gameplayBannerImage;
        [SerializeField] bool useGameplayDonationSprite = true;
        [SerializeField] FlounderPlayerSlotView[] gameplaySlotAnchors;
        [SerializeField] Sprite[] gameplayStarBalloonSprites;
        [SerializeField] [Range(0.15f, 1f)] float slotStarBalloonScale = 0.48f;
        [SerializeField] [Range(0.12f, 0.8f)] float slotStarBalloonScaleMin = 0.26f;
        [SerializeField] RectTransform gameplayHudLiveUi;
        [SerializeField] RectTransform gameplayHudCountdown;
        [SerializeField] float gameplayHudClearance = 12f;
        [Tooltip("게이지 후원 알림(별풍선+텍스트) 하단이 슬롯 라벨 상단에서 이만큼 위에 오도록 합니다.")]
        [SerializeField] float gameplayAlertAboveLabelGap = 10f;
        [SerializeField] TextMeshProUGUI gameplayHeaderText;
        [SerializeField] TextMeshProUGUI gameplayMessageText;
        [SerializeField] TextMeshProUGUI gameplayAmountText;
        [SerializeField] TextMeshProUGUI gameplayGameTitleText;
        [SerializeField] float gameplayAlertSeconds = 2.5f;
        [SerializeField] TMP_FontAsset donationFont;
        [SerializeField] FlounderPlayerSlotView typographyReferenceSlot;
        [SerializeField] [Range(0.35f, 1.35f)] float centerStarBalloonScale = 1.1f;
        [Header("씬 레이아웃")]
        [Tooltip("켜면 전체(시작) 후원창만 씬 배치를 유지합니다. 게이지 후원 알림은 슬롯 위 자동 배치입니다.")]
        [SerializeField] bool preserveDonationUiSceneLayout = true;
        [SerializeField] float gameplayAmountFontRatio = 0.52f;
        [SerializeField] float starBalloonMessageFontSize = 45f;
        [SerializeField] float starBalloonTaglineFontSize = 80f;
        [SerializeField] float centerStartTitleFontSize = 50f;
        [SerializeField] string centerTaglineText = "물 만난 광어 드가자";

        Transform _gameplayPanelHomeParent;

        readonly List<FlounderDonationEventData> _history = new List<FlounderDonationEventData>();
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

        public float CenterPopupSeconds => centerPopupSeconds;
        public IReadOnlyList<FlounderDonationEventData> History => _history;

        void Awake()
        {
            if (gameplayAlertRoot != null)
                _gameplayPanelHomeParent = gameplayAlertRoot.transform.parent;

            CaptureDonationUiSceneLayout();

            EnsureRootCoversCanvas();
            EnsureDonationUiDrawsOnTop();
            EnsureGameplaySlotAnchors();
            EnsureGameplayHudRects();
            EnsureCenterTaglineFont();
            EnsureGameplayStarBalloonSprites();
            EnsureStartDonationSprite();
            LayoutStarBalloonTextAnchors();
            RemoveLegacyGameplayOverlays();
            ApplyCrispSpriteLayout();
            ApplyDonationTypography();

            if (Application.isPlaying)
                HideCenterPopup();
        }

        public void ClearFeed()
        {
            _history.Clear();
            _alertQueue.Clear();
            _centerPhase = CenterPhase.Idle;
            _gameplayPhase = GameplayPhase.Hidden;
            HideCenterPopup();

            if (gameplayAlertRoot != null)
                gameplayAlertRoot.SetActive(false);

            RestoreGameplayPanelHome();
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
