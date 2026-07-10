using MiniParty.Core;
using MiniParty.Minigames;
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
            ForEachSlot(ApplySlotChromeAndSequenceLayout);
            ResetAllGuideFeedback();

            if (!_ctx.IsPractice)
            {
                ResetAllCatAnimatorsToIdle();
                ResetAllCatMovement();
                ResetAllSlotUiShake();
            }

            StopTierBgm();

            UpdateMainTimerUi();
            ValidatePatternSfxSetup();
            ForEachSlot(FlushUi);
        }

        void ResetSlotAtBegin(int i)
        {
            bool play = _ctx.Slots[i].State == SlotState.PLAYING;

            _aliveMask[i] = play;
            ref SlotRuntime sr = ref _slots[i];
            sr.Gauge01 = play ? 1f : 0f;
            sr.Cursor = 0;
            sr.ConsecutiveLoopSuccesses = 0;
            sr.ScoreSum = 0;
            sr.InputLockTimer = 0f;
            sr.FailFlashTimer = 0f;
            sr.InTypoState = false;
            sr.ShuffleEffectTimer = 0f;
            sr.TierBumpBlurRemaining = 0f;
            sr.BurstPool = new BurstTextFx[BurstTextPoolSize];
            AssignDefaultButtonMapping(ref sr);
            _practiceReady[i] = false;

            if (TryGetBinding(i, out SlotUiBindings b))
            {
                if (b.SequenceText != null)
                    _sequenceTextBaseFontSize[i] = b.SequenceText.fontSize;
                else
                    _sequenceTextBaseFontSize[i] = -1f;

                if (b.SlotPanelBackgroundImage != null)
                    _slotPanelBgRestColor[i] = b.SlotPanelBackgroundImage.color;
                else
                    _slotPanelBgRestColor[i] = new Color(1f, 1f, 1f, 1f);

                if (_blurRestSiblingIndex[i] < 0 && b.Blur != null)
                    _blurRestSiblingIndex[i] = b.Blur.transform.GetSiblingIndex();

                if (b.WaitingText != null)
                    b.WaitingText.gameObject.SetActive(IsSlotEmptyForUi(i));

                InitializeShuffleEffectVisual(i, b);
                InitializeBurstTextPool(i);
            }
            else
            {
                _sequenceTextBaseFontSize[i] = -1f;
                _slotPanelBgRestColor[i] = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}
