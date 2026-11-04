using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>
    /// 씬에 연결된 제목·플래시로 라운드 안내 연출을 재생합니다.
    /// </summary>
    public sealed class TimeChallengeRoundSting : MonoBehaviour
    {
        [SerializeField] Image flash;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text subtitle;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float duration = 1.15f;

        Vector3 _titleBase = Vector3.one;
        Vector3 _subBase = Vector3.one;
        float _elapsed = -1f;
        Color _titleColor = Color.white;
        Color _subColor = Color.white;

        void OnEnable()
        {
            if (title == null || flash == null || canvasGroup == null)
                Debug.LogWarning("[TimeChallenge] 라운드 안내 Inspector 연결이 없습니다.", this);
            Cache();
        }

        public void Play(string titleText, string subtitleText, bool finale, float playDuration = -1f)
        {
            if (playDuration > 0.05f)
                duration = playDuration;

            if (title != null)
                title.transform.localScale = Vector3.one;
            if (subtitle != null)
                subtitle.transform.localScale = Vector3.one;

            Cache();
            gameObject.SetActive(true);
            SetVisible(true);
            enabled = true;

            _titleColor = finale
                ? new Color(1f, 0.82f, 0.28f, 1f)
                : Color.white;
            _subColor = finale
                ? new Color(1f, 0.92f, 0.62f, 1f)
                : new Color(0.85f, 1f, 0.9f, 1f);

            if (title != null)
                title.text = titleText ?? string.Empty;
            if (subtitle != null)
                subtitle.text = subtitleText ?? string.Empty;

            _elapsed = 0f;
            Apply(0f);
        }

        public void Hide()
        {
            _elapsed = -1f;
            SetVisible(false);
        }

        void Update()
        {
            if (!Application.isPlaying)
                return;

            if (_elapsed < 0f)
                return;

            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.2f, duration));
            Apply(t);
            if (t >= 1f)
                Hide();
        }

        void Cache()
        {
            if (title != null) _titleBase = title.transform.localScale;
            if (subtitle != null) _subBase = subtitle.transform.localScale;
        }

        void SetVisible(bool on)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = on ? 1f : 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                return;
            }

            Debug.LogWarning("[TimeChallenge] RoundSting CanvasGroup 연결이 없습니다.", this);
        }

        void Apply(float t)
        {
            float flashPeak = t < 0.16f
                ? Mathf.Sin(t / 0.16f * Mathf.PI) * 0.58f
                : Mathf.Lerp(0.22f, 0f, Mathf.Clamp01((t - 0.16f) / 0.28f));
            if (flash != null)
                flash.color = new Color(1f, 1f, 1f, Mathf.Clamp01(flashPeak));

            float popT = Mathf.Clamp01(t / 0.4f);
            float pop = popT <= 0.0001f
                ? 0.85f
                : 1f + 1.8f * Mathf.Pow(2f, -8f * popT) * Mathf.Sin(popT * 6.4f * Mathf.PI);

            float fade = t < 0.78f
                ? 1f
                : 1f - Mathf.Clamp01((t - 0.78f) / 0.22f);

            if (title != null)
            {
                title.transform.localScale = _titleBase * pop;
                Color c = _titleColor;
                c.a = fade;
                title.color = c;
            }

            if (subtitle != null)
            {
                float subPop = Mathf.Lerp(0.6f, 1f, popT);
                subtitle.transform.localScale = _subBase * subPop;
                Color c = _subColor;
                c.a = fade * 0.95f;
                subtitle.color = c;
            }
        }
    }
}
