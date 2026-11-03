#if UNITY_EDITOR
using MiniParty.Minigames.Flounder;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniParty.EditorTools
{
    [InitializeOnLoad]
    static class FlounderDonationUIScenePreview
    {
        static FlounderDonationUIScenePreview()
        {
            EditorSceneManager.sceneOpened += (_, __) => ScheduleRefresh();
            EditorApplication.delayCall += RefreshOpenScenes;
        }

        static void ScheduleRefresh() => EditorApplication.delayCall += RefreshOpenScenes;

        static void RefreshOpenScenes()
        {
            if (Application.isPlaying)
                return;

            FlounderSceneLayoutPreserve layout = Object.FindObjectOfType<FlounderSceneLayoutPreserve>(true);
            layout?.CaptureAll();

            foreach (FlounderDonationUI ui in Object.FindObjectsOfType<FlounderDonationUI>(true))
                ui.RefreshStartDonationEditorPreview();
        }
    }
}
#endif
