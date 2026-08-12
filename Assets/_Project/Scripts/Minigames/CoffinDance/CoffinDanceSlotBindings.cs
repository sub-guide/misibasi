using System;
using TMPro;
using UnityEngine;

namespace MiniParty.Minigames.CoffinDance
{
    /// <summary>
    /// 슬롯 프리팹 루트. 관·운구인·스코어 UI를 Module에 연결한다.
    /// Pallbearers[0..2]=좌 · [3..5]=우. Pose는 각 루트에서 자동 탐색.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoffinDanceSlotBindings : MonoBehaviour
    {
        [Header("3D")]
        [Tooltip("연출용 부모(운구인 포함). Yaw만 적용. 관 Z 기울기는 Rigidbody.")]
        public Transform TiltRoot;

        [Tooltip("관 Cube + CoffinDanceCoffinBody(Rigidbody).")]
        public Transform Coffin;

        [Tooltip("관 물리 컴포넌트. 비우면 Coffin에서 GetComponent.")]
        public CoffinDanceCoffinBody CoffinBody;

        [Tooltip("운구인 루트 6개. [0..2]=좌 · [3..5]=우.")]
        public Transform[] Pallbearers = new Transform[6];

        public Camera SlotCamera;

        [Header("UI")]
        public TMP_Text JumpPromptText;
        public TMP_Text ScoreText;
        public TMP_Text PracticeReadyText;
        public TMP_Text EliminatedText;
        public TMP_Text PlayerLabelText;

        public CoffinDanceCoffinBody ResolveCoffinBody()
        {
            if (CoffinBody != null)
                return CoffinBody;

            if (Coffin != null)
                CoffinBody = Coffin.GetComponent<CoffinDanceCoffinBody>();

            return CoffinBody;
        }

        public void PrepareAllPoses()
        {
            for (var i = 0; i < 6; i++)
                GetOrAddPoseAt(i)?.PrepareForGameplay();

            IgnoreCoffinFootCollisions();
        }

        /// <summary>
        /// 관 Collider ↔ 발/발가락 Collider 충돌 무시 (어깨 Sphere는 유지).
        /// </summary>
        public void IgnoreCoffinFootCollisions()
        {
            CoffinDanceCoffinBody coffinBody = ResolveCoffinBody();
            if (coffinBody == null)
                return;

            Collider[] coffinCols = coffinBody.GetComponentsInChildren<Collider>(true);
            if (coffinCols == null || coffinCols.Length == 0)
                return;

            for (var i = 0; i < 6; i++)
            {
                if (Pallbearers == null || i >= Pallbearers.Length || Pallbearers[i] == null)
                    continue;

                Collider[] cols = Pallbearers[i].GetComponentsInChildren<Collider>(true);
                for (var c = 0; c < cols.Length; c++)
                {
                    Collider foot = cols[c];
                    if (foot == null || !IsFootRelatedCollider(foot))
                        continue;

                    for (var k = 0; k < coffinCols.Length; k++)
                    {
                        Collider coffinCol = coffinCols[k];
                        if (coffinCol == null)
                            continue;

                        Physics.IgnoreCollision(coffinCol, foot, true);
                    }
                }
            }
        }

        public void ApplySideExtension(bool leftSide, float extension)
        {
            int start = leftSide ? 0 : 3;
            for (var i = start; i < start + 3; i++)
            {
                CoffinDancePallbearerPose pose = GetOrAddPoseAt(i);
                if (pose == null)
                    continue;

                pose.SetExtension(extension);
            }
        }

        /// <summary>지상 · 현재 프레임 → JumpStart 첫 프레임 CrossFade (Impulse 없음).</summary>
        public void BeginJumpAnim(float blendSeconds)
        {
            ForEachPose(p => p.BeginJumpStartBlend(blendSeconds));
        }

        /// <summary>페이드 종료: JumpStart 첫 프레임부터 재생 + Impulse 동시.</summary>
        public void CommitJumpAfterBlend(float impulseY, float startAnimSpeed)
        {
            ForEachPose(p => p.BeginJumpWithMotion(impulseY, startAnimSpeed));
        }

        /// <summary>페이드 0일 때: JumpStart 첫 프레임 + Impulse 즉시.</summary>
        public void BeginJump(float impulseY, float startAnimSpeed)
        {
            ForEachPose(p => p.BeginJumpWithMotion(impulseY, startAnimSpeed));
        }

        public float EnterJumpLand(float landAnimSpeed, float blendSeconds, float fallbackSeconds)
        {
            float blend = Mathf.Max(0f, blendSeconds);
            float duration = blend + fallbackSeconds / Mathf.Max(0.01f, landAnimSpeed);
            ForEachPose(p =>
            {
                p.PlayJumpLand(landAnimSpeed, blend);
                duration = blend + p.GetPlayingStateDurationOr(
                    CoffinDancePallbearerPose.StateJumpLand,
                    fallbackSeconds,
                    landAnimSpeed);
            });
            return Mathf.Max(0.01f, duration);
        }

        public void EnterExtensionBlend(float blendSeconds)
        {
            ForEachPose(p =>
            {
                p.ClearLandYOffset();
                p.PlayExtensionBlend(blendSeconds);
            });
        }

        /// <summary>Land Y 오프셋 점진 이동 시작. duration≤0이면 즉시.</summary>
        public void BeginLandYOffsetLerp(float offset, float duration)
        {
            ForEachPose(p => p.BeginLandYOffsetLerp(offset, duration));
        }

        public void TickLandYOffsetLerp(float dt)
        {
            ForEachPose(p => p.TickLandYOffsetLerp(dt));
        }

        /// <summary>즉시 스냅(점진 이동은 BeginLandYOffsetLerp).</summary>
        public void ApplyLandYOffset(float offset)
        {
            ForEachPose(p => p.SetLandYOffset(offset));
        }

        public void ClearLandYOffset()
        {
            ForEachPose(p => p.ClearLandYOffset());
        }

        public void SoftResetAllPallbearers()
        {
            ForEachPose(p =>
            {
                p.SoftResetPhysics();
                p.ResetPoseAnim();
            });
        }

        public void SetPallbearerSimulationActive(bool active)
        {
            ForEachPose(p => p.SetSimulationActive(active));
        }

        /// <summary>전원 이탈 후 재접지(착지)면 true.</summary>
        public bool AreAllReadyToLand()
        {
            bool any = false;
            bool ready = true;
            ForEachPose(p =>
            {
                any = true;
                if (!p.LeftGroundSinceJump || !p.IsGrounded)
                    ready = false;
            });
            return any && ready;
        }

        /// <summary>전원 JumpStart 클립 종료. 끝나기 전엔 Land로 가지 않음.</summary>
        public bool HasJumpStartCompleted()
        {
            return AllPosesCompleted(CoffinDancePallbearerPose.StateJumpStart);
        }

        public bool HasJumpLandCompleted()
        {
            return AllPosesCompleted(CoffinDancePallbearerPose.StateJumpLand);
        }

        bool AllPosesCompleted(string stateName)
        {
            bool any = false;
            bool allDone = true;
            ForEachPose(p =>
            {
                any = true;
                if (!p.HasAnimStateCompleted(stateName))
                    allDone = false;
            });
            return any && allDone;
        }

        void ForEachPose(System.Action<CoffinDancePallbearerPose> action)
        {
            for (var i = 0; i < 6; i++)
            {
                CoffinDancePallbearerPose pose = GetOrAddPoseAt(i);
                if (pose != null)
                    action(pose);
            }
        }

        CoffinDancePallbearerPose GetOrAddPoseAt(int pallbearerIndex)
        {
            if (Pallbearers == null || pallbearerIndex < 0 || pallbearerIndex >= Pallbearers.Length)
                return null;

            return GetOrAddPose(Pallbearers[pallbearerIndex]);
        }

        static CoffinDancePallbearerPose GetOrAddPose(Transform root)
        {
            if (root == null)
                return null;

            var pose = root.GetComponent<CoffinDancePallbearerPose>();
            if (pose == null)
                pose = root.gameObject.AddComponent<CoffinDancePallbearerPose>();
            return pose;
        }

        /// <summary>Foot/Toe 본 계열 Collider만 (어깨 Arm Sphere는 제외).</summary>
        static bool IsFootRelatedCollider(Collider col)
        {
            Transform t = col.transform;
            int depth = 0;
            while (t != null && depth < 8)
            {
                string n = t.name;
                if (n.IndexOf("Foot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Toe", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                // 어깨/팔이면 발이 아님
                if (n.IndexOf("Arm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Shoulder", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Hand", StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;

                t = t.parent;
                depth++;
            }

            return false;
        }
    }
}
