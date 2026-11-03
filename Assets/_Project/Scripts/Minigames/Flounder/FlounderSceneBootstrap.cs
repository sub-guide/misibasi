using MiniParty.Core;
using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed class FlounderSceneBootstrap : MonoBehaviour
    {
        [SerializeField] FlounderMinigameModule module;
        [SerializeField] bool debugSoloWithoutPartySession;
        [SerializeField] int debugActivePlayerCount = 1;

        void Start()
        {
            if (module == null)
                module = GetComponent<FlounderMinigameModule>();

            if (debugSoloWithoutPartySession)
            {
                Debug.LogWarning(
                    "[FlounderSceneBootstrap] debugSoloWithoutPartySession — " +
                    $"디버그 인원 {debugActivePlayerCount}명으로 시작합니다.");
                BeginDebug();
                return;
            }

            PartySession session = PartySession.Instance;
            if (session == null)
            {
                Debug.LogError("[FlounderSceneBootstrap] PartySession 이 없습니다.");
                enabled = false;
                return;
            }

            if (module == null)
            {
                Debug.LogError("[FlounderSceneBootstrap] FlounderMinigameModule 을 찾을 수 없습니다.");
                enabled = false;
                return;
            }

            var ctx = new MinigameContext(
                session.Slots,
                session.LastPractice,
                report => session.EndMinigameAndOpenResultScene(report));

            module.Begin(ctx);
        }

        void BeginDebug()
        {
            if (module == null)
                return;

            var slots = new PlayerSlotModel[4];
            for (var i = 0; i < 4; i++)
            {
                slots[i] = new PlayerSlotModel(i, 3);
                if (i < debugActivePlayerCount)
                    slots[i].EnterPlaying();
            }

            module.Begin(new MinigameContext(slots, false, _ => { }));
        }

        void Update()
        {
            if (module != null)
                module.Tick();
        }
    }
}
