using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        readonly bool[] _baseFeverActive = new bool[SlotCount];

        bool IsFeverActive(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _baseFeverActive.Length)
                return false;

            return _baseFeverActive[slotIndex];
        }

        /// <summary>
        /// 참가 슬롯을 라운드 끝까지 전 키 정답·관중·Scream 상태로 둔다.
        /// 입력 판정보다 먼저 호출한다.
        /// </summary>
        void UpdateGlobalTierFeverMode()
        {
            if (_slots == null)
                return;

            for (var i = 0; i < SlotCount; i++)
            {
                if (_aliveMask[i])
                    BeginBaseFever(i);
            }
        }

        void BeginBaseFever(int slotIndex)
        {
            if (_baseFeverActive[slotIndex])
                return;

            _baseFeverActive[slotIndex] = true;
            ActivateAllDjTargets(slotIndex);
            BeginCrowdFever(slotIndex);
            RefreshFeverScreamAudio();
        }

        void EnterTier2()
        {
            if (_tier2Active)
                return;

            _tier2Active = true;
            UpdateGlobalTierFeverMode();
            ForEachSlot(ApplySlotChrome);
            ForEachSlot(i =>
            {
                if (_aliveMask[i])
                    UpdateCatAnimationMode(i);
            });
        }

        void ResetGlobalTierFeverMode()
        {
            for (var i = 0; i < _baseFeverActive.Length; i++)
                _baseFeverActive[i] = false;
        }
    }
}
