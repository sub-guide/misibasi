using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        public void Tick()
        {
            if (!_running || _completing)
                return;

            float dt = Time.deltaTime;
            float speed = Mathf.Max(0f, cursorSpeed);

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_participatedMask[i])
                    continue;

                TickCursorMove(i, speed, dt);
                TickPeck(i, dt);
            }

            TickPour(dt);
            TickRoundTimer(dt);
        }

        public void RequestEarlyExit()
        {
            CompleteSession();
        }
    }
}
