using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            _ctx = context;
            gameObject.SetActive(true);
            _running = true;
            _completing = false;
            _phase = Phase.Idle;
            _roundIndex = 0;

            ValidateSceneReferences();
            StopPerfectHitVideos();
            if (exitScreenFader != null) exitScreenFader.SetInstant(0f);

            var anyPlaying = false;
            for (var i = 0; i < SlotCount; i++)
            {
                bool play = _ctx.Slots != null &&
                            i < _ctx.Slots.Length &&
                            _ctx.Slots[i].State == SlotState.PLAYING;

                _participatedMask[i] = play;
                anyPlaying |= play;
                _stopped[i] = false;
                _records[i] = 0d;
                _errors[i] = 0d;
                _roundScores[i] = 0;
                _totalScores[i] = 0;
            }

            if (!anyPlaying)
            {
                Debug.LogWarning("[TimeChallenge] 참가자가 없어 게임을 시작하지 않습니다.", this);
                _running = false;
                return;
            }

            ApplyOccupiedSlots();
            SnapScoreBoards();
            RefreshHud();
            StartRound(0);
        }
    }
}
