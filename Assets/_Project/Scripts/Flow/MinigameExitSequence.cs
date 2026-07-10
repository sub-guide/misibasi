using System;
using System.Collections;
using UnityEngine;

namespace MiniParty.Flow
{
    /// <summary>미니게임 종료 시 짧은 대기 후 전체 화면 Fade Out.</summary>
    public static class MinigameExitSequence
    {
        public static IEnumerator Run(
            ScreenFader fader,
            float holdSeconds,
            float fadeOutSeconds,
            Action onBeforeFade = null,
            Action onAfterFade = null)
        {
            onBeforeFade?.Invoke();

            if (holdSeconds > 0f)
                yield return new WaitForSecondsRealtime(holdSeconds);

            if (fader != null && fadeOutSeconds > 0f)
                yield return fader.FadeTo(1f, fadeOutSeconds);

            onAfterFade?.Invoke();
        }
    }
}
