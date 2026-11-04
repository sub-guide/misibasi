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

        const float CenterMoveSeconds = 2f;
        const float CenterScale = 5f;

        float stopAt = -1f;
        float audioGain = 1f;
        RectTransform _rect;
        bool _homeCached;
        Vector2 _homeAnchored;
        Vector3 _homeScale;
        float _moveStart = -1f;

        public bool PlaybackBegan { get; private set; }

        public void Play(float volume, float seconds)
        {
            PlaybackBegan = false;
            if (player == null || player.clip == null || player.targetTexture == null ||
                audioSource == null || image == null || aspectFitter == null)
            {
                PlaybackBegan = true;
                Debug.LogWarning("[TimeChallenge] 적중 영상 Inspector 연결이 없습니다.", this);
                return;
            }
            Stop();
            RestoreHome();
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
            PlaybackBegan = true;
            image.enabled = true;
            source.Play();
            _moveStart = Time.unscaledTime;
            ApplyCenterMove(0f);
        }

        void OnVideoError(VideoPlayer source, string message)
        {
            PlaybackBegan = true;
            Debug.LogWarning("[TimeChallenge] 적중 영상 재생 실패: " + message, this);
            Stop();
        }

        void Update()
        {
            if (_moveStart >= 0f)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - _moveStart) / CenterMoveSeconds);
                ApplyCenterMove(t);
            }

            if (stopAt > 0f && Time.unscaledTime >= stopAt) Stop();
        }

        public void Stop()
        {
            ReleasePlayback();
            RestoreHome();
            gameObject.SetActive(false);
        }

        void RestoreHome()
        {
            CacheHome();
            _moveStart = -1f;
            _rect.anchoredPosition = _homeAnchored;
            _rect.localScale = _homeScale;
        }

        void CacheHome()
        {
            if (_rect == null)
                _rect = (RectTransform)transform;
            if (_homeCached)
                return;

            _homeAnchored = _rect.anchoredPosition;
            _homeScale = _rect.localScale;
            _homeCached = true;
        }

        void ApplyCenterMove(float t)
        {
            CacheHome();
            _rect.anchoredPosition = Vector2.Lerp(_homeAnchored, ScreenCenterLocal(), t);
            _rect.localScale = _homeScale * Mathf.Lerp(1f, CenterScale, t);
        }

        Vector2 ScreenCenterLocal()
        {
            Canvas canvas = _rect.GetComponent<Canvas>();
            Camera cam = canvas != null ? canvas.worldCamera : null;
            Vector3 world = cam != null
                ? new Vector3(cam.transform.position.x, cam.transform.position.y, _rect.position.z)
                : new Vector3(960f, 540f, _rect.position.z);
            Vector3 local = _rect.parent != null ? _rect.parent.InverseTransformPoint(world) : world;
            return new Vector2(local.x, local.y);
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
