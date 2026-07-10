using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// 크로마키 MP4 성공 이펙트 프리팹 루트. RenderTexture·RawImage·VideoPlayer를 자동 연결하고,
    /// Vertex Color 틴트를 적용한 뒤 재생 완료 시 자신을 파괴한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(RawImage))]
    public sealed class OiiaVideoEffectController : MonoBehaviour
    {
        const int FallbackRenderTextureSize = 512;

        [Tooltip("비우면 RawImage에 지정된 Material을 인스턴스로 사용.")]
        [SerializeField] Material chromaKeyMaterialTemplate;

        VideoPlayer _player;
        RawImage _rawImage;
        RenderTexture _renderTexture;
        Material _materialInstance;

        void Awake()
        {
            _player = GetComponent<VideoPlayer>();
            _rawImage = GetComponent<RawImage>();
            _player.playOnAwake = false;
            _player.isLooping = false;
            _player.loopPointReached += OnLoopPointReached;
            EnsureVideoPipeline();
        }

        void OnDestroy()
        {
            if (_player != null)
                _player.loopPointReached -= OnLoopPointReached;

            ReleaseRenderTexture();
        }

        /// <summary>틴트(RGB)와 재생 속도를 적용하고 비디오 재생을 시작한다. 알파는 항상 1.</summary>
        public void PlayWithTint(Color tint, float playbackSpeed = 1f)
        {
            EnsureVideoPipeline();
            _rawImage.color = new Color(tint.r, tint.g, tint.b, 1f);
            _player.playbackSpeed = Mathf.Max(0.01f, playbackSpeed);
            _player.Play();
        }

        void EnsureVideoPipeline()
        {
            EnsureChromaMaterial();
            EnsureRenderTexture();

            _player.renderMode = VideoRenderMode.RenderTexture;
            _player.targetTexture = _renderTexture;
            _rawImage.texture = _renderTexture;
        }

        void EnsureChromaMaterial()
        {
            if (_materialInstance != null)
                return;

            Material source = chromaKeyMaterialTemplate != null
                ? chromaKeyMaterialTemplate
                : _rawImage.material;

            if (source == null)
            {
                Debug.LogWarning(
                    "[OiiaVideoEffectController] 크로마키 Material이 없습니다. " +
                    "RawImage Material에 UI/OiiaChromaKey 를 지정하세요.",
                    this);
                return;
            }

            _materialInstance = Instantiate(source);
            _rawImage.material = _materialInstance;
        }

        void EnsureRenderTexture()
        {
            if (_renderTexture != null)
                return;

            if (_player.clip == null)
            {
                Debug.LogWarning("[OiiaVideoEffectController] Video Clip이 비어 있습니다.", this);
                return;
            }

            int width = Mathf.Max(16, (int)_player.clip.width);
            int height = Mathf.Max(16, (int)_player.clip.height);

            if (width <= 0 || height <= 0)
            {
                width = FallbackRenderTextureSize;
                height = FallbackRenderTextureSize;
            }

            _renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _renderTexture.Create();
            ClearRenderTextureTransparent(_renderTexture);
        }

        static void ClearRenderTextureTransparent(RenderTexture rt)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prev;
        }

        void ReleaseRenderTexture()
        {
            if (_renderTexture == null)
                return;

            if (_player != null && _player.targetTexture == _renderTexture)
                _player.targetTexture = null;

            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }

        void OnLoopPointReached(VideoPlayer source)
        {
            Destroy(gameObject);
        }
    }
}
