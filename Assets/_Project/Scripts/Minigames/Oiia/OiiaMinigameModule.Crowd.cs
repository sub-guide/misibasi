using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        enum CrowdPhase
        {
            Hidden,
            Rising,
            Active,
            Fading
        }

        [Header("관중 이펙트 (피버)")]
        [Tooltip("피버 시작 시 아래에서 올라오는 거리(px).")]
        [SerializeField] float crowdRiseDistance = 120f;

        [Tooltip("상승·페이드인 시간(초).")]
        [SerializeField] float crowdRiseSeconds = 0.35f;

        [Tooltip("피버 종료 후 페이드아웃 시간(초).")]
        [SerializeField] float crowdFadeSeconds = 0.4f;

        [Tooltip("피버 중 관중 진동 진폭(px).")]
        [SerializeField] float crowdShakeAmplitude = 8f;

        [Tooltip("피버 중 관중 진동 주파수(Hz).")]
        [SerializeField] float crowdShakeFrequency = 18f;

        [Tooltip("켜면 Rest Y를 DjBox 상단 + 패딩으로 맞춤. 끄면 에디터에 배치한 Crowd 위치 사용.")]
        [SerializeField] bool crowdAutoPlaceAboveDjBox = false;

        [Tooltip("DjBox 상단에서 관중 Rest까지 추가 오프셋(px).")]
        [SerializeField] float crowdAboveDjBoxPadding = 40f;

        struct CrowdRuntime
        {
            public CrowdPhase Phase;
            public float PhaseElapsed;
            public float FadeStartAlpha;
            public Vector2 EditorAnchored;
            public Vector2 RestAnchored;
            public Color RestColor;
            public float ShakePhaseX;
            public float ShakePhaseY;
            public bool RestCaptured;
        }

        readonly CrowdRuntime[] _crowd = new CrowdRuntime[SlotCount];

        void ResetCrowdAtBegin(int i)
        {
            CaptureCrowdRest(i);
            HideCrowdImmediate(i);
        }

        void CaptureCrowdRest(int i)
        {
            if (!TryGetBinding(i, out SlotUiBindings b) || b.CrowdRoot == null)
            {
                _crowd[i].RestCaptured = false;
                return;
            }

            _crowd[i].EditorAnchored = b.CrowdRoot.anchoredPosition;
            _crowd[i].RestAnchored = _crowd[i].EditorAnchored;

            if (b.CrowdImage != null)
            {
                Color c = b.CrowdImage.color;
                if (c.a < 0.01f)
                    c.a = 1f;
                _crowd[i].RestColor = c;
            }
            else
            {
                _crowd[i].RestColor = Color.white;
            }

            _crowd[i].RestCaptured = true;
            _crowd[i].ShakePhaseX = Random.Range(0f, 1000f);
            _crowd[i].ShakePhaseY = Random.Range(0f, 1000f);
        }

        void ResolveCrowdRestAboveDjBox(int i, SlotUiBindings b)
        {
            if (!crowdAutoPlaceAboveDjBox || b.DjBoxRoot == null || b.CrowdRoot == null)
                return;

            Transform parent = b.CrowdRoot.parent;
            if (parent == null)
                return;

            var corners = new Vector3[4];
            b.DjBoxRoot.GetWorldCorners(corners);
            Vector3 topMid = (corners[1] + corners[2]) * 0.5f;
            Vector3 local = parent.InverseTransformPoint(topMid);

            _crowd[i].RestAnchored = new Vector2(
                _crowd[i].EditorAnchored.x,
                local.y + crowdAboveDjBoxPadding);
        }

        void EnsureCrowdDrawOrderBehindDjBox(SlotUiBindings b)
        {
            if (b.CrowdRoot == null || b.DjBoxRoot == null)
                return;

            if (b.CrowdRoot.parent != b.DjBoxRoot.parent)
                return;

            int djIndex = b.DjBoxRoot.GetSiblingIndex();
            int crowdIndex = b.CrowdRoot.GetSiblingIndex();
            if (crowdIndex > djIndex)
                b.CrowdRoot.SetSiblingIndex(djIndex);
            else if (crowdIndex < djIndex - 1)
                b.CrowdRoot.SetSiblingIndex(djIndex);
        }

        void HideCrowdImmediate(int i)
        {
            _crowd[i].Phase = CrowdPhase.Hidden;
            _crowd[i].PhaseElapsed = 0f;

            if (!TryGetBinding(i, out SlotUiBindings b))
                return;

            if (b.CrowdRoot != null && _crowd[i].RestCaptured)
                b.CrowdRoot.anchoredPosition = _crowd[i].EditorAnchored;

            if (b.CrowdImage != null)
            {
                Color c = _crowd[i].RestCaptured ? _crowd[i].RestColor : b.CrowdImage.color;
                c.a = 0f;
                b.CrowdImage.color = c;
            }

            if (b.CrowdRoot != null)
                b.CrowdRoot.gameObject.SetActive(false);
        }

        void BeginCrowdFever(int slotIndex)
        {
            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.CrowdRoot == null)
            {
                Debug.LogWarning(
                    $"[Oiia] Crowd 바인딩 없음 (slot {slotIndex}). Prefab에 Crowd/CrowdPeople + CrowdRoot 연결을 확인하세요.",
                    this);
                return;
            }

            if (!_crowd[slotIndex].RestCaptured)
                CaptureCrowdRest(slotIndex);

            if (!_crowd[slotIndex].RestCaptured)
                return;

            ResolveCrowdRestAboveDjBox(slotIndex, b);
            EnsureCrowdDrawOrderBehindDjBox(b);

            b.CrowdRoot.gameObject.SetActive(true);
            if (b.CrowdImage != null)
                b.CrowdImage.enabled = true;

            float rise = Mathf.Max(0f, crowdRiseDistance);
            b.CrowdRoot.anchoredPosition = _crowd[slotIndex].RestAnchored + new Vector2(0f, -rise);

            if (b.CrowdImage != null)
            {
                Color c = _crowd[slotIndex].RestColor;
                c.a = 0f;
                b.CrowdImage.color = c;
            }

            _crowd[slotIndex].Phase = CrowdPhase.Rising;
            _crowd[slotIndex].PhaseElapsed = 0f;
            _crowd[slotIndex].ShakePhaseX = Random.Range(0f, 1000f);
            _crowd[slotIndex].ShakePhaseY = Random.Range(0f, 1000f);
        }

        void EndCrowdFever(int slotIndex)
        {
            if (_crowd[slotIndex].Phase == CrowdPhase.Hidden)
                return;

            if (!TryGetBinding(slotIndex, out SlotUiBindings b) || b.CrowdImage == null)
            {
                HideCrowdImmediate(slotIndex);
                return;
            }

            _crowd[slotIndex].FadeStartAlpha = Mathf.Max(b.CrowdImage.color.a, 0.01f);
            _crowd[slotIndex].Phase = CrowdPhase.Fading;
            _crowd[slotIndex].PhaseElapsed = 0f;
        }

        void TickCrowd(int i)
        {
            CrowdPhase phase = _crowd[i].Phase;
            if (phase == CrowdPhase.Hidden)
                return;

            if (!TryGetBinding(i, out SlotUiBindings b) || b.CrowdRoot == null)
                return;

            switch (phase)
            {
                case CrowdPhase.Rising:
                    TickCrowdRising(i, b);
                    break;
                case CrowdPhase.Active:
                    TickCrowdActive(i, b);
                    break;
                case CrowdPhase.Fading:
                    TickCrowdFading(i, b);
                    break;
            }
        }

        void TickCrowdRising(int i, SlotUiBindings b)
        {
            float dur = Mathf.Max(0.05f, crowdRiseSeconds);
            _crowd[i].PhaseElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_crowd[i].PhaseElapsed / dur);
            float e = 1f - (1f - t) * (1f - t);

            float rise = Mathf.Max(0f, crowdRiseDistance);
            Vector2 start = _crowd[i].RestAnchored + new Vector2(0f, -rise);
            b.CrowdRoot.anchoredPosition = Vector2.Lerp(start, _crowd[i].RestAnchored, e);

            if (b.CrowdImage != null)
            {
                Color c = _crowd[i].RestColor;
                c.a = _crowd[i].RestColor.a * e;
                b.CrowdImage.color = c;
            }

            if (t < 1f)
                return;

            b.CrowdRoot.anchoredPosition = _crowd[i].RestAnchored;
            if (b.CrowdImage != null)
                b.CrowdImage.color = _crowd[i].RestColor;

            _crowd[i].Phase = CrowdPhase.Active;
            _crowd[i].PhaseElapsed = 0f;
        }

        void TickCrowdActive(int i, SlotUiBindings b)
        {
            float amp = Mathf.Max(0f, crowdShakeAmplitude);
            float hz = Mathf.Max(1f, crowdShakeFrequency);
            float time = Time.unscaledTime * hz;
            float ox = (Mathf.PerlinNoise(_crowd[i].ShakePhaseX, time) - 0.5f) * 2f * amp;
            float oy = (Mathf.PerlinNoise(_crowd[i].ShakePhaseY, time + 2.3f) - 0.5f) * 2f * amp;
            b.CrowdRoot.anchoredPosition = _crowd[i].RestAnchored + new Vector2(ox, oy);

            if (b.CrowdImage != null)
                b.CrowdImage.color = _crowd[i].RestColor;
        }

        void TickCrowdFading(int i, SlotUiBindings b)
        {
            float dur = Mathf.Max(0.05f, crowdFadeSeconds);
            _crowd[i].PhaseElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_crowd[i].PhaseElapsed / dur);
            float life = 1f - t;

            float amp = Mathf.Max(0f, crowdShakeAmplitude) * life;
            float hz = Mathf.Max(1f, crowdShakeFrequency);
            float time = Time.unscaledTime * hz;
            float ox = (Mathf.PerlinNoise(_crowd[i].ShakePhaseX, time) - 0.5f) * 2f * amp;
            float oy = (Mathf.PerlinNoise(_crowd[i].ShakePhaseY, time + 2.3f) - 0.5f) * 2f * amp;
            b.CrowdRoot.anchoredPosition = _crowd[i].RestAnchored + new Vector2(ox, oy);

            if (b.CrowdImage != null)
            {
                Color c = _crowd[i].RestColor;
                c.a = Mathf.Lerp(0f, _crowd[i].FadeStartAlpha, life);
                b.CrowdImage.color = c;
            }

            if (t < 1f)
                return;

            HideCrowdImmediate(i);
        }
    }
}
