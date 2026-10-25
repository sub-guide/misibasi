namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>버튼 입력 1회 점수.</summary>
        const int DjHitScore = 100;

        /// <summary>`oiiaiooiiiai` 12글자를 채운 입력에 추가로 더하는 점수.</summary>
        const int PatternCompleteBonusScore = 500;

        const int HpLowScoreThresholdDefault = 8_000;

        int HpLowScoreThreshold => HpLowScoreThresholdDefault;
    }
}
