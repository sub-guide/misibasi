using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        void CacheFailFloorLaunchColliders(CoffinDanceSlotBindings bind, CoffinDanceCoffinBody body, CoffinDanceFailFloorLaunchState launch)
        {
            launch.FailFloorCollider = bind != null ? bind.ResolveFailFloorCollider() : null;

            launch.CoffinFloorCollider = null;
            if (body != null)
            {
                Collider coffinCol = body.GetComponent<Collider>();
                if (coffinCol != null && coffinCol.enabled && !coffinCol.isTrigger)
                    launch.CoffinFloorCollider = coffinCol;
            }

            for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
                launch.PallLaunchFloorColliders[p] = bind != null ? bind.GetPallbearerLaunchFloorCollider(p) : null;

            PreparePallLaunchCollidersForDepenet(launch);
        }

        void PreparePallLaunchCollidersForDepenet(CoffinDanceFailFloorLaunchState launch)
        {
            launch.ResetPallLaunchColliderDepenet();

            for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
            {
                Collider col = launch.PallLaunchFloorColliders[p];
                if (col == null)
                    continue;

                launch.PallLaunchColliderWasTrigger[p] = col.isTrigger;
                launch.PallLaunchColliderDepenetPrepared[p] = true;
                col.isTrigger = false;
            }
        }

        void RestorePallLaunchColliderTriggers(CoffinDanceFailFloorLaunchState launch)
        {
            if (launch == null)
                return;

            for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
            {
                if (!launch.PallLaunchColliderDepenetPrepared[p])
                    continue;

                Collider col = launch.PallLaunchFloorColliders[p];
                if (col != null)
                    col.isTrigger = launch.PallLaunchColliderWasTrigger[p];

                launch.PallLaunchColliderDepenetPrepared[p] = false;
            }
        }

        void ApplyFailFloorLaunchDepenet(
            CoffinDanceSlotBindings bind,
            CoffinDanceCoffinBody body,
            CoffinDanceFailFloorLaunchState launch)
        {
            Collider floor = launch.FailFloorCollider;
            if (floor == null)
                return;

            float clearance = Mathf.Max(0f, failFloorLaunchFloorClearance);
            int passes = Mathf.Clamp(failFloorLaunchDepenetPassCount, 1, 4);

            for (var pass = 0; pass < passes; pass++)
            {
                if (body != null && launch.CoffinFloorCollider != null)
                {
                    if (TryDepenetrateCollider(launch.CoffinFloorCollider, floor, clearance, out Vector3 worldDelta))
                    {
                        Transform ct = body.transform;
                        ct.position += worldDelta;
                        ClampLaunchLocalVelocityUp(ref launch.CoffinLocalVel, ct.parent, worldDelta, floor.transform.up);
                    }
                }

                if (bind?.Pallbearers == null)
                    continue;

                Vector3 floorUp = floor.transform.up;

                for (var p = 0; p < CoffinDanceFailFloorLaunchState.PallCount; p++)
                {
                    Collider pallCol = launch.PallLaunchFloorColliders[p];
                    if (pallCol == null)
                        continue;

                    Transform root = p < bind.Pallbearers.Length ? bind.Pallbearers[p] : null;
                    if (root == null)
                        continue;

                    if (TryDepenetrateCollider(pallCol, floor, clearance, out Vector3 pallDelta))
                    {
                        root.position += pallDelta;
                        ClampLaunchLocalVelocityUp(ref launch.PallLocalVel[p], root.parent, pallDelta, floorUp);
                        continue;
                    }

                    ApplyLaunchFloorBoundsLift(pallCol, floor, clearance, root, ref launch.PallLocalVel[p], floorUp);
                }
            }

            Physics.SyncTransforms();
        }

        static bool TryDepenetrateCollider(Collider body, Collider floor, float clearance, out Vector3 worldDelta)
        {
            worldDelta = Vector3.zero;
            if (body == null || floor == null || !body.enabled || !floor.enabled)
                return false;

            Transform bodyTransform = body.transform;
            Transform floorTransform = floor.transform;
            if (!Physics.ComputePenetration(
                    body,
                    bodyTransform.position,
                    bodyTransform.rotation,
                    floor,
                    floorTransform.position,
                    floorTransform.rotation,
                    out Vector3 direction,
                    out float distance))
            {
                return false;
            }

            if (distance <= 0f)
                return false;

            worldDelta = direction * (distance + clearance);
            return worldDelta.sqrMagnitude > 0f;
        }

        static void ApplyLaunchFloorBoundsLift(
            Collider body,
            Collider floor,
            float clearance,
            Transform moveRoot,
            ref Vector3 localVel,
            Vector3 floorUp)
        {
            if (body == null || floor == null || moveRoot == null)
                return;

            float floorTop = Vector3.Dot(floor.bounds.max, floorUp);
            float bodyBottom = Vector3.Dot(body.bounds.min, floorUp);
            float gap = floorTop + clearance - bodyBottom;
            if (gap <= 0f)
                return;

            Vector3 worldDelta = floorUp * gap;
            moveRoot.position += worldDelta;
            ClampLaunchLocalVelocityUp(ref localVel, moveRoot.parent, worldDelta, floorUp);
        }

        static void ClampLaunchLocalVelocityUp(
            ref Vector3 localVel,
            Transform parent,
            Vector3 worldCorrection,
            Vector3 floorUp)
        {
            if (Vector3.Dot(worldCorrection, floorUp) <= 0.0001f)
                return;

            Vector3 worldVel = parent != null ? parent.TransformVector(localVel) : localVel;
            float normalSpeed = Vector3.Dot(worldVel, floorUp);
            if (normalSpeed >= 0f)
                return;

            worldVel -= floorUp * normalSpeed;
            localVel = parent != null ? parent.InverseTransformVector(worldVel) : worldVel;
        }
    }
}
