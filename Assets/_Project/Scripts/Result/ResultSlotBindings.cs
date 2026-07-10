using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Result
{
    /// <summary>
    /// <c>ResultSlotPanel</c> 프리팹 루트. 자식 이름 규칙으로 UI 참조를 채운다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResultSlotBindings : MonoBehaviour
    {
        public TMP_Text PlayerText;
        public TMP_Text RankText;
        public TMP_Text ScoreText;
        public Transform HpDisplay;
        public Image Blur;
        public Image ReadyBorder;
        public Image Spotlight;
        public RectTransform ShakeRoot;
        public Transform GameOverAnchor;

        void OnValidate() => AutoWireFromHierarchy();

        public void AutoWireFromHierarchy()
        {
            Transform root = transform;

            PlayerText = GetDirectChildComponent<TMP_Text>(root, "PlayerText");
            RankText = GetDirectChildComponent<TMP_Text>(root, "RankText");
            ScoreText = GetDirectChildComponent<TMP_Text>(root, "ScoreText");
            HpDisplay = GetDirectChildTransform(root, "HpDisplay");
            Blur = GetDirectChildComponent<Image>(root, "Blur");
            ReadyBorder = GetDirectChildComponent<Image>(root, "ReadyBorder");
            Spotlight = GetDirectChildComponent<Image>(root, "Spotlight");
            ShakeRoot = GetDirectChildComponent<RectTransform>(root, "ShakeRoot");
            GameOverAnchor = GetDirectChildTransform(root, "GameOverAnchor");
        }

        static T GetDirectChildComponent<T>(Transform root, string childName) where T : Component
        {
            Transform child = root.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        static Transform GetDirectChildTransform(Transform root, string childName)
        {
            Transform child = root.Find(childName);
            return child;
        }
    }
}
