using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 전용 바닥 마커. 관 Collider가 닿으면 로컬 Y·Z=0 SmoothStep 복구(·본게임 감점). 탈락/SoftReset 없음.
    /// Slot 프리팹 자식 Floor(Collider)에 부착.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceFailFloor : MonoBehaviour
    {
    }
}
