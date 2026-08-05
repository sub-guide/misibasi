using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        /// <summary>
        /// A안: x_bias(입력 Hold) + DanceWave×NoiseAmp → x_target,
        /// x_current = MoveTowards(x_target, MaxSpeed). Y_L=x · Y_R=1-x.
        /// </summary>
        void StepShoulderControl(ref SlotRuntime sr, float dt, float leftHeld, float rightHeld)
        {
            float maxSpeed = Mathf.Max(0f, maxNoiseSpeed);

            // ← = Y_L↑ (x→1) · → = Y_R↑ (x→0). 키를 떼면 bias Hold (중립 복귀 없음).
            if (leftHeld > 0.5f)
                sr.SeesawBias = Mathf.MoveTowards(sr.SeesawBias, MaxExtension, maxSpeed * dt);
            else if (rightHeld > 0.5f)
                sr.SeesawBias = Mathf.MoveTowards(sr.SeesawBias, MinExtension, maxSpeed * dt);

            float danceWave = ComputeDanceWave();
            float xTarget = Mathf.Clamp01(sr.SeesawBias + danceWave * noiseAmp);
            sr.SeesawXCurrent = Mathf.MoveTowards(sr.SeesawXCurrent, xTarget, maxSpeed * dt);
            sr.SeesawXCurrent = Mathf.Clamp01(sr.SeesawXCurrent);
        }

        float ComputeDanceWave()
        {
            return Mathf.Sin(Time.time * danceSineHz * Mathf.PI * 2f);
        }

        void ApplyPallbearerPoses(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            float jumpT = 0f;
            if (sr.JumpActive && jumpLockoutSeconds > 0.01f)
                jumpT = Mathf.Clamp01(sr.JumpElapsed / jumpLockoutSeconds);

            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            bind.ApplySideExtension(leftSide: true, x, jumpT);
            bind.ApplySideExtension(leftSide: false, 1f - x, jumpT);
        }

        void ApplyPresentationYaw(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind?.TiltRoot == null)
                return;

            bind.TiltRoot.localRotation = Quaternion.Euler(0f, presentationYawDegrees, 0f);
        }

        float GetSlotTiltDegrees(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null)
                return 0f;

            return body.GetTiltZDegrees();
        }

        bool CheckFailFloorAndMaybeEliminate(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null || !body.HasTouchedFailFloor)
                return false;

            body.ClearFailFloorContact();

            if (_ctx.IsPractice)
            {
                SoftResetSlot(i, ref sr);
                return false;
            }

            EliminateSlot(i, ref sr);
            return true;
        }

        void SoftResetSlot(int i, ref SlotRuntime sr)
        {
            float sign = (_rng != null && _rng.Next(0, 2) == 0) ? -1f : 1f;
            ResetSeesawToNeutral(ref sr);
            sr.JumpActive = false;
            sr.JumpElapsed = 0f;
            sr.JumpLockoutRemain = 0f;

            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            bind.PrepareAllPoses();
            ApplyPresentationYaw(i);
            ApplyPallbearerPoses(i, ref sr);

            CoffinDanceCoffinBody body = bind.ResolveCoffinBody();
            if (body != null)
            {
                body.SetSimulationActive(false);
                body.SoftReset(sign * initialTiltDegrees * 0.5f, -sign * initialAngularSpeed * 0.5f);
                body.SetSimulationActive(true);
            }
        }

        void ResetSeesawToNeutral(ref SlotRuntime sr)
        {
            float n = Mathf.Clamp01(xSeesawNeutral);
            sr.SeesawBias = n;
            sr.SeesawXCurrent = n;
        }

        void ApplyLandingImpulse(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            bind?.ResolveCoffinBody()?.ApplyLandingImpulse();
        }
    }
}
