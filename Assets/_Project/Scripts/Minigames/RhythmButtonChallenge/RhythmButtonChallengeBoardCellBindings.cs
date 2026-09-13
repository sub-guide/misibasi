using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// 보드 칸. Inspector에서 ButtonIcon · ActiveHighlight 연결.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RhythmButtonChallengeBoardCellBindings : MonoBehaviour
    {
        public Image ButtonIcon;
        public Image ActiveHighlight;
    }
}
