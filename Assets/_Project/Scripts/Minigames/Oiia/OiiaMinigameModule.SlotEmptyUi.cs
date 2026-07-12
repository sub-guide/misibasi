using MiniParty.Core;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const float WaitingTextPulseSpeed = 2.2f;

        const float WaitingTextAlphaMin = 0.2f;

        const float WaitingTextAlphaMax = 1f;

        bool IsSlotEmptyForUi(int i)
        {
            return _ctx.Slots != null && i >= 0 && i < _ctx.Slots.Length && _ctx.Slots[i].State == SlotState.EMPTY;
        }

        static Color WaitingTextPulseColor()
        {
            float t = Mathf.Abs(Mathf.Sin(Time.unscaledTime * WaitingTextPulseSpeed));
            float alpha = Mathf.Lerp(WaitingTextAlphaMin, WaitingTextAlphaMax, t);
            return new Color(0.9f, 0.9f, 0.9f, alpha);
        }
    }
}
