# 05_Coffin_Dance (관짝춤)

> **문서 기준일**: 2026-08-12 — 운구인 **물리 점프**(RB Impulse) · Land=접지 후 클립 종료까지 잠금 · Animator Jump **에디터** · 어깨 SphereCollider Radius **27**.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.  
> **과거 Capture/본 Slerp/각도 Stumble/밸런스 게이지/HumanDummy ProtoType** 은 폐기. 최신 진실은 아래 §0·§3.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| C# 게임 로직 | **완료·미검증(물리 점프)** | 시소 · FailFloor · RB Impulse 점프 · Land 클립 후 조작 재개 · 관 Force/Torque 없음 |
| 점프·착지 쏠림 | **C# 완료 · Play 미검증** | `jumpImpulse` · `jumpStart/LandAnimSpeed` · 절차적 Y **제거** |
| Animator Jump 상태 | **에디터 연결됨(컨트롤러)** | 프리팹 RB·발 Collider·제약은 **사용자** |
| Flow·Result 연동 | **유지** | `GameFlowDirector` · `PartySession` · Result flavor |
| 운구인 에셋 | **완료·검증** | `Pallbearer.fbx` + UAL1 · **우어깨만** SphereCollider(**Radius 27**) · 반대편 **Scale X=-1** |
| 씬·슬롯 프리팹 | **완료·검증** | `Pallbearers[6]`만 (`Left/Right Pose` 배열 **없음**) |
| Build Settings | **등록·검증** | 사용자 확인 |
| 메뉴 진입 테스트 | **검증** | MainMenu→관짝춤 |
| 시소·노이즈 개편 | **완료·검증** | 시소 Hold · Sine 씰룩임 · 걷기 클립 · Loop Time (사용자, 2026-08-05) |
| 좌우 조작 개편 | **완료·검증** | 수치 확정 · 중력형 도치 · 어깨 Radius 27 (2026-08-07) |
| 관 물리 경로 | **완료·검증** | 어깨 SphereCollider + 중력만 (2026-08-08 Play) |

### 다음 세션이 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 자세 | Capture 없음 · `ExtensionBlend` 1D Blend + **`JumpStartHold`/`JumpStart`/`JumpLand`** |
| 어깨 높이 | **단일 시소** `x` · `Y_L=x` · `Y_R=1-x` (합=1, 순수 Z 기울기) |
| 입력 Hold | 키를 떼도 `x_bias` **즉시 중립 복귀하지 않음** · **기운 쪽으로 중력형 미세 도치** + 비선형 풀 |
| 점프 잠금 | `BlendIn`→`Airborne`→`Land`(클립 끝) · `x_bias` Hold |
| 점프 물리 | 운구인 루트 RB · `AddForce(VelocityChange)` · 발 Collider↔FailFloor(지면) · **제약은 에디터** |
| 점프 애니 | `JumpStart`/`JumpLand` 연출 · `jumpStartAnimSpeed`/`jumpLandAnimSpeed` · 루트 Y는 물리만 |
| 자율 흔들림 | **Sine만** 즉시 반영 · 고정 `noiseAmp` (Phase별 난이도 후속) · Rate Limiter **없음** |
| FBX 클립명 | Unity에선 `Armature\|Walk_Formal_Loop` / `Armature\|Crouch_Fwd_Loop` (생성 메뉴가 `\|` 접미사도 매칭) |
| Controller 재생성 | 메뉴가 **기존 에셋 재사용**(GUID 유지) · 프리팹 Animator 재연결 불필요 |
| 클립 Loop | `Crouch_Fwd_Loop` · `Walk_Formal_Loop` **Loop Time ON** (사용자 수동, 2026-08-05 검증) |
| 애니 소스 | `UAL1_Standard.fbx` (**RM 파일 쓰지 않음**) |
| Edit Mode 미리보기 | **의도적 비활성** · Play에서만 블렌드 |
| 어깨 Collider | **RightArm만** · `Pallbearer.prefab` SphereCollider **Radius 27** (25→27, 2026-08-07) · 반대편 Scale 반전 |
| Slot 연결 | `Pallbearers[0..2]`=←쪽 · `[3..5]`=→쪽 |
| 실패 | 각도 Stumble 아님 · `CoffinDanceFailFloor` 접촉 (`x_bias` 0/1 한계 → 어깨 기울기로 탈락) |
| 관 운동 | **코드 Force/Torque 없음** · 운구인 어깨 SphereCollider 충돌(+중력)만 |
| 레거시 | `PallbearerProtoType`(HumanDummy) · Capture Rest/Crouch — **참고만** |

---

## 1. 한 줄 요약

4인 세로 분할 슬롯에서 **6명 운구인 어깨 Collider 위 관(Rigidbody)** 균형을 ←/→ 시소로 유지하고, **A로 자유 점프**하는 60초 타임어택.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`Ctrl` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| 좌 | `stick/left` | `A` | `x_bias`↑ → `Y_L`↑ · `Y_R`↓ (홀드 시 가속) |
| 우 | `stick/right` | `D` | `x_bias`↓ → `Y_R`↑ · `Y_L`↓ (홀드 시 가속) |
| 점프 | `button2` (Face A) | `H` | **자유 점프** (프롬프트 없음) |
| 연습 READY | Start | `B` | — |
| 본게임 전환 | 운영자 Enter | — | — |

키를 떼면 `x_bias`는 중립으로 스냅되지 않는다. 미입력(또는 좌·우 동시)이면 **현재 기울기 방향 중력형 미세 도치**가 들어가고, 중앙 이탈 시 **비선형 풀**이 더해진다.  
extension 범위는 **0(앉음) ~ 1(기립)** · `Y_L + Y_R = 1` 항상 유지.

---

## 3. 물리 (순수 충돌 + 시소 제어)

### 관 (`CoffinDanceCoffinBody`)

- `Rigidbody` + Collider
- `Freeze Position`: **X, Z** (Y만 이동)
- `Freeze Rotation`: **X, Y** (Z만 회전)
- `centerOfMass` 로컬 Y를 지지점보다 살짝 높게 (Inspector)
- 기울기(점수용): `transform` 로컬 Z 각(도)
- **실패**: `CoffinDanceFailFloor` Collider에 닿으면 본게임 ELIMINATED / 연습 SoftReset
- **플레이 중 운동**: 운구인 어깨 SphereCollider 충돌 + 중력만. **Force/Torque 코드 경로 없음**(착지 Impulse **제거**, 2026-08-08)

### FailFloor (`CoffinDanceFailFloor`)

- **슬롯 프리팹 자식**으로 둔다 (씬 공용 Floor 금지 — 슬롯 X 분리)
- BoxCollider + 마커 컴포넌트 · 관이 넘어지면 닿는 높이

### 시소 제어 (Module · A안)

| 변수 | 역할 |
|------|------|
| `x_bias` (`SeesawBias`) | 좌우·미세 도치·비선형 풀로 변경 · `Clamp01` · **점프 중 Hold** |
| `HoldTimer` | 슬롯별 단일 홀드 누적(초). 미입력·동시 입력 시 0 |
| `LandingDriftTimer` | 착지 직후 미세 도치 증폭 남은 시간(초). SoftReset/Begin 시 0 |
| `DanceWave` | `Sin(2π · danceSineHz · Time.time)` · [-1, 1] |
| `x` (`SeesawXCurrent`) | `Clamp01(x_bias + DanceWave × noiseAmp)` (즉시) |
| `Y_L` / `Y_R` | `x` / `1 - x` |

#### 조작 파라미터 (Inspector · **Play 검증 확정 2026-08-07** · 착지 도치 **코드 기본 2026-08-08**)

| 필드 | 기본(=씬) | 설명 |
|------|-----------|------|
| `seesawBaseSpeed` | 1.2 | 좌/우 단일 입력 기본 탭 이동 속도 |
| `holdMaxMultiplier` | 3.0 | 홀드 시 최대 가속 배율 |
| `holdAccelTime` | 0.2 | 최대 가속 도달 시간(초) |
| `microDriftSpeed` | 0.5 | 미입력·동시 입력 시 현재 기울기 방향 중력형 도치 속도 |
| `pullCoefficient` | 2.0 | 중앙(0.5) 이탈 비선형 가속 계수 |
| `landingDriftMultiplier` | 2.5 | 착지(Land 클립 종료) 직후 미세 도치 배율 |
| `landingDriftDuration` | 0.3 | 착지 도치 증폭 유지 시간(초) |
| `jumpImpulse` | 4 | 점프 Y VelocityChange |
| `jumpAnimBlendSeconds` | 0.1 (코드) / **씬 0.15** | JumpStart/Land/Extension CrossFade (**벽시계 초** · `animSpeed`와 무관) · 0=즉시 Play · 씬 미직렬화 시 Unity float **0** 주의 |
| `jumpStartAnimSpeed` | 1 | JumpStart 재생 배속 |
| `jumpLandAnimSpeed` | 1 | JumpLand 재생 배속 |
| `jumpLandYOffset` | 0 (코드) / 씬 -0.2 | Land 시작 시 rest+offset **점진 이동** · 종료 시 rest **즉시** 복귀 |
| `jumpLandYOffsetDuration` | 0.15 (코드) / 씬 0.1 | Y 오프셋 점진 이동 시간(초) · 0=즉시 스냅 |

#### `x_bias` 프레임별 연산 (`StepShoulderControl`)

> **점프 잠금(`JumpActive` = `JumpPhase != None`)**: 본 함수를 호출하지 않음 → `x_bias`·`x`·도치·풀·입력 모두 직전 상태 Hold.

1. **착지 도치 타이머**: `LandingDriftTimer > 0`이면 `dt` 차감 · `effectiveDriftSpeed = microDriftSpeed × landingDriftMultiplier` (아니면 `microDriftSpeed`)
2. **입력**
   - **미입력 또는 좌·우 동시**: `HoldTimer = 0` · `driftDir = Sign(x_bias - 0.5)`(정중앙이면 0) · `x_bias += driftDir × effectiveDriftSpeed × dt` (기운 쪽으로 계속 밀림 = 중력형)
   - **좌 또는 우 단일**: `HoldTimer += dt` · `speedMul = Lerp(1, holdMaxMultiplier, HoldTimer / holdAccelTime)` · `x_bias += inputDir × seesawBaseSpeed × speedMul × dt` (`inputDir`: 좌=+1, 우=-1) — **착지 증폭 중이어도 입력은 즉시 가산**(Edge Case A)
3. **비선형 이탈 가속 (공통)**: `offset = x_bias - 0.5` · `pullForce = pullCoefficient × offset² × Sign(offset)` · `x_bias += pullForce × dt`
4. **범위**: `x_bias = Clamp01(x_bias)` · 0.0/1.0 한계에 닿으면 어깨 기울기로 FailFloor 접촉 탈락 가능

#### 리셋 (Edge Case)

`Begin` · `SoftResetSlot` → `ResetSeesawToNeutral`: `HoldTimer = 0` · `LandingDriftTimer = 0` · `x_bias = x_current = xSeesawNeutral`(기본 0.5).

### 운구인 (`CoffinDancePallbearerPose`)

- Animator: `ExtensionBlend` · `JumpStart` · `JumpLand` — `CrossFadeInFixedTime` + `animator.speed`
- JumpLand: 시작과 함께 `jumpLandYOffsetDuration`동안 Y 점진 이동 · 종료 시 rest **즉시** 복귀
- 루트 `Rigidbody` + 발 Collider(에디터) · FailFloor 접지 · **관↔발 IgnoreCollision**
- 점프 높이 = 물리 Impulse · 애니는 CrossFade 연출(+Land Y 오프셋)
- 어깨: **`mixamorig:RightArm` SphereCollider Radius 27** · 반대편 Scale X=-1
- 모델: `Prefabs/Pallbearer.prefab`

---

## 4. 점수

| 항목 | 값 |
|------|-----|
| 생존 | 초당 **100** |
| 중앙 유지 (`|기울기| ≤ 10°`) | 초당 **50** |
| Phase4 (50~60초) | **전체 ×2.0** |

자유 점프에는 별도 성공 점수 없음. 연습 UI 점수는 `-`, Report `FinalScore`는 0.

---

## 5. JUMP (자유 · 물리 + 애니)

| 단계 | 동작 |
|------|------|
| A | `BlendIn`: 지상 · 현재 프레임 → **`JumpStartHold`(Speed0=첫 프레임 고정)** CrossFade · Impulse 없음 |
| BlendIn 끝 | `Play(JumpStart, t=0)` + Impulse 동시 → `Airborne` |
| Airborne | `JumpStart` 종료 + FailFloor 이탈·재접지 후 Land (Start 중 Land 금지) |
| Land | `CrossFade(JumpLand)` · 시작과 함께 Y **점진 이동**(`jumpLandYOffsetDuration`) · 종료 시 Y 즉시 복귀 |
| Land 끝 | 오프셋 해제 · `CrossFade(ExtensionBlend)` · `landingDrift` · 조작 재개 |

- CrossFade(BlendIn) → `JumpStartHold`(동일 Jump_Start 클립 · **State Speed 0**) · 벽시계 `jumpAnimBlendSeconds` · 0이면 스킵
- Animator 상태: `ExtensionBlend` · `JumpStartHold` · `JumpStart` · `JumpLand`
- 절차적 Start 홉 **제거** · `jumpLandYOffset`는 Land **시작/종료**만
- 관 ↔ 발/Toe Collider `Physics.IgnoreCollision` (어깨 Sphere 유지)
- 연습 SoftReset: 운구인 Y = Begin 시 rest (rest 재캐시 안 함)
- RB 제약(Y만 이동 등)은 **에디터** · 코드가 덮어쓰지 않음
- 지면 = 슬롯 FailFloor (관 탈락 판정은 관 Collider만)

---

## 6. Phase (본게임 60초)

| 구간 | Phase | 조작·노이즈 | 점수 |
|------|-------|-------------|------|
| 0~20초 | 1 | **고정** (`noiseAmp` · 시소 조작 파라미터) | ×1 |
| 20~40초 | 2 | 동일 | ×1 |
| 40~50초 | 3 | 동일 | ×1 |
| 50~60초 | 4 | 동일 | ×2 |

Phase는 **HUD 라벨 + Phase4 점수×2** 만. 단계별 Amp/Speed 난이도는 **제거**(후속 재도입 가능).  
Stumble(각도 한도) **제거**. 실패: FailFloor 접촉.

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
| `CoffinDanceMinigameModule` (+ partial) | `IMinigameModule` · 시소·노이즈 |
| `CoffinDanceCoffinBody` | 관 Rigidbody·CoM·FailFloor 감지 (**Force/Torque 없음**) |
| `CoffinDancePallbearerPose` | ExtensionBlend · JumpStart/Land CrossFade · 절차적 Y 홉 |
| `CoffinDanceFailFloor` | 실패 바닥 마커 |
| `CoffinDanceSceneBootstrap` | Begin/Tick |
| `CoffinDanceSlotBindings` | `Pallbearers[6]` · PrepareAllPoses / ApplySideExtension / Jump API |
| `CoffinDanceHpLossRules` | HP 판정 |
| `CoffinDanceResultMinigameFlavor` | Result ID 매칭 |
| Editor `CoffinDancePallbearerAnimatorSetup` | Controller 생성 메뉴 |

`BuiltInId` = `"coffin_dance"` · DisplayName 기본 `"관짝춤"`.

---

## 10. Inspector 필드 (Module)

| 필드 | 기본 | 설명 |
|------|------|------|
| `slotBindings[4]` | — | `CoffinDanceSlotBindings` |
| `mainRoundTimerCentralTop` | — | 중앙 타이머 TMP |
| `phaseLabelText` | — | Phase TMP |
| `xSeesawNeutral` | 0.5 | 시작·SoftReset 시소 x |
| `seesawBaseSpeed` | 1.2 | ←/→ 기본 탭 이동 속도 |
| `holdMaxMultiplier` | 3.0 | 홀드 최대 가속 배율 |
| `holdAccelTime` | 0.2 | 최대 가속 도달 시간(초) |
| `microDriftSpeed` | 0.5 | 미입력·동시 입력 · 기울기 방향 중력형 도치 |
| `pullCoefficient` | 2.0 | 중앙 이탈 비선형 가속 계수 |
| `danceSineHz` | 1.2 | DanceWave Sine 주파수 (씬 **2**) |
| `noiseAmp` | 0.12 | 고정 노이즈 진폭 (씬 **0.03**) |
| `jumpImpulse` | 4 | 점프 Y VelocityChange |
| `jumpAnimBlendSeconds` | 0.1 / 씬 0.15 | CrossFade 시간(벽시계 초) |
| `jumpStartAnimSpeed` | 1 | JumpStart 배속 |
| `jumpLandAnimSpeed` | 1 | JumpLand 배속 |
| `jumpLandYOffset` | 0 / 씬 -0.2 | Land 시작 점진 Y 보정 · 종료 시 즉시 복귀 |
| `jumpLandYOffsetDuration` | 0.15 / 씬 0.1 | Y 점진 이동 시간(초) |
| `landingDriftMultiplier` | 2.5 | 착지 직후 미세 도치 배율 |
| `landingDriftDuration` | 0.3 | 착지 도치 증폭 유지(초) |
| `hpLowScoreThreshold` | 3000 | 1P 저점수 컷 |
| ~~`presentationYawDegrees`~~ | — | **제거** · TiltRoot 회전은 프리팹 값 사용 |
| `slotWorldSpacing` | 40 | 슬롯 X 분리 |
| `disableMainCameraOnBegin` | true | Main Camera off |
| `bindSlotCanvasesToSlotCamera` | true | 슬롯 Canvas → SSC |
| `exitScreenFader` | — | FadeOverlay |
| `coffinDanceSceneName` (GameFlow) | `Minigame_CoffinDance` | 로드 씬명 |

**제거됨**: `jumpHeight` · `jumpLockoutSeconds` · `seesawMoveSpeed` · `presentationYawDegrees` · `initialTiltDegrees` · `initialAngularSpeed` · `shoulderReturnSpeed` · `shoulderRaiseSpeed` · `neutralExtension` · `phase2/3/4ShoulderMul` · `noiseAmpPhase1~4` · `maxNoiseSpeedPhase1~4` · `maxNoiseSpeed` · `dancePerlinHz` · **`landingTorqueImpulse` / `ApplyLandingImpulse`**.

SoftReset: rest 위치·회전 · 속도 0 · `HoldTimer=0` · `LandingDriftTimer=0` · `x_bias=xSeesawNeutral` (초기 기울기/각속도 **없음**).

### SlotBindings

| 필드 | 용도 |
|------|------|
| `TiltRoot` | 연출용 회전 · **프리팹에서 자유 설정**(코드가 덮어쓰지 않음) |
| `Coffin` / `CoffinBody` | 관 Transform · `CoffinDanceCoffinBody` |
| `Pallbearers[6]` | [0..2]=좌 · [3..5]=우 |
| `SlotCamera` | 세로 1/4 |
| HUD | Score · PracticeReady · Eliminated |

### CoffinBody

| 필드 | 용도 |
|------|------|
| `centerOfMassLocal` | 기본 (0, 0.15, 0) |
| ~~`landingTorqueImpulse`~~ | **제거** · 관은 어깨 Collider만 |

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

문서 갱신: **2026-08-07** (좌우 조작 Play 검증·수치 확정 · 어깨 SphereCollider Radius 25→27)
