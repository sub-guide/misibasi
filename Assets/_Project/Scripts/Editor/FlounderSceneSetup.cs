using System.Collections.Generic;
using System.IO;
using MiniParty.Minigames.Flounder;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace MiniParty.EditorTools
{
    public static class FlounderSceneSetup
    {
        const string ScenePath = "Assets/Scenes/Minigame_Flounder.unity";
        const string BackgroundPath = "Assets/Minigames/Flounder/Sprites/Background_Studio.jpg";
        const string FontFilePath = "Assets/Minigames/Flounder/Fonts/NanumGothic-Regular.ttf";
        const string FontAssetPath = "Assets/Minigames/Flounder/Fonts/NanumGothic SDF.asset";
        const string CharacterFramesFolder = "Assets/Minigames/Flounder/Characters/PlayerFlop_Frames";
        const int CharacterFrameCount = 5;
        const float CharacterGifScale1P = 0.78f;

        static readonly Vector2[] SlotAnchors =
        {
            new(0.06f, 0.08f), new(0.29f, 0.08f), new(0.52f, 0.08f), new(0.75f, 0.08f)
        };

        static readonly float[] SlotWorldX = { -3.2f, -1.1f, 1.1f, 3.2f };

        [MenuItem("Mini Party/물 만난 광어/새 씬 만들기")]
        public static void CreateFreshSceneAndOpen()
        {
            if (!EditorUtility.DisplayDialog(
                    "물 만난 광어",
                    "빈 씬부터 Minigame_Flounder.unity 를 새로 만듭니다.\n기존 씬 내용은 덮어씁니다.",
                    "만들기",
                    "취소"))
                return;

            CreateOrUpdateSceneInternal(openAfterSave: true);
        }

        [MenuItem("Mini Party/물 만난 광어/씬 구성")]
        public static void CreateOrUpdateSceneMenu() => CreateOrUpdateSceneInternal(openAfterSave: false);

        /// <summary>Unity batchmode -executeMethod 용 (인자 없음).</summary>
        public static void CreateOrUpdateScene() => CreateOrUpdateSceneInternal(openAfterSave: false);

        static void CreateOrUpdateSceneInternal(bool openAfterSave)
        {
            bool ok = false;
            try
            {
                Sprite bg = EnsureBackgroundSprite();
                TMP_FontAsset font = EnsureFontAsset();
                BuildScene(bg, font);
                ok = true;
                Debug.Log("[Flounder] 새 씬 저장: " + ScenePath);

                if (openAfterSave)
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }

            if (Application.isBatchMode)
                EditorApplication.Exit(ok ? 0 : 1);
        }

        static void BuildScene(Sprite background, TMP_FontAsset font)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Flounder");

            var module = root.AddComponent<FlounderMinigameModule>();
            var manager = root.AddComponent<FlounderGameManager>();
            var donations = root.AddComponent<FlounderDonationManager>();
            var bootstrap = root.AddComponent<FlounderSceneBootstrap>();
            Camera cam = CreateCamera(root.transform);
            CreateBackground(root.transform, cam, background);

            Canvas canvas = CreateCanvas(root.transform);
            CreateEventSystem();

            UiPack ui = BuildUi(canvas.transform, font);
            FlounderPlayerSlotView[] slots = BuildPlayerSlots(root.transform, canvas.transform, font);

            FlounderInput[] inputs = new FlounderInput[4];
            for (var i = 0; i < 4; i++)
            {
                var inputGo = new GameObject("Input_" + (i + 1) + "P");
                inputGo.transform.SetParent(root.transform, false);
                var input = inputGo.AddComponent<FlounderInput>();
                var so = new SerializedObject(input);
                so.FindProperty("playerSlotIndex").intValue = i;
                so.ApplyModifiedPropertiesWithoutUndo();
                inputs[i] = input;
            }

            WireManager(manager, ui, slots, inputs, donations);
            WireModule(module, manager);
            WireBootstrap(bootstrap, module);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
        }

        static void WireBootstrap(FlounderSceneBootstrap bootstrap, FlounderMinigameModule module)
        {
            var so = new SerializedObject(bootstrap);
            so.FindProperty("module").objectReferenceValue = module;
            so.FindProperty("debugSoloWithoutPartySession").boolValue = true;
            so.FindProperty("debugActivePlayerCount").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireModule(FlounderMinigameModule module, FlounderGameManager manager)
        {
            var so = new SerializedObject(module);
            so.FindProperty("gameManager").objectReferenceValue = manager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireManager(
            FlounderGameManager manager,
            UiPack ui,
            FlounderPlayerSlotView[] slots,
            FlounderInput[] inputs,
            FlounderDonationManager donations)
        {
            var so = new SerializedObject(manager);
            so.FindProperty("donationManager").objectReferenceValue = donations;
            so.FindProperty("donationUi").objectReferenceValue = ui.DonationUi;
            so.FindProperty("countdown").objectReferenceValue = ui.Countdown;
            so.FindProperty("liveUi").objectReferenceValue = ui.LiveUi;
            so.FindProperty("resultUi").objectReferenceValue = ui.ResultUi;
            so.FindProperty("roundTimerText").objectReferenceValue = ui.TimerText;

            var slotProp = so.FindProperty("slotViews");
            slotProp.arraySize = 4;
            for (var i = 0; i < 4; i++)
                slotProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];

            var inputProp = so.FindProperty("slotInputs");
            inputProp.arraySize = 4;
            for (var i = 0; i < 4; i++)
                inputProp.GetArrayElementAtIndex(i).objectReferenceValue = inputs[i];

            so.FindProperty("gifCharacterOnlyOnFirstSlot").boolValue = true;
            so.FindProperty("firstSlotGifScale").floatValue = CharacterGifScale1P;
            so.FindProperty("firstSlotGifLocalY").floatValue = 0.42f;

            so.ApplyModifiedPropertiesWithoutUndo();

        }

        static Camera CreateCamera(Transform parent)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 0f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.08f, 0.18f);
            go.AddComponent<AudioListener>();
            return cam;
        }

        static void CreateBackground(Transform parent, Camera cam, Sprite sprite)
        {
            var go = new GameObject("Background");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 0f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100;
            var fit = go.AddComponent<FlounderBackgroundFit>();
            var so = new SerializedObject(fit);
            so.FindProperty("targetCamera").objectReferenceValue = cam;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();
            fit.Fit();
        }

        static Canvas CreateCanvas(Transform parent)
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1 |
                AdditionalCanvasShaderChannels.Normal |
                AdditionalCanvasShaderChannels.Tangent;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        static UiPack BuildUi(Transform canvas, TMP_FontAsset font)
        {
            var pack = new UiPack();

            pack.LiveUi = CreateLive(canvas, font, out pack.LiveLabel);
            pack.TimerText = CreateLabel(canvas, "Timer", new Vector2(0.42f, 0.9f), new Vector2(0.58f, 0.98f),
                "30.0", 56f, TextAlignmentOptions.Top, font, Color.white);

            pack.DonationUi = CreateDonationUi(canvas, font, out pack.Countdown);
            WireCountdownStyle(pack.Countdown, pack.TimerText);
            pack.ResultUi = CreateResultUi(canvas, font);

            return pack;
        }

        static FlounderLiveUI CreateLive(Transform canvas, TMP_FontAsset font, out TextMeshProUGUI label)
        {
            var go = new GameObject("LiveUI", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            StretchAnchors(go.GetComponent<RectTransform>(), new Vector2(0.02f, 0.9f), new Vector2(0.28f, 0.98f));

            Image liveBg = CreateSolid(go.transform, "LiveBadge", new Vector2(0f, 0.2f), new Vector2(0.22f, 0.85f),
                new Color(0.85f, 0.1f, 0.12f, 1f));
            CreateLabel(liveBg.rectTransform, "Text", Vector2.zero, Vector2.one, "LIVE", 28f,
                TextAlignmentOptions.Center, font, Color.white);

            label = CreateLabel(go.transform, "ViewerCount", new Vector2(0.24f, 0.2f), new Vector2(1f, 0.85f),
                "LIVE  12,000", 32f, TextAlignmentOptions.MidlineLeft, font, new Color(0.95f, 0.15f, 0.15f, 1f));

            var live = go.AddComponent<FlounderLiveUI>();
            var so = new SerializedObject(live);
            so.FindProperty("liveLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
            return live;
        }

        const string DonationFontAssetPath = "Assets/Fonts/온글잎 박다현체 SDF.asset";

        static FlounderDonationUI CreateDonationUi(Transform canvas, TMP_FontAsset font, out FlounderCountdown countdown)
        {
            var uiGo = new GameObject("DonationUI", typeof(RectTransform));
            uiGo.transform.SetParent(canvas, false);
            StretchAnchors(uiGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            var ui = uiGo.AddComponent<FlounderDonationUI>();

            TMP_FontAsset donationFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DonationFontAssetPath) ?? font;

            GameObject gamePanel = CreateGameplayDonationPanel(uiGo.transform);
            gamePanel.AddComponent<CanvasGroup>();
            gamePanel.SetActive(false);
            TextMeshProUGUI gHeader = CreateLabel(gamePanel.transform, "Header", new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.96f),
                "DONATION", 22f, TextAlignmentOptions.Center, donationFont, new Color(1f, 0.88f, 0.4f, 1f));
            gHeader.gameObject.SetActive(false);
            TextMeshProUGUI gMessage = CreateLabel(gamePanel.transform, "Message", new Vector2(0.33f, 0.5f), new Vector2(0.96f, 0.8f),
                "광어사랑123님이 2P에게 후원했습니다!", 17f, TextAlignmentOptions.Center, donationFont, Color.white);
            gMessage.richText = true;
            TextMeshProUGUI gAmount = CreateLabel(gamePanel.transform, "Amount", new Vector2(0.33f, 0.18f), new Vector2(0.96f, 0.5f),
                "10,000원!", 30f, TextAlignmentOptions.Center, donationFont, new Color(1f, 0.88f, 0.4f, 1f));
            TextMeshProUGUI gTitle = CreateLabel(gamePanel.transform, "GameTitle", new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.24f),
                "물 만난 광어", 22f, TextAlignmentOptions.Center, donationFont, Color.white);
            gTitle.gameObject.SetActive(false);

            GameObject center = CreateStartDonationPanel(uiGo.transform);
            center.AddComponent<CanvasGroup>();
            TextMeshProUGUI special = CreateLabel(center.transform, "SpecialHeader", new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.96f),
                "SPECIAL DONATION", 26f, TextAlignmentOptions.Center, donationFont, new Color(1f, 0.88f, 0.4f, 1f));
            special.gameObject.SetActive(false);
            TextMeshProUGUI t1 = CreateLabel(center.transform, "Title", new Vector2(0.12f, 0.4f), new Vector2(0.88f, 0.6f),
                "모든 플레이어에게 후원!", 30f, TextAlignmentOptions.Center, donationFont, Color.white);
            TextMeshProUGUI t2 = CreateLabel(center.transform, "Amount", new Vector2(0.12f, 0.24f), new Vector2(0.88f, 0.42f),
                "50,000원씩", 38f, TextAlignmentOptions.Center, donationFont, new Color(1f, 0.88f, 0.4f, 1f));
            TextMeshProUGUI t3 = CreateLabel(center.transform, "GameTitle", new Vector2(0.32f, 0.07f), new Vector2(0.68f, 0.2f),
                "물 만난 광어", 22f, TextAlignmentOptions.Center, donationFont, Color.white);
            center.SetActive(false);

            var cdGo = new GameObject("Countdown", typeof(RectTransform));
            cdGo.transform.SetParent(uiGo.transform, false);
            StretchAnchors(cdGo.GetComponent<RectTransform>(), new Vector2(0.35f, 0.42f), new Vector2(0.65f, 0.58f));
            cdGo.SetActive(false);
            TextMeshProUGUI cdLabel = CreateLabel(cdGo.transform, "Label", Vector2.zero, Vector2.one,
                "3", 100f, TextAlignmentOptions.Center, font, Color.black);
            countdown = cdGo.AddComponent<FlounderCountdown>();
            var cdSo = new SerializedObject(countdown);
            cdSo.FindProperty("root").objectReferenceValue = cdGo;
            cdSo.FindProperty("label").objectReferenceValue = cdLabel;
            cdSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(ui);
            so.FindProperty("centerPopupRoot").objectReferenceValue = center;
            so.FindProperty("centerCanvasGroup").objectReferenceValue = center.GetComponent<CanvasGroup>();
            so.FindProperty("centerPopupImage").objectReferenceValue = center.GetComponent<Image>();
            so.FindProperty("useStartDonationSprite").boolValue = true;
            so.FindProperty("centerSpecialHeaderText").objectReferenceValue = special;
            so.FindProperty("centerTitleText").objectReferenceValue = t1;
            so.FindProperty("centerAmountText").objectReferenceValue = t2;
            so.FindProperty("centerGameTitleText").objectReferenceValue = t3;
            so.FindProperty("gameplayAlertRoot").objectReferenceValue = gamePanel;
            so.FindProperty("gameplayCanvasGroup").objectReferenceValue = gamePanel.GetComponent<CanvasGroup>();
            so.FindProperty("gameplayBannerImage").objectReferenceValue = gamePanel.GetComponent<Image>();
            so.FindProperty("useGameplayDonationSprite").boolValue = true;
            so.FindProperty("gameplayHeaderText").objectReferenceValue = gHeader;
            so.FindProperty("gameplayMessageText").objectReferenceValue = gMessage;
            so.FindProperty("gameplayAmountText").objectReferenceValue = gAmount;
            so.FindProperty("gameplayGameTitleText").objectReferenceValue = gTitle;
            so.FindProperty("donationFont").objectReferenceValue = donationFont;
            so.ApplyModifiedPropertiesWithoutUndo();

            return ui;
        }

        static void WireCountdownStyle(FlounderCountdown countdown, TextMeshProUGUI timerText)
        {
            if (countdown == null || timerText == null)
                return;

            var cdSo = new SerializedObject(countdown);
            cdSo.FindProperty("styleReference").objectReferenceValue = timerText;
            cdSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static FlounderResultUI CreateResultUi(Transform canvas, TMP_FontAsset font)
        {
            Image panel = CreateSolid(canvas, "ResultPanel", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.75f));
            panel.gameObject.SetActive(false);
            TextMeshProUGUI body = CreateLabel(panel.rectTransform, "Body", new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.75f),
                "결과", 36f, TextAlignmentOptions.Top, font, Color.white);
            Button restart = CreateButton(panel.rectTransform, "Restart", "다시 하기", font,
                new Vector2(0.38f, 0.12f), new Vector2(0.62f, 0.2f));

            var result = panel.gameObject.AddComponent<FlounderResultUI>();
            var so = new SerializedObject(result);
            so.FindProperty("root").objectReferenceValue = panel.gameObject;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("restartButton").objectReferenceValue = restart;
            so.ApplyModifiedPropertiesWithoutUndo();
            return result;
        }

        static FlounderPlayerSlotView[] BuildPlayerSlots(Transform worldRoot, Transform canvas, TMP_FontAsset font)
        {
            Sprite[] flopFrames = LoadCharacterFlopFrames();
            var slots = new FlounderPlayerSlotView[4];
            for (var i = 0; i < 4; i++)
            {
                float ax = SlotAnchors[i].x;
                var slotUi = new GameObject("Slot_" + (i + 1) + "P", typeof(RectTransform));
                slotUi.transform.SetParent(canvas, false);
                StretchAnchors(slotUi.GetComponent<RectTransform>(),
                    new Vector2(ax, 0.06f), new Vector2(ax + 0.2f, 0.28f));

                TextMeshProUGUI label = CreateLabel(slotUi.transform, "Label", new Vector2(0f, 0.72f), new Vector2(1f, 1f),
                    (i + 1) + "P", 24f, TextAlignmentOptions.Center, font, Color.white);

                Image track = CreateSolid(slotUi.transform, "GaugeTrack", new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.55f),
                    new Color(0f, 0f, 0f, 0.45f));
                Image fill = CreateSolid(track.rectTransform, "Fill", Vector2.zero, Vector2.one,
                    new Color(0.2f, 0.9f, 1f, 1f));
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = 0f;
                TextMeshProUGUI pct = CreateLabel(slotUi.transform, "Percent", new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.34f),
                    "0%", 22f, TextAlignmentOptions.Center, font, Color.white);

                var gauge = slotUi.AddComponent<FlounderGauge>();
                var gaugeSo = new SerializedObject(gauge);
                gaugeSo.FindProperty("fill").objectReferenceValue = fill;
                gaugeSo.FindProperty("percentLabel").objectReferenceValue = pct;
                gaugeSo.ApplyModifiedPropertiesWithoutUndo();

                var charRoot = new GameObject("Character_" + (i + 1) + "P");
                charRoot.transform.SetParent(worldRoot, false);
                charRoot.transform.localPosition = new Vector3(SlotWorldX[i], -2.45f, 0f);

                FlounderDance dance = null;
                if (i == 0)
                    dance = CreateGifCharacter(charRoot.transform, flopFrames, CharacterGifScale1P);

                var slotView = slotUi.AddComponent<FlounderPlayerSlotView>();
                var slotSo = new SerializedObject(slotView);
                slotSo.FindProperty("slotIndex").intValue = i;
                slotSo.FindProperty("characterRoot").objectReferenceValue = charRoot.transform;
                slotSo.FindProperty("gauge").objectReferenceValue = gauge;
                slotSo.FindProperty("dance").objectReferenceValue = dance;
                slotSo.FindProperty("slotLabel").objectReferenceValue = label;
                slotSo.ApplyModifiedPropertiesWithoutUndo();

                slots[i] = slotView;
            }

            return slots;
        }

        static FlounderDance CreateGifCharacter(Transform root, Sprite[] frames, float scale)
        {
            var visual = new GameObject("GifVisual");
            visual.transform.SetParent(root, false);
            visual.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            visual.transform.localScale = Vector3.one * scale;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 25;
            renderer.sprite = frames[0];

            var dance = root.gameObject.AddComponent<FlounderDance>();
            var danceSo = new SerializedObject(dance);
            danceSo.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            SerializedProperty frameProp = danceSo.FindProperty("frames");
            frameProp.arraySize = frames.Length;
            for (var f = 0; f < frames.Length; f++)
                frameProp.GetArrayElementAtIndex(f).objectReferenceValue = frames[f];
            SetIntArray(danceSo.FindProperty("liftFrameIndices"), new[] { 0, 1, 3 });
            SetIntArray(danceSo.FindProperty("lowerFromPeakIndices"), new[] { 4, 0 });
            SetIntArray(danceSo.FindProperty("lowerFromMidIndices"), new[] { 0 });
            danceSo.ApplyModifiedPropertiesWithoutUndo();
            return dance;
        }

        static Sprite[] LoadCharacterFlopFrames()
        {
            var frames = new Sprite[CharacterFrameCount];
            for (var i = 0; i < CharacterFrameCount; i++)
            {
                string path = $"{CharacterFramesFolder}/frame_{i:D3}.png";
                if (!File.Exists(path))
                    throw new FileNotFoundException("캐릭터 GIF 프레임이 없습니다. PlayerFlop_Frames 를 확인하세요.", path);

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.spritePixelsPerUnit = 100f;
                    importer.SaveAndReimport();
                }

                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i] == null)
                    throw new FileNotFoundException("스프라이트로 불러오지 못했습니다.", path);
            }

            return frames;
        }

        static Image CreateSolid(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            StretchAnchors(go.GetComponent<RectTransform>(), min, max);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static void AddGoldOutline(GameObject go)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.82f, 0.2f, 1f);
            outline.effectDistance = new Vector2(3f, -3f);
        }

        static GameObject CreateGameplayDonationPanel(Transform parent)
        {
            const string spritePath = "Assets/Minigames/Flounder/Sprites/StarBalloon_1000.png";
            var go = new GameObject("GameDonationPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            go.layer = 5;
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Image image = go.GetComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
                image.SetNativeSize();
                rt.sizeDelta *= 0.48f;
            }

            image.raycastTarget = false;
            return go;
        }

        static GameObject CreateStartDonationPanel(Transform parent)
        {
            const string spritePath = "Assets/Minigames/Flounder/Sprites/StartDonationPopup.png";
            var go = new GameObject("StartDonationPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            StretchAnchors(go.GetComponent<RectTransform>(), new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f));

            Image image = go.GetComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            else
            {
                image.color = new Color(0.35f, 0.12f, 0.55f, 0.95f);
            }

            image.raycastTarget = false;
            return go;
        }

        static TextMeshProUGUI CreateLabel(
            Transform parent, string name, Vector2 min, Vector2 max, string text, float size,
            TextAlignmentOptions align, TMP_FontAsset font, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            StretchAnchors(go.GetComponent<RectTransform>(), min, max);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.font = font;
            if (font != null && font.material != null)
                label.fontSharedMaterial = font.material;
            label.text = text;
            label.fontSize = size;
            label.alignment = align;
            label.color = color;
            if (font != null && font.material != null)
            {
                label.outlineWidth = 0.2f;
                label.outlineColor = new Color32(0, 0, 0, 180);
            }
            label.raycastTarget = false;
            return label;
        }

        static Button CreateButton(Transform parent, string name, string label, TMP_FontAsset font, Vector2 min, Vector2 max)
        {
            Image img = CreateSolid(parent, name, min, max, new Color(0.9f, 0.45f, 0.15f, 1f));
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            CreateLabel(img.rectTransform, "Label", Vector2.zero, Vector2.one, label, 28f,
                TextAlignmentOptions.Center, font, Color.white);
            return btn;
        }

        static void StretchAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static Sprite EnsureBackgroundSprite()
        {
            if (!File.Exists(BackgroundPath))
                throw new FileNotFoundException("배경 없음", BackgroundPath);

            var importer = AssetImporter.GetAtPath(BackgroundPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.spritePixelsPerUnit = 100f;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        }

        static TMP_FontAsset EnsureFontAsset()
        {
            Directory.CreateDirectory("Assets/Minigames/Flounder/Fonts");

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null && existing.material != null)
                return existing;

            if (existing != null)
                AssetDatabase.DeleteAsset(FontAssetPath);

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontFilePath);
            if (font == null)
                throw new FileNotFoundException("NanumGothic.ttf 없음", FontFilePath);

            var importer = AssetImporter.GetAtPath(FontFilePath) as TrueTypeFontImporter;
            if (importer != null && !importer.includeFontData)
            {
                importer.includeFontData = true;
                importer.SaveAndReimport();
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
                font, 48, 5, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            asset.name = "NanumGothic SDF";
            asset.TryAddCharacters(
                "물만난광어LIVE0123456789.,%원P후보너스연타결과다시하기시작FEVER TIME모든플레이어에게후원내역",
                out _);

            AssetDatabase.CreateAsset(asset, FontAssetPath);
            if (asset.material != null)
            {
                asset.material.name = "NanumGothic SDF Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }

            if (asset.atlasTextures != null)
            {
                for (int i = 0; i < asset.atlasTextures.Length; i++)
                {
                    if (asset.atlasTextures[i] == null)
                        continue;
                    asset.atlasTextures[i].name = "NanumGothic Atlas " + i;
                    AssetDatabase.AddObjectToAsset(asset.atlasTextures[i], asset);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(FontAssetPath);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        }

        static void SetIntArray(SerializedProperty property, int[] values)
        {
            if (property == null || values == null)
                return;

            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).intValue = values[i];
        }

        static void EnsureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene s in scenes)
            {
                if (s.path == ScenePath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        sealed class UiPack
        {
            public FlounderLiveUI LiveUi;
            public TextMeshProUGUI LiveLabel;
            public TextMeshProUGUI TimerText;
            public FlounderDonationUI DonationUi;
            public FlounderCountdown Countdown;
            public FlounderResultUI ResultUi;
        }
    }
}
