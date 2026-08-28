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

            TickDevGodModeToggle();

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

                if (_remainingMainTime <= 0f)
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

        void LateUpdate()
        {
            if (!_running || _completing || _slots == null)
                return;

            if (_flowState != CdFlowState.Playing)
                return;

            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                SnapAttachedCoffinToShoulders(i, ref _slots[i]);
            });
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
    }
}
