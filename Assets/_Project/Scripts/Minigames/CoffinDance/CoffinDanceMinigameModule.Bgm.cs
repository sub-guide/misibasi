using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        [Header("BGM")]
        [Tooltip("본게임 BGM. 비우면 무음.")]
        [SerializeField] AudioClip mainBgmClip;

        [Tooltip("BGM 전용 AudioSource. 비우면 런타임에 보조 AudioSource 생성.")]
        [SerializeField] AudioSource mainBgmSource;

        [Tooltip("BGM 볼륨(0~1).")]
        [Range(0f, 1f)]
        [SerializeField] float mainBgmVolume = DefaultMainBgmVolume;

        [Tooltip("BGM 클립에서 재생을 시작할 위치(초). 0이면 곡의 처음. Begin 후 루프.")]
        [SerializeField, Min(0f)]
        float bgmStartTimeSeconds = 0f;

        void EnsureMainBgmAudioSource()
        {
            if (_mainBgmRuntime != null)
                return;

            if (mainBgmSource != null)
            {
                _mainBgmRuntime = mainBgmSource;
            }
            else
            {
                AudioSource[] arr = GetComponents<AudioSource>();
                if (arr.Length >= 1 && sfxSource != null && arr[0] != sfxSource)
                    _mainBgmRuntime = arr[0];
                else if (arr.Length >= 2)
                    _mainBgmRuntime = arr[1];
                else
                    _mainBgmRuntime = gameObject.AddComponent<AudioSource>();
            }

            _mainBgmRuntime.playOnAwake = false;
            _mainBgmRuntime.loop = true;
            _mainBgmRuntime.spatialBlend = 0f;
            _mainBgmRuntime.volume = mainBgmVolume;
            _mainBgmRuntime.pitch = 1f;
        }

        void StopMainBgm()
        {
            AudioSource s = mainBgmSource != null ? mainBgmSource : _mainBgmRuntime;
            if (s == null)
                return;

            s.Stop();
            s.clip = null;
            s.pitch = 1f;
        }

        void UpdateMainBgm()
        {
            if (!_running || _completing)
            {
                StopMainBgm();
                return;
            }

            if (mainBgmClip == null)
            {
                StopMainBgm();
                return;
            }

            EnsureMainBgmAudioSource();
            if (_mainBgmRuntime == null)
                return;

            _mainBgmRuntime.volume = mainBgmVolume;
            _mainBgmRuntime.pitch = 1f;

            if (_mainBgmRuntime.clip == mainBgmClip && _mainBgmRuntime.isPlaying)
                return;

            _mainBgmRuntime.Stop();
            _mainBgmRuntime.clip = mainBgmClip;
            _mainBgmRuntime.loop = true;
            _mainBgmRuntime.Play();
            _mainBgmRuntime.time = ClampBgmStartTime(mainBgmClip);
        }

        float ClampBgmStartTime(AudioClip clip)
        {
            if (clip == null || clip.length <= 0.05f)
                return 0f;

            return Mathf.Clamp(bgmStartTimeSeconds, 0f, clip.length - 0.05f);
        }
    }
}
