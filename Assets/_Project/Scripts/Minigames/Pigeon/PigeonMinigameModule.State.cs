namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        MinigameContext _ctx;
        bool _running;
        readonly bool[] _participatedMask = new bool[SlotCount];
    }
}
