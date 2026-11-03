using System.IO;
using MiniParty.Minigames.Flounder;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniParty.EditorTools
{
    public static class FlounderSlotCharacterSetup
    {
        const string ScenePath = "Assets/Scenes/Minigame_Flounder.unity";
        const int FrameCount = 5;

        static readonly Vector3 GifVisualLocalPosition = new(-2.57f, 0.93f, 0f);
        static readonly Vector3 GifVisualLocalScale = new(1.5f, 1.5f, 1.5f);

        const string Player1FramesFolder = "PlayerFlop_Frames";

        [MenuItem("Mini Party/물 만난 광어/1P GIF 캐릭터 복구")]
        public static void Restore1PGifFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Restore1PGifToSceneInternal();
        }

        internal static void Restore1PGifToSceneInternal()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            FlounderGameManager manager = Object.FindObjectOfType<FlounderGameManager>(true);

            SetupCharacter("Character_1P", 0, Player1FramesFolder, null);

            if (manager != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("gifCharacterOnlyOnFirstSlot").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FlounderSlotCharacterSetup] 1P GIF 캐릭터 복구 완료");
        }

        [MenuItem("Mini Party/물 만난 광어/2P 핑크 GIF 캐릭터 씬에 적용")]
        public static void Apply2PPinkFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Apply2PPinkToSceneInternal();
        }

        internal static void Apply2PPinkToSceneInternal()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            FlounderDance referenceDance = FindReferenceDance();
            FlounderGameManager manager = Object.FindObjectOfType<FlounderGameManager>(true);

            SetupCharacter("Character_2P", 1, "Player2P_Frames", referenceDance);

            if (manager != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("gifCharacterOnlyOnFirstSlot").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            FlounderPlayerSlotView[] slots = Object.FindObjectsOfType<FlounderPlayerSlotView>(true);
            foreach (FlounderPlayerSlotView slot in slots)
            {
                if (slot.SlotIndex != 1)
                    continue;

                var slotSo = new SerializedObject(slot);
                slotSo.FindProperty("gifCharacterTint").colorValue = Color.white;
                slotSo.ApplyModifiedPropertiesWithoutUndo();
                slot.ApplyGifCharacterTint();
                break;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FlounderSlotCharacterSetup] 2P 핑크 GIF 캐릭터 적용 완료");
        }

        [MenuItem("Mini Party/물 만난 광어/1P 캐릭터를 2P·3P·4P에 복제")]
        public static void Apply1PCloneFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Apply1PCloneToSceneInternal();
        }

        [MenuItem("Mini Party/물 만난 광어/2P·3P GIF 캐릭터 씬에 적용")]
        public static void ApplyFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            ApplyToSceneInternal();
        }

        /// <summary>Unity batchmode -executeMethod 용.</summary>
        public static void ApplyToFlounderSceneBatch() => ApplyToSceneInternal();

        public static void Apply1PCloneToFlounderSceneBatch() => Apply1PCloneToSceneInternal();

        internal static void Apply1PCloneToSceneInternal()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            FlounderDance referenceDance = FindReferenceDance();
            FlounderGameManager manager = Object.FindObjectOfType<FlounderGameManager>(true);

            SetupCharacter("Character_1P", 0, Player1FramesFolder, referenceDance);
            SetupCharacter("Character_2P", 1, Player1FramesFolder, referenceDance);
            SetupCharacter("Character_3P", 2, Player1FramesFolder, referenceDance);
            SetupCharacter("Character_4P", 3, Player1FramesFolder, referenceDance);

            if (manager != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("gifCharacterOnlyOnFirstSlot").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FlounderSlotCharacterSetup] 1P 캐릭터 2P·3P·4P 복제 완료");
        }

        internal static void ApplyToSceneInternal()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            FlounderDance referenceDance = FindReferenceDance();
            FlounderGameManager manager = Object.FindObjectOfType<FlounderGameManager>(true);

            SetupCharacter("Character_2P", 1, "Player2P_Frames", referenceDance);
            SetupCharacter("Character_3P", 2, "Player3P_Frames", referenceDance);

            if (manager != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("gifCharacterOnlyOnFirstSlot").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FlounderSlotCharacterSetup] 2P·3P GIF 캐릭터 적용 완료");
        }

        static FlounderDance FindReferenceDance()
        {
            FlounderPlayerSlotView[] slots = Object.FindObjectsOfType<FlounderPlayerSlotView>(true);
            foreach (FlounderPlayerSlotView slot in slots)
            {
                if (slot.SlotIndex == 0 && slot.Dance != null)
                    return slot.Dance;
            }

            return Object.FindObjectOfType<FlounderDance>(true);
        }

        static void SetupCharacter(string rootName, int slotIndex, string framesFolder, FlounderDance referenceDance)
        {
            Transform root = GameObject.Find(rootName)?.transform;
            if (root == null)
            {
                Debug.LogWarning("[FlounderSlotCharacterSetup] 찾을 수 없음: " + rootName);
                return;
            }

            Sprite[] frames = LoadFrames(framesFolder);
            if (frames.Length == 0)
                return;

            Transform visual = root.Find("GifVisual");
            if (visual == null)
            {
                var visualGo = new GameObject("GifVisual");
                visualGo.transform.SetParent(root, false);
                visual = visualGo.transform;
            }

            visual.localPosition = GifVisualLocalPosition;
            visual.localScale = GifVisualLocalScale;

            var renderer = visual.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = visual.gameObject.AddComponent<SpriteRenderer>();

            renderer.sortingOrder = 25;
            renderer.sprite = frames[0];

            FlounderDance dance = root.GetComponent<FlounderDance>();
            if (dance == null)
                dance = root.gameObject.AddComponent<FlounderDance>();

            var danceSo = new SerializedObject(dance);
            danceSo.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            SerializedProperty frameProp = danceSo.FindProperty("frames");
            frameProp.arraySize = frames.Length;
            for (var i = 0; i < frames.Length; i++)
                frameProp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];

            CopyDanceMotionFromReference(danceSo, referenceDance);
            danceSo.ApplyModifiedPropertiesWithoutUndo();

            FlounderPlayerSlotView[] slotViews = Object.FindObjectsOfType<FlounderPlayerSlotView>(true);
            foreach (FlounderPlayerSlotView slot in slotViews)
            {
                if (slot.SlotIndex != slotIndex)
                    continue;

                var slotSo = new SerializedObject(slot);
                slotSo.FindProperty("characterRoot").objectReferenceValue = root;
                slotSo.FindProperty("dance").objectReferenceValue = dance;
                slotSo.ApplyModifiedPropertiesWithoutUndo();
                slot.ApplyGifCharacterTint();
                break;
            }
        }

        static void CopyDanceMotionFromReference(SerializedObject danceSo, FlounderDance reference)
        {
            if (reference == null)
            {
                SetIntArray(danceSo.FindProperty("liftFrameIndices"), new[] { 0, 1, 3 });
                SetIntArray(danceSo.FindProperty("lowerFromPeakIndices"), new[] { 4, 0 });
                SetIntArray(danceSo.FindProperty("lowerFromMidIndices"), new[] { 0 });
                danceSo.FindProperty("flapFps").floatValue = 16f;
                danceSo.FindProperty("mashSpeedBoost").floatValue = 14f;
                danceSo.FindProperty("mashBoostDecay").floatValue = 10f;
                danceSo.FindProperty("liftAdvancePerFlap").floatValue = 0.55f;
                danceSo.FindProperty("lowerAdvancePerFlap").floatValue = 0.65f;
                danceSo.FindProperty("peakFlutterFps").floatValue = 28f;
                return;
            }

            var refSo = new SerializedObject(reference);
            danceSo.FindProperty("flapFps").floatValue = refSo.FindProperty("flapFps").floatValue;
            danceSo.FindProperty("mashSpeedBoost").floatValue = refSo.FindProperty("mashSpeedBoost").floatValue;
            danceSo.FindProperty("mashBoostDecay").floatValue = refSo.FindProperty("mashBoostDecay").floatValue;
            danceSo.FindProperty("liftAdvancePerFlap").floatValue = refSo.FindProperty("liftAdvancePerFlap").floatValue;
            danceSo.FindProperty("lowerAdvancePerFlap").floatValue = refSo.FindProperty("lowerAdvancePerFlap").floatValue;
            danceSo.FindProperty("peakFlutterFps").floatValue = refSo.FindProperty("peakFlutterFps").floatValue;
            CopyIntArray(refSo.FindProperty("liftFrameIndices"), danceSo.FindProperty("liftFrameIndices"));
            CopyIntArray(refSo.FindProperty("lowerFromPeakIndices"), danceSo.FindProperty("lowerFromPeakIndices"));
            CopyIntArray(refSo.FindProperty("lowerFromMidIndices"), danceSo.FindProperty("lowerFromMidIndices"));
        }

        static Sprite[] LoadFrames(string folderName)
        {
            string basePath = $"Assets/Minigames/Flounder/Characters/{folderName}";
            var frames = new Sprite[FrameCount];
            for (var i = 0; i < FrameCount; i++)
            {
                string path = $"{basePath}/frame_{i:D3}.png";
                if (!File.Exists(path))
                {
                    Debug.LogError("[FlounderSlotCharacterSetup] 프레임 없음: " + path);
                    return System.Array.Empty<Sprite>();
                }

                ConfigureSpriteImporter(path);
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return frames;
        }

        static void ConfigureSpriteImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }

        static void SetIntArray(SerializedProperty prop, int[] values)
        {
            prop.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).intValue = values[i];
        }

        static void CopyIntArray(SerializedProperty from, SerializedProperty to)
        {
            to.arraySize = from.arraySize;
            for (var i = 0; i < from.arraySize; i++)
                to.GetArrayElementAtIndex(i).intValue = from.GetArrayElementAtIndex(i).intValue;
        }
    }
}
