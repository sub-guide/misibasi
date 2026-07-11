using MiniParty.Core;
using MiniParty.Input;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        MinigameContext _ctx;

        SlotUiBindings[] bindings;

        SlotRuntime[] _slots;
        bool _running;

        float _remainingMainTime;

        readonly bool[] _aliveMask = new bool[SlotCount];

        readonly bool[] _practiceReady = new bool[SlotCount];

        readonly Color[] _slotPanelBgRestColor = new Color[SlotCount];

        readonly int[] _blurRestSiblingIndex = { -1, -1, -1, -1 };

        readonly OperatorInputService _operatorInput = new();

        AudioSource _tierBgmRuntime;

        bool _warnedTimerMissing;
    }
}
