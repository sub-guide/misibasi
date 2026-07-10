using System;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        static void ForEachSlot(Action<int> action)
        {
            for (var i = 0; i < SlotCount; i++)
                action(i);
        }

        bool TryGetBinding(int slotIndex, out SlotUiBindings binding)
        {
            binding = null;

            if (bindings == null || slotIndex < 0 || slotIndex >= bindings.Length)
                return false;

            binding = bindings[slotIndex];
            return binding != null;
        }

        char PatternLowerAt(int cursor)
        {
            cursor = UnityEngine.Mathf.Clamp(cursor, 0, _patternLower.Length - 1);
            return _patternLower[cursor];
        }

        static int ApplyScoreDeltaNonNegative(int current, int delta) =>
            UnityEngine.Mathf.Max(0, current + delta);

        static bool MaintainingGameplayGauge(ref SlotRuntime sr) =>
            sr.InputLockTimer <= 0f &&
            sr.Gauge01 > GaugeEmptyThreshold;
    }
}
