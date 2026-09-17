using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        [Header("오디오")]
        [SerializeField] AudioSource musicSource;
        [SerializeField] AudioClip sessionTrack;

        void StartSessionAudio()
        {
            StopSessionAudio();

            if (musicSource == null)
            {
                Debug.LogError(
                    "[RhythmButtonChallengeMinigameModule] musicSource 를 Inspector에 연결하세요.",
                    this);
                return;
            }

            if (sessionTrack == null)
            {
                Debug.LogError(
                    "[RhythmButtonChallengeMinigameModule] sessionTrack (RBC_Track) 을 Inspector에 연결하세요.",
                    this);
                return;
            }

            musicSource.loop = false;
            musicSource.clip = sessionTrack;
            musicSource.time = 0f;
            musicSource.Play();
        }

        void StopSessionAudio()
        {
            if (musicSource == null || !musicSource.isPlaying)
                return;

            musicSource.Stop();
        }
    }
}
