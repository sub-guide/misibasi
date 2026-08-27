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

        [Tooltip("LB/RB 단일 입력 시 기본 탭 이동 속도.")]
        [SerializeField] float seesawBaseSpeed = DefaultSeesawBaseSpeed;

        [Tooltip("홀드 누적 시 최대 가속 배율.")]
        [SerializeField] float holdMaxMultiplier = DefaultHoldMaxMultiplier;

        [Tooltip("홀드 최대 가속에 도달하는 시간(초).")]
        [SerializeField] float holdAccelTime = DefaultHoldAccelTime;

        [Tooltip("미입력·LB/RB 동시 입력 시 현재 기울기 방향(중앙 이탈)으로 계속 미는 중력형 도치 속도.")]
        [SerializeField] float microDriftSpeed = DefaultMicroDriftSpeed;

        [Tooltip("중앙(0.5) 이탈량 제곱에 비례하는 비선형 가속 계수.")]
        [SerializeField] float pullCoefficient = DefaultPullCoefficient;

        [Header("자율 스텝 노이즈 (고정 · Sine)")]
        [Tooltip("Sine 파동 주파수(Hz). DanceWave = Sin(2π·f·t).")]
        [SerializeField] float danceSineHz = DefaultDanceSineHz;

        [Tooltip("DanceWave 진폭. x = Clamp01(bias + wave×Amp).")]
        [SerializeField] float noiseAmp = DefaultNoiseAmp;

        [Header("FailFloor (접촉 1회 복구 · 본게임 감점)")]
        [Tooltip("본게임 FailFloor 접촉 1회당 감점. 0 미만으로 내려가지 않음. 연습은 감점 없음.")]
        [SerializeField] int failFloorPenaltyScore = DefaultFailFloorPenaltyScore;

        [Tooltip("FailFloor 접촉 시 관이 SmoothStep으로 이동하는 로컬 Y. 1~4P 공통. Play에서 조절.")]
        [SerializeField] float failFloorRecoverLocalY = DefaultFailFloorRecoverLocalY;

        [Tooltip("FailFloor 복구 시간(초). 같은 시간 동안 관↔어깨 충돌을 무시. 0이면 그 프레임에 목표로 붙인 뒤 중력. Play에서 조절.")]
        [SerializeField] float failFloorRecoverDuration = DefaultFailFloorRecoverDuration;

        [Header("어깨 겹침")]
        [Tooltip("시소가 최대(0/1)가 아닐 때, 관이 어깨와 겹치면 +Y로만 밀어 올리는 한 프레임 최대량.")]
        [SerializeField] float shoulderDepenetrationMaxY = DefaultShoulderDepenetrationMaxY;

        [Header("점수")]
        [Tooltip("노이즈 포함 최종 시소 x가 0.5에서 이 값 이내면 정중앙. 기본 0.05 → 0.45~0.55.")]
        [SerializeField] float centerZoneThreshold = DefaultCenterZoneThreshold;

        [Tooltip("정중앙 유지 시 초당 추가 점수. 생존 100과 합쳐 250. Phase4에서는 획득 ×2.")]
        [SerializeField] float centerBonusScorePerSec = DefaultCenterBonusScorePerSec;

        [Header("정중앙 카메라")]
        [Tooltip("정중앙+어깨 지지 시 목표 FOV = 슬롯 카메라 rest FOV × 이 값. 기본 0.85.")]
        [SerializeField] float centerFovMultiplier = DefaultCenterFovMultiplier;

        [Tooltip("정중앙 유지 중 카메라 로컬 Z = 관 로컬 Z × 이 값. 기본 0.5.")]
        [SerializeField] float centerCamTiltRatio = DefaultCenterCamTiltRatio;

        [Tooltip("줌인 블렌드 0→1에 걸리는 시간(초). SmoothStep.")]
        [SerializeField] float camZoomInDuration = DefaultCamZoomInDuration;

        [Tooltip("원복 시간 = 줌인 시간 ÷ 이 값. 기본 3 → 0.15초.")]
        [SerializeField] float camZoomOutSpeedMul = DefaultCamZoomOutSpeedMul;

        [Tooltip("정중앙 유지 중 카메라 Z √추종 세기. 클수록 관 기울기에 빨리 붙음.")]
        [SerializeField] float camTiltFollowGain = DefaultCamTiltFollowGain;

        [Tooltip("정중앙 이탈 시 Z를 0으로 되돌리는 스프링 주파수(Hz).")]
        [SerializeField] float camReturnSpringHz = DefaultCamReturnSpringHz;

        [Tooltip("정중앙 이탈 시 Z 스프링 감쇠비. 1이면 탄력 없음, 작을수록 더 출렁.")]
        [SerializeField] float camReturnSpringDamping = DefaultCamReturnSpringDamping;

        [Header("개발 무적 (Editor / Development Build)")]
        [Tooltip("Backspace 무적 ON일 때 1P가 LB/RB를 안 누르면 bias·최종 x가 0.5로 돌아오는 속도(초당). Play에서 조절.")]
        [SerializeField] float devGodReturnSpeed = DefaultDevGodReturnSpeed;

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
