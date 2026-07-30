using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MiniParty.EditorTools
{
    /// <summary>
    /// UAL1 Idle_Loop ↔ Crouch_Idle_Loop 1D Blend AnimatorController 생성.
    /// </summary>
    public static class CoffinDancePallbearerAnimatorSetup
    {
        const string FbxPath = "Assets/CoffinDance/Animations/UAL1_Standard.fbx";
        const string ControllerPath = "Assets/CoffinDance/Animations/PallbearerPose.controller";
        const string IdleClipName = "Idle_Loop";
        const string CrouchClipName = "Crouch_Idle_Loop";
        const string ExtensionParam = MiniParty.Minigames.CoffinDance.CoffinDancePallbearerPose.ExtensionParam;

        [MenuItem("Mini Party/Coffin Dance/Create Pallbearer Animator")]
        public static void CreatePallbearerAnimator()
        {
            AnimationClip idle = FindClipExact(FbxPath, IdleClipName);
            AnimationClip crouch = FindClipExact(FbxPath, CrouchClipName);

            if (idle == null || crouch == null)
            {
                EditorUtility.DisplayDialog(
                    "Pallbearer Animator",
                    BuildMissingClipMessage(idle, crouch),
                    "OK");
                return;
            }

            if (idle == crouch)
            {
                EditorUtility.DisplayDialog(
                    "Pallbearer Animator",
                    "Idle/Crouch가 같은 클립으로 해석되었습니다. FBX 클립 이름을 확인하세요.",
                    "OK");
                return;
            }

            string dir = Path.GetDirectoryName(ControllerPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir))
                Directory.CreateDirectory(dir);

            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            // 기본값 1 = 기립 (0이면 Crouch만 보여 미리보기가 앉은 채로 고정된 것처럼 보임)
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = ExtensionParam,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });

            AnimatorStateMachine root = controller.layers[0].stateMachine;
            AnimatorState state = root.AddState("ExtensionBlend");
            root.defaultState = state;

            var tree = new BlendTree
            {
                name = "IdleCrouch",
                blendType = BlendTreeType.Simple1D,
                blendParameter = ExtensionParam,
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(tree, controller);

            // 0=Crouch · 1=Idle
            tree.AddChild(crouch, 0f);
            tree.AddChild(idle, 1f);
            state.motion = tree;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = controller;
            EditorGUIUtility.PingObject(controller);
            Debug.Log(
                $"[CoffinDance] Created {ControllerPath}\n" +
                $"  @0 {crouch.name} (instanceID={crouch.GetInstanceID()})\n" +
                $"  @1 {idle.name} (instanceID={idle.GetInstanceID()})");
        }

        static AnimationClip FindClipExact(string assetPath, string clipName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets == null)
                return null;

            AnimationClip pipeSuffix = null;

            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not AnimationClip clip)
                    continue;

                if (clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                    continue;

                if (clip.name == clipName)
                    return clip;

                // Blender/Mixamo export: "Armature|Idle_Loop"
                if (pipeSuffix == null &&
                    clip.name.EndsWith("|" + clipName, System.StringComparison.Ordinal))
                    pipeSuffix = clip;
            }

            return pipeSuffix;
        }

        static string BuildMissingClipMessage(AnimationClip idle, AnimationClip crouch)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"클립을 찾지 못했습니다.\nFBX: {FbxPath}");
            if (idle == null)
                sb.AppendLine($"필요(미발견): {IdleClipName}");
            if (crouch == null)
                sb.AppendLine($"필요(미발견): {CrouchClipName}");

            sb.AppendLine("\nFBX 내 AnimationClip 목록:");
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
            if (assets != null)
            {
                for (var i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is AnimationClip clip &&
                        !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                        sb.AppendLine($"  - {clip.name}");
                }
            }

            return sb.ToString();
        }
    }
}
