using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Flow
{
    /// <summary>메인 메뉴·결과 화면 상단에 신성한 중재 표시.</summary>
    public static class SacredMediationBanner
    {
        const string ObjectName = "SacredMediationLabel";

        public static void Sync(Transform parent, bool on)
        {
            if (parent == null)
                return;

            Transform existing = parent.Find(ObjectName);
            if (!on)
            {
                if (existing != null)
                    existing.gameObject.SetActive(false);
                return;
            }

            Text label = existing != null
                ? existing.GetComponent<Text>()
                : Create(parent);

            if (label == null)
                return;

            label.gameObject.SetActive(true);
            label.text = PartySession.SacredMediationDisplayName;
        }

        static Text Create(Transform parent)
        {
            var go = new GameObject(ObjectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -12f);
            rt.sizeDelta = new Vector2(960f, 80f);

            var label = go.AddComponent<Text>();
            label.font = ResolveFont();
            label.fontSize = 40;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.UpperCenter;
            label.color = new Color(1f, 0.92f, 0.55f, 1f);
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.text = PartySession.SacredMediationDisplayName;
            return label;
        }

        static Font ResolveFont()
        {
            Font font = Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "맑은 고딕", "NanumGothic", "Arial" },
                40);

            if (font != null)
                return font;

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
