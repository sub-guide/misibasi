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

        [Header("물리")]
        [SerializeField] float gravityTorque = 2.8f;
        [SerializeField] float controlTorque = 9.5f;
        [SerializeField] float rotationalDamping = 0.35f;
        [SerializeField] float maxAngularSpeed = 8f;
        [SerializeField] float initialTiltDegrees = 6f;
        [SerializeField] float initialAngularSpeed = 0.4f;

        [Tooltip("관 기울기 절대값 상한(도). 이 각도를 넘지 않음. 도달 시 Stumble 유예 후 탈락.")]
        [SerializeField] [Range(10f, 90f)] float maxTiltDegrees = DefaultMaxTiltDegrees;

        [Tooltip("최대 각도 도달 후 탈락까지 유예 시간(초).")]
        [SerializeField] [Min(0f)] float stumbleBufferSeconds = DefaultStumbleBufferSeconds;

        [Header("점프")]
        [SerializeField] float jumpInputWindowSeconds = 1.4f;
        [SerializeField] float jumpLockoutSeconds = 0.35f;
        [SerializeField] float jumpLandingTorqueImpulse = 3.2f;
        [SerializeField] float jumpFailTiltImpulse = 2.4f;
        [SerializeField] [Range(0f, 1f)] float doubleJumpChanceFromPhase3 = DefaultDoubleJumpChanceFromPhase3;

        [Header("Phase 외력·가중")]
        [SerializeField] float phase2ExternalForce = 0.35f;
        [SerializeField] float phase3GravityMul = 1.35f;
        [SerializeField] float phase4GravityMul = 2f;
        [SerializeField] float phase4InertiaMul = 1.6f;

        [Header("HP")]
        [SerializeField] int hpLowScoreThreshold = DefaultLowScoreThreshold;

        [Header("연출")]
        [SerializeField] float presentationYawDegrees = 22f;

        [Header("슬롯 화면 분할")]
        [Tooltip("슬롯 루트를 X축으로 떨어뜨려 서로 카메라에 안 보이게 함.")]
        [SerializeField] float slotWorldSpacing = 40f;

        [Tooltip("Begin 시 Main Camera 비활성화 (슬롯 카메라만 사용).")]
        [SerializeField] bool disableMainCameraOnBegin = true;

        [Tooltip("슬롯 자식 Canvas를 Screen Space - Camera 로 바꿔 해당 슬롯 viewport에만 그림.")]
        [SerializeField] bool bindSlotCanvasesToSlotCamera = true;

        [Header("운구인 ↔ 관 모서리 (에디터 튜닝)")]
        [Tooltip("켜면 관 모서리 높이에 맞춰 운구인 키를 조절.")]
        [SerializeField] bool scalePallbearersToCoffinCorners = true;

        [Tooltip("0 = 원래 크기 유지 · 1 = 모서리에 완전 맞춤. 과장되면 값을 낮추세요.")]
        [SerializeField] [Range(0f, 1f)] float pallbearerCornerFollow = 0.25f;

        [Tooltip("목표 머리 높이 오프셋(로컬 Y). 음수면 모서리보다 아래.")]
        [SerializeField] float pallbearerCornerHeightOffset = -0.15f;

        [Tooltip("운구인 localScale.y 하한.")]
        [SerializeField] float pallbearerMinScaleY = 0.7f;

        [Tooltip("운구인 localScale.y 상한.")]
        [SerializeField] float pallbearerMaxScaleY = 1.35f;

        [Tooltip("Unity Capsule 기본 half-height(로컬). Scale.y 와 곱해 머리 높이 계산.")]
        [SerializeField] float pallbearerCapsuleHalfHeight = 1f;
    }
}
