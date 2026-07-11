using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        const string DefaultSlotPanelsContainerName = "Panel_O.I.I.A._4Way";

        [Header("UI (슬롯 0~3)")]
        [Tooltip("OiiaSlotPanel 프리팹 루트. 비우면 `Panel_O.I.I.A._4Way` 직계 자식에서 자동 할당(에디터·플레이).")]
        [SerializeField] OiiaSlotPanelBindings[] slotPanels = new OiiaSlotPanelBindings[4];

        [Tooltip("비우면 플레이 시작 시 `Panel_O.I.I.A._4Way` 직계 자식에서 `OiiaSlotPanelBindings` 를 찾는다.")]
        [SerializeField] Transform slotPanelsContainer;

        void Awake() => ResolveSlotBindingsFromPanels();

        void ResolveSlotBindingsFromPanels()
        {
            OiiaSlotPanelBindings[] panels = CollectSlotPanels();
            int n = panels != null ? panels.Length : 0;

            if (n == 0)
            {
                bindings = new SlotUiBindings[4];
                Debug.LogError(
                    "[OiiaMinigameModule] slotPanels 가 비어 있습니다. " +
                    "씬의 `Panel_O.I.I.A._4Way` 아래 OiiaSlotPanel 4개에 `OiiaSlotPanelBindings` 가 있는지 확인하세요.",
                    this);
                return;
            }

            bindings = new SlotUiBindings[n];
            for (var i = 0; i < n; i++)
            {
                OiiaSlotPanelBindings panel = panels[i];
                bindings[i] = panel != null ? panel.ToSlotUiBindings() : new SlotUiBindings();
            }
        }

        OiiaSlotPanelBindings[] CollectSlotPanels()
        {
            if (slotPanels != null && slotPanels.Length > 0 && !HasAnyNull(slotPanels))
                return slotPanels;

            Transform root = slotPanelsContainer;
            if (root == null)
            {
                var containerGo = GameObject.Find(DefaultSlotPanelsContainerName);
                if (containerGo != null)
                    root = containerGo.transform;
            }

            if (root == null)
                return slotPanels;

            var found = new OiiaSlotPanelBindings[root.childCount];
            var count = 0;
            for (var c = 0; c < root.childCount; c++)
            {
                var panel = root.GetChild(c).GetComponent<OiiaSlotPanelBindings>();
                if (panel == null)
                    continue;

                if (count >= found.Length)
                    break;

                found[count++] = panel;
            }

            if (count == 0)
                return slotPanels;

            var trimmed = new OiiaSlotPanelBindings[count];
            for (var i = 0; i < count; i++)
                trimmed[i] = found[i];

            return trimmed;
        }

        static bool HasAnyNull(OiiaSlotPanelBindings[] panels)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                if (panels[i] == null)
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
                return;

            TryAutoAssignSlotPanelsEditor();
        }

        void TryAutoAssignSlotPanelsEditor()
        {
            if (slotPanels != null && slotPanels.Length == 4 && !HasAnyNull(slotPanels))
                return;

            Transform root = slotPanelsContainer;
            if (root == null)
            {
                var containerGo = GameObject.Find(DefaultSlotPanelsContainerName);
                if (containerGo != null)
                    root = containerGo.transform;
            }

            if (root == null)
                return;

            var list = new System.Collections.Generic.List<OiiaSlotPanelBindings>(4);
            for (var c = 0; c < root.childCount; c++)
            {
                var panel = root.GetChild(c).GetComponent<OiiaSlotPanelBindings>();
                if (panel != null)
                    list.Add(panel);
            }

            if (list.Count != 4)
                return;

            slotPanels = list.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
