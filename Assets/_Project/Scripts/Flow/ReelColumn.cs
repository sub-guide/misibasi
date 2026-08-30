using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>세로 릴 한 줄. 심볼 풀 생성·순환 배치·실루엣/깊이 적용.</summary>
    public sealed class ReelColumn : MonoBehaviour
    {
        [Tooltip("심볼 부모. 비우면 이 오브젝트의 RectTransform.")]
        [SerializeField] RectTransform contentRoot;

        ReelSymbolView[] _symbols;
        float[] _ys;
        GameCatalogEntry[] _boundEntries;

        public void EnsurePool(ReelSymbolView prefab, int count)
        {
            if (count < 1)
                return;

            if (contentRoot == null)
                contentRoot = (RectTransform)transform;

            if (_symbols != null && _symbols.Length == count)
                return;

            if (contentRoot.childCount >= count)
            {
                _symbols = new ReelSymbolView[count];
                for (var i = 0; i < count; i++)
                    _symbols[i] = contentRoot.GetChild(i).GetComponent<ReelSymbolView>();

                return;
            }

            if (prefab == null)
            {
                Debug.LogError("[ReelColumn] symbolPrefab 이 비었습니다. Inspector에 프리팹을 연결하세요.", this);
                return;
            }

            int existing = contentRoot.childCount;
            for (var i = existing; i < count; i++)
                Instantiate(prefab, contentRoot);

            _symbols = new ReelSymbolView[count];
            for (var i = 0; i < count; i++)
                _symbols[i] = contentRoot.GetChild(i).GetComponent<ReelSymbolView>();
        }

        public void ApplyLayout(
            GameCatalogEntry[] catalog,
            int selectedIndex,
            float scrollOffset,
            float slotHeight,
            Color silhouetteColor,
            float depthScaleMin,
            float depthAlphaMin,
            AnimationCurve depthFalloff)
        {
            if (_symbols == null || _symbols.Length == 0)
                return;

            if (catalog == null || catalog.Length == 0)
                return;

            int n = _symbols.Length;
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
                if (_symbols[i] == null)
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
                ReelSymbolView symbol = _symbols[i];
                if (symbol == null)
                    continue;

                GameCatalogEntry entry = _boundEntries[i];
                symbol.Bind(entry);
                symbol.SetAnchoredY(_ys[i]);

                float dist = slotHeight > 0.0001f ? Mathf.Abs(_ys[i]) / slotHeight : 0f;
                float t = depthFalloff != null && depthFalloff.length > 0
                    ? Mathf.Clamp01(depthFalloff.Evaluate(dist))
                    : Mathf.Clamp01(dist);

                float scale = Mathf.Lerp(1f, depthScaleMin, t);
                float alpha = Mathf.Lerp(1f, depthAlphaMin, t);
                Color accent = entry != null ? entry.accentColor : Color.white;
                symbol.ApplyVisual(i == closest, accent, silhouetteColor, scale, alpha);
            }
        }
    }
}
