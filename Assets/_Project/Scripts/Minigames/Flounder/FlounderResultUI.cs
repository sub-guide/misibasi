using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Flounder
{
    public sealed class FlounderResultUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI bodyText;
        [SerializeField] Button restartButton;

        public Button RestartButton => restartButton;

        public void Show(IReadOnlyList<FlounderPlayer> players)
        {
            if (root != null)
                root.SetActive(true);

            if (bodyText == null || players == null)
                return;

            var ranked = new List<FlounderPlayer>();
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i].Active)
                    ranked.Add(players[i]);
            }

            ranked.Sort((a, b) => b.FinalScore.CompareTo(a.FinalScore));

            var sb = new StringBuilder();
            sb.AppendLine("결과");
            for (var r = 0; r < ranked.Count; r++)
            {
                FlounderPlayer p = ranked[r];
                sb.AppendLine($"{r + 1}위  {(p.SlotIndex + 1)}P  {p.FinalScore:N0}점");
                sb.AppendLine($"   연타 {p.MashCount} · 후원 보너스 {p.DonationBonusScore:N0}");
            }

            bodyText.text = sb.ToString();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
