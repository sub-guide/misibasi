using System.Collections;
using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>전체 화면 검정 오버레이 페이드. CanvasGroup alpha 0=투명, 1=검정.</summary>
    [DisallowMultipleComponent]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;

        public float Alpha
        {
            get => canvasGroup != null ? canvasGroup.alpha : 0f;
            set
            {
                if (canvasGroup == null)
                    return;

                canvasGroup.alpha = Mathf.Clamp01(value);
                canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.01f;
            }
        }

        void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetInstant(float alpha) => Alpha = alpha;

        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (canvasGroup == null)
                yield break;

            float start = canvasGroup.alpha;
            targetAlpha = Mathf.Clamp01(targetAlpha);

            if (duration <= 0f)
            {
                Alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Alpha = Mathf.Lerp(start, targetAlpha, t);
                yield return null;
            }

            Alpha = targetAlpha;
        }
    }
}
