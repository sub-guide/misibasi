using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        const string DefaultExitFadeOverlayName = "FadeOverlay";

        [Header("종료 → Result 씬")]
        [SerializeField] ScreenFader exitScreenFader;
        [SerializeField] float sessionEndHoldSeconds = 0.35f;
        [SerializeField] float exitFadeOutSeconds = 1f;
        [SerializeField] AudioClip sessionEndClip;

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
            _flowState = RbcFlowState.Complete;

            if (musicSource != null)
                musicSource.Stop();

            PlaySessionEndSfx();
            FlushAllUi();

            MinigameSessionReport report = BuildSessionReport();

            ScreenFader fader = ResolveExitScreenFader();
            fader?.SetInstant(0f);

            yield return MinigameExitSequence.Run(
                fader,
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
                RhythmButtonChallengeHpLossRules.FillHpLost(report.FinalScore, participated, report.HpLostThisSession);

            return report;
        }

        void PlaySessionEndSfx()
        {
            if (musicSource == null || sessionEndClip == null)
                return;

            musicSource.PlayOneShot(sessionEndClip);
        }

        ScreenFader ResolveExitScreenFader()
        {
            if (exitScreenFader != null)
                return exitScreenFader;

            var overlay = GameObject.Find(DefaultExitFadeOverlayName);
            return overlay != null ? overlay.GetComponent<ScreenFader>() : null;
        }
    }
}
