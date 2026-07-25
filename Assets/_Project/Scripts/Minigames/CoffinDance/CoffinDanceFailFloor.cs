using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 전용 실패 바닥 마커. 관 Collider가 닿으면 탈락/SoftReset.
    /// Slot 프리팹 자식 Floor(Collider)에 부착.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceFailFloor : MonoBehaviour
    {
    }
}
