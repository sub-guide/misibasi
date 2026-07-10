using TMPro;
using UnityEngine;

namespace MiniParty.Result
{
    /// <summary>슬롯 <c>GameOverAnchor</c> 에 두는 GAME OVER 패널.</summary>
    [DisallowMultipleComponent]
    public sealed class ResultGameOverPanel : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text gameOverText;
        [SerializeField] TMP_Text playerNumberText;

        void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            HideImmediate();
        }

        public void Show(int slotIndex)
        {
            gameObject.SetActive(true);

            if (gameOverText != null)
                gameOverText.text = "GAME OVER";

            if (playerNumberText != null)
                playerNumberText.text = $"P{slotIndex + 1}";

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
        }
    }
}
