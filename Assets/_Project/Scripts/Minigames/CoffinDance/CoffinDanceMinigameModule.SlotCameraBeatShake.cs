using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        // FailFloor 접촉 시 카메라 연출은 별도 기획 예정 — 본 흔들림과 분리 유지.

        readonly int[] _camShakeBeatIndex = new int[SlotCount];
        readonly float[] _camShakePeakX = new float[SlotCount];
        readonly float[] _camShakePeakY = new float[SlotCount];
        readonly float[] _camShakeCurrentX = new float[SlotCount];
        readonly float[] _camShakeCurrentY = new float[SlotCount];

        bool IsSlotCameraBeatShakeActive()
        {
            return slotCameraBeatShakeEnabled && _flowState == CdFlowState.Playing;
        }

        void ResetSlotCameraBeatShake(int i)
        {
            _camShakeBeatIndex[i] = int.MinValue;
            _camShakePeakX[i] = 0f;
            _camShakePeakY[i] = 0f;
            _camShakeCurrentX[i] = 0f;
            _camShakeCurrentY[i] = 0f;
        }

        void UpdateSlotCameraBeatShake(int i)
        {
            if (!IsSlotCameraBeatShakeActive())
            {
                _camShakeCurrentX[i] = 0f;
                _camShakeCurrentY[i] = 0f;
                return;
            }

            float beats = GetBeatShowcaseGlobalBeats();
            int beatIndex = Mathf.FloorToInt(beats);
            if (beatIndex < 0)
                beatIndex = 0;

            if (_camShakeBeatIndex[i] != beatIndex)
            {
                PickRandomCamShakePeak(i);
                _camShakeBeatIndex[i] = beatIndex;
            }

            float envelope = ComputeSlotCameraShakeReturnEnvelope(beats - beatIndex);
            _camShakeCurrentX[i] = _camShakePeakX[i] * envelope;
            _camShakeCurrentY[i] = _camShakePeakY[i] * envelope;
        }

        /// <summary>0→피크→0. 박 시작·끝(및 펄스 구간 이후)은 rest 오프셋 0.</summary>
        float ComputeSlotCameraShakeReturnEnvelope(float elapsedInBeat)
        {
            float fraction = Mathf.Clamp(slotCameraShakeBeatMoveFraction, 0.05f, 1f);
            if (elapsedInBeat >= fraction)
                return 0f;

            float phase = Mathf.Clamp01(elapsedInBeat / fraction);
            if (slotCameraShakeUseSmoothStep)
                phase = Mathf.SmoothStep(0f, 1f, phase);

            return Mathf.Sin(Mathf.PI * phase);
        }

        void PickRandomCamShakePeak(int i)
        {
            float max = slotCameraShakeMaxOffset;
            if (max <= 0f)
            {
                _camShakePeakX[i] = 0f;
                _camShakePeakY[i] = 0f;
                return;
            }

            float angle = Random.Range(0f, Mathf.PI * 2f);
            _camShakePeakX[i] = Mathf.Cos(angle) * max;
            _camShakePeakY[i] = Mathf.Sin(angle) * max;
        }

        void ApplySlotCameraPoseForNonGameplaySlot(int i)
        {
            if (!IsSlotCameraBeatShakeActive())
                return;

            if (_participatedMask[i] && _aliveMask[i])
                return;

            CoffinDanceSlotBindings bind = GetBindings(i);
            Camera cam = ResolveSlotCamera(bind);
            if (cam == null || _slots == null)
                return;

            if (!_camRestCaptured[i])
                CaptureSlotCameraRestIfNeeded(i, cam);

            ref SlotRuntime sr = ref _slots[i];
            ApplySlotCameraPose(i, cam, ref sr);
        }
    }
}
