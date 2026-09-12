using System.Collections.Generic;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        enum PourPhase
        {
            Cooldown,
            Forward,
            Spawning,
            Reverse
        }

        enum PeckPhase
        {
            Idle,
            Forward,
            Reverse
        }

        struct FloorPile
        {
            public GameObject Go;
            public Collider2D Col;
            public SpriteRenderer Sr;
        }

        MinigameContext _ctx;
        bool _running;
        readonly bool[] _participatedMask = new bool[SlotCount];

        bool _pourArmed;
        PourPhase _pourPhase;
        float _pourElapsed;
        float _pourClipDuration;
        int _nextSortingOrder;

        Vector2[] _pourJitters;
        int _pourJitterCount;
        int _pourJitterIndex;

        readonly List<FloorPile> _floorPiles = new List<FloorPile>();
        readonly PeckPhase[] _peckPhase = new PeckPhase[SlotCount];
        readonly bool[] _peckFromRight = new bool[SlotCount];
        readonly float[] _peckElapsed = new float[SlotCount];
        readonly float[] _peckClipDuration = new float[SlotCount];
        readonly int[] _score = new int[SlotCount];
        readonly Collider2D[] _peckOverlap = new Collider2D[24];
    }
}
