using MiniParty.Core;
using MiniParty.Minigames;
using MiniParty.UI.ControllerButtons;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            ResolveSlotBindingsFromPanels();

            _ctx = context;
            gameObject.SetActive(true);
            _running = true;

            _slots = new SlotRuntime[SlotCount];

            _remainingMainTime = Mathf.Max(MainRoundMinSeconds, MainRoundDurationSeconds);

            if (!_ctx.IsPractice && mainRoundTimerCentralTop == null && !_warnedTimerMissing)
            {
                _warnedTimerMissing = true;
                Debug.LogWarning("[OiiaMinigameModule] 본게임 타이머용 TMP(mainRoundTimerCentralTop)가 비었습니다. Inspector에서 연결하세요.", this);
            }

            ForEachSlot(ResetSlotAtBegin);
            ForEachSlot(ApplySlotChrome);

            if (!_ctx.IsPractice)
            {
                ResetAllCatAnimatorsToIdle();
                ResetAllCatMovement();
                ResetAllSlotUiShake();
            }

            StopTierBgm();

            UpdateMainTimerUi();
            ForEachSlot(FlushUi);
        }

        void ResetSlotAtBegin(int i)
        {
            bool play = _ctx.Slots[i].State == SlotState.PLAYING;

            _aliveMask[i] = play;
            ref SlotRuntime sr = ref _slots[i];
            sr.ScoreSum = 0;
            sr.Combo = 0;
            sr.InputLockTimer = 0f;
            sr.FeverRemaining = 0f;
            sr.FeverCharge = 0;
            sr.ConsecutiveLoopSuccesses = 0;
            _practiceReady[i] = false;

            if (TryGetBinding(i, out SlotUiBindings b))
            {
                if (b.SlotPanelBackgroundImage != null)
                    _slotPanelBgRestColor[i] = b.SlotPanelBackgroundImage.color;
                else
                    _slotPanelBgRestColor[i] = new Color(1f, 1f, 1f, 1f);

                if (b.WaitingText != null)
                    b.WaitingText.gameObject.SetActive(IsSlotEmptyForUi(i));

                SyncDjPadPlayerIndices(i, b);
                ClearDjPadHighlights(b);
                ApplyOiiaLrDjBoxVisualOverrides(b);

                if (play)
                {
                    if (IsDevGodModeSlot(i))
                        ActivateAllDjTargets(i);
                    else
                        SeedDjActiveTargets(i);
                }
            }
            else
            {
                _slotPanelBgRestColor[i] = new Color(1f, 1f, 1f, 1f);
            }
        }

        static void ClearDjPadHighlights(SlotUiBindings b)
        {
            if (b.DjPadButtons == null)
                return;

            for (var k = 0; k < b.DjPadButtons.Length; k++)
            {
                SnesControllerButtonVisual visual = b.DjPadButtons[k];
                if (visual != null)
                    visual.SetHighlighted(false);
            }
        }
    }
}
