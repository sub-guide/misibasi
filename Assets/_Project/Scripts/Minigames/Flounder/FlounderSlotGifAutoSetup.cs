using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    /// <summary>
    /// 2P·3P 캐릭터 GIF를 1P와 동일한 GifVisual·FlounderDance 설정으로 구성합니다.
    /// </summary>
    public sealed class FlounderSlotGifAutoSetup : MonoBehaviour
    {
        [SerializeField] string character2PName = "Character_2P";
        [SerializeField] string character3PName = "Character_3P";
        [SerializeField] string frames2PResourceFolder = "Flounder/Player2P_Frames";
        [SerializeField] string frames3PResourceFolder = "Flounder/Player3P_Frames";
        [SerializeField] int frameCount = 5;

        static readonly Vector3 GifVisualLocalPosition = new(-2.57f, 0.93f, 0f);
        static readonly Vector3 GifVisualLocalScale = new(1.5f, 1.5f, 1.5f);

        void Awake()
        {
            FlounderDance reference = FindReferenceDance();
            SetupCharacter(character2PName, 1, frames2PResourceFolder, reference);
            SetupCharacter(character3PName, 2, frames3PResourceFolder, reference);
        }

        static FlounderDance FindReferenceDance()
        {
            FlounderPlayerSlotView[] slots = FindObjectsOfType<FlounderPlayerSlotView>(true);
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].SlotIndex == 0 && slots[i].Dance != null)
                    return slots[i].Dance;
            }

            return FindObjectOfType<FlounderDance>(true);
        }

        void SetupCharacter(string rootName, int slotIndex, string resourceFolder, FlounderDance reference)
        {
            Transform root = GameObject.Find(rootName)?.transform;
            if (root == null)
                return;

            Sprite[] frames = LoadFrames(resourceFolder);
            if (frames.Length == 0)
                return;

            Transform visual = root.Find("GifVisual");
            bool createdVisual = visual == null;
            if (createdVisual)
            {
                var visualGo = new GameObject("GifVisual");
                visualGo.transform.SetParent(root, false);
                visual = visualGo.transform;
            }

            if (!FlounderSceneLayoutPreserve.ShouldPreserve || createdVisual)
            {
                visual.localPosition = GifVisualLocalPosition;
                visual.localScale = GifVisualLocalScale;
            }

            var renderer = visual.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = visual.gameObject.AddComponent<SpriteRenderer>();

            renderer.sortingOrder = 25;
            renderer.sprite = frames[0];

            FlounderDance dance = root.GetComponent<FlounderDance>();
            if (dance == null)
                dance = root.gameObject.AddComponent<FlounderDance>();

            dance.SetFrames(frames);
            if (reference != null)
                dance.CopyMotionFrom(reference);

            WireSlotView(slotIndex, root, dance);
        }

        static void WireSlotView(int slotIndex, Transform characterRoot, FlounderDance dance)
        {
            FlounderPlayerSlotView[] slots = FindObjectsOfType<FlounderPlayerSlotView>(true);
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].SlotIndex != slotIndex)
                    continue;

                if (slots[i].CharacterRoot != characterRoot)
                    continue;

                if (slots[i].Dance == null)
                    slots[i].BindDance(dance);

                break;
            }
        }

        Sprite[] LoadFrames(string resourceFolder)
        {
            var frames = new Sprite[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                string path = $"{resourceFolder}/frame_{i:D3}";
                frames[i] = Resources.Load<Sprite>(path);
                if (frames[i] == null)
                {
                    Debug.LogWarning("[FlounderSlotGifAutoSetup] 프레임 없음: " + path);
                    return System.Array.Empty<Sprite>();
                }
            }

            return frames;
        }
    }
}
