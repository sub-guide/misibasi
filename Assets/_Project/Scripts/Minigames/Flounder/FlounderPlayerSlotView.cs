using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    /// <summary>
    /// 슬롯별 캐릭터 자리 + 개인 게이지 UI.
    /// </summary>
    public sealed class FlounderPlayerSlotView : MonoBehaviour
    {
        [SerializeField] int slotIndex;
        [SerializeField] Transform characterRoot;
        [SerializeField] GameObject inactiveRoot;
        [SerializeField] FlounderGauge gauge;
        [SerializeField] FlounderDance dance;
        [SerializeField] TextMeshProUGUI slotLabel;
        [SerializeField] TextMeshProUGUI scoreLabel;

        [Tooltip("Character_*P / GifVisual 의 SpriteRenderer.")]
        [SerializeField] SpriteRenderer gifRenderer;
        [Tooltip("GifVisual 전체에 곱해지는 색. (단일 스프라이트라 옷만 분리 색칠은 불가 — 피부·머리도 함께 틴트됨)")]
        [SerializeField] Color gifCharacterTint = Color.white;

        public int SlotIndex => slotIndex;
        public RectTransform SlotLabelRect =>
            slotLabel != null ? slotLabel.rectTransform : transform as RectTransform;
        public FlounderGauge Gauge => gauge;
        public FlounderDance Dance => dance;

        void Awake()
        {
            if (gifRenderer != null)
                gifRenderer.color = gifCharacterTint;
        }

        public void SetActiveSlot(bool playerParticipating)
        {
            gameObject.SetActive(true);

            if (inactiveRoot != null)
                inactiveRoot.SetActive(!playerParticipating);

            if (characterRoot != null)
                characterRoot.gameObject.SetActive(playerParticipating);
        }

        public void SetLabel(string text)
        {
            if (slotLabel != null)
                slotLabel.text = text;
        }

        public void SetScore(int score)
        {
            if (scoreLabel != null)
                scoreLabel.text = "Score: " + score.ToString("N0");
        }
    }
}
