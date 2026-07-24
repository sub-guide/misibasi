using System.Collections;
using MiniParty.Flow;
using MiniParty.Input;
using MiniParty.Minigames;
using MiniParty.Minigames.CoffinDance;
using MiniParty.Minigames.Oiia;
using MiniParty.Minigames.RhythmButtonChallenge;
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

        [Header("페이드")]
        [SerializeField] ScreenFader screenFader;

        [Header("타이밍")]
        [SerializeField] float introFadeInSeconds = 1f;

        [SerializeField] float exitFadeOutSeconds = 1f;

        [Header("미니게임별 장식 (선택)")]
        [SerializeField] MonoBehaviour[] minigameFlavorProviders;

        readonly OperatorInputService _operatorInput = new();
        IResultMinigameFlavor[] _flavors;

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
            ResolveMinigameFlavors();
            StartCoroutine(CoRunFlow());
        }

        void ResolveMinigameFlavors()
        {
            if (minigameFlavorProviders == null || minigameFlavorProviders.Length == 0)
            {
                _flavors = new IResultMinigameFlavor[]
                {
                    new OiiaResultMinigameFlavor(),
                    new RhythmButtonChallengeResultMinigameFlavor(),
                    new CoffinDanceResultMinigameFlavor()
                };
                return;
            }

            var list = new System.Collections.Generic.List<IResultMinigameFlavor>();
            for (var i = 0; i < minigameFlavorProviders.Length; i++)
            {
                if (minigameFlavorProviders[i] is IResultMinigameFlavor flavor)
                    list.Add(flavor);
            }

            _flavors = list.Count > 0 ? list.ToArray() : new IResultMinigameFlavor[]
            {
                new OiiaResultMinigameFlavor(),
                new RhythmButtonChallengeResultMinigameFlavor(),
                new CoffinDanceResultMinigameFlavor()
            };
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

            if (screenFader != null)
            {
                screenFader.SetInstant(1f);
                yield return screenFader.FadeTo(0f, introFadeInSeconds);
            }
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
            }

            ApplyMinigameFlavorIntro();
        }

        void ApplyMinigameFlavorIntro()
        {
            if (_flavors == null || _report == null)
                return;

            string id = _report.MinigameId;
            for (var f = 0; f < _flavors.Length; f++)
            {
                if (_flavors[f].TryApplyIntro(id, slotViews, _playedMask, _report, _practice))
                    break;
            }
        }

        IEnumerator CoExitToMainMenu()
        {
            if (screenFader != null && exitFadeOutSeconds > 0f)
                yield return screenFader.FadeTo(1f, exitFadeOutSeconds);

            ReturnToMainMenuImmediate();
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
