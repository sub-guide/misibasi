using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.UI.ControllerButtons
{
    /// <summary>
    /// D-Pad 중앙 허브 1장. 팔은 <see cref="SnesControllerButtonVisual"/> + crop SpriteSet.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class SnesDpadBaseVisual : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] Sprite center;

        public Image Icon => icon;
        public Sprite Center => center;

        void Awake()
        {
            if (icon == null)
                icon = GetComponent<Image>();

            ApplyCenter();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (icon == null)
                icon = GetComponent<Image>();

            if (!Application.isPlaying)
                ApplyCenter();
        }
#endif

        public void SetCenterSprite(Sprite sprite)
        {
            center = sprite;
            ApplyCenter();
        }

        void ApplyCenter()
        {
            if (icon == null || center == null)
                return;

            icon.sprite = center;
        }
    }
}
