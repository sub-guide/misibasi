using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("오디오·연출")]
        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioClip buzzClip;

        [Tooltip("본게임 1·2단계(유지 0초~10초 미만) 루프 BGM. 비우면 해당 구간 무음. 기본: `O.I.I.A/Sounds/drum.MP3`.")]
        [SerializeField] AudioClip tier2DrumLoop;

        [Tooltip("본게임 3단계(유지 ≥10초) 루프 BGM. 비우면 해당 구간 무음. 기본: `O.I.I.A/Sounds/oiia beat.MP3`.")]
        [SerializeField] AudioClip tier3BeatLoop;

        [Tooltip("티어 BGM 전용 AudioSource. 비우면 `sfxSource`와 분리된 두 번째 AudioSource를 런타임에 붙임.")]
        [SerializeField] AudioSource tierBgmSource;

        [Tooltip("티어 BGM 볼륨(0~1).")]
        [Range(0f, 1f)]
        [SerializeField] float tierBgmVolume = 0.85f;

        [Tooltip("2단계(drum, 유지 ≥5초~10초 미만) BGM 피치 배율. 1·3단계·정지는 1.")]
        [SerializeField] float tier2BgmPitchScale = 2f;

        [Tooltip("패턴 순서별 효과음. 길이는 패턴 글자 수와 동일해야 함. 인덱스 0 → 1번째 입력 성공 시, … 12 → 13번째.")]
        [SerializeField] AudioClip[] patternStepSfx = new AudioClip[13];

        void ValidatePatternSfxSetup()
        {
            int need = _patternLower.Length;

            if (patternStepSfx != null && patternStepSfx.Length >= need)
                return;

            if (_loggedPatternSfxArrayWarning)
                return;

            _loggedPatternSfxArrayWarning = true;
            Debug.LogWarning(
                $"[OiiaMinigameModule] patternStepSfx 배열이 패턴 길이({need})보다 짧거나 없습니다. " +
                "Inspector에서 Size={need} 로 맞추고 1~{need}번 효과음을 순서대로 넣으세요.",
                this);
        }

        void PlayPatternStepSfx(int stepIndexZeroBased)
        {
            if (sfxSource == null || patternStepSfx == null)
                return;

            if (stepIndexZeroBased < 0 || stepIndexZeroBased >= patternStepSfx.Length)
                return;

            AudioClip clip = patternStepSfx[stepIndexZeroBased];
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }

        void PlayBuzz()
        {
            if (sfxSource == null || buzzClip == null)
                return;

            sfxSource.PlayOneShot(buzzClip);
        }
    }
}
