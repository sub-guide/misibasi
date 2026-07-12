using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>피버 중 패턴 한 글자 진행 간격(초). 12글자 루프 연속재생.</summary>
        [Header("피버 패턴 연속재생")]
        [Tooltip("피버 중 SubPattern 한 글자마다의 간격(초).")]
        [SerializeField] float feverSubPatternStepSeconds = 0.1f;

        /// <summary>
        /// 레거시 SequenceText A안과 동일: 맞춘 접두만 대문자로 표시.
        /// 폰트 크기·색·다음 글자 미리보기 없음.
        /// 완성 시 피버 진입. 피버 중에는 자동으로 oiia 루프 연속재생.
        /// </summary>
        void AdvanceSubPatternOnHit(int slotIndex, ref SlotRuntime sr)
        {
            if (sr.FeverRemaining > 0f)
                return;

            int len = SubPatternLower.Length;
            if (len <= 0)
            {
                sr.SubPatternMatched = 0;
                return;
            }

            if (sr.SubPatternMatched >= len)
                return;

            sr.SubPatternMatched++;
            if (sr.SubPatternMatched >= len)
            {
                sr.SubPatternMatched = len;
                NotifySubPatternStepFromMatched(slotIndex, ref sr);
                TryBeginFeverOnPatternComplete(slotIndex);
            }
            else
            {
                NotifySubPatternStepFromMatched(slotIndex, ref sr);
            }
        }

        void ResetSubPatternProgress(ref SlotRuntime sr)
        {
            sr.SubPatternMatched = 0;
            sr.FeverSubPatternStepTimer = 0f;
        }

        /// <summary>피버 중 `oiiaiooiiiai` 접두를 스텝마다 갱신·SFX 재생·랩.</summary>
        void TickFeverSubPatternReplay(int slotIndex)
        {
            ref SlotRuntime sr = ref _slots[slotIndex];
            if (sr.FeverRemaining <= 0f)
                return;

            int len = SubPatternLower.Length;
            if (len <= 0)
                return;

            float step = Mathf.Max(0.02f, feverSubPatternStepSeconds);
            sr.FeverSubPatternStepTimer += Time.deltaTime;

            while (sr.FeverSubPatternStepTimer >= step)
            {
                sr.FeverSubPatternStepTimer -= step;
                AdvanceFeverSubPatternStep(slotIndex, ref sr, len);
            }
        }

        void AdvanceFeverSubPatternStep(int slotIndex, ref SlotRuntime sr, int len)
        {
            sr.SubPatternMatched++;
            if (sr.SubPatternMatched > len)
                sr.SubPatternMatched = 1;

            NotifySubPatternStepFromMatched(slotIndex, ref sr);

            if (patternStepSfx != null && patternStepSfx.Length > 0)
                PlayPatternStepSfx((sr.SubPatternMatched - 1) % patternStepSfx.Length);
        }

        void FlushSubPatternGuideUi(SlotUiBindings ui, ref SlotRuntime sr)
        {
            if (ui.SubPatternGuideText == null)
                return;

            TMP_Text tmp = ui.SubPatternGuideText;
            tmp.richText = false;

            int matched = sr.SubPatternMatched;
            if (matched <= 0)
            {
                tmp.text = string.Empty;
                return;
            }

            int len = SubPatternLower.Length;
            if (len <= 0)
            {
                tmp.text = string.Empty;
                return;
            }

            if (matched > len)
                matched = len;

            tmp.text = SubPatternLower.Substring(0, matched).ToUpperInvariant();
        }
    }
}
