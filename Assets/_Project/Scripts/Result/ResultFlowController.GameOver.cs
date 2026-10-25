using System.Collections;
using MiniParty.Core;
using UnityEngine;

namespace MiniParty.Result
{
    public sealed partial class ResultFlowController
    {
        [Header("GAME OVER (4단계)")]
        [Tooltip("카드 뒤집기 후 Fade 알파 0→목표까지 시간(초).")]
        [SerializeField] float gameOverFadeSeconds = 0.5f;

        [Tooltip("Fade 목표 알파 0~255.")]
        [SerializeField] int gameOverFadeAlpha = 250;

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

            float fadeSeconds = Mathf.Max(0f, gameOverFadeSeconds);
            float targetAlpha = Mathf.Clamp(gameOverFadeAlpha, 0, 255) / 255f;
            int remaining = 0;

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                if (!_willGameOver[i] && models[i].HP > 0)
                    continue;

                models[i].EnterGameOver();

                ResultSlotView view = GetSlotView(i);
                if (view == null)
                    continue;

                remaining++;
                StartCoroutine(CoPlayGameOverView(view, fadeSeconds, targetAlpha, () => remaining--));
            }

            while (remaining > 0)
                yield return null;

            Debug.Log("[ResultFlowController] 4단계 GameOver 완료.");
        }

        IEnumerator CoPlayGameOverView(
            ResultSlotView view,
            float fadeSeconds,
            float targetAlpha,
            System.Action onDone)
        {
            yield return view.PlayGameOver(fadeSeconds, targetAlpha);
            onDone?.Invoke();
        }
    }
}
