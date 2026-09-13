using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        void TickRoundTimer(float dt)
        {
            float duration = Mathf.Max(0f, roundDuration);
            if (duration <= 0f)
                return;

            _roundElapsed += dt;
            if (_roundElapsed >= duration)
                _roundElapsed = duration;

            RefreshRoundTimer();

            if (_roundElapsed >= duration)
                CompleteSession();
        }

        void RefreshRoundTimer()
        {
            if (roundTimerFill == null)
                return;

            float duration = Mathf.Max(0f, roundDuration);
            if (duration <= 0f)
            {
                roundTimerFill.fillAmount = 0f;
                return;
            }

            roundTimerFill.fillAmount = Mathf.Clamp01(_roundElapsed / duration);
        }
    }
}
