using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("피버 사운드")]
        [Tooltip("피버 슬롯이 하나라도 있는 동안 반복 재생할 사운드.")]
        [SerializeField] AudioClip feverScreamClip;

        [Tooltip("피버 Scream 전용 AudioSource. 비우면 런타임에 자동 생성.")]
        [SerializeField] AudioSource feverScreamSource;

        AudioSource _feverScreamRuntime;

        void EnsureFeverScreamAudioSource()
        {
            if (_feverScreamRuntime != null)
                return;

            _feverScreamRuntime = feverScreamSource != null
                ? feverScreamSource
                : gameObject.AddComponent<AudioSource>();

            _feverScreamRuntime.playOnAwake = false;
            _feverScreamRuntime.loop = true;
            _feverScreamRuntime.spatialBlend = 0f;
            _feverScreamRuntime.pitch = 1f;
        }

        void RefreshFeverScreamAudio()
        {
            if (!_running || feverScreamClip == null || !HasAnyActiveFever())
            {
                StopFeverScreamAudio();
                return;
            }

            EnsureFeverScreamAudioSource();
            if (_feverScreamRuntime == null)
                return;

            if (_feverScreamRuntime.clip == feverScreamClip && _feverScreamRuntime.isPlaying)
                return;

            _feverScreamRuntime.Stop();
            _feverScreamRuntime.clip = feverScreamClip;
            _feverScreamRuntime.loop = true;
            _feverScreamRuntime.Play();
        }

        bool HasAnyActiveFever()
        {
            if (_slots == null)
                return false;

            int count = Mathf.Min(SlotCount, _slots.Length);
            for (var i = 0; i < count; i++)
            {
                if (_slots[i].FeverRemaining > 0f)
                    return true;
            }

            return false;
        }

        void StopFeverScreamAudio()
        {
            AudioSource source = feverScreamSource != null
                ? feverScreamSource
                : _feverScreamRuntime;

            if (source == null)
                return;

            source.Stop();
            source.clip = null;
        }
    }
}
