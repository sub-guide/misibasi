using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>1.5단계: 레거시 메타(Burst/Shuffle/Neon) 제거. 타이머성 필드만 감소.</summary>
        void TickMeta(int i)
        {
            ref SlotRuntime sr = ref _slots[i];

            if (sr.InputLockTimer > 0f)
                sr.InputLockTimer -= Time.deltaTime;

            if (sr.FailFlashTimer > 0f)
                sr.FailFlashTimer -= Time.deltaTime;

            if (sr.TierBumpBlurRemaining > 0f)
                sr.TierBumpBlurRemaining -= Time.deltaTime;
        }

        /// <summary>
        /// 1.5단계: 문자 패턴·게이지·가이드 판정 제거.
        /// 연습 START READY 는 <see cref="TickPracticeReadyAndMaybeStartMain"/> 담당.
        /// 2단계에서 10키 활성 타겟 판정으로 교체.
        /// </summary>
        void TickGameplay(int i)
        {
            // intentionally empty until Phase 2 DJ pad gameplay
        }
    }
}
