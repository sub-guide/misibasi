using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    /// <summary>
    /// Oiia 전용 씬 루트에 붙임. PartySession 슬롯으로 컨텍스트 생성 후 모듈 구동.
    /// </summary>
    public sealed class OiiaSceneBootstrap : MonoBehaviour
    {
        [SerializeField] OiiaMinigameModule module;

        void Start()
        {
            PartySession session = PartySession.Instance;
            if (session == null)
            {
                Debug.LogError("[OiiaSceneBootstrap] PartySession 이 없습니다. 메인 씬에 PartySession 을 두고 진입하세요.");
                enabled = false;
                return;
            }

            if (module == null)
                module = GetComponent<OiiaMinigameModule>() ?? FindObjectOfType<OiiaMinigameModule>();

            if (module == null)
            {
                Debug.LogError("[OiiaSceneBootstrap] OiiaMinigameModule 을 찾을 수 없습니다.");
                enabled = false;
                return;
            }

            var ctx = new MinigameContext(session.Slots, session.LastPractice, report => session.EndMinigameAndOpenResultScene(report));
            module.Begin(ctx);
        }

        void Update()
        {
            if (module != null)
                module.Tick();
        }
    }
}
