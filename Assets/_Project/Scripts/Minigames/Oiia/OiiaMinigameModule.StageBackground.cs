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

            SetStageBackgroundAlphasImmediate(
                ui,
                space: _tier2Active ? 0f : 1f,
                club: _tier2Active ? 1f : 0f);
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

            SetStageBackgroundAlphasImmediate(
                ui,
                space: _tier2Active ? 0f : 1f,
                club: _tier2Active ? 1f : 0f);
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
