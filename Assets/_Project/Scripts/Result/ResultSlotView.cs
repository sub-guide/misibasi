using System.Collections;
using UnityEngine;

namespace MiniParty.Result
{
    /// <summary>결과 슬롯 1칸. HP는 카드 2장 + <c>Card_Flip</c> Animator (<c>FaceUp</c> bool).</summary>
    [DisallowMultipleComponent]
    public sealed class ResultSlotView : MonoBehaviour
    {
        const string StateFaceUp = "FaceUp";
        const string StateFaceDown = "FaceDown";

        static readonly int FaceUpHash = Animator.StringToHash("FaceUp");

        [Header("HP 카드 (0=왼쪽, 1=오른쪽)")]
        [Tooltip("Card_0 · Card_1 의 Animator. Controller = Card_Flip.")]
        [SerializeField] Animator[] hpCardAnimators = new Animator[2];

        [Header("HP 카드 연출")]
        [Tooltip("FlipToBack 클립 대기 상한(초). Controller Exit Time 기준.")]
        [SerializeField] float cardFlipWaitSeconds = 0.29f;

        public void SetupIntro(int slotIndex, bool participated)
        {
        }

        public void RevealRanking(int rank, int score, bool practice)
        {
        }

        public void SetEmptySlotLook()
        {
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
    }
}
