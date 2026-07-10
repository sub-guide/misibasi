using System.Collections;
using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Result
{
    public sealed partial class ResultFlowController
    {
        readonly bool[] _readyMask = new bool[4];

        IEnumerator CoReadyPhase()
        {
            _phase = ResultPhase.Ready;

            for (var i = 0; i < 4; i++)
                _readyMask[i] = false;

            Debug.Log("[ResultFlowController] Ready: 참가 슬롯 START 토글 → 전원 READY 후 운영자 Enter.");

            while (_running && _phase == ResultPhase.Ready)
            {
                TickReadyInput();

                if (_operatorInput.Confirm && AllParticipatedReady())
                {
                    yield return CoExitToMainMenu();
                    yield break;
                }

                yield return null;
            }
        }

        void TickReadyInput()
        {
            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                if (_willGameOver[i])
                    continue;

                if (!SlotGamepad.StartPressed(i))
                    continue;

                _readyMask[i] = !_readyMask[i];
                GetSlotView(i)?.SetReadyBorder(_readyMask[i]);
            }
        }

        bool AllParticipatedReady()
        {
            bool anyNeedReady = false;
            bool allReady = true;

            for (var i = 0; i < 4; i++)
            {
                if (!_playedMask[i])
                    continue;

                if (_willGameOver[i])
                    continue;

                anyNeedReady = true;

                if (!_readyMask[i])
                    allReady = false;
            }

            // 전원 GAME OVER 등 Ready 대상이 없으면 Enter 허용
            if (!anyNeedReady)
                return true;

            return allReady;
        }
    }
}
