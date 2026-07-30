using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void StepShoulderControl(ref SlotRuntime sr, float dt, float leftHeld, float rightHeld)
        {
            float raise = shoulderRaiseSpeed * _shoulderSpeedMul;
            float ret = shoulderReturnSpeed * _shoulderSpeedMul;

            // ← = 좌측 들어올림 · → = 우측 들어올림 (반대쪽은 낮춤)
            // extension 범위: 0=앉음 · 1=기립 (오버슈트 없음)
            if (leftHeld > 0.5f)
            {
                sr.LeftExtension = Mathf.MoveTowards(sr.LeftExtension, MaxExtension, raise * dt);
                sr.RightExtension = Mathf.MoveTowards(sr.RightExtension, MinExtension, raise * dt);
            }
            else if (rightHeld > 0.5f)
            {
                sr.RightExtension = Mathf.MoveTowards(sr.RightExtension, MaxExtension, raise * dt);
                sr.LeftExtension = Mathf.MoveTowards(sr.LeftExtension, MinExtension, raise * dt);
            }
            else
            {
                sr.LeftExtension = Mathf.MoveTowards(sr.LeftExtension, neutralExtension, ret * dt);
                sr.RightExtension = Mathf.MoveTowards(sr.RightExtension, neutralExtension, ret * dt);
            }
        }

        void ApplyPallbearerPoses(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            float jumpT = 0f;
            if (sr.JumpActive && jumpLockoutSeconds > 0.01f)
                jumpT = Mathf.Clamp01(sr.JumpElapsed / jumpLockoutSeconds);

            bind.ApplySideExtension(leftSide: true, sr.LeftExtension, jumpT);
            bind.ApplySideExtension(leftSide: false, sr.RightExtension, jumpT);
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
            sr.LeftExtension = neutralExtension;
            sr.RightExtension = neutralExtension;
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

        void ApplyLandingImpulse(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            bind?.ResolveCoffinBody()?.ApplyLandingImpulse();
        }
    }
}
