using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderGameManager
    {
        [Header("후원·라운드 음원 (순서 재생)")]
        [Tooltip("전체 후원창 — 3초 intro")]
        [SerializeField] AudioClip startDonationIntroClip;
        [Tooltip("intro 종료 후 TTS")]
        [SerializeField] AudioClip startDonationTtsClip;
        [Tooltip("카운트다운 시작 시 재생 (5초 카운트 + 25초 플레이)")]
        [SerializeField] AudioClip countdownBgmClip;
        [SerializeField] AudioSource startDonationBgmSource;
        [SerializeField] [Range(0f, 1f)] float startDonationBgmVolume = 1f;

        Coroutine _startDonationBgmRoutine;

        void SyncStartDonationBgmFromScene()
        {
            if (startDonationBgmSource == null)
                startDonationBgmSource = GetComponent<AudioSource>();

            if (startDonationIntroClip == null && startDonationBgmSource != null && startDonationBgmSource.clip != null)
                startDonationIntroClip = startDonationBgmSource.clip;
        }

        void EnsureStartDonationSequenceClips()
        {
            SyncStartDonationBgmFromScene();

#if UNITY_EDITOR
            if (startDonationIntroClip == null)
            {
                startDonationIntroClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    FlounderDonationUiConstants.StartDonationIntroBgmPath);
            }

            if (startDonationTtsClip == null)
            {
                startDonationTtsClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    FlounderDonationUiConstants.StartDonationTtsPath);
            }

            if (countdownBgmClip == null)
            {
                countdownBgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    FlounderDonationUiConstants.CountdownBgmPath);
            }
#endif
        }

        void EnsureCountdownBgmClip()
        {
#if UNITY_EDITOR
            if (countdownBgmClip == null)
            {
                countdownBgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    FlounderDonationUiConstants.CountdownBgmPath);
            }
#endif
        }

        public void PlayCountdownBgm()
        {
            EnsureCountdownBgmClip();
            if (countdownBgmClip == null)
            {
                Debug.LogWarning("[FlounderGameManager] 카운트다운 BGM 클립이 없습니다.", this);
                return;
            }

            PrepareClip(countdownBgmClip);
            AudioSource source = ResolveStartDonationBgmSource();
            source.enabled = true;
            source.mute = false;
            source.volume = startDonationBgmVolume;
            source.loop = false;
            source.clip = countdownBgmClip;
            source.time = 0f;
            source.Stop();
            source.Play();

            if (!source.isPlaying)
                source.PlayOneShot(countdownBgmClip, startDonationBgmVolume);
        }

        AudioSource ResolveStartDonationBgmSource()
        {
            if (startDonationBgmSource == null)
                startDonationBgmSource = GetComponent<AudioSource>();

            if (startDonationBgmSource == null)
                startDonationBgmSource = gameObject.AddComponent<AudioSource>();

            ConfigureStartDonationBgmSource(startDonationBgmSource);
            return startDonationBgmSource;
        }

        static void ConfigureStartDonationBgmSource(AudioSource source)
        {
            if (source == null)
                return;

            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.ignoreListenerPause = true;
        }

        void EnsureStartDonationBgmSource()
        {
            ConfigureStartDonationBgmSource(ResolveStartDonationBgmSource());
            SyncStartDonationBgmFromScene();
        }

        public void PlayStartDonationBgm()
        {
            if (_startDonationBgmRoutine != null)
                StopCoroutine(_startDonationBgmRoutine);

            _startDonationBgmRoutine = StartCoroutine(PlayStartDonationSequenceRoutine());
        }

        IEnumerator PlayStartDonationSequenceRoutine()
        {
            EnsureStartDonationSequenceClips();

            if (startDonationIntroClip == null)
            {
                Debug.LogWarning("[FlounderGameManager] 후원 intro 클립이 없습니다.", this);
                _startDonationBgmRoutine = null;
                yield break;
            }

            AudioSource source = ResolveStartDonationBgmSource();
            source.enabled = true;
            source.mute = false;
            source.volume = startDonationBgmVolume;

            yield return PlayClipToEnd(source, startDonationIntroClip);

            if (startDonationTtsClip != null)
                yield return PlayClipToEnd(source, startDonationTtsClip);

            _startDonationBgmRoutine = null;
        }

        static void PrepareClip(AudioClip clip)
        {
            if (clip == null)
                return;

            if (clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();
        }

        IEnumerator PlayClipToEnd(AudioSource source, AudioClip clip)
        {
            PrepareClip(clip);
            yield return null;

            source.loop = false;
            source.clip = clip;
            source.time = 0f;
            source.Stop();
            source.Play();

            if (!source.isPlaying)
            {
                source.PlayOneShot(clip, source.volume);
                yield return new WaitForSeconds(clip.length);
                yield break;
            }

            float safety = clip.length + 0.25f;
            float elapsed = 0f;
            while (source.isPlaying && elapsed < safety)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public void StopStartDonationBgm()
        {
            if (_startDonationBgmRoutine != null)
            {
                StopCoroutine(_startDonationBgmRoutine);
                _startDonationBgmRoutine = null;
            }

            AudioSource source = startDonationBgmSource != null ? startDonationBgmSource : GetComponent<AudioSource>();
            if (source == null)
                return;

            source.loop = false;
            if (source.isPlaying)
                source.Stop();
        }
    }
}
