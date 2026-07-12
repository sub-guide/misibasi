using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("고양이 Animator (UI Image일 때)")]
        [Tooltip(
            "Animator는 Image와 같은 GameObject에 두세요. 클립은 SpriteRenderer용(SpiningCat.anim)이 아니라 " +
            "`SpiningCat_UI.anim` / `SpiningCat_UI_Loop.anim`(Image의 m_Sprite)을 컨트롤러에 넣어야 합니다.")]
        [SerializeField] string catAnimatorIdleState = "Idle";

        [SerializeField] string catAnimatorSpinOnceState = "SpinOnce";

        [SerializeField] string catAnimatorSpinLoopState = "SpinLoop";

        [SerializeField] int catAnimatorLayer = 0;

        bool ShouldCatLoopInMain(ref SlotRuntime sr, int i)
        {
            if (_ctx.IsPractice)
                return false;

            if (!_aliveMask[i])
                return false;

            // 글로벌 3티어: 미스·입력 잠금과 무관하게 SpinLoop 유지 (원작 밈).
            return ResolveGlobalTier() >= 3;
        }

        static int CatStateHash(string stateName) => Animator.StringToHash(stateName);

        void ResetAllCatAnimatorsToIdle()
        {
            ForEachSlot(i =>
            {
                if (!TryGetBinding(i, out SlotUiBindings b))
                    return;

                Animator a = b.CatAnimator;
                if (a == null)
                    return;

                ConfigureCatAnimator(a);
                a.Play(CatStateHash(catAnimatorIdleState), catAnimatorLayer, 0f);
            });
        }

        static void ConfigureCatAnimator(Animator a)
        {
            if (a == null) return;

            // UI Image는 레이어/카메라 정리 때문에 기본 Culling에서 애니가 멈출 수 있음
            a.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        bool IsCatSpinOnceStillRunning(AnimatorStateInfo st)
        {
            return st.IsName(catAnimatorSpinOnceState) && st.normalizedTime < 0.99f;
        }

        void UpdateCatAnimationMode(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            Animator a = b.CatAnimator;
            if (a == null)
                return;

            ref SlotRuntime sr = ref _slots[i];

            int layer = catAnimatorLayer;

            bool wantLoop = ShouldCatLoopInMain(ref sr, i);
            AnimatorStateInfo st = a.GetCurrentAnimatorStateInfo(layer);

            if (wantLoop)
            {
                // SpinOnce 한 사이클 도중에는 끊지 않고, 끝난 뒤 Loop로 진입
                if (IsCatSpinOnceStillRunning(st))
                    return;

                if (!st.IsName(catAnimatorSpinLoopState))
                    a.Play(CatStateHash(catAnimatorSpinLoopState), layer, 0f);

                return;
            }

            if (st.IsName(catAnimatorSpinLoopState))
                a.Play(CatStateHash(catAnimatorIdleState), layer, 0f);
        }

        void ForceCatAnimatorIdle(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            Animator a = b.CatAnimator;
            if (a == null)
                return;

            ConfigureCatAnimator(a);
            a.Play(CatStateHash(catAnimatorIdleState), catAnimatorLayer, 0f);
        }

        void PlayCatSingleCycleIfIdle(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            Animator a = b.CatAnimator;
            if (a == null)
                return;

            ref SlotRuntime sr = ref _slots[i];

            if (ShouldCatLoopInMain(ref sr, i))
                return;

            int layer = catAnimatorLayer;
            AnimatorStateInfo st = a.GetCurrentAnimatorStateInfo(layer);

            if (IsCatSpinOnceStillRunning(st))
                return;

            if (st.IsName(catAnimatorSpinLoopState))
                return;

            a.Play(CatStateHash(catAnimatorSpinOnceState), layer, 0f);
        }
    }
}
