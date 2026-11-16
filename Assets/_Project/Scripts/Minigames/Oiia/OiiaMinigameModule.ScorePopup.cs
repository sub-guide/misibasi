using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const int ScorePopupPerTier = 4;
        const float ScorePopupDurationSeconds = 0.2f;
        const float ScorePopupRise = 40f;

        struct ScorePopupFx
        {
            public TMP_Text Text;
            public float Elapsed;
            public Vector2 Start;
            public Color BaseColor;
        }

        ScorePopupFx[][] _scorePopups;

        void ResetScorePopups(int slotIndex)
        {
            EnsureScorePopups(slotIndex);
            ScorePopupFx[] pool = _scorePopups[slotIndex];
            if (pool == null)
                return;

            for (var i = 0; i < pool.Length; i++)
            {
                if (pool[i].Text == null)
                    continue;

                pool[i].Elapsed = -1f;
                SetScorePopupAlpha(ref pool[i], 0f);
                pool[i].Text.gameObject.SetActive(false);
            }
        }

        void EnsureScorePopups(int slotIndex)
        {
            if (_scorePopups == null)
                _scorePopups = new ScorePopupFx[SlotCount][];

            if (_scorePopups[slotIndex] != null)
                return;

            var pool = new ScorePopupFx[PlayerTierMax * ScorePopupPerTier];
            if (TryGetBinding(slotIndex, out SlotUiBindings b) && b.HudScoreText != null)
            {
                EnsureScoreFront(b.HudScoreText);
                CollectScorePopups(b.HudScoreText.transform, pool);
            }

            _scorePopups[slotIndex] = pool;
        }

        /// <summary>
        /// 부모 글자는 자식보다 뒤에 그려진다. 점수 숫자만 마지막 자식으로 옮겨 이펙트 앞에 둔다.
        /// </summary>
        static void EnsureScoreFront(TMP_Text score)
        {
            Transform existing = score.transform.Find("ScoreFront");
            TMP_Text front;
            if (existing != null)
            {
                front = existing.GetComponent<TMP_Text>();
            }
            else
            {
                var go = new GameObject("ScoreFront", typeof(RectTransform));
                go.transform.SetParent(score.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;

                front = go.AddComponent<TextMeshProUGUI>();
                front.font = score.font;
                front.fontSharedMaterial = score.fontSharedMaterial;
                front.fontSize = score.fontSize;
                front.enableAutoSizing = score.enableAutoSizing;
                front.fontSizeMin = score.fontSizeMin;
                front.fontSizeMax = score.fontSizeMax;
                front.fontStyle = score.fontStyle;
                front.color = score.color;
                front.alignment = score.alignment;
                front.enableWordWrapping = score.enableWordWrapping;
                front.overflowMode = score.overflowMode;
                front.richText = score.richText;
                front.characterSpacing = score.characterSpacing;
                front.raycastTarget = false;
                front.text = score.text;
            }

            if (front != null)
                front.transform.SetAsLastSibling();

            score.enabled = false;
        }

        static void CollectScorePopups(Transform score, ScorePopupFx[] pool)
        {
            var counts = new int[PlayerTierMax];
            for (var c = 0; c < score.childCount; c++)
            {
                Transform child = score.GetChild(c);
                int tier = ScorePopupTierFromName(child.name);
                if (tier < PlayerTierMin || tier > PlayerTierMax)
                    continue;

                int slotInTier = counts[tier - 1];
                if (slotInTier >= ScorePopupPerTier)
                    continue;

                TMP_Text tmp = child.GetComponent<TMP_Text>();
                if (tmp == null)
                    continue;

                counts[tier - 1] = slotInTier + 1;
                pool[(tier - 1) * ScorePopupPerTier + slotInTier] = new ScorePopupFx
                {
                    Text = tmp,
                    Elapsed = -1f,
                    BaseColor = tmp.color
                };
            }
        }

        static int ScorePopupTierFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            if (name.StartsWith("T3"))
                return 3;
            if (name.StartsWith("T2"))
                return 2;
            if (name.StartsWith("T1"))
                return 1;

            return 0;
        }

        /// <summary>연타 입력으로 점수가 오른 직후. <paramref name="tier"/>는 점수를 준 티어.</summary>
        void PlayScorePopup(int slotIndex, int tier)
        {
            if (tier < PlayerTierMin || tier > PlayerTierMax)
                return;

            EnsureScorePopups(slotIndex);
            ScorePopupFx[] pool = _scorePopups[slotIndex];
            if (pool == null)
                return;

            int origin = (tier - 1) * ScorePopupPerTier;
            int pick = -1;
            float oldest = -1f;
            for (var n = 0; n < ScorePopupPerTier; n++)
            {
                int index = origin + n;
                if (pool[index].Text == null)
                    continue;

                if (pool[index].Elapsed < 0f)
                {
                    pick = index;
                    break;
                }

                if (pool[index].Elapsed > oldest)
                {
                    oldest = pool[index].Elapsed;
                    pick = index;
                }
            }

            if (pick < 0)
                return;

            ref ScorePopupFx fx = ref pool[pick];
            fx.Elapsed = 0f;
            fx.Start = new Vector2(Random.Range(-150f, 150f), Random.Range(-20f, 20f));
            fx.Text.rectTransform.anchoredPosition = fx.Start;
            SetScorePopupAlpha(ref fx, 1f);
            fx.Text.gameObject.SetActive(true);
            fx.Text.transform.parent.Find("ScoreFront")?.SetAsLastSibling();
        }

        void TickScorePopups(int slotIndex)
        {
            if (_scorePopups == null || _scorePopups[slotIndex] == null)
                return;

            ScorePopupFx[] pool = _scorePopups[slotIndex];
            float duration = Mathf.Max(0.01f, ScorePopupDurationSeconds);
            for (var i = 0; i < pool.Length; i++)
            {
                if (pool[i].Text == null || pool[i].Elapsed < 0f)
                    continue;

                pool[i].Elapsed += Time.deltaTime;
                float t = pool[i].Elapsed / duration;
                if (t >= 1f)
                {
                    pool[i].Elapsed = -1f;
                    SetScorePopupAlpha(ref pool[i], 0f);
                    pool[i].Text.gameObject.SetActive(false);
                    continue;
                }

                pool[i].Text.rectTransform.anchoredPosition =
                    pool[i].Start + new Vector2(0f, ScorePopupRise * t);
                SetScorePopupAlpha(ref pool[i], 1f - t);
            }
        }

        static void SetScorePopupAlpha(ref ScorePopupFx fx, float alpha)
        {
            if (fx.Text == null)
                return;

            Color c = fx.BaseColor;
            c.a = fx.BaseColor.a * Mathf.Clamp01(alpha);
            fx.Text.color = c;
        }
    }
}
