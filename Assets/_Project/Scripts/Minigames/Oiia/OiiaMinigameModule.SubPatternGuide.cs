using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>
        /// 맞춘 접두만 대문자로 표시. 12글자를 채우면 보너스 후 진행도 0.
        /// 반환값: 이번 입력의 1-based 위치(스텝 SFX). 진행 없으면 0.
        /// 리셋 후에도 반환값으로 마지막 글자 SFX를 재생한다.
        /// </summary>
        int AdvanceSubPatternOnHit(int slotIndex, ref SlotRuntime sr, out bool patternCompleted)
        {
            patternCompleted = false;

            int len = SubPatternLower.Length;
            if (len <= 0)
            {
                sr.SubPatternMatched = 0;
                return 0;
            }

            sr.SubPatternMatched++;
            int step = sr.SubPatternMatched;
            NotifySubPatternStepFromMatched(slotIndex, ref sr);

            if (sr.SubPatternMatched >= len)
            {
                patternCompleted = true;
                sr.SubPatternMatched = 0;
            }

            return step;
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
