using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Flow
{
    /// <summary>릴 한 칸. Image는 프리팹 Inspector에서 연결한다.</summary>
    public sealed class ReelSymbolView : MonoBehaviour
    {
        [SerializeField] Image iconImage;

        RectTransform _rect;

        public RectTransform Rect
        {
            get
            {
                CacheRect();
                return _rect;
            }
        }

        void Awake() => CacheRect();

        void CacheRect()
        {
            if (_rect == null)
                _rect = (RectTransform)transform;
        }

        public void Bind(GameCatalogEntry entry)
        {
            if (iconImage == null)
                return;

            iconImage.sprite = entry != null ? entry.icon : null;
            iconImage.enabled = true;
        }

        public void SetAnchoredY(float y)
        {
            CacheRect();
            Vector2 p = _rect.anchoredPosition;
            p.y = y;
            _rect.anchoredPosition = p;
        }

        public void ApplyVisual(bool isCenter, Color accent, Color silhouette, float scale, float alpha)
        {
            CacheRect();
            _rect.localScale = new Vector3(scale, scale, 1f);

            if (iconImage == null)
                return;

            Color c = isCenter ? accent : silhouette;
            c.a *= alpha;
            iconImage.color = c;
        }
    }
}
