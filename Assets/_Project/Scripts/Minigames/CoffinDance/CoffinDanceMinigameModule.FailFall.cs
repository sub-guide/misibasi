using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void ResetFailFallVisual(CoffinDanceSlotBindings bind)
        {
            RectTransform sprite = bind != null ? bind.FailSprite : null;
            if (sprite == null)
                return;

            SetFailSpriteX(sprite, FailSpriteStartX);
            sprite.gameObject.SetActive(false);
        }

        void BeginFailFall(int i, ref SlotRuntime sr)
        {
            sr.FailFallActive = true;

            RectTransform sprite = GetBindings(i)?.FailSprite;
            if (sprite == null)
                return;

            SetFailSpriteX(sprite, FailSpriteStartX);
            sprite.gameObject.SetActive(true);
        }

        /// <summary>복구 경과와 같은 u로 X를 등속 이동한다. 끄기는 EndFailFall.</summary>
        void ApplyFailFallAlongRecover(int i, ref SlotRuntime sr)
        {
            if (!sr.FailFallActive)
                return;

            float dur = sr.FailFloorRecoverDuration;
            float u = dur <= 0f ? 1f : Mathf.Clamp01(sr.FailFloorRecoverElapsed / dur);
            float x = Mathf.Lerp(FailSpriteStartX, FailSpriteEndX, u);

            RectTransform sprite = GetBindings(i)?.FailSprite;
            if (sprite != null)
                SetFailSpriteX(sprite, x);
        }

        void EndFailFall(int i, ref SlotRuntime sr)
        {
            sr.FailFallActive = false;
            ResetFailFallVisual(GetBindings(i));
        }

        static void SetFailSpriteX(RectTransform sprite, float x)
        {
            Vector2 pos = sprite.anchoredPosition;
            pos.x = x;
            sprite.anchoredPosition = pos;
        }
    }
}
