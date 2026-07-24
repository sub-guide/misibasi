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
            bool alive = _aliveMask[i];

            if (bind.PlayerLabelText != null)
                bind.PlayerLabelText.text = $"{i + 1}P";

            if (bind.ScoreText != null)
            {
                if (!participated)
                    bind.ScoreText.text = string.Empty;
                else if (_ctx.IsPractice)
                    bind.ScoreText.text = "-";
                else
                    bind.ScoreText.text = $"{_slots[i].ScoreSum}";
            }

            if (bind.PracticeReadyText != null)
            {
                bool show = _ctx.IsPractice && alive && _practiceReady[i];
                bind.PracticeReadyText.gameObject.SetActive(show);
                if (show)
                    bind.PracticeReadyText.text = "READY";
            }

            UpdateBalanceGauge(i, bind);
        }

        void UpdateBalanceGauge(int i, CoffinDanceSlotBindings bind)
        {
            if (bind.BalanceGaugeFill == null)
                return;

            if (!_participatedMask[i] || _slots == null)
            {
                bind.BalanceGaugeFill.fillAmount = bind.GaugeFillAtCenter;
                return;
            }

            float limit = StumbleLimitDegrees * Mathf.Deg2Rad;
            float t = Mathf.Clamp(_slots[i].ThetaRad / limit, -1f, 1f);
            // -1 → 0, 0 → center, +1 → 1 (또는 GaugeFillAtLimit 쪽)
            float center = bind.GaugeFillAtCenter;
            float edge = Mathf.Abs(bind.GaugeFillAtLimit - center) > 0.01f
                ? bind.GaugeFillAtLimit
                : (t >= 0f ? 1f : 0f);

            if (t >= 0f)
                bind.BalanceGaugeFill.fillAmount = Mathf.Lerp(center, edge >= center ? edge : 1f, t);
            else
                bind.BalanceGaugeFill.fillAmount = Mathf.Lerp(center, edge <= center ? edge : 0f, -t);
        }

        void UpdateCentralTimerUi()
        {
            if (mainRoundTimerCentralTop == null)
                return;

            if (_ctx.IsPractice)
            {
                mainRoundTimerCentralTop.gameObject.SetActive(true);
                mainRoundTimerCentralTop.text = "PRACTICE";
                return;
            }

            mainRoundTimerCentralTop.gameObject.SetActive(true);
            int sec = Mathf.CeilToInt(_remainingMainTime);
            mainRoundTimerCentralTop.text = $"{sec}";
        }

        void UpdatePhaseLabelUi()
        {
            if (phaseLabelText == null)
                return;

            if (_ctx.IsPractice)
            {
                phaseLabelText.text = "연습";
                return;
            }

            phaseLabelText.text = _phase switch
            {
                CdPhase.Phase1 => "PHASE 1",
                CdPhase.Phase2 => "PHASE 2",
                CdPhase.Phase3 => "PHASE 3",
                _ => "PHASE 4 ×2"
            };
        }

        void ShowJumpPrompt(int i, bool isDouble)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null || bind.JumpPromptText == null)
                return;

            bind.JumpPromptText.gameObject.SetActive(true);
            bind.JumpPromptText.text = isDouble ? "JUMP! JUMP!" : "JUMP!";
        }

        void HideJumpPrompt(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null || bind.JumpPromptText == null)
                return;

            bind.JumpPromptText.gameObject.SetActive(false);
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
