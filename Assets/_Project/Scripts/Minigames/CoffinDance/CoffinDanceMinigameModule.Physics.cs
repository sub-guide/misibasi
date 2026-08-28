using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        /// <summary>
        /// A안: x_bias(홀드 가속·미세 도치·비선형 풀) + DanceWave×noiseAmp → x 즉시 반영.
        /// Y_L=x · Y_R=1-x. Rate Limiter 없음(Sine이 이미 부드러움).
        /// </summary>
        void StepShoulderControl(ref SlotRuntime sr, float dt, float leftHeld, float rightHeld)
        {
            bool left = leftHeld > 0.5f;
            bool right = rightHeld > 0.5f;
            float danceWave = ComputeDanceWave();
            float effectiveDriftSpeed = Mathf.Max(0f, microDriftSpeed);

            // 미입력 또는 LB/RB 동시 입력 → 홀드 리셋 + 현재 기울기 방향 중력형 미세 도치
            if ((!left && !right) || (left && right))
            {
                sr.HoldTimer = 0f;
                // 중앙(0.5)보다 오른쪽(+)/왼쪽(-)으로 기운 쪽을 계속 밀어 불안정 평형
                float driftDir = Mathf.Sign(sr.SeesawBias - 0.5f);
                sr.SeesawBias += driftDir * effectiveDriftSpeed * dt;
            }
            else
            {
                // ← = Y_L↑ (x→1, +1) · → = Y_R↑ (x→0, -1). 홀드 누적 가속.
                sr.HoldTimer += dt;
                float accelT = Mathf.Max(0.0001f, holdAccelTime);
                float speedMul = Mathf.Lerp(1f, holdMaxMultiplier, Mathf.Clamp01(sr.HoldTimer / accelT));
                float inputDir = left ? 1f : -1f;
                sr.SeesawBias += inputDir * Mathf.Max(0f, seesawBaseSpeed) * speedMul * dt;
            }

            // 중앙(0.5) 이탈 비선형 가속 (공통)
            float offset = sr.SeesawBias - 0.5f;
            float pullForce = pullCoefficient * (offset * offset) * Mathf.Sign(offset);
            sr.SeesawBias += pullForce * dt;

            sr.SeesawBias = Mathf.Clamp01(sr.SeesawBias);
            sr.SeesawXCurrent = Mathf.Clamp01(sr.SeesawBias + danceWave * noiseAmp);
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

            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            bind.ApplySideExtension(leftSide: true, x);
            bind.ApplySideExtension(leftSide: false, 1f - x);
        }

        void HandleFailFloorContact(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null || !body.HasTouchedFailFloor)
                return;

            body.ClearFailFloorContact();
            if (sr.FailFloorRecoverActive)
                return;

            BeginFailFloorRecover(i, ref sr, body);

            if (_ctx.IsPractice)
                return;

            sr.ScoreExact = Mathf.Max(0f, sr.ScoreExact - Mathf.Max(0, failFloorPenaltyScore));
            sr.ScoreSum = Mathf.FloorToInt(sr.ScoreExact);
        }

        void BeginFailFloorRecover(int i, ref SlotRuntime sr, CoffinDanceCoffinBody body)
        {
            if (body == null)
                return;

            float dur = Mathf.Max(0f, failFloorRecoverDuration);
            sr.FailFloorRecoverStartY = body.transform.localPosition.y;
            sr.FailFloorRecoverStartZ = body.GetTiltZDegrees();
            sr.FailFloorRecoverStartSeesaw = sr.SeesawBias;
            sr.FailFloorRecoverDuration = dur;
            sr.FailFloorRecoverElapsed = 0f;

            body.BeginKinematicHold();
            sr.CoffinShoulderAttached = false;
            sr.CoffinFallLockedUntilFloor = false;

            if (dur <= 0f)
            {
                ApplyFailFloorRecoverPose(body, 1f, ref sr);
                EndFailFloorRecover(i, ref sr, body);
                return;
            }

            CoffinDanceSlotBindings bind = GetBindings(i);
            bind?.SetCoffinShoulderCollisionsIgnored(true);
            body.ClearShoulderContacts();
            sr.FailFloorRecoverActive = true;
            sr.ShoulderIgnoreRemain = dur;
        }

        void TickFailFloorRecover(int i, ref SlotRuntime sr, float dt)
        {
            if (!sr.FailFloorRecoverActive)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null)
            {
                sr.FailFloorRecoverActive = false;
                sr.ShoulderIgnoreRemain = 0f;
                sr.CoffinFallLockedUntilFloor = false;
                return;
            }

            sr.FailFloorRecoverElapsed += Mathf.Max(0f, dt);
            float dur = sr.FailFloorRecoverDuration;
            float u = dur <= 0f ? 1f : Mathf.Clamp01(sr.FailFloorRecoverElapsed / dur);
            ApplyFailFloorRecoverPose(body, u, ref sr);
            sr.ShoulderIgnoreRemain = Mathf.Max(0f, dur - sr.FailFloorRecoverElapsed);

            if (u < 1f)
                return;

            EndFailFloorRecover(i, ref sr, body);
        }

        void ApplyFailFloorRecoverPose(CoffinDanceCoffinBody body, float linear01, ref SlotRuntime sr)
        {
            if (body == null)
                return;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(linear01));
            float y = Mathf.Lerp(sr.FailFloorRecoverStartY, failFloorRecoverLocalY, t);
            float z = Mathf.Lerp(sr.FailFloorRecoverStartZ, 0f, t);
            body.SetLocalYAndZDegrees(y, z);

            float seesaw = Mathf.Lerp(sr.FailFloorRecoverStartSeesaw, 0.5f, t);
            sr.SeesawBias = seesaw;
            sr.SeesawXCurrent = seesaw;
            sr.HoldTimer = 0f;
        }

        void EndFailFloorRecover(int i, ref SlotRuntime sr, CoffinDanceCoffinBody body)
        {
            ApplyFailFloorRecoverPose(body, 1f, ref sr);
            body?.EndKinematicHold();
            body?.ClearShoulderContacts();
            body?.ClearFailFloorContact();

            GetBindings(i)?.SetCoffinShoulderCollisionsIgnored(false);

            sr.FailFloorRecoverActive = false;
            sr.FailFloorRecoverElapsed = 0f;
            sr.FailFloorRecoverDuration = 0f;
            sr.ShoulderIgnoreRemain = 0f;
            sr.CoffinShoulderAttached = false;
            sr.CoffinFallLockedUntilFloor = false;
        }

        static bool IsSeesawBiasAtExtreme(float bias)
        {
            return bias <= 0f || bias >= 1f;
        }

        void UpdateExtremeSeesawShoulderColliders(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            if (!IsSeesawBiasAtExtreme(sr.SeesawBias))
            {
                bind.SetSideShoulderCollidersEnabled(leftSide: true, enabled: true);
                bind.SetSideShoulderCollidersEnabled(leftSide: false, enabled: true);
                return;
            }

            bool leftLow = sr.SeesawBias <= 0f;
            bind.SetSideShoulderCollidersEnabled(leftSide: true, enabled: !leftLow);
            bind.SetSideShoulderCollidersEnabled(leftSide: false, enabled: leftLow);
        }

        void ApplyShoulderDepenetration(int i, ref SlotRuntime sr)
        {
            if (sr.CoffinShoulderAttached)
                return;

            if (sr.CoffinFallLockedUntilFloor)
                return;

            if (IsSeesawBiasAtExtreme(sr.SeesawBias))
                return;

            if (sr.ShoulderIgnoreRemain > 0f)
                return;

            float maxY = Mathf.Max(0f, shoulderDepenetrationMaxY);
            if (maxY <= 0f)
                return;

            GetBindings(i)?.ApplyUpwardShoulderDepenetration(maxY);
        }

        void ResetSeesawToNeutral(ref SlotRuntime sr)
        {
            float n = Mathf.Clamp01(xSeesawNeutral);
            sr.SeesawBias = n;
            sr.SeesawXCurrent = n;
            sr.HoldTimer = 0f;
        }

        void UpdateCoffinShoulderAttach(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null)
            {
                sr.CoffinShoulderAttached = false;
                return;
            }

            if (sr.FailFloorRecoverActive)
                return;

            if (IsSeesawBiasAtExtreme(sr.SeesawBias))
            {
                if (sr.CoffinShoulderAttached)
                    BeginCoffinFallLock(i, ref sr, body);
                return;
            }

            if (sr.CoffinFallLockedUntilFloor)
                return;

            if (sr.CoffinShoulderAttached)
                return;

            if (!body.IsTouchingShoulder)
                return;

            body.BeginKinematicHold();
            sr.CoffinShoulderAttached = true;
            SnapAttachedCoffinToShoulders(i, ref sr);
        }

        void BeginCoffinFallLock(int i, ref SlotRuntime sr, CoffinDanceCoffinBody body)
        {
            sr.CoffinShoulderAttached = false;
            sr.CoffinFallLockedUntilFloor = true;
            body?.EndKinematicHold();
            GetBindings(i)?.SetCoffinShoulderCollisionsIgnored(true);
            body?.ClearShoulderContacts();
        }

        void SnapAttachedCoffinToShoulders(int i, ref SlotRuntime sr)
        {
            if (!sr.CoffinShoulderAttached || sr.FailFloorRecoverActive)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (bind == null || body == null)
                return;

            if (!bind.TryComputeCoffinSupportLocalPose(out float localY, out float zDegrees))
                return;

            body.SetLocalYAndZDegrees(localY, zDegrees);
        }
    }
}
