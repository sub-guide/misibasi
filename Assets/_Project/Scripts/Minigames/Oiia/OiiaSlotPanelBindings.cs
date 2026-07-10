using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// <see cref="OiiaSlotPanel"/> 프리팹 루트. 자식 이름 규칙으로 UI 참조를 채우고
    /// <see cref="OiiaMinigameModule"/> 에 전달한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OiiaSlotPanelBindings : MonoBehaviour
    {
        public TMP_Text SequenceText;
        public Slider GaugeSlider;
        public Image Blur;
        public TMP_Text ScoreText;
        public TMP_Text PracticeReadyText;
        public Animator CatAnimator;
        public Image SlotPanelBackgroundImage;
        public TMP_Text WaitingText;

        [Header("컨트롤러 가이드 UI")]
        public GameObject ControllerGuideRoot;
        public Image ControllerBodyImage;
        public Image GuideButtonY;
        public Image GuideButtonX;
        public Image GuideButtonA;
        public Image GuideButtonB;

        [Header("셔플 이펙트")]
        public Image ShuffleEffect;

        [Header("코믹스 BurstText 풀")]
        public TMP_Text[] BurstTextPool;
        public RectTransform BurstTextContainer;

        public OiiaMinigameModule.SlotUiBindings ToSlotUiBindings() =>
            new()
            {
                SequenceText = SequenceText,
                GaugeSlider = GaugeSlider,
                Blur = Blur,
                ScoreText = ScoreText,
                PracticeReadyText = PracticeReadyText,
                CatAnimator = CatAnimator,
                SlotPanelBackgroundImage = SlotPanelBackgroundImage,
                WaitingText = WaitingText,
                ControllerGuideRoot = ControllerGuideRoot,
                ControllerBodyImage = ControllerBodyImage,
                GuideButtonY = GuideButtonY,
                GuideButtonX = GuideButtonX,
                GuideButtonA = GuideButtonA,
                GuideButtonB = GuideButtonB,
                ShuffleEffect = ShuffleEffect,
                BurstTextPool = BurstTextPool,
                BurstTextContainer = BurstTextContainer,
            };

        void Awake()
        {
            AutoWireFromHierarchy();
            HideIdleBurstTextPoolInHierarchy();
        }

        void OnValidate() => AutoWireFromHierarchy();

        public void AutoWireFromHierarchy()
        {
            Transform root = transform;

            SequenceText = GetDirectChildComponent<TMP_Text>(root, "Sequence");
            GaugeSlider = GetDirectChildComponent<Slider>(root, "Gauge");
            ScoreText = GetDirectChildComponent<TMP_Text>(root, "Score");
            Blur = GetDirectChildComponent<Image>(root, "Blur");
            PracticeReadyText = GetDirectChildComponent<TMP_Text>(root, "Ready");
            CatAnimator = GetDirectChildComponent<Animator>(root, "Cat");
            WaitingText = GetDirectChildComponent<TMP_Text>(root, "Waiting");
            SlotPanelBackgroundImage = GetComponent<Image>();

            Transform guideRoot = root.Find("ControllerGuide");
            if (guideRoot != null)
            {
                ControllerGuideRoot = guideRoot.gameObject;
                ControllerBodyImage = GetDirectChildComponent<Image>(guideRoot, "Body");
                GuideButtonY = GetDirectChildComponent<Image>(guideRoot, "BtnY");
                GuideButtonX = GetDirectChildComponent<Image>(guideRoot, "BtnX");
                GuideButtonA = GetDirectChildComponent<Image>(guideRoot, "BtnA");
                GuideButtonB = GetDirectChildComponent<Image>(guideRoot, "BtnB");
            }

            Transform shuffleEffect = root.Find("ShuffleEffect");
            if (shuffleEffect == null)
                shuffleEffect = root.Find("ShuffleMapOverlay");

            if (shuffleEffect != null)
                ShuffleEffect = shuffleEffect.GetComponent<Image>();

            Transform burstContainer = root.Find("BurstTextContainer");
            BurstTextContainer = burstContainer != null ? burstContainer as RectTransform : null;

            if (BurstTextPool == null || BurstTextPool.Length == 0)
                CollectBurstTextPool(root);
        }

        void HideIdleBurstTextPoolInHierarchy()
        {
            if (BurstTextPool == null || BurstTextPool.Length == 0)
                CollectBurstTextPool(transform);

            if (BurstTextPool == null)
                return;

            for (var i = 0; i < BurstTextPool.Length; i++)
            {
                TMP_Text tmp = BurstTextPool[i];
                if (tmp == null)
                    continue;

                tmp.gameObject.SetActive(false);
                tmp.text = string.Empty;
            }
        }

        void CollectBurstTextPool(Transform root)
        {
            var list = new System.Collections.Generic.List<TMP_Text>(OiiaMinigameModule.BurstTextPoolSize);
            CollectBurstTextRecursive(BurstTextContainer != null ? BurstTextContainer : root, list);

            if (list.Count > 0)
                BurstTextPool = list.ToArray();
        }

        static void CollectBurstTextRecursive(Transform node, System.Collections.Generic.List<TMP_Text> list)
        {
            if (node.name.StartsWith("BurstText"))
            {
                TMP_Text tmp = node.GetComponent<TMP_Text>();
                if (tmp != null && !list.Contains(tmp))
                    list.Add(tmp);
            }

            for (var c = 0; c < node.childCount; c++)
                CollectBurstTextRecursive(node.GetChild(c), list);
        }

        static T GetDirectChildComponent<T>(Transform root, string childName) where T : Component
        {
            Transform child = root.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }
    }
}
