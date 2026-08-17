namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        public const int SlotCount = 4;

        public const float MainDurationSeconds = 60f;

        public const float SurvivalScorePerSecond = 100f;

        /// <summary>최종 시소 x가 0.5에서 이 값 이내면 정중앙 보너스. 기본 0.05 → 0.45~0.55.</summary>
        public const float DefaultCenterZoneThreshold = 0.05f;

        /// <summary>정중앙 유지 시 초당 추가 점수. Phase4 배율은 생존 점수와 같이 적용.</summary>
        public const float DefaultCenterBonusScorePerSec = 150f;

        /// <summary>FailFloor 접촉 1회당 월드 +Y Impulse (ForceMode.Impulse).</summary>
        public const float DefaultFailFloorUpwardImpulse = 80f;

        /// <summary>FailFloor 접촉 1회당 본게임 감점. 연습은 0.</summary>
        public const int DefaultFailFloorPenaltyScore = 500;

        /// <summary>FailFloor 접촉 후 관↔어깨 SphereCollider IgnoreCollision 유지 시간(초).</summary>
        public const float DefaultFailFloorShoulderIgnoreSeconds = 0.3f;

        /// <summary>어깨와 겹칠 때 관을 +Y로만 밀어 올리는 한 프레임 최대량.</summary>
        public const float DefaultShoulderDepenetrationMaxY = 0.5f;

        public const float Phase1EndSeconds = 20f;
        public const float Phase2EndSeconds = 40f;
        public const float Phase3EndSeconds = 50f;

        public const float SessionEndDelaySeconds = 1f;

        public const int DefaultLowScoreThreshold = 3000;

        public const float DefaultSeesawNeutral = 0.5f;
        public const float MinExtension = 0f;
        public const float MaxExtension = 1f;

        /// <summary>LB/RB 기본 탭 이동 속도.</summary>
        public const float DefaultSeesawBaseSpeed = 1.2f;

        /// <summary>홀드 조작 시 최대 가속 배율.</summary>
        public const float DefaultHoldMaxMultiplier = 3.0f;

        /// <summary>홀드 최대 가속 도달 시간(초).</summary>
        public const float DefaultHoldAccelTime = 0.2f;

        /// <summary>미입력·동시 입력 시 현재 기울기 방향 중력형 미세 도치 속도.</summary>
        public const float DefaultMicroDriftSpeed = 0.5f;

        /// <summary>중앙(0.5) 이탈 시 비선형 가속 계수.</summary>
        public const float DefaultPullCoefficient = 2.0f;

        /// <summary>고정 씰룩임 진폭 (Phase 난이도 없음 · 후속 재도입 가능).</summary>
        public const float DefaultNoiseAmp = 0.12f;

        public const float DefaultDanceSineHz = 1.2f;
    }
}
