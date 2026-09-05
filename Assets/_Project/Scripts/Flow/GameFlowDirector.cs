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
    /// 메인 메뉴 씬 전용: 로비(JOIN/READY) + 카탈로그 선택 + 미니게임 씬 로드.
    /// ↑/↓·Space는 이 디렉터만. 릴은 PlayStep / PlaySpin.
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
        [SerializeField] TMP_Text detailTitle;
        [SerializeField] TMP_Text detailBody;

        [Tooltip("없으면 ↑/↓는 우측 TMP만. Space 스핀도 없음.")]
        [SerializeField] MainMenuReelController reel;

        [Header("슬롯 HUD (4)")]
        [Tooltip("Slot_Player_1~4 의 SlotPokerHud. TMP Line1~3 바인딩은 제거됨.")]
        [SerializeField] SlotPokerHud[] slotPokerHud = new SlotPokerHud[4];

        [Header("메뉴 BGM")]
        [Tooltip("Play On Awake 끄기. Spatial Blend 0 (2D).")]
        [SerializeField] AudioSource menuBgmSource;

        [Tooltip("비우면 AudioSource에 꽂힌 클립을 씀.")]
        [SerializeField] AudioClip menuBgmClip;

        [Tooltip("메뉴 BGM 볼륨.")]
        [SerializeField] [Range(0f, 1f)] float menuBgmVolume = 1f;

        readonly OperatorInputService _operatorInput = new();

        PlayerSlotModel[] _slots;

        readonly bool[] _playedThisSession = new bool[4];
        bool _sessionPractice;
        string _lastStartedMinigameId;

        int _selectedCatalogIndex;
        PartyGamePhase _phase = PartyGamePhase.MainMenu;
        bool _waitingSpinEnd;

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

            EnsureCatalogPopulatedFallback();
            ClampSelection();
            if (reel != null)
                reel.BindCatalog(catalog, _selectedCatalogIndex);

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
                PlayMenuBgm();
                return;
            }

            if (ps.TryConsumePendingReport(out MinigameSessionReport legacy))
            {
                ApplySessionContextFromParty(ps);
                if (menuCanvas != null)
                    menuCanvas.enabled = true;

                OnMinigameComplete(legacy, fromResultScene: false);
            }

            PlayMenuBgm();
        }

        AudioClip ResolveMenuBgmClip()
        {
            if (menuBgmClip != null)
                return menuBgmClip;

            return menuBgmSource != null ? menuBgmSource.clip : null;
        }

        void PlayMenuBgm()
        {
            if (menuBgmSource == null)
                return;

            AudioClip clip = ResolveMenuBgmClip();
            if (clip == null)
                return;

            menuBgmSource.clip = clip;
            menuBgmSource.loop = true;
            menuBgmSource.spatialBlend = 0f;
            menuBgmSource.volume = menuBgmVolume;
            if (!menuBgmSource.isPlaying)
                menuBgmSource.Play();
        }

        void StopMenuBgm()
        {
            if (menuBgmSource == null)
                return;

            menuBgmSource.Stop();
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

            for (var i = 0; i < 4; i++)
            {
                if (slotPokerHud == null || i >= slotPokerHud.Length || slotPokerHud[i] == null)
                    Debug.LogWarning($"[GameFlowDirector] Slot Poker Hud Element {i} 가 비었습니다.", this);
            }

            if (detailTitle == null || detailBody == null)
                Debug.LogWarning("[GameFlowDirector] Detail Title/Body TMP 가 비어 있을 수 있습니다.", this);

            if (reel == null)
                Debug.LogWarning("[GameFlowDirector] reel 이 비었습니다. ↑/↓는 TMP만 갱신합니다.", this);

            if (menuBgmSource == null || ResolveMenuBgmClip() == null)
                Debug.LogWarning("[GameFlowDirector] 메뉴 BGM AudioSource 또는 AudioClip 이 비었습니다.", this);
        }

        void OnEnable()
        {
            SyncSlotsFromPartySession();

            if (_slots != null && enabled && _phase == PartyGamePhase.MainMenu)
                RefreshSlotHud();
        }

        void OnDestroy()
        {
            StopMenuBgm();

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
                TickMenuLobby();
        }

        void TickMenuLobby()
        {
            if (_waitingSpinEnd && (reel == null || !reel.IsSpinning))
            {
                _waitingSpinEnd = false;
                RefreshMenuUi(forceDetail: true);
            }

            if (catalog != null && catalog.Length > 0)
            {
                if (!CatalogLocked() && _operatorInput.Shuffle)
                    TryShuffleCatalog();

                if (!CatalogLocked() && _operatorInput.MenuUp)
                    TryStepCatalog(-1);

                if (!CatalogLocked() && _operatorInput.MenuDown)
                    TryStepCatalog(1);
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

            if (_operatorInput.Confirm && !CatalogLocked())
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

        bool CatalogLocked()
        {
            if (reel == null)
                return false;

            if (reel.IsSpinning)
                return true;

            return reel.LockInputUntilSettled && reel.IsSettling;
        }

        void TryShuffleCatalog()
        {
            if (catalog == null || catalog.Length == 0)
                return;

            if (reel == null)
                return;

            if (CatalogLocked())
                return;

            int winner = UnityEngine.Random.Range(0, catalog.Length);
            _selectedCatalogIndex = winner;
            if (detailTitle != null)
                detailTitle.text = string.Empty;
            if (detailBody != null)
                detailBody.text = string.Empty;

            reel.PlaySpin(winner);
            _waitingSpinEnd = true;
        }

        void TryStepCatalog(int delta)
        {
            if (catalog == null || catalog.Length == 0)
                return;

            if (CatalogLocked())
                return;

            _selectedCatalogIndex = GameCatalogEntry.WrapIndex(_selectedCatalogIndex + delta, catalog.Length);
            if (reel != null)
                reel.PlayStep(_selectedCatalogIndex, delta);

            RefreshMenuUi(forceDetail: true);
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
            if (catalog == null || catalog.Length == 0)
                return;

            GameCatalogEntry entry = catalog[GameCatalogEntry.WrapIndex(_selectedCatalogIndex, catalog.Length)];

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

            StopMenuBgm();

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

            _phase = PartyGamePhase.MainMenu;

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

        void RefreshMenuUi(bool forceDetail)
        {
            if (!forceDetail || detailTitle == null || detailBody == null)
                return;

            if (catalog == null || catalog.Length == 0)
                return;

            GameCatalogEntry e = catalog[GameCatalogEntry.WrapIndex(_selectedCatalogIndex, catalog.Length)];
            detailTitle.text = e.title;
            detailBody.text = e.blurb;
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

        void FlushSlotHudRow(int i)
        {
            if (slotPokerHud == null || i < 0 || i >= slotPokerHud.Length || slotPokerHud[i] == null)
                return;

            if (_slots == null || i >= _slots.Length)
                return;

            int startHp = partySession != null ? partySession.StartingHp : 3;
            slotPokerHud[i].Apply(_slots[i], startHp);
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
    }
}
