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

            // 착지 직후 미세 도치 증폭 (타이머는 입력 유무와 무관하게 차감 · Edge Case A: 입력은 아래 분기로 즉시 가산)
            float effectiveDriftSpeed = Mathf.Max(0f, microDriftSpeed);
            if (sr.LandingDriftTimer > 0f)
            {
                sr.LandingDriftTimer = Mathf.Max(0f, sr.LandingDriftTimer - dt);
                effectiveDriftSpeed *= Mathf.Max(0f, landingDriftMultiplier);
            }

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

            float jumpT = 0f;
            if (sr.JumpActive && jumpLockoutSeconds > 0.01f)
                jumpT = Mathf.Clamp01(sr.JumpElapsed / jumpLockoutSeconds);

            float x = Mathf.Clamp01(sr.SeesawXCurrent);
            bind.ApplySideExtension(leftSide: true, x, jumpT);
            bind.ApplySideExtension(leftSide: false, 1f - x, jumpT);
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
            ResetSeesawToNeutral(ref sr);
            sr.JumpActive = false;
            sr.JumpElapsed = 0f;
            sr.JumpLockoutRemain = 0f;
            sr.LandingDriftTimer = 0f;

            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            bind.PrepareAllPoses();
            ApplyPallbearerPoses(i, ref sr);

            CoffinDanceCoffinBody body = bind.ResolveCoffinBody();
            if (body != null)
            {
                body.SetSimulationActive(false);
                body.SoftReset();
                body.SetSimulationActive(true);
            }
        }

        void ResetSeesawToNeutral(ref SlotRuntime sr)
        {
            float n = Mathf.Clamp01(xSeesawNeutral);
            sr.SeesawBias = n;
            sr.SeesawXCurrent = n;
            sr.HoldTimer = 0f;
            sr.LandingDriftTimer = 0f;
        }
    }
}
