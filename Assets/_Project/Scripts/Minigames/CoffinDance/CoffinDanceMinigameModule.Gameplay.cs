using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void TickSlotGameplay(int i, float dt)
        {
            ref SlotRuntime sr = ref _slots[i];

            ReadBalanceInput(i, out float left, out float right);
            if (!sr.FailFloorRecoverActive)
            {
                if (IsDevGodModeSlot(i) && !IsExclusiveShoulderInput(left, right))
                    StepDevGodIdleReturn(ref sr, dt);
                else
                    StepShoulderControl(ref sr, dt, left, right);
            }

            HandleFailFloorContact(i, ref sr);
            TickFailFloorRecover(i, ref sr, dt);
            ApplyPallbearerPoses(i, ref sr);
            UpdateExtremeSeesawShoulderColliders(i, ref sr);
            UpdateCoffinShoulderAttach(i, ref sr);
            AccruePassiveScore(i, ref sr, dt);
            ApplyShoulderDepenetration(i, ref sr);
            TickCenterBalanceCameraFx(i, ref sr, dt);
            ApplyCenterFocusFx(i, ref sr, GetBindings(i));
        }

        /// <summary>운영자 부여. 생존 1초분(100)만 더하고 정중앙 보너스는 포함하지 않는다.</summary>
        void TickOperatorScoreGrants()
        {
            if (_flowState != CdFlowState.Playing)
                return;

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_participatedMask[i] || !_aliveMask[i] || !OperatorScoreGrantInput.WasPressed(i))
                    continue;

                ref SlotRuntime sr = ref _slots[i];
                sr.ScoreExact += SurvivalScorePerSecond;
                sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
            }
        }

        void AccruePassiveScore(int i, ref SlotRuntime sr, float dt)
        {
            if (sr.FailFloorRecoverActive)
                return;

            float gain = SurvivalScorePerSecond * dt;
            if (IsCenterBalanceActive(i, ref sr))
                gain += centerBonusScorePerSec * dt;

            sr.ScoreExact += gain;
            sr.ScoreSum = Mathf.Max(0, Mathf.FloorToInt(sr.ScoreExact));
        }

        bool IsCenterBalanceActive(int i, ref SlotRuntime sr)
        {
            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            if (Mathf.Abs(x - 0.5f) > centerZoneThreshold)
                return false;

            if (sr.ShoulderIgnoreRemain > 0f)
                return false;

            return sr.CoffinShoulderAttached;
        }
    }
}
