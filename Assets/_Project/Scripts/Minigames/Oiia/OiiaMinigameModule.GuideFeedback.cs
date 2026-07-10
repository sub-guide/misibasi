using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const string NeonOutlineChildName = "Neon_Outline";
        const string NeonShockwaveChildName = "Neon_Shockwave";

        readonly Image[][] _guideNeonOutline = CreateGuideNeonGrid();
        readonly Image[][] _guideNeonShockwave = CreateGuideNeonGrid();
        readonly NeonShockwaveFx[][] _neonShockwaveFx = CreateNeonShockwaveFxGrid();
        readonly Vector3[][] _guideButtonRestScale = CreateGuideScaleGrid();
        readonly bool[] _guideButtonScaleCaptured = new bool[SlotCount];

        static Image[][] CreateGuideNeonGrid()
        {
            var grid = new Image[SlotCount][];
            for (var i = 0; i < SlotCount; i++)
                grid[i] = new Image[GuideButtonsPerSlot];

            return grid;
        }

        static NeonShockwaveFx[][] CreateNeonShockwaveFxGrid()
        {
            var grid = new NeonShockwaveFx[SlotCount][];
            for (var i = 0; i < SlotCount; i++)
                grid[i] = new NeonShockwaveFx[GuideButtonsPerSlot];

            return grid;
        }

        static Vector3[][] CreateGuideScaleGrid()
        {
            var grid = new Vector3[SlotCount][];
            for (var i = 0; i < SlotCount; i++)
                grid[i] = new Vector3[GuideButtonsPerSlot];

            return grid;
        }

        static int GuideButtonIndexForPhysical(OiiaPhysicalButton button) =>
            button switch
            {
                OiiaPhysicalButton.Y => 0,
                OiiaPhysicalButton.X => 1,
                OiiaPhysicalButton.A => 2,
                OiiaPhysicalButton.B => 3,
                _ => -1
            };

        void ResetAllGuideFeedback()
        {
            ForEachSlot(ResetGuideFeedbackSlot);
        }

        void ResetGuideFeedbackSlot(int i)
        {
            _guideButtonScaleCaptured[i] = false;
            ClearAllNeonShockwavesForSlot(i);

            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            Image[] buttons = GuideButtonsArray(b);
            for (var k = 0; k < GuideButtonsPerSlot; k++)
            {
                Image outline = _guideNeonOutline[i][k];
                if (outline != null)
                {
                    outline.gameObject.SetActive(false);
                    ResetNeonVisual(outline);
                }

                Image shock = _guideNeonShockwave[i][k];
                if (shock != null)
                {
                    shock.gameObject.SetActive(false);
                    ResetNeonVisual(shock);
                }

                Image btn = buttons[k];
                if (btn != null)
                    ApplyGuideButtonRestVisual(btn, _guideButtonRestScale[i][k]);
            }
        }

        void ClearAllNeonShockwavesForSlot(int slotIndex)
        {
            NeonShockwaveFx[] fxRow = _neonShockwaveFx[slotIndex];
            if (fxRow == null)
                return;

            for (var k = 0; k < GuideButtonsPerSlot; k++)
                fxRow[k] = default;
        }

        static Image[] GuideButtonsArray(SlotUiBindings b) =>
            new[] { b.GuideButtonY, b.GuideButtonX, b.GuideButtonA, b.GuideButtonB };

        void EnsureGuideNeonCaptured(int i, SlotUiBindings b)
        {
            Image[] buttons = GuideButtonsArray(b);

            if (!_guideButtonScaleCaptured[i])
            {
                for (var k = 0; k < GuideButtonsPerSlot; k++)
                {
                    Image btn = buttons[k];
                    if (btn == null)
                        continue;

                    _guideButtonRestScale[i][k] = btn.rectTransform.localScale;
                }

                _guideButtonScaleCaptured[i] = true;
            }

            for (var k = 0; k < GuideButtonsPerSlot; k++)
            {
                Image btn = buttons[k];
                if (btn == null)
                    continue;

                if (_guideNeonOutline[i][k] == null)
                {
                    Transform outlineT = btn.transform.Find(NeonOutlineChildName);
                    _guideNeonOutline[i][k] = outlineT != null ? outlineT.GetComponent<Image>() : null;
                }

                if (_guideNeonShockwave[i][k] == null)
                    _guideNeonShockwave[i][k] = ResolveNeonShockwaveImage(btn, _guideNeonOutline[i][k]);
            }
        }

        static Image ResolveNeonShockwaveImage(Image buttonImage, Image outlineTemplate)
        {
            if (buttonImage == null)
                return null;

            Transform btnT = buttonImage.transform;
            Transform shockT = btnT.Find(NeonShockwaveChildName);
            if (shockT != null)
                return shockT.GetComponent<Image>();

            if (outlineTemplate == null)
                return null;

            Image shock = Instantiate(outlineTemplate, btnT);
            shock.name = NeonShockwaveChildName;
            shock.gameObject.SetActive(false);
            ResetNeonVisual(shock);
            shock.rectTransform.SetAsLastSibling();
            return shock;
        }

        void UpdateGuideHoldFeedback(int i, Joystick pad)
        {
            if (!TryGetBinding(i, out SlotUiBindings b) || b.ControllerGuideRoot == null)
                return;

            if (!_aliveMask[i] || IsSlotEmptyForUi(i))
                return;

            if (IsDevGodModeSlot(i))
            {
                UpdateGuideHoldFeedbackDevGodMode1P(i, pad);
                return;
            }

            EnsureGuideNeonCaptured(i, b);

            Image[] buttons = GuideButtonsArray(b);
            var physicalButtons = new[]
            {
                OiiaPhysicalButton.Y,
                OiiaPhysicalButton.X,
                OiiaPhysicalButton.A,
                OiiaPhysicalButton.B
            };

            for (var k = 0; k < GuideButtonsPerSlot; k++)
            {
                Image btn = buttons[k];
                if (btn == null)
                    continue;

                bool held = IsPhysicalHeld(i, pad, physicalButtons[k]);
                Vector3 restScale = _guideButtonRestScale[i][k];

                if (held)
                {
                    btn.rectTransform.localScale = restScale * GuideButtonHoldScale;
                    ApplyGuideButtonBrightness(btn, GuideButtonHoldBrightness);
                }
                else
                {
                    btn.rectTransform.localScale = restScale;
                    ApplyGuideButtonBrightness(btn, GuideButtonIdleBrightness);
                }
            }
        }

        void UpdateGuideNeonTarget(int i, SlotUiBindings b, ref SlotRuntime sr)
        {
            EnsureGuideNeonCaptured(i, b);

            char targetLetter = PatternLowerAt(sr.Cursor);
            OiiaPhysicalButton targetPhysical = PatternLetterToPhysical(ref sr, targetLetter);
            int targetIndex = GuideButtonIndexForPhysical(targetPhysical);
            for (var k = 0; k < GuideButtonsPerSlot; k++)
            {
                Image outline = _guideNeonOutline[i][k];
                if (outline == null)
                    continue;

                bool isTarget = k == targetIndex;
                if (isTarget)
                {
                    if (!outline.gameObject.activeSelf)
                    {
                        outline.gameObject.SetActive(true);
                        ResetNeonVisual(outline);
                    }
                }
                else if (outline.gameObject.activeSelf)
                {
                    outline.gameObject.SetActive(false);
                    ResetNeonVisual(outline);
                }
            }
        }

        static void ResetNeonVisual(Image neon)
        {
            if (neon == null)
                return;

            neon.rectTransform.localScale = Vector3.one;
            Color c = neon.color;
            c.a = 1f;
            neon.color = c;
        }

        static void ApplyGuideButtonBrightness(Image buttonImage, float brightness)
        {
            if (buttonImage == null)
                return;

            float b = Mathf.Clamp01(brightness);
            buttonImage.color = new Color(b, b, b, 1f);
        }

        static void ApplyGuideButtonRestVisual(Image buttonImage, Vector3 restScale)
        {
            if (buttonImage == null)
                return;

            buttonImage.rectTransform.localScale = restScale == Vector3.zero ? Vector3.one : restScale;
            ApplyGuideButtonBrightness(buttonImage, GuideButtonIdleBrightness);
        }

        void TriggerNeonShockwave(int slotIndex, char completedLetterLower)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b))
                return;

            EnsureGuideNeonCaptured(slotIndex, b);

            OiiaPhysicalButton physical = PatternLetterToPhysical(ref _slots[slotIndex], completedLetterLower);
            int btnIndex = GuideButtonIndexForPhysical(physical);
            if (btnIndex < 0)
                return;

            Image shock = _guideNeonShockwave[slotIndex][btnIndex];
            if (shock == null)
                return;

            shock.gameObject.SetActive(true);
            ResetNeonVisual(shock);
            shock.rectTransform.SetAsLastSibling();

            ref NeonShockwaveFx fx = ref _neonShockwaveFx[slotIndex][btnIndex];
            fx.Remaining = NeonShockwaveDuration;
            fx.Duration = NeonShockwaveDuration;
            fx.TargetScale = Random.Range(NeonShockwaveMinScale, NeonShockwaveMaxScale);
            fx.NeonImage = shock;
        }

        void TickNeonShockwave(int i, float deltaTime)
        {
            NeonShockwaveFx[] fxRow = _neonShockwaveFx[i];
            if (fxRow == null)
                return;

            for (var k = 0; k < GuideButtonsPerSlot; k++)
                TickOneNeonShockwave(ref fxRow[k], deltaTime);
        }

        static void TickOneNeonShockwave(ref NeonShockwaveFx fx, float deltaTime)
        {
            if (fx.Remaining <= 0f || fx.NeonImage == null)
                return;

            fx.Remaining -= deltaTime;

            float dur = Mathf.Max(0.0001f, fx.Duration);
            float t = 1f - Mathf.Clamp01(fx.Remaining / dur);
            float scale = Mathf.Lerp(1f, fx.TargetScale, t);

            Image neon = fx.NeonImage;
            neon.rectTransform.localScale = Vector3.one * scale;

            Color c = neon.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            neon.color = c;

            if (fx.Remaining > 0f)
                return;

            neon.gameObject.SetActive(false);
            ResetNeonVisual(neon);
            fx = default;
        }

        void FlushGuideUi(int i, SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.ControllerGuideRoot == null)
                return;

            bool show = _aliveMask[i] && !IsSlotEmptyForUi(i);
            ui.ControllerGuideRoot.SetActive(show);
            if (!show)
                return;

            UpdateGuideNeonTarget(i, ui, ref sr);
        }
    }
}
