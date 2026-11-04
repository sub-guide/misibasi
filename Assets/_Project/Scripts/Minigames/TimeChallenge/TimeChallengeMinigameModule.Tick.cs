using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        public void Tick()
        {
            if (!_running || _completing)
                return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                CompleteSession();
                return;
            }

            float dt = Time.unscaledDeltaTime;
            _phaseElapsed += dt;

            switch (_phase)
            {
                case Phase.Briefing:
                    TickTableButtons();
                    TickBriefing();
                    break;
                case Phase.Countdown:
                    TickTableButtons();
                    TickCountdown();
                    break;
                case Phase.Running:
                    TickInput();
                    TickDebugForce1PRound1Perfect();
                    TickRunningTimeout();
                    break;
                case Phase.Result:
                    TickTableButtons();
                    TickResultHold();
                    break;
            }

            TickOperatorScoreGrants();
            TickScoreBoards(dt);
            TickTargetLabel(dt);
            RefreshHud();
        }

        public void RequestEarlyExit()
        {
            CompleteSession();
        }
    }
}
