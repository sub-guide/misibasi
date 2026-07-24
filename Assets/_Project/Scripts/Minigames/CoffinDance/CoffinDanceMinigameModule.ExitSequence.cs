using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        const string DefaultExitFadeOverlayName = "FadeOverlay";

        [Header("종료 → Result 씬")]
        [SerializeField] ScreenFader exitScreenFader;
        [SerializeField] float sessionEndHoldSeconds = 0.35f;
        [SerializeField] float exitFadeOutSeconds = 1f;
        [SerializeField] AudioClip sessionEndClip;
        [SerializeField] AudioSource sfxSource;

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
            _flowState = CdFlowState.Complete;

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
                participated[i] = _participatedMask[i];
                report.FinalScore[i] = !_ctx.IsPractice && _participatedMask[i]
                    ? Mathf.Max(0, _slots[i].ScoreSum)
                    : 0;
            });

            if (!_ctx.IsPractice)
                CoffinDanceHpLossRules.FillHpLost(
                    report.FinalScore,
                    participated,
                    report.HpLostThisSession,
                    hpLowScoreThreshold);

            return report;
        }

        void PlaySessionEndSfx()
        {
            if (sfxSource == null || sessionEndClip == null)
                return;

            sfxSource.PlayOneShot(sessionEndClip);
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
