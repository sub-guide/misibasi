using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.TimeChallenge
{
    public sealed partial class TimeChallengeMinigameModule
    {
        void PlayRoundAdvanceEffect()
        {
            if (roundSting == null) return;
            int n = _roundIndex + 1;
            roundSting.Play(n >= RoundCount ? $"FINAL ROUND {n}" : $"ROUND {n}",
                n <= 1 ? "카운트다운이 끝나면 스톱워치가 바로 시작됩니다" : string.Empty,
                n >= RoundCount, n <= 1 ? Mathf.Max(0.35f, briefingSeconds) : 1.15f);
        }

        void RefreshHud()
        {
            if (roundLabel != null) roundLabel.text = $"ROUND {_roundIndex + 1} / {RoundCount}";
            if (targetTimeLabel != null) targetTimeLabel.text = $"목표  {GetRoundTargetSeconds():0.00}초";
            if (countdownLabel != null) countdownLabel.text = CountdownText();
            if (statusLabel != null) statusLabel.gameObject.SetActive(false);
            RefreshClocks();
        }

        void SnapScoreBoards()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                TimeChallengeScoreBoard board = GetArray(playerScoreBoards, i);
                if (board != null)
                    board.SnapTo(_totalScores[i]);
            }
        }

        void PlayScoreBoardAwards()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                TimeChallengeScoreBoard board = GetArray(playerScoreBoards, i);
                if (board == null)
                    continue;

                if (_participatedMask[i])
                    board.AnimateTo(_totalScores[i], scoreCountDuration);
                else
                    board.SnapTo(0);
            }
        }

        void TickScoreBoards(float unscaledDeltaTime)
        {
            for (var i = 0; i < SlotCount; i++)
            {
                TimeChallengeScoreBoard board = GetArray(playerScoreBoards, i);
                if (board != null)
                    board.Tick(unscaledDeltaTime);
            }
        }

        void ApplyOccupiedSlots()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                bool occupied = _participatedMask[i];

                TimeChallengeLedClock clock = GetArray(playerClocks, i);
                if (clock != null)
                {
                    clock.gameObject.SetActive(true);
                    clock.SetVacant(!occupied);
                }

                TimeChallengeTableButton table = GetArray(playerTables, i);
                if (table != null)
                    table.gameObject.SetActive(true);

                TimeChallengeScoreBoard board = GetArray(playerScoreBoards, i);
                if (board != null)
                    board.gameObject.SetActive(true);

                ApplySeatLabelPower(i, occupied);
            }
        }

        void ApplySeatLabelPower(int slotIndex, bool occupied)
        {
            TMP_Text label = playerSeatLabels[slotIndex];
            if (label == null)
                return;

            if (!_playerSeatRestCached[slotIndex])
            {
                _playerSeatRest[slotIndex] = label.color;
                _playerSeatRestCached[slotIndex] = true;
            }

            Color rest = _playerSeatRest[slotIndex];
            label.color = occupied
                ? rest
                : new Color(rest.r * 0.28f, rest.g * 0.32f, rest.b * 0.28f, Mathf.Max(0.18f, rest.a * 0.28f));
        }

        void RefreshClocks()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                TimeChallengeLedClock clock = GetArray(playerClocks, i);
                if (clock == null)
                    continue;

                if (!_participatedMask[i])
                {
                    clock.SetVacant(true);
                    continue;
                }

                bool covered = _phase == Phase.Running && _clockStarted[i];
                clock.SetCovered(covered);

                if (_phase == Phase.Result)
                {
                    if (_stopped[i])
                        clock.ShowLit(_records[i]);
                    else if (_clockStarted[i])
                        clock.ShowMiss();
                    else
                        clock.ShowOff();
                    continue;
                }

                if (_phase == Phase.Running && _clockStarted[i])
                {
                    if (_stopped[i])
                        clock.ShowLit(_records[i]);
                    else
                        clock.ShowRunning(Time.realtimeSinceStartupAsDouble - _slotStartRealtime[i]);
                    continue;
                }

                clock.ShowOff();
            }
        }

        string CountdownText()
        {
            switch (_phase)
            {
                case Phase.Countdown:
                    return CountdownDisplayNumber().ToString();
                case Phase.Running:
                    return _phaseElapsed < Mathf.Max(0.05f, goVisibleSeconds) ? "GO" : string.Empty;
                default:
                    return string.Empty;
            }
        }

        static T GetArray<T>(T[] arr, int i) where T : class
        {
            if (arr == null || i < 0 || i >= arr.Length)
                return null;

            return arr[i];
        }

        void CacheTargetLabelRest()
        {
            if (_targetRestCached || targetTimeLabel == null)
                return;

            RectTransform rt = targetTimeLabel.rectTransform;
            _targetRestPos = rt.anchoredPosition;
            _targetRestScale = rt.localScale;
            if (_targetRestScale.sqrMagnitude < 0.0001f)
                _targetRestScale = Vector3.one;
            _targetRestAlign = targetTimeLabel.alignment;
            _targetRestCached = true;
        }

        void ShowTargetLabelCloseup()
        {
            if (targetTimeLabel == null || roundTitle == null || hudCamera == null || targetLabelCanvas == null)
                return;

            CacheTargetLabelRest();
            _targetCloseupPos = _targetRestPos;
            if (roundTitle != null && hudCamera != null &&
                targetTimeLabel.rectTransform.parent is RectTransform parent)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(hudCamera, roundTitle.position);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, hudCamera, out Vector2 local))
                    _targetCloseupPos = local + new Vector2(0f, -TargetCloseupBelowTitle);
            }
            SetTargetLabelOnTop(true);
            _targetSettling = false;
            _targetAnim = 0f;
            _targetAnimFrom = 0f;
            _targetAnimTo = 0f;
            _targetAnimElapsed = 0f;
            ApplyTargetLabelPose(0f, 0f);
        }

        void SettleTargetLabel()
        {
            if (targetTimeLabel == null)
                return;

            CacheTargetLabelRest();
            _targetSettling = true;
            _targetAnimFrom = _targetAnim;
            _targetAnimTo = 1f;
            _targetAnimElapsed = 0f;
        }

        void TickTargetLabel(float dt)
        {
            if (targetTimeLabel == null || !_targetRestCached)
                return;

            if (!_targetSettling)
            {
                ApplyTargetLabelPose(_targetAnim, 0f);
                return;
            }

            _targetAnimElapsed += Mathf.Max(0f, dt);
            float t = Mathf.Clamp01(_targetAnimElapsed / TargetSettleSeconds);
            float posT = t * t * (3f - 2f * t);
            _targetAnim = Mathf.Lerp(_targetAnimFrom, _targetAnimTo, posT);
            ApplyTargetLabelPose(_targetAnim, t);
            if (t >= 1f)
            {
                _targetSettling = false;
                SetTargetLabelOnTop(false);
            }
        }

        void ApplyTargetLabelPose(float t, float settleRaw)
        {
            RectTransform rt = targetTimeLabel.rectTransform;
            rt.anchoredPosition = Vector2.Lerp(_targetCloseupPos, _targetRestPos, t);
            float scaleT = 1f - Mathf.Pow(1f - Mathf.Clamp01(settleRaw > 0.001f ? settleRaw : t), 2.4f);
            if (t <= 0.001f)
                scaleT = 0f;
            rt.localScale = Vector3.Lerp(_targetRestScale * TargetCloseupScale, _targetRestScale, scaleT);
            targetTimeLabel.alignment = t < 0.72f
                ? TextAlignmentOptions.Center
                : _targetRestAlign;
        }
        void SetTargetLabelOnTop(bool onTop)
        {
            if (targetLabelCanvas == null) return;
            targetLabelCanvas.overrideSorting = onTop;
            targetLabelCanvas.sortingOrder = onTop ? 50 : 0;
        }
    }
}
