using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("오디오·연출")]
        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioClip buzzClip;

        [Tooltip("본게임 2티어 루프 BGM. 비우면 해당 구간 무음.")]
        [SerializeField] AudioClip tier2DrumLoop;

        [Tooltip("본게임 3티어 루프 BGM. 비우면 해당 구간 무음.")]
        [SerializeField] AudioClip tier3BeatLoop;

        [Tooltip("티어 BGM 전용 AudioSource. 비우면 런타임에 보조 AudioSource 생성.")]
        [SerializeField] AudioSource tierBgmSource;

        [Tooltip("티어 BGM 볼륨(0~1).")]
        [Range(0f, 1f)]
        [SerializeField] float tierBgmVolume = 0.85f;

        [Tooltip("2티어 BGM 피치 배율.")]
        [SerializeField] float tier2BgmPitchScale = 2f;

        [Tooltip("레거시 패턴 스텝 SFX(개편 전). 2단계+ 디제잉 입력음으로 교체 예정. 현재 미사용.")]
        [SerializeField] AudioClip[] patternStepSfx = new AudioClip[0];

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
