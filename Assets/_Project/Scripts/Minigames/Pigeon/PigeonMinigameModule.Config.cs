using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        [Header("표시")]
        [SerializeField] string displayName = "비둘기야 먹자";

        [Header("커서 (1P~4P)")]
        [Tooltip("Hierarchy Cursor_P1 … Cursor_P4. 인덱스 0=1P.")]
        [SerializeField] Transform[] cursors = new Transform[SlotCount];

        [Header("이동")]
        [Tooltip("PlayField 로컬 유닛/초. D-Pad 홀드 이동.")]
        [SerializeField] float cursorSpeed = DefaultCursorSpeed;

        [Tooltip("화면 안으로 가둘 카메라. 비우면 가두지 않음.")]
        [SerializeField] Camera playCamera;

        [Header("면 스폰")]
        [Tooltip("바닥 면 더미. Assets/Pigeon/Prefabs/Noodle.")]
        [SerializeField] GameObject noodlePrefab;

        [Tooltip("Hierarchy CupNoodle. 시작 비활성, 켜면 정방향.")]
        [SerializeField] GameObject cupNoodle;

        [Tooltip("CupNoodle의 Animator. 드래그. GetComponent 하지 않음.")]
        [SerializeField] Animator cupAnimator;

        [Tooltip("쏟기 기준점. Hierarchy NoodlePosition.")]
        [SerializeField] Transform noodlePosition;

        [Tooltip("더미 부모. 비우면 NoodlePosition의 부모(PlayField). 면에 붙이면 다음 쏟기 때 같이 움직임.")]
        [SerializeField] Transform pileParent;

        [Tooltip("한 쏟기에 깔 개수.")]
        [SerializeField] int pilesPerPour = DefaultPilesPerPour;

        [Tooltip("NoodlePosition 로컬 원 안 무작위 오프셋.")]
        [SerializeField] float spawnJitterRadius = DefaultSpawnJitterRadius;

        [Tooltip("중앙에 가까운 더미부터 다음 더미까지 초.")]
        [SerializeField] float pileSpawnStagger = DefaultPileSpawnStagger;

        [Tooltip("거꾸로 재생이 끝난 뒤 다음 정방향까지 초.")]
        [SerializeField] float pourCooldown = DefaultPourCooldown;

        [Tooltip("정방향·거꾸로 각각 대기 초. 0이면 Animator 클립 길이.")]
        [SerializeField] float cupPourDuration;
    }
}
