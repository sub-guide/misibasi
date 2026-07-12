using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>피버 진입에 필요한 콤보.</summary>
        public const int FeverComboThreshold = 30;

        /// <summary>피버 지속 시간(초). 전 버튼 정답.</summary>
        public const float FeverDurationSeconds = 3f;

        bool IsFeverActive(int slotIndex)
        {
            if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
                return false;

            return _slots[slotIndex].FeverRemaining > 0f;
        }

        /// <summary>피버 또는 Dev God — 전 키 Highlight·정답 모드.</summary>
        bool IsAllKeysCorrectMode(int slotIndex) =>
            IsDevGodModeSlot(slotIndex) || IsFeverActive(slotIndex);

        void TickFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.FeverRemaining <= 0f)
                return;

            sr.FeverRemaining -= Time.deltaTime;
            if (sr.FeverRemaining > 0f)
                return;

            sr.FeverRemaining = 0f;
            EndFever(slotIndex);
        }

        void TryBeginFeverOnCharge(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.FeverRemaining > 0f)
                return;

            if (sr.FeverCharge < FeverComboThreshold)
                return;

            BeginFever(slotIndex);
        }

        void BeginFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.FeverCharge = FeverComboThreshold;
            sr.FeverRemaining = FeverDurationSeconds;
            ActivateAllDjTargets(slotIndex);
        }

        void EndFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.FeverRemaining = 0f;
            sr.FeverCharge = 0;

            if (IsDevGodModeSlot(slotIndex))
            {
                ActivateAllDjTargets(slotIndex);
                return;
            }

            SeedDjActiveTargets(slotIndex);
        }
    }
}
