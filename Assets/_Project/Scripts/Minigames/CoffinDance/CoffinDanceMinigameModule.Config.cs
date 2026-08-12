using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        [Header("표시")]
        [SerializeField] string displayName = "관짝춤";

        [Header("슬롯 (1P~4P)")]
        [SerializeField] CoffinDanceSlotBindings[] slotBindings = new CoffinDanceSlotBindings[SlotCount];

        [Header("공용 HUD")]
        [SerializeField] TMP_Text mainRoundTimerCentralTop;
        [SerializeField] TMP_Text phaseLabelText;

        [Header("시소 (단일 x)")]
        [Tooltip("시작·중립 시소 위치. Y_L=x · Y_R=1-x. 기본 0.5=수평.")]
        [SerializeField] [Range(0f, 1f)] float xSeesawNeutral = DefaultSeesawNeutral;

        [Tooltip("←/→ 단일 입력 시 기본 탭 이동 속도.")]
        [SerializeField] float seesawBaseSpeed = DefaultSeesawBaseSpeed;

        [Tooltip("홀드 누적 시 최대 가속 배율.")]
        [SerializeField] float holdMaxMultiplier = DefaultHoldMaxMultiplier;

        [Tooltip("홀드 최대 가속에 도달하는 시간(초).")]
        [SerializeField] float holdAccelTime = DefaultHoldAccelTime;

        [Tooltip("미입력·좌우 동시 입력 시 현재 기울기 방향(중앙 이탈)으로 계속 미는 중력형 도치 속도.")]
        [SerializeField] float microDriftSpeed = DefaultMicroDriftSpeed;

        [Tooltip("중앙(0.5) 이탈량 제곱에 비례하는 비선형 가속 계수.")]
        [SerializeField] float pullCoefficient = DefaultPullCoefficient;

        [Header("자율 스텝 노이즈 (고정 · Sine)")]
        [Tooltip("Sine 파동 주파수(Hz). DanceWave = Sin(2π·f·t).")]
        [SerializeField] float danceSineHz = DefaultDanceSineHz;

        [Tooltip("DanceWave 진폭. x = Clamp01(bias + wave×Amp).")]
        [SerializeField] float noiseAmp = DefaultNoiseAmp;

        [Header("점프 (자유 · A/button2 · 물리)")]
        [Tooltip("점프 시 루트 Rigidbody VelocityChange Impulse (Y). Module 공용.")]
        [SerializeField] float jumpImpulse = DefaultJumpImpulse;

        [Tooltip("현재 프레임→JumpStart 첫 프레임 페이드(초). 끝난 뒤 모션+Impulse. 0=즉시.")]
        [SerializeField] float jumpAnimBlendSeconds = DefaultJumpAnimBlendSeconds;

        [Tooltip("JumpStart Animator.speed. 에디터에서 조절.")]
        [SerializeField] float jumpStartAnimSpeed = DefaultJumpStartAnimSpeed;

        [Tooltip("JumpLand Animator.speed. 에디터에서 조절. Land 클립 끝난 뒤 조작 재개.")]
        [SerializeField] float jumpLandAnimSpeed = DefaultJumpLandAnimSpeed;

        [Tooltip("JumpLand 시작 시 rest+offset으로 점진 이동. 클립 종료 시 rest 즉시 복귀. 음수=내림.")]
        [SerializeField] float jumpLandYOffset = DefaultJumpLandYOffset;

        [Tooltip("Y 오프셋까지 점진 이동 시간(초). 0이면 즉시 스냅. 복귀는 즉시.")]
        [SerializeField] float jumpLandYOffsetDuration = DefaultJumpLandYOffsetDuration;

        [Tooltip("착지(Land 클립 종료) 직후 미세 도치 속도 배율.")]
        [SerializeField] float landingDriftMultiplier = DefaultLandingDriftMultiplier;

        [Tooltip("착지 도치 증폭 유지 시간(초).")]
        [SerializeField] float landingDriftDuration = DefaultLandingDriftDuration;

        [Header("HP")]
        [SerializeField] int hpLowScoreThreshold = DefaultLowScoreThreshold;

        [Header("슬롯 화면 분할")]
        [Tooltip("슬롯 루트를 X축으로 떨어뜨려 서로 카메라에 안 보이게 함.")]
        [SerializeField] float slotWorldSpacing = 40f;

        [Tooltip("Begin 시 Main Camera 비활성화 (슬롯 카메라만 사용).")]
        [SerializeField] bool disableMainCameraOnBegin = true;

        [Tooltip("슬롯 자식 Canvas를 Screen Space - Camera 로 바꿔 해당 슬롯 viewport에만 그림.")]
        [SerializeField] bool bindSlotCanvasesToSlotCamera = true;
    }
}
