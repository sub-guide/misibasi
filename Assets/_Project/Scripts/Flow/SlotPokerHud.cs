using MiniParty.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Flow
{
    /// <summary>
    /// 메인 하단 1슬롯 포커 카드 HUD. 위치·부채꼴·착지는 Animator 클립(에디터).
    /// 코드는 상태·HP에 맞는 Mode/Pad만 넣는다.
    /// </summary>
    public sealed class SlotPokerHud : MonoBehaviour
    {
        /// <summary>Animator Int <c>Mode</c> 값. 컨트롤러 파라미터 이름·정수와 같아야 한다.</summary>
        public enum CardMode
        {
            Hidden = 0,
            Fan = 1,
            FaceDown = 2,
            Slam = 3
        }

        static readonly int ModeHash = Animator.StringToHash("Mode");
        static readonly int PadHash = Animator.StringToHash("Pad");

        [Header("착지 칸 (왼쪽=0)")]
        [Tooltip("card_outline 가로 2개. 클립 종점 맞춤용. 코드는 이동에 쓰지 않음.")]
        [SerializeField] RectTransform[] landPads;

        [Header("카드 (인덱스 = 칸. 0이 왼쪽)")]
        [SerializeField] CardBind[] cards;

        [Header("슬롯 오버레이")]
        [Tooltip("EMPTY 이고 HP>0 (미참가).")]
        [SerializeField] Image dimOverlay;

        [Tooltip("EMPTY 이고 HP≤0 (탈락 복귀). Start 재JOIN은 그대로.")]
        [SerializeField] Image blackoutOverlay;

        bool _loggedCapacity;

        [System.Serializable]
        public sealed class CardBind
        {
            [Tooltip("카드 루트. Hidden이면 비활성.")]
            public GameObject root;

            public Animator animator;

            [Tooltip("앞면(하트 에이스). Fan·Slam.")]
            public GameObject front;

            [Tooltip("뒷면. FaceDown.")]
            public GameObject back;
        }

        public void Apply(PlayerSlotModel slot, int startingHp)
        {
            if (slot == null)
                return;

            WarnIfCapacityShort(startingHp);

            int hp = slot.HP;
            SlotState state = slot.State;

            bool empty = state == SlotState.EMPTY;
            bool noLives = hp <= 0;

            SetOverlay(dimOverlay, empty && !noLives);
            SetOverlay(blackoutOverlay, noLives);

            if (empty || noLives)
            {
                HideAllCards();
                return;
            }

            int lost = startingHp - hp;
            if (lost < 0)
                lost = 0;

            bool slam = state is SlotState.READY or SlotState.PLAYING or SlotState.RESULT;
            int n = cards != null ? cards.Length : 0;

            for (var i = 0; i < n; i++)
            {
                if (i >= startingHp)
                {
                    ApplyCard(cards[i], CardMode.Hidden, 0);
                    continue;
                }

                if (i < lost)
                {
                    ApplyCard(cards[i], CardMode.FaceDown, i);
                    continue;
                }

                if (slam && i == lost)
                {
                    ApplyCard(cards[i], CardMode.Slam, i);
                    continue;
                }

                ApplyCard(cards[i], CardMode.Fan, 0);
            }
        }

        void HideAllCards()
        {
            if (cards == null)
                return;

            for (var i = 0; i < cards.Length; i++)
                ApplyCard(cards[i], CardMode.Hidden, 0);
        }

        void WarnIfCapacityShort(int startingHp)
        {
            if (_loggedCapacity)
                return;

            int n = cards != null ? cards.Length : 0;
            if (startingHp > n)
            {
                _loggedCapacity = true;
                Debug.LogError(
                    $"[SlotPokerHud] startingHp({startingHp}) > 카드 슬롯({n}). 추가 카드는 만들지 않음. 프리팹에 칸을 더 두세요.",
                    this);
                return;
            }

            if (landPads != null && landPads.Length != n)
            {
                _loggedCapacity = true;
                Debug.LogError(
                    $"[SlotPokerHud] landPads({landPads.Length})와 cards({n}) 개수가 다릅니다. 가로 테두리와 카드를 같게 두세요.",
                    this);
            }
        }

        static void SetOverlay(Image image, bool on)
        {
            if (image == null)
                return;

            image.gameObject.SetActive(on);
        }

        static void ApplyCard(CardBind card, CardMode mode, int pad)
        {
            if (card == null)
                return;

            bool show = mode != CardMode.Hidden;
            GameObject root = card.root;
            if (root == null && card.animator != null)
                root = card.animator.gameObject;

            if (root != null)
                root.SetActive(show);

            if (!show)
                return;

            if (card.front != null)
                card.front.SetActive(mode != CardMode.FaceDown);

            if (card.back != null)
                card.back.SetActive(mode == CardMode.FaceDown);

            Animator an = card.animator;
            if (an == null)
                return;

            an.SetInteger(PadHash, pad);
            an.SetInteger(ModeHash, (int)mode);
        }
    }
}
