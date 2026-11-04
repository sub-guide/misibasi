using System.Collections;
using UnityEngine;

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

        public void PlayCountdownBgm()
        {
            if (startDonationBgmSource == null || countdownBgmClip == null)
            {
                Debug.LogWarning("[FlounderGameManager] startDonationBgmSource / countdownBgmClip 을 Inspector에 연결하세요.", this);
                return;
            }

            PrepareClip(countdownBgmClip);
            startDonationBgmSource.volume = startDonationBgmVolume;
            startDonationBgmSource.loop = false;
            startDonationBgmSource.clip = countdownBgmClip;
            startDonationBgmSource.time = 0f;
            startDonationBgmSource.Stop();
            startDonationBgmSource.Play();

            if (!startDonationBgmSource.isPlaying)
                startDonationBgmSource.PlayOneShot(countdownBgmClip, startDonationBgmVolume);
        }

        public void PlayStartDonationBgm()
        {
            if (_startDonationBgmRoutine != null)
                StopCoroutine(_startDonationBgmRoutine);

            _startDonationBgmRoutine = StartCoroutine(PlayStartDonationSequenceRoutine());
        }

        IEnumerator PlayStartDonationSequenceRoutine()
        {
            if (startDonationBgmSource == null || startDonationIntroClip == null)
            {
                Debug.LogWarning("[FlounderGameManager] startDonationBgmSource / startDonationIntroClip 을 Inspector에 연결하세요.", this);
                _startDonationBgmRoutine = null;
                yield break;
            }

            startDonationBgmSource.volume = startDonationBgmVolume;

            yield return PlayClipToEnd(startDonationBgmSource, startDonationIntroClip);

            if (startDonationTtsClip != null)
                yield return PlayClipToEnd(startDonationBgmSource, startDonationTtsClip);

            _startDonationBgmRoutine = null;
        }

        static void PrepareClip(AudioClip clip)
        {
            if (clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();
        }

        static IEnumerator PlayClipToEnd(AudioSource source, AudioClip clip)
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

            if (startDonationBgmSource != null && startDonationBgmSource.isPlaying)
                startDonationBgmSource.Stop();
        }
    }
}
