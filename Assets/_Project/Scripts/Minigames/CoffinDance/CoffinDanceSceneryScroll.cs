using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 바닥 UV와 옆 배경을 민다.
    /// 옆 배경은 줄마다 스프라이트 긴 변 방향이고, 그 중 +Z가 화면 안쪽이다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceSceneryScroll : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Strip
        {
            [Tooltip("같은 기울기의 복사본. 옆 배경은 2장. 긴 변 위에 둔다.")]
            public Transform[] pieces;

            [Tooltip("다음 복사본까지의 거리. 스프라이트 긴 변 방향. 0이면 배치된 간격으로 계산한다.")]
            public float spacing;
        }

        [Header("속도")]
        [Tooltip("초당 이동. 각 옆 배경의 긴 변 중 +Z(화면 안쪽) 쪽.")]
        [SerializeField] float scrollSpeed = 1f;

        [Header("옆 배경")]
        [Tooltip("0 = 왼쪽, 1 = 오른쪽. 각 줄에 복사본 2장.")]
        [SerializeField] Strip[] strips = { new Strip(), new Strip() };

        [Header("바닥")]
        [Tooltip("UV를 밀 렌더러. FailFloor 콜라이더와 분리된 보이는 바닥.")]
        [SerializeField] Renderer floorRenderer;

        [Tooltip("속도 1일 때 초당 UV. 흙이 옆으로 흐르면 X/Y를 바꾼다.")]
        [SerializeField] Vector2 floorUvPerMeter = new Vector2(0f, 0.05f);

        [Tooltip("Toil(Standard)는 _MainTex.")]
        [SerializeField] string floorTextureProperty = "_MainTex";

        float _traveled;
        Vector2 _uvOffset;
        MaterialPropertyBlock _block;
        float[][] _baseAlong;
        Vector3[][] _basePerp;
        Vector3[] _axis;
        float[] _rangeMin;
        float[] _span;
        int _floorStId;
        string _cachedProperty;

        public void ResetScroll()
        {
            _traveled = 0f;
            _uvOffset = Vector2.zero;
            CaptureBases();
            ApplyStrips();
            ApplyFloor();
        }

        public void Tick(float dt)
        {
            if (dt == 0f)
                return;

            if (_baseAlong == null)
                CaptureBases();

            _traveled += scrollSpeed * dt;
            _uvOffset += floorUvPerMeter * (scrollSpeed * dt);
            _uvOffset.x = Mathf.Repeat(_uvOffset.x, 1f);
            _uvOffset.y = Mathf.Repeat(_uvOffset.y, 1f);
            ApplyStrips();
            ApplyFloor();
        }

        void CaptureBases()
        {
            int n = strips != null ? strips.Length : 0;
            _baseAlong = new float[n][];
            _basePerp = new Vector3[n][];
            _axis = new Vector3[n];
            _rangeMin = new float[n];
            _span = new float[n];

            for (var s = 0; s < n; s++)
            {
                Strip strip = strips[s];
                Transform[] pieces = strip != null ? strip.pieces : null;
                int count = pieces != null ? pieces.Length : 0;
                var along = new float[count];
                var perp = new Vector3[count];
                Vector3 axis = Vector3.forward;
                var haveAxis = false;
                float min = float.MaxValue;
                float max = float.MinValue;
                var alive = 0;

                for (var i = 0; i < count; i++)
                {
                    Transform piece = pieces[i];
                    if (piece == null)
                        continue;

                    if (!haveAxis)
                    {
                        axis = AxisTowardPositiveZ(piece);
                        haveAxis = true;
                    }

                    float t = Vector3.Dot(piece.localPosition, axis);
                    along[i] = t;
                    perp[i] = piece.localPosition - axis * t;
                    alive++;
                    if (t < min)
                        min = t;
                    if (t > max)
                        max = t;
                }

                _axis[s] = axis;
                _baseAlong[s] = along;
                _basePerp[s] = perp;
                float spacing = strip != null ? strip.spacing : 0f;
                if (spacing <= 0f && alive >= 2)
                    spacing = (max - min) / (alive - 1);

                if (spacing > 0f && alive >= 2)
                {
                    _span[s] = spacing * alive;
                    _rangeMin[s] = min;
                }
            }
        }

        /// <summary>스프라이트 긴 변(로컬 +X)을 부모 공간으로. +Z 성분이 화면 안쪽.</summary>
        static Vector3 AxisTowardPositiveZ(Transform piece)
        {
            Vector3 axis = piece.right;
            if (piece.parent != null)
                axis = piece.parent.InverseTransformDirection(axis);

            axis.y = 0f;
            if (axis.sqrMagnitude < 0.0001f)
                return Vector3.forward;

            axis.Normalize();
            if (axis.z < 0f)
                axis = -axis;

            return axis;
        }

        void ApplyStrips()
        {
            if (strips == null || _baseAlong == null)
                return;

            int n = Mathf.Min(strips.Length, _baseAlong.Length);
            for (var s = 0; s < n; s++)
            {
                Strip strip = strips[s];
                Transform[] pieces = strip != null ? strip.pieces : null;
                float[] along = _baseAlong[s];
                Vector3[] perp = _basePerp[s];
                if (pieces == null || along == null || perp == null)
                    continue;

                Vector3 axis = _axis[s];
                float span = _span[s];
                float rangeMin = _rangeMin[s];
                int count = Mathf.Min(pieces.Length, along.Length);
                for (var i = 0; i < count; i++)
                {
                    Transform piece = pieces[i];
                    if (piece == null)
                        continue;

                    float t = along[i] + _traveled;
                    if (span > 0f)
                        t = rangeMin + Mathf.Repeat(t - rangeMin, span);

                    piece.localPosition = perp[i] + axis * t;
                }
            }
        }

        void ApplyFloor()
        {
            if (floorRenderer == null)
                return;

            Material shared = floorRenderer.sharedMaterial;
            if (shared == null || string.IsNullOrEmpty(floorTextureProperty))
                return;

            if (_block == null)
                _block = new MaterialPropertyBlock();

            if (_cachedProperty != floorTextureProperty)
            {
                _cachedProperty = floorTextureProperty;
                _floorStId = Shader.PropertyToID(floorTextureProperty + "_ST");
            }

            Vector2 scale = shared.GetTextureScale(floorTextureProperty);
            floorRenderer.GetPropertyBlock(_block);
            _block.SetVector(_floorStId, new Vector4(scale.x, scale.y, _uvOffset.x, _uvOffset.y));
            floorRenderer.SetPropertyBlock(_block);
        }
    }
}
