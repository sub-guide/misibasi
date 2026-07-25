#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniParty.Editor
{
    /// <summary>
    /// 에디터에서 Play를 누를 때 현재 열린 씬과 관계없이 MainMenu에서 시작한다.
    /// Play 직전 Hierarchy 선택을 비워, 씬 전환 시 GameObjectInspector /
    /// SerializedObjectNotCreatableException 콘솔 스팸을 줄인다.
    /// </summary>
    [InitializeOnLoad]
    static class PlayModeStartSceneSetup
    {
        const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        static PlayModeStartSceneSetup()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
            if (scene == null)
            {
                Debug.LogWarning($"[PlayModeStartSceneSetup] 씬을 찾을 수 없습니다: {MainMenuScenePath}");
            }
            else
            {
                EditorSceneManager.playModeStartScene = scene;
            }

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // ExitingEditMode = Play 버튼 직후, 씬이 MainMenu로 바뀌기 직전
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            Selection.objects = System.Array.Empty<Object>();
            Selection.activeObject = null;
            Selection.activeGameObject = null;
        }
    }
}
#endif
