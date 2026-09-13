using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        static readonly string[] ButtonChildNames =
        {
            "A", "B", "X", "Y", "LB", "RB", "Up", "Down", "Left", "Right"
        };

        [Header("보드")]
        [Tooltip("Square_1=박0 … Square_8=박7. 행 우선 4×2.")]
        [SerializeField] RectTransform[] boardSquares;

        struct BoardCellCache
        {
            public GameObject Outline;
            public GameObject[] IconByButton;
        }

        [System.NonSerialized] BoardCellCache[] _boardCells;
        [System.NonSerialized] bool _boardReady;

        void ResolveBoard()
        {
            _boardReady = false;
            _boardCells = null;

            if (boardSquares == null || boardSquares.Length != BeatsPerSegment)
            {
                Debug.LogError(
                    "[RhythmButtonChallengeMinigameModule] boardSquares 에 Square_1~8 을 8칸 연결하세요.",
                    this);
                return;
            }

            var cells = new BoardCellCache[BeatsPerSegment];
            for (var i = 0; i < BeatsPerSegment; i++)
            {
                RectTransform square = boardSquares[i];
                if (square == null)
                {
                    Debug.LogError(
                        $"[RhythmButtonChallengeMinigameModule] boardSquares[{i}] 가 비어 있습니다.",
                        this);
                    return;
                }

                Transform iconRoot = square.Find("Icon");
                Transform outline = square.Find("Outline");
                if (iconRoot == null || outline == null)
                {
                    Debug.LogError(
                        $"[RhythmButtonChallengeMinigameModule] {square.name} 아래 Icon 또는 Outline 이 없습니다.",
                        square);
                    return;
                }

                var icons = new GameObject[ButtonChildNames.Length];
                for (var b = 0; b < ButtonChildNames.Length; b++)
                {
                    Transform child = iconRoot.Find(ButtonChildNames[b]);
                    if (child == null)
                    {
                        Debug.LogError(
                            $"[RhythmButtonChallengeMinigameModule] {square.name}/Icon/{ButtonChildNames[b]} 이 없습니다.",
                            square);
                        return;
                    }

                    icons[b] = child.gameObject;
                }

                cells[i] = new BoardCellCache
                {
                    Outline = outline.gameObject,
                    IconByButton = icons
                };
            }

            _boardCells = cells;
            _boardReady = true;
        }

        void RefreshBoard()
        {
            if (!_boardReady)
                return;

            for (var i = 0; i < BeatsPerSegment; i++)
            {
                RbcButton? shown = ButtonToShow(i);
                SetCellIcons(i, shown);

                bool outlineOn = _segmentKind == RbcSegmentKind.StageInput && i == _beatIndex;
                GameObject outline = _boardCells[i].Outline;
                if (outline.activeSelf != outlineOn)
                    outline.SetActive(outlineOn);
            }
        }

        RbcButton? ButtonToShow(int cellIndex)
        {
            switch (_segmentKind)
            {
                case RbcSegmentKind.StageReveal:
                    return cellIndex <= _beatIndex ? _currentPattern[cellIndex] : (RbcButton?)null;
                case RbcSegmentKind.StageInput:
                    return _currentPattern[cellIndex];
                default:
                    return null;
            }
        }

        void SetCellIcons(int cellIndex, RbcButton? shown)
        {
            GameObject[] icons = _boardCells[cellIndex].IconByButton;
            int shownIndex = shown.HasValue ? (int)shown.Value : -1;
            for (var b = 0; b < icons.Length; b++)
            {
                bool on = b == shownIndex;
                if (icons[b].activeSelf != on)
                    icons[b].SetActive(on);
            }
        }
    }
}
