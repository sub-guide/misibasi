using UnityEngine;

namespace MiniParty.Minigames.RhythmButtonChallenge
{
    public sealed partial class RhythmButtonChallengeMinigameModule
    {
        [Header("오디오 (각 8개, 길이 동일 전제)")]
        [SerializeField] AudioSource musicSource;

        [Tooltip("Phase 시작: 0_0 ~ 0_7")]
        [SerializeField] AudioClip[] phaseIntroClips = new AudioClip[BeatsPerSegment];

        [Tooltip("Stage 공개구간: 1_0 ~ 1_7")]
        [SerializeField] AudioClip[] revealClips = new AudioClip[BeatsPerSegment];

        [Tooltip("Stage 입력구간: 2_0 ~ 2_7")]
        [SerializeField] AudioClip[] inputClips = new AudioClip[BeatsPerSegment];

        [Header("SPEED UP")]
        [SerializeField] float speedUpDisplaySeconds = DefaultSpeedUpDisplaySeconds;

        void ApplyAudioPitch(float pitch)
        {
            if (musicSource == null)
                return;

            musicSource.pitch = pitch;
        }

        float ResolveBeatDurationSec()
        {
            AudioClip sample = phaseIntroClips != null && phaseIntroClips.Length > 0
                ? phaseIntroClips[0]
                : revealClips != null && revealClips.Length > 0
                    ? revealClips[0]
                    : inputClips != null && inputClips.Length > 0
                        ? inputClips[0]
                        : null;

            if (sample == null)
                return 0.5f;

            float pitch = musicSource != null ? Mathf.Max(0.01f, musicSource.pitch) : 1f;
            return sample.length / pitch;
        }

        void StartCurrentSegmentAudio()
        {
            _segmentStartTime = Time.unscaledTimeAsDouble;
            _beatDurationSec = ResolveBeatDurationSec();
            _beatIndex = -1;
            _segmentAudioStarted = true;
            AdvanceBeatIfNeeded(forceFirst: true);
        }

        void TickSegmentAudio()
        {
            if (!_segmentAudioStarted || _beatDurationSec <= 0f)
                return;

            AdvanceBeatIfNeeded(forceFirst: false);
        }

        void AdvanceBeatIfNeeded(bool forceFirst)
        {
            double elapsed = Time.unscaledTimeAsDouble - _segmentStartTime;
            int targetBeat = Mathf.Clamp(Mathf.FloorToInt((float)(elapsed / _beatDurationSec)), 0, BeatsPerSegment - 1);

            if (!forceFirst && targetBeat <= _beatIndex)
                return;

            for (int b = _beatIndex + 1; b <= targetBeat; b++)
            {
                if (b > 0 && _segmentKind == RbcSegmentKind.StageInput)
                    FinalizeInputBeat(b - 1);

                _beatIndex = b;
                PlayClipForCurrentBeat();
                OnBeatStarted(b);
            }

            if (_beatIndex >= BeatsPerSegment - 1)
            {
                double segmentEnd = _segmentStartTime + _beatDurationSec * BeatsPerSegment;
                if (Time.unscaledTimeAsDouble >= segmentEnd - 0.001)
                    OnSegmentFinished();
            }
        }

        void PlayClipForCurrentBeat()
        {
            if (musicSource == null)
                return;

            AudioClip clip = ResolveClip(_segmentKind, _beatIndex);
            if (clip == null)
                return;

            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = false;
            musicSource.Play();
        }

        AudioClip ResolveClip(RbcSegmentKind kind, int beat)
        {
            AudioClip[] arr = kind switch
            {
                RbcSegmentKind.PhaseIntro => phaseIntroClips,
                RbcSegmentKind.StageReveal => revealClips,
                RbcSegmentKind.StageInput => inputClips,
                _ => null
            };

            if (arr == null || beat < 0 || beat >= arr.Length)
                return null;

            return arr[beat];
        }

        void OnBeatStarted(int beatIndex)
        {
            UpdateBoardForBeat(beatIndex);

            if (_segmentKind == RbcSegmentKind.StageInput)
            {
                double beatStart = _segmentStartTime + beatIndex * _beatDurationSec;
                _inputBeatWindow = new BeatWindow
                {
                    BeatIndex = beatIndex,
                    StartTime = beatStart,
                    EndTime = beatStart + _beatDurationSec,
                    Active = true
                };
            }
        }

        void OnSegmentFinished()
        {
            _segmentAudioStarted = false;

            if (_segmentKind == RbcSegmentKind.StageInput)
            {
                FinalizeInputBeat(_beatIndex);
                ApplyEightBeatBonusIfEligible();
            }

            switch (_flowState)
            {
                case RbcFlowState.PhaseIntro:
                    BeginStageReveal(1);
                    StartCurrentSegmentAudio();
                    break;

                case RbcFlowState.StageReveal:
                    BeginStageInput();
                    StartCurrentSegmentAudio();
                    break;

                case RbcFlowState.StageInput:
                    if (_stageIndex < StagesPerPhase)
                    {
                        BeginStageReveal(_stageIndex + 1);
                        StartCurrentSegmentAudio();
                    }
                    else if (_phaseNumber == 1)
                    {
                        BeginSpeedUp();
                    }
                    else
                    {
                        CompleteSession();
                    }

                    break;
            }
        }

        void BeginSpeedUp()
        {
            _flowState = RbcFlowState.SpeedUp;
            _speedUpTimer = speedUpDisplaySeconds;
            ShowSpeedUpOverlay();
            ApplyAudioPitch(Phase2Pitch);
        }
    }
}
