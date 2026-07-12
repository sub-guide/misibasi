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

            if (_ctx.IsPractice)
                TickPracticeReadyAndMaybeStartMain();

            if (!_ctx.IsPractice && !IsDevGodModeActive())
            {
                _remainingMainTime -= Time.deltaTime;

                if (_remainingMainTime <= 0f)
                {
                    UpdateMainTimerUi();
                    CompleteSession();
                    return;
                }
            }

            ForEachSlot(i =>
            {
                TickMeta(i);

                if (_aliveMask[i])
                    TickGameplay(i);

                if (!_ctx.IsPractice)
                {
                    UpdateCatAnimationMode(i);
                    UpdateSlotUiShake(i);
                }

                TickSpotlight(i);
                TickStageBackground(i);
                TickSpeakers(i);
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
