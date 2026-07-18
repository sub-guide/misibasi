using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>피버 지속 시간(초). 전 버튼 정답.</summary>
        public const float FeverDurationSeconds = 3f;

        readonly bool[] _tier3ForcedFever = new bool[SlotCount];

        bool IsFeverActive(int slotIndex)
        {
            if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
                return false;

            return _tier3ForcedFever[slotIndex] ||
                   _slots[slotIndex].FeverRemaining > 0f;
        }

        /// <summary>피버 또는 Dev God — 전 키 Highlight·정답 모드.</summary>
        bool IsAllKeysCorrectMode(int slotIndex) =>
            IsDevGodModeSlot(slotIndex) || IsFeverActive(slotIndex);

        void TickFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];

            if (_tier3ForcedFever[slotIndex])
            {
                // T3 전체가 피버이므로 일반 3초 타이머가 소모되지 않게 항상 최대치 유지.
                sr.FeverRemaining = FeverDurationSeconds;
                return;
            }

            if (sr.FeverRemaining <= 0f)
                return;

            sr.FeverRemaining -= Time.deltaTime;
            if (sr.FeverRemaining > 0f)
                return;

            sr.FeverRemaining = 0f;
            EndFever(slotIndex);
        }

        /// <summary>패턴 12글자 완성 시 피버 진입.</summary>
        void TryBeginFeverOnPatternComplete(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.FeverRemaining > 0f)
                return;

            int len = SubPatternLower.Length;
            if (len <= 0 || sr.SubPatternMatched < len)
                return;

            // 패턴 완성 피버는 T1에서만. T2는 패턴만 반복하고 T3는 강제 피버.
            if (ResolveGlobalTier() != 1)
                return;

            BeginFever(slotIndex);
        }

        void BeginFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.FeverRemaining = FeverDurationSeconds;
            sr.SubPatternMatched = 0;
            ActivateAllDjTargets(slotIndex);
            BeginCrowdFever(slotIndex);
            RefreshFeverScreamAudio();
        }

        void EndFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];

            if (_tier3ForcedFever[slotIndex])
            {
                sr.FeverRemaining = FeverDurationSeconds;
                return;
            }

            sr.FeverRemaining = 0f;
            ResetSubPatternProgress(ref sr);
            EndCrowdFever(slotIndex);
            RefreshFeverScreamAudio();

            if (IsDevGodModeSlot(slotIndex))
            {
                ActivateAllDjTargets(slotIndex);
                return;
            }

            SeedDjActiveTargets(slotIndex);
        }

        /// <summary>
        /// T2에서는 T1에서 넘어온 일반 피버를 끝내고, T3 진입 시 모든 참가 슬롯을
        /// 라운드 종료까지 강제 피버로 전환한다. 입력 판정보다 먼저 호출해야 한다.
        /// </summary>
        void UpdateGlobalTierFeverMode()
        {
            if (_ctx.IsPractice || _slots == null)
                return;

            int tier = ResolveGlobalTier();
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                    continue;

                if (tier >= 3)
                {
                    BeginTier3ForcedFever(i);
                    continue;
                }

                if (tier == 2 && _slots[i].FeverRemaining > 0f)
                    EndFever(i);
            }
        }

        void BeginTier3ForcedFever(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];

            if (_tier3ForcedFever[slotIndex])
            {
                sr.FeverRemaining = FeverDurationSeconds;
                return;
            }

            bool wasFeverActive = IsFeverActive(slotIndex);

            _tier3ForcedFever[slotIndex] = true;
            sr.FeverRemaining = FeverDurationSeconds;
            ActivateAllDjTargets(slotIndex);

            if (!wasFeverActive)
            {
                sr.SubPatternMatched = 0;
                BeginCrowdFever(slotIndex);
            }

            RefreshFeverScreamAudio();
        }

        void ResetGlobalTierFeverMode()
        {
            for (var i = 0; i < _tier3ForcedFever.Length; i++)
                _tier3ForcedFever[i] = false;
        }
    }
}
