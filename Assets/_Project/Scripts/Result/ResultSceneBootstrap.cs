using MiniParty.Flow;
using UnityEngine;

namespace MiniParty.Result
{
    /// <summary>Result 씬 진입 시 PartySession 리포트를 소비하고 <see cref="ResultFlowController"/> 를 시작한다.</summary>
    public sealed class ResultSceneBootstrap : MonoBehaviour
    {
        [SerializeField] ResultFlowController flowController;

        void Start()
        {
            if (flowController == null)
                flowController = FindObjectOfType<ResultFlowController>();

            if (flowController == null)
            {
                Debug.LogError("[ResultSceneBootstrap] ResultFlowController 를 찾을 수 없습니다.", this);
                return;
            }

            PartySession session = PartySession.Instance;
            if (session == null)
            {
                Debug.LogError("[ResultSceneBootstrap] PartySession 이 없습니다. 메인 씬에서 진입하세요.", this);
                return;
            }

            flowController.Begin(session);
        }
    }
}
