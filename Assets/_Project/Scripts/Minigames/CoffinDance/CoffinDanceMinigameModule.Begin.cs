using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            _ctx = context;
            gameObject.SetActive(true);
            _running = true;
            _completing = false;
            _flowState = CdFlowState.Playing;
            _endDelayRemain = 0f;

            _elapsedMainTime = 0f;
            _remainingMainTime = MainDurationSeconds;
            _rng = new System.Random(Random.Range(int.MinValue, int.MaxValue));

            _jumpPromptState = CdJumpPromptState.Idle;
            _jumpPromptRemain = 0f;
            _jumpRequiredPresses = 1;
            _jumpIsDouble = false;

            _slots = new SlotRuntime[SlotCount];

            ForEachSlot(i =>
            {
                bool play = _ctx.Slots != null &&
                            i < _ctx.Slots.Length &&
                            _ctx.Slots[i].State == SlotState.PLAYING;

                _participatedMask[i] = play;
                _aliveMask[i] = play;
                _practiceReady[i] = false;

                ref SlotRuntime sr = ref _slots[i];
                sr = default;

                if (!play)
                    return;

                float sign = (_rng.Next(0, 2) == 0) ? -1f : 1f;
                sr.ThetaRad = sign * initialTiltDegrees * Mathf.Deg2Rad;
                sr.Omega = -sign * initialAngularSpeed;
                sr.ScoreExact = 0f;
                sr.ScoreSum = 0;
                sr.Eliminated = false;
                sr.StumbleTimer = 0f;
                sr.InStumble = false;
                sr.JumpLockoutRemain = 0f;
                sr.JumpSucceededThisPrompt = false;
                sr.JumpPressesThisPrompt = 0;
                sr.JumpVisualT = 0f;

                CoffinDanceSlotBindings bind = GetBindings(i);
                if (bind != null && bind.TiltRoot != null)
                {
                    sr.CoffinBaseLocalPos = bind.TiltRoot.localPosition;
                    sr.HasCoffinBase = true;
                    ApplyPresentationYaw(bind.TiltRoot);
                }

                ApplyCameraViewport(i, bind);
                HideJumpPrompt(i);
                SetEliminatedUi(i, false);
            });

            RefreshPhaseParameters();
            ScheduleNextJump(immediate: true);
            FlushAllUi();
        }

        void ApplyPresentationYaw(Transform tiltRoot)
        {
            if (tiltRoot == null)
                return;

            Vector3 e = tiltRoot.localEulerAngles;
            e.y = presentationYawDegrees;
            tiltRoot.localEulerAngles = e;
        }

        void ApplyCameraViewport(int slotIndex, CoffinDanceSlotBindings bind)
        {
            if (bind == null || bind.SlotCamera == null)
                return;

            float w = 1f / SlotCount;
            bind.SlotCamera.rect = new Rect(slotIndex * w, 0f, w, 1f);
        }
    }
}
