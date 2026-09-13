using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
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

            TickBeatClock();
        }

        public void RequestEarlyExit()
        {
            if (_running)
                CompleteSession();
        }
    }
}
