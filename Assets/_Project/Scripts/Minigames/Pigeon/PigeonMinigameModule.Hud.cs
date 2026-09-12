using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        void RefreshScoreLabel(int slotIndex)
        {
            TMP_Text label = GetScoreLabel(slotIndex);
            if (label == null)
                return;

            bool show = slotIndex >= 0 &&
                        slotIndex < SlotCount &&
                        _participatedMask[slotIndex];

            label.gameObject.SetActive(show);
            if (!show)
                return;

            label.text = $"{_score[slotIndex]}점";
        }

        void RefreshAllScoreLabels()
        {
            for (var i = 0; i < SlotCount; i++)
                RefreshScoreLabel(i);
        }

        TMP_Text GetScoreLabel(int slotIndex)
        {
            if (scoreLabels == null || slotIndex < 0 || slotIndex >= scoreLabels.Length)
                return null;

            return scoreLabels[slotIndex];
        }
    }
}
