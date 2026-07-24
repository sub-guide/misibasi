using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void StepPhysics(ref SlotRuntime sr, float dt, float leftHeld, float rightHeld)
        {
            float gMul = gravityTorque * _gravityMul;
            float alpha = gMul * Mathf.Sin(sr.ThetaRad);

            float control = (rightHeld - leftHeld) * controlTorque * _inertiaMul;
            alpha += control;

            if (_externalForceAmp > 0f)
            {
                float noise = (Mathf.PerlinNoise(Time.time * 1.7f + sr.ThetaRad, 0.37f) - 0.5f) * 2f;
                alpha += noise * _externalForceAmp;
            }

            sr.Omega += alpha * dt;
            sr.Omega *= Mathf.Clamp01(1f - rotationalDamping * dt);
            float maxW = maxAngularSpeed * _inertiaMul;
            sr.Omega = Mathf.Clamp(sr.Omega, -maxW, maxW);
            sr.ThetaRad += sr.Omega * dt;
        }

        bool CheckStumbleAndMaybeEliminate(int i, ref SlotRuntime sr, float dt)
        {
            float limit = StumbleLimitDegrees * Mathf.Deg2Rad;
            float abs = Mathf.Abs(sr.ThetaRad);

            if (abs < limit)
            {
                sr.InStumble = false;
                sr.StumbleTimer = 0f;
                return false;
            }

            if (!sr.InStumble)
            {
                sr.InStumble = true;
                sr.StumbleTimer = StumbleBufferSeconds;
            }

            sr.StumbleTimer -= dt;
            if (sr.StumbleTimer > 0f)
                return false;

            // 연습: 탈락 대신 기울기 소프트 리셋 (본게임만 ELIMINATED)
            if (_ctx.IsPractice)
            {
                SoftResetTilt(ref sr);
                return false;
            }

            EliminateSlot(i, ref sr);
            return true;
        }

        void ApplyLandingImpulse(ref SlotRuntime sr)
        {
            float sign = sr.Omega >= 0f ? 1f : -1f;
            if (Mathf.Abs(sr.Omega) < 0.05f)
                sign = sr.ThetaRad >= 0f ? 1f : -1f;

            sr.Omega += sign * jumpLandingTorqueImpulse;
        }

        void ApplyFailTiltImpulse(ref SlotRuntime sr)
        {
            float sign = sr.ThetaRad >= 0f ? 1f : -1f;
            if (Mathf.Abs(sr.ThetaRad) < 0.02f)
                sign = (_rng != null && _rng.Next(0, 2) == 0) ? -1f : 1f;

            sr.Omega += sign * jumpFailTiltImpulse;
            sr.ThetaRad += sign * 12f * Mathf.Deg2Rad;
        }

        void ApplyTiltVisual(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null || bind.TiltRoot == null)
                return;

            float deg = sr.ThetaRad * Mathf.Rad2Deg;
            Vector3 e = bind.TiltRoot.localEulerAngles;
            e.z = deg;
            e.y = presentationYawDegrees;
            bind.TiltRoot.localEulerAngles = e;

            if (sr.JumpLockoutRemain > 0f)
            {
                float t = 1f - Mathf.Clamp01(sr.JumpLockoutRemain / Mathf.Max(0.01f, jumpLockoutSeconds));
                float hop = Mathf.Sin(t * Mathf.PI) * 0.35f;
                if (sr.HasCoffinBase)
                    bind.TiltRoot.localPosition = sr.CoffinBaseLocalPos + Vector3.up * hop;
            }
            else if (sr.HasCoffinBase)
            {
                bind.TiltRoot.localPosition = sr.CoffinBaseLocalPos;
            }
        }
    }
}
