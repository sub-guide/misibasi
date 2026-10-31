using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        static readonly float[] FailSpriteRandomAnchorXs =
        {
            1716.5f,
            1200f,
            480f,
            -260f,
            -800f,
            -1250f,
            -1600f,
        };

        readonly Color[] _failSpriteRestColor = new Color[SlotCount];
        readonly bool[] _failSpriteRestColorCaptured = new bool[SlotCount];

        void ResetFailFallVisual(int i, CoffinDanceSlotBindings bind)
        {
            if (!TryGetFailSpriteImage(bind, out Image image))
                return;

            CaptureFailSpriteRestColor(i, image);
            RestoreFailSpriteRestColor(i, image);
            image.rectTransform.gameObject.SetActive(false);
        }

        void BeginFailFall(int i, ref SlotRuntime sr)
        {
            sr.FailFallActive = true;

            CoffinDanceSlotBindings bind = GetBindings(i);
            if (!TryGetFailSpriteImage(bind, out Image image))
                return;

            CaptureFailSpriteRestColor(i, image);

            float x = PickRandomFailSpriteAnchorX();
            SetFailSpriteX(image.rectTransform, x);
            SetFailSpriteAlpha(image, 1f);

            image.gameObject.SetActive(true);
        }

        void TickFailFallVisual(int i, ref SlotRuntime sr)
        {
            if (!sr.FailFallActive)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            if (!TryGetFailSpriteImage(bind, out Image image))
                return;

            float dur = sr.FailFloorRecoverDuration;
            float u = dur <= 0f ? 1f : Mathf.Clamp01(sr.FailFloorRecoverElapsed / dur);
            float fadeStart = Mathf.Clamp01(failSpriteFadeStartNormalized);

            float alpha = 1f;
            if (u > fadeStart)
            {
                float span = Mathf.Max(0.0001f, 1f - fadeStart);
                float t = (u - fadeStart) / span;
                alpha = 1f - Mathf.Clamp01(t);
            }

            SetFailSpriteAlpha(image, alpha);
        }

        void EndFailFall(int i, ref SlotRuntime sr)
        {
            sr.FailFallActive = false;
            ResetFailFallVisual(i, GetBindings(i));
        }

        void CaptureFailSpriteRestColor(int i, Image image)
        {
            if (image == null || i < 0 || i >= SlotCount || _failSpriteRestColorCaptured[i])
                return;

            _failSpriteRestColor[i] = image.color;
            _failSpriteRestColorCaptured[i] = true;
        }

        void RestoreFailSpriteRestColor(int i, Image image)
        {
            if (image == null || i < 0 || i >= SlotCount || !_failSpriteRestColorCaptured[i])
                return;

            image.color = _failSpriteRestColor[i];
        }

        static float PickRandomFailSpriteAnchorX()
        {
            int index = Random.Range(0, FailSpriteRandomAnchorXs.Length);
            return FailSpriteRandomAnchorXs[index];
        }

        static bool TryGetFailSpriteImage(CoffinDanceSlotBindings bind, out Image image)
        {
            image = null;
            RectTransform sprite = bind != null ? bind.FailSprite : null;
            if (sprite == null)
                return false;

            image = sprite.GetComponent<Image>();
            return image != null;
        }

        static void SetFailSpriteX(RectTransform sprite, float x)
        {
            Vector2 pos = sprite.anchoredPosition;
            pos.x = x;
            sprite.anchoredPosition = pos;
        }

        static void SetFailSpriteAlpha(Image image, float alpha01)
        {
            Color c = image.color;
            c.a = Mathf.Clamp01(alpha01);
            image.color = c;
        }
    }
}
