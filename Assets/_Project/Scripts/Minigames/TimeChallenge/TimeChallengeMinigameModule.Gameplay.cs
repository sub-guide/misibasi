using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        void StartRound(int roundIndex)
        {
            _roundIndex = roundIndex;
            _phase = Phase.Briefing;
            _phaseElapsed = 0f;
            _roundStartRealtime = 0d;
            RollRoundTarget();

            for (var i = 0; i < SlotCount; i++)
            {
                _clockStarted[i] = false;
                _slotStartRealtime[i] = 0d;
                _stopped[i] = false;
                _records[i] = 0d;
                _errors[i] = 0d;
                _roundScores[i] = 0;
            }

            _perfectHitThisResult = false;
            _perfectHitVideoStarted = false;
            StopPerfectHitVideos();
            RefreshHud();
            ShowTargetLabelCloseup();
            PlayRoundAdvanceEffect();
        }

        void TickBriefing()
        {
            if (_phase != Phase.Briefing)
                return;

            float hold = _roundIndex == 0
                ? Mathf.Max(0.35f, briefingSeconds)
                : TargetCloseupSeconds;
            if (_phaseElapsed < hold)
                return;

            _phase = Phase.Countdown;
            _phaseElapsed = 0f;
            SettleTargetLabel();
        }

        void TickCountdown()
        {
            if (_phase != Phase.Countdown)
                return;

            float step = Mathf.Max(0.05f, countdownStepSeconds);
            if (_phaseElapsed < step * 3f)
                return;

            BeginRunning();
        }

        void BeginRunning()
        {
            _phase = Phase.Running;
            _phaseElapsed = 0f;
            double now = Time.realtimeSinceStartupAsDouble;
            _roundStartRealtime = now;

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_participatedMask[i])
                    continue;

                _clockStarted[i] = true;
                _slotStartRealtime[i] = now;
                _stopped[i] = false;
                _records[i] = 0d;
            }

            PlayStopwatchClick();
            _lastHeartbeatSecond = 0;
        }

        void TickRunningTimeout()
        {
            if (AllSlotsResolved())
                FinishRound();
        }

        void FinishRound()
        {
            if (_phase != Phase.Running)
                return;

            double target = GetRoundTargetSeconds();
            bool lastRound = _roundIndex >= RoundCount - 1;

            for (var i = 0; i < SlotCount; i++)
            {
                if (_clockStarted[i] || _stopped[i])
                    _participatedMask[i] = true;

                if (!_participatedMask[i] || !_stopped[i])
                    continue;

                _errors[i] = TimeChallengeLedClock.DisplayErrorCentiseconds(_records[i], target) / 100d;
            }

            TimeChallengeRanking.FillRoundScores(
                _participatedMask,
                _stopped,
                _errors,
                lastRound,
                _roundScores);
            TimeChallengeRanking.ApplyPerfectHitBonus(
                _participatedMask,
                _stopped,
                _records,
                target,
                _roundScores);

            for (var i = 0; i < SlotCount; i++)
            {
                if (_participatedMask[i])
                    _totalScores[i] += _roundScores[i];
            }

            PlayScoreBoardAwards();
            _perfectHitThisResult = HasPerfectHit(target);
            _perfectHitVideoStarted = false;

            _phase = Phase.Result;
            _phaseElapsed = 0f;
        }

        bool HasPerfectHit(double target)
        {
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_participatedMask[i] || !_stopped[i])
                    continue;

                if (TimeChallengeLedClock.DisplayErrorCentiseconds(_records[i], target) == 0)
                    return true;
            }

            return false;
        }

        void PlayPerfectHitVideos()
        {
            if (_perfectHitVideoStarted)
                return;

            _perfectHitVideoStarted = true;
            if (!_perfectHitThisResult)
                return;

            double target = GetRoundTargetSeconds();
            float playSeconds = Mathf.Max(0.2f, perfectHitVideoSeconds);
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_participatedMask[i] || !_stopped[i])
                    continue;

                if (TimeChallengeLedClock.DisplayErrorCentiseconds(_records[i], target) != 0)
                    continue;

                TimeChallengePerfectVideo video = GetArray(perfectHitVideos, i);
                if (video != null) video.Play(perfectHitVolume, playSeconds);
            }
        }

        void TickResultHold()
        {
            if (_perfectHitThisResult && !_perfectHitVideoStarted &&
                _phaseElapsed >= Mathf.Max(0f, perfectHitRevealSeconds))
            {
                PlayPerfectHitVideos();
            }

            float hold = Mathf.Max(0.2f, resultHoldSeconds);
            hold = Mathf.Max(hold, Mathf.Max(0.2f, scoreCountDuration) + 0.35f);
            if (_perfectHitThisResult)
            {
                float reveal = Mathf.Max(0f, perfectHitRevealSeconds);
                hold = Mathf.Max(hold, reveal + Mathf.Max(0.2f, perfectHitVideoSeconds));
            }
            if (_phaseElapsed < hold)
                return;

            if (_roundIndex >= RoundCount - 1)
            {
                CompleteSession();
                return;
            }

            StartRound(_roundIndex + 1);
        }

        bool AllSlotsResolved()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            double limit = GetRoundTimeoutSeconds();

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_clockStarted[i])
                    continue;

                if (_stopped[i])
                    continue;

                if (_roundStartRealtime <= 0d)
                    return false;

                if (now - _roundStartRealtime + 0.0000001d < limit)
                    return false;
            }

            return true;
        }

        void RollRoundTarget()
        {
            if (_roundIndex <= 0)
            {
                _roundTargetSeconds = System.Math.Max(0.01d, round1Seconds);
                return;
            }

            double min = System.Math.Max(0.01d, randomTargetMinSeconds);
            double max = System.Math.Max(min, randomTargetMaxSeconds);
            int minCs = Mathf.RoundToInt((float)(min * 100d));
            int maxCs = Mathf.RoundToInt((float)(max * 100d));
            if (maxCs < minCs)
                maxCs = minCs;

            int cs = minCs;
            if (maxCs > minCs)
            {
                cs = Random.Range(minCs, maxCs + 1);
                int prevCs = Mathf.RoundToInt((float)(_roundTargetSeconds * 100d));
                if (cs == prevCs)
                    cs = cs >= maxCs ? minCs : cs + 1;
            }

            _roundTargetSeconds = cs / 100d;
        }

        double GetRoundTargetSeconds()
        {
            return System.Math.Max(0.01d, _roundTargetSeconds);
        }

        double GetRoundTimeoutSeconds()
        {
            return GetRoundTargetSeconds() + System.Math.Max(0d, timeoutExtraSeconds);
        }

        int CountdownDisplayNumber()
        {
            float step = Mathf.Max(0.05f, countdownStepSeconds);
            int elapsedSteps = Mathf.FloorToInt(_phaseElapsed / step);
            int n = 3 - elapsedSteps;
            return n < 1 ? 1 : n;
        }
    }
}
