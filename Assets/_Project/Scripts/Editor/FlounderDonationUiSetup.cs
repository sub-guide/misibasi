using MiniParty.Minigames.Flounder;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.EditorTools
{
    public static class FlounderDonationUiSetup
    {
        const string ScenePath = "Assets/Scenes/Minigame_Flounder.unity";
        const string FontAssetPath = "Assets/Fonts/온글잎 박다현체 SDF.asset";
        const string CenterTaglineFontPath = "Assets/Fonts/NanumBarunpenB SDF.asset";
        const string StartDonationSpritePath = "Assets/Minigames/Flounder/Sprites/StartDonation_100000.png";
        const string StartDonationSpriteFallbackPath = "Assets/Minigames/Flounder/Sprites/StartDonationPopup.png";
        static readonly string[] GameplayTierSpritePaths =
        {
            "Assets/Minigames/Flounder/Sprites/StarBalloon_1000.png",
            "Assets/Minigames/Flounder/Sprites/StarBalloon_3000.png",
            "Assets/Minigames/Flounder/Sprites/StarBalloon_5000.png",
            "Assets/Minigames/Flounder/Sprites/StarBalloon_10000.png",
            "Assets/Minigames/Flounder/Sprites/StarBalloon_30000.png",
            "Assets/Minigames/Flounder/Sprites/StarBalloon_50000.png"
        };

        static readonly Color GoldText = new Color(1f, 0.88f, 0.4f, 1f);

        [MenuItem("Mini Party/물 만난 광어/후원 알림 UI 씬에 적용")]
        public static void ApplyFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            ApplyToScene();
        }

        public static void ApplyToFlounderSceneBatch() => ApplyToScene();

        static void ApplyToScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            TMP_FontAsset centerFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CenterTaglineFontPath);
            FlounderDonationUI ui = Object.FindObjectOfType<FlounderDonationUI>(true);
            if (ui == null)
            {
                Debug.LogError("[FlounderDonationUiSetup] DonationUI 를 찾을 수 없습니다.");
                return;
            }

            Transform root = ui.transform;
            var rootRect = root as RectTransform;
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            GameObject center = FindChild(root, "StartDonationPanel") ?? FindChild(root, "CenterPopup");
            GameObject feed = FindChild(root, "GameDonationPanel");
            if (feed != null)
                feed.name = "GameDonationPanel";

            EnsureCanvasGroup(center);
            EnsureCanvasGroup(feed);
            ApplyStartDonationSprite(center, font, centerFont);
            ApplyGameplayDonationSprite(feed, font);

            TextMeshProUGUI centerHeader = FindTmp(center != null ? center.transform : null, "SpecialHeader");
            if (centerHeader != null)
                centerHeader.gameObject.SetActive(false);

            TextMeshProUGUI centerTitle = FindTmp(center != null ? center.transform : null, "Title");
            TextMeshProUGUI centerAmount = FindTmp(center != null ? center.transform : null, "Amount");
            TextMeshProUGUI centerGameTitle = FindTmp(center != null ? center.transform : null, "GameTitle");

            TextMeshProUGUI message = FindTmp(feed != null ? feed.transform : null, "Message");
            TextMeshProUGUI amount = FindTmp(feed != null ? feed.transform : null, "Amount");
            TextMeshProUGUI gameplayTagline = FindTmp(feed != null ? feed.transform : null, "GameTitle");

            if (feed != null)
                feed.SetActive(false);

            var so = new SerializedObject(ui);
            so.FindProperty("centerPopupRoot").objectReferenceValue = center;
            so.FindProperty("centerCanvasGroup").objectReferenceValue = center != null ? center.GetComponent<CanvasGroup>() : null;
            so.FindProperty("centerPopupImage").objectReferenceValue = center != null ? center.GetComponent<Image>() : null;
            so.FindProperty("useStartDonationSprite").boolValue = true;
            so.FindProperty("centerSpecialHeaderText").objectReferenceValue = centerHeader;
            so.FindProperty("centerTitleText").objectReferenceValue = centerTitle;
            so.FindProperty("centerAmountText").objectReferenceValue = centerAmount;
            so.FindProperty("centerGameTitleText").objectReferenceValue = centerGameTitle;
            so.FindProperty("gameplayAlertRoot").objectReferenceValue = feed;
            so.FindProperty("gameplayCanvasGroup").objectReferenceValue = feed != null ? feed.GetComponent<CanvasGroup>() : null;
            so.FindProperty("gameplayBannerImage").objectReferenceValue = feed != null ? feed.GetComponent<Image>() : null;
            so.FindProperty("useGameplayDonationSprite").boolValue = true;
            so.FindProperty("gameplayHeaderText").objectReferenceValue = null;
            so.FindProperty("gameplayMessageText").objectReferenceValue = message;
            so.FindProperty("gameplayAmountText").objectReferenceValue = amount;
            so.FindProperty("gameplayGameTitleText").objectReferenceValue = gameplayTagline;

            FlounderPlayerSlotView slot1P = null;
            var slotViews = new FlounderPlayerSlotView[4];
            foreach (FlounderPlayerSlotView slot in Object.FindObjectsOfType<FlounderPlayerSlotView>(true))
            {
                if (slot.SlotIndex >= 0 && slot.SlotIndex < slotViews.Length)
                    slotViews[slot.SlotIndex] = slot;

                if (slot.SlotIndex == 0)
                    slot1P = slot;
            }

            var tierSprites = new Sprite[GameplayTierSpritePaths.Length];
            for (var i = 0; i < GameplayTierSpritePaths.Length; i++)
            {
                tierSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(GameplayTierSpritePaths[i]);
                if (tierSprites[i] == null)
                    Debug.LogWarning("[FlounderDonationUiSetup] 별풍선 스프라이트 없음: " + GameplayTierSpritePaths[i]);
            }

            so.FindProperty("donationFont").objectReferenceValue = font;
            so.FindProperty("centerTaglineFont").objectReferenceValue = centerFont;
            so.FindProperty("typographyReferenceSlot").objectReferenceValue = slot1P;
            so.FindProperty("gameplayHudCountdown").objectReferenceValue = root.Find("Countdown") as RectTransform;
            so.FindProperty("gameplayHudLiveUi").objectReferenceValue =
                root.parent != null ? root.parent.Find("LiveUI") as RectTransform : null;
            so.FindProperty("gameplaySlotAnchors").arraySize = slotViews.Length;
            for (var i = 0; i < slotViews.Length; i++)
                so.FindProperty("gameplaySlotAnchors").GetArrayElementAtIndex(i).objectReferenceValue = slotViews[i];

            so.FindProperty("gameplayStarBalloonSprites").arraySize = tierSprites.Length;
            for (var i = 0; i < tierSprites.Length; i++)
                so.FindProperty("gameplayStarBalloonSprites").GetArrayElementAtIndex(i).objectReferenceValue = tierSprites[i];

            so.FindProperty("slotStarBalloonScale").floatValue = 0.48f;
            so.FindProperty("slotStarBalloonScaleMin").floatValue = 0.26f;
            so.FindProperty("gameplayHudClearance").floatValue = 12f;
            so.FindProperty("gameplayAlertAboveLabelGap").floatValue = 10f;
            so.FindProperty("centerStarBalloonScale").floatValue = 1.1f;
            so.FindProperty("starBalloonMessageFontSize").floatValue = 45f;
            so.FindProperty("starBalloonTaglineFontSize").floatValue = 80f;
            so.FindProperty("centerTaglineText").stringValue = "물 만난 광어 드가자";
            so.FindProperty("previewStartDonationInEditMode").boolValue = true;
            so.FindProperty("preserveDonationUiSceneLayout").boolValue = true;
            so.FindProperty("gameplayAmountFontRatio").floatValue = 0.52f;
            so.ApplyModifiedPropertiesWithoutUndo();

            ui.RefreshStartDonationEditorPreview();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FlounderDonationUiSetup] 후원 UI 연결 완료: " + ScenePath);
        }

        static GameObject FindChild(Transform parent, string name)
        {
            if (parent == null)
                return null;

            Transform t = parent.Find(name);
            return t != null ? t.gameObject : null;
        }

        static TextMeshProUGUI FindTmp(Transform parent, string childName)
        {
            if (parent == null)
                return null;

            Transform t = parent.Find(childName);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        static void EnsureCanvasGroup(GameObject go)
        {
            if (go == null)
                return;

            if (go.GetComponent<CanvasGroup>() == null)
                go.AddComponent<CanvasGroup>();
        }

        static void ApplyStartDonationSprite(GameObject panel, TMP_FontAsset font, TMP_FontAsset centerTaglineFont)
        {
            if (panel == null)
                return;

            if (panel.name != "StartDonationPanel")
                panel.name = "StartDonationPanel";

            var outline = panel.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;

            Image image = panel.GetComponent<Image>();
            if (image == null)
                image = panel.AddComponent<Image>();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(StartDonationSpritePath);
            if (sprite == null)
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(StartDonationSpriteFallbackPath);

            if (sprite == null)
            {
                Debug.LogWarning("[FlounderDonationUiSetup] 시작 후원 스프라이트 없음.");
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            EnsureLabel(panel.transform, "SpecialHeader",
                new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.96f), "SPECIAL DONATION", 26f, GoldText, font)
                ?.gameObject.SetActive(false);

            EnsureLabel(panel.transform, "Title",
                new Vector2(0.12f, 0.4f), new Vector2(0.88f, 0.6f), "모든 플레이어에게 후원!", 39f, Color.white, font);

            TextMeshProUGUI amount = EnsureLabel(panel.transform, "Amount",
                new Vector2(0.2f, 0.68f), new Vector2(0.8f, 0.96f), "50,000", 56f, new Color(0.92f, 0.15f, 0.12f, 1f), font);
            if (amount != null)
                amount.gameObject.SetActive(false);

            TMP_FontAsset taglineFont = centerTaglineFont != null ? centerTaglineFont : font;
            TextMeshProUGUI tagline = EnsureLabel(panel.transform, "GameTitle",
                new Vector2(0.02f, -0.42f), new Vector2(0.98f, -0.04f), "물 만난 광어 드가자", 80f, Color.white, taglineFont);
            if (tagline != null)
            {
                tagline.richText = true;
                tagline.alignment = TextAlignmentOptions.Center;
                tagline.fontStyle = FontStyles.Normal;
                tagline.outlineWidth = 0f;
            }
        }

        static void ApplyGameplayDonationSprite(GameObject panel, TMP_FontAsset font)
        {
            if (panel == null)
                return;

            RemoveLegacyGameplayOverlays(panel.transform);

            var outline = panel.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;

            Image image = panel.GetComponent<Image>();
            if (image == null)
                image = panel.AddComponent<Image>();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GameplayTierSpritePaths[0]);

            if (sprite == null)
            {
                Debug.LogWarning("[FlounderDonationUiSetup] 게임 중 별풍선 스프라이트 없음.");
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            TextMeshProUGUI message = EnsureLabel(panel.transform, "Message",
                new Vector2(0.02f, -0.42f), new Vector2(0.98f, -0.04f),
                "광어사랑123님이 <color=#FFE066><b>2P</b></color>에게 후원!", 45f, Color.white, font);
            if (message != null)
            {
                message.richText = true;
                message.alignment = TextAlignmentOptions.Center;
            }

            TextMeshProUGUI gAmount = EnsureLabel(panel.transform, "Amount",
                new Vector2(0.18f, 0.66f), new Vector2(0.82f, 0.94f), "10,000", 56f, new Color(0.92f, 0.15f, 0.12f, 1f), font);
            if (gAmount != null)
                gAmount.gameObject.SetActive(false);

            TextMeshProUGUI tagline = EnsureLabel(panel.transform, "GameTitle",
                new Vector2(0.02f, -0.38f), new Vector2(0.98f, -0.2f), "물 만난 광어 드가자잇", 30f, Color.white, font);
            if (tagline != null)
            {
                tagline.alignment = TextAlignmentOptions.Center;
                tagline.fontStyle = FontStyles.Bold;
                tagline.outlineWidth = 0.18f;
                tagline.outlineColor = new Color32(0, 0, 0, 200);
            }
        }

        static void RemoveLegacyGameplayOverlays(Transform panel)
        {
            foreach (string name in new[] { "Feed", "Header", "Footer", "TextBackdrop", "SlotBadge" })
            {
                Transform child = panel.Find(name);
                if (child != null)
                    Object.DestroyImmediate(child.gameObject);
            }

            var outline = panel.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }

        static TextMeshProUGUI EnsureLabel(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string text,
            float fontSize,
            Color color,
            TMP_FontAsset font)
        {
            if (parent == null)
                return null;

            Transform existing = parent.Find(name);
            TextMeshProUGUI tmp;
            if (existing == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(parent, false);
                tmp = go.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                tmp = existing.GetComponent<TextMeshProUGUI>();
                if (tmp == null)
                    tmp = existing.gameObject.AddComponent<TextMeshProUGUI>();
            }

            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            if (font != null)
            {
                tmp.font = font;
                if (font.material != null)
                    tmp.fontSharedMaterial = font.material;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.gameObject.SetActive(true);
            return tmp;
        }
    }
}
