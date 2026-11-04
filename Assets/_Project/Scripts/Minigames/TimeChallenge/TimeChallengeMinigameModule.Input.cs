using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        void TickInput()
        {
            TickTableButtons();

            if (_phase != Phase.Running)
                return;

            double now = Time.realtimeSinceStartupAsDouble;

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_clockStarted[i] || _stopped[i])
                    continue;

                if (!WasClockButtonPressed(i))
                    continue;

                _participatedMask[i] = true;
                StopSlot(i, now);
            }
        }

        bool WasClockButtonPressed(int slotIndex)
        {
            return BoothUsbSlotInput.WasPathPressed(slotIndex, SlotGamepad.Get(slotIndex), BoothUsbGamepadLayout.FaceA);
        }

        bool IsClockButtonHeld(int slotIndex)
        {
            return BoothUsbSlotInput.IsPathHeld(slotIndex, SlotGamepad.Get(slotIndex), BoothUsbGamepadLayout.FaceA);
        }

        void StopSlot(int slotIndex, double stopRealtime)
        {
            if (!_clockStarted[slotIndex] || _stopped[slotIndex])
                return;

            _stopped[slotIndex] = true;
            _records[slotIndex] = System.Math.Max(0d, stopRealtime - _slotStartRealtime[slotIndex]);
            PlayStopwatchClick();

            if (debugForce1PRound1Perfect && slotIndex == 0 && _roundIndex == 0)
            {
                _records[slotIndex] = GetRoundTargetSeconds();
                debugForce1PRound1Perfect = false;
            }
        }

        void TickDebugForce1PRound1Perfect()
        {
            if (!debugForce1PRound1Perfect)
                return;
            if (!Application.isEditor)
                return;
            if (_roundIndex != 0 || !_clockStarted[0] || _stopped[0])
                return;

            double target = GetRoundTargetSeconds();
            double now = Time.realtimeSinceStartupAsDouble;
            if (now - _slotStartRealtime[0] + 0.0000001d < target)
                return;

            _participatedMask[0] = true;
            StopSlot(0, _slotStartRealtime[0] + target);
            debugForce1PRound1Perfect = false;
        }
    }
}
