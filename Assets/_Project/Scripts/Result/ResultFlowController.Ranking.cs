using System.Collections;
using System.Collections.Generic;
using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Result
{
    public sealed partial class ResultFlowController
    {
        [Header("등수 공개 (2단계)")]
        [SerializeField] float rankingRevealIntervalSeconds = 1f;

        [SerializeField] AudioSource sfxSource;

        [SerializeField] AudioClip rankRevealClip;

        List<ResultRankingUtility.RankRevealGroup> _rankRevealGroups;

        IEnumerator CoRankingReveal()
        {
            _phase = ResultPhase.RankingReveal;

            if (_practice)
            {
                ApplyPracticeRankingSkip();
                _phase = ResultPhase.HpProcess;
                yield break;
            }

            EnsureRanksComputed();
            _rankRevealGroups = ResultRankingUtility.BuildRevealGroups(_report.Rank, _playedMask);

            if (_rankRevealGroups == null || _rankRevealGroups.Count == 0)
            {
                Debug.LogWarning("[ResultFlowController] 공개할 등수 그룹이 없습니다.", this);
                yield break;
            }

            for (var g = 0; g < _rankRevealGroups.Count; g++)
            {
                ResultRankingUtility.RankRevealGroup group = _rankRevealGroups[g];
                RevealGroup(group);
                PlayRankRevealSfx();

                bool isLast = g >= _rankRevealGroups.Count - 1;
                if (!isLast && rankingRevealIntervalSeconds > 0f)
                    yield return new WaitForSecondsRealtime(rankingRevealIntervalSeconds);
            }

            _phase = ResultPhase.HpProcess;
            Debug.Log("[ResultFlowController] 2단계 RankingReveal 완료. (3단계 HpProcess 예정)");
        }

        void EnsureRanksComputed()
        {
            if (_report?.Rank == null || _report.FinalScore == null)
                return;

            bool anyRank = false;
            for (var i = 0; i < _report.Rank.Length; i++)
            {
                if (_playedMask[i] && _report.Rank[i] > 0)
                {
                    anyRank = true;
                    break;
                }
            }

            if (!anyRank)
                ResultRankingUtility.FillRanks(_report.FinalScore, _playedMask, _report.Rank);
        }

        void RevealGroup(ResultRankingUtility.RankRevealGroup group)
        {
            if (group?.SlotIndices == null || slotViews == null)
                return;

            for (var i = 0; i < group.SlotIndices.Length; i++)
            {
                int slot = group.SlotIndices[i];
                if (slot < 0 || slot >= slotViews.Length)
                    continue;

                ResultSlotView view = slotViews[slot];
                if (view == null)
                    continue;

                int score = _report?.FinalScore != null && slot < _report.FinalScore.Length
                    ? _report.FinalScore[slot]
                    : 0;

                view.RevealRanking(group.Rank, score, practice: false);
            }
        }

        void ApplyPracticeRankingSkip()
        {
            if (slotViews == null)
                return;

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                ResultSlotView view = i < slotViews.Length ? slotViews[i] : null;
                view?.RevealRanking(0, 0, practice: true);
            }
        }

        void PlayRankRevealSfx()
        {
            if (sfxSource == null || rankRevealClip == null)
                return;

            sfxSource.PlayOneShot(rankRevealClip);
        }

        /// <summary>Ready 전까지 START·운영자 Enter 무시.</summary>
        bool IsPlayerAndOperatorInputLocked() =>
            _phase is not ResultPhase.Ready;

        void DrainLockedInputThisFrame()
        {
            if (!IsPlayerAndOperatorInputLocked())
                return;

            for (var i = 0; i < 4; i++)
                _ = SlotGamepad.StartPressed(i);

            _ = _operatorInput.Confirm;
        }
    }
}
