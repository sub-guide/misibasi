using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public static class FlounderLayoutSnapshots
    {
        public struct RectTransformSnapshot
        {
            public Transform Parent;
            public int SiblingIndex;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 AnchoredPosition;
            public Vector2 Pivot;
            public Vector2 SizeDelta;
            public Vector3 LocalScale;
            public Quaternion LocalRotation;
        }

        public struct TransformSnapshot
        {
            public Transform Parent;
            public int SiblingIndex;
            public Vector3 LocalPosition;
            public Vector3 LocalScale;
            public Quaternion LocalRotation;
        }

        public static RectTransformSnapshot Capture(RectTransform rt)
        {
            return new RectTransformSnapshot
            {
                Parent = rt.parent,
                SiblingIndex = rt.GetSiblingIndex(),
                AnchorMin = rt.anchorMin,
                AnchorMax = rt.anchorMax,
                AnchoredPosition = rt.anchoredPosition,
                Pivot = rt.pivot,
                SizeDelta = rt.sizeDelta,
                LocalScale = rt.localScale,
                LocalRotation = rt.localRotation
            };
        }

        public static TransformSnapshot Capture(Transform t)
        {
            return new TransformSnapshot
            {
                Parent = t.parent,
                SiblingIndex = t.GetSiblingIndex(),
                LocalPosition = t.localPosition,
                LocalScale = t.localScale,
                LocalRotation = t.localRotation
            };
        }

        public static void Apply(RectTransform rt, RectTransformSnapshot snapshot)
        {
            if (rt == null)
                return;

            if (rt.parent != snapshot.Parent)
                rt.SetParent(snapshot.Parent, false);

            rt.SetSiblingIndex(snapshot.SiblingIndex);
            rt.anchorMin = snapshot.AnchorMin;
            rt.anchorMax = snapshot.AnchorMax;
            rt.pivot = snapshot.Pivot;
            rt.anchoredPosition = snapshot.AnchoredPosition;
            rt.sizeDelta = snapshot.SizeDelta;
            rt.localScale = snapshot.LocalScale;
            rt.localRotation = snapshot.LocalRotation;
        }

        public static void Apply(Transform t, TransformSnapshot snapshot)
        {
            if (t == null)
                return;

            if (t.parent != snapshot.Parent)
                t.SetParent(snapshot.Parent, false);

            t.SetSiblingIndex(snapshot.SiblingIndex);
            t.localPosition = snapshot.LocalPosition;
            t.localScale = snapshot.LocalScale;
            t.localRotation = snapshot.LocalRotation;
        }
    }
}
