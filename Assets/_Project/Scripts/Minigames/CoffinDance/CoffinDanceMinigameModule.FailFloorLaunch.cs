using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void EnsureFailLaunchArray()
        {
            if (_failLaunch != null && _failLaunch.Length == SlotCount)
                return;

            _failLaunch = new CoffinDanceFailFloorLaunchState[SlotCount];
            for (var i = 0; i < SlotCount; i++)
                _failLaunch[i] = new CoffinDanceFailFloorLaunchState();
        }

        CoffinDanceFailFloorLaunchState GetFailLaunch(int i)
        {
            EnsureFailLaunchArray();
            return _failLaunch[i];
        }

        void BeginFailFloorLaunch(int i, CoffinDanceSlotBindings bind, CoffinDanceCoffinBody body)
        {
            CoffinDanceFailFloorLaunchState launch = GetFailLaunch(i);
            launch.ClearReturnSnapshot();
            RestoreFailFloorLaunchColliderModes(launch);

            float speedMin = Mathf.Min(failFloorLaunchSpeedMin, failFloorLaunchSpeedMax);
            float speedMax = Mathf.Max(failFloorLaunchSpeedMin, failFloorLaunchSpeedMax);
            float spinMax = Mathf.Max(0f, failFloorLaunchSpinDegPerSec);

            launch.CoffinLocalVel = RandomLaunchLocalVelocity(speedMin, speedMax);
            launch.CoffinLocalAngularVelDeg = RandomLaunchAngularVelocityDeg(spinMax);
            CacheFailFloorLaunchColliders(bind, body, launch);
            BeginFailCameraLaunch(i, bind, launch);

            body.GetRestLocal(out Vector3 coffinRestPos, out Quaternion coffinRestRot);
            launch.CoffinTargetLocalPos = new Vector3(coffinRestPos.x, failFloorRecoverLocalY, coffinRestPos.z);
            launch.CoffinTargetLocalRot = CoffinRecoverTargetRotation(coffinRestRot);

            if (bind?.Pallbearers != null)
            {
                for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
                {
                    launch.PallLocalVel[p] = RandomLaunchLocalVelocity(speedMin, speedMax);
                    launch.PallLocalAngularVelDeg[p] = RandomLaunchAngularVelocityDeg(spinMax);

                    CoffinDancePallbearerPose pose = bind.GetPallbearerPose(p);
                    if (pose != null)
                        pose.GetRestLocal(out launch.PallTargetLocalPos[p], out launch.PallTargetLocalRot[p]);
                    else
                    {
                        launch.PallTargetLocalPos[p] = Vector3.zero;
                        launch.PallTargetLocalRot[p] = Quaternion.identity;
                    }
                }
            }
        }

        static Quaternion CoffinRecoverTargetRotation(Quaternion restLocalRot)
        {
            Vector3 e = restLocalRot.eulerAngles;
            float x = NormalizeSignedEulerDegrees(e.x);
            float y = NormalizeSignedEulerDegrees(e.y);
            return Quaternion.Euler(x, y, 0f);
        }

        static float NormalizeSignedEulerDegrees(float degrees)
        {
            return Mathf.Repeat(degrees + 180f, 360f) - 180f;
        }

        static Vector3 RandomLaunchLocalVelocity(float speedMin, float speedMax)
        {
            float speed = Random.Range(speedMin, speedMax);
            return Random.onUnitSphere * speed;
        }

        static Vector3 RandomLaunchAngularVelocityDeg(float spinMax)
        {
            if (spinMax <= 0f)
                return Vector3.zero;

            return Random.onUnitSphere * Random.Range(0f, spinMax);
        }

        void TickFailFloorLaunchFly(
            int i,
            CoffinDanceSlotBindings bind,
            CoffinDanceCoffinBody body,
            CoffinDanceFailFloorLaunchState launch,
            float dt)
        {
            float gravity = Mathf.Max(0f, failFloorLaunchGravity);
            Vector3 grav = Vector3.down * gravity;

            if (body != null)
            {
                Transform ct = body.transform;
                launch.CoffinLocalVel += grav * dt;
                Vector3 lp = ct.localPosition + launch.CoffinLocalVel * dt;
                Quaternion lr = ct.localRotation * Quaternion.Euler(launch.CoffinLocalAngularVelDeg * dt);
                body.SetLocalPose(lp, lr);
            }

            if (bind?.Pallbearers != null)
            {
                for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
                {
                    Transform root = p < bind.Pallbearers.Length ? bind.Pallbearers[p] : null;
                    if (root == null)
                        continue;

                    launch.PallLocalVel[p] += grav * dt;
                    root.localPosition += launch.PallLocalVel[p] * dt;
                    root.localRotation *= Quaternion.Euler(launch.PallLocalAngularVelDeg[p] * dt);
                }
            }

            float camG = Mathf.Max(0f, failCameraGravity);
            Vector3 camGrav = Vector3.down * camG;
            TickFailCameraLaunchFly(bind, launch, camGrav, dt);

            Physics.SyncTransforms();
            ApplyFailFloorLaunchDepenet(bind, body, launch);
        }

        void BeginFailCameraLaunch(int i, CoffinDanceSlotBindings bind, CoffinDanceFailFloorLaunchState launch)
        {
            float down = Mathf.Max(0f, failCameraInitialDownSpeed);
            launch.CamLocalVel = down > 0f ? Vector3.down * down : Vector3.zero;
            launch.CamLocalAngularVelDeg = Vector3.zero;
            launch.CamTargetLocalPos = Vector3.zero;
            launch.CamTargetLocalRot = Quaternion.identity;

            if (!failCameraLaunchEnabled || bind == null)
                return;

            Camera cam = ResolveSlotCamera(bind);
            if (cam == null)
                return;

            CaptureSlotCameraRestIfNeeded(i, cam);
            if (!_camRestCaptured[i])
                return;

            launch.CamLocalAngularVelDeg = RandomFailCameraAngularVelocityDeg();
            launch.CamTargetLocalPos = _camRestLocalPos[i];
            launch.CamTargetLocalRot = Quaternion.Euler(_camRestEulerX[i], _camRestEulerY[i], 0f);
        }

        Vector3 RandomFailCameraAngularVelocityDeg()
        {
            float spinMax = Mathf.Max(0f, failCameraSpinDegPerSec);
            if (spinMax <= 0f)
                return Vector3.zero;

            return Random.insideUnitSphere * Random.Range(0f, spinMax);
        }

        void TickFailCameraLaunchFly(
            CoffinDanceSlotBindings bind,
            CoffinDanceFailFloorLaunchState launch,
            Vector3 grav,
            float dt)
        {
            if (!failCameraLaunchEnabled || bind == null || launch.CamLaunchFloorCollider == null)
                return;

            Camera cam = ResolveSlotCamera(bind);
            if (cam == null)
                return;

            Transform ct = cam.transform;
            launch.CamLocalVel += grav * dt;
            ct.localPosition += launch.CamLocalVel * dt;
            ct.localRotation *= Quaternion.Euler(launch.CamLocalAngularVelDeg * dt);
        }

        void SnapshotFailFloorReturnStarts(
            CoffinDanceSlotBindings bind,
            CoffinDanceCoffinBody body,
            CoffinDanceFailFloorLaunchState launch)
        {
            if (body != null)
            {
                Transform ct = body.transform;
                launch.CoffinReturnStartLocalPos = ct.localPosition;
                launch.CoffinReturnStartLocalRot = ct.localRotation;
            }

            if (bind?.Pallbearers == null)
                return;

            for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
            {
                Transform root = p < bind.Pallbearers.Length ? bind.Pallbearers[p] : null;
                if (root == null)
                    continue;

                launch.PallReturnStartLocalPos[p] = root.localPosition;
                launch.PallReturnStartLocalRot[p] = root.localRotation;
            }

            if (failCameraLaunchEnabled && bind != null)
            {
                Camera cam = ResolveSlotCamera(bind);
                if (cam != null)
                {
                    Transform ct = cam.transform;
                    launch.CamReturnStartLocalPos = ct.localPosition;
                    launch.CamReturnStartLocalRot = ct.localRotation;
                }
            }
        }

        void ApplyFailFloorReturnLerp(
            CoffinDanceSlotBindings bind,
            CoffinDanceCoffinBody body,
            CoffinDanceFailFloorLaunchState launch,
            float returnLinear01,
            ref SlotRuntime sr)
        {
            float t = Mathf.Clamp01(returnLinear01);

            if (body != null)
            {
                Vector3 pos = Vector3.Lerp(launch.CoffinReturnStartLocalPos, launch.CoffinTargetLocalPos, t);
                Quaternion rot = Quaternion.Slerp(launch.CoffinReturnStartLocalRot, launch.CoffinTargetLocalRot, t);
                body.SetLocalPose(pos, rot);
            }

            float seesaw = Mathf.Lerp(sr.FailFloorRecoverStartSeesaw, 0.5f, t);
            sr.SeesawBias = seesaw;
            sr.SeesawXCurrent = seesaw;
            sr.HoldTimer = 0f;

            if (bind?.Pallbearers != null)
            {
                for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
                {
                    Transform root = p < bind.Pallbearers.Length ? bind.Pallbearers[p] : null;
                    if (root == null)
                        continue;

                    root.localPosition = Vector3.Lerp(launch.PallReturnStartLocalPos[p], launch.PallTargetLocalPos[p], t);
                    root.localRotation = Quaternion.Slerp(
                        launch.PallReturnStartLocalRot[p],
                        launch.PallTargetLocalRot[p],
                        t);
                }
            }

            ApplyFailCameraReturnLerp(bind, launch, t);

            Physics.SyncTransforms();
        }

        void ApplyFailCameraReturnLerp(CoffinDanceSlotBindings bind, CoffinDanceFailFloorLaunchState launch, float t)
        {
            if (!failCameraLaunchEnabled || bind == null || launch.CamLaunchFloorCollider == null)
                return;

            Camera cam = ResolveSlotCamera(bind);
            if (cam == null)
                return;

            Transform ct = cam.transform;
            ct.localPosition = Vector3.Lerp(launch.CamReturnStartLocalPos, launch.CamTargetLocalPos, t);
            ct.localRotation = Quaternion.Slerp(launch.CamReturnStartLocalRot, launch.CamTargetLocalRot, t);
        }
    }
}
