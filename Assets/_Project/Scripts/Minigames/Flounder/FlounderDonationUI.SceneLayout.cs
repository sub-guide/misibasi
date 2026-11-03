using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderDonationUI
    {
        /// <summary>전체(시작) 후원창만 씬 배치 유지. 게이지 후원 알림은 슬롯 자동 배치.</summary>
        bool PreserveCenterDonationLayout =>
            FlounderSceneLayoutPreserve.ShouldPreserve || preserveDonationUiSceneLayout;

        void CaptureDonationUiSceneLayout()
        {
            if (FlounderSceneLayoutPreserve.Instance != null)
                FlounderSceneLayoutPreserve.Instance.CaptureAll();
        }

        void RestoreCenterPanelSceneLayout()
        {
            if (!PreserveCenterDonationLayout || centerPopupRoot == null)
                return;

            FlounderSceneLayoutPreserve.Instance?.RestoreHierarchy(centerPopupRoot.transform);
        }
    }
}
