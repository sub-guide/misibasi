using UnityEngine;
using UnityEngine.Audio;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("피버 OIIA 떼창")]
        [Tooltip("피버 중 패턴 스텝 1회에 겹쳐 재생할 음성 수. 첫 레이어는 원본 피치·지연 0으로 재생.")]
        [Range(1, 12)]
        [SerializeField] int feverChantLayerCount = 12;

        [Tooltip("떼창 레이어 AudioSource 풀 크기. 빠른 연타 시 이전 음성을 덜 끊으려면 늘린다.")]
        [Range(4, 64)]
        [SerializeField] int feverChantPoolSize = 32;

        [Tooltip("분산 레이어 최소 피치.")]
        [Range(0.25f, 3f)]
        [SerializeField] float feverChantPitchMin = 0.98f;

        [Tooltip("분산 레이어 최대 피치.")]
        [Range(0.25f, 3f)]
        [SerializeField] float feverChantPitchMax = 1.02f;

        [Tooltip("레이어 최소 볼륨.")]
        [Range(0f, 1f)]
        [SerializeField] float feverChantVolumeMin = 0.5f;

        [Tooltip("레이어 최대 볼륨. 첫 원본 레이어도 이 볼륨을 사용.")]
        [Range(0f, 1f)]
        [SerializeField] float feverChantVolumeMax = 1f;

        [Tooltip("분산 레이어 최대 시작 지연(초).")]
        [Range(0f, 0.25f)]
        [SerializeField] float feverChantMaxStartDelay = 0.15f;

        Transform _feverChantPoolRoot;
        AudioSource[] _feverChantSources;
        int _feverChantSourceCursor;

        void PlayFeverChantStep(AudioClip clip)
        {
            if (clip == null)
                return;

            EnsureFeverChantPool();
            if (_feverChantSources == null || _feverChantSources.Length == 0)
                return;

            int layers = Mathf.Clamp(
                feverChantLayerCount,
                1,
                _feverChantSources.Length);

            float pitchLow = Mathf.Min(feverChantPitchMin, feverChantPitchMax);
            float pitchHigh = Mathf.Max(feverChantPitchMin, feverChantPitchMax);
            float volumeLow = Mathf.Min(feverChantVolumeMin, feverChantVolumeMax);
            float volumeHigh = Mathf.Max(feverChantVolumeMin, feverChantVolumeMax);
            float maxDelay = Mathf.Max(0f, feverChantMaxStartDelay);

            for (var layer = 0; layer < layers; layer++)
            {
                AudioSource source = AcquireFeverChantSource();
                if (source == null)
                    continue;

                source.Stop();
                source.clip = clip;
                source.loop = false;

                if (layer == 0)
                {
                    // 중심 음성을 선명하게 남기고 나머지 레이어만 군중처럼 분산.
                    source.pitch = 1f;
                    source.volume = volumeHigh;
                    source.Play();
                }
                else
                {
                    source.pitch = Random.Range(pitchLow, pitchHigh);
                    source.volume = Random.Range(volumeLow, volumeHigh);
                    source.PlayDelayed(Random.Range(0f, maxDelay));
                }
            }
        }

        void EnsureFeverChantPool()
        {
            int want = Mathf.Clamp(feverChantPoolSize, 4, 64);
            if (_feverChantSources != null &&
                _feverChantSources.Length == want &&
                _feverChantPoolRoot != null)
            {
                return;
            }

            DestroyFeverChantPool();

            var root = new GameObject("FeverChantAudioPool");
            root.transform.SetParent(transform, false);
            _feverChantPoolRoot = root.transform;

            AudioMixerGroup mixerGroup = sfxSource != null
                ? sfxSource.outputAudioMixerGroup
                : null;

            _feverChantSources = new AudioSource[want];
            for (var i = 0; i < want; i++)
            {
                AudioSource source = root.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.outputAudioMixerGroup = mixerGroup;
                _feverChantSources[i] = source;
            }

            _feverChantSourceCursor = 0;
        }

        AudioSource AcquireFeverChantSource()
        {
            if (_feverChantSources == null || _feverChantSources.Length == 0)
                return null;

            int index = _feverChantSourceCursor;
            _feverChantSourceCursor =
                (_feverChantSourceCursor + 1) % _feverChantSources.Length;
            return _feverChantSources[index];
        }

        void StopFeverChantAudio()
        {
            if (_feverChantSources == null)
                return;

            for (var i = 0; i < _feverChantSources.Length; i++)
            {
                AudioSource source = _feverChantSources[i];
                if (source == null)
                    continue;

                source.Stop();
                source.clip = null;
                source.pitch = 1f;
            }
        }

        void DestroyFeverChantPool()
        {
            StopFeverChantAudio();

            if (_feverChantPoolRoot != null)
                Destroy(_feverChantPoolRoot.gameObject);

            _feverChantPoolRoot = null;
            _feverChantSources = null;
            _feverChantSourceCursor = 0;
        }
    }
}
