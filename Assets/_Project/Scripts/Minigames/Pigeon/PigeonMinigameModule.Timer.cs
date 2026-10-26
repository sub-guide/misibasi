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

            AdvanceRoundBuffer();
            RefreshRoundTimer();

            if (_roundElapsed >= duration)
                CompleteSession();
        }

        void ResetRoundBuffer()
        {
            float duration = Mathf.Max(0f, roundDuration);
            _bufferSeconds = duration <= 0f
                ? 0f
                : Mathf.Min(Mathf.Max(0f, bufferFirstChunkSeconds), duration);
            _bufferJumpAt = _roundElapsed + NextBufferJumpInterval();
        }

        void AdvanceRoundBuffer()
        {
            float duration = Mathf.Max(0f, roundDuration);
            if (duration <= 0f)
            {
                _bufferSeconds = 0f;
                return;
            }

            if (_roundElapsed >= duration)
            {
                _bufferSeconds = duration;
                return;
            }

            for (var guard = 0; guard < 8 && _bufferSeconds < duration; guard++)
            {
                bool jumpDue = _roundElapsed >= _bufferJumpAt;
                bool caught = _roundElapsed >= _bufferSeconds;
                if (!jumpDue && !caught)
                    break;

                float chunk = NextBufferChunkSeconds();
                if (chunk <= 0f)
                    break;

                _bufferSeconds = Mathf.Min(duration, _bufferSeconds + chunk);
                _bufferJumpAt = _roundElapsed + NextBufferJumpInterval();
            }
        }

        float NextBufferChunkSeconds()
        {
            float min = bufferChunkMinSeconds;
            float max = bufferChunkMaxSeconds;
            if (max < min)
                (min, max) = (max, min);
            if (max <= 0f)
                return 0f;
            min = Mathf.Max(0f, min);
            return Random.Range(min, max);
        }

        float NextBufferJumpInterval()
        {
            float min = bufferJumpIntervalMinSeconds;
            float max = bufferJumpIntervalMaxSeconds;
            if (max < min)
                (min, max) = (max, min);
            min = Mathf.Max(0f, min);
            max = Mathf.Max(min, max);
            return Random.Range(min, max);
        }

        void RefreshRoundTimer()
        {
            float duration = Mathf.Max(0f, roundDuration);
            if (duration > 0f && _roundElapsed >= duration)
                _bufferSeconds = duration;

            if (roundTimerFill != null)
            {
                roundTimerFill.fillAmount = duration <= 0f
                    ? 0f
                    : Mathf.Clamp01(_roundElapsed / duration);
            }

            if (roundTimerBuffer != null)
            {
                roundTimerBuffer.fillAmount = duration <= 0f
                    ? 0f
                    : Mathf.Clamp01(_bufferSeconds / duration);
            }

            if (roundTimerLabel != null)
                roundTimerLabel.text = FormatRoundClock(_roundElapsed) + "/" + FormatRoundClock(duration);

            MovePlayheadAlongRound();
        }

        void MovePlayheadAlongRound()
        {
            if (roundTimerPlayhead == null)
                return;

            float duration = Mathf.Max(0f, roundDuration);
            float t = duration <= 0f ? 0f : Mathf.Clamp01(_roundElapsed / duration);
            Vector2 pos = roundTimerPlayhead.anchoredPosition;
            pos.x = Mathf.Lerp(playheadStartX, playheadEndX, t);
            roundTimerPlayhead.anchoredPosition = pos;
        }

        static string FormatRoundClock(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = total / 60;
            int secs = total % 60;
            return $"{minutes}:{secs:00}";
        }
    }
}
