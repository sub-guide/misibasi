using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
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
            float duration = Mathf.Max(0f, roundDuration);
            if (duration > 0f)
                _roundElapsed = duration;
            RefreshRoundTimer();

            for (var i = 0; i < SlotCount; i++)
            {
                if (_peckPhase[i] != PeckPhase.Idle)
                    EndPeck(i);
            }

            MinigameSessionReport report = BuildSessionReport();

            if (exitScreenFader == null)
                Debug.LogError("[PigeonMinigameModule] exitScreenFader 를 Inspector에 연결하세요.", this);
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

            for (var i = 0; i < SlotCount; i++)
            {
                report.FinalScore[i] = _participatedMask[i] ? Mathf.Max(0, _score[i]) : 0;
            }

            PigeonHpLossRules.FillHpLost(report.FinalScore, _participatedMask, report.HpLostThisSession);

            return report;
        }
    }
}
