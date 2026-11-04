using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MiniParty.Minigames.TimeChallenge
{
    /// <summary>씬에 저장한 슬롯별 영상·오디오·출력 텍스처를 재생한다.</summary>
    public sealed class TimeChallengePerfectVideo : MonoBehaviour
    {
        [SerializeField] VideoPlayer player;
        [SerializeField] AudioSource audioSource;
        [SerializeField] RawImage image;
        [SerializeField] AspectRatioFitter aspectFitter;
        float stopAt = -1f;
        float audioGain = 1f;

        public void Play(float volume, float seconds)
        {
            if (player == null || player.clip == null || player.targetTexture == null ||
                audioSource == null || image == null || aspectFitter == null)
            {
                Debug.LogWarning("[TimeChallenge] 적중 영상 Inspector 연결이 없습니다.", this);
                return;
            }
            Stop();
            gameObject.SetActive(true);
            stopAt = Time.unscaledTime + Mathf.Max(0.2f, seconds);
            audioGain = Mathf.Max(0.01f, volume);
            audioSource.volume = Mathf.Clamp01(audioGain);
            image.enabled = false;
            player.prepareCompleted += OnPrepared;
            player.errorReceived += OnVideoError;
            player.Prepare();
        }

        void OnPrepared(VideoPlayer source)
        {
            if (stopAt < 0f) return;
            if (source.width > 16 && source.height > 16)
                aspectFitter.aspectRatio = source.width / (float)source.height;
            image.enabled = true;
            source.Play();
        }

        void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogWarning("[TimeChallenge] 적중 영상 재생 실패: " + message, this);
            Stop();
        }

        void Update()
        {
            if (stopAt > 0f && Time.unscaledTime >= stopAt) Stop();
        }

        public void Stop()
        {
            ReleasePlayback();
            gameObject.SetActive(false);
        }

        void OnDisable() => ReleasePlayback();

        void ReleasePlayback()
        {
            stopAt = -1f;
            if (player != null)
            {
                player.prepareCompleted -= OnPrepared;
                player.errorReceived -= OnVideoError;
                player.Stop();
            }
            if (audioSource != null) audioSource.Stop();
            if (image != null) image.enabled = false;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            float gain = audioGain;
            if (gain <= 1.01f || data == null) return;
            for (int i = 0; i < data.Length; i++)
                data[i] = Mathf.Clamp(data[i] * gain, -1f, 1f);
        }
    }
}
