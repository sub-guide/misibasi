using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MiniParty.EditorTools
{
    /// <summary>
    /// UAL1 Crouch_Fwd_Loop ↔ Walk_Formal_Loop 1D Blend AnimatorController 생성·갱신.
    /// 컨트롤러가 이미 있으면 에셋을 재사용해 GUID(프리팹·씬 참조)를 유지한다.
    /// </summary>
    public static class CoffinDancePallbearerAnimatorSetup
    {
        const string FbxPath = "Assets/CoffinDance/Animations/UAL1_Standard.fbx";
        const string ControllerPath = "Assets/CoffinDance/Animations/PallbearerPose.controller";

        /// <summary>Extension = 1 (기립 · 정중히 걷기).</summary>
        const string UprightClipName = "Walk_Formal_Loop";

        /// <summary>Extension = 0 (웅크린 채 전진).</summary>
        const string CrouchClipName = "Crouch_Fwd_Loop";

        const string StateName = "ExtensionBlend";
        const string BlendTreeName = "CrouchWalk";
        const string DirectBlendParam = "Blend";
        const string ExtensionParam = MiniParty.Minigames.CoffinDance.CoffinDancePallbearerPose.ExtensionParam;

        [MenuItem("Mini Party/Coffin Dance/Create Pallbearer Animator")]
        public static void CreatePallbearerAnimator()
        {
            AnimationClip upright = FindClipExact(FbxPath, UprightClipName);
            AnimationClip crouch = FindClipExact(FbxPath, CrouchClipName);

            if (upright == null || crouch == null)
            {
                EditorUtility.DisplayDialog(
                    "Pallbearer Animator",
                    BuildMissingClipMessage(upright, crouch),
                    "OK");
                return;
            }

            if (upright == crouch)
            {
                EditorUtility.DisplayDialog(
                    "Pallbearer Animator",
                    $"{UprightClipName}/{CrouchClipName}가 같은 클립으로 해석되었습니다. FBX 클립 이름을 확인하세요.",
                    "OK");
                return;
            }

            string dir = Path.GetDirectoryName(ControllerPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir))
                Directory.CreateDirectory(dir);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            bool created = controller == null;
            if (created)
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            EnsureExtensionParameter(controller);

            AnimatorStateMachine root = controller.layers[0].stateMachine;
            AnimatorState state = FindOrCreateState(root);

            if (state.motion is not BlendTree tree)
            {
                tree = new BlendTree();
                AssetDatabase.AddObjectToAsset(tree, controller);
                state.motion = tree;
            }

            tree.name = BlendTreeName;
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = ExtensionParam;
            tree.useAutomaticThresholds = false;
            tree.children = new[]
            {
                NewChild(crouch, 0f),
                NewChild(upright, 1f)
            };

            EditorUtility.SetDirty(tree);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = controller;
            EditorGUIUtility.PingObject(controller);
            Debug.Log(
                $"[CoffinDance] {(created ? "Created" : "Updated")} {ControllerPath}\n" +
                $"  @0 {crouch.name} (instanceID={crouch.GetInstanceID()})\n" +
                $"  @1 {upright.name} (instanceID={upright.GetInstanceID()})");
        }

        static ChildMotion NewChild(AnimationClip clip, float threshold)
        {
            return new ChildMotion
            {
                motion = clip,
                threshold = threshold,
                timeScale = 1f,
                cycleOffset = 0f,
                directBlendParameter = DirectBlendParam
            };
        }

        static void EnsureExtensionParameter(AnimatorController controller)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == ExtensionParam)
                    return;
            }

            // 기본값 1 = 기립 (0이면 웅크린 채로 고정된 것처럼 보임)
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = ExtensionParam,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });
        }

        static AnimatorState FindOrCreateState(AnimatorStateMachine root)
        {
            ChildAnimatorState[] states = root.states;
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == StateName)
                    return states[i].state;
            }

            AnimatorState state = root.AddState(StateName);
            root.defaultState = state;
            return state;
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

                // Blender/Mixamo export: "Armature|Walk_Formal_Loop"
                if (pipeSuffix == null &&
                    clip.name.EndsWith("|" + clipName, System.StringComparison.Ordinal))
                    pipeSuffix = clip;
            }

            return pipeSuffix;
        }

        static string BuildMissingClipMessage(AnimationClip upright, AnimationClip crouch)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"클립을 찾지 못했습니다.\nFBX: {FbxPath}");
            if (upright == null)
                sb.AppendLine($"필요(미발견): {UprightClipName}");
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
