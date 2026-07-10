using System.Collections;
using MiniParty.Core;
using UnityEngine;

namespace MiniParty.Result
{
    public sealed partial class ResultFlowController
    {
        [Header("GAME OVER (4단계)")]
        [SerializeField] float gameOverDelaySeconds = 2f;

        IEnumerator CoGameOverSequence()
        {
            if (_practice || _session?.Slots == null)
                yield break;

            PlayerSlotModel[] models = _session.Slots;
            bool any = false;

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                if (!_willGameOver[i] && models[i].HP > 0)
                    continue;

                any = true;
                break;
            }

            if (!any)
                yield break;

            _phase = ResultPhase.GameOver;

            if (gameOverDelaySeconds > 0f)
                yield return new WaitForSecondsRealtime(gameOverDelaySeconds);

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                if (!_willGameOver[i] && models[i].HP > 0)
                    continue;

                models[i].EnterGameOver();

                ResultSlotView view = GetSlotView(i);
                view?.SetDimmed(true);
                view?.ShowGameOver(i);
            }

            Debug.Log("[ResultFlowController] 4단계 GameOver 완료.");
        }
    }
}
