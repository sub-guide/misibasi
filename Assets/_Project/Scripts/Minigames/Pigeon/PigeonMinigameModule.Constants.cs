namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        public const int SlotCount = 4;

        /// <summary>PlayField 로컬 유닛/초. 사용자 승인 2026-09-09.</summary>
        public const float DefaultCursorSpeed = 120f;

        /// <summary>한 쏟기당 면 더미 수. 사용자 승인 2026-09-09.</summary>
        public const int DefaultPilesPerPour = 5;

        /// <summary>NoodlePosition 로컬 지터 반경. 사용자 승인 2026-09-09.</summary>
        public const float DefaultSpawnJitterRadius = 30f;

        /// <summary>거꾸로 끝난 뒤 다음 정방향까지 초. 사용자 승인 2026-09-09.</summary>
        public const float DefaultPourCooldown = 4f;

        /// <summary>중앙에 가까운 면부터 다음 면까지 초. 추천 2026-09-10.</summary>
        public const float DefaultPileSpawnStagger = 0.15f;

        /// <summary>CupNoodle.controller 정방향 상태. 에디터 이름 계약.</summary>
        public const string CupPourForwardState = "CupNoodle";

        /// <summary>같은 클립, 상태 Speed -1. 에디터 이름 계약.</summary>
        public const string CupPourReverseState = "CupNoodleReverse";

        /// <summary>CupNoodle.anim stop. cupPourDuration이 0이면 클립 길이, 없으면 이 값.</summary>
        public const float DefaultCupPourDuration = 1.8833333f;

        /// <summary>Pigeon.controller 정방향. 에디터 이름 계약.</summary>
        public const string PeckForwardState = "Peck";

        /// <summary>같은 Peck.anim, 상태 Speed -1. 에디터 이름 계약.</summary>
        public const string PeckReverseState = "PeckReverse";

        /// <summary>Peck.anim stop. peckDuration 0이면 클립 길이, 없으면 이 값.</summary>
        public const float DefaultPeckDuration = 0.33333334f;

        /// <summary>면 더미 1개. 기획 확정.</summary>
        public const int DefaultScorePerPile = 100;
    }
}
