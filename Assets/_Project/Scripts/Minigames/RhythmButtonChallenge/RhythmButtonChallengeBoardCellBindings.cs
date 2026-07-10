using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// 8칸 보드의 각 Cell 루트. 자식 이름 규칙으로 UI 참조를 채운다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RhythmButtonChallengeBoardCellBindings : MonoBehaviour
    {
        public Image ButtonIcon;
        public Image ActiveHighlight;
        public Image Judgment1P;
        public Image Judgment2P;
        public Image Judgment3P;
        public Image Judgment4P;

        public RhythmButtonChallengeMinigameModule.BoardCellBindings ToBoardCellBindings() =>
            new()
            {
                ButtonIcon = ButtonIcon,
                ActiveHighlight = ActiveHighlight,
                Judgment1P = Judgment1P,
                Judgment2P = Judgment2P,
                Judgment3P = Judgment3P,
                Judgment4P = Judgment4P
            };

        void OnValidate() => AutoWireFromHierarchy();

        public void AutoWireFromHierarchy()
        {
            Transform root = transform;
            ButtonIcon = GetDirectChildComponent<Image>(root, "ButtonIcon");
            ActiveHighlight = GetDirectChildComponent<Image>(root, "ActiveHighlight");
            Judgment1P = GetDirectChildComponent<Image>(root, "Judgment1P");
            Judgment2P = GetDirectChildComponent<Image>(root, "Judgment2P");
            Judgment3P = GetDirectChildComponent<Image>(root, "Judgment3P");
            Judgment4P = GetDirectChildComponent<Image>(root, "Judgment4P");
        }

        static T GetDirectChildComponent<T>(Transform root, string childName) where T : Component
        {
            Transform child = root.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }
    }
}
