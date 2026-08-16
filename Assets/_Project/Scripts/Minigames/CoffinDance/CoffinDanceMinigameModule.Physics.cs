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

            // 미입력 또는 좌/우 동시 입력 → 홀드 리셋 + 현재 기울기 방향 중력형 미세 도치
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

        float GetSlotTiltDegrees(int i)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null)
                return 0f;

            return body.GetTiltZDegrees();
        }

        void HandleFailFloorContact(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            CoffinDanceCoffinBody body = bind != null ? bind.ResolveCoffinBody() : null;
            if (body == null || !body.HasTouchedFailFloor)
                return;

            body.ClearFailFloorContact();
            body.ApplyUpwardImpulse(Mathf.Max(0f, failFloorUpwardImpulse));
            BeginShoulderIgnore(i, ref sr);

            if (_ctx.IsPractice)
                return;

            sr.ScoreExact = Mathf.Max(0f, sr.ScoreExact - Mathf.Max(0, failFloorPenaltyScore));
            sr.ScoreSum = Mathf.FloorToInt(sr.ScoreExact);
        }

        void BeginShoulderIgnore(int i, ref SlotRuntime sr)
        {
            float dur = Mathf.Max(0f, failFloorShoulderIgnoreSeconds);
            if (dur <= 0f)
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            bind?.SetCoffinShoulderCollisionsIgnored(true);
            sr.ShoulderIgnoreRemain = dur;
        }

        void TickShoulderIgnore(int i, ref SlotRuntime sr, float dt)
        {
            if (sr.ShoulderIgnoreRemain <= 0f)
                return;

            sr.ShoulderIgnoreRemain -= dt;
            if (sr.ShoulderIgnoreRemain > 0f)
                return;

            sr.ShoulderIgnoreRemain = 0f;
            GetBindings(i)?.SetCoffinShoulderCollisionsIgnored(false);
        }

        void ResetSeesawToNeutral(ref SlotRuntime sr)
        {
            float n = Mathf.Clamp01(xSeesawNeutral);
            sr.SeesawBias = n;
            sr.SeesawXCurrent = n;
            sr.HoldTimer = 0f;
        }
    }
}
