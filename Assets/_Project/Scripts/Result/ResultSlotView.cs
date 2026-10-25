using System.Collections;
using TMPro;
using UnityEngine;

namespace MiniParty.Result
{
    /// <summary>결과 슬롯 1칸. HP는 카드 2장 + <c>Card_Flip</c> Animator (<c>FaceUp</c> bool).</summary>
    [DisallowMultipleComponent]
    public sealed class ResultSlotView : MonoBehaviour
    {
        const string StateFaceUp = "FaceUp";
        const string StateFaceDown = "FaceDown";
        const string PracticeDash = "-";

        static readonly int FaceUpHash = Animator.StringToHash("FaceUp");

        static readonly Color RankGold = new Color(1f, 1f, 0f, 1f);
        static readonly Color RankSilver = new Color(0.75f, 0.75f, 0.78f, 1f);
        static readonly Color RankBronze = new Color(0.80f, 0.50f, 0.20f, 1f);
        static readonly Color RankGray = new Color(0.72f, 0.72f, 0.74f, 1f);

        [Header("HP 카드 (0=왼쪽, 1=오른쪽)")]
        [Tooltip("Card_0 · Card_1 의 Animator. Controller = Card_Flip.")]
        [SerializeField] Animator[] hpCardAnimators = new Animator[2];

        [Header("HP 카드 연출")]
        [Tooltip("FlipToBack 클립 대기 상한(초). Controller Exit Time 기준.")]
        [SerializeField] float cardFlipWaitSeconds = 0.29f;

        [Header("등수 · 점수")]
        [Tooltip("슬롯 자식 RankText.")]
        [SerializeField] TMP_Text rankText;

        [Tooltip("슬롯 자식 ScoreText.")]
        [SerializeField] TMP_Text scoreText;

        public void SetupIntro(int slotIndex, bool participated)
        {
            SetRankScoreVisible(false);
        }

        public void RevealRanking(int rank, int score, bool practice)
        {
            SetRankScoreVisible(true);

            if (practice)
            {
                ApplyRank(PracticeDash, Color.white);
                ApplyScore(PracticeDash);
                return;
            }

            ApplyRank(FormatRankLabel(rank), GetRankColor(rank));
            ApplyScore(score.ToString());
        }

        public void SetEmptySlotLook()
        {
            SetRankScoreVisible(false);
        }

        /// <summary>카드 i: <c>i &gt;= (max - current)</c> 이면 앞면 idle 스냅.</summary>
        public void SetHp(int current, int max)
        {
            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);
            int lost = max - current;

            int n = hpCardAnimators != null ? hpCardAnimators.Length : 0;
            for (var i = 0; i < n; i++)
            {
                if (i >= max)
                {
                    SetCardActive(i, active: false);
                    continue;
                }

                SetCardActive(i, active: true);
                bool faceUp = i >= lost;
                SnapCard(hpCardAnimators[i], faceUp);
            }
        }

        public IEnumerator PlayLoserHit(int hpBefore, int hpAfter, int maxHp, float duration)
        {
            int flipIndex = maxHp - hpBefore;
            if (flipIndex < 0 || hpCardAnimators == null || flipIndex >= hpCardAnimators.Length)
            {
                yield break;
            }

            Animator anim = hpCardAnimators[flipIndex];
            if (anim == null)
            {
                yield break;
            }

            yield return CoFlipCardToBack(anim, duration);
        }

        public IEnumerator PlayWinnerSpotlight(float duration)
        {
            yield break;
        }

        public void SetDimmed(bool on)
        {
        }

        public void ShowGameOver(int slotIndex)
        {
        }

        public void SetReadyBorder(bool on)
        {
        }

        IEnumerator CoFlipCardToBack(Animator anim, float durationFallback)
        {
            anim.SetBool(FaceUpHash, false);

            float limit = Mathf.Max(cardFlipWaitSeconds, durationFallback);
            float elapsed = 0f;

            while (elapsed < limit)
            {
                elapsed += Time.unscaledDeltaTime;

                AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(StateFaceDown))
                    yield break;

                yield return null;
            }
        }

        static void SnapCard(Animator anim, bool faceUp)
        {
            if (anim == null)
                return;

            anim.SetBool(FaceUpHash, faceUp);
            anim.Play(faceUp ? StateFaceUp : StateFaceDown, 0, 1f);
            anim.Update(0f);
        }

        void SetCardActive(int index, bool active)
        {
            if (hpCardAnimators == null || index < 0 || index >= hpCardAnimators.Length)
                return;

            Animator anim = hpCardAnimators[index];
            if (anim != null)
                anim.gameObject.SetActive(active);
        }

        void SetRankScoreVisible(bool visible)
        {
            if (rankText != null)
                rankText.gameObject.SetActive(visible);
            if (scoreText != null)
                scoreText.gameObject.SetActive(visible);
        }

        void ApplyRank(string label, Color color)
        {
            if (rankText == null)
                return;

            rankText.text = label;
            rankText.color = color;
        }

        void ApplyScore(string value)
        {
            if (scoreText == null)
                return;

            scoreText.text = $"SCORE\n\n{value}";
        }

        static string FormatRankLabel(int rank)
        {
            switch (rank)
            {
                case 1: return "1st";
                case 2: return "2nd";
                case 3: return "3rd";
                default: return rank + "th";
            }
        }

        static Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return RankGold;
                case 2: return RankSilver;
                case 3: return RankBronze;
                default: return RankGray;
            }
        }
    }
}
