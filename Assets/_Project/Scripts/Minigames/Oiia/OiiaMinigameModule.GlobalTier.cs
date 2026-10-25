using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>글로벌 2티어 시작(초, 본게임 경과). T1 = 0–27.</summary>
        public const float GlobalTier2StartSeconds = 27f;

        /// <summary>글로벌 3티어 시작(초). T2 = 27–33.5.</summary>
        public const float GlobalTier3StartSeconds = 33.5f;

        /// <summary>T2 진입 전 흰색 Beam 스트로보 시작(초).</summary>
        public const float BeamTeaseBeforeTier2StartSeconds = 15f;

        /// <summary>T3 진입 전 흰색 Beam 스트로보 시작(초).</summary>
        public const float BeamTeaseBeforeTier3StartSeconds = 32f;

        /// <summary>본게임 경과 시간(초). 0 → <see cref="MainRoundDurationSeconds"/>.</summary>
        float GetMainElapsedSeconds()
        {
            if (!_running || _ctx.IsPractice)
                return 0f;

            return Mathf.Clamp(
                MainRoundDurationSeconds - _remainingMainTime,
                0f,
                MainRoundDurationSeconds);
        }

        /// <summary>
        /// 본게임은 시작부터 끝까지 글로벌 3티어(클럽·전 키 정답). 연습 모드에서는 1.
        /// </summary>
        int ResolveGlobalTier()
        {
            if (!_running || _ctx.IsPractice)
                return 1;

            return 3;
        }

        /// <summary>티어 전환이 없으므로 흰색 Beam 예고 점멸도 없다.</summary>
        bool IsBeamTierTeaseWindow() => false;
    }
}
