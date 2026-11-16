using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        void ResetStageBackgroundAtBegin(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            if (!_aliveMask[i])
            {
                SetStageBackgroundAlphasImmediate(ui, space: 0f, club: 0f);
                return;
            }

            ResolveStageBackground(i, out float space, out float club);
            SetStageBackgroundAlphasImmediate(ui, space, club);
        }

        void TickStageBackground(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            if (!_aliveMask[i])
            {
                SetStageBackgroundAlphasImmediate(ui, space: 0f, club: 0f);
                return;
            }

            ResolveStageBackground(i, out float space, out float club);
            SetStageBackgroundAlphasImmediate(ui, space, club);
        }

        /// <summary>
        /// 빌드업은 Space. 연타 중 티어 1은 StageScreen 기본 이미지(둘 다 끔),
        /// 티어 2는 Club, 티어 3은 Space.
        /// </summary>
        void ResolveStageBackground(int i, out float space, out float club)
        {
            if (!_tier2Active)
            {
                space = 1f;
                club = 0f;
                return;
            }

            int tier = PlayerTier(i);
            if (tier >= PlayerTierMax)
            {
                space = 1f;
                club = 0f;
                return;
            }

            if (tier >= 2)
            {
                space = 0f;
                club = 1f;
                return;
            }

            space = 0f;
            club = 0f;
        }

        static void SetStageBackgroundAlphasImmediate(SlotUiBindings ui, float space, float club)
        {
            SetGraphicAlpha(ui.StageBackgroundSpace, space);
            SetGraphicAlpha(ui.StageBackgroundClub, club);
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
