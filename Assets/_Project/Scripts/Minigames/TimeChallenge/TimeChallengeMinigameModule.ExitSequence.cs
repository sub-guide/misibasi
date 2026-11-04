using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using MiniParty.Result;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
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
            StopPerfectHitVideos();

            if (exitScreenFader != null)
                exitScreenFader.SetInstant(0f);

            yield return MinigameExitSequence.Run(
                exitScreenFader,
                sessionEndHoldSeconds,
                exitFadeOutSeconds);

            MinigameSessionReport report = BuildSessionReport();
            StopGameplayAudio();
            gameObject.SetActive(false);
            _ctx.OnComplete?.Invoke(report);
        }

        MinigameSessionReport BuildSessionReport()
        {
            var report = new MinigameSessionReport(SlotCount)
            {
                MinigameId = BuiltInId
            };

            for (var i = 0; i < SlotCount; i++)
            {
                report.FinalScore[i] = _participatedMask[i] ? _totalScores[i] : 0;
                report.HpLostThisSession[i] = false;
            }

            ResultRankingUtility.FillRanks(report.FinalScore, _participatedMask, report.Rank);
            TimeChallengeHpLossRules.FillHpLost(
                report.FinalScore,
                _participatedMask,
                report.HpLostThisSession);
            return report;
        }
    }
}
