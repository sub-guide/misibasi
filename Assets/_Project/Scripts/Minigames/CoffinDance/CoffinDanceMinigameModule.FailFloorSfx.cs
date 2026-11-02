using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        [Header("FailFloor 연출 SFX")]
        [Tooltip("FailFloor 복구 연출(`failFloorRecoverDuration`) 동안만 재생. 종료 시 Stop. 비우면 무음.")]
        [SerializeField] AudioClip failFloorSfxClip;

        [Tooltip("Fail SFX 전용 AudioSource. 비우면 런타임에 보조 AudioSource 생성.")]
        [SerializeField] AudioSource failFloorSfxSource;

        [Tooltip("Fail SFX 볼륨(0~1).")]
        [Range(0f, 1f)]
        [SerializeField] float failFloorSfxVolume = DefaultFailFloorSfxVolume;

        [Tooltip("클립에서 재생을 시작할 위치(초). 0이면 처음부터. 복구 중에만 재생(루프 없음).")]
        [SerializeField, Min(0f)]
        float failFloorSfxStartTimeSeconds = 0f;

        AudioSource _failFloorSfxRuntime;

        void EnsureFailFloorSfxAudioSource()
        {
            if (_failFloorSfxRuntime != null)
                return;

            if (failFloorSfxSource != null)
            {
                _failFloorSfxRuntime = failFloorSfxSource;
            }
            else
            {
                AudioSource bgm = mainBgmSource != null ? mainBgmSource : _mainBgmRuntime;
                AudioSource[] arr = GetComponents<AudioSource>();
                for (int n = 0; n < arr.Length; n++)
                {
                    AudioSource candidate = arr[n];
                    if (candidate == null || candidate == bgm || candidate == sfxSource)
                        continue;

                    _failFloorSfxRuntime = candidate;
                    break;
                }

                if (_failFloorSfxRuntime == null)
                    _failFloorSfxRuntime = gameObject.AddComponent<AudioSource>();
            }

            _failFloorSfxRuntime.playOnAwake = false;
            _failFloorSfxRuntime.loop = false;
            _failFloorSfxRuntime.spatialBlend = 0f;
            _failFloorSfxRuntime.pitch = 1f;
        }

        void BeginFailFloorSfxScope()
        {
            if (failFloorSfxClip == null)
                return;

            _failFloorSfxScopeCount++;
            if (_failFloorSfxScopeCount != 1)
                return;

            EnsureFailFloorSfxAudioSource();
            if (_failFloorSfxRuntime == null)
                return;

            _failFloorSfxRuntime.Stop();
            _failFloorSfxRuntime.clip = failFloorSfxClip;
            _failFloorSfxRuntime.loop = false;
            _failFloorSfxRuntime.volume = failFloorSfxVolume;
            _failFloorSfxRuntime.pitch = 1f;
            _failFloorSfxRuntime.Play();
            _failFloorSfxRuntime.time = ClampFailFloorSfxStartTime(failFloorSfxClip);
        }

        void EndFailFloorSfxScope()
        {
            if (failFloorSfxClip == null)
                return;

            if (_failFloorSfxScopeCount <= 0)
                return;

            _failFloorSfxScopeCount--;
            if (_failFloorSfxScopeCount > 0)
                return;

            StopFailFloorSfxImmediate();
        }

        void StopFailFloorSfxImmediate()
        {
            _failFloorSfxScopeCount = 0;
            AudioSource s = failFloorSfxSource != null ? failFloorSfxSource : _failFloorSfxRuntime;
            if (s == null)
                return;

            s.Stop();
            s.clip = null;
            s.pitch = 1f;
        }

        float ClampFailFloorSfxStartTime(AudioClip clip)
        {
            if (clip == null || clip.length <= 0.05f)
                return 0f;

            return Mathf.Clamp(failFloorSfxStartTimeSeconds, 0f, clip.length - 0.05f);
        }
    }
}
