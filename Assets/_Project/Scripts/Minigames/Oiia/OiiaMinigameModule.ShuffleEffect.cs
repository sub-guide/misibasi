using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("셔플 이펙트")]
        [Tooltip("루프 완주 셔플 연출 스프라이트. 비면 슬롯 ShuffleEffect Image.sprite 유지.")]
        [SerializeField] Sprite shuffleEffectSprite;

        [Tooltip("슬롯 중앙 스프라이트 확대·페이드 총 시간(초).")]
        [SerializeField] float shuffleEffectDuration = 1f;

        [Tooltip("연출 시작 크기 (localScale).")]
        [SerializeField] float shuffleEffectStartScale = 0.25f;

        [Tooltip("연출 최대 크기 (localScale).")]
        [SerializeField] float shuffleEffectEndScale = 2.2f;

        [Tooltip("확대 속도 배율. 1=연출 시간 내내 선형 확대. 클수록 빨리 최대 크기에 도달.")]
        [SerializeField] float shuffleEffectScaleGrowSpeed = 1f;

        void InitializeShuffleEffectVisual(int slotIndex, SlotUiBindings b)
        {
            EnsureShuffleEffectSprite(b);
            HideShuffleEffect(b);
        }

        void EnsureShuffleEffectSprite(SlotUiBindings b)
        {
            if (b.ShuffleEffect == null || shuffleEffectSprite == null)
                return;

            b.ShuffleEffect.sprite = shuffleEffectSprite;
        }

        void HideShuffleEffect(SlotUiBindings b)
        {
            if (b.ShuffleEffect == null)
                return;

            b.ShuffleEffect.gameObject.SetActive(false);
            ResetShuffleEffectVisual(b.ShuffleEffect);
        }

        static void ResetShuffleEffectVisual(Image img)
        {
            img.rectTransform.localScale = Vector3.one;
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        void BeginShuffleEffect(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            sr.ShuffleEffectTimer = Mathf.Max(0.01f, shuffleEffectDuration);
            sr.Cursor = 0;
            sr.InTypoState = false;

            if (!_ctx.IsPractice)
                sr.Gauge01 = 1f;

            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.ShuffleEffect == null)
                return;

            EnsureShuffleEffectSprite(b);

            Image img = b.ShuffleEffect;
            img.gameObject.SetActive(true);
            img.rectTransform.SetAsLastSibling();

            float start = Mathf.Max(0.01f, shuffleEffectStartScale);
            img.rectTransform.localScale = Vector3.one * start;
            Color c = img.color;
            c.a = 0f;
            img.color = c;

            float dur = Mathf.Max(0.01f, shuffleEffectDuration);
            ApplyShuffleEffectFrame(img, sr.ShuffleEffectTimer, dur);
        }

        void TickShuffleEffect(int slotIndex, float deltaTime)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];

            if (sr.ShuffleEffectTimer <= 0f)
                return;

            sr.ShuffleEffectTimer -= deltaTime;

            if (TryGetBinding(slotIndex, out SlotUiBindings b) && b.ShuffleEffect != null &&
                b.ShuffleEffect.gameObject.activeSelf)
            {
                float dur = Mathf.Max(0.01f, shuffleEffectDuration);
                ApplyShuffleEffectFrame(b.ShuffleEffect, Mathf.Max(0f, sr.ShuffleEffectTimer), dur);
            }

            if (sr.ShuffleEffectTimer > 0f)
                return;

            sr.ShuffleEffectTimer = 0f;

            if (TryGetBinding(slotIndex, out SlotUiBindings bb))
                HideShuffleEffect(bb);
        }

        void ApplyShuffleEffectFrame(Image img, float remaining, float duration)
        {
            float dur = Mathf.Max(0.0001f, duration);
            float t = 1f - Mathf.Clamp01(remaining / dur);

            float start = Mathf.Max(0.01f, shuffleEffectStartScale);
            float end = Mathf.Max(start, shuffleEffectEndScale);
            float growSpeed = Mathf.Max(0.01f, shuffleEffectScaleGrowSpeed);
            float scaleT = Mathf.Clamp01(t * growSpeed);
            float scale = Mathf.Lerp(start, end, scaleT);
            img.rectTransform.localScale = Vector3.one * scale;

            Color c = img.color;
            c.a = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
            img.color = c;
        }
    }
}
