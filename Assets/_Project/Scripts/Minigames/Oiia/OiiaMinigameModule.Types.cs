using MiniParty.UI.ControllerButtons;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        public const int BurstTextPoolSize = 6;

        public const int GuideButtonsPerSlot = 4;

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

        struct SequenceTextMainLayoutSnapshot
        {
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
        }

        struct BurstTextFx
        {
            public float Remaining;
            public float Duration;
            public float StartAlpha;
            public float BaseRotationZ;
            public float SwingPhase;
            public Vector2 AnchorBase;
            public float PhaseX;
            public float PhaseY;
            public float ShakeAmplitude;
            public float ShakeFrequency;
        }

        struct NeonShockwaveFx
        {
            public float Remaining;
            public float Duration;
            public float TargetScale;
            public Image NeonImage;
        }

        /// <summary>부스 패드 물리 버튼 4종 (사용자 표기 X/Y/A/B). 레거시 O/I/A 매핑용.</summary>
        public enum OiiaPhysicalButton
        {
            X,
            Y,
            A,
            B
        }

        struct SlotRuntime
        {
            public float Gauge01;
            public float InputLockTimer;
            public int Cursor;
            public int ConsecutiveLoopSuccesses;
            public int ScoreSum;
            public float FailFlashTimer;
            public bool InTypoState;
            public float ShuffleEffectTimer;
            public float TierBumpBlurRemaining;

            public OiiaPhysicalButton MapO;
            public OiiaPhysicalButton MapI;
            public OiiaPhysicalButton MapA;

            public BurstTextFx[] BurstPool;
        }

        [System.Serializable]
        public sealed class SlotUiBindings
        {
            // ── 레거시 (문자 패턴·게이지·4버튼 가이드) — 개편 완료 전까지 유지 ──

            [Header("레거시 — 문자 패턴·게이지 (개편 후 비활성 예정)")]
            [Tooltip("레거시: oiia 패턴 글자. 개편 후 SubPatternGuideText로 역할 이전.")]
            public TMP_Text SequenceText;

            [Tooltip("레거시: 세로 게이지. 디제잉 레이브에서는 미사용(삭제·숨김 예정).")]
            public Slider GaugeSlider;

            [FormerlySerializedAs("FailOverlay")]
            [Tooltip("실패 시 빨간 플래시, 난이도 상승 직전 0.5초는 흰색 깜빡임.")]
            public Image Blur;

            [Tooltip("레거시 점수 TMP. 개편 후 HudScoreText와 동일 오브젝트를 가리켜도 됨.")]
            public TMP_Text ScoreText;

            [Tooltip("연습: START로 준비 시 \'READY\' 표시. 본게임에선 자동 비표시.")]
            public TMP_Text PracticeReadyText;

            [Tooltip("고양이 UI Image와 같은 오브젝트의 Animator. StageScreen 안 Cat과 동일 참조 권장.")]
            public Animator CatAnimator;

            [Tooltip("슬롯 흰색 패널 배경 Image만. 본게임 유지 5초 이상이면 이 Image 알파만 0, 실패 시 Begin 때 색으로 복구.")]
            public Image SlotPanelBackgroundImage;

            [Tooltip("비참가(EMPTY) 슬롯 중앙 WAITING 문구. ACTIVE/READY/PLAYING 에선 비표시.")]
            public TMP_Text WaitingText;

            [Header("레거시 — 컨트롤러 가이드 (다이아몬드 Y/X/A/B, 교체 예정)")]
            [Tooltip("레거시: Y/X/A/B 가이드 루트. 디제잉 박스(SNES 10키)로 교체 예정.")]
            public GameObject ControllerGuideRoot;

            [Tooltip("(선택) 반원 바디 Image. 코드 로직 미사용.")]
            public Image ControllerBodyImage;

            [Tooltip("좌측 — Y (Button 4).")]
            public Image GuideButtonY;

            [Tooltip("상단 — X (Trigger).")]
            public Image GuideButtonX;

            [Tooltip("우측 — A (Button 2).")]
            public Image GuideButtonA;

            [Tooltip("하단 — B (Button 3).")]
            public Image GuideButtonB;

            [Header("레거시 — 셔플 이펙트 (개편 후 재검토)")]
            [Tooltip("12글자 루프 완주 시 슬롯 중앙 스프라이트 확대·페이드.")]
            public Image ShuffleEffect;

            [Header("코믹스 BurstText 풀")]
            [Tooltip("정답 시 고양이 위치에 팝업. 5~6개 미리 배치.")]
            public TMP_Text[] BurstTextPool;

            [Tooltip("BurstText 좌표 부모. 비우면 슬롯 패널 루트.")]
            public RectTransform BurstTextContainer;

            // ── 디제잉 레이브 (1단계 바인딩) ──

            [Header("디제잉 박스 (Rave)")]
            [Tooltip("디제잉 박스 프레임 루트. 자식에 Buttons_ABXY / Direction / LR + Hud + SubPattern.")]
            public RectTransform DjBoxRoot;

            [Tooltip("Buttons_ABXY 인스턴스 루트 드라이버. Face A/B/X/Y.")]
            public SnesPlayerFaceButtons DjFaceButtons;

            [Tooltip("Buttons_Direction 인스턴스 루트 드라이버. Up/Down/Left/Right.")]
            public SnesPlayerDpadButtons DjDpadButtons;

            [Tooltip("Buttons_LR 인스턴스 루트 드라이버. L/R.")]
            public SnesPlayerShoulderButtons DjShoulderButtons;

            [Tooltip("길이 10. 인덱스 = OiiaDjPadButtonId (A…Right). 비우면 드라이버에서 자동 채움.")]
            public SnesControllerButtonVisual[] DjPadButtons;

            [Header("소형 가변 디스플레이 (Score / Combo / Fever)")]
            [Tooltip("디제잉 박스 상단 Score TMP.")]
            public TMP_Text HudScoreText;

            [Tooltip("디제잉 박스 상단 Combo TMP.")]
            public TMP_Text HudComboText;

            [Tooltip("디제잉 박스 상단 Fever TMP (피버 진입 시 표시).")]
            public TMP_Text HudFeverText;

            [Header("서브 패턴 가이드 (가사 흐름)")]
            [Tooltip("디제잉 박스 하단 — 원래 가사/패턴 흐름 표시용 TMP.")]
            public TMP_Text SubPatternGuideText;

            [Header("전광판 스크린 (티어 배경·고양이)")]
            [Tooltip("전광판 레이어 루트. 배경 레이어 + Cat 자식.")]
            public RectTransform StageScreenRoot;

            [Tooltip("1티어: 크로마키 배경 (Image 또는 RawImage+Video). Graphic 공통.")]
            public Graphic StageBackgroundChromaKey;

            [Tooltip("2티어: 우주 배경.")]
            public Graphic StageBackgroundSpace;

            [Tooltip("3티어: 클럽 배경.")]
            public Graphic StageBackgroundClub;
        }
    }
}
