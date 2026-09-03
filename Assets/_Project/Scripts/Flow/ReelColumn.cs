using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>세로 릴 한 줄. 에디터 심볼만 Recycle. Instantiate 없음.</summary>
    public sealed class ReelColumn : MonoBehaviour
    {
        [Tooltip("위→아래 Symbol_0~4. Bracket 넣지 말 것.")]
        [SerializeField] ReelSymbolView[] symbols;

        float[] _ys;
        GameCatalogEntry[] _boundEntries;
        bool _loggedMissing;

        public void ApplyLayout(
            GameCatalogEntry[] catalog,
            int selectedIndex,
            float scrollOffset,
            float slotHeight,
            Color silhouetteColor)
        {
            if (symbols == null || symbols.Length == 0)
            {
                if (!_loggedMissing)
                {
                    _loggedMissing = true;
                    Debug.LogError("[ReelColumn] symbols 가 비었습니다. Inspector에 Symbol_0~4 를 연결하세요.", this);
                }

                return;
            }

            if (catalog == null || catalog.Length == 0)
                return;

            int n = symbols.Length;
            int center = n / 2;
            float strip = n * slotHeight;
            float halfSpan = n * 0.5f * slotHeight;

            if (_ys == null || _ys.Length != n)
            {
                _ys = new float[n];
                _boundEntries = new GameCatalogEntry[n];
            }

            float closestAbs = float.MaxValue;
            int closest = center;

            for (var i = 0; i < n; i++)
            {
                if (symbols[i] == null)
                    continue;

                float y = (center - i) * slotHeight + scrollOffset;
                int wrapSlots = 0;

                if (strip > 0.0001f)
                {
                    while (y > halfSpan)
                    {
                        y -= strip;
                        wrapSlots++;
                    }

                    while (y < -halfSpan)
                    {
                        y += strip;
                        wrapSlots--;
                    }
                }

                int dataIndex = GameCatalogEntry.WrapIndex(selectedIndex + (i - center) + wrapSlots, catalog.Length);
                _ys[i] = y;
                _boundEntries[i] = catalog[dataIndex];

                float abs = Mathf.Abs(y);
                if (abs < closestAbs)
                {
                    closestAbs = abs;
                    closest = i;
                }
            }

            for (var i = 0; i < n; i++)
            {
                ReelSymbolView symbol = symbols[i];
                if (symbol == null)
                    continue;

                GameCatalogEntry entry = _boundEntries[i];
                symbol.Bind(entry);
                symbol.SetAnchoredY(_ys[i]);

                Color accent = entry != null ? entry.accentColor : Color.white;
                symbol.ApplyVisual(i == closest, accent, silhouetteColor);
            }
        }
    }
}
