using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const float CatBounceFanHalfAngleDegrees = 80f;
        const float CatTier3ScaleMultiplier = 2f;
        const string CatScreenMovementOverlayName = "CatScreenMovementOverlay";

        [Header("고양이 UI 바운스 (2티어 기준 속도)")]
        [Tooltip("2티어에서 슬롯 패널 안 직선 이동 속도(anchoredPosition 단위/초).")]
        [SerializeField] float catTier2MoveSpeed = 120f;

        [Tooltip("2티어에서 Z축 회전 속도(도/초). 시계/반시계는 랜덤 시작·벽 반사마다 반전.")]
        [SerializeField] float catTier2RotateSpeed = 90f;

        [Header("고양이 UI 바운스 (3티어 속도)")]
        [Tooltip("3티어 화면 전체 이동 직선 속도(anchoredPosition 단위/초). 2티어와 독립 Inspector 튜닝.")]
        [SerializeField] float catTier3MoveSpeed = 240f;

        [Tooltip("3티어 Z축 회전 속도(도/초).")]
        [SerializeField] float catTier3RotateSpeed = 180f;

        [Tooltip("슬롯 패널 테두리 추가 여백(픽셀). 음수면 이동 범위를 더 넓힘. 고양이 Pivot 0.5 기준.")]
        [SerializeField] float catBoundaryPadding = 8f;

        [Tooltip(
            "2·3티어 경계 판정 half-size 배율. 1=이미지 전체 유지. " +
            "낮을수록 중심 이동 범위 넓어짐. 3티어는 슬롯이 아닌 화면 전체 경계에 동일 배율 적용.")]
        [SerializeField] [Range(0.05f, 1f)] float catBoundaryCollisionScale = 0.35f;

        [Tooltip("2·3티어 이동 중 Cat에 붙는 Canvas.sortingOrder = 이 값 + 슬롯 번호. 인접 슬롯 UI 위에 그림.")]
        [SerializeField] int catMovementDrawSortOrderBase = 100;

        RectTransform _catScreenMovementOverlay;

        readonly float[] _catMoveAngleDegrees = new float[SlotCount];
        readonly float[] _catMoveRotateSign = { 1f, 1f, 1f, 1f };
        readonly int[] _catMovementTierApplied = { 1, 1, 1, 1 };
        readonly bool[] _catDrawSortCanvasAdded = new bool[SlotCount];
        readonly Transform[] _catSlotHomeParent = new Transform[SlotCount];
        readonly Vector3[] _catRestLocalScale = new Vector3[SlotCount];
        readonly bool[] _catHomeCaptured = new bool[SlotCount];
        readonly bool[] _catTier3ScreenMode = new bool[SlotCount];

        int ResolveCatGameplayTier(ref SlotRuntime sr) => ResolveGameplayTier(ref sr);

        float CatMoveSpeedForTier(int tier) =>
            tier >= 3 ? catTier3MoveSpeed : catTier2MoveSpeed;

        float CatRotateSpeedForTier(int tier) =>
            tier >= 3 ? catTier3RotateSpeed : catTier2RotateSpeed;

        float CatBoundaryCollisionScale() => Mathf.Clamp(catBoundaryCollisionScale, 0.05f, 1f);

        void ResetAllCatMovement()
        {
            ForEachSlot(ResetCatMovementImmediate);

            for (var i = 0; i < SlotCount; i++)
            {
                _catHomeCaptured[i] = false;
                _catTier3ScreenMode[i] = false;
            }
        }

        void EnsureCatMovementHomeCaptured(int i, RectTransform catRt)
        {
            if (_catHomeCaptured[i])
                return;

            _catSlotHomeParent[i] = catRt.parent;
            _catRestLocalScale[i] = catRt.localScale;
            _catHomeCaptured[i] = true;
        }

        RectTransform GetSlotPanelRect(int i)
        {
            if (slotPanels != null && i >= 0 && i < slotPanels.Length && slotPanels[i] != null)
                return slotPanels[i].transform as RectTransform;

            if (!TryGetBinding(i, out SlotUiBindings b) || b.CatAnimator == null)
                return null;

            return b.CatAnimator.transform.parent as RectTransform;
        }

        RectTransform EnsureCatScreenMovementOverlay()
        {
            if (_catScreenMovementOverlay != null)
                return _catScreenMovementOverlay;

            Transform parent = slotPanelsContainer != null ? slotPanelsContainer.parent : null;
            if (parent == null)
            {
                var canvasGo = GameObject.Find("Canvas_Minigame");
                if (canvasGo != null)
                    parent = canvasGo.transform;
            }

            if (parent == null)
                parent = transform;

            Transform existing = parent.Find(CatScreenMovementOverlayName);
            if (existing != null)
            {
                _catScreenMovementOverlay = existing as RectTransform;
                return _catScreenMovementOverlay;
            }

            var go = new GameObject(CatScreenMovementOverlayName, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.SetAsLastSibling();
            _catScreenMovementOverlay = rt;
            return rt;
        }

        void EnterCatTier3ScreenMode(int i, RectTransform catRt)
        {
            EnsureCatMovementHomeCaptured(i, catRt);

            RectTransform overlay = EnsureCatScreenMovementOverlay();
            if (overlay == null)
                return;

            catRt.SetParent(overlay, worldPositionStays: true);
            catRt.localScale = _catRestLocalScale[i] * CatTier3ScaleMultiplier;
            _catTier3ScreenMode[i] = true;
        }

        void RestoreCatToSlotHome(int i, RectTransform catRt)
        {
            EnsureCatMovementHomeCaptured(i, catRt);

            Transform home = _catSlotHomeParent[i];
            if (home != null && catRt.parent != home)
                catRt.SetParent(home, worldPositionStays: false);

            catRt.anchoredPosition = Vector2.zero;
            catRt.localRotation = Quaternion.identity;
            catRt.localScale = _catRestLocalScale[i];
            _catTier3ScreenMode[i] = false;
        }

        void ResetCatMovementImmediate(int i)
        {
            _catMoveAngleDegrees[i] = 0f;
            _catMoveRotateSign[i] = 1f;
            _catMovementTierApplied[i] = 1;

            if (!TryGetBinding(i, out SlotUiBindings b) || b.CatAnimator == null)
                return;

            RectTransform catRt = b.CatAnimator.transform as RectTransform;
            if (catRt == null)
                return;

            RestoreCatToSlotHome(i, catRt);
            SetCatDrawOnTop(i, false);
        }

        void SetCatDrawOnTop(int i, bool onTop)
        {
            if (!TryGetBinding(i, out SlotUiBindings b) || b.CatAnimator == null)
                return;

            GameObject go = b.CatAnimator.gameObject;
            Canvas canvas = go.GetComponent<Canvas>();

            if (onTop)
            {
                if (canvas == null)
                {
                    canvas = go.AddComponent<Canvas>();
                    _catDrawSortCanvasAdded[i] = true;
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = catMovementDrawSortOrderBase + i;
                return;
            }

            if (canvas == null)
                return;

            if (_catDrawSortCanvasAdded[i])
            {
                Destroy(canvas);
                _catDrawSortCanvasAdded[i] = false;
                return;
            }

            canvas.overrideSorting = false;
        }

        void BeginCatMovement(int i)
        {
            _catMoveAngleDegrees[i] = Random.Range(0f, 360f);
            _catMoveRotateSign[i] = Random.value < 0.5f ? -1f : 1f;
        }

        bool TryGetCatBoundaryHalfExtents(int i, int tier, RectTransform catRt, out float maxX, out float maxY)
        {
            maxX = 0f;
            maxY = 0f;

            if (catRt == null)
                return false;

            RectTransform boundsRt = tier >= 3 ? EnsureCatScreenMovementOverlay() : GetSlotPanelRect(i);
            if (boundsRt == null)
                return false;

            Vector2 boundsHalf = boundsRt.rect.size * 0.5f;
            Vector2 catHalf = catRt.rect.size;
            float collisionScale = CatBoundaryCollisionScale();
            catHalf.x *= Mathf.Abs(catRt.localScale.x) * 0.5f * collisionScale;
            catHalf.y *= Mathf.Abs(catRt.localScale.y) * 0.5f * collisionScale;

            float pad = catBoundaryPadding;
            maxX = Mathf.Max(0f, boundsHalf.x - catHalf.x - pad);
            maxY = Mathf.Max(0f, boundsHalf.y - catHalf.y - pad);
            return maxX > 0.001f && maxY > 0.001f;
        }

        static float InwardNormalAngleDegrees(bool hitLeft, bool hitRight, bool hitBottom, bool hitTop)
        {
            if (hitLeft)
                return 0f;

            if (hitRight)
                return 180f;

            if (hitBottom)
                return 90f;

            if (hitTop)
                return 270f;

            return 0f;
        }

        void ReflectCatDirectionInFan(ref float angleDegrees, bool hitLeft, bool hitRight, bool hitBottom, bool hitTop)
        {
            float baseAngle = InwardNormalAngleDegrees(hitLeft, hitRight, hitBottom, hitTop);
            angleDegrees = baseAngle + Random.Range(-CatBounceFanHalfAngleDegrees, CatBounceFanHalfAngleDegrees);
        }

        void ResolveCatBoundaryBounce(ref Vector2 pos, ref float angleDegrees, ref float rotateSign, float maxX, float maxY)
        {
            bool hitLeft = pos.x < -maxX;
            bool hitRight = pos.x > maxX;
            bool hitBottom = pos.y < -maxY;
            bool hitTop = pos.y > maxY;

            if (!hitLeft && !hitRight && !hitBottom && !hitTop)
                return;

            float overflowX = 0f;
            if (hitLeft)
                overflowX = -maxX - pos.x;
            else if (hitRight)
                overflowX = pos.x - maxX;

            float overflowY = 0f;
            if (hitBottom)
                overflowY = -maxY - pos.y;
            else if (hitTop)
                overflowY = pos.y - maxY;

            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);

            bool useX = overflowX >= overflowY;
            if (useX)
                ReflectCatDirectionInFan(ref angleDegrees, hitLeft, hitRight, false, false);
            else
                ReflectCatDirectionInFan(ref angleDegrees, false, false, hitBottom, hitTop);

            rotateSign = -rotateSign;
        }

        static bool CatNeedsMovementReset(RectTransform catRt, int movementTierApplied, bool tier3ScreenMode)
        {
            if (movementTierApplied > 1)
                return true;

            if (tier3ScreenMode)
                return true;

            if (catRt.localRotation != Quaternion.identity)
                return true;

            return catRt.anchoredPosition.sqrMagnitude > 0.0001f;
        }

        void UpdateCatMovement(int i)
        {
            if (_ctx.IsPractice || !_aliveMask[i])
                return;

            if (!TryGetBinding(i, out SlotUiBindings b) || b.CatAnimator == null)
                return;

            RectTransform catRt = b.CatAnimator.transform as RectTransform;
            if (catRt == null)
                return;

            ref SlotRuntime sr = ref _slots[i];
            int tier = ResolveCatGameplayTier(ref sr);

            if (tier <= 1 || !MaintainingGameplayGauge(ref sr))
            {
                if (CatNeedsMovementReset(catRt, _catMovementTierApplied[i], _catTier3ScreenMode[i]))
                    ResetCatMovementImmediate(i);

                return;
            }

            EnsureCatMovementHomeCaptured(i, catRt);

            if (_catMovementTierApplied[i] <= 1)
                BeginCatMovement(i);

            if (tier >= 3 && !_catTier3ScreenMode[i])
                EnterCatTier3ScreenMode(i, catRt);
            else if (tier == 2 && _catTier3ScreenMode[i])
                RestoreCatToSlotHome(i, catRt);

            _catMovementTierApplied[i] = tier;
            SetCatDrawOnTop(i, true);

            if (!TryGetCatBoundaryHalfExtents(i, tier, catRt, out float maxX, out float maxY))
                return;

            float dt = Time.deltaTime;
            float moveSpeed = CatMoveSpeedForTier(tier);
            float rotateSpeed = CatRotateSpeedForTier(tier);

            float angle = _catMoveAngleDegrees[i];
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = catRt.anchoredPosition;
            pos += new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (moveSpeed * dt);

            float rotateSign = _catMoveRotateSign[i];
            ResolveCatBoundaryBounce(ref pos, ref angle, ref rotateSign, maxX, maxY);

            _catMoveAngleDegrees[i] = angle;
            _catMoveRotateSign[i] = rotateSign;

            catRt.anchoredPosition = pos;

            Vector3 euler = catRt.localEulerAngles;
            euler.z += rotateSpeed * rotateSign * dt;
            catRt.localEulerAngles = euler;
        }
    }
}
