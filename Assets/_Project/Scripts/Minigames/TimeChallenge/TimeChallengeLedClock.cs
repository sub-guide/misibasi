using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>
    /// 씬 Canvas에 미리 둔 시계 텍스트만 갱신한다. 오브젝트를 새로 만들지 않는다.
    /// </summary>
    public sealed class TimeChallengeLedClock : MonoBehaviour
    {
        public enum Mode
        {
            Off,
            Running,
            Lit
        }

        [SerializeField] TMP_Text readout;
        [SerializeField] SpriteRenderer clockRenderer;
        [SerializeField] TMP_Text[] digits = new TMP_Text[4];
        [SerializeField] TMP_Text colon;
        [SerializeField] GameObject cover;

        public Mode CurrentMode { get; private set; } = Mode.Off;

        string _shown = "00:00";
        bool _vacant;
        bool _spriteRestCached;
        Color _spriteRest = Color.white;

        readonly System.Collections.Generic.Dictionary<int, Color> _textRest =
            new System.Collections.Generic.Dictionary<int, Color>(8);

        public void SetVacant(bool vacant)
        {
            gameObject.SetActive(true);
            _vacant = vacant;
            if (vacant)
            {
                SetCovered(false);
                CurrentMode = Mode.Off;
                _shown = "88:88";
            }

            Apply();
        }

        public bool HasDisplay
        {
            get
            {
                if (readout != null)
                    return true;
                if (colon != null)
                    return true;
                if (digits == null)
                    return false;
                for (var i = 0; i < digits.Length; i++)
                {
                    if (digits[i] != null)
                        return true;
                }

                return false;
            }
        }

        void OnEnable()
        {
            if (readout == null || cover == null || clockRenderer == null)
                Debug.LogWarning("[TimeChallenge] 시계 TMP·가림 이미지·SpriteRenderer 연결이 없습니다.", this);
            Apply();
        }

        public void SetCovered(bool covered)
        {
            if (cover != null)
                cover.SetActive(covered);

            SetReadoutsVisible(!covered);
        }

        void SetReadoutsVisible(bool visible)
        {
            SetTextVisible(readout, visible);
            SetTextVisible(colon, visible);

            if (digits != null)
            {
                for (var i = 0; i < digits.Length; i++)
                    SetTextVisible(digits[i], visible);
            }

        }

        static void SetTextVisible(TMP_Text text, bool visible)
        {
            if (text == null)
                return;

            text.enabled = true;
            Color c = text.color;
            c.a = visible ? 1f : 0f;
            text.color = c;
        }

        public void ShowOff()
        {
            CurrentMode = Mode.Off;
            _shown = "00:00";
            Apply();
        }

        public void ShowRunning(double seconds)
        {
            CurrentMode = Mode.Running;
            _shown = FormatStopwatch(seconds);
            Apply();
        }

        public void ShowLit(double seconds)
        {
            CurrentMode = Mode.Lit;
            _shown = FormatStopwatch(seconds);
            Apply();
        }

        public void ShowMiss()
        {
            CurrentMode = Mode.Lit;
            _shown = "--:--";
            Apply();
        }

        public static int ToCentiseconds(double seconds)
        {
            if (seconds < 0d)
                seconds = 0d;

            int centi = (int)System.Math.Round(seconds * 100d);
            if (centi > 9999)
                centi = 9999;
            return centi;
        }

        public static int DisplayErrorCentiseconds(double recordSeconds, double targetSeconds)
        {
            return System.Math.Abs(ToCentiseconds(recordSeconds) - ToCentiseconds(targetSeconds));
        }

        public static string FormatStopwatch(double seconds)
        {
            int centi = ToCentiseconds(seconds);
            int s = centi / 100;
            int c = centi % 100;
            return $"{s:00}:{c:00}";
        }

        void Apply()
        {
            string body = string.IsNullOrEmpty(_shown) ? "00:00" : _shown;
            if (body.Length < 5)
                body = FormatStopwatch(0d);

            if (readout != null)
                readout.text = body;

            if (digits != null)
            {
                string compact = body.Replace(":", string.Empty);
                if (compact.Length < 4)
                    compact = compact.PadLeft(4, '0');

                for (var i = 0; i < 4 && i < digits.Length; i++)
                {
                    if (digits[i] == null)
                        continue;
                    digits[i].text = compact[i].ToString();
                }
            }

            if (colon != null)
                colon.text = ":";

            ForceMesh(readout);
            ForceMesh(colon);
            if (digits != null)
            {
                for (var i = 0; i < digits.Length; i++)
                    ForceMesh(digits[i]);
            }

            ApplyPowerStyle();
        }

        static void ForceMesh(TMP_Text text)
        {
            if (text != null && text.isActiveAndEnabled)
                text.ForceMeshUpdate();
        }

        void ApplyPowerStyle()
        {
            CacheSpriteRest();
            var sr = clockRenderer;
            if (sr != null)
            {
                sr.color = _vacant
                    ? new Color(_spriteRest.r * 0.42f, _spriteRest.g * 0.42f, _spriteRest.b * 0.48f, _spriteRest.a)
                    : _spriteRest;
            }

            ApplyTextPower(readout);
            ApplyTextPower(colon);
            if (digits != null)
            {
                for (var i = 0; i < digits.Length; i++)
                    ApplyTextPower(digits[i]);
            }

        }

        void CacheSpriteRest()
        {
            if (_spriteRestCached)
                return;

            var sr = clockRenderer;
            if (sr != null)
                _spriteRest = sr.color;
            _spriteRestCached = true;
        }

        void ApplyTextPower(TMP_Text text)
        {
            if (text == null)
                return;

            int id = text.GetInstanceID();
            if (!_textRest.ContainsKey(id))
                _textRest[id] = text.color;

            Color rest = _textRest[id];
            Color c = _vacant
                ? new Color(rest.r * 0.28f, rest.g * 0.32f, rest.b * 0.28f, Mathf.Max(0.18f, rest.a * 0.28f))
                : rest;

            if (cover != null && cover.activeInHierarchy)
                c.a = 0f;

            text.color = c;
        }
    }
}
