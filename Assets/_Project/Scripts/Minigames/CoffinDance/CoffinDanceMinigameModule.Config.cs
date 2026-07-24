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
    }
}
