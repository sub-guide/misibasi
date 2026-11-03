using UnityEditor;
using UnityEngine;

namespace MiniParty.EditorTools
{
    public static class FlounderEditorBatch
    {
        /// <summary>Unity batchmode: 후원 UI + 2P·3P 캐릭터를 한 번에 적용.</summary>
        public static void ApplyDonationAndSlotCharactersBatch()
        {
            var ok = false;
            try
            {
                FlounderDonationUiSetup.ApplyToFlounderSceneBatch();
                FlounderSlotCharacterSetup.ApplyToSceneInternal();
                ok = true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }

            if (Application.isBatchMode)
                EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
