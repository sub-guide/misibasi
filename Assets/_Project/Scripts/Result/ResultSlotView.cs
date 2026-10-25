using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Result
{
    /// <summary>결과 슬롯 1칸. HP는 카드 2장 + <c>Card_Flip</c> Animator (<c>FaceUp</c> bool).</summary>
    [DisallowMultipleComponent]
    public sealed class ResultSlotView : MonoBehaviour
    {
        const string StateFaceUp = "FaceUp";
        const string StateFaceDown = "FaceDown";
        const string PracticeDash = "-";
        const string FailLabel = "FAIL";
        const float GameOverLetterDurationSeconds = 0.15f;
        const float GameOverLetterStaggerSeconds = 0.08f;
        const float GameOverLetterFlyDistance = 80f;

        static readonly int FaceUpHash = Animator.StringToHash("FaceUp");

        static readonly Color RankGold = new Color(1f, 1f, 0f, 1f);
        static readonly Color RankSilver = new Color(0.75f, 0.75f, 0.78f, 1f);
        static readonly Color RankBronze = new Color(0.80f, 0.50f, 0.20f, 1f);
        static readonly Color RankGray = new Color(0.72f, 0.72f, 0.74f, 1f);
        static readonly Color RankFail = new Color32(0xC8, 0xC8, 0xC8, 0xFF);

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

        [Header("등장")]
        [Tooltip("슬롯 자식 Fade. Intro·빈 슬롯 ON, 등수 공개 시 OFF.")]
        [SerializeField] GameObject fade;

        [Header("GAME OVER")]
        [Tooltip("슬롯 자식 GameOver.")]
        [SerializeField] TMP_Text gameOverText;

        public void SetupIntro(int slotIndex, bool participated)
        {
            SetGameOverVisible(false);
            SetRankScoreVisible(false);
            SetFadeVisible(true);
        }

        public void RevealRanking(int rank, int score, bool practice, bool hpLost = false)
        {
            SetFadeVisible(false);
            SetRankScoreVisible(true);

            if (practice)
            {
                ApplyRank(PracticeDash, Color.white);
                ApplyScore(PracticeDash);
                return;
            }

            if (hpLost)
            {
                ApplyRank(FailLabel, RankFail);
                ApplyScore(score.ToString());
                return;
            }

            ApplyRank(FormatRankLabel(rank), GetRankColor(rank));
            ApplyScore(score.ToString());
        }

        public void SetEmptySlotLook()
        {
            SetGameOverVisible(false);
            SetRankScoreVisible(false);
            SetFadeVisible(true);
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

        /// <summary>Fade를 알파 0으로 켠 뒤 목표 알파까지 보간, 끝나면 GameOver 텍스트 ON.</summary>
        public IEnumerator PlayGameOver(float fadeSeconds, float targetAlpha)
        {
            SetGameOverVisible(false);

            Image fadeImage = fade != null ? fade.GetComponent<Image>() : null;
            SetFadeVisible(true);
            if (fadeImage != null)
                SetImageAlpha(fadeImage, 0f);

            float duration = Mathf.Max(0f, fadeSeconds);
            float endAlpha = Mathf.Clamp01(targetAlpha);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                if (fadeImage != null)
                    SetImageAlpha(fadeImage, Mathf.Lerp(0f, endAlpha, t));
                yield return null;
            }

            if (fadeImage != null)
                SetImageAlpha(fadeImage, endAlpha);

            SetGameOverVisible(true);
            yield return CoFlyInGameOverLetters();
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

        void SetFadeVisible(bool visible)
        {
            if (fade != null)
                fade.SetActive(visible);
        }

        void SetGameOverVisible(bool visible)
        {
            if (gameOverText != null)
                gameOverText.gameObject.SetActive(visible);
        }

        IEnumerator CoFlyInGameOverLetters()
        {
            if (gameOverText == null)
                yield break;

            gameOverText.ForceMeshUpdate();
            TMP_TextInfo textInfo = gameOverText.textInfo;
            if (textInfo == null || textInfo.characterCount <= 0)
                yield break;

            int visibleCount = 0;
            for (var c = 0; c < textInfo.characterCount; c++)
            {
                if (textInfo.characterInfo[c].isVisible)
                    visibleCount++;
            }

            if (visibleCount <= 0)
                yield break;

            var fromOffsets = new Vector3[visibleCount];
            var materialIndex = new int[visibleCount];
            var vertexIndex = new int[visibleCount];
            float distance = GameOverLetterFlyDistance;
            int vi = 0;
            for (var c = 0; c < textInfo.characterCount; c++)
            {
                TMP_CharacterInfo ch = textInfo.characterInfo[c];
                if (!ch.isVisible)
                    continue;

                float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                fromOffsets[vi] = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * distance;
                materialIndex[vi] = ch.materialReferenceIndex;
                vertexIndex[vi] = ch.vertexIndex;
                vi++;
            }

            var originalVerts = new Vector3[textInfo.meshInfo.Length][];
            var originalCols = new Color32[textInfo.meshInfo.Length][];
            for (var m = 0; m < textInfo.meshInfo.Length; m++)
            {
                originalVerts[m] = (Vector3[])textInfo.meshInfo[m].vertices.Clone();
                originalCols[m] = (Color32[])textInfo.meshInfo[m].colors32.Clone();
            }

            float letterDur = GameOverLetterDurationSeconds;
            float stagger = GameOverLetterStaggerSeconds;
            float total = letterDur + stagger * (visibleCount - 1);
            float elapsed = 0f;

            ApplyGameOverLetterFrame(
                textInfo, originalVerts, originalCols,
                fromOffsets, materialIndex, vertexIndex, visibleCount,
                elapsed, letterDur, stagger);
            gameOverText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            if (total <= 0f)
            {
                RestoreGameOverMesh(textInfo, originalVerts, originalCols);
                gameOverText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                yield break;
            }

            while (elapsed < total)
            {
                elapsed += Time.unscaledDeltaTime;
                ApplyGameOverLetterFrame(
                    textInfo, originalVerts, originalCols,
                    fromOffsets, materialIndex, vertexIndex, visibleCount,
                    elapsed, letterDur, stagger);
                gameOverText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                yield return null;
            }

            RestoreGameOverMesh(textInfo, originalVerts, originalCols);
            gameOverText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        static void ApplyGameOverLetterFrame(
            TMP_TextInfo textInfo,
            Vector3[][] originalVerts,
            Color32[][] originalCols,
            Vector3[] fromOffsets,
            int[] materialIndex,
            int[] vertexIndex,
            int visibleCount,
            float elapsed,
            float letterDur,
            float stagger)
        {
            RestoreGameOverMesh(textInfo, originalVerts, originalCols);

            for (var i = 0; i < visibleCount; i++)
            {
                float localT = letterDur <= 0f
                    ? 1f
                    : (elapsed - i * stagger) / letterDur;

                int mesh = materialIndex[i];
                int v0 = vertexIndex[i];
                Vector3[] verts = textInfo.meshInfo[mesh].vertices;
                Color32[] cols = textInfo.meshInfo[mesh].colors32;

                if (localT <= 0f)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        Color32 col = cols[v0 + k];
                        col.a = 0;
                        cols[v0 + k] = col;
                    }

                    continue;
                }

                float t = Mathf.Clamp01(localT);
                float ease = 1f - (1f - t) * (1f - t) * (1f - t);
                Vector3 offset = fromOffsets[i] * (1f - ease);
                for (var k = 0; k < 4; k++)
                    verts[v0 + k] += offset;
            }
        }

        static void RestoreGameOverMesh(
            TMP_TextInfo textInfo,
            Vector3[][] originalVerts,
            Color32[][] originalCols)
        {
            for (var m = 0; m < originalVerts.Length; m++)
            {
                Array.Copy(originalVerts[m], textInfo.meshInfo[m].vertices, originalVerts[m].Length);
                Array.Copy(originalCols[m], textInfo.meshInfo[m].colors32, originalCols[m].Length);
            }
        }

        static void SetImageAlpha(Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
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
