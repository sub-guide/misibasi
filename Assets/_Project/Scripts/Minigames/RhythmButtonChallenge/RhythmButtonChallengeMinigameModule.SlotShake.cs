using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        [System.Serializable]
        struct SlotShakeSettings
        {
            [Tooltip("진폭(px). 0이면 해당 판정 진동 없음.")]
            public float Amplitude;

            [Tooltip("임펄스 지속(초).")]
            public float Duration;

            [Tooltip("Perlin 주파수(Hz).")]
            public float Frequency;
        }

        struct SlotShakeRuntime
        {
            public Vector2 RestAnchoredPosition;
            public bool RestCaptured;
            public float Remaining;
            public float Intensity;
            public float ActiveDuration;
            public float ActiveFrequency;
            public float PhaseX;
            public float PhaseY;
            public bool AppliedLastFrame;
        }

        [Header("슬롯 UI 진동 (판정별)")]
        [SerializeField] SlotShakeSettings slotShakeOnSuccess = new()
        {
            Amplitude = 6f,
            Duration = 0.2f,
            Frequency = 28f
        };

        [SerializeField] SlotShakeSettings slotShakeOnFail = new()
        {
            Amplitude = 14f,
            Duration = 0.35f,
            Frequency = 48f
        };

        [SerializeField] SlotShakeSettings slotShakeOnBonus = new()
        {
            Amplitude = 10f,
            Duration = 0.5f,
            Frequency = 20f
        };

        SlotShakeRuntime[] _slotShake;

        void ResetAllSlotShakes()
        {
            _slotShake = new SlotShakeRuntime[SlotCount];
            if (playerSlots == null)
                return;

            for (var i = 0; i < SlotCount; i++)
                StopSlotShake(i);
        }

        void StopSlotShake(int slotIndex)
        {
            if (_slotShake == null || slotIndex < 0 || slotIndex >= SlotCount)
                return;

            RestoreSlotShakeRest(slotIndex);
            _slotShake[slotIndex] = default;
        }

        void RestoreSlotShakeRest(int slotIndex)
        {
            if (_slotShake == null || playerSlots == null || slotIndex >= playerSlots.Length)
                return;

            ref SlotShakeRuntime rt = ref _slotShake[slotIndex];
            if (!rt.RestCaptured || !rt.AppliedLastFrame)
                return;

            RectTransform slot = playerSlots[slotIndex];
            if (slot != null)
                slot.anchoredPosition = rt.RestAnchoredPosition;

            rt.AppliedLastFrame = false;
        }

        void EnsureSlotShakeRestCaptured(int slotIndex)
        {
            if (_slotShake == null || playerSlots == null || slotIndex >= playerSlots.Length)
                return;

            ref SlotShakeRuntime rt = ref _slotShake[slotIndex];
            if (rt.RestCaptured)
                return;

            RectTransform slot = playerSlots[slotIndex];
            if (slot == null)
                return;

            rt.RestAnchoredPosition = slot.anchoredPosition;
            rt.RestCaptured = true;
            rt.PhaseX = Random.Range(0f, 1000f);
            rt.PhaseY = Random.Range(0f, 1000f);
        }

        void TriggerSlotShake(int slotIndex, SlotShakeSettings settings)
        {
            if (!_aliveMask[slotIndex] || settings.Amplitude <= 0f)
                return;

            EnsureSlotShakeRestCaptured(slotIndex);
            ref SlotShakeRuntime rt = ref _slotShake[slotIndex];
            rt.PhaseX = Random.Range(0f, 1000f);
            rt.PhaseY = Random.Range(0f, 1000f);

            float dur = Mathf.Max(0.01f, settings.Duration);
            rt.Remaining = dur;
            rt.ActiveDuration = dur;
            rt.Intensity = settings.Amplitude;
            rt.ActiveFrequency = Mathf.Max(1f, settings.Frequency);
        }

        void TriggerSlotShakeOnSuccess(int slotIndex) =>
            TriggerSlotShake(slotIndex, slotShakeOnSuccess);

        void TriggerSlotShakeOnFail(int slotIndex) =>
            TriggerSlotShake(slotIndex, slotShakeOnFail);

        void TriggerSlotShakeOnBonus(int slotIndex) =>
            TriggerSlotShake(slotIndex, slotShakeOnBonus);

        void TickSlotShakes()
        {
            if (_slotShake == null || playerSlots == null)
                return;

            float dt = Time.unscaledDeltaTime;
            float hitTimeBase = (float)Time.unscaledTimeAsDouble;

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                {
                    if (_slotShake[i].Remaining > 0f || _slotShake[i].AppliedLastFrame)
                        StopSlotShake(i);
                    continue;
                }

                EnsureSlotShakeRestCaptured(i);
                ref SlotShakeRuntime rt = ref _slotShake[i];

                if (rt.Remaining <= 0f)
                {
                    if (rt.AppliedLastFrame)
                        RestoreSlotShakeRest(i);
                    continue;
                }

                rt.Remaining -= dt;
                float dur = Mathf.Max(0.0001f, rt.ActiveDuration);
                float decay = Mathf.Clamp01(rt.Remaining / dur);
                float hitAmp = rt.Intensity * decay;
                float hitHz = rt.ActiveFrequency;
                float hitTime = (float)hitTimeBase * hitHz;

                RectTransform slot = playerSlots[i];
                if (slot == null)
                    continue;

                float ox = (Mathf.PerlinNoise(rt.PhaseX, hitTime) - 0.5f) * 2f * hitAmp;
                float oy = (Mathf.PerlinNoise(rt.PhaseY, hitTime + 4.1f) - 0.5f) * 2f * hitAmp;
                slot.anchoredPosition = rt.RestAnchoredPosition + new Vector2(ox, oy);
                rt.AppliedLastFrame = true;

                if (rt.Remaining <= 0f)
                    RestoreSlotShakeRest(i);
            }
        }
    }
}
