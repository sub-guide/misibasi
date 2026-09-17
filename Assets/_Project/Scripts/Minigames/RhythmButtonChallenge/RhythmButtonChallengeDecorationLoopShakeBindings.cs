using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// Decoration 자식 RectTransform. Inspector에서 LoopShakeTargets 연결.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RhythmButtonChallengeDecorationLoopShakeBindings : MonoBehaviour
    {
        public RectTransform[] LoopShakeTargets;
    }
}
