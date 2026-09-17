using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        [Header("종료 → Result 씬")]
        [SerializeField] ScreenFader exitScreenFader;
        [SerializeField] float sessionEndHoldSeconds = 0.35f;
        [SerializeField] float exitFadeOutSeconds = 1f;

        void CompleteSession()
        {
            if (!_running || _completing)
                return;

            StartCoroutine(CoCompleteSessionWithExit());
        }

        IEnumerator CoCompleteSessionWithExit()
        {
            _completing = true;
            _running = false;
            StopSessionAudio();

            MinigameSessionReport report = BuildSessionReport();

            if (exitScreenFader == null)
                Debug.LogError("[RhythmButtonChallengeMinigameModule] exitScreenFader 를 Inspector에 연결하세요.", this);
            else
                exitScreenFader.SetInstant(0f);

            yield return MinigameExitSequence.Run(
                exitScreenFader,
                sessionEndHoldSeconds,
                exitFadeOutSeconds);

            gameObject.SetActive(false);
            _ctx.OnComplete?.Invoke(report);
        }

        MinigameSessionReport BuildSessionReport()
        {
            var report = new MinigameSessionReport(SlotCount)
            {
                MinigameId = BuiltInId
            };

            var participated = new bool[SlotCount];
            ForEachSlot(i =>
            {
                participated[i] = _aliveMask[i];
                report.FinalScore[i] = !_ctx.IsPractice ? _slots[i].ScoreSum : 0;
            });

            if (!_ctx.IsPractice)
            {
                RhythmButtonChallengeHpLossRules.FillHpLost(
                    report.FinalScore,
                    participated,
                    report.HpLostThisSession,
                    hpLowScoreThreshold);
            }

            return report;
        }
    }
}
