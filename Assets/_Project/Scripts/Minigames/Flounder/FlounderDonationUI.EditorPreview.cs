#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        [Header("에디터 Scene 미리보기")]
        [SerializeField] bool previewStartDonationInEditMode = true;

        void OnValidate()
        {
            if (Application.isPlaying)
                return;

            EditorApplication.delayCall += DelayedEditorPreview;
        }

        void DelayedEditorPreview()
        {
            EditorApplication.delayCall -= DelayedEditorPreview;
            if (this == null || Application.isPlaying)
                return;

            RefreshStartDonationEditorPreview();
        }

        /// <summary>Scene 뷰에서 전체 후원창 레이아웃을 런타임과 같이 표시합니다 (플레이 중에는 호출하지 않음).</summary>
        public void RefreshStartDonationEditorPreview()
        {
            if (Application.isPlaying)
                return;

            if (!previewStartDonationInEditMode)
            {
                HideCenterPopup();
                return;
            }

            if (centerPopupRoot == null)
                return;

            CaptureDonationUiSceneLayout();

            EnsureCenterTaglineFont();
            EnsureStartDonationSprite();
            if (!PreserveCenterDonationLayout)
                LayoutStarBalloonTextAnchors();
            ApplyDonationTypography();

            centerPopupRoot.SetActive(true);
            ApplyStartDonationVisuals(50000, centerTaglineText);
            RestoreCenterPanelSceneLayout();

            if (centerCanvasGroup != null)
                SetCanvasAlpha(centerCanvasGroup, 1f);

            if (gameplayAlertRoot != null)
                gameplayAlertRoot.SetActive(false);

            SceneView.RepaintAll();
        }
    }
}
#endif
