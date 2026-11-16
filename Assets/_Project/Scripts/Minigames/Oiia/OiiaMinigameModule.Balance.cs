namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>`oiiaiooiiiai` 12글자를 채운 입력에 추가로 더하는 점수. 티어와 무관.</summary>
        const int PatternCompleteBonusScore = 500;

        const int HpLowScoreThresholdDefault = 8_000;

        int HpLowScoreThreshold => HpLowScoreThresholdDefault;
    }
}
