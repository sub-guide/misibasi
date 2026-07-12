using System.Collections.Generic;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const float UiShakeTier3IntensityMultiplier = 2f;

        [Header("슬롯 UI 흔들림 (2티어 기준)")]
        [Tooltip("2티어 정답 시 게이지·패턴·점수·가이드 버튼 anchoredPosition 흔들림 진폭(px). 3티어는 2배.")]
        [SerializeField] float uiShakeAmplitudeTier2 = 10f;

        [Tooltip("정답 1회당 흔들림 지속 시간(초).")]
        [SerializeField] float uiShakeDuration = 0.25f;

        [Tooltip("흔들림 주파수(Hz). 값이 클수록 더 빠르게 진동.")]
        [SerializeField] float uiShakeFrequency = 28f;

        struct UiShakeTarget
        {
            public RectTransform Rect;
            public Vector2 RestAnchoredPosition;
            public float PhaseX;
            public float PhaseY;
        }

        readonly UiShakeTarget[][] _uiShakeTargets = new UiShakeTarget[SlotCount][];
        readonly bool[] _uiShakeTargetsCaptured = new bool[SlotCount];
        readonly float[] _uiShakeRemaining = new float[SlotCount];
        readonly float[] _uiShakeIntensity = new float[SlotCount];

        void ResetAllSlotUiShake()
        {
            ForEachSlot(StopSlotUiShakeAndClearTargets);
        }

        void StopSlotUiShakeAndClearTargets(int i)
        {
            RestoreSlotUiShakeRestPositions(i);
            _uiShakeRemaining[i] = 0f;
            _uiShakeIntensity[i] = 0f;
            _uiShakeTargetsCaptured[i] = false;
            _uiShakeTargets[i] = null;
        }

        void StopSlotUiShake(int i)
        {
            RestoreSlotUiShakeRestPositions(i);
            _uiShakeRemaining[i] = 0f;
            _uiShakeIntensity[i] = 0f;
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
                PhaseX = Random.Range(0f, 1000f),
                PhaseY = Random.Range(0f, 1000f)
            });
        }

        void RerollSlotUiShakeTargetPhases(int i)
        {
            UiShakeTarget[] targets = _uiShakeTargets[i];
            if (targets == null)
                return;

            for (var t = 0; t < targets.Length; t++)
            {
                targets[t].PhaseX = Random.Range(0f, 1000f);
                targets[t].PhaseY = Random.Range(0f, 1000f);
            }
        }

        void TriggerSlotUiShakeOnCorrect(int i, ref SlotRuntime sr)
        {
            if (_ctx.IsPractice)
                return;

            int tier = ResolveGameplayTier(ref sr);
            if (tier < 2)
                return;

            CaptureSlotUiShakeTargets(i);
            RerollSlotUiShakeTargetPhases(i);

            float mul = tier >= 3 ? UiShakeTier3IntensityMultiplier : 1f;
            _uiShakeRemaining[i] = Mathf.Max(0.01f, uiShakeDuration);
            _uiShakeIntensity[i] = Mathf.Max(0f, uiShakeAmplitudeTier2) * mul;
        }

        void UpdateSlotUiShake(int i)
        {
            if (_ctx.IsPractice)
                return;

            ref SlotRuntime sr = ref _slots[i];

            if (_uiShakeRemaining[i] <= 0f)
                return;

            CaptureSlotUiShakeTargets(i);

            UiShakeTarget[] targets = _uiShakeTargets[i];
            if (targets == null || targets.Length == 0)
            {
                _uiShakeRemaining[i] = 0f;
                return;
            }

            _uiShakeRemaining[i] -= Time.deltaTime;

            float dur = Mathf.Max(0.0001f, uiShakeDuration);
            float decay = Mathf.Clamp01(_uiShakeRemaining[i] / dur);
            float amp = _uiShakeIntensity[i] * decay;
            float time = Time.unscaledTime * Mathf.Max(1f, uiShakeFrequency);

            for (var t = 0; t < targets.Length; t++)
            {
                RectTransform rt = targets[t].Rect;
                if (rt == null)
                    continue;

                ref UiShakeTarget target = ref targets[t];
                float offsetX = (Mathf.PerlinNoise(target.PhaseX, time) - 0.5f) * 2f * amp;
                float offsetY = (Mathf.PerlinNoise(target.PhaseY, time + 4.1f) - 0.5f) * 2f * amp;
                rt.anchoredPosition = target.RestAnchoredPosition + new Vector2(offsetX, offsetY);
            }

            if (_uiShakeRemaining[i] <= 0f)
                RestoreSlotUiShakeRestPositions(i);
        }
    }
}
