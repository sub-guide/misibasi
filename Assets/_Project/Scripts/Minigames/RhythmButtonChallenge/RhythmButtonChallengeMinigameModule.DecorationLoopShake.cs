using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        struct DecorationLoopShakeTarget
        {
            public RectTransform Rect;
            public Vector2 RestAnchoredPosition;
            public float PhaseX;
            public float PhaseY;
        }

        [Header("Decoration 루프 진동")]
        [SerializeField] RhythmButtonChallengeDecorationLoopShakeBindings decorationLoopShake;

        [Tooltip("Decoration 자식 공통 Perlin 진폭(px). 0이면 끔.")]
        [SerializeField] float decorationLoopShakeAmplitude = 5f;

        [Tooltip("Decoration 자식 공통 Perlin 주파수(Hz).")]
        [SerializeField] float decorationLoopShakeFrequency = 12f;

        DecorationLoopShakeTarget[] _decorationLoopShakeTargets;

        void ResolveDecorationLoopShake()
        {
            _decorationLoopShakeTargets = null;
            if (decorationLoopShake == null || decorationLoopShake.LoopShakeTargets == null)
                return;

            RectTransform[] sources = decorationLoopShake.LoopShakeTargets;
            var list = new DecorationLoopShakeTarget[sources.Length];
            var count = 0;

            for (var i = 0; i < sources.Length; i++)
            {
                RectTransform rt = sources[i];
                if (rt == null)
                    continue;

                list[count++] = new DecorationLoopShakeTarget
                {
                    Rect = rt,
                    RestAnchoredPosition = rt.anchoredPosition,
                    PhaseX = Random.Range(0f, 1000f),
                    PhaseY = Random.Range(0f, 1000f)
                };
            }

            if (count == 0)
                return;

            if (count == list.Length)
            {
                _decorationLoopShakeTargets = list;
                return;
            }

            var trimmed = new DecorationLoopShakeTarget[count];
            for (var i = 0; i < count; i++)
                trimmed[i] = list[i];
            _decorationLoopShakeTargets = trimmed;
        }

        void RestoreDecorationLoopShake()
        {
            if (_decorationLoopShakeTargets == null)
                return;

            for (var i = 0; i < _decorationLoopShakeTargets.Length; i++)
            {
                RectTransform rt = _decorationLoopShakeTargets[i].Rect;
                if (rt != null)
                    rt.anchoredPosition = _decorationLoopShakeTargets[i].RestAnchoredPosition;
            }
        }

        void TickDecorationLoopShake()
        {
            if (_decorationLoopShakeTargets == null || decorationLoopShakeAmplitude <= 0f)
                return;

            float amp = decorationLoopShakeAmplitude;
            float hz = Mathf.Max(1f, decorationLoopShakeFrequency);
            float time = (float)Time.unscaledTimeAsDouble * hz;

            for (var i = 0; i < _decorationLoopShakeTargets.Length; i++)
            {
                ref DecorationLoopShakeTarget target = ref _decorationLoopShakeTargets[i];
                RectTransform rt = target.Rect;
                if (rt == null)
                    continue;

                float ox = (Mathf.PerlinNoise(target.PhaseX, time) - 0.5f) * 2f * amp;
                float oy = (Mathf.PerlinNoise(target.PhaseY, time + 2.7f) - 0.5f) * 2f * amp;
                rt.anchoredPosition = target.RestAnchoredPosition + new Vector2(ox, oy);
            }
        }
    }
}
