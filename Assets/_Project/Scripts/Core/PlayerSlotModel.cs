using System;

namespace MiniParty.Core
{
    /// <summary>
    /// 논리 슬롯. 항상 4개 존재하며, 컨트롤러 번호와 1:1 고정된다.
    /// </summary>
    public sealed class PlayerSlotModel
    {
        public int Index { get; }
        public int ControllerIndex => Index;

        public SlotState State { get; private set; } = SlotState.EMPTY;

        public int HP { get; private set; }
        public int WinStreak { get; private set; }

        public PlayerSlotModel(int index, int startingHp)
        {
            Index = index;
            HP = startingHp;
            WinStreak = 0;
        }

        public event Action<PlayerSlotModel, SlotState, SlotState> StateChanged;

        public bool IsOccupied =>
            State is SlotState.ACTIVE or SlotState.READY or SlotState.PLAYING or SlotState.RESULT or SlotState.GAMEOVER;

        /// <summary>연습 라운드 종료 후 로비 복귀. HP가 있으면 ACTIVE.</summary>
        public void ResetToLobbyAfterMinigame()
        {
            if (HP <= 0)
            {
                TransitionToEmpty();
                return;
            }

            SetActiveIfParticipating();
        }

        /// <summary>본게임 Result 이후 메인 복귀. HP≤0만 EMPTY, 생존자는 참가 유지(ACTIVE).</summary>
        public void ReturnToMainMenuAfterRound()
        {
            if (HP <= 0)
            {
                TransitionToEmpty();
                return;
            }

            SetActiveIfParticipating();
        }

        /// <summary>탈락·퇴장 슬롯. HP·연승은 유지하며 JOIN 시 초기화한다.</summary>
        public void TransitionToEmpty()
        {
            if (State == SlotState.EMPTY)
                return;

            SlotState prev = State;
            State = SlotState.EMPTY;
            StateChanged?.Invoke(this, prev, State);
        }

        /// <summary>EMPTY에서 START(조인). HP·연승을 기본값으로 맞춘 뒤 ACTIVE.</summary>
        public bool TryJoinFromEmpty(int startingHp)
        {
            if (State != SlotState.EMPTY)
                return false;

            HP = startingHp;
            WinStreak = 0;

            SlotState prev = State;
            State = SlotState.ACTIVE;
            StateChanged?.Invoke(this, prev, State);
            return true;
        }

        void SetActiveIfParticipating()
        {
            if (State is SlotState.ACTIVE or SlotState.READY)
                return;

            if (State is not (
                SlotState.PLAYING or SlotState.RESULT or SlotState.GAMEOVER))
                return;

            SlotState prev = State;
            State = SlotState.ACTIVE;
            StateChanged?.Invoke(this, prev, State);
        }

        public void ToggleReady()
        {
            if (State != SlotState.ACTIVE && State != SlotState.READY)
                return;

            SlotState prev = State;

            State = prev == SlotState.ACTIVE ? SlotState.READY : SlotState.ACTIVE;

            StateChanged?.Invoke(this, prev, State);
        }

        public void EnterPlaying()
        {
            SetState(SlotState.PLAYING);
        }

        public void EnterResult()
        {
            SetState(SlotState.RESULT);
        }

        public void EnterGameOver()
        {
            SetState(SlotState.GAMEOVER);
        }

        internal void SetState(SlotState next)
        {
            SlotState prev = State;
            State = next;
            StateChanged?.Invoke(this, prev, State);
        }

        public void ApplyHpDelta(int delta, int minHp = 0)
        {
            HP = Math.Max(minHp, HP + delta);
        }

        public void ResetWinStreak()
        {
            WinStreak = 0;
        }

        public void IncrementWinStreak()
        {
            WinStreak++;
        }
    }
}
