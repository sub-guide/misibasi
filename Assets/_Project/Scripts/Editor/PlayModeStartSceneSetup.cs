#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniParty.Editor
{
    /// <summary>
    /// 에디터에서 Play를 누를 때 현재 열린 씬과 관계없이 MainMenu에서 시작한다.
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
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
        }
    }
}
#endif
