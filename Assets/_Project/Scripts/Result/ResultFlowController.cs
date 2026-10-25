using System.Collections;
using MiniParty.Flow;
using MiniParty.Input;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Result
{
    /// <summary>
    /// Result 씬 연출 상태 머신.
    /// Intro → 등수 공개 → HP 연출 → GAME OVER → Ready
    /// </summary>
    public sealed partial class ResultFlowController : MonoBehaviour
    {
        const string DefaultSlotPanelsContainerName = "Panel_Result_4Way";

        [Header("슬롯 (4)")]
        [Tooltip("비우면 `Panel_Result_4Way` 직계 자식에서 ResultSlotView 를 수집한다.")]
        [SerializeField] Transform slotPanelsContainer;

        [SerializeField] ResultSlotView[] slotViews = new ResultSlotView[4];

        readonly OperatorInputService _operatorInput = new();

        PartySession _session;
        MinigameSessionReport _report;
        bool[] _playedMask = new bool[4];
        bool _practice;
        ResultPhase _phase = ResultPhase.IntroFade;
        bool _running;

        public ResultPhase Phase => _phase;

        public void Begin(PartySession session)
        {
            if (_running)
                return;

            _session = session;

            _report = session.PeekPendingReport();
            if (_report == null)
            {
                Debug.LogError("[ResultFlowController] 대기 중인 MinigameSessionReport 가 없습니다.", this);
                ReturnToMainMenuImmediate();
                return;
            }

            _practice = session.LastPractice;
            bool[] mask = session.LastPlayedMask;
            for (var i = 0; i < 4; i++)
                _playedMask[i] = mask != null && i < mask.Length && mask[i];

            for (var i = 0; i < 4; i++)
                _willGameOver[i] = false;

            ResolveSlotViews();
            StartCoroutine(CoRunFlow());
        }

        void ResolveSlotViews()
        {
            if (slotViews != null && slotViews.Length >= 4 && !HasAnyNull(slotViews))
                return;

            Transform root = slotPanelsContainer;
            if (root == null)
            {
                var containerGo = GameObject.Find(DefaultSlotPanelsContainerName);
                if (containerGo != null)
                    root = containerGo.transform;
            }

            if (root == null)
            {
                Debug.LogError(
                    "[ResultFlowController] slotViews 가 비었고 Panel_Result_4Way 도 없습니다.",
                    this);
                return;
            }

            var found = new ResultSlotView[root.childCount];
            var count = 0;
            for (var c = 0; c < root.childCount; c++)
            {
                var view = root.GetChild(c).GetComponent<ResultSlotView>();
                if (view == null)
                    continue;

                if (count >= found.Length)
                    break;

                found[count++] = view;
            }

            if (count < 4)
            {
                Debug.LogWarning(
                    $"[ResultFlowController] Panel_Result_4Way 아래 ResultSlotView 가 {count}개입니다. 4개 필요.",
                    this);
            }

            slotViews = new ResultSlotView[4];
            for (var i = 0; i < 4; i++)
                slotViews[i] = i < count ? found[i] : null;
        }

        IEnumerator CoRunFlow()
        {
            _running = true;

            yield return CoIntroFade();
            yield return CoRankingReveal();
            yield return CoHpProcess();
            yield return CoGameOverSequence();
            yield return CoReadyPhase();
        }

        IEnumerator CoIntroFade()
        {
            _phase = ResultPhase.IntroFade;
            ApplyIntroSlotUi();
            yield break;
        }

        void ApplyIntroSlotUi()
        {
            if (slotViews == null)
                return;

            for (var i = 0; i < 4; i++)
            {
                ResultSlotView view = i < slotViews.Length ? slotViews[i] : null;
                if (view == null)
                    continue;

                if (!_playedMask[i])
                {
                    view.SetEmptySlotLook();
                    continue;
                }

                view.SetupIntro(i, participated: true);

                if (_session?.Slots != null)
                {
                    int maxHp = _session.StartingHp;
                    view.SetHp(_session.Slots[i].HP, maxHp);
                }
            }
        }

        IEnumerator CoExitToMainMenu()
        {
            ReturnToMainMenuImmediate();
            yield break;
        }

        void ReturnToMainMenuImmediate()
        {
            _running = false;

            if (_session != null)
                _session.CompleteResultAndOpenMainMenu(_report);
            else
                Debug.LogError("[ResultFlowController] PartySession 이 없어 메인으로 돌아갈 수 없습니다.", this);
        }

        static bool HasAnyNull(ResultSlotView[] views)
        {
            for (var i = 0; i < views.Length; i++)
            {
                if (views[i] == null)
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (slotViews != null && slotViews.Length == 4 && !HasAnyNull(slotViews))
                return;

            Transform root = slotPanelsContainer;
            if (root == null)
            {
                var containerGo = GameObject.Find(DefaultSlotPanelsContainerName);
                if (containerGo != null)
                    root = containerGo.transform;
            }

            if (root == null)
                return;

            var list = new System.Collections.Generic.List<ResultSlotView>(4);
            for (var c = 0; c < root.childCount; c++)
            {
                var view = root.GetChild(c).GetComponent<ResultSlotView>();
                if (view != null)
                    list.Add(view);
            }

            if (list.Count != 4)
                return;

            slotViews = list.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
