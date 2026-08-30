using System;
using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>메인 메뉴 3릴. 선택 인덱스·수동 1칸 오버슈트 스크롤 소유. 셔플은 없음.</summary>
    [DefaultExecutionOrder(40)]
    public sealed class MainMenuReelController : MonoBehaviour
    {
        [Header("컬럼")]
        [SerializeField] ReelColumn[] columns = new ReelColumn[3];

        [Tooltip("릴 심볼 프리팹 (ReelSymbolView). BindCatalog 때 컬럼당 N개 Instantiate.")]
        [SerializeField] ReelSymbolView symbolPrefab;

        [Header("풀")]
        [Tooltip("컬럼당 심볼 수. 중앙 1 + 상하 + 리사이클 여유.")]
        [SerializeField] int symbolsPerColumn = 5;

        [Tooltip("한 칸 높이(px).")]
        [SerializeField] float slotHeight = 160f;

        [Header("바운스")]
        [Tooltip("한 칸 착지까지 시간(초).")]
        [SerializeField] float bounceDuration = 0.22f;

        [Tooltip("한 칸 대비 오버슈트 비율. bounceCurve 가 비어 있을 때 기본 탄성에 사용.")]
        [SerializeField] float overshootStrength = 0.18f;

        [Tooltip("0→1 이동량. 1을 넘기면 오버슈트. 키가 없으면 overshootStrength 기본 탄성.")]
        [SerializeField] AnimationCurve bounceCurve;

        [Tooltip("바운스 중 ↑/↓ 무시.")]
        [SerializeField] bool lockInputUntilSettled = true;

        [Header("실루엣 · 깊이")]
        [SerializeField] Color silhouetteColor = Color.black;

        [Tooltip("중앙에서 한 칸 떨어졌을 때 스케일.")]
        [SerializeField] float depthScaleMin = 0.72f;

        [Tooltip("중앙에서 한 칸 떨어졌을 때 알파.")]
        [SerializeField] float depthAlphaMin = 0.35f;

        [Tooltip("거리(칸) → 0~1 감쇄. 비우면 선형.")]
        [SerializeField] AnimationCurve depthFalloff;

        readonly OperatorInputService _operatorInput = new();

        GameCatalogEntry[] _catalog;
        bool _settling;
        float _bounceElapsed;
        int _bounceDelta;
        bool _loggedMissingPrefab;

        public int SelectedIndex { get; private set; }

        public event Action<int> OnSelectionChanged;

        public void BindCatalog(GameCatalogEntry[] catalog, int selectedIndex = 0)
        {
            _catalog = catalog;
            int length = catalog != null ? catalog.Length : 0;
            SelectedIndex = GameCatalogEntry.WrapIndex(selectedIndex, length);
            EnsureAllPools();
            ApplyLayout(0f);
        }

        void Update()
        {
            if (_catalog == null || _catalog.Length == 0)
                return;

            if (_settling)
            {
                TickBounce();
                if (_settling && lockInputUntilSettled)
                    return;
            }

            if (_operatorInput.MenuUp)
            {
                if (_settling)
                    FinishBounce();

                TryStep(-1);
            }
            else if (_operatorInput.MenuDown)
            {
                if (_settling)
                    FinishBounce();

                TryStep(1);
            }
        }

        void TryStep(int delta)
        {
            if (_catalog == null || _catalog.Length == 0)
                return;

            SelectedIndex = GameCatalogEntry.WrapIndex(SelectedIndex + delta, _catalog.Length);
            OnSelectionChanged?.Invoke(SelectedIndex);

            _bounceDelta = delta;
            _bounceElapsed = 0f;
            _settling = bounceDuration > 0f && slotHeight > 0f;
            ApplyLayout(CurrentBounceOffset());
        }

        void TickBounce()
        {
            _bounceElapsed += Time.unscaledDeltaTime;
            if (_bounceElapsed >= bounceDuration)
            {
                FinishBounce();
                return;
            }

            ApplyLayout(CurrentBounceOffset());
        }

        void FinishBounce()
        {
            _settling = false;
            _bounceElapsed = 0f;
            ApplyLayout(0f);
        }

        float CurrentBounceOffset()
        {
            if (!_settling)
                return 0f;

            float u = bounceDuration <= 0f ? 1f : Mathf.Clamp01(_bounceElapsed / bounceDuration);
            float n = EvaluateBounce(u);
            if (u >= 1f)
                n = 1f;

            return -_bounceDelta * slotHeight * (1f - n);
        }

        float EvaluateBounce(float u)
        {
            if (bounceCurve != null && bounceCurve.length > 0)
                return bounceCurve.Evaluate(u);

            return DefaultElastic(u, overshootStrength);
        }

        static float DefaultElastic(float u, float overshoot)
        {
            const float peakAt = 0.55f;
            if (u < peakAt)
            {
                float p = u / peakAt;
                p = 1f - (1f - p) * (1f - p);
                return Mathf.Lerp(0f, 1f + overshoot, p);
            }

            float q = (u - peakAt) / (1f - peakAt);
            q *= q;
            return Mathf.Lerp(1f + overshoot, 1f, q);
        }

        void EnsureAllPools()
        {
            int count = Mathf.Max(1, symbolsPerColumn);
            if (symbolPrefab == null && !_loggedMissingPrefab)
            {
                _loggedMissingPrefab = true;
                Debug.LogError("[MainMenuReelController] Symbol Prefab 이 비었습니다. Inspector에 ReelSymbolView 프리팹을 연결하세요.", this);
            }

            if (columns == null)
                return;

            for (var i = 0; i < columns.Length; i++)
            {
                if (columns[i] == null)
                    continue;

                columns[i].EnsurePool(symbolPrefab, count);
            }
        }

        void ApplyLayout(float scrollOffset)
        {
            if (columns == null)
                return;

            for (var i = 0; i < columns.Length; i++)
            {
                if (columns[i] == null)
                    continue;

                columns[i].ApplyLayout(
                    _catalog,
                    SelectedIndex,
                    scrollOffset,
                    slotHeight,
                    silhouetteColor,
                    depthScaleMin,
                    depthAlphaMin,
                    depthFalloff);
            }
        }
    }
}
