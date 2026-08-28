using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 관 Rigidbody 설정(제약·무게중심). 어깨에 붙은 동안 Module이 가운데 운구인 2점 지지로
    /// 로컬 Y·Z를 즉시 맞춘다(강체 자세만). 낙하·FailFloor 복구 중에는 어깨 충돌 + 중력.
    /// 관 위치는 에디터에서 배치하고, Play 시작·재부착은 중력으로 어깨에 닿은 뒤 붙인다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CoffinDanceCoffinBody : MonoBehaviour
    {
        [Header("무게중심")]
        [Tooltip("로컬 공간 CoM. (0,0,0)=기하 중심(박스 콜라이더와 동일).")]
        [SerializeField] Vector3 centerOfMassLocal = Vector3.zero;

        Rigidbody _rb;
        Quaternion _restLocalRotation;
        Vector3 _restLocalPosition;
        bool _cachedRest;
        bool _touchedFailFloor;
        int _shoulderContactCount;

        public Rigidbody Body => _rb != null ? _rb : (_rb = GetComponent<Rigidbody>());

        public bool HasTouchedFailFloor => _touchedFailFloor;

        public bool IsTouchingShoulder => _shoulderContactCount > 0;

        public void ClearFailFloorContact() => _touchedFailFloor = false;

        public void ClearShoulderContacts() => _shoulderContactCount = 0;

        public void BeginKinematicHold()
        {
            Rigidbody rb = Body;
            if (rb == null)
                return;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        public void EndKinematicHold()
        {
            Rigidbody rb = Body;
            if (rb == null)
                return;

            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void SetLocalYAndZDegrees(float localY, float zDegrees)
        {
            Transform t = transform;
            Vector3 lp = t.localPosition;
            lp.y = localY;

            Vector3 euler = t.localEulerAngles;
            float x = NormalizeSignedEuler(euler.x);
            float y = NormalizeSignedEuler(euler.y);
            Quaternion localRot = Quaternion.Euler(x, y, zDegrees);

            Transform parent = t.parent;
            Vector3 worldPos = parent != null ? parent.TransformPoint(lp) : lp;
            Quaternion worldRot = parent != null ? parent.rotation * localRot : localRot;

            Rigidbody rb = Body;
            if (rb != null)
            {
                rb.MovePosition(worldPos);
                rb.MoveRotation(worldRot);
            }
            else
            {
                t.localPosition = lp;
                t.localRotation = localRot;
            }
        }

        static float NormalizeSignedEuler(float degrees)
        {
            return Mathf.Repeat(degrees + 180f, 360f) - 180f;
        }

        public void LiftWorldY(float deltaY)
        {
            Rigidbody rb = Body;
            if (rb == null || rb.isKinematic || deltaY <= 0f)
                return;

            rb.MovePosition(rb.position + Vector3.up * deltaY);
            Vector3 v = rb.velocity;
            if (v.y < 0f)
                rb.velocity = new Vector3(v.x, 0f, v.z);
        }

        void Awake()
        {
            EnsureConfigured();
            // Begin 전까지 낙하하지 않도록
            SetSimulationActive(false);
        }

        public void EnsureConfigured()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            if (_rb == null)
                return;

            _rb.useGravity = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.constraints =
                RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY;
            _rb.centerOfMass = centerOfMassLocal;

            if (!_cachedRest)
            {
                _restLocalPosition = transform.localPosition;
                _restLocalRotation = transform.localRotation;
                _cachedRest = true;
            }
        }

        public float GetTiltZDegrees()
        {
            Vector3 euler = transform.localEulerAngles;
            float z = euler.z;
            if (z > 180f)
                z -= 360f;
            return z;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null)
                return;

            if (collision.collider.GetComponentInParent<CoffinDanceFailFloor>() != null)
                _touchedFailFloor = true;

            if (IsEnabledShoulderCollider(collision.collider))
                _shoulderContactCount++;
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision == null || collision.collider == null)
                return;

            if (IsEnabledShoulderCollider(collision.collider) || IsShoulderCollider(collision.collider))
                _shoulderContactCount = Mathf.Max(0, _shoulderContactCount - 1);
        }

        static bool IsEnabledShoulderCollider(Collider col)
        {
            return col != null && col.enabled && IsShoulderCollider(col);
        }

        static bool IsShoulderCollider(Collider col)
        {
            if (!(col is SphereCollider))
                return false;

            return col.GetComponentInParent<CoffinDancePallbearerPose>() != null;
        }

        public void SoftReset()
        {
            EnsureConfigured();
            ClearFailFloorContact();
            ClearShoulderContacts();
            Rigidbody rb = Body;
            if (rb == null)
                return;

            bool wasKinematic = rb.isKinematic;
            if (wasKinematic)
                rb.isKinematic = false;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (_cachedRest)
            {
                transform.localPosition = _restLocalPosition;
                transform.localRotation = _restLocalRotation;
            }

            Physics.SyncTransforms();

            if (wasKinematic)
                rb.isKinematic = true;
        }

        public void SetSimulationActive(bool active)
        {
            EnsureConfigured();
            Rigidbody rb = Body;
            if (rb == null)
                return;

            if (!active)
            {
                if (!rb.isKinematic)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
    }
}
