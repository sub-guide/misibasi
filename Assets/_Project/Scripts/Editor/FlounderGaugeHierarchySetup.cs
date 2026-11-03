using System.IO;
using MiniParty.Minigames.Flounder;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniParty.EditorTools
{
    public static class FlounderGaugeHierarchySetup
    {
        const string ScenePath = "Assets/Scenes/Minigame_Flounder.unity";
        const string SpritePath = "Assets/Minigames/Flounder/Sprites/GaugeRoundRect.png";
        const float BorderInset = 3f;

        [MenuItem("Mini Party/물 만난 광어/게이지 Hierarchy 씬에 적용")]
        public static void ApplyFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            ApplyToScene(openScene: true);
        }

        public static void ApplyToFlounderSceneBatch() => ApplyToScene(openScene: false);

        static void ApplyToScene(bool openScene)
        {
            Sprite roundSprite = GetOrCreateRoundRectSprite();
            if (roundSprite == null)
            {
                Debug.LogError("[FlounderGauge] 둥근 스프라이트를 만들지 못했습니다.");
                return;
            }

            Scene scene;
            if (openScene)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            FlounderGauge[] gauges = Object.FindObjectsOfType<FlounderGauge>(true);
            var changed = 0;
            foreach (FlounderGauge gauge in gauges)
            {
                if (ConfigureGaugeHierarchy(gauge, roundSprite))
                    changed++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[FlounderGauge] Hierarchy 적용 완료 — 게이지 {changed}개, 씬 저장: {ScenePath}");
        }

        static bool ConfigureGaugeHierarchy(FlounderGauge gauge, Sprite roundSprite)
        {
            var so = new SerializedObject(gauge);
            SerializedProperty fillProp = so.FindProperty("fill");
            Image fill = fillProp.objectReferenceValue as Image;
            if (fill == null)
                return false;

            Transform trackTransform = fill.transform.parent;
            if (trackTransform == null)
                return false;

            Image track = trackTransform.GetComponent<Image>();
            if (track == null)
                return false;

            Transform innerTransform = trackTransform.Find("GaugeInner");
            Image inner;
            if (innerTransform == null)
            {
                var innerGo = new GameObject("GaugeInner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                innerGo.transform.SetParent(trackTransform, false);
                innerGo.transform.SetSiblingIndex(0);
                inner = innerGo.GetComponent<Image>();
            }
            else
            {
                inner = innerTransform.GetComponent<Image>();
            }

            fill.transform.SetSiblingIndex(trackTransform.childCount - 1);

            ApplyTrackImage(track, roundSprite);
            ApplyInnerImage(inner, roundSprite);
            ApplyFillImage(fill, roundSprite);

            ApplyInset(inner.rectTransform, BorderInset);
            ApplyInset(fill.rectTransform, BorderInset);

            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("innerBackground").objectReferenceValue = inner;
            so.FindProperty("fill").objectReferenceValue = fill;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(gauge);
            EditorUtility.SetDirty(track);
            EditorUtility.SetDirty(inner);
            EditorUtility.SetDirty(fill);
            return true;
        }

        static void ApplyTrackImage(Image track, Sprite sprite)
        {
            track.sprite = sprite;
            track.type = Image.Type.Sliced;
            track.color = Color.black;
            track.raycastTarget = false;
        }

        static void ApplyInnerImage(Image inner, Sprite sprite)
        {
            inner.sprite = sprite;
            inner.type = Image.Type.Sliced;
            inner.color = new Color(0.12f, 0.12f, 0.14f, 1f);
            inner.raycastTarget = false;
        }

        static void ApplyFillImage(Image fill, Sprite sprite)
        {
            if (fill.color.a < 0.01f)
                fill.color = new Color(0.25f, 0.92f, 1f, 1f);

            fill.sprite = sprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;
        }

        static void ApplyInset(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        static Sprite GetOrCreateRoundRectSprite()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (existing != null)
                return existing;

            const int size = 64;
            const float radius = 14f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    float alpha = SampleRoundedRect(x + 0.5f, y + 0.5f, size, size, radius);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(SpritePath) ?? "Assets/Minigames/Flounder/Sprites");
            File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();

            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = new Vector4(radius, radius, radius, radius);
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        static float SampleRoundedRect(float x, float y, float w, float h, float r)
        {
            r = Mathf.Min(r, w * 0.5f - 0.5f, h * 0.5f - 0.5f);
            float left = r;
            float right = w - r;
            float bottom = r;
            float top = h - r;

            if (x >= left && x <= right && y >= 0f && y <= h)
                return 1f;
            if (y >= bottom && y <= top && x >= 0f && x <= w)
                return 1f;

            float cx = x < left ? left : right;
            float cy = y < bottom ? bottom : top;
            float dx = x - cx;
            float dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(1f - (dist - r));
        }
    }
}
