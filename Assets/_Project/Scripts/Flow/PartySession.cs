using MiniParty.Core;
using MiniParty.Minigames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniParty.Flow
{
    /// <summary>
    /// 메인 메뉴 씬이 바뀌어도 유지되는 슬롯 상태·라운드 준비 정보·복귀 시 결과 보관.
    /// 메인 씬에 한 번 두고(빈 오브젝트), DontDestroyOnLoad로 보존된다.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class PartySession : MonoBehaviour
    {
        public static PartySession Instance { get; private set; }

        [SerializeField] int startingHp = 3;

        [Tooltip("미니게임 종료 후 돌아올 씬 이름. File > Build Settings에 동일 이름으로 등록할 것.")]
        [SerializeField] string mainMenuSceneName = "MainMenu";

        [Tooltip("미니게임 종료 후 결과 연출 씬. File > Build Settings에 동일 이름으로 등록할 것.")]
        [SerializeField] string resultSceneName = "Result";

        PlayerSlotModel[] _slots;

        /// <summary>다른 스크립트 Awake 순서와 관계없이 첫 접근 시 슬롯 배열을 만든다.</summary>
        public PlayerSlotModel[] Slots
        {
            get
            {
                EnsureSlotsAllocated();
                return _slots;
            }
        }

        public int StartingHp => startingHp;

        bool _lastPractice;
        readonly bool[] _lastPlayed = new bool[4];

        MinigameSessionReport _pendingReport;
        bool _hasPending;

        MinigameSessionReport _postResultReport;
        bool _hasPostResultReport;

        public bool LastPractice => _lastPractice;

        public string ResultSceneName => resultSceneName;

        public bool[] LastPlayedMask => _lastPlayed;

        public bool HasPendingReport => _hasPending;

        /// <summary>
        /// Oiia: 연습 세션이 끝나면 true로 설정. 다음 메뉴에서 Enter로 진입할 때 본게임 1회로 소비된다.
        /// 본게임까지 끝나면 <see cref="ResetOiiaCycleAfterMainSession"/> 으로 다시 연습부터.
        /// </summary>
        bool _oiiaMainRoundQueuedAfterPractice;

        /// <summary>관짝춤: Oiia와 동일 — 연습 Result 후 본게임 1회 예약.</summary>
        bool _coffinMainRoundQueuedAfterPractice;

        /// <summary>다음 Oiia 로드가 연습이면 true, 본게임이면 false. 본게임 예약이 있으면 큐를 소비한다.</summary>
        public bool TakeOiiaNextRoundIsPractice()
        {
            if (!_oiiaMainRoundQueuedAfterPractice)
                return true;

            _oiiaMainRoundQueuedAfterPractice = false;
            return false;
        }

        public void QueueOiiaMainRoundAfterPracticeEnded() => _oiiaMainRoundQueuedAfterPractice = true;

        public void ResetOiiaCycleAfterMainSession() => _oiiaMainRoundQueuedAfterPractice = false;

        public bool TakeCoffinDanceNextRoundIsPractice()
        {
            if (!_coffinMainRoundQueuedAfterPractice)
                return true;

            _coffinMainRoundQueuedAfterPractice = false;
            return false;
        }

        public void QueueCoffinDanceMainRoundAfterPracticeEnded() => _coffinMainRoundQueuedAfterPractice = true;

        public void ResetCoffinDanceCycleAfterMainSession() => _coffinMainRoundQueuedAfterPractice = false;

        /// <summary>
        /// Result 연출 직후·메인 씬 로드 전에 호출. DontDestroyOnLoad 슬롯에 로비 상태를 반영한다.
        /// </summary>
        public void FinalizeLobbyAfterMinigame(MinigameSessionReport report)
        {
            EnsureSlotsAllocated();

            for (var i = 0; i < 4; i++)
            {
                if (!WasParticipantInLastRound(i))
                    continue;

                PlayerSlotModel s = _slots[i];

                if (_lastPractice)
                {
                    if (s.State is SlotState.PLAYING or SlotState.GAMEOVER)
                        s.ResetToLobbyAfterMinigame();

                    continue;
                }

                bool lostHp = report?.HpLostThisSession != null &&
                              i < report.HpLostThisSession.Length &&
                              report.HpLostThisSession[i];

                if (!lostHp)
                    s.IncrementWinStreak();

                s.ReturnToMainMenuAfterRound();
            }
        }

        bool WasParticipantInLastRound(int index)
        {
            if (_lastPlayed[index])
                return true;

            SlotState st = _slots[index].State;
            return st is SlotState.PLAYING or SlotState.RESULT or SlotState.GAMEOVER;
        }

        void EnsureSlotsAllocated()
        {
            if (_slots != null)
                return;

            _slots = new PlayerSlotModel[4];
            for (var i = 0; i < 4; i++)
                _slots[i] = new PlayerSlotModel(i, startingHp);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSlotsAllocated();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>메인에서 미니게임 씬으로 뜨기 직전 호출.</summary>
        public void PrepareRound(bool practice, bool[] playedMask)
        {
            _lastPractice = practice;
            for (var i = 0; i < 4; i++)
                _lastPlayed[i] = i < playedMask.Length && playedMask[i];
        }

        /// <summary>미니게임 씬에서 완료 시 호출 → 메인 씬 로드 (레거시·디버그용).</summary>
        public void EndMinigameAndOpenMainMenu(MinigameSessionReport report)
        {
            _pendingReport = report;
            _hasPending = true;
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        /// <summary>미니게임 씬에서 완료 시 호출 → Result 씬 로드.</summary>
        public void EndMinigameAndOpenResultScene(MinigameSessionReport report)
        {
            _pendingReport = report;
            _hasPending = true;

            if (string.IsNullOrWhiteSpace(resultSceneName))
            {
                Debug.LogWarning("[PartySession] resultSceneName 이 비어 있어 MainMenu 로 보냅니다.");
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
                return;
            }

            SceneManager.LoadScene(resultSceneName, LoadSceneMode.Single);
        }

        /// <summary>Result 씬이 읽기 전용으로 보관 중인 리포트.</summary>
        public MinigameSessionReport PeekPendingReport() => _hasPending ? _pendingReport : null;

        /// <summary>Result 연출 종료 후 메인으로. 리포트는 메인에서 후처리용으로 넘긴다.</summary>
        public void CompleteResultAndOpenMainMenu(MinigameSessionReport reportFromResultScene = null)
        {
            MinigameSessionReport handoff = reportFromResultScene;

            if (handoff == null && _hasPending)
                handoff = _pendingReport;

            if (handoff != null)
            {
                FinalizeLobbyAfterMinigame(handoff);
                _postResultReport = handoff;
                _hasPostResultReport = true;
            }

            _pendingReport = null;
            _hasPending = false;

            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        /// <summary>Result 씬을 거친 뒤 메인에서 한 번 소비.</summary>
        public bool TryConsumePostResultReport(out MinigameSessionReport report)
        {
            report = null;

            if (!_hasPostResultReport || _postResultReport == null)
                return false;

            report = _postResultReport;
            _postResultReport = null;
            _hasPostResultReport = false;
            return true;
        }

        public bool TryConsumePendingReport(out MinigameSessionReport report)
        {
            report = null;

            if (!_hasPending || _pendingReport == null)
                return false;

            report = _pendingReport;
            _pendingReport = null;
            _hasPending = false;
            return true;
        }
    }
}
