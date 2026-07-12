using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("전광판 배경")]
        [Tooltip("티어 배경 크로스페이드 시간(초).")]
        [SerializeField, Min(0.05f)]
        float stageBgCrossfadeSeconds = 0.4f;

        void ResetStageBackgroundAtBegin(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            if (!_aliveMask[i])
            {
                SetStageBackgroundAlphasImmediate(ui, chroma: 0f, space: 0f, club: 0f);
                return;
            }

            // Begin / 연습: T1 ChromaKey
            SetStageBackgroundAlphasImmediate(ui, chroma: 1f, space: 0f, club: 0f);
        }

        void TickStageBackground(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            if (!_aliveMask[i])
            {
                SetStageBackgroundAlphasImmediate(ui, chroma: 0f, space: 0f, club: 0f);
                return;
            }

            int tier = ResolveGlobalTier();
            float targetChroma = tier <= 1 ? 1f : 0f;
            float targetSpace = tier == 2 ? 1f : 0f;
            float targetClub = tier >= 3 ? 1f : 0f;

            float speed = 1f / Mathf.Max(0.05f, stageBgCrossfadeSeconds);
            float step = speed * Time.deltaTime;

            LerpGraphicAlpha(ui.StageBackgroundChromaKey, targetChroma, step);
            LerpGraphicAlpha(ui.StageBackgroundSpace, targetSpace, step);
            LerpGraphicAlpha(ui.StageBackgroundClub, targetClub, step);
        }

        static void SetStageBackgroundAlphasImmediate(SlotUiBindings ui, float chroma, float space, float club)
        {
            SetGraphicAlpha(ui.StageBackgroundChromaKey, chroma);
            SetGraphicAlpha(ui.StageBackgroundSpace, space);
            SetGraphicAlpha(ui.StageBackgroundClub, club);
        }

        static void LerpGraphicAlpha(Graphic g, float target, float step)
        {
            if (g == null)
                return;

            Color c = g.color;
            c.a = Mathf.MoveTowards(c.a, target, step);
            g.color = c;

            // 완전 투명이면 레이캐스트·드로우 부담 줄이기(선택적 활성).
            bool show = c.a > 0.001f;
            if (g.gameObject.activeSelf != show)
                g.gameObject.SetActive(show);
        }

        static void SetGraphicAlpha(Graphic g, float alpha)
        {
            if (g == null)
                return;

            Color c = g.color;
            c.a = Mathf.Clamp01(alpha);
            g.color = c;

            bool show = c.a > 0.001f;
            if (g.gameObject.activeSelf != show)
                g.gameObject.SetActive(show);
        }
    }
}
