using MiniParty.Flow;
using MiniParty.Minigames;
using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    /// <summary>
    /// Rhythm Button Challenge 전용 씬 루트에 붙임. PartySession 슬롯으로 컨텍스트 생성 후 모듈 구동.
    /// </summary>
    public sealed class RhythmButtonChallengeSceneBootstrap : MonoBehaviour
    {
        [SerializeField] RhythmButtonChallengeMinigameModule module;

        void Start()
        {
            PartySession session = PartySession.Instance;
            if (session == null)
            {
                Debug.LogError("[RhythmButtonChallengeSceneBootstrap] PartySession 이 없습니다. 메인 씬에 PartySession 을 두고 진입하세요.");
                enabled = false;
                return;
            }

            if (module == null)
                module = GetComponent<RhythmButtonChallengeMinigameModule>() ??
                         FindObjectOfType<RhythmButtonChallengeMinigameModule>();

            if (module == null)
            {
                Debug.LogError("[RhythmButtonChallengeSceneBootstrap] RhythmButtonChallengeMinigameModule 을 찾을 수 없습니다.");
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
