using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>네온 테이블을 물리 버튼처럼 눌렸다 뗀다.</summary>
    public sealed class TimeChallengeTableButton : MonoBehaviour
    {
        [SerializeField] float pressDepth = 22f;
        [SerializeField] float pressedScaleY = 0.86f;
        [SerializeField] Color pressedTint = new Color(0.75f, 1.15f, 0.85f, 1f);

        Vector3 _restPos;
        Vector3 _restScale;
        [SerializeField] SpriteRenderer tableRenderer;
        Color _restColor;
        bool _captured;
        bool _pressed;

        public bool IsPressed => _pressed;

        void Awake()
        {
            if (tableRenderer == null) Debug.LogWarning("[TimeChallenge] 책상 SpriteRenderer 연결이 없습니다.", this);
            CaptureRest();
        }

        public void CaptureRest()
        {
            _restPos = transform.localPosition;
            _restScale = transform.localScale;
            if (tableRenderer != null)
                _restColor = tableRenderer.color;
            _captured = true;
        }

        public void SetPressed(bool pressed)
        {
            if (!_captured)
                CaptureRest();

            if (_pressed == pressed)
                return;

            _pressed = pressed;
            if (pressed)
            {
                transform.localPosition = _restPos + Vector3.down * pressDepth;
                transform.localScale = new Vector3(_restScale.x, _restScale.y * pressedScaleY, _restScale.z);
            }
            else
            {
                transform.localPosition = _restPos;
                transform.localScale = _restScale;
            }

            ApplyPressedColor();
        }

        void ApplyPressedColor()
        {
            if (tableRenderer == null)
                return;

            tableRenderer.color = _pressed ? _restColor * pressedTint : _restColor;
        }
    }
}
