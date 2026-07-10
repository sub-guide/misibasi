using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
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

            if (_flowState == RbcFlowState.SpeedUp)
            {
                _speedUpTimer -= Time.unscaledDeltaTime;
                if (_speedUpTimer <= 0f)
                {
                    HideSpeedUpOverlay();
                    _phaseNumber = 2;
                    BeginPhaseIntro();
                    StartCurrentSegmentAudio();
                }

                FlushAllUi();
                return;
            }

            if (_flowState == RbcFlowState.Complete)
                return;

            if (!_segmentAudioStarted)
                StartCurrentSegmentAudio();

            TickSegmentAudio();
            TickGameplayInput();
            FlushAllUi();
        }

        public void RequestEarlyExit()
        {
            if (_running)
                CompleteSession();
        }

        static bool EscapePressed() =>
            UnityEngine.Input.GetKeyDown(KeyCode.Escape);
    }
}
