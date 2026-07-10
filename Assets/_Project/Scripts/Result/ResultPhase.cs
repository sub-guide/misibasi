namespace MiniParty.Result
{
    /// <summary>Result 씬 연출 단계. 1단계(MVP)는 <see cref="IntroFade"/> 만 사용한다.</summary>
    public enum ResultPhase
    {
        IntroFade,
        RankingReveal,
        HpProcess,
        GameOver,
        Ready
    }
}
