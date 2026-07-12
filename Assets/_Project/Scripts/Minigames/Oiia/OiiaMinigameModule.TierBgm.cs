using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
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

        /// <summary>
        /// 원작 밈 트랙 단일 BGM. 본게임 시작부터 루프. 티어와 무관.
        /// </summary>
        void UpdateTierBgm()
        {
            if (!_running || _ctx.IsPractice)
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
        }
    }
}
