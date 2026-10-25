using System.Collections;
using UnityEngine;

namespace MiniParty.Result
{
    /// <summary>
    /// 결과 슬롯 1칸. 옛 하트/Blur/Spotlight/Ready/GAME OVER UI는 제거됨.
    /// 시각 연출은 이후 카드 리메이크에서 채운다. 플로우 훅만 유지한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResultSlotView : MonoBehaviour
    {
        public void SetupIntro(int slotIndex, bool participated)
        {
        }

        public void RevealRanking(int rank, int score, bool practice)
        {
        }

        public void SetEmptySlotLook()
        {
        }

        public void SetHp(int current, int max)
        {
        }

        public IEnumerator PlayLoserHit(int hpBefore, int hpAfter, int maxHp, float duration)
        {
            yield break;
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
    }
}
