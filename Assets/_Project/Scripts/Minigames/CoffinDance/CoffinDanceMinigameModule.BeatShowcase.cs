using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        /// <summary>빈 슬롯 시소·슬롯 카메라 박자 연출 공통 BGM/경과 시계.</summary>
        float GetBeatShowcaseMusicTime()
        {
            if (emptySlotShowcaseSyncToBgm)
            {
                AudioSource source = mainBgmSource != null ? mainBgmSource : _mainBgmRuntime;
                if (source != null && source.isPlaying && mainBgmClip != null)
                    return source.time;
            }

            return _elapsedMainTime;
        }

        float GetBeatShowcaseBpm()
        {
            return Mathf.Max(1f, emptySlotShowcaseBpm);
        }

        /// <summary>4/4 마디 안 0~4 연속 phase (시소 좌·우 패턴용).</summary>
        float GetBeatShowcaseBarBeatPhase()
        {
            float beats = GetBeatShowcaseGlobalBeats();
            float phase = beats % 4f;
            if (phase < 0f)
                phase += 4f;
            return phase;
        }

        /// <summary>전역 quarter note 누적(카메라 박마다 랜덤 방향용).</summary>
        float GetBeatShowcaseGlobalBeats()
        {
            return GetBeatShowcaseMusicTime() * GetBeatShowcaseBpm() / 60f;
        }
    }
}
