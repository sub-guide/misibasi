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
        public TMP_Text PracticeReadyText;
        public Animator CatAnimator;
        public Image SlotPanelBackgroundImage;
        public TMP_Text WaitingText;

        [Header("디제잉 박스 (Rave)")]
        public RectTransform DjBoxRoot;
        public SnesPlayerFaceButtons DjFaceButtons;
        public SnesPlayerDpadButtons DjDpadButtons;
        public SnesPlayerShoulderButtons DjShoulderButtons;
        public SnesControllerButtonVisual[] DjPadButtons;

        [Header("소형 가변 디스플레이 (Score ↔ FEVER 상호 배타)")]
        public TMP_Text HudScoreText;
        public TMP_Text HudFeverText;

        [Header("콤보 (HudDisplay 밖 · 독립)")]
        public TMP_Text HudComboText;

        [Header("피버 게이지 (DjBox 독립 · Filled Image ×2)")]
        public Image FeverGaugeImage;
        public Image FeverGaugeImageB;

        [Header("서브 패턴 가이드")]
        public TMP_Text SubPatternGuideText;

        [Header("스피커 (DjBox 위 L/R)")]
        public RectTransform SpeakerLRoot;
        public RectTransform SpeakerLBody;
        public RectTransform SpeakerLWooferTop;
        public RectTransform SpeakerLWooferBottom;
        public RectTransform SpeakerRRoot;
        public RectTransform SpeakerRBody;
        public RectTransform SpeakerRWooferTop;
        public RectTransform SpeakerRWooferBottom;

        [Header("전광판 스크린")]
        public RectTransform StageScreenRoot;
        public Graphic StageBackgroundChromaKey;
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
                PracticeReadyText = PracticeReadyText,
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
                HudFeverText = HudFeverText,
                FeverGaugeImage = FeverGaugeImage,
                FeverGaugeImageB = FeverGaugeImageB,
                SubPatternGuideText = SubPatternGuideText,
                SpeakerLRoot = SpeakerLRoot,
                SpeakerLBody = SpeakerLBody,
                SpeakerLWooferTop = SpeakerLWooferTop,
                SpeakerLWooferBottom = SpeakerLWooferBottom,
                SpeakerRRoot = SpeakerRRoot,
                SpeakerRBody = SpeakerRBody,
                SpeakerRWooferTop = SpeakerRWooferTop,
                SpeakerRWooferBottom = SpeakerRWooferBottom,
                StageScreenRoot = StageScreenRoot,
                StageBackgroundChromaKey = StageBackgroundChromaKey,
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

            PracticeReadyText = GetDirectChildComponent<TMP_Text>(root, "Ready");
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

            // HudDisplay: Score / Fever만 (한 화면 · 상호 배타).
            Transform hud = djBox.Find("HudDisplay");
            if (hud == null)
                hud = djBox.Find("Hud");

            if (hud != null)
            {
                HudScoreText = FindTmp(hud, "Score", "HudScore");
                HudFeverText = FindTmp(hud, "Fever", "HudFever");
            }
            else
            {
                if (HudScoreText == null)
                    HudScoreText = FindTmp(djBox, "Score", "HudScore");
                if (HudFeverText == null)
                    HudFeverText = FindTmp(djBox, "Fever", "HudFever");
            }

            // Combo: HudDisplay 밖. DjBox 직속 `Combo` 권장.
            HudComboText = FindTmp(djBox, "Combo", "HudCombo");
            if (HudComboText == null)
                HudComboText = FindTmp(root, "Combo", "HudCombo");

            // FeverGauge ×2: Hud/Combo와 독립. DjBox 직속 `FeverGauge` / `FeverGaugeB` 권장.
            if (FeverGaugeImage == null)
                FeverGaugeImage = FindImage(djBox, "FeverGauge", "FeverBar");
            if (FeverGaugeImage == null)
                FeverGaugeImage = FindImage(root, "FeverGauge", "FeverBar");

            if (FeverGaugeImageB == null)
                FeverGaugeImageB = FindImage(djBox, "FeverGaugeB", "FeverGauge2", "FeverBarB");
            if (FeverGaugeImageB == null)
                FeverGaugeImageB = FindImage(root, "FeverGaugeB", "FeverGauge2", "FeverBarB");

            if (SubPatternGuideText == null)
                SubPatternGuideText = FindTmp(djBox, "SubPatternGuide", "SubPattern", "LyricsGuide");

            AutoWireSpeaker(djBox, "SpeakerL", "Speaker_L", "SpeakerLeft",
                ref SpeakerLRoot, ref SpeakerLBody, ref SpeakerLWooferTop, ref SpeakerLWooferBottom);
            AutoWireSpeaker(djBox, "SpeakerR", "Speaker_R", "SpeakerRight",
                ref SpeakerRRoot, ref SpeakerRBody, ref SpeakerRWooferTop, ref SpeakerRWooferBottom);

            Transform speakersFolder = djBox.Find("Speakers");
            if (speakersFolder != null)
            {
                if (SpeakerLRoot == null)
                    AutoWireSpeaker(speakersFolder, "SpeakerL", "Speaker_L", "SpeakerLeft",
                        ref SpeakerLRoot, ref SpeakerLBody, ref SpeakerLWooferTop, ref SpeakerLWooferBottom);

                if (SpeakerRRoot == null)
                    AutoWireSpeaker(speakersFolder, "SpeakerR", "Speaker_R", "SpeakerRight",
                        ref SpeakerRRoot, ref SpeakerRBody, ref SpeakerRWooferTop, ref SpeakerRWooferBottom);
            }

            if (SpeakerLRoot == null)
                AutoWireSpeaker(root, "SpeakerL", "Speaker_L", "SpeakerLeft",
                    ref SpeakerLRoot, ref SpeakerLBody, ref SpeakerLWooferTop, ref SpeakerLWooferBottom);

            if (SpeakerRRoot == null)
                AutoWireSpeaker(root, "SpeakerR", "Speaker_R", "SpeakerRight",
                    ref SpeakerRRoot, ref SpeakerRBody, ref SpeakerRWooferTop, ref SpeakerRWooferBottom);

            if (CrowdRoot == null)
                AutoWireCrowd(djBox);
            if (CrowdRoot == null)
                AutoWireCrowd(root);
        }

        static void AutoWireSpeaker(
            Transform parent,
            string nameA,
            string nameB,
            string nameC,
            ref RectTransform rootRt,
            ref RectTransform body,
            ref RectTransform wooferTop,
            ref RectTransform wooferBottom)
        {
            Transform speaker = parent.Find(nameA);
            if (speaker == null)
                speaker = parent.Find(nameB);
            if (speaker == null)
                speaker = parent.Find(nameC);

            if (speaker == null)
                return;

            if (rootRt == null)
                rootRt = speaker as RectTransform;

            if (body == null)
                body = FindRect(speaker, "Body", "SpeakerBody", "Cabinet");

            if (wooferTop == null)
                wooferTop = FindRect(speaker, "WooferTop", "Woofer_Top", "WooferUp");

            if (wooferBottom == null)
                wooferBottom = FindRect(speaker, "WooferBottom", "Woofer_Bottom", "WooferDown");
        }

        static RectTransform FindRect(Transform parent, params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                Transform t = parent.Find(names[i]);
                if (t == null)
                    continue;

                RectTransform rt = t as RectTransform;
                if (rt != null)
                    return rt;
            }

            return null;
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
