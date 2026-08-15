using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        public void Tick()
        {
            if (!_running || _completing)
                return;

            if (EscapePressed())
            {
                CompleteSession();
                return;
            }

            float dt = Time.deltaTime;

            if (_ctx.IsPractice)
                TickPracticeReadyAndMaybeStartMain();

            if (_flowState == CdFlowState.Ending)
            {
                _endDelayRemain -= dt;
                FlushAllUi();
                if (_endDelayRemain <= 0f)
                    CompleteSession();
                return;
            }

            if (!_ctx.IsPractice)
            {
                _elapsedMainTime += dt;
                _remainingMainTime = Mathf.Max(0f, MainDurationSeconds - _elapsedMainTime);
                RefreshPhaseParameters();

                if (_remainingMainTime <= 0f || CountAlive() == 0)
                {
                    BeginEndDelay();
                    FlushAllUi();
                    return;
                }
            }
            else
            {
                RefreshPhaseParameters();
            }

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                TickSlotGameplay(i, dt);
            });

            FlushAllUi();
        }

        public void RequestEarlyExit()
        {
            if (_running)
                CompleteSession();
        }

        void BeginEndDelay()
        {
            if (_flowState != CdFlowState.Playing)
                return;

            _flowState = CdFlowState.Ending;
            _endDelayRemain = SessionEndDelaySeconds;
        }

        int CountAlive()
        {
            var n = 0;
            for (var i = 0; i < SlotCount; i++)
            {
                if (_aliveMask[i])
                    n++;
            }

            return n;
        }
    }
}
