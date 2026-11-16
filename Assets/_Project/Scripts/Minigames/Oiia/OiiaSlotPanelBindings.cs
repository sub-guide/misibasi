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
        [Header("공통")]
        public Animator CatAnimator;
        public Image SlotPanelBackgroundImage;
        public TMP_Text WaitingText;

        [Header("디제잉 박스 (Rave)")]
        public RectTransform DjBoxRoot;
        public SnesPlayerFaceButtons DjFaceButtons;
        public SnesPlayerDpadButtons DjDpadButtons;
        public SnesPlayerShoulderButtons DjShoulderButtons;
        public SnesControllerButtonVisual[] DjPadButtons;

        [Header("점수")]
        public TMP_Text HudScoreText;

        [Header("콤보 (HudDisplay 밖 · 독립)")]
        public TMP_Text HudComboText;

        [Header("티어 게이지 (DjBox 직속 Gauge_L/R_1~3)")]
        public Image[] TierGaugeLeft;
        public Image[] TierGaugeRight;

        [Header("서브 패턴 가이드")]
        public TMP_Text SubPatternGuideText;

        [Header("스피커 (DjBox 직속 SpeakerL/R_1~3)")]
        public RectTransform[] SpeakerLeft;
        public RectTransform[] SpeakerRight;

        [Header("전광판 스크린")]
        public RectTransform StageScreenRoot;
        public Graphic StageBackgroundSpace;
        public Graphic StageBackgroundClub;

        [Header("관중 이펙트 (피버)")]
        public RectTransform CrowdRoot;
        public Image CrowdImage;

        [Header("스포트라이트 (L/R · Fixture+Beam)")]
        public RectTransform SpotlightLRoot;
        public Image SpotlightLFixture;
        public Image SpotlightLBeam;
        public RectTransform SpotlightRRoot;
        public Image SpotlightRFixture;
        public Image SpotlightRBeam;

        public OiiaMinigameModule.SlotUiBindings ToSlotUiBindings()
        {
            EnsureDjPadButtonsArray();

            return new()
            {
                CatAnimator = CatAnimator,
                SlotPanelBackgroundImage = SlotPanelBackgroundImage,
                WaitingText = WaitingText,
                DjBoxRoot = DjBoxRoot,
                DjFaceButtons = DjFaceButtons,
                DjDpadButtons = DjDpadButtons,
                DjShoulderButtons = DjShoulderButtons,
                DjPadButtons = DjPadButtons,
                HudScoreText = HudScoreText,
                HudComboText = HudComboText,
                TierGaugeLeft = TierGaugeLeft,
                TierGaugeRight = TierGaugeRight,
                SubPatternGuideText = SubPatternGuideText,
                SpeakerLeft = SpeakerLeft,
                SpeakerRight = SpeakerRight,
                StageScreenRoot = StageScreenRoot,
                StageBackgroundSpace = StageBackgroundSpace,
                StageBackgroundClub = StageBackgroundClub,
                CrowdRoot = CrowdRoot,
                CrowdImage = CrowdImage,
                SpotlightLRoot = SpotlightLRoot,
                SpotlightLFixture = SpotlightLFixture,
                SpotlightLBeam = SpotlightLBeam,
                SpotlightRRoot = SpotlightRRoot,
                SpotlightRFixture = SpotlightRFixture,
                SpotlightRBeam = SpotlightRBeam,
            };
        }

        void Awake() => AutoWireFromHierarchy();

        void OnValidate() => AutoWireFromHierarchy();

        public void AutoWireFromHierarchy()
        {
            Transform root = transform;

            WaitingText = GetDirectChildComponent<TMP_Text>(root, "Waiting");
            SlotPanelBackgroundImage = GetComponent<Image>();

            if (CatAnimator == null)
                CatAnimator = GetDirectChildComponent<Animator>(root, "Cat");

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

            HudScoreText = FindTmp(djBox, "Score", "HudScore");
            if (HudScoreText == null && hud != null)
                HudScoreText = FindTmp(hud, "Score", "HudScore");

            if (hud != null)
                HudComboText = FindTmp(hud, "Combo", "HudCombo");
            if (HudComboText == null)
                HudComboText = FindTmp(djBox, "Combo", "HudCombo");
            if (HudComboText == null)
                HudComboText = FindTmp(root, "Combo", "HudCombo");

            KeepTierGauges(djBox, "Gauge_L_", ref TierGaugeLeft);
            KeepTierGauges(djBox, "Gauge_R_", ref TierGaugeRight);

            if (SubPatternGuideText == null)
                SubPatternGuideText = FindTmp(djBox, "SubPatternGuide", "SubPattern", "LyricsGuide");

            SpeakerLeft = WireSpeakerRoots(djBox, "SpeakerL_");
            SpeakerRight = WireSpeakerRoots(djBox, "SpeakerR_");

            if (CrowdRoot == null)
                AutoWireCrowd(djBox);
            if (CrowdRoot == null)
                AutoWireCrowd(root);
        }

        static RectTransform[] WireSpeakerRoots(Transform djBox, string prefix)
        {
            var roots = new RectTransform[3];
            for (var tier = 1; tier <= roots.Length; tier++)
            {
                Transform speaker = djBox.Find(prefix + tier);
                roots[tier - 1] = speaker as RectTransform;
            }

            return roots;
        }

        void AutoWireStageScreen(Transform root)
        {
            Transform stage = root.Find("StageScreen");
            if (stage == null)
                stage = root.Find("BillboardScreen");

            if (stage == null)
                return;

            StageScreenRoot = stage as RectTransform;
            Transform bgParent = stage.Find("Mask");
            if (bgParent == null)
                bgParent = stage;

            if (StageBackgroundSpace == null)
                StageBackgroundSpace = FindGraphic(bgParent, "Bg_Space", "BgSpace", "Space");

            if (StageBackgroundClub == null)
                StageBackgroundClub = FindGraphic(bgParent, "Bg_Club", "BgClub", "Club");

            if (CatAnimator == null)
            {
                Transform cat = stage.Find("Cat");
                if (cat != null)
                    CatAnimator = cat.GetComponent<Animator>();
            }

            AutoWireSpotlight(stage, "SpotlightL", "SpotL", "Light_L",
                ref SpotlightLRoot, ref SpotlightLFixture, ref SpotlightLBeam);
            AutoWireSpotlight(stage, "SpotlightR", "SpotR", "Light_R",
                ref SpotlightRRoot, ref SpotlightRFixture, ref SpotlightRBeam);

            if (SpotlightLRoot == null)
                AutoWireSpotlight(root, "SpotlightL", "SpotL", "Light_L",
                    ref SpotlightLRoot, ref SpotlightLFixture, ref SpotlightLBeam);

            if (SpotlightRRoot == null)
                AutoWireSpotlight(root, "SpotlightR", "SpotR", "Light_R",
                    ref SpotlightRRoot, ref SpotlightRFixture, ref SpotlightRBeam);

            AutoWireCrowd(stage);
            if (CrowdRoot == null)
                AutoWireCrowd(root);
        }

        void AutoWireCrowd(Transform parent)
        {
            if (parent == null || CrowdRoot != null)
                return;

            Transform crowd = parent.Find("Crowd");
            if (crowd == null)
                crowd = parent.Find("CrowdPeople");
            if (crowd == null)
                crowd = parent.Find("Audience");

            if (crowd == null)
                return;

            CrowdRoot = crowd as RectTransform;
            if (CrowdImage == null)
                CrowdImage = crowd.GetComponent<Image>();
            if (CrowdImage == null)
                CrowdImage = FindImage(crowd, "Crowd", "CrowdPeople", "Image");
        }

        static void AutoWireSpotlight(
            Transform parent,
            string rootNameA,
            string rootNameB,
            string rootNameC,
            ref RectTransform rootRt,
            ref Image fixture,
            ref Image beam)
        {
            Transform spot = parent.Find(rootNameA);
            if (spot == null)
                spot = parent.Find(rootNameB);
            if (spot == null)
                spot = parent.Find(rootNameC);

            if (spot == null)
                return;

            if (rootRt == null)
                rootRt = spot as RectTransform;

            if (fixture == null)
            {
                fixture = FindImage(spot, "Fixture", "Spotlight", "Body");
                if (fixture == null)
                    fixture = spot.GetComponent<Image>();
            }

            if (beam == null)
                beam = FindImage(spot, "Beam", "Light", "Ray");
        }

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

        /// <summary>
        /// 이미 Inspector에 연결된 칸은 유지한다. 빈 칸만 DjBox 직속에서 채운다.
        /// </summary>
        static void KeepTierGauges(Transform djBox, string prefix, ref Image[] gauges)
        {
            if (gauges == null || gauges.Length != 3)
                gauges = new Image[3];

            for (var tier = 1; tier <= 3; tier++)
            {
                if (gauges[tier - 1] != null)
                    continue;

                gauges[tier - 1] = FindImage(djBox, prefix + tier);
            }
        }

        static Image FindImage(Transform parent, params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                Transform t = parent.Find(names[i]);
                if (t == null)
                    continue;

                Image img = t.GetComponent<Image>();
                if (img != null)
                    return img;
            }

            return null;
        }
    }
}
