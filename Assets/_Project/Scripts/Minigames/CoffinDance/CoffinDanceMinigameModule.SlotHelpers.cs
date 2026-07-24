using System;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        static void ForEachSlot(Action<int> action)
        {
            for (var i = 0; i < SlotCount; i++)
                action(i);
        }

        CoffinDanceSlotBindings GetBindings(int i)
        {
            if (slotBindings == null || i < 0 || i >= slotBindings.Length)
                return null;

            return slotBindings[i];
        }
    }
}
