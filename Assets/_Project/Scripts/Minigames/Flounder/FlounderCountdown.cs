using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    /// <summary>시작 후원 알림 종료 후 5-4-3-2-1-START 카운트다운.</summary>
    public sealed class FlounderCountdown : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] TextMeshProUGUI styleReference;
        [SerializeField] int countdownStart = 5;
        [SerializeField] float stepSeconds = 1f;
        [SerializeField] float startDisplaySeconds = 0.35f;

        int _stepIndex;
        float _timer;
        bool _running;

        public bool IsRunning => _running;

        public void Begin()
        {
            ApplyStyleFromReference();
            _running = true;
            _stepIndex = 0;
            _timer = 0f;
            if (root != null)
                root.SetActive(true);
            ShowNumber(Mathf.Max(1, countdownStart));
        }

        void ApplyStyleFromReference()
        {
            if (label == null || styleReference == null)
                return;

            label.font = styleReference.font;
            label.fontSharedMaterial = styleReference.fontSharedMaterial;
            label.fontSize = styleReference.fontSize;
            label.fontStyle = styleReference.fontStyle;
            label.color = styleReference.color;
        }

        public void Cancel()
        {
            _running = false;
            if (root != null)
                root.SetActive(false);
        }

        /// <returns>카운트다운이 끝나 START! 직후 true (한 프레임).</returns>
        public bool Tick(float deltaTime)
        {
            if (!_running)
                return false;

            int start = Mathf.Max(1, countdownStart);
            bool showingStart = _stepIndex >= start;
            float wait = showingStart ? startDisplaySeconds : stepSeconds;
            _timer += deltaTime;
            if (_timer < wait)
                return false;

            _timer = 0f;
            _stepIndex++;

            if (_stepIndex < start)
            {
                ShowNumber(start - _stepIndex);
                return false;
            }

            if (_stepIndex == start)
            {
                ShowStart();
                return false;
            }

            _running = false;
            if (root != null)
                root.SetActive(false);
            return true;
        }

        void ShowNumber(int value)
        {
            if (label == null)
                return;

            label.text = value.ToString();
        }

        void ShowStart()
        {
            if (label == null)
                return;

            label.text = "START!";
        }
    }
}
