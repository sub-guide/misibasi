using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public void Tick()
        {
            if (!_running || _completing)
                return;

            if (EscapePressed())
            {
                UpdateMainTimerUi();
                CompleteSession();
                return;
            }

            TickDevGodModeToggle();

            if (!IsDevGodModeActive())
            {
                _remainingMainTime -= Time.deltaTime;

                if (_remainingMainTime <= 0f)
                {
                    UpdateMainTimerUi();
                    CompleteSession();
                    return;
                }
            }

            UpdateGlobalTierFeverMode();

            ForEachSlot(i =>
            {
                if (_aliveMask[i])
                    TickGameplay(i);

                UpdateCatAnimationMode(i);
                UpdateSlotUiShake(i);
                TickSpotlight(i);
                TickStageBackground(i);
                TickSpeakers(i);
                TickCrowd(i);
                FlushUi(i);
            });

            UpdateTierBgm();
            UpdateMainTimerUi();
        }

        public void RequestEarlyExit()
        {
            if (_running)
                CompleteSession();
        }
    }
}
