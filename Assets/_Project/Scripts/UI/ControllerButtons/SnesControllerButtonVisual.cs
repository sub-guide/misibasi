using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// SNES Face·D-Pad 팔 시각: Idle / Highlighted / Pressing / Held / Releasing.
    /// Color: *Press 시트 4프레임 애니. 빠른 탭 시 눌림 전구간→해제 재생.
    /// 2D(무애니): 홀드 중 즉시 Idle↔Held — <see cref="instantHoldVisual"/> 또는 <c>pressFrames</c> 없음.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class SnesControllerButtonVisual : MonoBehaviour
    {
        enum VisualPhase
        {
            Idle,
            Highlighted,
            Pressing,
            Held,
            Releasing
        }

        [SerializeField] SnesButtonSpriteSet spriteSet;
        [SerializeField] Image icon;

        [Header("애니 타이밍")]
        [Tooltip("Press/Release 스프라이트 1장당 표시 시간(초). 빌드·에디터 동일. 기본 0.1.")]
        [SerializeField] [Min(0.001f)] float secondsPerSprite = 0.1f;

        [Tooltip("Press 시트에서 눌림에 쓰는 프레임 수(앞에서부터). -1 = 자동(절반).")]
        [SerializeField] int pressFrameCount = -1;

        [Tooltip("Held 상태 localScale 배율. 시트에 눌림이 그려져 있으면 1 권장.")]
        [SerializeField] float heldScale = 1f;

        [Header("2D (무애니)")]
        [Tooltip("true면 Press/Release 코루틴 없이 Idle↔Held 즉시. pressFrames 없으면 자동 true.")]
        [SerializeField] bool instantHoldVisual;

        [Header("강조 오버라이드 (선택)")]
        [Tooltip("비우면 SpriteSet.Highlighted. 미니게임별 강조 스프라이트 교체용.")]
        [SerializeField] Sprite highlightedOverride;

        public float SecondsPerSprite => secondsPerSprite;
        public int PressFrameCount => pressFrameCount;
        public float HeldScale => heldScale;
        public bool InstantHoldVisual => instantHoldVisual;

        /// <summary>에디터·부모 드라이버에서 타이밍·2D 무애니 일괄 적용.</summary>
        public void ApplyAnimationSettings(
            float secondsPerSpriteValue,
            int pressFrameCountValue,
            float heldScaleValue,
            bool instantHoldVisualValue = false)
        {
            this.secondsPerSprite = Mathf.Max(0.001f, secondsPerSpriteValue);
            this.pressFrameCount = pressFrameCountValue;
            this.heldScale = heldScaleValue;
            this.instantHoldVisual = instantHoldVisualValue;

            if (Application.isPlaying && UsesInstantHold())
            {
                StopTransition();
                ApplyImmediateVisual();
            }
        }

        /// <summary>런타임 강조 스프라이트 교체. null이면 SpriteSet 기본 Highlighted.</summary>
        public void SetHighlightedSpriteOverride(Sprite sprite)
        {
            highlightedOverride = sprite;
            if (_wantHighlight && !_wantHeld)
                ApplyImmediateVisual();
        }

        /// <summary>
        /// Highlighted 스프라이트를 그릴 때만 Image를 흰색으로.
        /// 그 외(Pressed/Held/Idle)는 에디터 Image 색. Idle 명도 오버라이드는 <see cref="ConfigureUnpressedBrightness"/>.
        /// </summary>
        public void ConfigureWhiteIconOnlyWhenShowingHighlighted(bool enabled)
        {
            _whiteIconOnlyWhenShowingHighlighted = enabled;
            SyncIconColorToDisplayedSprite(icon != null ? icon.sprite : null);
        }

        /// <summary>하위 호환. restIconColor는 에디터 기준색으로 저장.</summary>
        public void ConfigureWhiteIconOnlyWhenShowingHighlighted(bool enabled, Color restIconColor)
        {
            _editorIconColor = restIconColor;
            ConfigureWhiteIconOnlyWhenShowingHighlighted(enabled);
        }

        /// <summary>
        /// Idle(Unpressed)일 때만 RGB 명도(0~255) 적용. 기본 OFF(프로젝트 공통 영향 없음).
        /// 미니게임(예: OIIA)에서만 켜서 사용.
        /// </summary>
        public void ConfigureUnpressedBrightness(bool enabled, int brightness0To255 = 100)
        {
            _unpressedBrightnessEnabled = enabled;
            _unpressedBrightness = Mathf.Clamp(brightness0To255, 0, 255);
            SyncIconColorToDisplayedSprite(icon != null ? icon.sprite : null);
        }

        /// <summary>Image 틴트(직접 지정). Idle 명도·Highlight 흰색 로직이 덮어쓸 수 있음.</summary>
        public void SetIconColor(Color color)
        {
            if (icon == null)
                icon = GetComponent<Image>();

            if (icon != null)
            {
                icon.color = color;
                _editorIconColor = color;
            }
        }

        Sprite ResolveHighlightedSprite()
        {
            if (highlightedOverride != null)
                return highlightedOverride;

            return spriteSet != null ? spriteSet.Highlighted : null;
        }

        void SyncIconColorToDisplayedSprite(Sprite displayed)
        {
            if (!_unpressedBrightnessEnabled && !_whiteIconOnlyWhenShowingHighlighted)
                return;

            if (icon == null)
                icon = GetComponent<Image>();

            if (icon == null)
                return;

            Sprite idle = spriteSet != null ? spriteSet.Idle : null;
            bool showingIdle =
                displayed != null &&
                idle != null &&
                ReferenceEquals(displayed, idle);

            if (_unpressedBrightnessEnabled && showingIdle)
            {
                float b = _unpressedBrightness / 255f;
                icon.color = new Color(b, b, b, _editorIconColor.a);
                return;
            }

            if (_whiteIconOnlyWhenShowingHighlighted)
            {
                Sprite highlighted = ResolveHighlightedSprite();
                bool showingHighlighted =
                    displayed != null &&
                    highlighted != null &&
                    ReferenceEquals(displayed, highlighted);

                icon.color = showingHighlighted ? Color.white : _editorIconColor;
                return;
            }

            icon.color = _editorIconColor;
        }

        bool UsesInstantHold() =>
            instantHoldVisual || spriteSet == null || !spriteSet.HasPressAnimation;

        VisualPhase _phase = VisualPhase.Idle;
        bool _wantHighlight;
        bool _wantHeld;
        Vector3 _restScale = Vector3.one;
        Coroutine _transition;
        bool _whiteIconOnlyWhenShowingHighlighted;
        bool _unpressedBrightnessEnabled;
        int _unpressedBrightness = 100;
        Color _editorIconColor = Color.white;

        public bool IsHighlighted => _wantHighlight;
        public bool IsHeld => _wantHeld;
        public SnesButtonSpriteSet SpriteSet => spriteSet;

        void Awake()
        {
            if (icon == null)
                icon = GetComponent<Image>();

            _restScale = icon != null ? icon.rectTransform.localScale : Vector3.one;
            if (icon != null)
                _editorIconColor = icon.color;

            ApplyImmediateVisual();
        }

        void OnDisable()
        {
            StopTransition();
            _phase = _wantHeld
                ? VisualPhase.Held
                : (_wantHighlight ? VisualPhase.Highlighted : VisualPhase.Idle);
            ApplyImmediateVisual();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (icon == null)
                icon = GetComponent<Image>();

            if (!Application.isPlaying && icon != null && spriteSet != null && spriteSet.HasIdle)
                icon.sprite = spriteSet.Idle;
        }
#endif

        public void SetHighlighted(bool value)
        {
            if (_wantHighlight == value)
                return;

            _wantHighlight = value;
            if (_wantHeld || _phase is VisualPhase.Pressing or VisualPhase.Releasing)
                return;

            GoToRestVisual();
        }

        public void SetHeld(bool value)
        {
            if (_wantHeld == value)
                return;

            _wantHeld = value;

            if (UsesInstantHold())
            {
                StopTransition();
                _phase = value
                    ? VisualPhase.Held
                    : (_wantHighlight ? VisualPhase.Highlighted : VisualPhase.Idle);
                ApplyImmediateVisual();
                return;
            }

            if (value)
            {
                BeginPress();
                return;
            }

            // 해제: 눌림 중이면 코루틴이 끝까지 재생 후 해제(CoPressThenMaybeRelease). 이미 Held면 해제만.
            if (_phase == VisualPhase.Pressing)
                return;

            if (_phase == VisualPhase.Releasing)
                return;

            if (_phase == VisualPhase.Held)
            {
                BeginRelease();
                return;
            }

            // 같은 프레임·극단적 탭: press 코루틴이 아직 Pressing에 못 들어간 경우
            PlayPressPulse();
        }

        public void PlayPressPulse()
        {
            if (UsesInstantHold())
                return;

            StopTransition();
            _wantHeld = false;
            _transition = StartCoroutine(CoPressPulse());
        }

        /// <summary>눌림 프레임 전부 재생 후, 아직 누르고 있지 않으면 해제 프레임 재생.</summary>
        IEnumerator CoPressThenMaybeRelease()
        {
            _phase = VisualPhase.Pressing;
            Sprite[] frames = GetFrames();
            GetSheetRanges(frames, out int pressCount, out int releaseStart, out int releaseEnd);
            yield return PlayFrameRange(frames, 0, pressCount - 1, 1f);

            if (_wantHeld)
            {
                _transition = null;
                _phase = VisualPhase.Held;
                Sprite heldSprite = spriteSet != null ? spriteSet.Held : null;
                if (heldSprite == null)
                    heldSprite = FrameAt(frames, pressCount - 1);
                ApplyFrame(heldSprite, heldScale);
                yield break;
            }

            _phase = VisualPhase.Releasing;
            yield return PlayFrameRange(frames, releaseStart, releaseEnd, 1f);

            _transition = null;
            _phase = _wantHighlight ? VisualPhase.Highlighted : VisualPhase.Idle;
            ApplyImmediateVisual();
        }

        public void SetSpriteSet(SnesButtonSpriteSet set)
        {
            spriteSet = set;
            ApplyImmediateVisual();
        }

        void BeginPress()
        {
            StopTransition();
            _transition = StartCoroutine(CoPressThenMaybeRelease());
        }

        void BeginRelease()
        {
            StopTransition();
            _transition = StartCoroutine(CoReleaseOnly());
        }

        void GoToRestVisual()
        {
            StopTransition();
            _phase = _wantHighlight ? VisualPhase.Highlighted : VisualPhase.Idle;
            ApplyImmediateVisual();
        }

        IEnumerator CoReleaseOnly()
        {
            _phase = VisualPhase.Releasing;
            Sprite[] frames = GetFrames();
            GetSheetRanges(frames, out _, out int releaseStart, out int releaseEnd);
            yield return PlayFrameRange(frames, releaseStart, releaseEnd, 1f);

            _transition = null;
            if (_wantHeld)
            {
                BeginPress();
                yield break;
            }

            _phase = _wantHighlight ? VisualPhase.Highlighted : VisualPhase.Idle;
            ApplyImmediateVisual();
        }

        IEnumerator CoPressPulse()
        {
            _phase = VisualPhase.Pressing;
            Sprite[] frames = GetFrames();
            GetSheetRanges(frames, out int pressCount, out int releaseStart, out int releaseEnd);
            yield return PlayFrameRange(frames, 0, pressCount - 1, 1f);

            _phase = VisualPhase.Releasing;
            yield return PlayFrameRange(frames, releaseStart, releaseEnd, 1f);

            _transition = null;
            _phase = _wantHighlight ? VisualPhase.Highlighted : VisualPhase.Idle;
            ApplyImmediateVisual();
        }

        Sprite[] GetFrames() =>
            spriteSet != null ? spriteSet.GetPressFrames() : null;

        /// <summary>
        /// 4프레임 시트: press [0..pressCount), release [pressCount..end].
        /// pressFrameCount &lt; 0 이면 Length/2.
        /// </summary>
        void GetSheetRanges(
            Sprite[] frames,
            out int pressCount,
            out int releaseStart,
            out int releaseEnd)
        {
            if (frames == null || frames.Length == 0)
            {
                pressCount = 0;
                releaseStart = 0;
                releaseEnd = 0;
                return;
            }

            if (frames.Length == 1)
            {
                pressCount = 1;
                releaseStart = 0;
                releaseEnd = 0;
                return;
            }

            int autoHalf = frames.Length / 2;
            pressCount = pressFrameCount >= 0
                ? Mathf.Clamp(pressFrameCount, 1, frames.Length)
                : Mathf.Max(1, autoHalf);

            if (pressCount >= frames.Length)
            {
                releaseStart = frames.Length - 1;
                releaseEnd = frames.Length - 1;
                return;
            }

            releaseStart = pressCount;
            releaseEnd = frames.Length - 1;
        }

        IEnumerator PlayFrameRange(Sprite[] frames, int fromInclusive, int toInclusive, float scaleMul)
        {
            if (frames == null || frames.Length == 0)
            {
                if (spriteSet != null)
                    ApplyFrame(fromInclusive <= toInclusive ? spriteSet.Held : spriteSet.Idle, scaleMul);
                yield break;
            }

            fromInclusive = Mathf.Clamp(fromInclusive, 0, frames.Length - 1);
            toInclusive = Mathf.Clamp(toInclusive, 0, frames.Length - 1);

            int step = fromInclusive <= toInclusive ? 1 : -1;
            for (int i = fromInclusive; ; i += step)
            {
                ApplyFrame(frames[i], scaleMul);
                yield return WaitSpriteHold();

                if (i == toInclusive)
                    break;
            }
        }

        IEnumerator WaitSpriteHold()
        {
            float duration = Mathf.Max(0.001f, secondsPerSprite);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        static Sprite FrameAt(Sprite[] frames, int index)
        {
            if (frames == null || frames.Length == 0)
                return null;
            return frames[Mathf.Clamp(index, 0, frames.Length - 1)];
        }

        void ApplyImmediateVisual()
        {
            if (spriteSet == null || icon == null)
                return;

            if (_wantHeld || _phase == VisualPhase.Held)
            {
                ApplyFrame(spriteSet.Held, heldScale);
                return;
            }

            if (_phase == VisualPhase.Pressing || _phase == VisualPhase.Releasing)
            {
                Sprite[] frames = GetFrames();
                if (frames.Length > 0)
                {
                    GetSheetRanges(frames, out int pressCount, out int releaseStart, out int releaseEnd);
                    int idx = _phase == VisualPhase.Pressing
                        ? pressCount - 1
                        : releaseEnd;
                    ApplyFrame(frames[Mathf.Clamp(idx, 0, frames.Length - 1)], 1f);
                    return;
                }
            }

            Sprite s = _wantHighlight || _phase == VisualPhase.Highlighted
                ? ResolveHighlightedSprite()
                : (spriteSet != null ? spriteSet.Idle : null);
            ApplyFrame(s, 1f);
        }

        void ApplyFrame(Sprite sprite, float scaleMul)
        {
            if (icon == null)
                return;

            if (sprite != null)
                icon.sprite = sprite;

            float mul = Mathf.Approximately(scaleMul, 1f) ? 1f : scaleMul;
            icon.rectTransform.localScale = _restScale * mul;
            SyncIconColorToDisplayedSprite(sprite != null ? sprite : icon.sprite);
        }

        void StopTransition()
        {
            if (_transition == null)
                return;

            StopCoroutine(_transition);
            _transition = null;
        }
    }
}
