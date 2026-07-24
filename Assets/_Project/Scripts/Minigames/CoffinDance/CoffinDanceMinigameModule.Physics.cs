using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void StepPhysics(ref SlotRuntime sr, float dt, float leftHeld, float rightHeld)
        {
            float gMul = gravityTorque * _gravityMul;
            float alpha = gMul * Mathf.Sin(sr.ThetaRad);

            // ← = 양의 Z 기울기 반대(복원), → = 음의 Z 반대. (화면에서 관이 기운 쪽 반대로 누르는 감각)
            float control = (leftHeld - rightHeld) * controlTorque * _inertiaMul;
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

            ClampThetaToMaxTilt(ref sr);
        }

        void ClampThetaToMaxTilt(ref SlotRuntime sr)
        {
            float limitRad = GetMaxTiltRadians();
            if (sr.ThetaRad > limitRad)
            {
                sr.ThetaRad = limitRad;
                if (sr.Omega > 0f)
                    sr.Omega = 0f;
            }
            else if (sr.ThetaRad < -limitRad)
            {
                sr.ThetaRad = -limitRad;
                if (sr.Omega < 0f)
                    sr.Omega = 0f;
            }
        }

        float GetMaxTiltRadians() =>
            Mathf.Max(1f, maxTiltDegrees) * Mathf.Deg2Rad;

        bool CheckStumbleAndMaybeEliminate(int i, ref SlotRuntime sr, float dt)
        {
            float limit = GetMaxTiltRadians();
            // 수치 오차로 한도 미감지 방지
            float abs = Mathf.Abs(sr.ThetaRad);
            bool atLimit = abs >= limit - 0.0001f;

            if (!atLimit)
            {
                sr.InStumble = false;
                sr.StumbleTimer = 0f;
                return false;
            }

            if (!sr.InStumble)
            {
                sr.InStumble = true;
                sr.StumbleTimer = Mathf.Max(0f, stumbleBufferSeconds);
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
            ClampThetaToMaxTilt(ref sr);
        }

        void ApplyTiltVisual(int i, ref SlotRuntime sr)
        {
            CoffinDanceSlotBindings bind = GetBindings(i);
            if (bind == null)
                return;

            float deg = sr.ThetaRad * Mathf.Rad2Deg;

            // 연출용 Yaw만 TiltRoot(운구인 포함). 균형 기울기(Z)는 관만.
            if (bind.TiltRoot != null)
                bind.TiltRoot.localRotation = Quaternion.Euler(0f, presentationYawDegrees, 0f);

            Transform coffin = ResolveCoffinTransform(bind);
            if (coffin == null)
                return;

            coffin.localRotation = Quaternion.Euler(0f, 0f, deg);

            if (sr.JumpLockoutRemain > 0f)
            {
                float t = 1f - Mathf.Clamp01(sr.JumpLockoutRemain / Mathf.Max(0.01f, jumpLockoutSeconds));
                float hop = Mathf.Sin(t * Mathf.PI) * 0.35f;
                if (sr.HasCoffinBase)
                    coffin.localPosition = sr.CoffinBaseLocalPos + Vector3.up * hop;
            }
            else if (sr.HasCoffinBase)
            {
                coffin.localPosition = sr.CoffinBaseLocalPos;
            }

            if (scalePallbearersToCoffinCorners)
                ApplyPallbearerScalesToCoffinCorners(ref sr, bind, coffin);
        }

        static void CachePallbearerBases(ref SlotRuntime sr, CoffinDanceSlotBindings bind)
        {
            if (bind?.Pallbearers == null)
                return;

            int n = bind.Pallbearers.Length;
            sr.PallbearerBaseScale = new Vector3[n];
            sr.PallbearerBasePos = new Vector3[n];

            for (var p = 0; p < n; p++)
            {
                Transform t = bind.Pallbearers[p];
                if (t == null)
                    continue;

                sr.PallbearerBaseScale[p] = t.localScale;
                sr.PallbearerBasePos[p] = t.localPosition;
            }
        }

        void ApplyPallbearerScalesToCoffinCorners(ref SlotRuntime sr, CoffinDanceSlotBindings bind, Transform coffin)
        {
            if (bind?.Pallbearers == null || sr.PallbearerBaseScale == null)
                return;

            // 기본 Cube 메시 half-extents = localScale/2
            Vector3 half = coffin.localScale * 0.5f;
            float hw = Mathf.Abs(half.x);
            float hh = Mathf.Abs(half.y);
            float halfCapsule = Mathf.Max(0.01f, pallbearerCapsuleHalfHeight);

            for (var p = 0; p < bind.Pallbearers.Length; p++)
            {
                Transform bearer = bind.Pallbearers[p];
                if (bearer == null || p >= sr.PallbearerBaseScale.Length)
                    continue;

                Vector3 baseScale = sr.PallbearerBaseScale[p];
                Vector3 basePos = sr.PallbearerBasePos != null && p < sr.PallbearerBasePos.Length
                    ? sr.PallbearerBasePos[p]
                    : bearer.localPosition;

                bool isLeft = basePos.x < coffin.localPosition.x;
                float localZ = basePos.z - coffin.localPosition.z;
                localZ = Mathf.Clamp(localZ, -Mathf.Abs(half.z), Mathf.Abs(half.z));

                // 관 하단 좌/우 모서리 (받침 높이)
                var cornerInCoffin = new Vector3(isLeft ? -hw : hw, -hh, localZ);
                Vector3 cornerInParent = coffin.localPosition + coffin.localRotation * cornerInCoffin;

                float restTopY = basePos.y + halfCapsule * baseScale.y;
                float cornerTopY = cornerInParent.y + pallbearerCornerHeightOffset;
                // follow=0 이면 원래 키, 1이면 모서리(+오프셋)에 맞춤
                float targetTopY = Mathf.Lerp(restTopY, cornerTopY, pallbearerCornerFollow);

                // 발 위치는 유지하고, 목표 머리 높이에 맞게 Y 스케일
                float feetY = basePos.y - halfCapsule * baseScale.y;
                float scaleY = (targetTopY - feetY) / halfCapsule;
                scaleY = Mathf.Clamp(scaleY, pallbearerMinScaleY, pallbearerMaxScaleY);

                bearer.localScale = new Vector3(baseScale.x, scaleY, baseScale.z);
                bearer.localPosition = new Vector3(basePos.x, feetY + halfCapsule * scaleY, basePos.z);
            }
        }

        static Transform ResolveCoffinTransform(CoffinDanceSlotBindings bind)
        {
            if (bind == null)
                return null;

            if (bind.Coffin != null)
                return bind.Coffin;

            // 구버전: Coffin 미연결 시 TiltRoot 전체 기울기 (하위 호환)
            return bind.TiltRoot;
        }
    }
}
