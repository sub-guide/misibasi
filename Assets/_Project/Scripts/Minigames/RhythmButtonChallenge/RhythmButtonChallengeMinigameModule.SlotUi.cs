using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        static readonly Color OutlineSuccess = Color.green;
        static readonly Color OutlineFail = Color.red;

        struct SlotUiCache
        {
            public GameObject Root;
            public Outline Outline;
            public TMP_Text ScoreText;
            public Animator PopupSuccess;
            public Animator PopupFail;
            public Animator PopupBonus;
            public Color IdleOutlineColor;
            public bool OutlineFading;
            public double OutlineFadeStart;
            public Color OutlineFadeFrom;
            public double PopupHideTime;
        }

        [System.NonSerialized] SlotUiCache[] _slotUi;
        [System.NonSerialized] bool _slotUiReady;

        void ResolveSlotUi()
        {
            _slotUiReady = false;
            _slotUi = null;

            if (playerSlots == null || playerSlots.Length != SlotCount)
            {
                Debug.LogError(
                    "[RhythmButtonChallengeMinigameModule] playerSlots 에 1P~4P 를 4칸 연결하세요.",
                    this);
                return;
            }

            if (scorePanels == null || scorePanels.Length != SlotCount)
            {
                Debug.LogError(
                    "[RhythmButtonChallengeMinigameModule] scorePanels 에 P1~P4 ScorePanelBindings 를 4칸 연결하세요.",
                    this);
                return;
            }

            var ui = new SlotUiCache[SlotCount];
            for (var i = 0; i < SlotCount; i++)
            {
                RectTransform slot = playerSlots[i];
                RhythmButtonChallengeScorePanelBindings panel = scorePanels[i];
                if (slot == null || panel == null)
                {
                    Debug.LogError(
                        $"[RhythmButtonChallengeMinigameModule] playerSlots[{i}] 또는 scorePanels[{i}] 가 비어 있습니다.",
                        this);
                    return;
                }

                var outline = slot.GetComponent<Outline>();
                if (outline == null || panel.ScoreText == null)
                {
                    Debug.LogError(
                        $"[RhythmButtonChallengeMinigameModule] {slot.name} Outline 또는 {panel.name} ScoreText 가 없습니다.",
                        panel);
                    return;
                }

                ui[i] = new SlotUiCache
                {
                    Root = slot.gameObject,
                    Outline = outline,
                    ScoreText = panel.ScoreText,
                    PopupSuccess = panel.PopupSuccess,
                    PopupFail = panel.PopupFail,
                    PopupBonus = panel.PopupBonus,
                    IdleOutlineColor = outline.effectColor
                };
            }

            _slotUi = ui;
            _slotUiReady = true;
        }

        void ApplySlotParticipation()
        {
            if (!_slotUiReady)
                return;

            ForEachSlot(i => _slotUi[i].Root.SetActive(_aliveMask[i]));
        }

        void RefreshScoreLabel(int slotIndex)
        {
            if (!_slotUiReady || !_aliveMask[slotIndex])
                return;

            _slotUi[slotIndex].ScoreText.text = $"{_slots[slotIndex].ScoreSum}점";
        }

        void RefreshAllScoreLabels()
        {
            ForEachSlot(RefreshScoreLabel);
        }

        void SetSlotOutlineJudged(int slotIndex, bool success)
        {
            if (!_slotUiReady || !_aliveMask[slotIndex])
                return;

            Color judged = success ? OutlineSuccess : OutlineFail;
            ref SlotUiCache ui = ref _slotUi[slotIndex];
            ui.Outline.effectColor = judged;
            ui.OutlineFadeFrom = judged;
            ui.OutlineFadeStart = Time.unscaledTimeAsDouble;
            ui.OutlineFading = true;
        }

        void TickOutlineFades()
        {
            if (!_slotUiReady)
                return;

            double now = Time.unscaledTimeAsDouble;
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                    continue;

                ref SlotUiCache ui = ref _slotUi[i];
                if (!ui.OutlineFading)
                    continue;

                float t = (float)((now - ui.OutlineFadeStart) / SlotOutlineFadeToIdleSeconds);
                if (t >= 1f)
                {
                    ui.Outline.effectColor = ui.IdleOutlineColor;
                    ui.OutlineFading = false;
                }
                else
                {
                    ui.Outline.effectColor = Color.Lerp(ui.OutlineFadeFrom, ui.IdleOutlineColor, t);
                }
            }
        }
    }
}
