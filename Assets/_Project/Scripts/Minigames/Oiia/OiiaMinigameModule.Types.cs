using MiniParty.UI.ControllerButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>
        /// 디제잉 레이브 10키. 인덱스 = <see cref="DjPadButtonCount"/> 배열 슬롯.
        /// 순서: A, B, X, Y, L, R, Up, Down, Left, Right.
        /// </summary>
        public enum OiiaDjPadButtonId
        {
            A = 0,
            B = 1,
            X = 2,
            Y = 3,
            L = 4,
            R = 5,
            Up = 6,
            Down = 7,
            Left = 8,
            Right = 9
        }

        /// <summary>디제잉 레이브 슬롯 런타임.</summary>
        struct SlotRuntime
        {
            public int ScoreSum;
            public int Combo;
            public float InputLockTimer;

            /// <summary>피버 남은 시간(초). &gt;0 이면 전 버튼 정답.</summary>
            public float FeverRemaining;

            /// <summary>피버 게이지 충전(0~FeverComboThreshold). 피버 중에는 증가하지 않음.</summary>
            public int FeverCharge;

            /// <summary>레거시 루프 티어 호환(Cat/BGM). 글로벌 티어 도입 전 0 유지.</summary>
            public int ConsecutiveLoopSuccesses;

            /// <summary>길이 <see cref="DjPadButtonCount"/>. true = 활성 타겟(Highlight).</summary>
            public bool[] DjActive;
        }

        [System.Serializable]
        public sealed class SlotUiBindings
        {
            [Header("공통")]
            [Tooltip("연습 READY 문구.")]
            public TMP_Text PracticeReadyText;

            [Tooltip("고양이 Animator. StageScreen/Cat 권장.")]
            public Animator CatAnimator;

            [Tooltip("슬롯 패널 배경 Image.")]
            public Image SlotPanelBackgroundImage;

            [Tooltip("비참가 WAITING.")]
            public TMP_Text WaitingText;

            [Header("디제잉 박스 (Rave)")]
            public RectTransform DjBoxRoot;
            public SnesPlayerFaceButtons DjFaceButtons;
            public SnesPlayerDpadButtons DjDpadButtons;
            public SnesPlayerShoulderButtons DjShoulderButtons;

            [Tooltip("길이 10. 인덱스 = OiiaDjPadButtonId.")]
            public SnesControllerButtonVisual[] DjPadButtons;

            [Header("소형 가변 디스플레이 (Score ↔ FEVER 상호 배타)")]
            public TMP_Text HudScoreText;
            public TMP_Text HudFeverText;

            [Header("콤보 (HudDisplay 밖 · 독립)")]
            public TMP_Text HudComboText;

            [Header("피버 게이지 (DjBox 독립 · Filled Image ×2)")]
            public Image FeverGaugeImage;
            public Image FeverGaugeImageB;

            [Header("서브 패턴 가이드")]
            public TMP_Text SubPatternGuideText;

            [Header("전광판 스크린")]
            public RectTransform StageScreenRoot;
            public Graphic StageBackgroundChromaKey;
            public Graphic StageBackgroundSpace;
            public Graphic StageBackgroundClub;

            [Header("스포트라이트 (L/R · Fixture+Beam)")]
            public RectTransform SpotlightLRoot;
            public Image SpotlightLFixture;
            public Image SpotlightLBeam;
            public RectTransform SpotlightRRoot;
            public Image SpotlightRFixture;
            public Image SpotlightRBeam;
        }
    }
}
