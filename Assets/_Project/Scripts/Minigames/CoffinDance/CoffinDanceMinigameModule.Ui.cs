using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void FlushAllUi()
        {
            UpdateCentralTimerUi();
            ForEachSlot(FlushSlotUi);
        }

        void FlushSlotUi(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            bool participated = _participatedMask[i];

            if (bind.PlayerLabelText != null)
                bind.PlayerLabelText.text = $"{i + 1}P";

            if (bind.ScoreText != null)
            {
                if (!participated)
                    bind.ScoreText.text = string.Empty;
                else
                    bind.ScoreText.text = $"{_slots[i].ScoreSum}";
            }
        }

        void UpdateCentralTimerUi()
        {
            if (mainRoundTimerCentralTop == null)
                return;

            mainRoundTimerCentralTop.gameObject.SetActive(true);
            int sec = Mathf.CeilToInt(_remainingMainTime);
            mainRoundTimerCentralTop.text = $"{sec}";
        }

        void SetEliminatedUi(int i, bool eliminated)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null || bind.EliminatedText == null)
                return;

            bind.EliminatedText.gameObject.SetActive(eliminated);
            if (eliminated)
                bind.EliminatedText.text = "ELIMINATED";
        }
    }
}
