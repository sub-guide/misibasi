using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>
    /// 책상 아래 합산 점수판. 라운드가 끝나면 목표 점수까지 올라가는 숫자 연출.
    /// </summary>
    public sealed class TimeChallengeScoreBoard : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] float countDuration = 0.85f;
        [SerializeField] float punchScale = 1.16f;

        Vector3 _baseScale = Vector3.one;
        float _display;
        float _from;
        int _target;
        float _elapsed;
        float _punch;
        bool _animating;
        bool _capturedScale;

        public int DisplayedScore => Mathf.RoundToInt(_display);

        void Awake()
        {
            if (label == null) Debug.LogWarning("[TimeChallenge] 점수판 TMP 연결이 없습니다.", this);
            CaptureScale();
        }

        public void SnapTo(int score)
        {
            CaptureScale();

            _target = Mathf.Max(0, score);
            _display = _target;
            _from = _target;
            _elapsed = 0f;
            _punch = 0f;
            _animating = false;
            transform.localScale = _baseScale;
            Apply();
        }

        public void AnimateTo(int score, float duration)
        {
            if (duration > 0.01f)
                countDuration = duration;

            AnimateTo(score);
        }

        public void AnimateTo(int score)
        {
            CaptureScale();

            int next = Mathf.Max(0, score);
            if (next == _target && !_animating)
            {
                Apply();
                return;
            }

            _from = _display;
            _target = next;
            _elapsed = 0f;
            _animating = true;
            _punch = 1f;
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (_animating)
            {
                float duration = Mathf.Max(0.05f, countDuration);
                _elapsed += Mathf.Max(0f, unscaledDeltaTime);
                float t = Mathf.Clamp01(_elapsed / duration);
                t = 1f - (1f - t) * (1f - t);
                _display = Mathf.Lerp(_from, _target, t);
                if (t >= 1f)
                {
                    _display = _target;
                    _animating = false;
                }

                Apply();
            }

            if (_punch <= 0f)
                return;

            _punch = Mathf.MoveTowards(_punch, 0f, Mathf.Max(0f, unscaledDeltaTime) / 0.35f);
            float wave = Mathf.Sin(_punch * Mathf.PI);
            transform.localScale = _baseScale * (1f + (Mathf.Max(1f, punchScale) - 1f) * wave);
            if (_punch <= 0f)
                transform.localScale = _baseScale;
        }

        void CaptureScale()
        {
            if (_capturedScale)
                return;

            _baseScale = transform.localScale;
            if (_baseScale.sqrMagnitude < 0.0001f)
                _baseScale = Vector3.one;
            _capturedScale = true;
        }

        void Apply()
        {
            if (label != null)
                label.text = DisplayedScore.ToString();
        }
    }
}
