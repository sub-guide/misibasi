using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Result
{
    /// <summary><c>HpDisplay</c> 아래 <c>Hp_1</c>~<c>Hp_n</c> Image 로 체력을 표시한다.</summary>
    [DisallowMultipleComponent]
    public sealed class ResultHpDisplay : MonoBehaviour
    {
        [SerializeField] Image[] hpIcons;

        static readonly Color HpFull = Color.white;
        static readonly Color HpEmpty = new(1f, 1f, 1f, 0.15f);

        void Awake() => EnsureIcons();

        void EnsureIcons()
        {
            if (hpIcons != null && hpIcons.Length > 0)
                return;

            Transform root = transform;
            var list = new System.Collections.Generic.List<Image>(4);
            for (var i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.TryGetComponent(out Image img))
                    list.Add(img);
            }

            hpIcons = list.ToArray();
        }

        public void SetHp(int current, int max)
        {
            EnsureIcons();
            if (hpIcons == null)
                return;

            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);

            for (var i = 0; i < hpIcons.Length; i++)
            {
                Image icon = hpIcons[i];
                if (icon == null)
                    continue;

                bool filled = i < current;
                icon.gameObject.SetActive(i < max);
                icon.color = filled ? HpFull : HpEmpty;
            }
        }
    }
}
