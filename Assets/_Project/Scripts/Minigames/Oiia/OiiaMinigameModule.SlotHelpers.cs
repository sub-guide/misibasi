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

        static int ApplyScoreDeltaNonNegative(int current, int delta) =>
            UnityEngine.Mathf.Max(0, current + delta);

        /// <summary>
        /// 레거시 게이지 유지 판정 대체. 1.5단계에서는 참가 중이면 유지로 간주(Cat/Blur/BGM 호환).
        /// 글로벌 티어 도입 시 교체.
        /// </summary>
        static bool MaintainingGameplaySustain(ref SlotRuntime sr) =>
            sr.InputLockTimer <= 0f;

        /// <summary>레거시 이름 호환. Cat/Blur/TierBgm이 호출.</summary>
        static bool MaintainingGameplayGauge(ref SlotRuntime sr) =>
            MaintainingGameplaySustain(ref sr);
    }
}
