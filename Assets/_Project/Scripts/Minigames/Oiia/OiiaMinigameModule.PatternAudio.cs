using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        [Header("오디오·연출")]
        [SerializeField] AudioSource sfxSource;

        [Tooltip("본게임 단일 BGM (원작 밈 트랙). 비우면 무음. 권장: W&W - OIIA OIIA (Spinning Cat).")]
        [SerializeField] AudioClip mainBgmClip;

        [Tooltip("BGM 전용 AudioSource. 비우면 런타임에 보조 AudioSource 생성.")]
        [SerializeField] AudioSource tierBgmSource;

        [Tooltip("BGM 볼륨(0~1).")]
        [Range(0f, 1f)]
        [SerializeField] float tierBgmVolume = 0.85f;

        [Tooltip("패턴 스텝 SFX 12개(oiiaiooiiiai). 입력마다 FeverChant 떼창.")]
        [SerializeField] AudioClip[] patternStepSfx = new AudioClip[0];

        void PlayPatternStepSfx(int stepIndexZeroBased)
        {
            if (patternStepSfx == null)
                return;

            if (stepIndexZeroBased < 0 || stepIndexZeroBased >= patternStepSfx.Length)
                return;

            AudioClip clip = patternStepSfx[stepIndexZeroBased];
            if (clip == null)
                return;

            PlayFeverChantStep(clip);
        }
    }
}
