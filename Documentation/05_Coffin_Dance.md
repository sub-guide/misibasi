# 05_Coffin_Dance (관짝춤)

> **문서 기준일**: 2026-08-16 — 시소 입력 **LB/RB**. 관 CoM 기하 중심. FailFloor 탈락 없음 · +Y Impulse · 60초 종료.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.  
> **과거 Capture/본 Slerp/각도 Stumble/밸런스 게이지/HumanDummy ProtoType/자유 점프/FailFloor 탈락/CoM Y 0.15 오프셋** 는 폐기. 최신 진실은 아래 §0·§3.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| C# 게임 로직 | **FailFloor Impulse + 어깨 Ignore** | 탈락 없음 · +Y Impulse · 바닥 접촉 후 0.3초 어깨 Ignore **구현·Play 미검증**. CoM `(0,0,0)`. 패널티 수치 보류 |
| 점프 | **제거·Play 검증** | A로 점프 안 됨 (사용자, 2026-08-15) |
| Animator | **ExtensionBlend만** | Jump 상태 에디터 삭제 완료 (사용자, 2026-08-15) |
| Flow·Result 연동 | **60초 종료 검증** | 연습 READY→본 · FailFloor 탈락 경로 없음. 60초 → Results (사용자, 2026-08-15) |
| 운구인 에셋 | **완료·검증** | `Pallbearer.fbx` + UAL1 · **우어깨만** SphereCollider(**Radius 27**) · 반대편 **Scale X=-1** · **루트 RB·발 Collider 없음**(사용자, 2026-08-15) |
| 씬·슬롯 프리팹 | **완료·검증** | `Pallbearers[6]`만 (`Left/Right Pose` 배열 **없음**) |
| Build Settings | **등록·검증** | 사용자 확인 |
| 메뉴 진입 테스트 | **검증** | MainMenu→관짝춤 |
| 시소·노이즈 개편 | **완료·검증** | 시소 Hold · Sine 씰룩임 · 걷기 클립 · Loop Time (사용자, 2026-08-05) |
| 좌우 조작 개편 | **완료·검증** | 수치 확정 · 중력형 도치 · 어깨 Radius 27 (2026-08-07) |
| 관 물리 경로 | **완료·검증** | 어깨 SphereCollider + 중력만 (2026-08-08 Play) |

### 다음 세션이 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 자세 | Capture 없음 · `ExtensionBlend` 1D Blend만 |
| 어깨 높이 | **단일 시소** `x` · `Y_L=x` · `Y_R=1-x` (합=1, 순수 Z 기울기) |
| 입력 Hold | 키를 떼도 `x_bias` **즉시 중립 복귀하지 않음** · **기운 쪽으로 중력형 미세 도치** + 비선형 풀 |
| 시소 입력 | **LB/RB** (`button5`/`button6`). 개발 키보드 **Q/E**. 스틱 좌우는 안 씀 |
| 점프 | **없음**. A(`button2`) 미사용 |
| 운구인 물리 | **루트 Rigidbody 없음** · **발/Toe Collider 없음** · 어깨 SphereCollider만 (정적 Collider로 관을 받침) |
| 자율 흔들림 | **Sine만** 즉시 반영 · 고정 `noiseAmp` (Phase별 난이도 후속) · Rate Limiter **없음** |
| FBX 클립명 | Unity에선 `Armature\|Walk_Formal_Loop` / `Armature\|Crouch_Fwd_Loop` (생성 메뉴가 `\|` 접미사도 매칭) |
| Controller 재생성 | 메뉴가 **기존 에셋 재사용**(GUID 유지) · 프리팹 Animator 재연결 불필요 |
| 클립 Loop | `Crouch_Fwd_Loop` · `Walk_Formal_Loop` **Loop Time ON** (사용자 수동, 2026-08-05 검증) |
| 애니 소스 | `UAL1_Standard.fbx` (**RM 파일 쓰지 않음**) |
| Edit Mode 미리보기 | **의도적 비활성** · Play에서만 블렌드 |
| 어깨 Collider | **RightArm만** · `Pallbearer.prefab` SphereCollider **Radius 27** (25→27, 2026-08-07) · 반대편 Scale 반전 |
| Slot 연결 | `Pallbearers[0..2]`=←쪽 · `[3..5]`=→쪽 |
| 실패 | 탈락 없음 · FailFloor 접촉 1회 → 월드 +Y Impulse |
| 관 운동 | 어깨 SphereCollider + 중력 + FailFloor `AddForce(+Y, Impulse)` · 씬 `failFloorUpwardImpulse` **1** (코드 기본 80) |
| 관 CoM | `centerOfMassLocal` **(0,0,0)** = 기하 중심. 예전 `(0, 0.15, 0)` 오프셋 **제거**(뒤집혀 어깨에 걸리던 원인) |
| 레거시 | `PallbearerProtoType`(HumanDummy) · Capture Rest/Crouch · Jump FSM — **참고만** |

---

## 1. 한 줄 요약

4인 세로 분할 슬롯에서 **6명 운구인 어깨 Collider 위 관(Rigidbody)** 균형을 LB/RB 시소로 유지하는 60초 타임어택.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`Ctrl` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| LB (L) | `ShoulderL` (`button5`) | `Q` | `x_bias`↑ → `Y_L`↑ · `Y_R`↓ (홀드 시 가속) |
| RB (R) | `ShoulderR` (`button6`) | `E` | `x_bias`↓ → `Y_R`↑ · `Y_L`↓ (홀드 시 가속) |
| 연습 READY | Start | `B` | — |
| 본게임 전환 | 운영자 Enter | — | — |

키를 떼면 `x_bias`는 중립으로 스냅되지 않는다. 미입력(또는 LB·RB 동시)이면 **현재 기울기 방향 중력형 미세 도치**가 들어가고, 중앙 이탈 시 **비선형 풀**이 더해진다.  
extension 범위는 **0(앉음) ~ 1(기립)** · `Y_L + Y_R = 1` 항상 유지.

---

## 3. 물리 (순수 충돌 + 시소 제어)

### 관 (`CoffinDanceCoffinBody`)

- `Rigidbody` + Collider
- `Freeze Position`: **X, Z** (Y만 이동)
- `Freeze Rotation`: **X, Y** (Z만 회전)
- `centerOfMass` 로컬 **(0,0,0)** = 기하 중심 (Inspector `centerOfMassLocal`)
- 기울기(점수용): `transform` 로컬 Z 각(도)
- **FailFloor 접촉** (`OnCollisionEnter` 1회): 월드 `Vector3.up` Impulse + `failFloorShoulderIgnoreSeconds`(기본 **0.3**) 동안 관↔어깨 SphereCollider `IgnoreCollision`. 본게임만 `failFloorPenaltyScore` 감점. **탈락·연습 SoftReset 없음**
- **플레이 중 운동**: 운구인 어깨 SphereCollider 충돌 + 중력 + FailFloor +Y Impulse. 바닥 접촉 직후 잠시 어깨 충돌 무시. Y 회전 Freeze 유지 (시소는 Z만)

### FailFloor (`CoffinDanceFailFloor`)

- **슬롯 프리팹 자식**으로 둔다 (씬 공용 Floor 금지 — 슬롯 X 분리)
- BoxCollider + 마커 컴포넌트 · 관이 넘어지면 닿는 높이
- 접촉이 유지되는 동안 Impulse는 재적용하지 않음. 떨어졌다가 다시 닿으면 다시 Impulse·감점·어깨 Ignore 타이머 갱신
- 접촉 직후 `failFloorShoulderIgnoreSeconds` 동안 관이 어깨를 통과해 올라감. 만료 시 충돌 복구. `Begin`에서도 복구

### 시소 제어 (Module · A안)

| 변수 | 역할 |
|------|------|
| `x_bias` (`SeesawBias`) | 좌우·미세 도치·비선형 풀로 변경 · `Clamp01` |
| `HoldTimer` | 슬롯별 단일 홀드 누적(초). 미입력·동시 입력 시 0 |
| `DanceWave` | `Sin(2π · danceSineHz · Time.time)` · [-1, 1] |
| `x` (`SeesawXCurrent`) | `Clamp01(x_bias + DanceWave × noiseAmp)` (즉시) |
| `Y_L` / `Y_R` | `x` / `1 - x` |

#### 조작 파라미터 (Inspector · **Play 검증 확정 2026-08-07**)

| 필드 | 기본(=씬) | 설명 |
|------|-----------|------|
| `seesawBaseSpeed` | 1.2 | LB/RB 단일 입력 기본 탭 이동 속도 |
| `holdMaxMultiplier` | 3.0 | 홀드 시 최대 가속 배율 |
| `holdAccelTime` | 0.2 | 최대 가속 도달 시간(초) |
| `microDriftSpeed` | 0.5 | 미입력·동시 입력 시 현재 기울기 방향 중력형 도치 속도 |
| `pullCoefficient` | 2.0 | 중앙(0.5) 이탈 비선형 가속 계수 |

#### `x_bias` 프레임별 연산 (`StepShoulderControl`)

1. **입력**
   - **미입력 또는 LB·RB 동시**: `HoldTimer = 0` · `driftDir = Sign(x_bias - 0.5)`(정중앙이면 0) · `x_bias += driftDir × microDriftSpeed × dt` (기운 쪽으로 계속 밀림 = 중력형)
   - **LB 또는 RB 단일**: `HoldTimer += dt` · `speedMul = Lerp(1, holdMaxMultiplier, HoldTimer / holdAccelTime)` · `x_bias += inputDir × seesawBaseSpeed × speedMul × dt` (`inputDir`: LB=+1, RB=-1)
2. **비선형 이탈 가속 (공통)**: `offset = x_bias - 0.5` · `pullForce = pullCoefficient × offset² × Sign(offset)` · `x_bias += pullForce × dt`
3. **범위**: `x_bias = Clamp01(x_bias)` · 0.0/1.0 한계에 닿으면 어깨 기울기로 FailFloor 접촉(Impulse·패널티) 가능

#### 리셋 (Edge Case)

`Begin` → `ResetSeesawToNeutral`: `HoldTimer = 0` · `x_bias = x_current = xSeesawNeutral`(기본 0.5). 플레이 중 FailFloor SoftReset **없음**.

### 운구인 (`CoffinDancePallbearerPose`)

- Animator: `ExtensionBlend` — `SetFloat(Extension)` + `Play`
- **루트 Rigidbody 없음** · **발 Collider 없음** (에디터 제거, 2026-08-15)
- 어깨: **`mixamorig:RightArm` SphereCollider Radius 27** · 반대편 Scale X=-1 (관은 이 정적 Collider + 중력만)
- 모델: `Prefabs/Pallbearer.prefab`

---

## 4. 점수

| 항목 | 값 |
|------|-----|
| 생존 | 초당 **100** |
| 중앙 유지 (`|기울기| ≤ 10°`) | 초당 **50** |
| Phase4 (50~60초) | **획득 ×2.0** (FailFloor 패널티에는 배율 없음) |
| FailFloor 접촉 1회 | **−500** (`failFloorPenaltyScore`, Inspector). 0 미만 clamp. **연습 감점 없음** |

연습 UI 점수는 `-`, Report `FinalScore`는 0.

---

## 5. JUMP — **제거** (2026-08-15)

A 자유 점프 · RB Impulse · `JumpStart`/`JumpStartHold`/`JumpLand` · Land Y 오프셋 · 착지 도치 · 점프 중 `x_bias` Hold · 발 접지 · 관↔발 `IgnoreCollision` · `JumpPromptText` **전부 폐기**.

운구인 루트 Rigidbody·발/Toe Collider는 **에디터에서 삭제됨**(사용자, 2026-08-15). 어깨 SphereCollider는 유지.

C#은 `ExtensionBlend`만 호출한다. `PallbearerPose.controller`의 Jump 상태는 **에디터에서 삭제됨**(사용자, 2026-08-15).

---

## 6. Phase (본게임 60초)

| 구간 | Phase | 조작·노이즈 | 점수 |
|------|-------|-------------|------|
| 0~20초 | 1 | **고정** (`noiseAmp` · 시소 조작 파라미터) | ×1 |
| 20~40초 | 2 | 동일 | ×1 |
| 40~50초 | 3 | 동일 | ×1 |
| 50~60초 | 4 | 동일 | ×2 |

Phase는 **HUD 라벨 + Phase4 점수×2** 만. 단계별 Amp/Speed 난이도는 **제거**(후속 재도입 가능).  
Stumble(각도 한도) **제거**. FailFloor 접촉은 탈락이 아니라 Impulse·패널티.

**60초** → **1초**(`SessionEndDelaySeconds`) 후 Results. 전원 탈락 조기 종료 **없음**.

---

## 7. HP (`CoffinDanceHpLossRules`)

- **1인**: 총점 `< hpLowScoreThreshold`(기본 **3000**) → HP −1
- **2인 이상**: 하위 50%만 −1
- 참가자: 60초 만료 시점 점수로 순위 (`_participatedMask` 유지). 판 중 탈락 없음.

---

## 8. 연습 → 본게임

OIIA와 동일: START READY → 운영자 Enter → `PrepareRound(false)` + `Begin` 재호출.  
메뉴 첫 진입: `PartySession.TakeCoffinDanceNextRoundIsPractice()`.

---

## 9. 주요 타입·파일

| 파일 | 역할 |
|------|------|
| `CoffinDanceMinigameModule` (+ partial) | `IMinigameModule` · 시소·노이즈 |
| `CoffinDanceCoffinBody` | 관 Rigidbody·CoM·FailFloor 감지 · `ApplyUpwardImpulse` |
| `CoffinDancePallbearerPose` | ExtensionBlend · `SoftResetTransform` (운구인 RB 없음) |
| `CoffinDanceFailFloor` | 바닥 마커 (탈락 아님) |
| `CoffinDanceSceneBootstrap` | Begin/Tick |
| `CoffinDanceSlotBindings` | `Pallbearers[6]` · PrepareAllPoses / ApplySideExtension / SoftResetAllPallbearers / `SetCoffinShoulderCollisionsIgnored` |
| `CoffinDanceHpLossRules` | HP 판정 |
| `CoffinDanceResultMinigameFlavor` | Result ID 매칭 |
| Editor `CoffinDancePallbearerAnimatorSetup` | Controller 생성 메뉴 (ExtensionBlend만) |

`BuiltInId` = `"coffin_dance"` · DisplayName 기본 `"관짝춤"`.

---

## 10. Inspector 필드 (Module)

| 필드 | 기본 | 설명 |
|------|------|------|
| `slotBindings[4]` | — | `CoffinDanceSlotBindings` |
| `mainRoundTimerCentralTop` | — | 중앙 타이머 TMP |
| `phaseLabelText` | — | Phase TMP |
| `xSeesawNeutral` | 0.5 | 시작·SoftReset 시소 x |
| `seesawBaseSpeed` | 1.2 | LB/RB 기본 탭 이동 속도 |
| `holdMaxMultiplier` | 3.0 | 홀드 최대 가속 배율 |
| `holdAccelTime` | 0.2 | 홀드 최대 가속 도달 시간(초) |
| `microDriftSpeed` | 0.5 | 미입력·동시 입력 · 기울기 방향 중력형 도치 |
| `pullCoefficient` | 2.0 | 중앙 이탈 비선형 가속 계수 |
| `danceSineHz` | 1.2 | DanceWave Sine 주파수 (씬 **2**) |
| `noiseAmp` | 0.12 | 고정 노이즈 진폭 (씬 **0.03**) |
| `failFloorUpwardImpulse` | 80 | FailFloor 접촉 1회 월드 +Y Impulse (**씬 1**) |
| `failFloorPenaltyScore` | 500 | 본게임 접촉 1회 감점 (연습 0). **수치 보류** |
| `failFloorShoulderIgnoreSeconds` | 0.3 | FailFloor 접촉 후 관↔어깨 IgnoreCollision 시간 |
| `hpLowScoreThreshold` | 3000 | 1P 저점수 컷 |
| ~~`presentationYawDegrees`~~ | — | **제거** · TiltRoot 회전은 프리팹 값 사용 |
| `slotWorldSpacing` | 40 | 슬롯 X 분리 |
| `disableMainCameraOnBegin` | true | Main Camera off |
| `bindSlotCanvasesToSlotCamera` | true | 슬롯 Canvas → SSC |
| `exitScreenFader` | — | FadeOverlay |
| `coffinDanceSceneName` (GameFlow) | `Minigame_CoffinDance` | 로드 씬명 |

**제거됨**: `jumpImpulse` · `jumpAnimBlendSeconds` · `jumpStartAnimSpeed` · `jumpLandAnimSpeed` · `jumpLandYOffset` · `jumpLandYOffsetDuration` · `landingDriftMultiplier` · `landingDriftDuration` · `jumpHeight` · `jumpLockoutSeconds` · `seesawMoveSpeed` · `presentationYawDegrees` · `initialTiltDegrees` · `initialAngularSpeed` · `shoulderReturnSpeed` · `shoulderRaiseSpeed` · `neutralExtension` · `phase2/3/4ShoulderMul` · `noiseAmpPhase1~4` · `maxNoiseSpeedPhase1~4` · `maxNoiseSpeed` · `dancePerlinHz` · **`landingTorqueImpulse` / `ApplyLandingImpulse`** · FailFloor **탈락/`EliminateSlot`/연습 SoftReset**.

`Begin` SoftReset: rest 위치·회전 · `HoldTimer=0` · `x_bias=xSeesawNeutral` (초기 기울기/각속도 **없음**). 플레이 중 SoftReset **없음**.

### SlotBindings

| 필드 | 용도 |
|------|------|
| `TiltRoot` | 연출용 회전 · **프리팹에서 자유 설정**(코드가 덮어쓰지 않음) |
| `Coffin` / `CoffinBody` | 관 Transform · `CoffinDanceCoffinBody` |
| `Pallbearers[6]` | [0..2]=좌 · [3..5]=우 |
| `SlotCamera` | 세로 1/4 |
| HUD | Score · PracticeReady · Eliminated (Begin에서 숨김 · 플레이 중 탈락 UI 없음) |

### CoffinBody

| 필드 | 용도 |
|------|------|
| `centerOfMassLocal` | 기본 **(0, 0, 0)** | 기하 중심. 예전 `(0, 0.15, 0)` 제거 |
| ~~`landingTorqueImpulse`~~ | **제거**. FailFloor +Y Impulse는 Module `failFloorUpwardImpulse` |

---

## 11. 목표 Hierarchy (참고명)

```
Minigame_CoffinDance
├── CoffinDance_Root
│   ├── CoffinDanceMinigameModule
│   └── CoffinDanceSceneBootstrap
├── Slot_1P ~ Slot_4P  (CoffinDanceSlotBindings)
│   ├── TiltRoot (회전은 프리팹 값)
│   │   ├── Pallbearer ×6 (실모델 + Animator + CoffinDancePallbearerPose)
│   │   │     └── mixamorig:RightArm + SphereCollider Radius 27 (반대편은 Scale X=-1)
│   │   └── Coffin (Cube + Rigidbody + CoffinDanceCoffinBody)
│   ├── FailFloor (BoxCollider + CoffinDanceFailFloor)
│   └── SlotCamera (로우앵글)
├── Canvas (Score·Timer·Phase)
└── FadeOverlay (ScreenFader)
```

---

문서 갱신: **2026-08-16** (시소 입력 stick 좌우 → LB/RB)
