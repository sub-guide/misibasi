using System.Collections.Generic;
using MiniParty.Core;
using MiniParty.Minigames;
using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Flounder
{
    public sealed partial class FlounderGameManager : MonoBehaviour
    {
        public const int SlotCount = 4;

        [Header("규칙")]
        [SerializeField] string gameTitle = "물 만난 광어";
        [SerializeField] float roundDurationSeconds = 25f;
        [SerializeField] int scorePerMash = 10;
        [Tooltip("시작 후 ↑/↓ 입력 후 중앙 후원창을 띄우기 전 대기(초)")]
        [SerializeField] float startDonationDelayAfterInputSeconds = 0.65f;

        [Header("시스템")]
        [SerializeField] FlounderDonationManager donationManager;
        [SerializeField] FlounderDonationUI donationUi;
        [SerializeField] FlounderCountdown countdown;
        [SerializeField] FlounderLiveUI liveUi;
        [SerializeField] FlounderResultUI resultUi;
        [SerializeField] FlounderMinigameModule minigameModule;
        [SerializeField] TextMeshProUGUI roundTimerText;

        [Header("슬롯")]
        [SerializeField] FlounderPlayerSlotView[] slotViews;
        [SerializeField] FlounderInput[] slotInputs;

        [Header("캐릭터 GIF (1P)")]
        [SerializeField] bool gifCharacterOnlyOnFirstSlot = true;

        [Tooltip("켜면 Awake에서 GifVisual 스케일·위치를 아래 값으로 덮어씁니다. 씬에서 직접 배치한 경우 끄세요.")]
        [SerializeField] bool overrideFirstSlotGifTransform;
        [SerializeField] float firstSlotGifScale = 0.78f;
        [SerializeField] float firstSlotGifLocalY = 0.42f;

        readonly FlounderPlayer[] _players = new FlounderPlayer[SlotCount];

        FlounderGamePhase _phase = FlounderGamePhase.Idle;
        float _roundElapsed;
        bool _running;

        /// <summary>-1 = 없음, 0 = ↑, 1 = ↓. 같은 방향 연속 연타는 게이지 미증가.</summary>
        readonly int[] _lastFlapDirection = new int[SlotCount];

        bool _startDonationInputReceived;
        bool _startDonationPopupShown;
        float _startDonationPrePopupTimer;

        public FlounderGamePhase CurrentPhase => _phase;
        public bool IsRunning => _running;
        public IReadOnlyList<FlounderPlayer> Players => _players;

        void Awake()
        {
            for (var i = 0; i < SlotCount; i++)
                _players[i] = new FlounderPlayer { SlotIndex = i };

            if (minigameModule == null)
                minigameModule = GetComponent<FlounderMinigameModule>();

            ApplyGifCharacterLayout();
            EnsureStartDonationBgmSource();
            SyncStartDonationBgmFromScene();
        }

        FlounderPlayerSlotView GetSlotView(int slotIndex)
        {
            if (slotViews == null)
                return null;

            for (var i = 0; i < slotViews.Length; i++)
            {
                FlounderPlayerSlotView view = slotViews[i];
                if (view != null && view.SlotIndex == slotIndex)
                    return view;
            }

            return slotIndex >= 0 && slotIndex < slotViews.Length ? slotViews[slotIndex] : null;
        }

        void ApplyGifCharacterLayout()
        {
            if (slotViews == null)
                return;

            for (var i = 0; i < SlotCount; i++)
            {
                FlounderPlayerSlotView view = GetSlotView(i);
                if (view == null || view.CharacterRoot == null)
                    continue;

                Transform root = view.CharacterRoot;
                if (gifCharacterOnlyOnFirstSlot && i != 0)
                {
                    Transform extraGif = root.Find("GifVisual");
                    if (extraGif != null)
                        Destroy(extraGif.gameObject);

                    var extraDance = root.GetComponent<FlounderDance>();
                    if (extraDance != null)
                        Destroy(extraDance);

                    continue;
                }

                view.ApplyGifCharacterTint();

                if (i != 0)
                    continue;

                if (!overrideFirstSlotGifTransform)
                    continue;

                Transform gif = root.Find("GifVisual");
                if (gif == null)
                    continue;

                gif.localScale = Vector3.one * firstSlotGifScale;
                gif.localPosition = new Vector3(0f, firstSlotGifLocalY, 0f);
            }
        }

        public void Begin(MinigameContext context)
        {
            _running = true;
            _phase = FlounderGamePhase.StartDonation;
            _roundElapsed = 0f;

            StopStartDonationBgm();
            donationUi?.ClearFeed();
            countdown?.Cancel();
            resultUi?.Hide();
            liveUi?.ResetViewers();

            _startDonationInputReceived = false;
            _startDonationPopupShown = false;
            _startDonationPrePopupTimer = 0f;

            for (var i = 0; i < SlotCount; i++)
                _lastFlapDirection[i] = -1;

            SetAllCharacterDancesFrozen(false);

            for (var i = 0; i < SlotCount; i++)
            {
                bool active = context.Slots != null &&
                                i < context.Slots.Length &&
                                context.Slots[i].State == SlotState.PLAYING;

                _players[i].Active = active;
                _players[i].ResetForRound();

                FlounderPlayerSlotView view = GetSlotView(i);
                if (view != null)
                {
                    view.SetActiveSlot(active);
                    view.SetLabel((i + 1) + "P");
                    view.Gauge?.ResetGauge();
                    view.SetScore(0);
                }
            }

            RefreshTimerLabel(roundDurationSeconds);
        }

        public void Tick()
        {
            if (!_running)
                return;

            float dt = Time.deltaTime;

            switch (_phase)
            {
                case FlounderGamePhase.StartDonation:
                    TickStartDonation(dt);
                    break;
                case FlounderGamePhase.Countdown:
                    TickCountdown(dt);
                    break;
                case FlounderGamePhase.Playing:
                    TickPlaying(dt);
                    break;
            }
        }

        void TickStartDonation(float dt)
        {
            if (donationUi == null)
            {
                BeginCountdownPhase();
                return;
            }

            if (!_startDonationPopupShown)
            {
                if (!_startDonationInputReceived)
                {
                    if (AnyActiveSlotFlapPressed())
                        _startDonationInputReceived = true;

                    return;
                }

                _startDonationPrePopupTimer += dt;
                if (_startDonationPrePopupTimer < startDonationDelayAfterInputSeconds)
                    return;

                int startAmount = donationManager != null ? donationManager.StartDonationAmount : 50000;
                donationUi.ShowStartDonationPopup(startAmount, gameTitle);
                _startDonationPopupShown = true;
                return;
            }

            if (!donationUi.TickStartSequence(dt))
                return;

            donationUi.HideCenterPopup();
            BeginCountdownPhase();
        }

        void BeginCountdownPhase()
        {
            countdown?.Begin();
            PlayCountdownBgm();
            _phase = FlounderGamePhase.Countdown;
        }

        bool AnyActiveSlotFlapPressed()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_players[i].Active)
                    continue;

                FlounderInput input = GetInput(i);
                if (input == null)
                    continue;

                if (input.PollUpperFlap() || input.PollLowerFlap())
                    return true;
            }

            return false;
        }

        void TickCountdown(float dt)
        {
            if (countdown != null && countdown.Tick(dt))
            {
                _phase = FlounderGamePhase.Playing;
                _roundElapsed = 0f;
            }
        }

        void TickPlaying(float dt)
        {
            _roundElapsed += dt;
            float remaining = Mathf.Max(0f, roundDurationSeconds - _roundElapsed);
            RefreshTimerLabel(remaining);

            for (var i = 0; i < SlotCount; i++)
            {
                if (!_players[i].Active)
                    continue;

                FlounderPlayerSlotView view = GetSlotView(i);
                FlounderGauge gauge = view?.Gauge;
                FlounderInput input = GetInput(i);
                FlounderDance dance = view?.Dance;

                gauge?.Tick(dt);

                if (input != null)
                {
                    if (input.PollUpperFlap())
                        RegisterMash(i, upper: true);
                    if (input.PollLowerFlap())
                        RegisterMash(i, upper: false);

                    dance?.ApplyDirectionalHold(input.IsUpperHeld(), input.IsLowerHeld());
                }

                if (gauge != null && gauge.IsFull)
                    TriggerGaugeBonus(i);
            }

            donationUi?.TickGameplayAlerts(dt);

            if (_roundElapsed >= roundDurationSeconds)
                EndRound();
        }

        void RegisterMash(int slotIndex, bool upper)
        {
            FlounderPlayer p = _players[slotIndex];

            int dir = upper ? 0 : 1;
            bool alternatesDirection = _lastFlapDirection[slotIndex] != dir;
            _lastFlapDirection[slotIndex] = dir;

            FlounderPlayerSlotView view = GetSlotView(slotIndex);
            if (upper)
                view?.Dance?.FlapUpper();
            else
                view?.Dance?.FlapLower();

            if (!alternatesDirection)
                return;

            p.MashCount++;
            p.MashScore += scorePerMash;

            view?.Gauge?.RegisterMash();
            view?.SetScore(p.FinalScore);
            liveUi?.RegisterMash();
        }

        void TriggerGaugeBonus(int slotIndex)
        {
            FlounderPlayer p = _players[slotIndex];
            if (!p.Active)
                return;

            FlounderPlayerSlotView view = GetSlotView(slotIndex);
            view?.Gauge?.ResetGauge();

            p.ReceivedGameplayDonation = true;

            FlounderDonationRoll roll = donationManager != null
                ? donationManager.RollGameplayDonation()
                : default;

            p.DonationCount++;
            p.TotalDonationWon += roll.AmountWon;
            p.DonationBonusScore += roll.BonusScore;

            var alert = FlounderDonationEventData.Gameplay(roll.DonorName, slotIndex, roll, gameTitle);
            donationUi?.EnqueueGameplayAlert(alert);
            liveUi?.RegisterDonation();

            view?.SetScore(p.FinalScore);
        }

        void EndRound()
        {
            StopStartDonationBgm();
            RefreshTimerLabel(0f);
            SetAllCharacterDancesFrozen(true);
            _phase = FlounderGamePhase.Ended;
            _running = false;
            minigameModule?.NotifyRoundEndedFromUi();
        }

        void SetAllCharacterDancesFrozen(bool frozen)
        {
            for (var i = 0; i < SlotCount; i++)
            {
                FlounderPlayerSlotView view = GetSlotView(i);
                view?.Dance?.SetFrozen(frozen);
            }
        }

        void RefreshTimerLabel(float seconds)
        {
            if (roundTimerText == null)
                return;

            roundTimerText.text = seconds.ToString("0.0");
        }

        FlounderInput GetInput(int i) =>
            slotInputs != null && i >= 0 && i < slotInputs.Length ? slotInputs[i] : null;
    }
}
