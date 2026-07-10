using MiniParty.UI.ControllerButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// <see cref="OiiaSlotPanel"/> 프리팹 루트. 자식 이름 규칙으로 UI 참조를 채우고
    /// <see cref="OiiaMinigameModule"/> 에 전달한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OiiaSlotPanelBindings : MonoBehaviour
    {
        [Header("레거시 — 문자 패턴·게이지")]
        public TMP_Text SequenceText;
        public Slider GaugeSlider;
        public Image Blur;
        public TMP_Text ScoreText;
        public TMP_Text PracticeReadyText;
        public Animator CatAnimator;
        public Image SlotPanelBackgroundImage;
        public TMP_Text WaitingText;

        [Header("레거시 — 컨트롤러 가이드 (Y/X/A/B)")]
        public GameObject ControllerGuideRoot;
        public Image ControllerBodyImage;
        public Image GuideButtonY;
        public Image GuideButtonX;
        public Image GuideButtonA;
        public Image GuideButtonB;

        [Header("레거시 — 셔플 이펙트")]
        public Image ShuffleEffect;

        [Header("코믹스 BurstText 풀")]
        public TMP_Text[] BurstTextPool;
        public RectTransform BurstTextContainer;

        [Header("디제잉 박스 (Rave)")]
        public RectTransform DjBoxRoot;
        public SnesPlayerFaceButtons DjFaceButtons;
        public SnesPlayerDpadButtons DjDpadButtons;
        public SnesPlayerShoulderButtons DjShoulderButtons;
        public SnesControllerButtonVisual[] DjPadButtons;

        [Header("소형 가변 디스플레이")]
        public TMP_Text HudScoreText;
        public TMP_Text HudComboText;
        public TMP_Text HudFeverText;

        [Header("서브 패턴 가이드")]
        public TMP_Text SubPatternGuideText;

        [Header("전광판 스크린")]
        public RectTransform StageScreenRoot;
        public Graphic StageBackgroundChromaKey;
        public Graphic StageBackgroundSpace;
        public Graphic StageBackgroundClub;

        public OiiaMinigameModule.SlotUiBindings ToSlotUiBindings()
        {
            EnsureDjPadButtonsArray();

            return new()
            {
                SequenceText = SequenceText,
                GaugeSlider = GaugeSlider,
                Blur = Blur,
                ScoreText = ScoreText,
                PracticeReadyText = PracticeReadyText,
                CatAnimator = CatAnimator,
                SlotPanelBackgroundImage = SlotPanelBackgroundImage,
                WaitingText = WaitingText,
                ControllerGuideRoot = ControllerGuideRoot,
                ControllerBodyImage = ControllerBodyImage,
                GuideButtonY = GuideButtonY,
                GuideButtonX = GuideButtonX,
                GuideButtonA = GuideButtonA,
                GuideButtonB = GuideButtonB,
                ShuffleEffect = ShuffleEffect,
                BurstTextPool = BurstTextPool,
                BurstTextContainer = BurstTextContainer,
                DjBoxRoot = DjBoxRoot,
                DjFaceButtons = DjFaceButtons,
                DjDpadButtons = DjDpadButtons,
                DjShoulderButtons = DjShoulderButtons,
                DjPadButtons = DjPadButtons,
                HudScoreText = HudScoreText,
                HudComboText = HudComboText,
                HudFeverText = HudFeverText,
                SubPatternGuideText = SubPatternGuideText,
                StageScreenRoot = StageScreenRoot,
                StageBackgroundChromaKey = StageBackgroundChromaKey,
                StageBackgroundSpace = StageBackgroundSpace,
                StageBackgroundClub = StageBackgroundClub,
            };
        }

        void Awake()
        {
            AutoWireFromHierarchy();
            HideIdleBurstTextPoolInHierarchy();
        }

        void OnValidate() => AutoWireFromHierarchy();

        public void AutoWireFromHierarchy()
        {
            Transform root = transform;

            SequenceText = GetDirectChildComponent<TMP_Text>(root, "Sequence");
            GaugeSlider = GetDirectChildComponent<Slider>(root, "Gauge");
            ScoreText = GetDirectChildComponent<TMP_Text>(root, "Score");
            Blur = GetDirectChildComponent<Image>(root, "Blur");
            PracticeReadyText = GetDirectChildComponent<TMP_Text>(root, "Ready");
            CatAnimator = GetDirectChildComponent<Animator>(root, "Cat");
            WaitingText = GetDirectChildComponent<TMP_Text>(root, "Waiting");
            SlotPanelBackgroundImage = GetComponent<Image>();

            Transform guideRoot = root.Find("ControllerGuide");
            if (guideRoot != null)
            {
                ControllerGuideRoot = guideRoot.gameObject;
                ControllerBodyImage = GetDirectChildComponent<Image>(guideRoot, "Body");
                GuideButtonY = GetDirectChildComponent<Image>(guideRoot, "BtnY");
                GuideButtonX = GetDirectChildComponent<Image>(guideRoot, "BtnX");
                GuideButtonA = GetDirectChildComponent<Image>(guideRoot, "BtnA");
                GuideButtonB = GetDirectChildComponent<Image>(guideRoot, "BtnB");
            }

            Transform shuffleEffect = root.Find("ShuffleEffect");
            if (shuffleEffect == null)
                shuffleEffect = root.Find("ShuffleMapOverlay");

            if (shuffleEffect != null)
                ShuffleEffect = shuffleEffect.GetComponent<Image>();

            Transform burstContainer = root.Find("BurstTextContainer");
            BurstTextContainer = burstContainer != null ? burstContainer as RectTransform : null;

            if (BurstTextPool == null || BurstTextPool.Length == 0)
                CollectBurstTextPool(root);

            AutoWireDjBox(root);
            AutoWireStageScreen(root);
            EnsureDjPadButtonsArray();
        }

        void AutoWireDjBox(Transform root)
        {
            Transform djBox = root.Find("DjBox");
            if (djBox == null)
                djBox = root.Find("DjingBox");

            if (djBox == null)
                return;

            DjBoxRoot = djBox as RectTransform;

            if (DjFaceButtons == null)
                DjFaceButtons = djBox.GetComponentInChildren<SnesPlayerFaceButtons>(true);

            if (DjDpadButtons == null)
                DjDpadButtons = djBox.GetComponentInChildren<SnesPlayerDpadButtons>(true);

            if (DjShoulderButtons == null)
                DjShoulderButtons = djBox.GetComponentInChildren<SnesPlayerShoulderButtons>(true);

            Transform hud = djBox.Find("HudDisplay");
            if (hud == null)
                hud = djBox.Find("Hud");

            if (hud != null)
            {
                HudScoreText = FindTmp(hud, "Score", "HudScore");
                HudComboText = FindTmp(hud, "Combo", "HudCombo");
                HudFeverText = FindTmp(hud, "Fever", "HudFever");
            }
            else
            {
                if (HudScoreText == null)
                    HudScoreText = FindTmp(djBox, "Score", "HudScore");
                if (HudComboText == null)
                    HudComboText = FindTmp(djBox, "Combo", "HudCombo");
                if (HudFeverText == null)
                    HudFeverText = FindTmp(djBox, "Fever", "HudFever");
            }

            if (HudScoreText == null && ScoreText != null)
                HudScoreText = ScoreText;

            if (SubPatternGuideText == null)
            {
                SubPatternGuideText = FindTmp(djBox, "SubPatternGuide", "SubPattern", "LyricsGuide");
                if (SubPatternGuideText == null && SequenceText != null)
                    SubPatternGuideText = SequenceText;
            }
        }

        void AutoWireStageScreen(Transform root)
        {
            Transform stage = root.Find("StageScreen");
            if (stage == null)
                stage = root.Find("BillboardScreen");

            if (stage == null)
                return;

            StageScreenRoot = stage as RectTransform;

            if (StageBackgroundChromaKey == null)
                StageBackgroundChromaKey = FindGraphic(stage, "Bg_ChromaKey", "BgChromaKey", "ChromaKey");

            if (StageBackgroundSpace == null)
                StageBackgroundSpace = FindGraphic(stage, "Bg_Space", "BgSpace", "Space");

            if (StageBackgroundClub == null)
                StageBackgroundClub = FindGraphic(stage, "Bg_Club", "BgClub", "Club");

            if (CatAnimator == null)
            {
                Transform cat = stage.Find("Cat");
                if (cat != null)
                    CatAnimator = cat.GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Face/D-Pad/Shoulder 드라이버에서 길이 10 <see cref="DjPadButtons"/> 를 채운다.
        /// 인덱스 = <see cref="OiiaMinigameModule.OiiaDjPadButtonId"/>.
        /// </summary>
        public void EnsureDjPadButtonsArray()
        {
            if (DjPadButtons == null || DjPadButtons.Length != OiiaMinigameModule.DjPadButtonCount)
                DjPadButtons = new SnesControllerButtonVisual[OiiaMinigameModule.DjPadButtonCount];

            if (DjFaceButtons != null)
            {
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.A, DjFaceButtons.ButtonA);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.B, DjFaceButtons.ButtonB);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.X, DjFaceButtons.ButtonX);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.Y, DjFaceButtons.ButtonY);
            }

            if (DjShoulderButtons != null)
            {
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.L, DjShoulderButtons.ButtonL);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.R, DjShoulderButtons.ButtonR);
            }

            if (DjDpadButtons != null)
            {
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.Up, DjDpadButtons.ButtonUp);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.Down, DjDpadButtons.ButtonDown);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.Left, DjDpadButtons.ButtonLeft);
                AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId.Right, DjDpadButtons.ButtonRight);
            }
        }

        void AssignIfNull(OiiaMinigameModule.OiiaDjPadButtonId id, SnesControllerButtonVisual visual)
        {
            int i = (int)id;
            if (i < 0 || i >= DjPadButtons.Length)
                return;

            if (DjPadButtons[i] == null && visual != null)
                DjPadButtons[i] = visual;
        }

        void HideIdleBurstTextPoolInHierarchy()
        {
            if (BurstTextPool == null || BurstTextPool.Length == 0)
                CollectBurstTextPool(transform);

            if (BurstTextPool == null)
                return;

            for (var i = 0; i < BurstTextPool.Length; i++)
            {
                TMP_Text tmp = BurstTextPool[i];
                if (tmp == null)
                    continue;

                tmp.gameObject.SetActive(false);
                tmp.text = string.Empty;
            }
        }

        void CollectBurstTextPool(Transform root)
        {
            var list = new System.Collections.Generic.List<TMP_Text>(OiiaMinigameModule.BurstTextPoolSize);
            CollectBurstTextRecursive(BurstTextContainer != null ? BurstTextContainer : root, list);

            if (list.Count > 0)
                BurstTextPool = list.ToArray();
        }

        static void CollectBurstTextRecursive(Transform node, System.Collections.Generic.List<TMP_Text> list)
        {
            if (node.name.StartsWith("BurstText"))
            {
                TMP_Text tmp = node.GetComponent<TMP_Text>();
                if (tmp != null && !list.Contains(tmp))
                    list.Add(tmp);
            }

            for (var c = 0; c < node.childCount; c++)
                CollectBurstTextRecursive(node.GetChild(c), list);
        }

        static T GetDirectChildComponent<T>(Transform root, string childName) where T : Component
        {
            Transform child = root.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        static TMP_Text FindTmp(Transform parent, params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                Transform t = parent.Find(names[i]);
                if (t == null)
                    continue;

                TMP_Text tmp = t.GetComponent<TMP_Text>();
                if (tmp != null)
                    return tmp;
            }

            return null;
        }

        static Graphic FindGraphic(Transform parent, params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                Transform t = parent.Find(names[i]);
                if (t == null)
                    continue;

                Graphic g = t.GetComponent<Graphic>();
                if (g != null)
                    return g;
            }

            return null;
        }
    }
}
