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

        /// <summary>부스 패드 물리 버튼 4종 (사용자 표기 X/Y/A/B).</summary>
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
            public TMP_Text SequenceText;
            public Slider GaugeSlider;

            [FormerlySerializedAs("FailOverlay")]
            [Tooltip("실패 시 빨간 플래시, 난이도 상승 직전 0.5초는 흰색 깜빡임.")]
            public Image Blur;

            public TMP_Text ScoreText;

            [Tooltip("연습: START로 준비 시 \'READY\' 표시. 본게임에선 자동 비표시.")]
            public TMP_Text PracticeReadyText;

            [Tooltip("고양이 UI Image와 같은 오브젝트의 Animator. `SpiningCat_UI` 클립을 SpinOnce/SpinLoop에 할당.")]
            public Animator CatAnimator;

            [Tooltip("슬롯 흰색 패널 배경 Image만. 본게임 유지 5초 이상이면 이 Image 알파만 0, 실패 시 Begin 때 색으로 복구.")]
            public Image SlotPanelBackgroundImage;

            [Tooltip("비참가(EMPTY) 슬롯 중앙 WAITING 문구. ACTIVE/READY/PLAYING 에선 비표시.")]
            public TMP_Text WaitingText;

            [Header("컨트롤러 가이드 UI (다이아몬드 패드)")]
            [Tooltip("Y/X/A/B 버튼을 묶는 루트. 비참가 슬롯에서는 숨김.")]
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

            [Header("셔플 이펙트")]
            [Tooltip("12글자 루프 완주 시 슬롯 중앙 스프라이트 확대·페이드.")]
            public Image ShuffleEffect;

            [Header("코믹스 BurstText 풀")]
            [Tooltip("정답 시 고양이 위치에 팝업. 5~6개 미리 배치.")]
            public TMP_Text[] BurstTextPool;

            [Tooltip("BurstText 좌표 부모. 비우면 슬롯 패널 루트.")]
            public RectTransform BurstTextContainer;
        }
    }
}
