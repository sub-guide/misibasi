using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void CaptureSlotCameraRestIfNeeded(int i, Camera cam)
        {
            if (cam == null || _camRestCaptured[i])
                return;

            _camRestFov[i] = cam.fieldOfView;
            Vector3 euler = cam.transform.localEulerAngles;
            _camRestEulerX[i] = NormalizeSignedEuler(euler.x);
            _camRestEulerY[i] = NormalizeSignedEuler(euler.y);
            _camRestCaptured[i] = true;
        }

        void ResetSlotCameraFx(int i, Camera cam)
        {
            if (_slots == null)
                return;

            ref SlotRuntime sr = ref _slots[i];
            sr.CamFovBlend = 0f;
            sr.CamZ = 0f;
            sr.CamZVelocity = 0f;
            ApplySlotCameraPose(i, cam, ref sr);
        }

        void TickCenterBalanceCameraFx(int i, ref SlotRuntime sr, float dt)
        {
            if (dt <= 0f)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            Camera cam = ResolveSlotCamera(bind);
            if (cam == null)
                return;

            if (!_camRestCaptured[i])
                CaptureSlotCameraRestIfNeeded(i, cam);

            bool active = IsCenterBalanceActive(i, ref sr);
            TickCamFovBlend(ref sr, dt, active);
            TickCamZ(bind, ref sr, dt, active);
            ApplySlotCameraPose(i, cam, ref sr);
        }

        void TickCamFovBlend(ref SlotRuntime sr, float dt, bool active)
        {
            float inDur = Mathf.Max(camZoomInDuration, 0.0001f);
            float outMul = Mathf.Max(camZoomOutSpeedMul, 0.0001f);
            float outDur = inDur / outMul;
            if (active)
                sr.CamFovBlend = Mathf.Min(1f, sr.CamFovBlend + dt / inDur);
            else
                sr.CamFovBlend = Mathf.Max(0f, sr.CamFovBlend - dt / outDur);
        }

        void TickCamZ(CoffinDanceSlotBindings bind, ref SlotRuntime sr, float dt, bool active)
        {
            if (active)
            {
                float coffinZ = 0f;
                CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
                if (body != null)
                    coffinZ = FoldCoffinZForCamera(body.GetTiltZDegrees());

                StepCamZSqrtFollow(ref sr, coffinZ * centerCamTiltRatio, dt);
            }
            else
            {
                StepCamZReturnSpring(ref sr, dt);
            }
        }

        void StepCamZSqrtFollow(ref SlotRuntime sr, float targetZ, float dt)
        {
            float err = targetZ - sr.CamZ;
            float absErr = Mathf.Abs(err);
            if (absErr <= 0.0001f)
            {
                sr.CamZ = targetZ;
                sr.CamZVelocity = 0f;
                return;
            }

            float gain = Mathf.Max(camTiltFollowGain, 0f);
            float step = gain * Mathf.Sqrt(absErr) * dt;
            if (step >= absErr)
            {
                sr.CamZVelocity = err / dt;
                sr.CamZ = targetZ;
            }
            else
            {
                float dz = Mathf.Sign(err) * step;
                sr.CamZ += dz;
                sr.CamZVelocity = dz / dt;
            }
        }

        void StepCamZReturnSpring(ref SlotRuntime sr, float dt)
        {
            float hz = Mathf.Max(camReturnSpringHz, 0.01f);
            float zeta = Mathf.Max(camReturnSpringDamping, 0f);
            float omega = 2f * Mathf.PI * hz;
            float f = 1f + 2f * dt * zeta * omega;
            float oo = omega * omega;
            float hoo = dt * oo;
            float hhoo = dt * hoo;
            float detInv = 1f / (f + hhoo);
            float x = sr.CamZ;
            float v = sr.CamZVelocity;
            sr.CamZ = (f * x + dt * v) * detInv;
            sr.CamZVelocity = (v + hoo * -x) * detInv;
        }

        void ApplySlotCameraPose(int i, Camera cam, ref SlotRuntime sr)
        {
            if (cam == null || !_camRestCaptured[i])
                return;

            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(sr.CamFovBlend));
            cam.fieldOfView = Mathf.Lerp(_camRestFov[i], _camRestFov[i] * centerFovMultiplier, eased);
            cam.transform.localRotation = Quaternion.Euler(
                _camRestEulerX[i],
                _camRestEulerY[i],
                sr.CamZ);
        }

        /// <summary>
        /// 카메라 전용. 뒤집힌 관(|Z|&gt;90)은 180°를 접어 -90~+90 기울기만 쓴다.
        /// 예: -170 → +10, +180 → 0. 점수·물리는 오일러 원본을 유지.
        /// </summary>
        static float FoldCoffinZForCamera(float zDegrees)
        {
            float z = NormalizeSignedEuler(zDegrees);
            if (z > 90f)
                z -= 180f;
            else if (z < -90f)
                z += 180f;
            return z;
        }

        static float NormalizeSignedEuler(float degrees)
        {
            return Mathf.Repeat(degrees + 180f, 360f) - 180f;
        }
    }
}
