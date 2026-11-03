using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    [ExecuteAlways]
    public sealed class FlounderBackgroundFit : MonoBehaviour
    {
        [SerializeField] Camera targetCamera;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] bool respectSceneLayoutPreserve = true;

        void OnEnable() => Fit();
        void LateUpdate() => Fit();

        public void Fit()
        {
            if (respectSceneLayoutPreserve && FlounderSceneLayoutPreserve.ShouldPreserve)
                return;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return;

            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null || !cam.orthographic)
                return;

            float viewHeight = cam.orthographicSize * 2f;
            float viewWidth = viewHeight * cam.aspect;
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            float scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);
            transform.localScale = new Vector3(scale, scale, 1f);
            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        }
    }
}
