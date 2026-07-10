using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        public enum RbcButton
        {
            A,
            B,
            X,
            Y,
            Lb,
            Rb,
            Up,
            Down,
            Left,
            Right
        }

        public enum RbcJudgment
        {
            None,
            Perfect,
            Fast,
            Slow,
            Miss,
            Wrong
        }

        enum RbcSegmentKind
        {
            PhaseIntro,
            StageReveal,
            StageInput
        }

        enum RbcFlowState
        {
            PhaseIntro,
            StageReveal,
            StageInput,
            SpeedUp,
            Complete
        }

        [System.Serializable]
        public sealed class BoardCellBindings
        {
            public Image ButtonIcon;
            public Image ActiveHighlight;
            public Image Judgment1P;
            public Image Judgment2P;
            public Image Judgment3P;
            public Image Judgment4P;
        }

        [System.Serializable]
        public sealed class ScorePanelBindings
        {
            public TMP_Text ScoreText;
            public TMP_Text PlayerLabel;
        }

        struct SlotRuntime
        {
            public int ScoreSum;
            public RbcJudgment[] BeatJudgments;
            public bool[] BeatJudged;
            public int ExtraInputCountOnBeat;
        }

        struct BeatWindow
        {
            public int BeatIndex;
            public double StartTime;
            public double EndTime;
            public bool Active;
        }
    }
}
