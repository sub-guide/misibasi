using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 관 Rigidbody 설정(제약·무게중심). 플레이 중 관 운동은 운구인 어깨 SphereCollider 충돌 +
    /// FailFloor 접촉 시 월드 +Y Impulse.
    /// 관 위치는 에디터에서 배치하고, Play 시 중력으로 어깨 Collider 위에 얹힌다.
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

        public Rigidbody Body => _rb != null ? _rb : (_rb = GetComponent<Rigidbody>());

        public bool HasTouchedFailFloor => _touchedFailFloor;

        public void ClearFailFloorContact() => _touchedFailFloor = false;

        public void ApplyUpwardImpulse(float impulse)
        {
            Rigidbody rb = Body;
            if (rb == null || rb.isKinematic || impulse <= 0f)
                return;

            rb.AddForce(Vector3.up * impulse, ForceMode.Impulse);
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
        }

        public void SoftReset()
        {
            EnsureConfigured();
            ClearFailFloorContact();
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
