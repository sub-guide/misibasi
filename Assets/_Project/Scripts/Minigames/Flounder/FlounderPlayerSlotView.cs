using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Tooltip("GifVisual 전체에 곱해지는 색. (단일 스프라이트라 옷만 분리 색칠은 불가 — 피부·머리도 함께 틴트됨)")]
        [SerializeField] Color gifCharacterTint = Color.white;

        public int SlotIndex => slotIndex;
        public RectTransform SlotLabelRect =>
            slotLabel != null ? slotLabel.rectTransform : transform as RectTransform;
        public Transform CharacterRoot => characterRoot;
        public FlounderGauge Gauge => gauge;
        public FlounderDance Dance => dance;
        public TMP_FontAsset LabelFont => slotLabel != null ? slotLabel.font : null;

        public void BindDance(FlounderDance value) => dance = value;
        public float LabelFontSize => slotLabel != null ? slotLabel.fontSize : 60f;

        void Awake() => ApplyGifCharacterTint();

        public void ApplyGifCharacterTint()
        {
            if (characterRoot == null)
                return;

            Transform visual = characterRoot.Find("GifVisual");
            if (visual == null)
                return;

            var renderer = visual.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.color = gifCharacterTint;
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
