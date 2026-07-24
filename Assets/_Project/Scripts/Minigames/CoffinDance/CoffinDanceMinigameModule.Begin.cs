using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        public void Begin(MinigameContext context)
        {
            _ctx = context;
            gameObject.SetActive(true);
            _running = true;
            _completing = false;
            _flowState = CdFlowState.Playing;
            _endDelayRemain = 0f;

            _elapsedMainTime = 0f;
            _remainingMainTime = MainDurationSeconds;
            _rng = new System.Random(Random.Range(int.MinValue, int.MaxValue));

            _jumpPromptState = CdJumpPromptState.Idle;
            _jumpPromptRemain = 0f;
            _jumpRequiredPresses = 1;
            _jumpIsDouble = false;

            _slots = new SlotRuntime[SlotCount];

            var slotCameras = new System.Collections.Generic.HashSet<Camera>();

            ForEachSlot(i =>
            {
                bool play = _ctx.Slots != null &&
                            i < _ctx.Slots.Length &&
                            _ctx.Slots[i].State == SlotState.PLAYING;

                _participatedMask[i] = play;
                _aliveMask[i] = play;
                _practiceReady[i] = false;

                ref SlotRuntime sr = ref _slots[i];
                sr = default;

                CoffinDanceSlotBindings bind = GetBindings(i);
                Camera slotCam = ResolveSlotCamera(bind);
                if (slotCam != null)
                    slotCameras.Add(slotCam);

                SetupSlotView(i, bind, slotCam);

                if (!play)
                {
                    if (slotCam != null)
                        slotCam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
                    HideJumpPrompt(i);
                    SetEliminatedUi(i, false);
                    return;
                }

                float sign = (_rng.Next(0, 2) == 0) ? -1f : 1f;
                sr.ThetaRad = sign * initialTiltDegrees * Mathf.Deg2Rad;
                sr.Omega = -sign * initialAngularSpeed;
                sr.ScoreExact = 0f;
                sr.ScoreSum = 0;
                sr.Eliminated = false;
                sr.StumbleTimer = 0f;
                sr.InStumble = false;
                sr.JumpLockoutRemain = 0f;
                sr.JumpSucceededThisPrompt = false;
                sr.JumpPressesThisPrompt = 0;
                sr.JumpVisualT = 0f;

                if (bind != null)
                {
                    Transform coffin = ResolveCoffinTransform(bind);
                    if (coffin != null)
                    {
                        sr.CoffinBaseLocalPos = coffin.localPosition;
                        sr.HasCoffinBase = true;
                    }

                    CachePallbearerBases(ref sr, bind);
                    ApplyTiltVisual(i, ref sr);
                }

                HideJumpPrompt(i);
                SetEliminatedUi(i, false);
            });

            if (disableMainCameraOnBegin && slotCameras.Count > 0)
                DisableSceneMainCamera(slotCameras);

            RefreshPhaseParameters();
            ScheduleNextJump(immediate: true);
            FlushAllUi();
        }

        static Camera ResolveSlotCamera(CoffinDanceSlotBindings bind)
        {
            if (bind == null)
                return null;

            if (bind.SlotCamera != null)
                return bind.SlotCamera;

            // Inspector 미연결 시 자식 Camera 자동 탐색 (SlotCamera_1P 등)
            bind.SlotCamera = bind.GetComponentInChildren<Camera>(true);
            return bind.SlotCamera;
        }

        void SetupSlotView(int slotIndex, CoffinDanceSlotBindings bind, Camera slotCam)
        {
            if (bind == null)
                return;

            Transform root = bind.transform;
            Vector3 p = root.position;
            p.x = slotIndex * slotWorldSpacing;
            p.y = 0f;
            p.z = 0f;
            root.position = p;

            ApplyCameraViewport(slotIndex, slotCam);

            if (bindSlotCanvasesToSlotCamera)
                BindSlotCanvasesToCamera(bind, slotCam);
        }

        void ApplyCameraViewport(int slotIndex, Camera cam)
        {
            if (cam == null)
                return;

            float w = 1f / SlotCount;
            cam.rect = new Rect(slotIndex * w, 0f, w, 1f);
            cam.depth = 10 + slotIndex;
            cam.enabled = true;
            cam.clearFlags = CameraClearFlags.SolidColor;

            var listener = cam.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = false;
        }

        void BindSlotCanvasesToCamera(CoffinDanceSlotBindings bind, Camera slotCam)
        {
            if (bind == null || slotCam == null)
                return;

            Canvas[] canvases = bind.GetComponentsInChildren<Canvas>(true);
            for (var c = 0; c < canvases.Length; c++)
            {
                Canvas canvas = canvases[c];
                if (canvas == null)
                    continue;

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = slotCam;
                canvas.planeDistance = Mathf.Max(1f, slotCam.nearClipPlane + 0.5f);
                canvas.sortingOrder = 20 + (10 * ArrayIndexOfBindings(bind));
            }
        }

        int ArrayIndexOfBindings(CoffinDanceSlotBindings bind)
        {
            if (slotBindings == null)
                return 0;

            for (var i = 0; i < slotBindings.Length; i++)
            {
                if (slotBindings[i] == bind)
                    return i;
            }

            return 0;
        }

        static void DisableSceneMainCamera(System.Collections.Generic.HashSet<Camera> keepEnabled)
        {
            Camera main = Camera.main;
            if (main != null && (keepEnabled == null || !keepEnabled.Contains(main)))
            {
                main.enabled = false;
                return;
            }

            var tagged = GameObject.FindWithTag("MainCamera");
            if (tagged == null)
                return;

            var cam = tagged.GetComponent<Camera>();
            if (cam != null && (keepEnabled == null || !keepEnabled.Contains(cam)))
                cam.enabled = false;
        }
    }
}
