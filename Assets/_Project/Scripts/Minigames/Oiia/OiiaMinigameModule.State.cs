using MiniParty.Core;
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

        readonly Color[] _slotPanelBgRestColor = new Color[SlotCount];

        AudioSource _tierBgmRuntime;

        bool _warnedTimerMissing;
    }
}
