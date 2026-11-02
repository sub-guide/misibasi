using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        static readonly float[] EmptySlotShowcaseBeatTargets =
        {
            0.75f, // 박 1 — 왼쪽
            0.25f, // 박 2 — 오른쪽
            0.75f, // 박 3 — 왼쪽
            0.25f, // 박 4 — 오른쪽
        };

        [Header("빈 슬롯(미참가) 연출")]
        [Tooltip("JOIN하지 않은 슬롯에서 4/4 박자 시소 쇼케이스.")]
        [SerializeField] bool emptySlotShowcaseEnabled = true;

        [Tooltip("4/4 quarter BPM. 빈 슬롯 시소·슬롯 카메라 박자 흔들림 공통. BGM과 맞출 때 Inspector에서 조절.")]
        [SerializeField, Min(1f)]
        float emptySlotShowcaseBpm = DefaultEmptySlotShowcaseBpm;

        [Tooltip("true면 재생 중 BGM time 기준. false면 경과 시간.")]
        [SerializeField] bool emptySlotShowcaseSyncToBgm = true;

        [Tooltip("0.5 ± 이 값으로 4박 목표 x를 만든다. 기본 0.25 → 0.75/0.25.")]
        [SerializeField, Range(0.05f, 0.49f)]
        float emptySlotShowcaseAmplitude = DefaultEmptySlotShowcaseAmplitude;

        bool IsEmptySlotShowcaseActive(int slotIndex)
        {
            if (!emptySlotShowcaseEnabled || _slots == null)
                return false;

            if (slotIndex < 0 || slotIndex >= SlotCount)
                return false;

            if (_participatedMask[slotIndex])
                return false;

            return _flowState == CdFlowState.Playing;
        }

        void SetupEmptySlotShowcaseAtBegin(int i, CoffinDanceSlotBindings bind, ref SlotRuntime sr)
        {
            ResetSeesawToNeutral(ref sr);
            if (bind == null)
                return;

            bind.PrepareAllPoses();
            ApplyEmptySlotShowcaseSeesaw(ref sr);
            ApplyPallbearerPoses(i, ref sr);

            CoffinDanceCoffinBody body = bind.ResolveCoffinBody();
            if (body == null)
                return;

            body.EnsureConfigured();
            body.SetSimulationActive(false);
            body.BeginKinematicHold();
            sr.CoffinShoulderAttached = true;
            SnapAttachedCoffinToShoulders(i, ref sr);
        }

        void TickEmptySlotShowcase(int i, float dt)
        {
            if (!IsEmptySlotShowcaseActive(i))
                return;

            ref SlotRuntime sr = ref _slots[i];
            ApplyEmptySlotShowcaseSeesaw(ref sr);
            ApplyPallbearerPoses(i, ref sr);
            SnapAttachedCoffinToShoulders(i, ref sr);
        }

        void ApplyEmptySlotShowcaseSeesaw(ref SlotRuntime sr)
        {
            float beatPhase = GetEmptySlotShowcaseBeatPhase();
            int beatIndex = Mathf.FloorToInt(beatPhase) % EmptySlotShowcaseBeatTargets.Length;
            if (beatIndex < 0)
                beatIndex += EmptySlotShowcaseBeatTargets.Length;

            int nextIndex = (beatIndex + 1) % EmptySlotShowcaseBeatTargets.Length;
            float tInBeat = beatPhase - Mathf.Floor(beatPhase);
            tInBeat = Mathf.SmoothStep(0f, 1f, tInBeat);

            float x0 = GetEmptySlotShowcaseTargetX(beatIndex);
            float x1 = GetEmptySlotShowcaseTargetX(nextIndex);
            float x = Mathf.Lerp(x0, x1, tInBeat);

            sr.SeesawBias = x;
            sr.SeesawXCurrent = x;
            sr.HoldTimer = 0f;
        }

        float GetEmptySlotShowcaseTargetX(int beatIndexZeroBased)
        {
            float center = xSeesawNeutral;
            float amp = emptySlotShowcaseAmplitude;
            float table = EmptySlotShowcaseBeatTargets[beatIndexZeroBased % EmptySlotShowcaseBeatTargets.Length];
            // 테이블은 0.75/0.5/0.25 패턴. amplitude로 스케일: center + (table - 0.5) * (amp / 0.25)
            float scale = amp / 0.25f;
            return Mathf.Clamp01(center + (table - 0.5f) * scale);
        }

        float GetEmptySlotShowcaseBeatPhase()
        {
            return GetBeatShowcaseBarBeatPhase();
        }
    }
}
