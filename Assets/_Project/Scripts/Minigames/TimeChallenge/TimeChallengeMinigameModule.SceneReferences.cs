using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        void ValidateSceneReferences()
        {
            if (roundLabel == null || targetTimeLabel == null || countdownLabel == null ||
                roundSting == null || roundTitle == null || hudCamera == null || targetLabelCanvas == null ||
                exitScreenFader == null || stopwatchClickSource == null || heartbeatSource == null ||
                stopwatchClickClip == null || heartbeatClip == null)
                Debug.LogWarning("[TimeChallenge] 공통 UI·페이드·오디오 Inspector 연결을 확인하세요.", this);
            for (int i = 0; i < SlotCount; i++)
                if (GetArray(playerClocks, i) == null || GetArray(playerTables, i) == null ||
                    GetArray(playerScoreBoards, i) == null || GetArray(playerSeatLabels, i) == null ||
                    GetArray(perfectHitVideos, i) == null)
                    Debug.LogWarning($"[TimeChallenge] {i + 1}P Inspector 연결을 확인하세요.", this);
        }
    }
}
