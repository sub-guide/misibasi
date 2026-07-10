using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Result
{
    /// <summary>결과 슬롯 1칸 UI. 연출 API는 단계별로 확장한다.</summary>
    [DisallowMultipleComponent]
    public sealed class ResultSlotView : MonoBehaviour
    {
        [SerializeField] ResultSlotBindings bindings;

        ResultHpDisplay _hpDisplay;
        ResultGameOverPanel _gameOverPanel;
        Vector2 _shakeRootRestPosition;
        Vector2 _spotlightRestAnchoredPosition;
        bool _shakeRestCached;

        static readonly Color ReadyBorderGreen = new(0.25f, 0.92f, 0.42f, 1f);
        static readonly Color LoserBlurColor = new(0f, 0f, 0f, 0.72f);
        static readonly Color DimBlurColor = new(0f, 0f, 0f, 0.82f);

        static readonly Color Rank1Gold = new(1f, 0.84f, 0f, 1f);
        static readonly Color Rank2Silver = new(0.75f, 0.78f, 0.82f, 1f);
        static readonly Color Rank3Bronze = new(0.8f, 0.5f, 0.2f, 1f);
        static readonly Color Rank4GrayWhite = new(0.88f, 0.88f, 0.9f, 1f);

        void Awake()
        {
            if (bindings == null)
                bindings = GetComponent<ResultSlotBindings>();

            if (bindings?.HpDisplay != null)
                _hpDisplay = bindings.HpDisplay.GetComponent<ResultHpDisplay>() ??
                              bindings.HpDisplay.gameObject.AddComponent<ResultHpDisplay>();

            if (bindings?.GameOverAnchor != null)
                _gameOverPanel = bindings.GameOverAnchor.GetComponentInChildren<ResultGameOverPanel>(true);

            CacheShakeAndSpotlightRest();
        }

        void CacheShakeAndSpotlightRest()
        {
            if (bindings?.ShakeRoot != null)
            {
                _shakeRootRestPosition = bindings.ShakeRoot.anchoredPosition;
                _shakeRestCached = true;
            }

            if (bindings?.Spotlight != null)
                _spotlightRestAnchoredPosition = bindings.Spotlight.rectTransform.anchoredPosition;
        }

        /// <summary>Intro: 플레이어 라벨만. 등수·점수는 <see cref="RevealRanking"/> 에서 공개.</summary>
        public void SetupIntro(int slotIndex, bool participated)
        {
            if (bindings == null)
                return;

            if (bindings.PlayerText != null)
            {
                bindings.PlayerText.gameObject.SetActive(participated);
                bindings.PlayerText.text = $"PLAYER {slotIndex + 1}";
            }

            if (bindings.ScoreText != null)
                bindings.ScoreText.gameObject.SetActive(false);

            if (bindings.RankText != null)
                bindings.RankText.gameObject.SetActive(false);

            SetImageActive(bindings.Blur, !participated);
            if (!participated)
                SetBlurAlpha(0.55f);

            SetImageActive(bindings.ReadyBorder, false);
            SetImageActive(bindings.Spotlight, false);
        }

        /// <summary>등수 공개 연출: RankText·ScoreText 표시.</summary>
        public void RevealRanking(int rank, int score, bool practice)
        {
            if (bindings == null)
                return;

            if (bindings.RankText != null)
            {
                bindings.RankText.gameObject.SetActive(true);
                if (practice)
                {
                    bindings.RankText.text = "-";
                    bindings.RankText.color = Color.white;
                }
                else
                {
                    bindings.RankText.text = FormatRankLabel(rank);
                    bindings.RankText.color = GetRankColor(rank);
                }
            }

            if (bindings.ScoreText != null)
            {
                bindings.ScoreText.gameObject.SetActive(true);
                bindings.ScoreText.text = practice ? "-" : score.ToString();
            }
        }

        public void SetEmptySlotLook()
        {
            if (bindings == null)
                return;

            if (bindings.PlayerText != null)
                bindings.PlayerText.gameObject.SetActive(false);

            if (bindings.ScoreText != null)
                bindings.ScoreText.gameObject.SetActive(false);

            if (bindings.RankText != null)
                bindings.RankText.gameObject.SetActive(false);

            SetImageActive(bindings.Blur, true);
            SetBlurAlpha(0.55f);
        }

        public void SetHp(int current, int max) => _hpDisplay?.SetHp(current, max);

        public IEnumerator PlayLoserHit(int hpBefore, int hpAfter, int maxHp, float duration)
        {
            if (bindings == null)
                yield break;

            if (!_shakeRestCached)
                CacheShakeAndSpotlightRest();

            SetImageActive(bindings.Blur, true);
            if (bindings.Blur != null)
                bindings.Blur.color = LoserBlurColor;

            SetImageActive(bindings.Spotlight, false);

            float elapsed = 0f;
            float shakeDuration = Mathf.Max(0.05f, duration);

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                if (bindings.ShakeRoot != null)
                {
                    float magnitude = 12f * (1f - elapsed / shakeDuration);
                    var offset = new Vector2(
                        Random.Range(-magnitude, magnitude),
                        Random.Range(-magnitude, magnitude));
                    bindings.ShakeRoot.anchoredPosition = _shakeRootRestPosition + offset;
                }

                yield return null;
            }

            if (bindings.ShakeRoot != null)
                bindings.ShakeRoot.anchoredPosition = _shakeRootRestPosition;

            SetHp(hpAfter, maxHp);
        }

        public IEnumerator PlayWinnerSpotlight(float duration)
        {
            if (bindings?.Spotlight == null)
                yield break;

            RectTransform spotRect = bindings.Spotlight.rectTransform;
            SetImageActive(bindings.Spotlight, true);

            var start = _spotlightRestAnchoredPosition + new Vector2(0f, 220f);
            var end = _spotlightRestAnchoredPosition;
            Color c = bindings.Spotlight.color;
            c.a = 0f;
            bindings.Spotlight.color = c;

            float elapsed = 0f;
            float dur = Mathf.Max(0.05f, duration);

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float eased = 1f - Mathf.Pow(1f - t, 2f);

                spotRect.anchoredPosition = Vector2.Lerp(start, end, eased);
                c.a = Mathf.Lerp(0f, 0.85f, eased);
                bindings.Spotlight.color = c;

                yield return null;
            }

            spotRect.anchoredPosition = end;
            c.a = 0.85f;
            bindings.Spotlight.color = c;
        }

        public void SetDimmed(bool on)
        {
            SetImageActive(bindings?.Blur, on);
            if (on && bindings?.Blur != null)
                bindings.Blur.color = DimBlurColor;
        }

        public void ShowGameOver(int slotIndex) => _gameOverPanel?.Show(slotIndex);

        public void SetReadyBorder(bool on)
        {
            if (bindings?.ReadyBorder == null)
                return;

            bindings.ReadyBorder.gameObject.SetActive(on);
            if (on)
                bindings.ReadyBorder.color = ReadyBorderGreen;
        }

        void SetBlurAlpha(float a)
        {
            if (bindings?.Blur == null)
                return;

            Color c = bindings.Blur.color;
            c.a = a;
            bindings.Blur.color = c;
        }

        static void SetImageActive(Image image, bool active)
        {
            if (image != null)
                image.gameObject.SetActive(active);
        }

        static string FormatRankLabel(int rank)
        {
            if (rank <= 0)
                return "-";

            return rank switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                4 => "4th",
                _ => $"{rank}{GetOrdinalSuffix(rank)}"
            };
        }

        static Color GetRankColor(int rank) =>
            rank switch
            {
                1 => Rank1Gold,
                2 => Rank2Silver,
                3 => Rank3Bronze,
                _ => Rank4GrayWhite
            };

        static string GetOrdinalSuffix(int rank)
        {
            int mod100 = rank % 100;
            if (mod100 is >= 11 and <= 13)
                return "th";

            return (rank % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }
    }
}
