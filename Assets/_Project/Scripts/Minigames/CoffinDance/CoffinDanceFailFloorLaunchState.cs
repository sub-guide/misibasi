using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>FailFloor 접촉 시 슬롯별 날림·복귀 스냅샷. Module이 소유.</summary>
    sealed class CoffinDanceFailFloorLaunchState
    {
        public const int PallCount = 6;

        public Vector3 CoffinLocalVel;
        public Vector3 CoffinLocalAngularVelDeg;

        public readonly Vector3[] PallLocalVel = new Vector3[PallCount];
        public readonly Vector3[] PallLocalAngularVelDeg = new Vector3[PallCount];

        public bool ReturnSnapshotted;

        public Vector3 CoffinReturnStartLocalPos;
        public Quaternion CoffinReturnStartLocalRot;
        public Vector3 CoffinTargetLocalPos;
        public Quaternion CoffinTargetLocalRot;

        public readonly Vector3[] PallReturnStartLocalPos = new Vector3[PallCount];
        public readonly Quaternion[] PallReturnStartLocalRot = new Quaternion[PallCount];
        public readonly Vector3[] PallTargetLocalPos = new Vector3[PallCount];
        public readonly Quaternion[] PallTargetLocalRot = new Quaternion[PallCount];

        public Collider FailFloorCollider;
        public Collider CoffinFloorCollider;
        public readonly Collider[] PallLaunchFloorColliders = new Collider[PallCount];
        public readonly bool[] PallLaunchColliderWasTrigger = new bool[PallCount];
        public readonly bool[] PallLaunchColliderDepenetPrepared = new bool[PallCount];

        public void ClearReturnSnapshot()
        {
            ReturnSnapshotted = false;
        }

        public void ResetPallLaunchColliderDepenet()
        {
            for (var p = 0; p < PallCount; p++)
                PallLaunchColliderDepenetPrepared[p] = false;
        }
    }
}
