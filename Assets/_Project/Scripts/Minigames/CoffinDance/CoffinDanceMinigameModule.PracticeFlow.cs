using MiniParty.Core;
using MiniParty.Flow;
using MiniParty.Input;
using MiniParty.Minigames;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void TickPracticeReadyAndMaybeStartMain()
        {
            ForEachSlot(i =>
            {
                if (!_aliveMask[i])
                    return;

                if (!SlotGamepad.StartPressed(i))
                    return;

                _practiceReady[i] = !_practiceReady[i];
            });

            if (!_operatorInput.Confirm)
                return;

            if (!AllAlivePracticeReady())
                return;

            TransitionPracticeToMainRound();
        }

        bool AllAlivePracticeReady()
        {
            bool anyAlive = false;

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                    continue;

                anyAlive = true;

                if (!_practiceReady[i])
                    return false;
            }

            return anyAlive;
        }

        void TransitionPracticeToMainRound()
        {
            PartySession session = PartySession.Instance;
            if (session == null)
            {
                Debug.LogError("[CoffinDanceMinigameModule] PartySession 이 없습니다. 메인에 PartySession 을 두세요.", this);
                return;
            }

            var played = new bool[SlotCount];
            ForEachSlot(i => played[i] = _participatedMask[i]);

            session.PrepareRound(false, played);
            Begin(new MinigameContext(session.Slots, false, _ctx.OnComplete));
        }

        static bool EscapePressed()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;

            return UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
