using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        /// <summary>
        /// 레거시 SequenceText A안과 동일: 맞춘 접두만 대문자로 표시.
        /// 폰트 크기·색·다음 글자 미리보기 없음.
        /// 완성 시 피버 진입. 피버 종료/미스 시 진행도를 초기화.
        /// 반환값: 이번 입력으로 진행한 글자의 1-based 위치(스텝 SFX 인덱스용). 진행 없으면 0.
        /// 완성 → 피버 진입으로 <see cref="SlotRuntime.SubPatternMatched"/>가 0으로 리셋돼도
        /// 반환값으로 마지막 글자 SFX를 재생할 수 있다.
        /// </summary>
        int AdvanceSubPatternOnHit(int slotIndex, ref SlotRuntime sr)
        {
            if (sr.FeverRemaining > 0f)
            {
                int feverLen = SubPatternLower.Length;
                if (feverLen <= 0)
                {
                    sr.SubPatternMatched = 0;
                    return 0;
                }

                sr.SubPatternMatched++;
                if (sr.SubPatternMatched > feverLen)
                    sr.SubPatternMatched = 1;

                NotifySubPatternStepFromMatched(slotIndex, ref sr);
                return sr.SubPatternMatched;
            }

            int len = SubPatternLower.Length;
            if (len <= 0)
            {
                sr.SubPatternMatched = 0;
                return 0;
            }

            // T2는 피버 없이 입력 패턴·가이드·스피커·SFX만 1~12로 계속 반복.
            if (ResolveGlobalTier() == 2)
            {
                sr.SubPatternMatched++;
                if (sr.SubPatternMatched > len)
                    sr.SubPatternMatched = 1;

                NotifySubPatternStepFromMatched(slotIndex, ref sr);
                return sr.SubPatternMatched;
            }

            if (sr.SubPatternMatched >= len)
                return 0;

            sr.SubPatternMatched++;
            int steppedPosition = sr.SubPatternMatched;
            if (sr.SubPatternMatched >= len)
            {
                sr.SubPatternMatched = len;
                steppedPosition = len;
                NotifySubPatternStepFromMatched(slotIndex, ref sr);
                TryBeginFeverOnPatternComplete(slotIndex);
            }
            else
            {
                NotifySubPatternStepFromMatched(slotIndex, ref sr);
            }

            return steppedPosition;
        }

        void ResetSubPatternProgress(ref SlotRuntime sr)
        {
            sr.SubPatternMatched = 0;
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
