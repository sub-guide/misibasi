using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    public sealed partial class CoffinDanceMinigameModule
    {
        float GetBuildUpDurationSeconds() => Mathf.Max(0f, buildUpDurationSeconds);

        bool ShouldRunBuildUpAtBegin() => buildUpRect != null && GetBuildUpDurationSeconds() > 0f;

        void ConfigureFlowStateAtBegin()
        {
            RestoreBuildUpHiddenUi();

            if (ShouldRunBuildUpAtBegin())
            {
                _flowState = CdFlowState.BuildUp;
                _buildUpElapsed = 0f;
                BeginBuildUpVisual();
                return;
            }

            _flowState = CdFlowState.Playing;
            ResetBuildUpVisual();
        }

        void BeginBuildUpVisual()
        {
            SuppressBuildUpHiddenUi();

            if (buildUpRect == null)
                return;

            buildUpRect.gameObject.SetActive(true);
            SetBuildUpAnchoredX(buildUpStartX);
        }

        void ResetBuildUpVisual()
        {
            RestoreBuildUpHiddenUi();

            if (buildUpRect == null)
                return;

            SetBuildUpAnchoredX(buildUpStartX);
            buildUpRect.gameObject.SetActive(false);
        }

        void TickBuildUp(float dt)
        {
            float duration = GetBuildUpDurationSeconds();
            _buildUpElapsed += Mathf.Max(0f, dt);

            float u = duration <= 0f ? 1f : Mathf.Clamp01(_buildUpElapsed / duration);
            float x = Mathf.Lerp(buildUpStartX, buildUpEndX, u);
            SetBuildUpAnchoredX(x);

            if (_buildUpElapsed >= duration)
                EndBuildUpAndEnterPlaying();
        }

        void EndBuildUpAndEnterPlaying()
        {
            if (buildUpRect != null)
            {
                SetBuildUpAnchoredX(buildUpStartX);
                buildUpRect.gameObject.SetActive(false);
            }

            RestoreBuildUpHiddenUi();
            _flowState = CdFlowState.Playing;
            _buildUpElapsed = 0f;
        }

        void SuppressBuildUpHiddenUi()
        {
            RestoreBuildUpHiddenUi();

            if (buildUpRect != null && buildUpRect.parent != null)
            {
                Transform shared = buildUpRect.parent;
                for (var c = 0; c < shared.childCount; c++)
                {
                    Transform child = shared.GetChild(c);
                    if (child == buildUpRect)
                        continue;

                    RememberAndHideUiRoot(child.gameObject);
                }
            }
            else if (mainRoundTimerCentralTop != null)
            {
                RememberAndHideUiRoot(mainRoundTimerCentralTop.gameObject);
            }

            ForEachSlot(i =>
            {
                CoffinDanceSlotBindings bind = GetBindings(i);
                if (bind == null)
                    return;

                Canvas[] canvases = bind.GetComponentsInChildren<Canvas>(true);
                for (var c = 0; c < canvases.Length; c++)
                {
                    Canvas canvas = canvases[c];
                    if (canvas == null)
                        continue;

                    RememberAndHideUiRoot(canvas.gameObject);
                }
            });
        }

        void RememberAndHideUiRoot(GameObject root)
        {
            if (root == null)
                return;

            for (var i = 0; i < _buildUpUiRestore.Count; i++)
            {
                if (_buildUpUiRestore[i].Root == root)
                    return;
            }

            _buildUpUiRestore.Add(new BuildUpUiRestoreEntry
            {
                Root = root,
                WasActive = root.activeSelf,
            });
            root.SetActive(false);
        }

        void RestoreBuildUpHiddenUi()
        {
            for (var i = 0; i < _buildUpUiRestore.Count; i++)
            {
                BuildUpUiRestoreEntry entry = _buildUpUiRestore[i];
                if (entry.Root != null)
                    entry.Root.SetActive(entry.WasActive);
            }

            _buildUpUiRestore.Clear();
        }

        void SetBuildUpAnchoredX(float x)
        {
            if (buildUpRect == null)
                return;

            Vector2 pos = buildUpRect.anchoredPosition;
            pos.x = x;
            buildUpRect.anchoredPosition = pos;
        }
    }
}
