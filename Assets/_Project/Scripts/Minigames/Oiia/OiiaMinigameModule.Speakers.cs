using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const int SpeakerWoofersPerSlot = 4;
        const int SpeakerLetterPoolPerSlot = 24;

        [System.Serializable]
        struct SpeakerTierFxSettings
        {
            [Tooltip("우퍼 스케일 펀치 배율.")]
            public float WooferPulseScale;

            [Tooltip("우퍼 펄스 지속(초).")]
            public float WooferPulseSeconds;

            [Tooltip("분출 글자 표시 시간(초).")]
            public float LetterDuration;

            [Tooltip("분출 글자 fontSize.")]
            public float LetterFontSize;

            [Tooltip("분출 글자 비행 거리(px).")]
            public float LetterFlyDistance;

            [Tooltip("분출 글자 진동 진폭(px).")]
            public float LetterShakeAmplitude;

            [Tooltip("분출 글자 진동 주파수(Hz).")]
            public float LetterShakeFrequency;

            [Tooltip("팝 스케일 시작(작음).")]
            public float LetterScaleStart;

            [Tooltip("팝 스케일 정점.")]
            public float LetterScalePeak;
        }

        [Header("스피커 연출 — 공통")]
        [Tooltip("분출 글자 스폰 랜덤 오프셋(px).")]
        [SerializeField] float speakerLetterSpawnJitter = 18f;

        [Tooltip("스피커 분출 TMP 템플릿. Font/Material/Outline/Color 등 Inspector에서 꾸민 뒤 연결. 비우면 런타임 기본 생성.")]
        [SerializeField] TextMeshProUGUI speakerLetterTemplate;

        [Tooltip("켜면 티어 LetterFontSize로 템플릿 fontSize를 덮어씀. 끄면 템플릿 크기 유지.")]
        [SerializeField] bool speakerLetterOverrideFontSizeFromTier = true;

        [Header("스피커 연출 — 티어별 (Inspector)")]
        [Tooltip("글로벌 T1 수치.")]
        [SerializeField] SpeakerTierFxSettings speakerTier1 = new SpeakerTierFxSettings
        {
            WooferPulseScale = 1.2f,
            WooferPulseSeconds = 0.12f,
            LetterDuration = 0.45f,
            LetterFontSize = 64f,
            LetterFlyDistance = 70f,
            LetterShakeAmplitude = 4f,
            LetterShakeFrequency = 22f,
            LetterScaleStart = 0.35f,
            LetterScalePeak = 1.2f
        };

        [Tooltip("글로벌 T2 수치.")]
        [SerializeField] SpeakerTierFxSettings speakerTier2 = new SpeakerTierFxSettings
        {
            WooferPulseScale = 1.35f,
            WooferPulseSeconds = 0.12f,
            LetterDuration = 0.45f,
            LetterFontSize = 80f,
            LetterFlyDistance = 95f,
            LetterShakeAmplitude = 8f,
            LetterShakeFrequency = 28f,
            LetterScaleStart = 0.35f,
            LetterScalePeak = 1.4f
        };

        [Tooltip("글로벌 T3 수치.")]
        [SerializeField] SpeakerTierFxSettings speakerTier3 = new SpeakerTierFxSettings
        {
            WooferPulseScale = 1.55f,
            WooferPulseSeconds = 0.14f,
            LetterDuration = 0.5f,
            LetterFontSize = 100f,
            LetterFlyDistance = 120f,
            LetterShakeAmplitude = 14f,
            LetterShakeFrequency = 34f,
            LetterScaleStart = 0.3f,
            LetterScalePeak = 1.65f
        };

        struct WooferPulseState
        {
            public RectTransform Rect;
            public Vector3 RestScale;
            public float Remaining;
            public float PeakMul;
            public float PulseDuration;
        }

        struct SpeakerLetterFx
        {
            public TMP_Text Tmp;
            public RectTransform Rect;
            public float Remaining;
            public float Duration;
            public Vector2 Velocity;
            public Vector2 StartAnchored;
            public float ShakePhaseX;
            public float ShakePhaseY;
            public float ShakeAmplitude;
            public float ShakeFrequency;
            public float ScaleStart;
            public float ScalePeak;
            public Color BaseColor;
            public bool Active;
        }

        readonly WooferPulseState[][] _speakerWoofers = new WooferPulseState[SlotCount][];
        readonly SpeakerLetterFx[][] _speakerLetters = new SpeakerLetterFx[SlotCount][];
        readonly Transform[] _speakerLetterParents = new Transform[SlotCount];

        SpeakerTierFxSettings GetSpeakerTierFx()
        {
            int tier = ResolveGlobalTier();
            if (tier >= 3)
                return speakerTier3;
            if (tier >= 2)
                return speakerTier2;
            return speakerTier1;
        }

        void ResetSpeakersAtBegin(int i)
        {
            if (_speakerWoofers[i] != null)
                RestoreSpeakerWooferScales(i);

            CaptureSpeakerWoofers(i);
            DestroySpeakerLetterPool(i);
            EnsureSpeakerLetterPool(i);
            HideAllSpeakerLetters(i);
        }

        void DestroySpeakerLetterPool(int i)
        {
            if (_speakerLetterParents[i] != null)
            {
                Destroy(_speakerLetterParents[i].gameObject);
                _speakerLetterParents[i] = null;
            }

            _speakerLetters[i] = null;
        }

        void CaptureSpeakerWoofers(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
            {
                _speakerWoofers[i] = null;
                return;
            }

            var list = new List<WooferPulseState>(SpeakerWoofersPerSlot);
            TryAddWoofer(list, b.SpeakerLWooferTop);
            TryAddWoofer(list, b.SpeakerLWooferBottom);
            TryAddWoofer(list, b.SpeakerRWooferTop);
            TryAddWoofer(list, b.SpeakerRWooferBottom);
            _speakerWoofers[i] = list.ToArray();
        }

        static void TryAddWoofer(List<WooferPulseState> list, RectTransform rt)
        {
            if (rt == null)
                return;

            list.Add(new WooferPulseState
            {
                Rect = rt,
                RestScale = rt.localScale,
                Remaining = 0f,
                PeakMul = 1f,
                PulseDuration = 0.12f
            });
        }

        void RestoreSpeakerWooferScales(int i)
        {
            WooferPulseState[] woofers = _speakerWoofers[i];
            if (woofers == null)
                return;

            for (var w = 0; w < woofers.Length; w++)
            {
                if (woofers[w].Rect == null)
                    continue;

                woofers[w].Rect.localScale = woofers[w].RestScale;
                woofers[w].Remaining = 0f;
                woofers[w].PeakMul = 1f;
            }
        }

        void EnsureSpeakerLetterPool(int i)
        {
            if (_speakerLetters[i] != null)
                return;

            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            Transform parent = b.DjBoxRoot != null ? b.DjBoxRoot : transform;
            var layerGo = new GameObject("SpeakerLetterLayer", typeof(RectTransform));
            Transform layer = layerGo.transform;
            layer.SetParent(parent, false);
            var layerRt = layer as RectTransform;
            if (layerRt != null)
            {
                layerRt.anchorMin = Vector2.zero;
                layerRt.anchorMax = Vector2.one;
                layerRt.offsetMin = Vector2.zero;
                layerRt.offsetMax = Vector2.zero;
                layerRt.localScale = Vector3.one;
            }

            _speakerLetterParents[i] = layer;

            var pool = new SpeakerLetterFx[SpeakerLetterPoolPerSlot];
            for (var p = 0; p < pool.Length; p++)
                pool[p] = CreateSpeakerLetterEntry(layer, p, b);

            _speakerLetters[i] = pool;

            if (speakerLetterTemplate != null)
                speakerLetterTemplate.gameObject.SetActive(false);
        }

        SpeakerLetterFx CreateSpeakerLetterEntry(Transform layer, int index, SlotUiBindings b)
        {
            TextMeshProUGUI tmp;
            RectTransform rt;

            if (speakerLetterTemplate != null)
            {
                GameObject go = Instantiate(speakerLetterTemplate.gameObject, layer, false);
                go.name = $"SpeakerLetter{index}";
                go.SetActive(false);
                tmp = go.GetComponent<TextMeshProUGUI>();
                rt = go.transform as RectTransform;
                if (tmp == null)
                    tmp = go.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                var go = new GameObject($"SpeakerLetter{index}", typeof(RectTransform));
                go.transform.SetParent(layer, false);
                rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(80f, 80f);
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;

                tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
                tmp.fontSize = Mathf.Max(8f, speakerTier1.LetterFontSize);
                tmp.color = Color.white;
                tmp.raycastTarget = false;
                tmp.richText = false;

                if (b.SubPatternGuideText != null && b.SubPatternGuideText.font != null)
                    tmp.font = b.SubPatternGuideText.font;

                try
                {
                    tmp.outlineWidth = 0.28f;
                    tmp.outlineColor = Color.black;
                }
                catch
                {
                    // Font/Material 없으면 아웃라인 스킵
                }

                go.SetActive(false);
            }

            if (rt != null)
                rt.localRotation = Quaternion.identity;

            tmp.raycastTarget = false;

            return new SpeakerLetterFx
            {
                Tmp = tmp,
                Rect = rt,
                BaseColor = tmp.color,
                Active = false
            };
        }

        void HideAllSpeakerLetters(int i)
        {
            SpeakerLetterFx[] pool = _speakerLetters[i];
            if (pool == null)
                return;

            for (var p = 0; p < pool.Length; p++)
            {
                if (pool[p].Tmp != null)
                    pool[p].Tmp.gameObject.SetActive(false);

                pool[p].Active = false;
                pool[p].Remaining = 0f;
            }
        }

        /// <summary>패턴 한 글자 진행 시 — 전 우퍼 펄스 + O/I/A 만화 분출.</summary>
        void NotifySubPatternStep(int slotIndex, char letterUpper)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
                return;

            if (_ctx.IsPractice)
                return;

            if (_speakerWoofers[slotIndex] == null)
                CaptureSpeakerWoofers(slotIndex);

            EnsureSpeakerLetterPool(slotIndex);
            PulseAllSpeakerWoofers(slotIndex);
            SpawnSpeakerLettersOnAllWoofers(slotIndex, letterUpper);
        }

        void NotifySubPatternStepFromMatched(int slotIndex, ref SlotRuntime sr)
        {
            int matched = sr.SubPatternMatched;
            int len = SubPatternLower.Length;
            if (matched <= 0 || len <= 0 || matched > len)
                return;

            char c = char.ToUpperInvariant(SubPatternLower[matched - 1]);
            NotifySubPatternStep(slotIndex, c);
        }

        void PulseAllSpeakerWoofers(int slotIndex)
        {
            WooferPulseState[] woofers = _speakerWoofers[slotIndex];
            if (woofers == null)
                return;

            SpeakerTierFxSettings fx = GetSpeakerTierFx();
            float dur = Mathf.Max(0.02f, fx.WooferPulseSeconds);
            float peak = Mathf.Max(1.01f, fx.WooferPulseScale);

            for (var w = 0; w < woofers.Length; w++)
            {
                if (woofers[w].Rect == null)
                    continue;

                woofers[w].Remaining = dur;
                woofers[w].PulseDuration = dur;
                woofers[w].PeakMul = peak;
                woofers[w].Rect.localScale = woofers[w].RestScale * peak;
            }
        }

        void SpawnSpeakerLettersOnAllWoofers(int slotIndex, char letterUpper)
        {
            WooferPulseState[] woofers = _speakerWoofers[slotIndex];
            SpeakerLetterFx[] pool = _speakerLetters[slotIndex];
            if (woofers == null || pool == null)
                return;

            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.DjBoxRoot == null)
                return;

            SpeakerTierFxSettings tierFx = GetSpeakerTierFx();

            char lower = char.ToLowerInvariant(letterUpper);
            char upper = char.ToUpperInvariant(letterUpper);
            float dur = Mathf.Max(0.05f, tierFx.LetterDuration);
            float fly = Mathf.Max(10f, tierFx.LetterFlyDistance);
            float jitter = Mathf.Max(0f, speakerLetterSpawnJitter);
            float fontSize = Mathf.Max(8f, tierFx.LetterFontSize);
            float shakeAmp = Mathf.Max(0f, tierFx.LetterShakeAmplitude);
            float shakeHz = Mathf.Max(1f, tierFx.LetterShakeFrequency);
            float scaleStart = Mathf.Max(0.05f, tierFx.LetterScaleStart);
            float scalePeak = Mathf.Max(scaleStart + 0.05f, tierFx.LetterScalePeak);

            for (var w = 0; w < woofers.Length; w++)
            {
                RectTransform woofer = woofers[w].Rect;
                if (woofer == null)
                    continue;

                int idx = FindInactiveSpeakerLetter(pool);
                if (idx < 0)
                    idx = 0;

                ref SpeakerLetterFx fx = ref pool[idx];
                if (fx.Tmp == null || fx.Rect == null)
                    continue;

                Vector2 localInDj = WorldToDjBoxAnchored(b.DjBoxRoot, woofer);
                localInDj += new Vector2(
                    Random.Range(-jitter, jitter),
                    Random.Range(-jitter, jitter));

                // 안쪽·위: L은 오른쪽 위, R은 왼쪽 위
                float side = localInDj.x >= 0f ? -1f : 1f;
                Vector2 dir = new Vector2(
                    side * Random.Range(0.35f, 1f),
                    Random.Range(0.55f, 1.15f)).normalized;

                fx.StartAnchored = localInDj;
                fx.Rect.SetParent(_speakerLetterParents[slotIndex], false);
                fx.Rect.anchoredPosition = localInDj;
                fx.Rect.localRotation = Quaternion.identity;
                fx.Rect.localScale = Vector3.one * scaleStart;
                fx.Velocity = dir * fly / dur;
                fx.ShakePhaseX = Random.Range(0f, 1000f);
                fx.ShakePhaseY = Random.Range(0f, 1000f);
                fx.ShakeAmplitude = shakeAmp;
                fx.ShakeFrequency = shakeHz;
                fx.ScaleStart = scaleStart;
                fx.ScalePeak = scalePeak;
                fx.Duration = dur;
                fx.Remaining = dur;
                fx.Active = true;

                char shown = Random.value < 0.5f ? upper : lower;
                fx.Tmp.text = shown.ToString();
                if (speakerLetterOverrideFontSizeFromTier)
                    fx.Tmp.fontSize = fontSize;

                Color c = fx.BaseColor;
                c.a = fx.BaseColor.a;
                fx.Tmp.color = c;
                fx.Tmp.gameObject.SetActive(true);
            }
        }

        static int FindInactiveSpeakerLetter(SpeakerLetterFx[] pool)
        {
            for (var p = 0; p < pool.Length; p++)
            {
                if (!pool[p].Active)
                    return p;
            }

            return -1;
        }

        static Vector2 WorldToDjBoxAnchored(RectTransform djBox, RectTransform worldRt)
        {
            Vector3 world = worldRt.TransformPoint(worldRt.rect.center);
            Vector3 local = djBox.InverseTransformPoint(world);
            return new Vector2(local.x, local.y);
        }

        void TickSpeakers(int i)
        {
            TickSpeakerWooferPulses(i);
            TickSpeakerLetters(i);
        }

        void TickSpeakerWooferPulses(int i)
        {
            WooferPulseState[] woofers = _speakerWoofers[i];
            if (woofers == null)
                return;

            for (var w = 0; w < woofers.Length; w++)
            {
                if (woofers[w].Remaining <= 0f || woofers[w].Rect == null)
                    continue;

                float dur = Mathf.Max(0.02f, woofers[w].PulseDuration);
                woofers[w].Remaining -= Time.deltaTime;
                float t = Mathf.Clamp01(woofers[w].Remaining / dur);
                float peak = Mathf.Max(1.01f, woofers[w].PeakMul);
                float mul = Mathf.Lerp(1f, peak, t);
                woofers[w].Rect.localScale = woofers[w].RestScale * mul;

                if (woofers[w].Remaining <= 0f)
                    woofers[w].Rect.localScale = woofers[w].RestScale;
            }
        }

        void TickSpeakerLetters(int i)
        {
            SpeakerLetterFx[] pool = _speakerLetters[i];
            if (pool == null)
                return;

            for (var p = 0; p < pool.Length; p++)
            {
                if (!pool[p].Active || pool[p].Rect == null || pool[p].Tmp == null)
                    continue;

                float dur = Mathf.Max(0.05f, pool[p].Duration);
                pool[p].Remaining -= Time.deltaTime;
                float life = Mathf.Clamp01(pool[p].Remaining / dur);
                float u = 1f - life;

                Vector2 basePos = pool[p].StartAnchored + pool[p].Velocity * (u * dur);

                float shake = pool[p].ShakeAmplitude * life;
                float time = Time.unscaledTime * Mathf.Max(1f, pool[p].ShakeFrequency);
                float ox = (Mathf.PerlinNoise(pool[p].ShakePhaseX, time) - 0.5f) * 2f * shake;
                float oy = (Mathf.PerlinNoise(pool[p].ShakePhaseY, time + 3.7f) - 0.5f) * 2f * shake;
                pool[p].Rect.anchoredPosition = basePos + new Vector2(ox, oy);
                pool[p].Rect.localRotation = Quaternion.identity;

                float pop = Mathf.Sin(Mathf.PI * u);
                float scale = Mathf.Lerp(pool[p].ScaleStart, pool[p].ScalePeak, pop);
                pool[p].Rect.localScale = Vector3.one * scale;

                Color c = pool[p].BaseColor;
                c.a = pool[p].BaseColor.a * life * life;
                pool[p].Tmp.color = c;

                if (pool[p].Remaining <= 0f)
                {
                    pool[p].Active = false;
                    pool[p].Tmp.gameObject.SetActive(false);
                }
            }
        }
    }
}
