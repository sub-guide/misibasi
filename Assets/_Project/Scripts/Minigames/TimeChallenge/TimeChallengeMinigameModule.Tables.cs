using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        [SerializeField] TimeChallengeTableButton[] playerTables = new TimeChallengeTableButton[SlotCount];

        [Header("스톱워치 효과음")]
        [SerializeField] AudioClip stopwatchClickClip;
        [SerializeField] [Range(0f, 1f)] float stopwatchClickVolume = 1f;
        [SerializeField] AudioClip heartbeatClip;
        [SerializeField] [Range(0f, 3f)] float heartbeatVolume = 2.2f;

        [SerializeField] AudioSource stopwatchClickSource;
        [SerializeField] AudioSource heartbeatSource;

        void PlayStopwatchClick()
        {
            if (stopwatchClickClip == null)
                return;

            if (stopwatchClickSource == null) return;
            stopwatchClickSource.PlayOneShot(stopwatchClickClip, Mathf.Clamp01(stopwatchClickVolume));
        }

        void PlayHeartbeat()
        {
            if (heartbeatClip == null)
                return;

            if (heartbeatSource == null) return;
            heartbeatSource.clip = heartbeatClip;
            heartbeatSource.volume = Mathf.Clamp(heartbeatVolume, 0f, 3f);
            heartbeatSource.Stop();
            heartbeatSource.Play();
        }

        void TickHeartbeat()
        {
            if (_phase != Phase.Running || _roundStartRealtime <= 0d)
                return;
            if (heartbeatClip == null)
                return;

            double elapsed = Time.realtimeSinceStartupAsDouble - _roundStartRealtime;
            int displaySeconds = TimeChallengeLedClock.ToCentiseconds(elapsed) / 100;
            if (displaySeconds < 1 || displaySeconds <= _lastHeartbeatSecond)
                return;

            _lastHeartbeatSecond = displaySeconds;
            PlayHeartbeat();
        }

        [Header("목표 시간 적중 영상")]
        [SerializeField] TimeChallengePerfectVideo[] perfectHitVideos = new TimeChallengePerfectVideo[SlotCount];
        [SerializeField] float perfectHitVolume = 2.4f;
        [SerializeField] float perfectHitVideoSeconds = DefaultPerfectHitVideoSeconds;
        [SerializeField] float perfectHitRevealSeconds = DefaultPerfectHitRevealSeconds;

        [Header("실험")]
        [Tooltip("에디터 Play 한 판만. 라운드 1의 1P 기록을 목표시간 03:00으로 맞춥니다. 확인 후 끄세요.")]
        [SerializeField] bool debugForce1PRound1Perfect;

        void TickTableButtons()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                TimeChallengeTableButton table = GetArray(playerTables, i);
                if (table == null)
                    continue;

                table.SetPressed(IsClockButtonHeld(i));
            }
        }

        void StopPerfectHitVideos()
        {
            if (perfectHitVideos == null) return;
            foreach (var video in perfectHitVideos)
                if (video != null) video.Stop();
        }

        void StopGameplayAudio()
        {
            if (stopwatchClickSource != null) stopwatchClickSource.Stop();
            if (heartbeatSource != null) heartbeatSource.Stop();
        }

        void OnDisable()
        {
            StopPerfectHitVideos();
            StopGameplayAudio();
        }
    }
}
