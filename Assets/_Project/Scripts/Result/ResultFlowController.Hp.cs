using System.Collections;
using MiniParty.Core;
using UnityEngine;

namespace MiniParty.Result
{
    public sealed partial class ResultFlowController
    {
        [Header("HP 연출 (3단계)")]
        [SerializeField] float loserHitDurationSeconds = 0.45f;

        [SerializeField] float spotlightDurationSeconds = 0.55f;

        [SerializeField] float hpStepDelaySeconds = 0.12f;

        [SerializeField] AudioClip hpHitClip;

        readonly bool[] _willGameOver = new bool[4];

        IEnumerator CoHpProcess()
        {
            _phase = ResultPhase.HpProcess;

            if (_practice || _session?.Slots == null)
            {
                yield break;
            }

            int maxHp = _session.StartingHp;
            PlayerSlotModel[] models = _session.Slots;

            InitHpDisplays(maxHp, models);

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                bool lost = _report?.HpLostThisSession != null &&
                            i < _report.HpLostThisSession.Length &&
                            _report.HpLostThisSession[i];

                if (!lost)
                    continue;

                ResultSlotView view = GetSlotView(i);
                if (view == null)
                    continue;

                int hpBefore = models[i].HP;
                int hpAfter = Mathf.Max(0, hpBefore - 1);

                PlayHpHitSfx();
                yield return view.PlayLoserHit(hpBefore, hpAfter, maxHp, loserHitDurationSeconds);

                models[i].ApplyHpDelta(-1);
                _willGameOver[i] = models[i].HP <= 0;

                if (hpStepDelaySeconds > 0f)
                    yield return new WaitForSecondsRealtime(hpStepDelaySeconds);
            }

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                bool lost = _report?.HpLostThisSession != null &&
                            i < _report.HpLostThisSession.Length &&
                            _report.HpLostThisSession[i];

                if (lost)
                    continue;

                ResultSlotView view = GetSlotView(i);
                if (view == null)
                    continue;

                yield return view.PlayWinnerSpotlight(spotlightDurationSeconds);
            }

            Debug.Log("[ResultFlowController] 3단계 HpProcess 완료.");
        }

        void InitHpDisplays(int maxHp, PlayerSlotModel[] models)
        {
            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                GetSlotView(i)?.SetHp(models[i].HP, maxHp);
            }
        }

        void PlayHpHitSfx()
        {
            if (sfxSource == null || hpHitClip == null)
                return;

            sfxSource.PlayOneShot(hpHitClip);
        }

        ResultSlotView GetSlotView(int index) =>
            slotViews != null && index >= 0 && index < slotViews.Length ? slotViews[index] : null;
    }
}
