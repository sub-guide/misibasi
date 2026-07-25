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

        [Header("물리 · SoftReset")]
        [SerializeField] float initialTiltDegrees = 6f;
        [SerializeField] float initialAngularSpeed = 25f;

        [Header("어깨 승강 (←/→)")]
        [Tooltip("시작·입력 없을 때 무릎 extension. 0=완전 앉음 · 1=기립 · 0.5≈반쯤 굽힘.")]
        [SerializeField] [Range(0f, 1f)] float neutralExtension = DefaultNeutralExtension;

        [Tooltip("입력으로 extension이 변하는 초당 속도.")]
        [SerializeField] float shoulderRaiseSpeed = 1.4f;

        [Tooltip("입력 없을 때 Neutral로 돌아오는 속도.")]
        [SerializeField] float shoulderReturnSpeed = 1.1f;

        [Header("점프 (자유 · A/button2)")]
        [Tooltip("점프~착지 총 시간(초). 이 동안 ←/→ 불가.")]
        [SerializeField] float jumpLockoutSeconds = 0.35f;

        [Header("Phase 민감도 가중")]
        [SerializeField] float phase2ShoulderMul = 1.25f;
        [SerializeField] float phase3ShoulderMul = 1.55f;
        [SerializeField] float phase4ShoulderMul = 2f;

        [Header("HP")]
        [SerializeField] int hpLowScoreThreshold = DefaultLowScoreThreshold;

        [Header("연출")]
        [Tooltip("관·운구인 정면이 화면 좌측을 보도록 TiltRoot Y 회전(도).")]
        [SerializeField] float presentationYawDegrees = 22f;

        [Header("슬롯 화면 분할")]
        [Tooltip("슬롯 루트를 X축으로 떨어뜨려 서로 카메라에 안 보이게 함.")]
        [SerializeField] float slotWorldSpacing = 40f;

        [Tooltip("Begin 시 Main Camera 비활성화 (슬롯 카메라만 사용).")]
        [SerializeField] bool disableMainCameraOnBegin = true;

        [Tooltip("슬롯 자식 Canvas를 Screen Space - Camera 로 바꿔 해당 슬롯 viewport에만 그림.")]
        [SerializeField] bool bindSlotCanvasesToSlotCamera = true;
    }
}
