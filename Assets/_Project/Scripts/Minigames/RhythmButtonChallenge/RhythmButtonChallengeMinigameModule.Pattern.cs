using System;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        static readonly RbcButton[][] StageButtonPools =
        {
            Array.Empty<RbcButton>(),
            new[] { RbcButton.A, RbcButton.B },
            new[] { RbcButton.A, RbcButton.B, RbcButton.X, RbcButton.Y },
            new[] { RbcButton.A, RbcButton.B, RbcButton.X, RbcButton.Y, RbcButton.Lb, RbcButton.Rb },
            new[]
            {
                RbcButton.A, RbcButton.B, RbcButton.X, RbcButton.Y, RbcButton.Lb, RbcButton.Rb,
                RbcButton.Up, RbcButton.Down, RbcButton.Left, RbcButton.Right
            },
            new[]
            {
                RbcButton.A, RbcButton.B, RbcButton.X, RbcButton.Y, RbcButton.Lb, RbcButton.Rb,
                RbcButton.Up, RbcButton.Down, RbcButton.Left, RbcButton.Right
            }
        };

        void GeneratePatternForStage(int stageIndex)
        {
            RbcButton[] pool = StageButtonPools[Mathf.Clamp(stageIndex, 1, StagesPerPhase)];
            var rng = new System.Random(ComputePatternSeed(stageIndex));

            for (var i = 0; i < BeatsPerSegment; i++)
            {
                RbcButton pick;
                var attempts = 0;
                do
                {
                    pick = pool[rng.Next(pool.Length)];
                    attempts++;
                } while (WouldViolateConsecutiveRule(pick, i) && attempts < 32);

                _currentPattern[i] = pick;
            }
        }

        int ComputePatternSeed(int stageIndex) =>
            unchecked(_sessionSeed * 397 ^ _phaseNumber * 17 ^ stageIndex);

        bool WouldViolateConsecutiveRule(RbcButton candidate, int beatIndex)
        {
            if (beatIndex < 2)
                return false;

            RbcButton a = _currentPattern[beatIndex - 1];
            RbcButton b = _currentPattern[beatIndex - 2];
            return candidate == a && a == b;
        }
    }
}
