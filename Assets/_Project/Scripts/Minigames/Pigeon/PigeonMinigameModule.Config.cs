using MiniParty.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Tooltip("Hierarchy Soup. 없어도 됨. 첫 면이 깔릴 때 한 번.")]
        [SerializeField] GameObject soup;

        [Tooltip("Soup의 Animator. 드래그. GetComponent 하지 않음.")]
        [SerializeField] Animator soupAnimator;

        [Header("쪽기")]
        [Tooltip("Cursor_P* 자식 Pigeon. 평소 비활성.")]
        [SerializeField] GameObject[] peckPigeons = new GameObject[SlotCount];

        [Tooltip("각 Pigeon의 Animator.")]
        [SerializeField] Animator[] peckAnimators = new Animator[SlotCount];

        [Tooltip("Pigeon 자식 입 Noodle. 적중 역재생 때만.")]
        [SerializeField] GameObject[] mouthNoodles = new GameObject[SlotCount];

        [Tooltip("판정용. Cursor_P* 의 CircleCollider2D.")]
        [SerializeField] CircleCollider2D[] peckCursorColliders = new CircleCollider2D[SlotCount];

        [Tooltip("정방향·거꾸로 각각 초. 0이면 Peck 클립 길이.")]
        [SerializeField] float peckDuration;

        [Tooltip("적중 1개 점수.")]
        [SerializeField] int scorePerPile = DefaultScorePerPile;

        [Tooltip("구~. 없어도 됨.")]
        [SerializeField] AudioSource peckSfxSource;

        [Tooltip("비둘기 등장마다 이 중 하나를 PlayOneShot. 빈 칸은 건너뜀.")]
        [SerializeField] AudioClip[] peckSfxClips = new AudioClip[5];

        [Header("점수 HUD")]
        [Tooltip("Canvas Score_P1 … Score_P4 TMP. 인덱스 0=1P.")]
        [SerializeField] TMP_Text[] scoreLabels = new TMP_Text[SlotCount];

        [Tooltip("Canvas Player1 … Player4. 비참가면 이 슬롯 전체를 끔.")]
        [SerializeField] GameObject[] scoreHudSlots = new GameObject[SlotCount];

        [Header("제한시간")]
        [Tooltip("본게임 초. 0 이하면 타이머 없음.")]
        [SerializeField] float roundDuration = DefaultRoundDuration;

        [Tooltip("Canvas 하단 채움 Image. Type=Filled, Horizontal, Origin Left. 빨간 경과.")]
        [SerializeField] Image roundTimerFill;

        [Tooltip("경과/전체. 예: 0:05/0:30. Timer 자식 Text. 비어 있으면 글자 없음.")]
        [SerializeField] TMP_Text roundTimerLabel;

        [Tooltip("Bar의 Circle. 게임 시간 동안 X만 Start→End. Y는 유지. 비어 있으면 안 움직임.")]
        [SerializeField] RectTransform roundTimerPlayhead;

        [Tooltip("재생 머리 Anchored X. 시작.")]
        [SerializeField] float playheadStartX = -900f;

        [Tooltip("재생 머리 Anchored X. 종료.")]
        [SerializeField] float playheadEndX = 900f;

        [Tooltip("경과 바 뒤의 하얀 로드. 비어 있으면 빨강 타이머만. Type=Filled, Horizontal, Origin Left.")]
        [SerializeField] Image roundTimerBuffer;

        [Tooltip("시작 시 하양이 미리 차 있는 초.")]
        [SerializeField] float bufferFirstChunkSeconds = 3f;

        [Tooltip("한 번에 뛰는 로드 초. 최소.")]
        [SerializeField] float bufferChunkMinSeconds = 1.5f;

        [Tooltip("한 번에 뛰는 로드 초. 최대.")]
        [SerializeField] float bufferChunkMaxSeconds = 4f;

        [Tooltip("다음 점프까지 기다리는 초. 최소.")]
        [SerializeField] float bufferJumpIntervalMinSeconds = 0.6f;

        [Tooltip("다음 점프까지 기다리는 초. 최대.")]
        [SerializeField] float bufferJumpIntervalMaxSeconds = 1.8f;

        [Header("종료 → Result 씬")]
        [Tooltip("Canvas FadeOverlay 의 ScreenFader.")]
        [SerializeField] ScreenFader exitScreenFader;

        [SerializeField] float sessionEndHoldSeconds = 0.35f;
        [SerializeField] float exitFadeOutSeconds = 1f;
    }
}
