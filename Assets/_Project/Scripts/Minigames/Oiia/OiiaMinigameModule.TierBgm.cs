using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("티어")]
        [Tooltip("T2에서 줄어드는 본게임 시간(초). T1 동안에는 이 숫자가 화면에 멈춘 채로 보인다.")]
        [SerializeField, Min(MainRoundMinSeconds)]
        float mainRoundDurationSeconds = 26f;

        [Tooltip("T1 빌드업 길이(초). 우주 배경만 보이고 입력·Highlight·연출은 없다. 끝나면 T2에서 본게임 타이머가 줄어든다. 0이면 T1 없이 바로 T2.")]
        [SerializeField, Min(0f)]
        float tier1DurationSeconds = 5f;

        [Tooltip("BGM 클립에서 재생을 시작할 위치(초). 0이면 곡의 처음. 라운드가 시작되면 바로 이 지점부터 루프한다.")]
        [SerializeField, Min(0f)]
        float bgmStartTimeSeconds = 0f;

        void EnsureTierBgmAudioSource()
        {
            if (_tierBgmRuntime != null)
                return;

            if (tierBgmSource != null)
            {
                _tierBgmRuntime = tierBgmSource;
            }
            else
            {
                AudioSource[] arr = GetComponents<AudioSource>();
                if (arr.Length >= 2)
                    _tierBgmRuntime = arr[1];
                else
                    _tierBgmRuntime = gameObject.AddComponent<AudioSource>();
            }

            _tierBgmRuntime.playOnAwake = false;
            _tierBgmRuntime.loop = true;
            _tierBgmRuntime.spatialBlend = 0f;
            _tierBgmRuntime.volume = tierBgmVolume;
            _tierBgmRuntime.pitch = 1f;
        }

        void StopTierBgm()
        {
            AudioSource s = tierBgmSource != null ? tierBgmSource : _tierBgmRuntime;
            if (s == null)
                return;

            s.Stop();
            s.clip = null;
            s.pitch = 1f;
        }

        /// <summary>라운드가 돌아가는 동안 <see cref="mainBgmClip"/>을 <see cref="bgmStartTimeSeconds"/> 지점부터 루프.</summary>
        void UpdateTierBgm()
        {
            if (!_running)
            {
                StopTierBgm();
                return;
            }

            AudioClip want = mainBgmClip;
            if (want == null)
            {
                StopTierBgm();
                return;
            }

            EnsureTierBgmAudioSource();
            if (_tierBgmRuntime == null)
                return;

            _tierBgmRuntime.volume = tierBgmVolume;
            _tierBgmRuntime.pitch = 1f;

            if (_tierBgmRuntime.clip == want && _tierBgmRuntime.isPlaying)
                return;

            _tierBgmRuntime.Stop();
            _tierBgmRuntime.clip = want;
            _tierBgmRuntime.loop = true;
            _tierBgmRuntime.Play();
            _tierBgmRuntime.time = ClampBgmStartTime(want);
        }

        float ClampBgmStartTime(AudioClip clip)
        {
            if (clip == null || clip.length <= 0.05f)
                return 0f;

            return Mathf.Clamp(bgmStartTimeSeconds, 0f, clip.length - 0.05f);
        }
    }
}
