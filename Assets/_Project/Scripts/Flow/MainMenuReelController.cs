using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>메인 3릴 뷰. ↑↓는 Director. 한 칸은 bounceCurve. Space 스핀은 PlaySpin.</summary>
    [DefaultExecutionOrder(40)]
    public sealed class MainMenuReelController : MonoBehaviour
    {
        enum Mode
        {
            Idle,
            Accel,
            Cruise,
            Stagger
        }

        [Header("컬럼")]
        [SerializeField] ReelColumn[] columns = new ReelColumn[3];

        [Tooltip("한 칸 높이(px). Reel_Column 심볼 간격과 같게.")]
        [SerializeField] float slotHeight = 300f;

        [Header("바운스")]
        [Tooltip("한 칸 착지까지 시간(초).")]
        [SerializeField] float duration = 0.22f;

        [Tooltip("가로 0~1 = Duration. 세로 1 = 착지, 1 넘으면 오버슈트.")]
        [SerializeField] AnimationCurve bounceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("바운스 중 ↑/↓ 무시.")]
        [SerializeField] bool lockInputUntilSettled = true;

        [Header("스핀")]
        [Tooltip("크루즈 속도까지 가속 시간(초).")]
        [SerializeField] float accelDuration = 0.35f;

        [Tooltip("최고속 유지 시간(초).")]
        [SerializeField] float cruiseDuration = 0.8f;

        [Tooltip("크루즈 속도(px/s). 아래 방향 = ↓. 가속·감속의 최고속.")]
        [SerializeField] float cruiseSpeed = 1800f;

        [Tooltip("오른쪽 줄이 왼쪽보다 카탈로그를 몇 바퀴 더 도는지. Reel_0=0, Reel_1=이 값, Reel_2=2배.")]
        [SerializeField] int extraLoopsPerColumn = 2;

        [Tooltip("최고속에서 정지까지 감속 시간(초). 0이면 최고속으로 목표까지.")]
        [SerializeField] float decelDuration = 0.35f;

        [Tooltip("가속 0→1. 비우면 선형.")]
        [SerializeField] AnimationCurve spinAccelCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("감속 가로 0=시작(최고속) 1=정지. 세로=속도 비율. 비우면 1→0 직선.")]
        [SerializeField] AnimationCurve spinDecelCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("실루엣")]
        [SerializeField] Color silhouetteColor = Color.black;

        GameCatalogEntry[] _catalog;
        bool _settling;
        float _bounceElapsed;
        int _bounceDelta;

        Mode _mode = Mode.Idle;
        float _phaseElapsed;
        int _spinFromIndex;
        int _winnerIndex;
        float[] _travel;
        float[] _stopToTravel;
        float[] _decelElapsed;
        bool[] _stopping;
        bool[] _deceling;
        bool[] _landed;

        public int SelectedIndex { get; private set; }

        public bool IsSettling => _settling;

        public bool IsSpinning => _mode != Mode.Idle;

        public bool IsBusy => _settling || IsSpinning;

        public bool LockInputUntilSettled => lockInputUntilSettled;

        public void BindCatalog(GameCatalogEntry[] catalog, int selectedIndex = 0)
        {
            _catalog = catalog;
            int length = catalog != null ? catalog.Length : 0;
            SelectedIndex = GameCatalogEntry.WrapIndex(selectedIndex, length);
            CancelMotion();
            ApplyIdleLayout();
        }

        /// <summary>인덱스는 Director가 wrap한 값. 릴은 바운스만.</summary>
        public void PlayStep(int selectedIndex, int delta)
        {
            if (IsSpinning)
                return;

            if (_catalog == null || _catalog.Length == 0)
                return;

            SelectedIndex = GameCatalogEntry.WrapIndex(selectedIndex, _catalog.Length);
            _bounceDelta = delta;
            _bounceElapsed = 0f;
            _settling = duration > 0f && slotHeight > 0f;
            ApplyIdleLayout();
            if (_settling)
                ApplyBounceLayout();
        }

        public void PlaySpin(int winnerIndex)
        {
            if (_catalog == null || _catalog.Length == 0)
                return;

            if (columns == null || columns.Length == 0)
                return;

            _winnerIndex = GameCatalogEntry.WrapIndex(winnerIndex, _catalog.Length);
            _spinFromIndex = SelectedIndex;
            EnsureSpinBuffers();
            _settling = false;
            _bounceElapsed = 0f;
            _phaseElapsed = 0f;
            _mode = Mode.Accel;

            int n = columns.Length;
            for (var i = 0; i < n; i++)
            {
                _travel[i] = 0f;
                _stopping[i] = false;
                _deceling[i] = false;
                _landed[i] = false;
                _stopToTravel[i] = 0f;
                _decelElapsed[i] = 0f;
            }

            ApplySpinLayout();
        }

        void Update()
        {
            if (IsSpinning)
            {
                TickSpin();
                return;
            }

            if (!_settling)
                return;

            _bounceElapsed += Time.unscaledDeltaTime;
            if (_bounceElapsed >= duration)
            {
                _settling = false;
                _bounceElapsed = 0f;
                ApplyIdleLayout();
                return;
            }

            ApplyBounceLayout();
        }

        void TickSpin()
        {
            float dt = Time.unscaledDeltaTime;

            if (_mode == Mode.Accel)
            {
                _phaseElapsed += dt;
                float u = accelDuration <= 0f ? 1f : Mathf.Clamp01(_phaseElapsed / accelDuration);
                IntegrateCruise(CruiseSpeedAt(u) * dt);
                if (u >= 1f)
                {
                    _mode = Mode.Cruise;
                    _phaseElapsed = 0f;
                }
                else
                {
                    ApplySpinLayout();
                    return;
                }
            }

            if (_mode == Mode.Cruise)
            {
                if (cruiseDuration <= 0f)
                {
                    _mode = Mode.Stagger;
                    ArmStops();
                }
                else
                {
                    _phaseElapsed += dt;
                    IntegrateCruise(cruiseSpeed * dt);
                    if (_phaseElapsed >= cruiseDuration)
                    {
                        _mode = Mode.Stagger;
                        ArmStops();
                    }
                    else
                    {
                        ApplySpinLayout();
                        return;
                    }
                }
            }

            if (_mode == Mode.Stagger)
            {
                TickStops(dt);
                if (AllLanded())
                {
                    FinishSpin();
                    return;
                }
            }

            ApplySpinLayout();
        }

        float CruiseSpeedAt(float accelU)
        {
            float k = spinAccelCurve != null && spinAccelCurve.length > 0
                ? spinAccelCurve.Evaluate(accelU)
                : accelU;
            return cruiseSpeed * Mathf.Clamp01(k);
        }

        void IntegrateCruise(float distanceDown)
        {
            if (columns == null)
                return;

            for (var i = 0; i < columns.Length; i++)
            {
                if (_landed[i] || _stopping[i])
                    continue;

                _travel[i] += distanceDown;
            }
        }

        void ArmStops()
        {
            if (columns == null)
                return;

            for (var i = 0; i < columns.Length; i++)
            {
                if (_landed[i] || _stopping[i])
                    continue;

                BeginStop(i);
            }
        }

        void BeginStop(int i)
        {
            float t = _travel[i];
            float loop = CatalogLoopPixels();
            int extra = extraLoopsPerColumn < 0 ? 0 : extraLoopsPerColumn;
            float target = ForwardStopTravel(t) + i * extra * loop;
            if (target < t)
                target = t;

            _stopToTravel[i] = target;
            _stopping[i] = true;
            _deceling[i] = false;
            _decelElapsed[i] = 0f;
        }

        float CatalogLoopPixels()
        {
            int length = _catalog != null ? _catalog.Length : 0;
            if (length <= 0 || slotHeight <= 0.0001f)
                return 0f;

            return length * slotHeight;
        }

        float ForwardStopTravel(float travel)
        {
            if (slotHeight <= 0.0001f || _catalog == null || _catalog.Length == 0)
                return travel;

            TravelPhase(travel, out int visual, out float frac);
            int length = _catalog.Length;
            int steps = GameCatalogEntry.WrapIndex(_winnerIndex - visual, length);
            bool onRest = frac <= 0.0001f;
            if (steps == 0 && onRest)
                return travel;

            if (steps == 0)
                return travel + (slotHeight - frac) + (length - 1) * slotHeight;

            if (onRest)
                return travel + steps * slotHeight;

            return travel + (slotHeight - frac) + (steps - 1) * slotHeight;
        }

        void TickStops(float dt)
        {
            if (columns == null)
                return;

            float cruise = Mathf.Max(0f, cruiseSpeed);
            float brakeDist = DecelDistance();
            for (var i = 0; i < columns.Length; i++)
            {
                if (!_stopping[i] || _landed[i])
                    continue;

                float target = _stopToTravel[i];
                float remaining = target - _travel[i];
                if (remaining <= 0.0001f || cruise <= 0f)
                {
                    LandColumn(i);
                    continue;
                }

                if (!_deceling[i] && (decelDuration <= 0f || remaining <= brakeDist + 0.0001f))
                {
                    _deceling[i] = true;
                    _decelElapsed[i] = 0f;
                }

                float speed = cruise;
                if (_deceling[i] && decelDuration > 0f)
                {
                    _decelElapsed[i] += dt;
                    float u = Mathf.Clamp01(_decelElapsed[i] / decelDuration);
                    speed = cruise * DecelSpeedFrac(u);
                    if (u >= 1f)
                    {
                        LandColumn(i);
                        continue;
                    }
                }

                float step = speed * dt;
                if (step >= remaining)
                {
                    LandColumn(i);
                    continue;
                }

                _travel[i] += step;
            }
        }

        void LandColumn(int i)
        {
            _travel[i] = _stopToTravel[i];
            _stopping[i] = false;
            _deceling[i] = false;
            _landed[i] = true;
        }

        float DecelSpeedFrac(float u)
        {
            if (spinDecelCurve != null && spinDecelCurve.length > 0)
                return Mathf.Clamp01(spinDecelCurve.Evaluate(u));

            return 1f - u;
        }

        float DecelDistance()
        {
            if (decelDuration <= 0f || cruiseSpeed <= 0f)
                return 0f;

            const int samples = 20;
            float acc = 0f;
            for (var s = 0; s < samples; s++)
            {
                float u0 = s / (float)samples;
                float u1 = (s + 1) / (float)samples;
                acc += 0.5f * (DecelSpeedFrac(u0) + DecelSpeedFrac(u1)) * (u1 - u0);
            }

            return cruiseSpeed * decelDuration * acc;
        }

        bool AllLanded()
        {
            if (columns == null || columns.Length == 0)
                return true;

            for (var i = 0; i < columns.Length; i++)
            {
                if (!_landed[i])
                    return false;
            }

            return true;
        }

        void FinishSpin()
        {
            _mode = Mode.Idle;
            SelectedIndex = _winnerIndex;
            if (columns != null)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    _travel[i] = 0f;
                    _stopping[i] = false;
                    _deceling[i] = false;
                    _landed[i] = true;
                }
            }

            ApplyIdleLayout();
        }

        void CancelMotion()
        {
            _settling = false;
            _bounceElapsed = 0f;
            _mode = Mode.Idle;
        }

        void TravelPhase(float travel, out int floorIndex, out float frac)
        {
            int length = _catalog != null ? _catalog.Length : 0;
            if (slotHeight <= 0.0001f || length <= 0)
            {
                floorIndex = _spinFromIndex;
                frac = 0f;
                return;
            }

            float t = travel;
            int slots = Mathf.FloorToInt(t / slotHeight);
            frac = t - slots * slotHeight;
            if (frac < 0f)
            {
                frac += slotHeight;
                slots--;
            }

            if (frac >= slotHeight - 0.0001f)
            {
                slots++;
                frac = 0f;
            }
            else if (frac <= 0.0001f)
            {
                frac = 0f;
            }

            floorIndex = GameCatalogEntry.WrapIndex(_spinFromIndex + slots, length);
        }

        void LayoutFromTravel(float travel, out int index, out float offset)
        {
            TravelPhase(travel, out int floorIndex, out float frac);
            if (frac <= 0.0001f)
            {
                index = floorIndex;
                offset = 0f;
                return;
            }

            int length = _catalog.Length;
            index = GameCatalogEntry.WrapIndex(floorIndex + 1, length);
            offset = -(slotHeight - frac);
        }

        void EnsureSpinBuffers()
        {
            int n = columns.Length;
            if (_travel != null && _travel.Length == n && _deceling != null && _deceling.Length == n)
                return;

            _travel = new float[n];
            _stopToTravel = new float[n];
            _decelElapsed = new float[n];
            _stopping = new bool[n];
            _deceling = new bool[n];
            _landed = new bool[n];
        }

        float CurrentBounceOffset()
        {
            if (!_settling)
                return 0f;

            float u = duration <= 0f ? 1f : Mathf.Clamp01(_bounceElapsed / duration);
            float n = bounceCurve != null && bounceCurve.length > 0
                ? bounceCurve.Evaluate(u)
                : u;

            return -_bounceDelta * slotHeight * (1f - n);
        }

        void ApplyBounceLayout()
        {
            ApplyColumns(CurrentBounceOffset(), forceAllSilhouette: false, perColumn: false);
        }

        void ApplyIdleLayout()
        {
            ApplyColumns(0f, forceAllSilhouette: false, perColumn: false);
        }

        void ApplySpinLayout()
        {
            ApplyColumns(0f, forceAllSilhouette: true, perColumn: true);
        }

        void ApplyColumns(float sharedOffset, bool forceAllSilhouette, bool perColumn)
        {
            if (columns == null)
                return;

            for (var i = 0; i < columns.Length; i++)
            {
                if (columns[i] == null)
                    continue;

                int index = SelectedIndex;
                float offset = sharedOffset;
                if (perColumn && _travel != null && i < _travel.Length)
                {
                    if (_landed[i])
                    {
                        index = _winnerIndex;
                        offset = 0f;
                    }
                    else
                    {
                        LayoutFromTravel(_travel[i], out index, out offset);
                    }
                }
                bool silhouette = perColumn && forceAllSilhouette && (_landed == null || !_landed[i]);
                columns[i].ApplyLayout(
                    _catalog,
                    index,
                    offset,
                    slotHeight,
                    silhouetteColor,
                    silhouette);
            }
        }
    }
}
