using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        static void AssignDefaultButtonMapping(ref SlotRuntime sr)
        {
            sr.MapO = OiiaPhysicalButton.X;
            sr.MapI = OiiaPhysicalButton.A;
            sr.MapA = OiiaPhysicalButton.Y;
        }

        void ShuffleButtonMapping(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];

            var pool = new[]
            {
                OiiaPhysicalButton.X,
                OiiaPhysicalButton.Y,
                OiiaPhysicalButton.A,
                OiiaPhysicalButton.B
            };

            for (var i = pool.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            sr.MapO = pool[0];
            sr.MapI = pool[1];
            sr.MapA = pool[2];
        }

        static Image GuideButtonForPhysical(SlotUiBindings b, OiiaPhysicalButton button) =>
            button switch
            {
                OiiaPhysicalButton.X => b.GuideButtonX,
                OiiaPhysicalButton.Y => b.GuideButtonY,
                OiiaPhysicalButton.A => b.GuideButtonA,
                OiiaPhysicalButton.B => b.GuideButtonB,
                _ => null
            };

        static Image GuideButtonForPatternLetter(SlotUiBindings b, ref SlotRuntime sr, char letterLower)
        {
            OiiaPhysicalButton physical = PatternLetterToPhysical(ref sr, letterLower);
            return GuideButtonForPhysical(b, physical);
        }

        static OiiaPhysicalButton PatternLetterToPhysical(ref SlotRuntime sr, char letterLower) =>
            letterLower switch
            {
                'o' => sr.MapO,
                'i' => sr.MapI,
                'a' => sr.MapA,
                _ => sr.MapO
            };

        static bool WasPhysicalPressed(int slotIndex, Joystick pad, OiiaPhysicalButton button)
        {
            return button switch
            {
                OiiaPhysicalButton.X => BoothUsbSlotInput.PrimaryTriggerWasPressed(slotIndex, pad),
                OiiaPhysicalButton.Y => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceY),
                OiiaPhysicalButton.A => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceA),
                OiiaPhysicalButton.B => BoothUsbSlotInput.WasPathPressed(slotIndex, pad, BoothUsbGamepadLayout.FaceB),
                _ => false
            };
        }

        static bool IsPhysicalHeld(int slotIndex, Joystick pad, OiiaPhysicalButton button)
        {
            return button switch
            {
                OiiaPhysicalButton.X => BoothUsbSlotInput.PrimaryTriggerIsHeld(slotIndex, pad),
                OiiaPhysicalButton.Y => BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.FaceY),
                OiiaPhysicalButton.A => BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.FaceA),
                OiiaPhysicalButton.B => BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.FaceB),
                _ => false
            };
        }
    }
}
