using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        void ResetRandomSfx()
        {
            _randomSfxWait = NextRandomSfxGap();
        }

        void TickRandomSfx(float dt)
        {
            if (peckSfxSource == null || randomSfxClip == null)
                return;

            _randomSfxWait -= dt;
            if (_randomSfxWait > 0f)
                return;

            peckSfxSource.PlayOneShot(randomSfxClip);
            _randomSfxWait = Mathf.Max(0f, randomSfxClip.length) + NextRandomSfxGap();
        }

        float NextRandomSfxGap()
        {
            float min = randomSfxIntervalMinSeconds;
            float max = randomSfxIntervalMaxSeconds;
            if (max < min)
                (min, max) = (max, min);
            min = Mathf.Max(0f, min);
            max = Mathf.Max(min, max);
            return Random.Range(min, max);
        }
    }
}
