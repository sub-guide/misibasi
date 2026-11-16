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

            /// <summary>길이 <see cref="DjPadButtonCount"/>. true = Highlight.</summary>
            public bool[] DjActive;

            /// <summary>SubPatternGuide 진행도(0~12). 12글자 완성 시 0.</summary>
            public int SubPatternMatched;

            /// <summary>연타 중 개인 티어. 1~3. 빌드업 시작 값은 1.</summary>
            public int Tier;

            /// <summary>개인 티어 게이지. 0~그 티어의 <c>GaugeMax</c>.</summary>
            public float Gauge;
        }

        [System.Serializable]
        public sealed class SlotUiBindings
        {
            [Header("공통")]
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

            [Header("점수")]
            public TMP_Text HudScoreText;

            [Header("콤보 (HudDisplay 밖 · 독립)")]
            public TMP_Text HudComboText;

            [Header("티어 게이지 (DjBox 직속 Gauge_L/R_1~3 · Filled Image)")]
            public Image[] TierGaugeLeft;
            public Image[] TierGaugeRight;

            [Header("서브 패턴 가이드")]
            public TMP_Text SubPatternGuideText;

            [Header("스피커 (DjBox 위 L/R · Body+WooferTop+WooferBottom)")]
            public RectTransform SpeakerLRoot;
            public RectTransform SpeakerLBody;
            public RectTransform SpeakerLWooferTop;
            public RectTransform SpeakerLWooferBottom;
            public RectTransform SpeakerRRoot;
            public RectTransform SpeakerRBody;
            public RectTransform SpeakerRWooferTop;
            public RectTransform SpeakerRWooferBottom;

            [Header("전광판 스크린")]
            public RectTransform StageScreenRoot;
            public Graphic StageBackgroundSpace;
            public Graphic StageBackgroundClub;

            [Header("관중 이펙트 (피버)")]
            [Tooltip("DjBox 뒤·위. Image+RectTransform. 이름 Crowd / CrowdPeople.")]
            public RectTransform CrowdRoot;
            public Image CrowdImage;

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
