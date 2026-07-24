using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        const string DefaultBoardContainerName = "Board_8Cells";
        const string DefaultScorePanelsContainerName = "Panel_RBC_Score_4Way";

        [Header("공용 보드 (8칸)")]
        [SerializeField] BoardCellBindings[] boardCells = new BoardCellBindings[BeatsPerSegment];

        [Header("하단 점수 (4슬롯)")]
        [SerializeField] ScorePanelBindings[] scorePanels = new ScorePanelBindings[SlotCount];

        [Header("버튼 아이콘 Sprite")]
        [SerializeField] Sprite spriteA;
        [SerializeField] Sprite spriteB;
        [SerializeField] Sprite spriteX;
        [SerializeField] Sprite spriteY;
        [SerializeField] Sprite spriteLb;
        [SerializeField] Sprite spriteRb;
        [SerializeField] Sprite spriteUp;
        [SerializeField] Sprite spriteDown;
        [SerializeField] Sprite spriteLeft;
        [SerializeField] Sprite spriteRight;

        [Header("판정 Sprite")]
        [SerializeField] Sprite spritePerfect;
        [SerializeField] Sprite spriteFast;
        [SerializeField] Sprite spriteSlow;
        [SerializeField] Sprite spriteMiss;
        [SerializeField] Sprite spriteWrong;

        [Header("SPEED UP 오버레이")]
        [SerializeField] TMP_Text speedUpText;

        void ResolveBoardAndScoreBindings()
        {
            if (boardCells == null || boardCells.Length == 0 || boardCells[0]?.ButtonIcon == null)
            {
                var boardRoot = GameObject.Find(DefaultBoardContainerName);
                if (boardRoot != null)
                {
                    var cellBindings = boardRoot.GetComponentsInChildren<RhythmButtonChallengeBoardCellBindings>(true);
                    if (cellBindings != null && cellBindings.Length >= BeatsPerSegment)
                    {
                        boardCells = new BoardCellBindings[BeatsPerSegment];
                        for (var i = 0; i < BeatsPerSegment; i++)
                            boardCells[i] = cellBindings[i].ToBoardCellBindings();
                    }
                }
            }

            if (scorePanels == null || scorePanels.Length == 0 || scorePanels[0]?.ScoreText == null)
            {
                var scoreRoot = GameObject.Find(DefaultScorePanelsContainerName);
                if (scoreRoot != null)
                {
                    var panelBindings = scoreRoot.GetComponentsInChildren<RhythmButtonChallengeScorePanelBindings>(true);
                    if (panelBindings != null && panelBindings.Length >= SlotCount)
                    {
                        scorePanels = new ScorePanelBindings[SlotCount];
                        for (var i = 0; i < SlotCount; i++)
                            scorePanels[i] = panelBindings[i].ToScorePanelBindings();
                    }
                }
            }
        }

        void FlushAllUi()
        {
            ForEachSlot(UpdateScorePanelUi);
            UpdateBoardHighlights();
        }

        void UpdateScorePanelUi(int slotIndex)
        {
            if (scorePanels == null || slotIndex >= scorePanels.Length)
                return;

            ScorePanelBindings panel = scorePanels[slotIndex];
            if (panel?.ScoreText == null)
                return;

            if (!_aliveMask[slotIndex])
            {
                panel.ScoreText.text = "-";
                return;
            }

            panel.ScoreText.text = _slots[slotIndex].ScoreSum.ToString("N0");
        }

        void UpdateBoardForBeat(int beatIndex)
        {
            if (boardCells == null)
                return;

            for (var c = 0; c < boardCells.Length && c < BeatsPerSegment; c++)
            {
                BoardCellBindings cell = boardCells[c];
                if (cell?.ButtonIcon == null)
                    continue;

                if (_segmentKind == RbcSegmentKind.StageReveal && c <= beatIndex)
                {
                    cell.ButtonIcon.sprite = SpriteForButton(_currentPattern[c]);
                    cell.ButtonIcon.enabled = cell.ButtonIcon.sprite != null;
                }
                else if (_segmentKind == RbcSegmentKind.StageInput)
                {
                    cell.ButtonIcon.sprite = SpriteForButton(_currentPattern[c]);
                    cell.ButtonIcon.enabled = cell.ButtonIcon.sprite != null;
                }
            }

            UpdateBoardHighlights();
        }

        void UpdateBoardHighlights()
        {
            if (boardCells == null)
                return;

            for (var c = 0; c < boardCells.Length && c < BeatsPerSegment; c++)
            {
                BoardCellBindings cell = boardCells[c];
                if (cell?.ActiveHighlight == null)
                    continue;

                // PhaseIntro / Reveal / Input 모두 현재 박 하이라이트 (05 문서 구간별 보드 동작과 일치)
                bool active = _segmentAudioStarted &&
                              (_segmentKind == RbcSegmentKind.PhaseIntro ||
                               _segmentKind == RbcSegmentKind.StageReveal ||
                               _segmentKind == RbcSegmentKind.StageInput) &&
                              c == _beatIndex;

                cell.ActiveHighlight.enabled = active;
            }
        }

        void ClearBoardIcons()
        {
            if (boardCells == null)
                return;

            foreach (BoardCellBindings cell in boardCells)
            {
                if (cell?.ButtonIcon == null)
                    continue;

                cell.ButtonIcon.sprite = null;
                cell.ButtonIcon.enabled = false;
            }
        }

        void ClearAllJudgmentImages()
        {
            if (boardCells == null)
                return;

            foreach (BoardCellBindings cell in boardCells)
            {
                SetImageHidden(cell?.Judgment1P);
                SetImageHidden(cell?.Judgment2P);
                SetImageHidden(cell?.Judgment3P);
                SetImageHidden(cell?.Judgment4P);
            }
        }

        void SetJudgmentImage(int slotIndex, int beatIndex, RbcJudgment judgment)
        {
            if (boardCells == null || beatIndex < 0 || beatIndex >= boardCells.Length)
                return;

            BoardCellBindings cell = boardCells[beatIndex];
            Image target = slotIndex switch
            {
                0 => cell?.Judgment1P,
                1 => cell?.Judgment2P,
                2 => cell?.Judgment3P,
                3 => cell?.Judgment4P,
                _ => null
            };

            if (target == null)
                return;

            Sprite sprite = SpriteForJudgment(judgment);
            target.sprite = sprite;
            target.enabled = sprite != null;
        }

        static void SetImageHidden(Image img)
        {
            if (img == null)
                return;

            img.sprite = null;
            img.enabled = false;
        }

        Sprite SpriteForButton(RbcButton button) =>
            button switch
            {
                RbcButton.A => spriteA,
                RbcButton.B => spriteB,
                RbcButton.X => spriteX,
                RbcButton.Y => spriteY,
                RbcButton.Lb => spriteLb,
                RbcButton.Rb => spriteRb,
                RbcButton.Up => spriteUp,
                RbcButton.Down => spriteDown,
                RbcButton.Left => spriteLeft,
                RbcButton.Right => spriteRight,
                _ => null
            };

        Sprite SpriteForJudgment(RbcJudgment judgment) =>
            judgment switch
            {
                RbcJudgment.Perfect => spritePerfect,
                RbcJudgment.Fast => spriteFast,
                RbcJudgment.Slow => spriteSlow,
                RbcJudgment.Miss => spriteMiss,
                RbcJudgment.Wrong => spriteWrong,
                _ => null
            };

        void ShowSpeedUpOverlay()
        {
            if (speedUpText == null)
                return;

            speedUpText.gameObject.SetActive(true);
            speedUpText.text = "SPEED UP!";
        }

        void HideSpeedUpOverlay()
        {
            if (speedUpText == null)
                return;

            speedUpText.gameObject.SetActive(false);
        }
    }
}
