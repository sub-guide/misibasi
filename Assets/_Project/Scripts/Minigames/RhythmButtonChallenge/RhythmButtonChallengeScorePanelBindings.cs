using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// 하단 4슬롯 점수 패널. 자식 이름 규칙으로 UI 참조를 채운다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RhythmButtonChallengeScorePanelBindings : MonoBehaviour
    {
        public TMP_Text ScoreText;
        public TMP_Text PlayerLabel;

        public RhythmButtonChallengeMinigameModule.ScorePanelBindings ToScorePanelBindings() =>
            new()
            {
                ScoreText = ScoreText,
                PlayerLabel = PlayerLabel
            };

        void OnValidate() => AutoWireFromHierarchy();

        public void AutoWireFromHierarchy()
        {
            Transform root = transform;
            ScoreText = GetDirectChildComponent<TMP_Text>(root, "Score");
            PlayerLabel = GetDirectChildComponent<TMP_Text>(root, "PlayerLabel");
        }

        static T GetDirectChildComponent<T>(Transform root, string childName) where T : Component
        {
            Transform child = root.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }
    }
}
