using System;
using UnityEngine;

namespace MiniParty.Flow
{
    [Serializable]
    public sealed class GameCatalogEntry
    {
        public string id;
        public string title;
        [TextArea(2, 6)] public string blurb;

        [Tooltip("릴 심볼 아이콘.")]
        public Sprite icon;

        [Tooltip("중앙 정차 시 아이콘 틴트. 기본 흰색.")]
        public Color accentColor = Color.white;

        public static int WrapIndex(int value, int length)
        {
            if (length <= 0)
                return 0;

            int m = value % length;
            return m < 0 ? m + length : m;
        }
    }
}
