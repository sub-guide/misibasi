using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const float SpotlightNeonLerpSpeed = 0.35f;

        const float SpotlightStrobeIntervalSeconds = 0.05f;

        [Header("Spotlight")]
        [Tooltip("Beam Image 알파 (0–1). Inspector에서 조절. 기본 ≈15/255.")]
        [SerializeField, Range(0f, 1f)]
        float spotlightBeamAlpha = 15f / 255f;

        static readonly Color SpotlightFixtureColor = Color.white;

        static readonly Color[] SpotlightNeonColors =
        {
            new(1f, 0.2f, 0.75f, 1f),
            new(0.15f, 0.95f, 1f, 1f),
            new(0.65f, 0.25f, 1f, 1f),
        };

        void ResetSpotlightAtBegin(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            EnsureSlotStageClipMask(ui);
            SetSpotlightRootsActive(ui, active: true);
            ApplySpotlightVisuals(ui, beamRgb: Color.white, beamVisible: false);
        }

        /// <summary>
        /// StageScreen(및 스포트라이트 부모)에 RectMask2D로 Beam이 인접 슬롯으로 넘치지 않게 클립.
        /// </summary>
        static void EnsureSlotStageClipMask(SlotUiBindings ui)
        {
            EnsureRectMask2D(ui.StageScreenRoot);

            if (ui.SpotlightLRoot != null)
                EnsureRectMask2D(ui.SpotlightLRoot.parent as RectTransform);

            if (ui.SpotlightRRoot != null)
                EnsureRectMask2D(ui.SpotlightRRoot.parent as RectTransform);

            // Override Sorting Canvas는 마스크 밖으로 새어 인접 슬롯 위에 그려질 수 있음.
            if (ui.StageScreenRoot != null)
            {
                Canvas stageCanvas = ui.StageScreenRoot.GetComponent<Canvas>();
                if (stageCanvas != null)
                    stageCanvas.overrideSorting = false;
            }
        }

        static void EnsureRectMask2D(RectTransform rt)
        {
            if (rt == null)
                return;

            if (rt.GetComponent<RectMask2D>() == null)
                rt.gameObject.AddComponent<RectMask2D>();
        }

        void TickSpotlight(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings ui))
                return;

            if (ui.SpotlightLRoot == null && ui.SpotlightRRoot == null)
                return;

            if (!_aliveMask[i] || !_tier2Active)
            {
                SetSpotlightRootsActive(ui, active: false);
                return;
            }

            EnsureSlotStageClipMask(ui);
            SetSpotlightRootsActive(ui, active: true);

            Color neon = EvaluateSpotlightNeonColor();
            bool strobeOn = EvaluateStrobeOn(SpotlightStrobeIntervalSeconds);
            ApplySpotlightVisuals(ui, beamRgb: neon, beamVisible: strobeOn);
        }

        static Color EvaluateSpotlightNeonColor()
        {
            float t = Time.unscaledTime * SpotlightNeonLerpSpeed;
            int count = SpotlightNeonColors.Length;
            int i0 = Mathf.FloorToInt(t) % count;
            if (i0 < 0)
                i0 += count;

            int i1 = (i0 + 1) % count;
            float f = t - Mathf.Floor(t);
            return Color.Lerp(SpotlightNeonColors[i0], SpotlightNeonColors[i1], f);
        }

        static bool EvaluateStrobeOn(float intervalSeconds)
        {
            float step = Mathf.Max(0.02f, intervalSeconds);
            int beat = Mathf.FloorToInt(Time.unscaledTime / step);
            return (beat & 1) == 0;
        }

        static void SetSpotlightRootsActive(SlotUiBindings ui, bool active)
        {
            if (ui.SpotlightLRoot != null)
                ui.SpotlightLRoot.gameObject.SetActive(active);

            if (ui.SpotlightRRoot != null)
                ui.SpotlightRRoot.gameObject.SetActive(active);
        }

        /// <summary>
        /// Fixture는 고정색. Beam만 <paramref name="beamRgb"/> / 가시성 변경.
        /// </summary>
        void ApplySpotlightVisuals(SlotUiBindings ui, Color beamRgb, bool beamVisible)
        {
            Color fixture = SpotlightFixtureColor;
            fixture.a = 1f;

            Color beam = beamRgb;
            beam.a = beamVisible ? spotlightBeamAlpha : 0f;

            SetImageColor(ui.SpotlightLFixture, fixture);
            SetImageColor(ui.SpotlightLBeam, beam);
            SetImageColor(ui.SpotlightRFixture, fixture);
            SetImageColor(ui.SpotlightRBeam, beam);

            SetImageRaycastOff(ui.SpotlightLFixture);
            SetImageRaycastOff(ui.SpotlightLBeam);
            SetImageRaycastOff(ui.SpotlightRFixture);
            SetImageRaycastOff(ui.SpotlightRBeam);
        }

        static void SetImageRaycastOff(Image img)
        {
            if (img != null)
                img.raycastTarget = false;
        }

        static void SetImageColor(Image img, Color c)
        {
            if (img != null)
                img.color = c;
        }
    }
}
