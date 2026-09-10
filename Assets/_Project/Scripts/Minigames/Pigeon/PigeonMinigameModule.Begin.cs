using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            _ctx = context;
            gameObject.SetActive(true);
            _running = true;

            for (var i = 0; i < SlotCount; i++)
            {
                bool play = _ctx.Slots != null &&
                            i < _ctx.Slots.Length &&
                            _ctx.Slots[i].State == SlotState.PLAYING;

                _participatedMask[i] = play;

                Transform cursor = GetCursor(i);
                if (cursor == null)
                    continue;

                cursor.gameObject.SetActive(play);
            }

            _nextSortingOrder = 0;
            _floorPiles.Clear();
            for (var i = 0; i < SlotCount; i++)
            {
                _score[i] = 0;
                _peckPhase[i] = PeckPhase.Idle;
                SetPeckVisuals(i, pigeonOn: false, mouthOn: false);
            }

            _pourArmed = TryArmPour();
            if (_pourArmed)
                StartPourForward();
        }
    }
}
