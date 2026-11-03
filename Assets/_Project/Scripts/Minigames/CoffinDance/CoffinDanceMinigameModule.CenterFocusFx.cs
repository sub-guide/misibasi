using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        static readonly int CenterFocusAnimState = Animator.StringToHash("Focus");

        readonly Color[] _centerFocusRestColor = new Color[SlotCount];
        readonly bool[] _centerFocusRestColorCaptured = new bool[SlotCount];
        readonly bool[] _centerFocusAnimStarted = new bool[SlotCount];

        void ResetCenterFocusVisual(int i, CoffinDanceSlotBindings bind)
        {
            if (i >= 0 && i < SlotCount)
                _centerFocusAnimStarted[i] = false;

            if (!TryGetCenterFocusImage(bind, out Image image))
                return;

            CaptureCenterFocusRestColor(i, image);
            RestoreCenterFocusRestColor(i, image);
            SetCenterFocusAlpha(image, 0f);
            image.rectTransform.gameObject.SetActive(false);

            Animator animator = image.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;
        }

        void ApplyCenterFocusFx(int i, ref SlotRuntime sr, CoffinDanceSlotBindings bind)
        {
            if (!TryGetCenterFocusImage(bind, out Image image))
                return;

            float alpha = sr.CamFovBlend;
            GameObject root = image.rectTransform.gameObject;

            if (alpha <= 0f)
            {
                if (root.activeSelf)
                {
                    SetCenterFocusAlpha(image, 0f);
                    root.SetActive(false);
                }

                if (i >= 0 && i < SlotCount)
                    _centerFocusAnimStarted[i] = false;

                Animator offAnim = image.GetComponent<Animator>();
                if (offAnim != null)
                    offAnim.enabled = false;
                return;
            }

            if (!root.activeSelf)
            {
                CaptureCenterFocusRestColor(i, image);
                root.SetActive(true);
            }

            SetCenterFocusAlpha(image, alpha);
            EnsureCenterFocusAnimPlaying(i, image);
        }

        void EnsureCenterFocusAnimPlaying(int i, Image image)
        {
            Animator animator = image.GetComponent<Animator>();
            if (animator == null)
                return;

            animator.enabled = true;

            if (i >= 0 && i < SlotCount && _centerFocusAnimStarted[i])
                return;

            animator.Play(CenterFocusAnimState, 0, 0f);
            animator.Update(0f);

            if (i >= 0 && i < SlotCount)
                _centerFocusAnimStarted[i] = true;
        }

        void CaptureCenterFocusRestColor(int i, Image image)
        {
            if (image == null || i < 0 || i >= SlotCount || _centerFocusRestColorCaptured[i])
                return;

            _centerFocusRestColor[i] = image.color;
            _centerFocusRestColorCaptured[i] = true;
        }

        void RestoreCenterFocusRestColor(int i, Image image)
        {
            if (image == null || i < 0 || i >= SlotCount || !_centerFocusRestColorCaptured[i])
                return;

            Color c = _centerFocusRestColor[i];
            c.a = image.color.a;
            image.color = c;
        }

        static bool TryGetCenterFocusImage(CoffinDanceSlotBindings bind, out Image image)
        {
            image = null;
            RectTransform focus = bind != null ? bind.CenterFocus : null;
            if (focus == null)
                return false;

            image = focus.GetComponent<Image>();
            return image != null;
        }

        static void SetCenterFocusAlpha(Image image, float alpha01)
        {
            Color c = image.color;
            c.a = Mathf.Clamp01(alpha01);
            image.color = c;
        }
    }
}
