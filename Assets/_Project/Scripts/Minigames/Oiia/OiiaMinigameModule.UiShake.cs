using System.Collections.Generic;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [System.Serializable]
        struct UiShakeTierSettings
        {
            [Tooltip("정답 임펄스 진폭(px). 0이면 해당 티어 정답 흔들림 없음.")]
            public float Amplitude;

            [Tooltip("정답 1회당 임펄스 지속 시간(초).")]
            public float Duration;

            [Tooltip("정답 임펄스 주파수(Hz).")]
            public float Frequency;
        }

        [System.Serializable]
        struct UiShakeIdleTierSettings
        {
            [Tooltip("상시 진동 진폭(px). 0이면 해당 티어 상시 진동 없음. 정답 임펄스와 별개·합산.")]
            public float Amplitude;

            [Tooltip("상시 진동 주파수(Hz).")]
            public float Frequency;
        }

        [Header("슬롯 UI 흔들림 — 정답 임펄스")]
        [Tooltip("개인 티어 1. 0이면 정답 흔들림 없음.")]
        [SerializeField] UiShakeTierSettings uiShakeTier1 = new UiShakeTierSettings
        {
            Amplitude = 0f,
            Duration = 0f,
            Frequency = 0f
        };

        [Tooltip("개인 티어 2.")]
        [SerializeField] UiShakeTierSettings uiShakeTier2 = new UiShakeTierSettings
        {
            Amplitude = 8f,
            Duration = 1f,
            Frequency = 5f
        };

        [Tooltip("개인 티어 3. 씬 YAML 키는 uiShakeTier3.")]
        [SerializeField] UiShakeTierSettings uiShakeTier3 = new UiShakeTierSettings
        {
            Amplitude = 20f,
            Duration = 0.25f,
            Frequency = 64f
        };

        [Header("슬롯 UI 흔들림 — 상시 진동")]
        [Tooltip("개인 티어 1 상시 진동.")]
        [SerializeField] UiShakeIdleTierSettings uiShakeIdleTier1 = new UiShakeIdleTierSettings
        {
            Amplitude = 0f,
            Frequency = 0f
        };

        [Tooltip("개인 티어 2 상시 진동.")]
        [SerializeField] UiShakeIdleTierSettings uiShakeIdleTier2 = new UiShakeIdleTierSettings
        {
            Amplitude = 10f,
            Frequency = 1f
        };

        [Tooltip("개인 티어 3 상시 진동. 씬 YAML 키는 uiShakeIdleTier3.")]
        [SerializeField] UiShakeIdleTierSettings uiShakeIdleTier3 = new UiShakeIdleTierSettings
        {
            Amplitude = 8f,
            Frequency = 32f
        };

        struct UiShakeTarget
        {
            public RectTransform Rect;
            public Vector2 RestAnchoredPosition;
            public float HitPhaseX;
            public float HitPhaseY;
            public float IdlePhaseX;
            public float IdlePhaseY;
        }

        readonly UiShakeTarget[][] _uiShakeTargets = new UiShakeTarget[SlotCount][];
        readonly bool[] _uiShakeTargetsCaptured = new bool[SlotCount];
        readonly bool[] _uiShakeAppliedLastFrame = new bool[SlotCount];
        readonly float[] _uiShakeRemaining = new float[SlotCount];
        readonly float[] _uiShakeIntensity = new float[SlotCount];
        readonly float[] _uiShakeActiveDuration = new float[SlotCount];
        readonly float[] _uiShakeActiveFrequency = new float[SlotCount];

        UiShakeTierSettings GetUiShakeTierSettings(int slotIndex)
        {
            int tier = PlayerTier(slotIndex);
            if (tier >= PlayerTierMax)
                return uiShakeTier3;
            if (tier >= 2)
                return uiShakeTier2;
            return uiShakeTier1;
        }

        UiShakeIdleTierSettings GetUiShakeIdleTierSettings(int slotIndex)
        {
            int tier = PlayerTier(slotIndex);
            if (tier >= PlayerTierMax)
                return uiShakeIdleTier3;
            if (tier >= 2)
                return uiShakeIdleTier2;
            return uiShakeIdleTier1;
        }

        void ResetAllSlotUiShake()
        {
            ForEachSlot(StopSlotUiShakeAndClearTargets);
        }

        void StopSlotUiShakeAndClearTargets(int i)
        {
            RestoreSlotUiShakeRestPositions(i);
            _uiShakeRemaining[i] = 0f;
            _uiShakeIntensity[i] = 0f;
            _uiShakeActiveDuration[i] = 0f;
            _uiShakeActiveFrequency[i] = 0f;
            _uiShakeAppliedLastFrame[i] = false;
            _uiShakeTargetsCaptured[i] = false;
            _uiShakeTargets[i] = null;
        }

        void RestoreSlotUiShakeRestPositions(int i)
        {
            UiShakeTarget[] targets = _uiShakeTargets[i];
            if (targets == null)
                return;

            for (var t = 0; t < targets.Length; t++)
            {
                RectTransform rt = targets[t].Rect;
                if (rt == null)
                    continue;

                rt.anchoredPosition = targets[t].RestAnchoredPosition;
            }
        }

        void CaptureSlotUiShakeTargets(int i)
        {
            if (_uiShakeTargetsCaptured[i])
                return;

            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            var list = new List<UiShakeTarget>(16);
            AddDjBoxChildShakeTargets(list, b.DjBoxRoot);
            AddStageBackgroundShakeTargets(list, b.StageScreenRoot);

            _uiShakeTargets[i] = list.ToArray();
            _uiShakeTargetsCaptured[i] = true;
        }

        /// <summary>
        /// DjBox 자신, <c>Gauge*</c>, <c>Guage</c>, <c>Speaker*</c>, <c>Score</c>, <c>HudDisplay</c>, <c>SubPatternGuide</c>는 고정.
        /// 스피커 위치는 슬라이드가 맡고, 그 위에 흔들림을 더한다.
        /// </summary>
        static void AddDjBoxChildShakeTargets(List<UiShakeTarget> list, RectTransform djBox)
        {
            if (djBox == null)
                return;

            for (var c = 0; c < djBox.childCount; c++)
            {
                Transform child = djBox.GetChild(c);
                if (child == null ||
                    child.name == "Score" ||
                    child.name == "HudDisplay" ||
                    child.name == "SubPatternGuide" ||
                    child.name.StartsWith("Gauge") ||
                    child.name.StartsWith("Guage") ||
                    child.name.StartsWith("Speaker"))
                    continue;

                TryAddUiShakeTarget(list, child);
            }
        }

        static void AddStageBackgroundShakeTargets(List<UiShakeTarget> list, RectTransform stage)
        {
            if (stage == null)
                return;

            Transform mask = stage.Find("Mask");
            Transform bgParent = mask != null ? mask : stage;
            for (var c = 0; c < bgParent.childCount; c++)
            {
                Transform child = bgParent.GetChild(c);
                if (child == null || !child.name.StartsWith("Bg"))
                    continue;

                TryAddUiShakeTarget(list, child);
            }
        }

        static void TryAddUiShakeTarget(List<UiShakeTarget> list, Component component)
        {
            if (component == null)
                return;

            RectTransform rt = component.transform as RectTransform;
            if (rt == null)
                return;

            list.Add(new UiShakeTarget
            {
                Rect = rt,
                RestAnchoredPosition = rt.anchoredPosition,
                HitPhaseX = Random.Range(0f, 1000f),
                HitPhaseY = Random.Range(0f, 1000f),
                IdlePhaseX = Random.Range(0f, 1000f),
                IdlePhaseY = Random.Range(0f, 1000f)
            });
        }

        void RerollSlotUiShakeHitPhases(int i)
        {
            UiShakeTarget[] targets = _uiShakeTargets[i];
            if (targets == null)
                return;

            for (var t = 0; t < targets.Length; t++)
            {
                targets[t].HitPhaseX = Random.Range(0f, 1000f);
                targets[t].HitPhaseY = Random.Range(0f, 1000f);
            }
        }

        void TriggerSlotUiShakeOnCorrect(int i, ref SlotRuntime sr)
        {
            UiShakeTierSettings fx = GetUiShakeTierSettings(i);
            if (fx.Amplitude <= 0f)
                return;

            CaptureSlotUiShakeTargets(i);
            RerollSlotUiShakeHitPhases(i);

            float dur = Mathf.Max(0.01f, fx.Duration);
            _uiShakeRemaining[i] = dur;
            _uiShakeActiveDuration[i] = dur;
            _uiShakeIntensity[i] = fx.Amplitude;
            _uiShakeActiveFrequency[i] = Mathf.Max(1f, fx.Frequency);
        }

        void UpdateSlotUiShake(int i)
        {
            if (!_tier2Active)
                return;

            CaptureSlotUiShakeTargets(i);

            UiShakeTarget[] targets = _uiShakeTargets[i];
            if (targets == null || targets.Length == 0)
            {
                _uiShakeRemaining[i] = 0f;
                _uiShakeAppliedLastFrame[i] = false;
                return;
            }

            UiShakeIdleTierSettings idleFx = GetUiShakeIdleTierSettings(i);
            float idleAmp = _aliveMask[i] ? Mathf.Max(0f, idleFx.Amplitude) : 0f;
            float idleHz = Mathf.Max(1f, idleFx.Frequency);

            bool hasHit = _uiShakeRemaining[i] > 0f;
            float hitAmp = 0f;
            float hitHz = 1f;
            if (hasHit)
            {
                _uiShakeRemaining[i] -= Time.deltaTime;
                float dur = Mathf.Max(0.0001f, _uiShakeActiveDuration[i]);
                float decay = Mathf.Clamp01(_uiShakeRemaining[i] / dur);
                hitAmp = _uiShakeIntensity[i] * decay;
                hitHz = Mathf.Max(1f, _uiShakeActiveFrequency[i]);
                if (_uiShakeRemaining[i] <= 0f)
                    hasHit = false;
            }

            if (idleAmp <= 0f && hitAmp <= 0f)
            {
                if (_uiShakeAppliedLastFrame[i])
                {
                    RestoreSlotUiShakeRestPositions(i);
                    _uiShakeAppliedLastFrame[i] = false;
                }

                return;
            }

            float idleTime = Time.unscaledTime * idleHz;
            float hitTime = Time.unscaledTime * hitHz;

            for (var t = 0; t < targets.Length; t++)
            {
                RectTransform rt = targets[t].Rect;
                if (rt == null)
                    continue;

                ref UiShakeTarget target = ref targets[t];
                Vector2 offset = Vector2.zero;

                if (idleAmp > 0f)
                {
                    offset.x += (Mathf.PerlinNoise(target.IdlePhaseX, idleTime) - 0.5f) * 2f * idleAmp;
                    offset.y += (Mathf.PerlinNoise(target.IdlePhaseY, idleTime + 2.7f) - 0.5f) * 2f * idleAmp;
                }

                if (hitAmp > 0f)
                {
                    offset.x += (Mathf.PerlinNoise(target.HitPhaseX, hitTime) - 0.5f) * 2f * hitAmp;
                    offset.y += (Mathf.PerlinNoise(target.HitPhaseY, hitTime + 4.1f) - 0.5f) * 2f * hitAmp;
                }

                rt.anchoredPosition = target.RestAnchoredPosition + offset;
            }

            _uiShakeAppliedLastFrame[i] = true;
        }
    }
}
