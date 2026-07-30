# 05_Coffin_Dance (관짝춤)

> **문서 기준일**: 2026-07-31 — 실모델 Animator Idle↔Crouch · FailFloor · **Play 검증 완료**.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.  
> **과거 Capture/본 Slerp/각도 Stumble/밸런스 게이지/HumanDummy ProtoType** 은 폐기. 최신 진실은 아래 §0·§3.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| C# 게임 로직 | **완료·검증** | Animator Extension · FailFloor · 자유 점프 |
| Flow·Result 연동 | **유지** | `GameFlowDirector` · `PartySession` · Result flavor |
| 운구인 에셋 | **완료·검증** | `Pallbearer.fbx` + UAL1 · **우어깨만** SphereCollider · 반대편 **Scale X=-1** |
| 씬·슬롯 프리팹 | **완료·검증** | `Pallbearers[6]`만 (`Left/Right Pose` 배열 **없음**) |
| Build Settings | **등록·검증** | 사용자 확인 |
| 메뉴 진입 테스트 | **검증** | MainMenu→관짝춤 |

### 다음 세션이 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 자세 | Capture 없음 · `PallbearerPose.controller` 1D Blend (`Extension` 0=Crouch · 1=Idle) |
| FBX 클립명 | Unity에선 `Armature\|Idle_Loop` / `Armature\|Crouch_Idle_Loop` (생성 메뉴가 `\|` 접미사 매칭) |
| 애니 소스 | `UAL1_Standard.fbx` (**RM 파일 쓰지 않음**) |
| Edit Mode 미리보기 | **의도적 비활성** (AnimationMode가 Prefab Transform 오염) · Play에서만 블렌드 |
| 어깨 Collider | 검증 기준 **RightArm만** · 관 반대편 운구인은 Scale 반전으로 맞춤 |
| Slot 연결 | `Pallbearers[0..2]`=←쪽 · `[3..5]`=→쪽 |
| 실패 | 각도 Stumble 아님 · `CoffinDanceFailFloor` 접촉 |
| 레거시 | `PallbearerProtoType`(HumanDummy) · Capture Rest/Crouch — **참고만, 실사용 아님** |

---

## 1. 한 줄 요약

4인 세로 분할 슬롯에서 **6명 운구인 어깨 Collider 위 관(Rigidbody)** 균형을 ←/→ 무릎 자세로 유지하고, **A로 자유 점프**하는 60초 타임어택.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`1` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| 좌 | `stick/left` | `A` | 좌측 운구인 무릎 펴기($Y_L$↑) · 우측 낮춤 |
| 우 | `stick/right` | `D` | 우측 운구인 무릎 펴기($Y_R$↑) · 좌측 낮춤 |
| 점프 | `button2` (Face A) | `H` | **자유 점프** (프롬프트 없음) |
| 연습 READY | Start | `B` | — |
| 본게임 전환 | 운영자 Enter | — | — |

extension 범위는 **0(앉음) ~ 1(기립)** 만. 반대쪽을 낮추며 관을 기울인다.

---

## 3. 물리 (순수 충돌)

### 관 (`CoffinDanceCoffinBody`)

- `Rigidbody` + Collider
- `Freeze Position`: **X, Z** (Y만 이동)
- `Freeze Rotation`: **X, Y** (Z만 회전)
- `centerOfMass` 로컬 Y를 지지점보다 살짝 높게 (Inspector)
- 기울기(점수용): `transform` 로컬 Z 각(도)
- **실패**: `CoffinDanceFailFloor` Collider에 닿으면 본게임 ELIMINATED / 연습 SoftReset

### FailFloor (`CoffinDanceFailFloor`)

- **슬롯 프리팹 자식**으로 둔다 (씬 공용 Floor 금지 — 슬롯 X 분리)
- BoxCollider + 마커 컴포넌트 · 관이 넘어지면 닿는 높이

### 운구인 (`CoffinDancePallbearerPose`)

- Animator **1D Blend**: `Extension` 0=`Crouch_Idle_Loop` · 1=`Idle_Loop` (`Assets/CoffinDance/Animations/UAL1_Standard.fbx`, **RM 아님**)
- 클립 에셋명: `Armature|Idle_Loop` / `Armature|Crouch_Idle_Loop`
- Controller: `Assets/CoffinDance/Animations/PallbearerPose.controller`  
  (재생성: 메뉴 `Mini Party/Coffin Dance/Create Pallbearer Animator`)
- `applyRootMotion = false` · Module `SetExtension` → `Animator.SetFloat("Extension")`
- Capture / 본 Slerp / 발 플랜트 / Edit Mode 애니 미리보기 **없음** (Play에서만 구동)
- 어깨 지지: **`mixamorig:RightArm` SphereCollider** (검증 배치). 관 반대편 운구인은 **Scale X=-1** 반전
- 점프: Extension dip + 루트 Y 홉 · 착지 Impulse
- 모델: `Assets/CoffinDance/Pallbearer.fbx` → `Prefabs/Pallbearer.prefab`
---

## 4. 점수

| 항목 | 값 |
|------|-----|
| 생존 | 초당 **100** |
| 중앙 유지 (`|기울기| ≤ 10°`) | 초당 **50** |
| Phase4 (50~60초) | **전체 ×2.0** |

자유 점프에는 별도 성공 점수 없음. 연습 UI 점수는 `-`, Report `FinalScore`는 0.

---

## 5. JUMP (자유)

- `A` 언제든 (지상·비잠금 시)
- `jumpLockoutSeconds`(기본 **0.35**) 동안 ←/→ 불가 · 공중
- 착지 순간 `CoffinDanceCoffinBody.ApplyLandingImpulse`
- 구버전 전역 JUMP! 프롬프트·더블점프 스케줄 **제거**

---

## 6. Phase (본게임 60초)

| 구간 | Phase | 어깨 승강 | 점수 |
|------|-------|-----------|------|
| 0~20초 | 1 | ×1 | ×1 |
| 20~40초 | 2 | `phase2ShoulderMul` | ×1 |
| 40~50초 | 3 | `phase3ShoulderMul` | ×1 |
| 50~60초 | 4 | `phase4ShoulderMul` | ×2 |

Stumble(각도 한도) **제거**.  
실패: 관이 슬롯 `CoffinDanceFailFloor`에 접촉 → 본게임 ELIMINATED / 연습 SoftReset.

전원 탈락 또는 60초 → **1초**(`SessionEndDelaySeconds`) 후 Results.

---

## 7. HP (`CoffinDanceHpLossRules`)

- **1인**: 총점 `< hpLowScoreThreshold`(기본 **3000**) → HP −1
- **2인 이상**: 하위 50%만 −1
- 탈락자: 탈락 시점 점수로 순위 (`_participatedMask` 유지)

---

## 8. 연습 → 본게임

OIIA와 동일: START READY → 운영자 Enter → `PrepareRound(false)` + `Begin` 재호출.  
메뉴 첫 진입: `PartySession.TakeCoffinDanceNextRoundIsPractice()`.

---

## 9. 주요 타입·파일

| 파일 | 역할 |
|------|------|
| `CoffinDanceMinigameModule` (+ partial) | `IMinigameModule` |
| `CoffinDanceCoffinBody` | 관 Rigidbody·CoM·착지 Impulse |
| `CoffinDancePallbearerPose` | Animator Extension 블렌드 · 점프 홉 (Edit Mode 미구동) |
| `CoffinDanceFailFloor` | 실패 바닥 마커 |
| `CoffinDanceSceneBootstrap` | Begin/Tick |
| `CoffinDanceSlotBindings` | `Pallbearers[6]` · PrepareAllPoses / ApplySideExtension |
| `CoffinDanceHpLossRules` | HP 판정 |
| `CoffinDanceResultMinigameFlavor` | Result ID 매칭 |
| Editor `CoffinDancePallbearerAnimatorSetup` | Controller 생성 메뉴 |

`BuiltInId` = `"coffin_dance"` · DisplayName 기본 `"관짝춤"`.  
실사용 에셋: `Assets/CoffinDance/` (`Pallbearer.fbx`, `Animations/UAL1_Standard.fbx`, `PallbearerPose.controller`).  
레거시: `PallbearerProtoType` / Kevin Iglesias HumanDummy — **미사용**.

---

## 10. Inspector 필드 (Module)

| 필드 | 기본 | 설명 |
|------|------|------|
| `slotBindings[4]` | — | `CoffinDanceSlotBindings` |
| `mainRoundTimerCentralTop` | — | 중앙 타이머 TMP |
| `phaseLabelText` | — | Phase TMP |
| `initialTiltDegrees` | 6 | SoftReset 초기 기울기 |
| `initialAngularSpeed` | 25 | SoftReset 초기 각속도 |
| `shoulderRaiseSpeed` | 1.4 | ←/→ extension 속도 |
| `neutralExtension` | 0.5 | 시작·중립 무릎 (0=앉음 · 1=기립) |
| `shoulderReturnSpeed` | 1.1 | 중립 복귀 |
| `jumpLockoutSeconds` | 0.35 | 점프 중 조작 불능 |
| `phase2/3/4ShoulderMul` | 1.25/1.55/2 | Phase 민감도 |
| `hpLowScoreThreshold` | 3000 | 1P 저점수 컷 |
| `presentationYawDegrees` | 22 | TiltRoot Y 회전 |
| `slotWorldSpacing` | 40 | 슬롯 X 분리 |
| `disableMainCameraOnBegin` | true | Main Camera off |
| `bindSlotCanvasesToSlotCamera` | true | 슬롯 Canvas → SSC |
| `exitScreenFader` | — | FadeOverlay |
| `coffinDanceSceneName` (GameFlow) | `Minigame_CoffinDance` | 로드 씬명 |

### SlotBindings

| 필드 | 용도 |
|------|------|
| `TiltRoot` | yaw(Y)만 |
| `Coffin` / `CoffinBody` | 관 Transform · `CoffinDanceCoffinBody` |
| `Pallbearers[6]` | [0..2]=좌 · [3..5]=우 · Pose는 각 루트에서 자동 |
| `SlotCamera` | 세로 1/4 · 로우앵글은 에디터 |
| HUD | Score · PracticeReady · Eliminated (`JumpPromptText`·게이지는 비활성) |

### CoffinBody

| 필드 | 용도 |
|------|------|
| `centerOfMassLocal` | 기본 (0, 0.15, 0) |
| `landingTorqueImpulse` | 착지 Z 토크 |

---

## 11. 목표 Hierarchy (참고명)

```
Minigame_CoffinDance
├── CoffinDance_Root
│   ├── CoffinDanceMinigameModule
│   └── CoffinDanceSceneBootstrap
├── Slot_1P ~ Slot_4P  (CoffinDanceSlotBindings)
│   ├── TiltRoot (Y≈22°)
│   │   ├── Pallbearer ×6 (실모델 + Animator + CoffinDancePallbearerPose)
│   │   │     └── mixamorig:RightArm + SphereCollider (반대편은 Scale X=-1)
│   │   └── Coffin (Cube + Rigidbody + CoffinDanceCoffinBody)
│   ├── FailFloor (BoxCollider + CoffinDanceFailFloor)
│   └── SlotCamera (로우앵글)
├── Canvas (Score·Timer·Phase)
└── FadeOverlay (ScreenFader)
```

---

문서 갱신: **2026-07-31** (다음 세션 인수인계용 문서 정합 · 우어깨·Edit미리보기·에셋 경로)
