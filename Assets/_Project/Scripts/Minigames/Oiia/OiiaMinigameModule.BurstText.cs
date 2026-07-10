using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("코믹스 BurstText")]
        [Tooltip("정답 글자 팝업 표시·페이드 시간(초).")]
        [SerializeField] float burstTextDuration = 0.55f;

        [Tooltip("고양이 중심 기준 생성 위치 랜덤 오프셋(±픽셀).")]
        [SerializeField] float burstTextRandomOffset = 50f;

        [Tooltip("생성 직후 초기 Z 회전(도). 이 각도를 중심축으로 스윙.")]
        [SerializeField] float burstTextSpawnRotationMin = -30f;

        [SerializeField] float burstTextSpawnRotationMax = 30f;

        [Tooltip("중심축 기준 스윙 각도 최소·최대(도). 예: -30~30.")]
        [SerializeField] float burstTextSwingMin = -30f;

        [SerializeField] float burstTextSwingMax = 30f;

        [Tooltip("스윙 속도(Hz). 값이 작을수록 천천히.")]
        [SerializeField] float burstTextSwingFrequency = 0.45f;

        [Tooltip("티어 1 BurstText Perlin 진동 진폭(px). 0이면 위치 고정.")]
        [SerializeField] float burstTextShakeAmplitudeTier1 = 8f;

        [Tooltip("티어 2 BurstText Perlin 진동 진폭(px).")]
        [SerializeField] float burstTextShakeAmplitudeTier2 = 10f;

        [Tooltip("티어 3 BurstText Perlin 진동 진폭(px).")]
        [SerializeField] float burstTextShakeAmplitudeTier3 = 14f;

        [Tooltip("티어 1 BurstText 진동 주파수(Hz).")]
        [SerializeField] float burstTextShakeFrequencyTier1 = 32f;

        [Tooltip("티어 2 BurstText 진동 주파수(Hz).")]
        [SerializeField] float burstTextShakeFrequencyTier2 = 36f;

        [Tooltip("티어 3 BurstText 진동 주파수(Hz).")]
        [SerializeField] float burstTextShakeFrequencyTier3 = 40f;

        [Tooltip("티어 1 BurstText fontSize. 프리팹 BurstText 기본 200과 동일.")]
        [SerializeField] float burstTextFontSizeTier1 = 200f;

        [Tooltip("티어 2 BurstText fontSize.")]
        [SerializeField] float burstTextFontSizeTier2 = 240f;

        [Tooltip("티어 3 BurstText fontSize.")]
        [SerializeField] float burstTextFontSizeTier3 = 300f;

        float BurstTextFontSizeForTier(int tier)
        {
            if (tier >= 3)
                return Mathf.Max(1f, burstTextFontSizeTier3);

            if (tier >= 2)
                return Mathf.Max(1f, burstTextFontSizeTier2);

            return Mathf.Max(1f, burstTextFontSizeTier1);
        }

        float BurstTextShakeAmplitudeForTier(int tier)
        {
            if (tier >= 3)
                return Mathf.Max(0f, burstTextShakeAmplitudeTier3);

            if (tier >= 2)
                return Mathf.Max(0f, burstTextShakeAmplitudeTier2);

            return Mathf.Max(0f, burstTextShakeAmplitudeTier1);
        }

        float BurstTextShakeFrequencyForTier(int tier)
        {
            if (tier >= 3)
                return Mathf.Max(0f, burstTextShakeFrequencyTier3);

            if (tier >= 2)
                return Mathf.Max(0f, burstTextShakeFrequencyTier2);

            return Mathf.Max(0f, burstTextShakeFrequencyTier1);
        }

        int ResolveBurstTextSortingOrder(int slotIndex) =>
            catMovementDrawSortOrderBase + SlotCount + BurstTextDrawSortOrderOffset + slotIndex;

        RectTransform ResolveBurstTextCoordinateParent(SlotUiBindings b) =>
            b.BurstTextContainer != null ? b.BurstTextContainer : null;

        void EnsureBurstTextDrawOnTop(int slotIndex, SlotUiBindings b)
        {
            RectTransform container = ResolveBurstTextCoordinateParent(b);
            if (container == null)
                return;

            Canvas canvas = container.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = container.gameObject.AddComponent<Canvas>();
                _burstTextSortCanvasAdded[slotIndex] = true;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = ResolveBurstTextSortingOrder(slotIndex);
        }

        void BringActiveBurstTextsToFront(int slotIndex, SlotUiBindings b, ref SlotRuntime sr)
        {
            EnsureBurstTextDrawOnTop(slotIndex, b);

            if (b.BurstTextPool == null || sr.BurstPool == null)
                return;

            for (var p = 0; p < b.BurstTextPool.Length && p < BurstTextPoolSize; p++)
            {
                if (sr.BurstPool[p].Remaining <= 0f)
                    continue;

                TMP_Text tmp = b.BurstTextPool[p];
                if (tmp == null || !tmp.gameObject.activeSelf)
                    continue;

                tmp.rectTransform.SetAsLastSibling();
            }
        }

        static void HideIdleBurstTextEntry(TMP_Text tmp)
        {
            if (tmp == null)
                return;

            tmp.gameObject.SetActive(false);
            tmp.text = string.Empty;
            Color c = tmp.color;
            c.a = 1f;
            tmp.color = c;
        }

        static void PrepareBurstTextPoolVisual(SlotUiBindings b)
        {
            if (b.BurstTextPool == null)
                return;

            for (var p = 0; p < b.BurstTextPool.Length; p++)
                HideIdleBurstTextEntry(b.BurstTextPool[p]);
        }

        void InitializeBurstTextPool(int slotIndex)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.BurstTextPool == null)
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.BurstPool == null || sr.BurstPool.Length != BurstTextPoolSize)
                sr.BurstPool = new BurstTextFx[BurstTextPoolSize];

            for (var p = 0; p < b.BurstTextPool.Length && p < BurstTextPoolSize; p++)
            {
                TMP_Text tmp = b.BurstTextPool[p];
                if (tmp == null)
                    continue;

                HideIdleBurstTextEntry(tmp);
                sr.BurstPool[p] = default;
                ApplyBurstTextP5Style(tmp);
            }

            EnsureBurstTextDrawOnTop(slotIndex, b);
        }

        static void ApplyBurstTextP5Style(TMP_Text tmp)
        {
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.35f;
            tmp.outlineColor = Color.black;

            Material mat = tmp.fontMaterial;
            if (mat == null)
                return;

            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, Color.white);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.08f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.08f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.45f);
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.12f);
            mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.28f);
        }

        void TriggerBurstTextOnCorrect(int slotIndex, char letterLower)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.BurstTextPool == null || b.BurstTextPool.Length == 0)
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.BurstPool == null)
                sr.BurstPool = new BurstTextFx[BurstTextPoolSize];

            int poolIndex = FindBurstPoolSlot(ref sr);
            if (poolIndex < 0)
                return;

            TMP_Text tmp = b.BurstTextPool[poolIndex];
            if (tmp == null)
                return;

            RectTransform parent = ResolveBurstTextCoordinateParent(b);
            if (parent == null)
                parent = tmp.rectTransform.parent as RectTransform;

            Vector2 catPos = GetCatAnchoredPositionInRect(slotIndex, parent);
            float spawnOffset = Mathf.Max(0f, burstTextRandomOffset);
            catPos.x += Random.Range(-spawnOffset, spawnOffset);
            catPos.y += Random.Range(-spawnOffset, spawnOffset);

            RectTransform rt = tmp.rectTransform;
            rt.SetParent(parent, false);
            rt.anchoredPosition = catPos;
            rt.localScale = Vector3.one;
            float baseRotation = Random.Range(burstTextSpawnRotationMin, burstTextSpawnRotationMax);

            int tier = ResolveGameplayTier(ref sr);
            tmp.fontSize = BurstTextFontSizeForTier(tier);
            tmp.text = char.ToUpperInvariant(letterLower).ToString();
            tmp.color = Color.white;
            tmp.gameObject.SetActive(true);
            rt.SetAsLastSibling();

            sr.BurstPool[poolIndex] = new BurstTextFx
            {
                Remaining = Mathf.Max(0.01f, burstTextDuration),
                Duration = Mathf.Max(0.01f, burstTextDuration),
                StartAlpha = 1f,
                BaseRotationZ = baseRotation,
                SwingPhase = Random.Range(0f, Mathf.PI * 2f),
                AnchorBase = catPos,
                PhaseX = Random.Range(0f, 1000f),
                PhaseY = Random.Range(0f, 1000f),
                ShakeAmplitude = BurstTextShakeAmplitudeForTier(tier),
                ShakeFrequency = BurstTextShakeFrequencyForTier(tier)
            };

            ApplyBurstTextMotion(tmp.rectTransform, ref sr.BurstPool[poolIndex], 0f);

            BringActiveBurstTextsToFront(slotIndex, b, ref sr);
        }

        static int FindBurstPoolSlot(ref SlotRuntime sr)
        {
            if (sr.BurstPool == null)
                return 0;

            for (var p = 0; p < sr.BurstPool.Length; p++)
            {
                if (sr.BurstPool[p].Remaining <= 0f)
                    return p;
            }

            int oldest = 0;
            float maxRemaining = sr.BurstPool[0].Remaining;
            for (var p = 1; p < sr.BurstPool.Length; p++)
            {
                if (sr.BurstPool[p].Remaining > maxRemaining)
                {
                    maxRemaining = sr.BurstPool[p].Remaining;
                    oldest = p;
                }
            }

            return oldest;
        }

        Vector2 GetCatAnchoredPositionInRect(int slotIndex, RectTransform targetRect)
        {
            if (targetRect == null || !TryGetBinding(slotIndex, out SlotUiBindings b) || b.CatAnimator == null)
                return Vector2.zero;

            RectTransform catRt = b.CatAnimator.transform as RectTransform;
            if (catRt == null)
                return Vector2.zero;

            Canvas canvas = targetRect.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, catRt.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPoint, cam, out Vector2 localPoint);
            return localPoint;
        }

        void ApplyBurstTextMotion(RectTransform rt, ref BurstTextFx fx, float deltaTime)
        {
            if (rt == null)
                return;

            if (deltaTime > 0f && burstTextSwingFrequency > 0f)
                fx.SwingPhase += deltaTime * burstTextSwingFrequency * Mathf.PI * 2f;

            float swingMin = burstTextSwingMin;
            float swingMax = burstTextSwingMax;
            if (swingMin > swingMax)
                (swingMin, swingMax) = (swingMax, swingMin);

            float swingT = (Mathf.Sin(fx.SwingPhase) + 1f) * 0.5f;
            float swingOffset = Mathf.Lerp(swingMin, swingMax, swingT);
            rt.localRotation = Quaternion.Euler(0f, 0f, fx.BaseRotationZ + swingOffset);

            if (fx.ShakeAmplitude <= 0f)
            {
                rt.anchoredPosition = fx.AnchorBase;
                return;
            }

            float dur = Mathf.Max(0.0001f, fx.Duration);
            float decay = Mathf.Clamp01(fx.Remaining / dur);
            float amp = fx.ShakeAmplitude * decay;
            float time = Time.unscaledTime * Mathf.Max(1f, fx.ShakeFrequency);
            float offsetX = (Mathf.PerlinNoise(fx.PhaseX, time) - 0.5f) * 2f * amp;
            float offsetY = (Mathf.PerlinNoise(fx.PhaseY, time) - 0.5f) * 2f * amp;
            rt.anchoredPosition = fx.AnchorBase + new Vector2(offsetX, offsetY);
        }

        void TickBurstTextPool(int slotIndex, float deltaTime)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.BurstTextPool == null)
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.BurstPool == null)
            {
                PrepareBurstTextPoolVisual(b);
                return;
            }

            for (var p = 0; p < b.BurstTextPool.Length && p < BurstTextPoolSize; p++)
            {
                TMP_Text tmp = b.BurstTextPool[p];
                if (tmp == null)
                    continue;

                ref BurstTextFx fx = ref sr.BurstPool[p];
                if (fx.Remaining <= 0f)
                {
                    if (tmp.gameObject.activeSelf)
                        HideIdleBurstTextEntry(tmp);
                    continue;
                }

                fx.Remaining -= deltaTime;

                float dur = Mathf.Max(0.0001f, fx.Duration);
                float t = 1f - Mathf.Clamp01(fx.Remaining / dur);
                Color c = tmp.color;
                c.a = Mathf.Lerp(fx.StartAlpha, 0f, t);
                tmp.color = c;

                ApplyBurstTextMotion(tmp.rectTransform, ref fx, deltaTime);

                if (fx.Remaining <= 0f)
                    HideIdleBurstTextEntry(tmp);
            }

            BringActiveBurstTextsToFront(slotIndex, b, ref sr);
        }

        void FlushBurstTextPool(int slotIndex)
        {
            TickBurstTextPool(slotIndex, 0f);
        }

        void ClearBurstTextPoolVisual(int slotIndex, SlotUiBindings b)
        {
            if (b.BurstTextPool == null)
                return;

            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.BurstPool != null)
            {
                for (var p = 0; p < b.BurstTextPool.Length && p < BurstTextPoolSize; p++)
                    sr.BurstPool[p] = default;
            }

            PrepareBurstTextPoolVisual(b);
        }
    }
}
