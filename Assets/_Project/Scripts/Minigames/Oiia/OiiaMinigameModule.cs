using MiniParty.Core;
using MiniParty.Flow;
using MiniParty.Input;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// OIIA 미니게임 모듈. 역할별 partial 파일로 분리되어 있다.
    /// <list type="bullet">
    /// <item><description><see cref="OiiaMinigameModule.Begin"/> — 시작·슬롯 초기화</description></item>
    /// <item><description><see cref="OiiaMinigameModule.Tick"/> — 매 프레임 흐름</description></item>
    /// <item><description>Gameplay / Ui / PatternAudio / TierBgm / … — 세부 역할</description></item>
    /// </list>
    /// 부스 패드 매핑: O=Trigger(X), I=Button2(A), A=Button4(Y), B=Button3 — <see cref="BoothUsbGamepadLayout"/>.
    /// 루프 완주 시 3종 중 셔플 매핑 + 셔플 이펙트. 상세는 <c>ButtonShuffle.cs</c> · <c>ShuffleEffect.cs</c>.
    /// 디제잉 레이브 개편: 1단계 UI 바인딩(<see cref="OiiaDjPadButtonId"/> · DjBox) — 게임플레이는 후속 단계.
    /// </summary>
    public sealed partial class OiiaMinigameModule : MonoBehaviour, IMinigameModule
    {
        public const string BuiltInId = "oiia";

        public string Id => BuiltInId;
        public string DisplayName => displayName;
    }
}
