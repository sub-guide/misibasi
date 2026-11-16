using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const int PlayerTierMin = 1;
        const int PlayerTierMax = 3;

        [System.Serializable]
        struct PlayerTierTuning
        {
            [Tooltip("이 티어 게이지 최대. 입력이 이 값을 넘기면 넘친 만큼만 다음 티어로 넘어간다.")]
            [Min(1f)] public float GaugeMax;

            [Tooltip("이 티어에서 연타 입력 1회당 게이지 증가. 한 프레임에 여러 키여도 1회.")]
            [Min(0f)] public float GaugePerHit;

            [Tooltip("이 티어에서 연타 중 초당 감소. 빌드업과 Dev God 중에는 줄지 않는다.")]
            [Min(0f)] public float GaugeDrainPerSecond;

            [Tooltip("이 티어에서 연타 입력 1회 점수. 그 입력으로 티어가 올라도 누르기 전 티어 점수를 준다.")]
            [Min(0)] public int HitScore;
        }

        [Header("플레이어 티어")]
        [SerializeField] PlayerTierTuning playerTier1 = new PlayerTierTuning
        {
            GaugeMax = 100f,
            GaugePerHit = 8f,
            GaugeDrainPerSecond = 20f,
            HitScore = 100
        };

        [SerializeField] PlayerTierTuning playerTier2 = new PlayerTierTuning
        {
            GaugeMax = 100f,
            GaugePerHit = 8f,
            GaugeDrainPerSecond = 20f,
            HitScore = 100
        };

        [SerializeField] PlayerTierTuning playerTier3 = new PlayerTierTuning
        {
            GaugeMax = 100f,
            GaugePerHit = 8f,
            GaugeDrainPerSecond = 20f,
            HitScore = 100
        };

        int PlayerTier(int slotIndex)
        {
            if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
                return PlayerTierMin;

            int tier = _slots[slotIndex].Tier;
            if (tier < PlayerTierMin)
                return PlayerTierMin;

            if (tier > PlayerTierMax)
                return PlayerTierMax;

            return tier;
        }

        /// <summary>티어 3. Scream·빔·관중·SpinLoop.</summary>
        bool IsFeverActive(int slotIndex) =>
            _tier2Active &&
            slotIndex >= 0 &&
            slotIndex < SlotCount &&
            _aliveMask[slotIndex] &&
            PlayerTier(slotIndex) >= PlayerTierMax;

        PlayerTierTuning TuningForTier(int tier)
        {
            if (tier >= PlayerTierMax)
                return playerTier3;
            if (tier >= 2)
                return playerTier2;
            return playerTier1;
        }

        float GaugeMaxForTier(int tier) => Mathf.Max(1f, TuningForTier(tier).GaugeMax);

        int HitScoreForTier(int tier) => Mathf.Max(0, TuningForTier(tier).HitScore);

        void ResetPlayerTier(ref SlotRuntime sr)
        {
            sr.Tier = PlayerTierMin;
            sr.Gauge = 0f;
        }

        /// <summary>
        /// 연타 중에만 줄어든다. 0이면 한 단계 내리고 게이지를 그 티어의 최대로 둔다.
        /// 그 최대치는 승급이 아니다. 버튼을 더 눌러 최대를 넘겨야 다시 올라간다.
        /// </summary>
        void TickPlayerTierGauge(int slotIndex)
        {
            if (!_tier2Active || !_aliveMask[slotIndex] || IsDevGodModeActive())
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            int previous = PlayerTier(slotIndex);
            sr.Tier = previous;
            float drain = Mathf.Max(0f, TuningForTier(previous).GaugeDrainPerSecond);
            sr.Gauge -= drain * Time.deltaTime;

            if (sr.Gauge > 0f)
                return;

            if (sr.Tier <= PlayerTierMin)
            {
                sr.Gauge = 0f;
                return;
            }

            sr.Tier--;
            sr.Gauge = GaugeMaxForTier(sr.Tier);
            ApplyPlayerTierTransition(slotIndex, previous);
        }

        /// <summary>입력 1회. 최대를 넘긴 양만 다음 티어 게이지로 이월한다. 3단계에서는 최대에 고정.</summary>
        void AddPlayerTierGaugeOnHit(int slotIndex)
        {
            if (!_tier2Active || !_aliveMask[slotIndex])
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            int previous = PlayerTier(slotIndex);
            sr.Tier = previous;

            sr.Gauge += Mathf.Max(0f, TuningForTier(previous).GaugePerHit);

            while (sr.Tier < PlayerTierMax && sr.Gauge > GaugeMaxForTier(sr.Tier))
            {
                sr.Gauge -= GaugeMaxForTier(sr.Tier);
                sr.Tier++;
            }

            if (sr.Tier >= PlayerTierMax && sr.Gauge > GaugeMaxForTier(sr.Tier))
                sr.Gauge = GaugeMaxForTier(sr.Tier);

            if (sr.Tier != previous)
                ApplyPlayerTierTransition(slotIndex, previous);
        }

        void ApplyPlayerTierTransition(int slotIndex, int previousTier)
        {
            int tier = PlayerTier(slotIndex);
            if (tier == previousTier)
                return;

            if (previousTier < PlayerTierMax && tier >= PlayerTierMax)
                BeginCrowdFever(slotIndex);
            else if (previousTier >= PlayerTierMax && tier < PlayerTierMax)
                HideCrowdImmediate(slotIndex);

            RefreshFeverScreamAudio();
            UpdateCatAnimationMode(slotIndex);
        }

        void EnterTier2()
        {
            if (_tier2Active)
                return;

            _tier2Active = true;

            for (var i = 0; i < SlotCount; i++)
            {
                if (_aliveMask[i])
                    ActivateAllDjTargets(i);
            }

            ForEachSlot(ApplySlotChrome);
            ForEachSlot(i =>
            {
                if (_aliveMask[i])
                    UpdateCatAnimationMode(i);
            });
        }
    }
}
