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
        /// 시간 기반 글로벌 티어. 1 = 0–27s, 2 = 27–33.5s, 3 = 33.5–60s.
        /// 연습 모드에서는 1.
        /// </summary>
        int ResolveGlobalTier()
        {
            if (!_running || _ctx.IsPractice)
                return 1;

            float elapsed = GetMainElapsedSeconds();
            if (elapsed < GlobalTier2StartSeconds)
                return 1;

            if (elapsed < GlobalTier3StartSeconds)
                return 2;

            return 3;
        }

        /// <summary>티어 전환 예고: 흰색 Beam 초고속 점멸 구간.</summary>
        bool IsBeamTierTeaseWindow()
        {
            if (!_running || _ctx.IsPractice)
                return false;

            float e = GetMainElapsedSeconds();
            bool beforeT2 = e >= BeamTeaseBeforeTier2StartSeconds && e < GlobalTier2StartSeconds;
            bool beforeT3 = e >= BeamTeaseBeforeTier3StartSeconds && e < GlobalTier3StartSeconds;
            return beforeT2 || beforeT3;
        }
    }
}
