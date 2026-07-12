using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
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

            TickFeverSubPatternReplay(slotIndex);

            sr.FeverRemaining -= Time.deltaTime;
            if (sr.FeverRemaining > 0f)
                return;

            sr.FeverRemaining = 0f;
            EndFever(slotIndex);
        }

        /// <summary>패턴 12글자 완성 시 피버 진입. 피버 중 패턴은 자동 연속재생.</summary>
        void TryBeginFeverOnPatternComplete(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.FeverRemaining > 0f)
                return;

            int len = SubPatternLower.Length;
            if (len <= 0 || sr.SubPatternMatched < len)
                return;

            BeginFever(slotIndex);
        }

        void BeginFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.FeverRemaining = FeverDurationSeconds;
            sr.FeverSubPatternStepTimer = 0f;
            // 완성문에서 곧바로 1글자부터 루프 재생
            sr.SubPatternMatched = 0;
            ActivateAllDjTargets(slotIndex);
            BeginCrowdFever(slotIndex);
        }

        void EndFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.FeverRemaining = 0f;
            ResetSubPatternProgress(ref sr);
            EndCrowdFever(slotIndex);

            if (IsDevGodModeSlot(slotIndex))
            {
                ActivateAllDjTargets(slotIndex);
                return;
            }

            SeedDjActiveTargets(slotIndex);
        }
    }
}
