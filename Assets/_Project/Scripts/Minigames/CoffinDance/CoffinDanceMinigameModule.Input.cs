using MiniParty.Input;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void ReadBalanceInput(int slotIndex, out float leftHeld, out float rightHeld)
        {
            leftHeld = 0f;
            rightHeld = 0f;

            if (!SlotGamepad.HasInput(slotIndex))
                return;

            Joystick pad = SlotGamepad.Get(slotIndex);

            if (BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.StickLeft))
                leftHeld = 1f;

            if (BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.StickRight))
                rightHeld = 1f;
        }
    }
}
