using System.Collections;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const string DefaultExitFadeOverlayName = "FadeOverlay";

        [Header("종료 → Result 씬")]
        [Tooltip("비우면 씬에서 `FadeOverlay` 를 찾는다.")]
        [SerializeField] ScreenFader exitScreenFader;

        [SerializeField] float sessionEndHoldSeconds = 0.35f;

        [SerializeField] float exitFadeOutSeconds = 1f;

        [SerializeField] AudioClip sessionEndClip;

        bool _completing;

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

            StopTierBgm();
            StopFeverScreamAudio();
            PlaySessionEndSfx();
            HideMainTimerImmediate();

            ForEachSlot(i => FlushUi(i));

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
                report.FinalScore[i] = !_ctx.IsPractice ? Mathf.Max(0, _slots[i].ScoreSum) : 0;
            });

            if (!_ctx.IsPractice)
                OiiaHpLossRules.FillHpLost(
                    report.FinalScore,
                    participated,
                    report.HpLostThisSession,
                    HpLowScoreThreshold);

            return report;
        }

        void PlaySessionEndSfx()
        {
            if (sfxSource == null || sessionEndClip == null)
                return;

            sfxSource.PlayOneShot(sessionEndClip);
        }

        void HideMainTimerImmediate()
        {
            if (mainRoundTimerCentralTop != null)
                mainRoundTimerCentralTop.gameObject.SetActive(false);
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
