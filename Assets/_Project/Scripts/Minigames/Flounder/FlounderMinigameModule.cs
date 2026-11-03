using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed class FlounderMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "flounder";

        [SerializeField] string displayName = "물 만난 광어";
        [SerializeField] FlounderGameManager gameManager;

        [Header("종료 연출")]
        [Tooltip("Canvas FadeOverlay 의 ScreenFader.")]
        [SerializeField] ScreenFader exitScreenFader;

        [Tooltip("라운드 종료 후 화면 정지 유지(초)")]
        [SerializeField] float sessionEndHoldSeconds = 3f;

        [Tooltip("정지 후 페이드 아웃(초)")]
        [SerializeField] float exitFadeOutSeconds = 0.85f;

        MinigameContext _ctx;
        bool _running;
        bool _completing;

        public string Id => BuiltInId;
        public string DisplayName => displayName;

        public void Begin(MinigameContext context)
        {
            _ctx = context;
            _running = true;
            _completing = false;

            if (gameManager == null)
                gameManager = GetComponent<FlounderGameManager>();

            if (exitScreenFader != null)
                exitScreenFader.SetInstant(0f);

            gameManager?.Begin(context);
        }

        public void Tick()
        {
            if (!_running || gameManager == null)
                return;

            gameManager.Tick();
        }

        public void RequestEarlyExit()
        {
            CompleteSession();
        }

        public void NotifyRoundEndedFromUi()
        {
            CompleteSession();
        }

        void CompleteSession()
        {
            if (_completing)
                return;

            _running = false;
            StartCoroutine(CoCompleteSessionWithExit());
        }

        IEnumerator CoCompleteSessionWithExit()
        {
            _completing = true;

            MinigameSessionReport report = BuildSessionReport();

            if (exitScreenFader == null)
                Debug.LogError("[FlounderMinigameModule] exitScreenFader 를 Inspector에 연결하세요.", this);
            else
                exitScreenFader.SetInstant(0f);

            yield return MinigameExitSequence.Run(
                exitScreenFader,
                sessionEndHoldSeconds,
                exitFadeOutSeconds);

            _ctx.OnComplete?.Invoke(report);
        }

        MinigameSessionReport BuildSessionReport()
        {
            var report = new MinigameSessionReport(FlounderGameManager.SlotCount)
            {
                MinigameId = BuiltInId
            };

            if (gameManager == null)
                return report;

            var participated = new bool[FlounderGameManager.SlotCount];
            for (var i = 0; i < FlounderGameManager.SlotCount; i++)
            {
                FlounderPlayer p = gameManager.Players[i];
                participated[i] = p.Active;
                report.FinalScore[i] = p.Active ? Mathf.Max(0, p.FinalScore) : 0;
            }

            FlounderHpLossRules.FillHpLost(report.FinalScore, participated, report.HpLostThisSession);

            return report;
        }
    }
}
