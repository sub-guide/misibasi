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

        [Header("슬롯 UI 흔들림 — 정답 임펄스 (티어별)")]
        [Tooltip("글로벌 T1 정답 시 HUD·DjBox·StageScreen 임펄스.")]
        [SerializeField] UiShakeTierSettings uiShakeTier1 = new UiShakeTierSettings
        {
            Amplitude = 0f,
            Duration = 0f,
            Frequency = 0f
        };

        [Tooltip("글로벌 T2 정답 시 HUD·DjBox·StageScreen 임펄스.")]
        [SerializeField] UiShakeTierSettings uiShakeTier2 = new UiShakeTierSettings
        {
            Amplitude = 8f,
            Duration = 1f,
            Frequency = 5f
        };

        [Tooltip("글로벌 T3 정답 시 HUD·DjBox·StageScreen 임펄스.")]
        [SerializeField] UiShakeTierSettings uiShakeTier3 = new UiShakeTierSettings
        {
            Amplitude = 20f,
            Duration = 0.25f,
            Frequency = 64f
        };

        [Header("슬롯 UI 흔들림 — 상시 진동 (티어별)")]
        [Tooltip("글로벌 T1 상시 진동. 정답 임펄스와 독립·합산.")]
        [SerializeField] UiShakeIdleTierSettings uiShakeIdleTier1 = new UiShakeIdleTierSettings
        {
            Amplitude = 0f,
            Frequency = 0f
        };

        [Tooltip("글로벌 T2 상시 진동.")]
        [SerializeField] UiShakeIdleTierSettings uiShakeIdleTier2 = new UiShakeIdleTierSettings
        {
            Amplitude = 10f,
            Frequency = 1f
        };

        [Tooltip("글로벌 T3 상시 진동.")]
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

        UiShakeTierSettings GetUiShakeTierSettings()
        {
            int tier = ResolveGlobalTier();
            if (tier >= 3)
                return uiShakeTier3;
            if (tier >= 2)
                return uiShakeTier2;
            return uiShakeTier1;
        }

        UiShakeIdleTierSettings GetUiShakeIdleTierSettings()
        {
            int tier = ResolveGlobalTier();
            if (tier >= 3)
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

        void StopSlotUiShake(int i)
        {
            RestoreSlotUiShakeRestPositions(i);
            _uiShakeRemaining[i] = 0f;
            _uiShakeIntensity[i] = 0f;
            _uiShakeActiveDuration[i] = 0f;
            _uiShakeActiveFrequency[i] = 0f;
            _uiShakeAppliedLastFrame[i] = false;
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

            var list = new List<UiShakeTarget>(8);
            TryAddUiShakeTarget(list, b.HudScoreText);
            TryAddUiShakeTarget(list, b.HudComboText);
            TryAddUiShakeTarget(list, b.HudFeverText);
            TryAddUiShakeTarget(list, b.FeverGaugeImage);
            TryAddUiShakeTarget(list, b.FeverGaugeImageB);
            TryAddUiShakeTarget(list, b.SubPatternGuideText);
            TryAddUiShakeTarget(list, b.DjBoxRoot);
            TryAddUiShakeTarget(list, b.StageScreenRoot);

            _uiShakeTargets[i] = list.ToArray();
            _uiShakeTargetsCaptured[i] = true;
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
            if (_ctx.IsPractice)
                return;

            UiShakeTierSettings fx = GetUiShakeTierSettings();
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
            if (_ctx.IsPractice)
                return;

            CaptureSlotUiShakeTargets(i);

            UiShakeTarget[] targets = _uiShakeTargets[i];
            if (targets == null || targets.Length == 0)
            {
                _uiShakeRemaining[i] = 0f;
                _uiShakeAppliedLastFrame[i] = false;
                return;
            }

            UiShakeIdleTierSettings idleFx = GetUiShakeIdleTierSettings();
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
