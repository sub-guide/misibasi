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
    }
}
