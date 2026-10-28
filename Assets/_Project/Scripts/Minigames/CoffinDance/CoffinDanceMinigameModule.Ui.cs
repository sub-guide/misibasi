using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void FlushAllUi()
        {
            UpdateCentralTimerUi();
            UpdatePhaseLabelUi();
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

        void UpdatePhaseLabelUi()
        {
            if (phaseLabelText == null)
                return;

            phaseLabelText.text = _phase switch
            {
                CdPhase.Phase1 => "PHASE 1",
                CdPhase.Phase2 => "PHASE 2",
                CdPhase.Phase3 => "PHASE 3",
                _ => "PHASE 4 ×2"
            };
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
