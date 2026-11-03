using MiniParty.Minigames.Flounder;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniParty.EditorTools
{
    public static class FlounderSlotScoreSetup
    {
        const string ScenePath = "Assets/Scenes/Minigame_Flounder.unity";
        const string FontAssetPath = "Assets/Minigames/Flounder/Fonts/NanumGothic SDF.asset";

        [MenuItem("Mini Party/물 만난 광어/슬롯 Score UI 씬에 적용")]
        public static void ApplyFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            ApplyToScene();
        }

        public static void ApplyToFlounderSceneBatch() => ApplyToScene();

        static void ApplyToScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

            FlounderPlayerSlotView[] slots = Object.FindObjectsOfType<FlounderPlayerSlotView>(true);
            var count = 0;
            foreach (FlounderPlayerSlotView slot in slots)
            {
                if (EnsureScoreLabel(slot, font))
                    count++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Flounder] Score UI 적용 — 슬롯 {count}개, 씬 저장: {ScenePath}");
        }

        static bool EnsureScoreLabel(FlounderPlayerSlotView slot, TMP_FontAsset font)
        {
            if (slot == null)
                return false;

            Transform slotRoot = slot.transform;
            Transform scoreTransform = slotRoot.Find("Score");
            TextMeshProUGUI scoreText;
            if (scoreTransform == null)
            {
                var go = new GameObject("Score", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(slotRoot, false);
                scoreText = go.AddComponent<TextMeshProUGUI>();
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.02f);
                rt.anchorMax = new Vector2(0.95f, 0.11f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
                if (scoreText == null)
                    scoreText = scoreTransform.gameObject.AddComponent<TextMeshProUGUI>();
            }

            if (font != null)
                scoreText.font = font;

            scoreText.fontSize = 20;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.color = Color.white;
            scoreText.raycastTarget = false;
            scoreText.text = "0";

            var so = new SerializedObject(slot);
            so.FindProperty("scoreLabel").objectReferenceValue = scoreText;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(slot);
            EditorUtility.SetDirty(scoreText);
            return true;
        }
    }
}
