using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Flounder
{
    public sealed class FlounderGauge : MonoBehaviour
    {
        [Header("규칙")]
        [Tooltip("방향이 바뀐 연타 1회당 게이지 증가량 (0.01 = 1%)")]
        [SerializeField] float gainPerMash = 0.01f;
        [SerializeField] float decayPerSecond = 0.005f;
        [SerializeField] float maxPercent = 1f;

        [Header("UI (GaugeTrack / GaugeInner / Fill)")]
        [SerializeField] Image track;
        [SerializeField] Image innerBackground;
        [SerializeField] Image fill;
        [SerializeField] TextMeshProUGUI percentLabel;

        float _percent;

        public float Percent => _percent;
        public bool IsFull => _percent >= maxPercent - 0.0001f;

        public void ResetGauge()
        {
            _percent = 0f;
            RefreshView();
        }

        public void RegisterMash()
        {
            _percent += gainPerMash;
            if (_percent >= maxPercent - 0.00001f)
                _percent = maxPercent;

            RefreshView();
        }

        public void ClearAfterDonation() => ResetGauge();

        public void Tick(float deltaTime)
        {
            if (_percent <= 0f)
                return;

            _percent = Mathf.Max(0f, _percent - decayPerSecond * deltaTime);
            RefreshView();
        }

        void RefreshView()
        {
            float normalized = maxPercent > 0f ? Mathf.Clamp01(_percent / maxPercent) : 0f;

            if (fill != null)
                fill.fillAmount = normalized;

            if (percentLabel != null)
            {
                int shown = normalized >= 0.999f ? 100 : Mathf.RoundToInt(normalized * 100f);
                percentLabel.text = shown + "%";
            }
        }
    }
}
