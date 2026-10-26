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

        [SerializeField] string catAnimatorSpinLoopState = "SpinLoop";

        [SerializeField] int catAnimatorLayer = 0;

        static int CatStateHash(string stateName) => Animator.StringToHash(stateName);

        /// <summary>꺼진 슬롯 고양이에 Play하면 Unity가 inactive 경고를 낸다.</summary>
        static bool CanDriveCatAnimator(Animator a) =>
            a != null && a.isActiveAndEnabled;

        void ResetAllCatAnimatorsToIdle()
        {
            ForEachSlot(i =>
            {
                if (!TryGetBinding(i, out SlotUiBindings b))
                    return;

                Animator a = b.CatAnimator;
                if (!CanDriveCatAnimator(a))
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

        void UpdateCatAnimationMode(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            Animator a = b.CatAnimator;
            if (!CanDriveCatAnimator(a))
                return;

            int layer = catAnimatorLayer;
            AnimatorStateInfo st = a.GetCurrentAnimatorStateInfo(layer);

            if (_aliveMask[i])
            {
                if (!st.IsName(catAnimatorSpinLoopState))
                    a.Play(CatStateHash(catAnimatorSpinLoopState), layer, 0f);

                return;
            }

            if (st.IsName(catAnimatorSpinLoopState))
                a.Play(CatStateHash(catAnimatorIdleState), layer, 0f);
        }
    }
}
