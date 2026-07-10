using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("타이머 (본게임만 표시)")]
        [Tooltip("화면 중앙 상단에 두는 TMP. 연습 모드에선 비표시.")]
        [FormerlySerializedAs("debugTimerText")]
        [SerializeField] TMP_Text mainRoundTimerCentralTop;

        void UpdateMainTimerUi()
        {
            if (mainRoundTimerCentralTop == null)
                return;

            if (_ctx.IsPractice)
            {
                mainRoundTimerCentralTop.gameObject.SetActive(false);
                return;
            }

            mainRoundTimerCentralTop.gameObject.SetActive(true);
            mainRoundTimerCentralTop.text = $"TIME {Mathf.Max(0f, _remainingMainTime):0.0}";
        }
    }
}
