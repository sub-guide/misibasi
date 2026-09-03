using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>메인 3릴 뷰. ↑↓는 Director. 한 칸은 bounceCurve. Space 스핀 없음.</summary>
    [DefaultExecutionOrder(40)]
    public sealed class MainMenuReelController : MonoBehaviour
    {
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

        [Header("실루엣")]
        [SerializeField] Color silhouetteColor = Color.black;

        GameCatalogEntry[] _catalog;
        bool _settling;
        float _bounceElapsed;
        int _bounceDelta;

        public int SelectedIndex { get; private set; }

        public bool IsSettling => _settling;

        public bool LockInputUntilSettled => lockInputUntilSettled;

        public void BindCatalog(GameCatalogEntry[] catalog, int selectedIndex = 0)
        {
            _catalog = catalog;
            int length = catalog != null ? catalog.Length : 0;
            SelectedIndex = GameCatalogEntry.WrapIndex(selectedIndex, length);
            _settling = false;
            _bounceElapsed = 0f;
            ApplyLayout(0f);
        }

        /// <summary>인덱스는 Director가 wrap한 값. 릴은 바운스만.</summary>
        public void PlayStep(int selectedIndex, int delta)
        {
            if (_catalog == null || _catalog.Length == 0)
                return;

            SelectedIndex = GameCatalogEntry.WrapIndex(selectedIndex, _catalog.Length);
            _bounceDelta = delta;
            _bounceElapsed = 0f;
            _settling = duration > 0f && slotHeight > 0f;
            ApplyLayout(CurrentBounceOffset());
        }

        void Update()
        {
            if (!_settling)
                return;

            _bounceElapsed += Time.unscaledDeltaTime;
            if (_bounceElapsed >= duration)
            {
                _settling = false;
                _bounceElapsed = 0f;
                ApplyLayout(0f);
                return;
            }

            ApplyLayout(CurrentBounceOffset());
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
                    silhouetteColor);
            }
        }
    }
}
