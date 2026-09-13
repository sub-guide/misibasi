using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        void RefreshScoreLabel(int slotIndex)
        {
            bool show = slotIndex >= 0 &&
                        slotIndex < SlotCount &&
                        _participatedMask[slotIndex];

            GameObject slot = GetScoreHudSlot(slotIndex);
            if (slot != null)
                slot.SetActive(show);

            TMP_Text label = GetScoreLabel(slotIndex);
            if (label == null)
                return;

            if (!show)
                return;

            label.gameObject.SetActive(true);
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

        GameObject GetScoreHudSlot(int slotIndex)
        {
            if (scoreHudSlots == null || slotIndex < 0 || slotIndex >= scoreHudSlots.Length)
                return null;

            return scoreHudSlots[slotIndex];
        }
    }
}
