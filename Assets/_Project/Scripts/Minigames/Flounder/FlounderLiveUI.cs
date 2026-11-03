using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed class FlounderLiveUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI liveLabel;
        [SerializeField] int baseViewerCount = 12000;
        [SerializeField] int viewerGainPerMash = 3;
        [SerializeField] int viewerGainPerDonation = 120;

        int _viewers;

        public void ResetViewers()
        {
            _viewers = baseViewerCount;
            Refresh();
        }

        public void RegisterMash(int count = 1)
        {
            _viewers += viewerGainPerMash * count;
            Refresh();
        }

        public void RegisterDonation()
        {
            _viewers += viewerGainPerDonation;
            Refresh();
        }

        void Refresh()
        {
            if (liveLabel != null)
                liveLabel.text = "LIVE  " + _viewers.ToString("N0");
        }
    }
}
