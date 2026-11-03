using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    /// <summary>
    /// ↑ 연타: 들어올림 프레임을 끊김 없이 이어 재생. ↓: 내림 시퀀스. GIF 5프레임 기준.
    /// </summary>
    public sealed class FlounderDance : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] frames;

        [Tooltip("↑ 들어올림 순서 (낮음 → 중간 → 최고)")]
        [SerializeField] int[] liftFrameIndices = { 0, 1, 3 };

        [Tooltip("↓ 최고 근처에서 내릴 때")]
        [SerializeField] int[] lowerFromPeakIndices = { 3, 4, 0 };

        [Tooltip("↓ 중간 높이에서 내릴 때")]
        [SerializeField] int[] lowerFromMidIndices = { 1, 0 };

        [SerializeField] float flapFps = 22f;
        [SerializeField] float mashSpeedBoost = 18f;
        [SerializeField] float mashBoostDecay = 12f;

        [Tooltip("↑ 1회당 경로상 전진량 (클수록 빠르게 오름)")]
        [SerializeField] float liftAdvancePerFlap = 0.55f;

        [Tooltip("↓ 1회당 경로상 전진량")]
        [SerializeField] float lowerAdvancePerFlap = 0.65f;

        [Tooltip("최고점에서 ↑ 연타 시 1↔3 프레임 왔다갔다 (파닥임)")]
        [SerializeField] float peakFlutterFps = 28f;

        enum MotionMode
        {
            Idle,
            Lifting,
            Lowering,
            PeakFlutter,
            HoldPeak,
            HoldRest,
        }

        MotionMode _mode = MotionMode.Idle;
        float _pathIndex;
        int[] _lowerPath;
        float _speedBoost;
        bool _frozen;
        float _peakFlutterPhase;

        public void SetFrozen(bool frozen) => _frozen = frozen;

        public void SetFrames(Sprite[] sequence)
        {
            frames = sequence;
            _mode = MotionMode.Idle;
            _pathIndex = 0f;
            _lowerPath = null;
            ApplyFrameIndex(ResolveRestFrameIndex());
        }

        public void CopyMotionFrom(FlounderDance source)
        {
            if (source == null || source == this)
                return;

            flapFps = source.flapFps;
            mashSpeedBoost = source.mashSpeedBoost;
            mashBoostDecay = source.mashBoostDecay;
            liftAdvancePerFlap = source.liftAdvancePerFlap;
            lowerAdvancePerFlap = source.lowerAdvancePerFlap;
            peakFlutterFps = source.peakFlutterFps;
            liftFrameIndices = (int[])source.liftFrameIndices?.Clone();
            lowerFromPeakIndices = (int[])source.lowerFromPeakIndices?.Clone();
            lowerFromMidIndices = (int[])source.lowerFromMidIndices?.Clone();
        }

        public void FlapUpper()
        {
            _speedBoost = mashSpeedBoost;

            if (_mode == MotionMode.Lowering)
                _pathIndex = MapLowerIndexToLift(_pathIndex);

            _mode = MotionMode.Lifting;
            float max = Mathf.Max(0f, GetLiftPathLength() - 1f);
            _pathIndex = Mathf.Min(_pathIndex + liftAdvancePerFlap, max);

            if (_pathIndex >= max - 0.05f)
                _mode = MotionMode.PeakFlutter;
        }

        public void FlapLower()
        {
            _speedBoost = mashSpeedBoost;

            int currentFrame = SampleCurrentFrameIndex();
            _lowerPath = PickLowerPath(currentFrame);
            _pathIndex = FindLowerPathIndexForFrame(_lowerPath, currentFrame);
            _mode = MotionMode.Lowering;
            _pathIndex = Mathf.Min(_pathIndex + lowerAdvancePerFlap, _lowerPath.Length - 1);
        }

        /// <summary>↑/↓ 키를 누르고 있는 동안 최고·바닥 포즈 유지.</summary>
        public void ApplyDirectionalHold(bool upperHeld, bool lowerHeld)
        {
            if (upperHeld && !lowerHeld)
            {
                _mode = MotionMode.HoldPeak;
                _pathIndex = Mathf.Max(0f, GetLiftPathLength() - 1f);
                ApplyPeakFrame();
                return;
            }

            if (lowerHeld && !upperHeld)
            {
                _mode = MotionMode.HoldRest;
                _pathIndex = 0f;
                ApplyFrameIndex(ResolveRestFrameIndex());
                return;
            }

            if (_mode is MotionMode.HoldPeak or MotionMode.HoldRest)
                _mode = MotionMode.Idle;
        }

        void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            ApplyFrameIndex(ResolveRestFrameIndex());
        }

        void Update()
        {
            if (_frozen)
                return;

            _speedBoost = Mathf.MoveTowards(_speedBoost, 0f, mashBoostDecay * Time.deltaTime);

            if (_mode is MotionMode.HoldPeak or MotionMode.HoldRest)
                return;

            float fps = flapFps + _speedBoost;

            switch (_mode)
            {
                case MotionMode.Lifting:
                    TickLiftPath(fps);
                    break;
                case MotionMode.PeakFlutter:
                    TickPeakFlutter(fps);
                    break;
                case MotionMode.Lowering:
                    TickLowerPath(fps);
                    break;
                default:
                    ApplyFrameIndex(ResolveRestFrameIndex());
                    break;
            }
        }

        void ApplyPeakFrame()
        {
            if (liftFrameIndices != null && liftFrameIndices.Length > 0)
            {
                ApplyFrameIndex(liftFrameIndices[liftFrameIndices.Length - 1]);
                return;
            }

            ApplyFrameIndex(0);
        }

        void TickLiftPath(float fps)
        {
            float max = Mathf.Max(0f, GetLiftPathLength() - 1f);
            _pathIndex += fps * Time.deltaTime;
            if (_pathIndex >= max)
            {
                _pathIndex = max;
                _mode = MotionMode.PeakFlutter;
            }

            ApplyLiftPathIndex(_pathIndex);
        }

        void TickPeakFlutter(float fps)
        {
            if (liftFrameIndices == null || liftFrameIndices.Length < 2)
            {
                ApplyLiftPathIndex(GetLiftPathLength() - 1f);
                return;
            }

            float flutterRate = peakFlutterFps + _speedBoost * 0.5f;

            _peakFlutterPhase += flutterRate * Time.deltaTime;
            float max = GetLiftPathLength() - 1f;
            float min = Mathf.Max(0f, max - 1f);
            _pathIndex = min + Mathf.PingPong(_peakFlutterPhase, 1f);
            ApplyLiftPathIndex(_pathIndex);
        }

        void TickLowerPath(float fps)
        {
            if (_lowerPath == null || _lowerPath.Length == 0)
            {
                SetIdle();
                return;
            }

            _pathIndex += fps * Time.deltaTime;
            float max = _lowerPath.Length - 1f;
            if (_pathIndex >= max)
            {
                SetIdle();
                return;
            }

            ApplyLowerPathIndex(_pathIndex);
        }

        void SetIdle()
        {
            _mode = MotionMode.Idle;
            _pathIndex = 0f;
            _lowerPath = null;
            ApplyFrameIndex(ResolveRestFrameIndex());
        }

        float GetLiftPathLength()
        {
            if (liftFrameIndices == null || liftFrameIndices.Length == 0)
                return 1f;

            return liftFrameIndices.Length;
        }

        void ApplyLiftPathIndex(float index)
        {
            if (liftFrameIndices == null || liftFrameIndices.Length == 0)
                return;

            int i0 = Mathf.FloorToInt(index);
            int i1 = Mathf.Min(i0 + 1, liftFrameIndices.Length - 1);
            float t = index - i0;
            int frame = t < 0.5f ? liftFrameIndices[i0] : liftFrameIndices[i1];
            ApplyFrameIndex(frame);
        }

        void ApplyLowerPathIndex(float index)
        {
            if (_lowerPath == null || _lowerPath.Length == 0)
                return;

            int i0 = Mathf.FloorToInt(index);
            int i1 = Mathf.Min(i0 + 1, _lowerPath.Length - 1);
            float t = index - i0;
            int frame = t < 0.5f ? _lowerPath[i0] : _lowerPath[i1];
            ApplyFrameIndex(frame);
        }

        int SampleCurrentFrameIndex()
        {
            if (_mode == MotionMode.Lowering && _lowerPath != null && _lowerPath.Length > 0)
            {
                int idx = Mathf.Clamp(Mathf.RoundToInt(_pathIndex), 0, _lowerPath.Length - 1);
                return _lowerPath[idx];
            }

            if (liftFrameIndices != null && liftFrameIndices.Length > 0)
                return SampleLiftFrameAt(_pathIndex);

            return ResolveRestFrameIndex();
        }

        int SampleLiftFrameAt(float index)
        {
            int i0 = Mathf.Clamp(Mathf.FloorToInt(index), 0, liftFrameIndices.Length - 1);
            int i1 = Mathf.Min(i0 + 1, liftFrameIndices.Length - 1);
            float t = index - i0;
            return t < 0.5f ? liftFrameIndices[i0] : liftFrameIndices[i1];
        }

        int[] PickLowerPath(int currentFrame)
        {
            if (currentFrame >= 3 && lowerFromPeakIndices != null && lowerFromPeakIndices.Length > 0)
                return lowerFromPeakIndices;

            if (lowerFromMidIndices != null && lowerFromMidIndices.Length > 0)
                return lowerFromMidIndices;

            return new[] { ResolveRestFrameIndex() };
        }

        static float FindLowerPathIndexForFrame(int[] path, int frame)
        {
            if (path == null || path.Length == 0)
                return 0f;

            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] == frame)
                    return i;
            }

            return 0f;
        }

        float MapLowerIndexToLift(float lowerIndex)
        {
            if (_lowerPath == null || _lowerPath.Length == 0)
                return 0f;

            int idx = Mathf.Clamp(Mathf.RoundToInt(lowerIndex), 0, _lowerPath.Length - 1);
            return FrameToLiftPathIndex(_lowerPath[idx]);
        }

        float FrameToLiftPathIndex(int frame)
        {
            if (liftFrameIndices == null || liftFrameIndices.Length == 0)
                return 0f;

            for (var i = 0; i < liftFrameIndices.Length; i++)
            {
                if (liftFrameIndices[i] == frame)
                    return i;
            }

            if (frame == 4 && liftFrameIndices.Length >= 2)
                return liftFrameIndices.Length - 1 - 0.5f;

            return 0f;
        }

        int ResolveRestFrameIndex()
        {
            if (liftFrameIndices != null && liftFrameIndices.Length > 0)
                return liftFrameIndices[0];

            return 0;
        }

        void ApplyFrameIndex(int frameIndex)
        {
            if (frames == null || frames.Length == 0 || spriteRenderer == null)
                return;

            frameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
