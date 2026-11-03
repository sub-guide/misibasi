using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    /// <summary>
    /// 슬롯별 위·아래 연타.
    /// 패드: Stick Up / Stick Down (<see cref="BoothUsbSlotInput"/> — 1P는 Ctrl 키보드 디버그 W/S 포함).
    /// </summary>
    public sealed class FlounderInput : MonoBehaviour
    {
        [SerializeField] int playerSlotIndex;

        public int SlotIndex => playerSlotIndex;

        public bool PollUpperFlap() => PadWasPressed(BoothUsbGamepadLayout.StickUp);

        public bool PollLowerFlap() => PadWasPressed(BoothUsbGamepadLayout.StickDown);

        public bool IsUpperHeld() => PadIsHeld(BoothUsbGamepadLayout.StickUp);

        public bool IsLowerHeld() => PadIsHeld(BoothUsbGamepadLayout.StickDown);

        bool PadWasPressed(string path) =>
            BoothUsbSlotInput.WasPathPressed(playerSlotIndex, SlotGamepad.Get(playerSlotIndex), path);

        bool PadIsHeld(string path) =>
            BoothUsbSlotInput.IsPathHeld(playerSlotIndex, SlotGamepad.Get(playerSlotIndex), path);
    }
}
