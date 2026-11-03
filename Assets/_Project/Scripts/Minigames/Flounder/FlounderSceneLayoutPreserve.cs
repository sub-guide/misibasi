using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniParty.Minigames.Flounder
{
    /// <summary>
    /// 씬에 배치한 UI·캐릭터 Transform을 스냅샷으로 저장하고, 런타임 자동 배치가 덮어쓰지 않게 합니다.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class FlounderSceneLayoutPreserve : MonoBehaviour
    {
        public static FlounderSceneLayoutPreserve Instance { get; private set; }

        public static bool ShouldPreserve => Instance != null && Instance.preserveSceneLayout;

        [SerializeField] bool preserveSceneLayout = true;
        [Tooltip("비어 있으면 이 오브젝트(Flounder) 하위 전체를 저장합니다.")]
        [SerializeField] Transform layoutRoot;
        [Tooltip("RectTransform이 아닌 Transform(캐릭터 GifVisual 등)도 local TRS를 저장합니다.")]
        [SerializeField] bool capturePlainTransforms = true;

        enum SnapshotKind
        {
            Rect,
            Transform
        }

        struct Entry
        {
            public SnapshotKind Kind;
            public FlounderLayoutSnapshots.RectTransformSnapshot Rect;
            public FlounderLayoutSnapshots.TransformSnapshot LocalTransform;
        }

        readonly Dictionary<int, Entry> _entries = new Dictionary<int, Entry>();

        void Awake()
        {
            Instance = this;
            if (preserveSceneLayout)
                CaptureAll();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
                return;

            EditorApplication.delayCall += DelayedEditorCapture;
        }

        void DelayedEditorCapture()
        {
            EditorApplication.delayCall -= DelayedEditorCapture;
            if (this == null || Application.isPlaying)
                return;

            CaptureAll();
        }
#endif

        public void CaptureAll()
        {
            _entries.Clear();
            Transform root = layoutRoot != null ? layoutRoot : transform;
            if (root == null)
                return;

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child == null)
                    continue;

                var rect = child as RectTransform;
                if (rect != null)
                {
                    _entries[child.GetInstanceID()] = new Entry
                    {
                        Kind = SnapshotKind.Rect,
                        Rect = FlounderLayoutSnapshots.Capture(rect)
                    };
                    continue;
                }

                if (capturePlainTransforms)
                {
                    _entries[child.GetInstanceID()] = new Entry
                    {
                        Kind = SnapshotKind.Transform,
                        LocalTransform = FlounderLayoutSnapshots.Capture(child)
                    };
                }
            }
        }

        public void Restore(Transform target)
        {
            if (!ShouldPreserve || target == null)
                return;

            if (!_entries.TryGetValue(target.GetInstanceID(), out Entry entry))
                return;

            if (entry.Kind == SnapshotKind.Rect && target is RectTransform rect)
                FlounderLayoutSnapshots.Apply(rect, entry.Rect);
            else if (entry.Kind == SnapshotKind.Transform)
                FlounderLayoutSnapshots.Apply(target, entry.LocalTransform);
        }

        public void RestoreHierarchy(Transform root)
        {
            if (!ShouldPreserve || root == null)
                return;

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                Restore(child);
        }
    }
}
