using System;
using MiniParty.Core;
using MiniParty.Input;
using MiniParty.Minigames;
using MiniParty.Minigames.CoffinDance;
using MiniParty.Minigames.Oiia;
using MiniParty.Minigames.RhythmButtonChallenge;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MiniParty.Flow
{
    /// <summary>
    /// 메인 메뉴 씬 전용: 로비 UI + 미니게임 씬 로드/복귀 후 결과 표시.
    /// 미니게임 본편은 별도 씬에서 <see cref="OiiaSceneBootstrap"/> 등이 구동한다.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class GameFlowDirector : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("메인 메뉴에 반드시 하나 둘 것 (DontDestroyOnLoad). 없으면 이 디렉터는 비활성화된다.")]
        [SerializeField] PartySession partySession;

        [Header("씬 ( Build Settings 이름과 동일해야 함 )")]
        [Tooltip("oiia 미니게임 전용 씬 이름. 예: Minigame_Oiia")]
        [SerializeField] string oiiaSceneName = "Minigame_Oiia";

        [Tooltip("Rhythm Button Challenge 전용 씬 이름. Build Settings 와 동일해야 함.")]
        [SerializeField] string rhythmButtonChallengeSceneName = "Minigame_RhythmButtonChallenge";

        [Tooltip("관짝춤(Coffin Dance) 전용 씬 이름. Build Settings 와 동일해야 함.")]
        [SerializeField] string coffinDanceSceneName = "Minigame_CoffinDance";

        [Header("디버그")]
        [Tooltip("체크 시 카탈로그 어떤 항목이든 oiia 씬으로 진입 (MVP 편의).")]
        [SerializeField] bool debugRouteAllToOiia;

        [Header("카탈로그")]
        [SerializeField] GameCatalogEntry[] catalog;

        [Header("메뉴 UI 전용")]
        [SerializeField] Canvas menuCanvas;
        [SerializeField] TMP_Text[] carouselRows = new TMP_Text[7];
        [SerializeField] TMP_Text detailTitle;
        [SerializeField] TMP_Text detailBody;

        [Header("슬롯 HUD (4)")]
        [SerializeField] SlotHudBind[] slotHud = new SlotHudBind[4];

        [Header("오버레이")]
        [SerializeField] GameObject resultRoot;
        [SerializeField] TMP_Text resultBody;

        readonly OperatorInputService _operatorInput = new();

        PlayerSlotModel[] _slots;

        readonly bool[] _playedThisSession = new bool[4];
        bool _sessionPractice;
        string _lastStartedMinigameId;

        int _selectedCatalogIndex;
        PartyGamePhase _phase = PartyGamePhase.MainMenu;

        const int VisibleWindow = 7;
        const int CenterRow = 3;

        [Serializable]
        public sealed class GameCatalogEntry
        {
            public string id;
            public string title;
            [TextArea(2, 6)] public string blurb;
        }

        [Serializable]
        public sealed class SlotHudBind
        {
            public TMP_Text Line1;
            public TMP_Text Line2;
            public TMP_Text Line3;
        }

        void Awake()
        {
            ResolvePartySession();

            if (_slots == null)
            {
                enabled = false;
                return;
            }

            for (var i = 0; i < 4; i++)
                WireSlotHud(i);

            HideResultImmediate();

            EnsureCatalogPopulatedFallback();
            ClampSelection();
            RefreshMenuUi(forceDetail: true);

            RefreshSlotHud();
            LogWeakUiAssignmentsOnce();
        }

        void Start()
        {
            SyncSlotsFromPartySession();

            if (_slots == null)
                return;

            PartySession ps = partySession;
            if (ps == null)
                return;

            if (ps.TryConsumePostResultReport(out MinigameSessionReport postResult))
            {
                ApplySessionContextFromParty(ps);
                if (menuCanvas != null)
                    menuCanvas.enabled = true;

                OnMinigameComplete(postResult, fromResultScene: true);
                return;
            }

            if (ps.TryConsumePendingReport(out MinigameSessionReport legacy))
            {
                ApplySessionContextFromParty(ps);
                if (menuCanvas != null)
                    menuCanvas.enabled = true;

                OnMinigameComplete(legacy, fromResultScene: false);
            }
        }

        void ApplySessionContextFromParty(PartySession ps)
        {
            _sessionPractice = ps.LastPractice;
            for (var i = 0; i < 4; i++)
                _playedThisSession[i] = ps.LastPlayedMask[i];
        }

        void ResolvePartySession() => SyncSlotsFromPartySession();

        void SyncSlotsFromPartySession()
        {
            PartySession resolved = PartySession.Instance;

            if (resolved == null && partySession != null)
                resolved = partySession;

            if (resolved == null)
                resolved = FindObjectOfType<PartySession>();

            if (resolved == null)
            {
                Debug.LogError("[GameFlowDirector] PartySession 이 씬에 없습니다. 빈 GameObject에 PartySession 을 추가하세요.");
                _slots = null;
                return;
            }

            partySession = resolved;
            _slots = partySession.Slots;

            for (var i = 0; i < 4; i++)
                WireSlotHud(i);
        }

        bool _loggedUiHints;

        void LogWeakUiAssignmentsOnce()
        {
            if (_loggedUiHints)
                return;

            _loggedUiHints = true;

            if (carouselRows == null || carouselRows.Length < VisibleWindow)
                Debug.LogWarning(
                    $"[GameFlowDirector] Carousel Rows 크기가 {VisibleWindow} 미만입니다. 메뉴 리스트가 보이지 않을 수 있습니다.",
                    this);

            for (var i = 0; i < VisibleWindow && carouselRows != null; i++)
            {
                if (carouselRows[i] == null)
                    Debug.LogWarning($"[GameFlowDirector] Carousel Rows Element {i} 가 비었습니다.", this);
            }

            for (var i = 0; i < 4; i++)
            {
                if (slotHud == null || i >= slotHud.Length || slotHud[i] == null)
                {
                    Debug.LogWarning($"[GameFlowDirector] Slot Hud Element {i} 가 비었습니다.", this);
                    continue;
                }

                if (slotHud[i].Line1 == null && slotHud[i].Line2 == null && slotHud[i].Line3 == null)
                    Debug.LogWarning($"[GameFlowDirector] Slot Hud {i} 의 TMP 참조가 모두 비었습니다.", this);
            }

            if (detailTitle == null || detailBody == null)
                Debug.LogWarning("[GameFlowDirector] Detail Title/Body TMP 가 비어 있을 수 있습니다.", this);
        }

        void OnEnable()
        {
            SyncSlotsFromPartySession();

            if (_slots != null && enabled && _phase == PartyGamePhase.MainMenu)
                RefreshSlotHud();
        }

        void OnDestroy()
        {
            if (_slots == null)
                return;

            for (var i = 0; i < 4; i++)
            {
                if (_slots[i] != null)
                    _slots[i].StateChanged -= OnSlotHudModelChanged;
            }
        }

        void Update()
        {
            if (_slots == null)
                return;

            if (_phase == PartyGamePhase.MainMenu)
            {
                TickMenuLobby();
                return;
            }

            if (_phase == PartyGamePhase.ResultSummary)
            {
                if (_operatorInput.Confirm)
                    FinishResultOverlay();
            }
        }

        void TickMenuLobby()
        {
            if (_operatorInput.MenuUp)
            {
                _selectedCatalogIndex = Wrap(_selectedCatalogIndex - 1, catalog.Length);
                RefreshMenuUi(forceDetail: true);
            }

            if (_operatorInput.MenuDown)
            {
                _selectedCatalogIndex = Wrap(_selectedCatalogIndex + 1, catalog.Length);
                RefreshMenuUi(forceDetail: true);
            }

            for (var i = 0; i < 4; i++)
            {
                if (!SlotGamepad.StartPressed(i))
                    continue;

                PlayerSlotModel s = _slots[i];

                switch (s.State)
                {
                    case SlotState.EMPTY:
                    {
                        int joinHp = partySession != null ? partySession.StartingHp : 3;
                        s.TryJoinFromEmpty(joinHp);
                        break;
                    }
                    case SlotState.ACTIVE:
                    case SlotState.READY:
                        s.ToggleReady();
                        break;
                }
            }

            if (_operatorInput.Confirm)
            {
                if (CanStartFromMenu())
                {
                    if (string.IsNullOrWhiteSpace(oiiaSceneName))
                    {
                        if (detailBody != null)
                            detailBody.text = "oiiaSceneName is empty. Set it on GameFlowDirector.";

                        return;
                    }

                    StartSelectedMinigame();
                }
            }

            RefreshSlotHud();
        }

        bool CanStartFromMenu()
        {
            bool anyReady = false;
            for (var i = 0; i < 4; i++)
            {
                SlotState st = _slots[i].State;
                if (st == SlotState.ACTIVE)
                    return false;

                if (st == SlotState.READY)
                    anyReady = true;
            }

            return anyReady;
        }

        void StartSelectedMinigame()
        {
            GameCatalogEntry entry = catalog[_selectedCatalogIndex];

            bool runRbc = string.Equals(entry.id, RhythmButtonChallengeMinigameModule.BuiltInId, StringComparison.OrdinalIgnoreCase);
            bool runCoffin = string.Equals(entry.id, CoffinDanceMinigameModule.BuiltInId, StringComparison.OrdinalIgnoreCase);
            bool runOiia = debugRouteAllToOiia ||
                string.Equals(entry.id, OiiaMinigameModule.BuiltInId, StringComparison.OrdinalIgnoreCase);

            if (!runRbc && !runCoffin && !runOiia)
            {
                partySession?.ResetOiiaCycleAfterMainSession();
                partySession?.ResetCoffinDanceCycleAfterMainSession();

                if (detailBody != null)
                    detailBody.text = "This minigame is not available yet.";

                return;
            }

            if (partySession == null)
                return;

            if (runRbc)
                _lastStartedMinigameId = RhythmButtonChallengeMinigameModule.BuiltInId;
            else if (runCoffin)
                _lastStartedMinigameId = CoffinDanceMinigameModule.BuiltInId;
            else
                _lastStartedMinigameId = OiiaMinigameModule.BuiltInId;

            if (runRbc)
                _sessionPractice = false;
            else if (runCoffin)
                _sessionPractice = partySession.TakeCoffinDanceNextRoundIsPractice();
            else
                _sessionPractice = partySession.TakeOiiaNextRoundIsPractice();

            for (var i = 0; i < 4; i++)
                _playedThisSession[i] = false;

            for (var i = 0; i < 4; i++)
            {
                PlayerSlotModel s = _slots[i];
                if (s.State != SlotState.READY)
                    continue;

                _playedThisSession[i] = true;
                s.EnterPlaying();
            }

            partySession.PrepareRound(_sessionPractice, _playedThisSession);

            if (menuCanvas != null)
                menuCanvas.enabled = false;

            string sceneName;
            if (runRbc)
                sceneName = rhythmButtonChallengeSceneName;
            else if (runCoffin)
                sceneName = coffinDanceSceneName;
            else
                sceneName = oiiaSceneName;

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        void OnMinigameComplete(MinigameSessionReport report, bool fromResultScene)
        {
            SyncSlotsFromPartySession();

            if (!fromResultScene)
                PostProcessScoresAndStates(report);

            PartySession psNotify = partySession;
            if (psNotify != null)
            {
                bool oiiaSession = string.Equals(_lastStartedMinigameId, OiiaMinigameModule.BuiltInId, StringComparison.OrdinalIgnoreCase);
                if (oiiaSession)
                {
                    if (_sessionPractice)
                        psNotify.QueueOiiaMainRoundAfterPracticeEnded();
                    else
                        psNotify.ResetOiiaCycleAfterMainSession();
                }

                bool coffinSession = string.Equals(_lastStartedMinigameId, CoffinDanceMinigameModule.BuiltInId, StringComparison.OrdinalIgnoreCase);
                if (coffinSession)
                {
                    if (_sessionPractice)
                        psNotify.QueueCoffinDanceMainRoundAfterPracticeEnded();
                    else
                        psNotify.ResetCoffinDanceCycleAfterMainSession();
                }
            }

            if (fromResultScene)
            {
                HideResultImmediate();
                _phase = PartyGamePhase.MainMenu;
            }
            else
            {
                _phase = PartyGamePhase.ResultSummary;
                BuildResultText(report);

                if (resultRoot != null)
                    resultRoot.SetActive(true);
            }

            RefreshSlotHud();
            RefreshMenuUi(forceDetail: true);
        }

        void PostProcessScoresAndStates(MinigameSessionReport report)
        {
            PartySession ps = partySession;
            if (ps == null)
                return;

            ps.FinalizeLobbyAfterMinigame(report);
        }

        void BuildResultText(MinigameSessionReport report)
        {
            if (resultBody == null) return;

            if (_sessionPractice)
            {
                resultBody.text =
                    "Practice ended (Esc).\n" +
                    "In practice: START when ready → all ready → Enter for MAIN.\n" +
                    "Or from menu: each START toggles READY, then operator Enter.";
                return;
            }

            string msg = "Round results\n";
            for (var i = 0; i < 4; i++)
            {
                if (!_playedThisSession[i])
                    continue;

                PlayerSlotModel s = _slots[i];
                msg += $"Slot {i + 1}: score {report.FinalScore[i]} / HP {s.HP} / streak {s.WinStreak}\n";
            }

            resultBody.text = msg;
        }

        void FinishResultOverlay()
        {
            HideResultImmediate();
            _phase = PartyGamePhase.MainMenu;
            RefreshMenuUi(forceDetail: true);
        }

        void HideResultImmediate()
        {
            if (resultRoot != null)
                resultRoot.SetActive(false);
        }

        void RefreshMenuUi(bool forceDetail)
        {
            if (carouselRows.Length < VisibleWindow)
                return;

            for (var row = 0; row < VisibleWindow; row++)
            {
                int idx = Wrap(_selectedCatalogIndex + (row - CenterRow), catalog.Length);
                TMP_Text label = carouselRows[row];
                if (label != null)
                {
                    string mark = row == CenterRow ? "[ " + catalog[idx].title + " ]" : catalog[idx].title;
                    label.text = mark;
                }
            }

            if (forceDetail && detailTitle != null && detailBody != null)
            {
                GameCatalogEntry e = catalog[_selectedCatalogIndex];
                detailTitle.text = e.title;
                detailBody.text = e.blurb;
            }
        }

        void RefreshSlotHud()
        {
            for (var i = 0; i < 4; i++)
                FlushSlotHudRow(i);
        }

        void WireSlotHud(int i)
        {
            _slots[i].StateChanged -= OnSlotHudModelChanged;

            _slots[i].StateChanged += OnSlotHudModelChanged;
        }

        void OnSlotHudModelChanged(PlayerSlotModel model, SlotState prev, SlotState next)
        {
            FlushSlotHudRow(model.Index);
        }

        static readonly Color SlotHudWhite = Color.white;

        /// <summary>EMPTY·ACTIVE Line2 흰색 안내 펄스(투명 ↔ 불투명). 값이 클수록 더 빠름.</summary>
        const float SlotHudLine2PulseSpeed = 3.6f;

        static readonly Color SlotHudReadyGreen = new(0.25f, 0.92f, 0.42f);

        /// <summary>#FFCD00 ACTIVE Line2 안내색.</summary>
        static readonly Color SlotHudActiveLine2 = new(1f, 205f / 255f, 0f);

        static float SlotHudLine2PulseAlpha()
        {
            float ping = Mathf.Sin(Time.unscaledTime * SlotHudLine2PulseSpeed) * 0.5f + 0.5f;
            return Mathf.Lerp(0.1f, 1f, ping);
        }

        void FlushSlotHudRow(int i)
        {
            if (i < 0 || i >= slotHud.Length || slotHud[i] == null)
                return;

            PlayerSlotModel s = _slots[i];
            TMP_Text l1 = slotHud[i].Line1;
            TMP_Text l2 = slotHud[i].Line2;
            TMP_Text l3 = slotHud[i].Line3;

            if (l1 != null)
                l1.text = $"PLAYER {i + 1}";

            if (l2 != null)
            {
                switch (s.State)
                {
                    case SlotState.EMPTY:
                        l2.text = "PRESS START TO JOIN";
                        l2.color = new Color(1f, 1f, 1f, SlotHudLine2PulseAlpha());
                        break;
                    case SlotState.ACTIVE:
                        l2.text = "PRESS START TO READY";
                        l2.color = new Color(
                            SlotHudActiveLine2.r,
                            SlotHudActiveLine2.g,
                            SlotHudActiveLine2.b,
                            SlotHudLine2PulseAlpha());
                        break;
                    case SlotState.READY:
                        l2.text = "READY";
                        l2.color = SlotHudReadyGreen;
                        break;
                    case SlotState.PLAYING:
                        l2.text = "PLAY";
                        l2.color = SlotHudWhite;
                        break;
                    case SlotState.RESULT:
                        l2.text = "RESULT";
                        l2.color = SlotHudWhite;
                        break;
                    case SlotState.GAMEOVER:
                        l2.text = "GAME OVER";
                        l2.color = SlotHudWhite;
                        break;
                    default:
                        l2.text = string.Empty;
                        l2.color = SlotHudWhite;
                        break;
                }
            }

            if (l3 != null)
            {
                l3.text = $"WIN {s.WinStreak}";
                l3.color = SlotHudWhite;
            }
        }

        void EnsureCatalogPopulatedFallback()
        {
            if (catalog != null && catalog.Length > 0)
                return;

            catalog = new[]
            {
                new GameCatalogEntry
                {
                    id = OiiaMinigameModule.BuiltInId,
                    title = "OIIA (Rhythm / Sustain)",
                    blurb = "X = O, A = I, Y = A\nRepeat pattern: oiiaiooiiiai.\nFail FX and rising difficulty."
                },
                new GameCatalogEntry
                {
                    id = RhythmButtonChallengeMinigameModule.BuiltInId,
                    title = "Rhythm Button Challenge",
                    blurb = "8-beat pattern memory.\nPhase 2 doubles speed.\nScore 500,000 to survive."
                },
                new GameCatalogEntry
                {
                    id = CoffinDanceMinigameModule.BuiltInId,
                    title = "관짝춤",
                    blurb = ""
                },
                new GameCatalogEntry { id = "placeholder_04", title = "GAME 04 (TBD)", blurb = "Coming soon." },
                new GameCatalogEntry { id = "placeholder_05", title = "GAME 05 (TBD)", blurb = "Coming soon." },
                new GameCatalogEntry { id = "placeholder_06", title = "GAME 06 (TBD)", blurb = "Coming soon." },
                new GameCatalogEntry { id = "placeholder_07", title = "GAME 07 (TBD)", blurb = "Coming soon." },
                new GameCatalogEntry { id = "placeholder_08", title = "GAME 08 (TBD)", blurb = "Coming soon." }
            };
        }

        void ClampSelection()
        {
            if (catalog == null || catalog.Length == 0)
                return;

            _selectedCatalogIndex = Mathf.Clamp(_selectedCatalogIndex, 0, catalog.Length - 1);
        }

        static int Wrap(int value, int length)
        {
            if (length <= 0) return 0;
            int m = value % length;
            return m < 0 ? m + length : m;
        }
    }
}
