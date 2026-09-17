using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        enum ScorePopupKind
        {
            Success,
            Fail,
            Bonus
        }

        void HideAllScorePopups()
        {
            if (!_slotUiReady)
                return;

            for (var i = 0; i < SlotCount; i++)
                HideSlotScorePopups(i);
        }

        void HideSlotScorePopups(int slotIndex)
        {
            ref SlotUiCache ui = ref _slotUi[slotIndex];
            SetPopupOff(ui.PopupSuccess);
            SetPopupOff(ui.PopupFail);
            SetPopupOff(ui.PopupBonus);
            ui.PopupHideTime = 0;
        }

        static void SetPopupOff(Animator animator)
        {
            if (animator != null && animator.gameObject.activeSelf)
                animator.gameObject.SetActive(false);
        }

        void PlayScorePopup(int slotIndex, ScorePopupKind kind)
        {
            if (!_slotUiReady || !_aliveMask[slotIndex])
                return;

            ref SlotUiCache ui = ref _slotUi[slotIndex];
            Animator animator = kind switch
            {
                ScorePopupKind.Success => ui.PopupSuccess,
                ScorePopupKind.Fail => ui.PopupFail,
                ScorePopupKind.Bonus => ui.PopupBonus,
                _ => null
            };

            HideSlotScorePopups(slotIndex);
            if (animator == null)
                return;

            animator.gameObject.SetActive(true);
            animator.Play(ScorePopupAnimStateHash, 0, 0f);
            ui.PopupHideTime = Time.unscaledTimeAsDouble + scorePopupVisibleSeconds;
        }

        void TickScorePopupHides()
        {
            if (!_slotUiReady)
                return;

            double now = Time.unscaledTimeAsDouble;
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                    continue;

                ref SlotUiCache ui = ref _slotUi[i];
                if (ui.PopupHideTime <= 0 || now < ui.PopupHideTime)
                    continue;

                HideSlotScorePopups(i);
            }
        }
    }
}
