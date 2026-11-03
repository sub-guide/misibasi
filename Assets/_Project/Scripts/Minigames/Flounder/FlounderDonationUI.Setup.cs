using System;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        void EnsureRootCoversCanvas()
        {
            if (PreserveCenterDonationLayout)
                return;

            var root = transform as RectTransform;
            if (root == null)
                return;

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.pivot = new Vector2(0.5f, 0.5f);
        }

        void EnsureDonationUiDrawsOnTop()
        {
            transform.SetAsLastSibling();

            if (centerPopupRoot != null)
                centerPopupRoot.transform.SetAsLastSibling();
        }

        void ApplyCrispSpriteLayout()
        {
            ApplyCenterPanelLayout();
        }

        void EnsureGameplaySlotAnchors()
        {
            if (gameplaySlotAnchors != null && gameplaySlotAnchors.Length > 0)
            {
                var allSet = true;
                for (var i = 0; i < gameplaySlotAnchors.Length; i++)
                {
                    if (gameplaySlotAnchors[i] == null)
                    {
                        allSet = false;
                        break;
                    }
                }

                if (allSet)
                    return;
            }

            var found = FindObjectsOfType<FlounderPlayerSlotView>(true);
            Array.Sort(found, (a, b) => a.SlotIndex.CompareTo(b.SlotIndex));
            gameplaySlotAnchors = found;
        }

        FlounderPlayerSlotView FindGameplaySlot(int slotIndex)
        {
            EnsureGameplaySlotAnchors();
            if (gameplaySlotAnchors == null)
                return null;

            for (var i = 0; i < gameplaySlotAnchors.Length; i++)
            {
                if (gameplaySlotAnchors[i] != null && gameplaySlotAnchors[i].SlotIndex == slotIndex)
                    return gameplaySlotAnchors[i];
            }

            if (slotIndex >= 0 && slotIndex < gameplaySlotAnchors.Length)
                return gameplaySlotAnchors[slotIndex];

            return null;
        }
    }
}
