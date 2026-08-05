# 05_Coffin_Dance (관짝춤)

> **문서 기준일**: 2026-08-05 — 시소(단일 x) · 고정 자율 노이즈 · **중립 복귀 제거** · Phase 조작 난이도 **없음**(후속).  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.  
> **과거 Capture/본 Slerp/각도 Stumble/밸런스 게이지/HumanDummy ProtoType** 은 폐기. 최신 진실은 아래 §0·§3.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| C# 게임 로직 | **완료** | 시소 x · 고정 NoiseAmp/MaxSpeed · FailFloor · 자유 점프 |
| Flow·Result 연동 | **유지** | `GameFlowDirector` · `PartySession` · Result flavor |
| 운구인 에셋 | **완료·검증** | `Pallbearer.fbx` + UAL1 · **우어깨만** SphereCollider · 반대편 **Scale X=-1** |
| 씬·슬롯 프리팹 | **완료·검증** | `Pallbearers[6]`만 (`Left/Right Pose` 배열 **없음**) |
| Build Settings | **등록·검증** | 사용자 확인 |
| 메뉴 진입 테스트 | **검증** | MainMenu→관짝춤 |
| 시소·노이즈 개편 | **코드 반영** | Play 체감 검증은 후속 |

### 다음 세션이 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 자세 | Capture 없음 · `PallbearerPose.controller` 1D Blend (`Extension` 0=Crouch · 1=Idle) |
| 어깨 높이 | **단일 시소** `x` · `Y_L=x` · `Y_R=1-x` (합=1, 순수 Z 기울기) |
| 입력 Hold | 키를 떼도 `x_bias` **유지** · `shoulderReturnSpeed` **없음** |
| 자율 흔들림 | **Sine만** → Rate Limiter · 고정 Amp/Speed (Phase별 난이도 후속) |
| FBX 클립명 | Unity에선 `Armature\|Idle_Loop` / `Armature\|Crouch_Idle_Loop` |
| 애니 소스 | `UAL1_Standard.fbx` (**RM 파일 쓰지 않음**) |
| Edit Mode 미리보기 | **의도적 비활성** · Play에서만 블렌드 |
| 어깨 Collider | 검증 기준 **RightArm만** · 관 반대편은 Scale 반전 |
| Slot 연결 | `Pallbearers[0..2]`=←쪽 · `[3..5]`=→쪽 |
| 실패 | 각도 Stumble 아님 · `CoffinDanceFailFloor` 접촉 |
| 레거시 | `PallbearerProtoType`(HumanDummy) · Capture Rest/Crouch — **참고만** |

---

## 1. 한 줄 요약

4인 세로 분할 슬롯에서 **6명 운구인 어깨 Collider 위 관(Rigidbody)** 균형을 ←/→ 시소로 유지하고, **A로 자유 점프**하는 60초 타임어택.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`1` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| 좌 | `stick/left` | `A` | `x_bias`↑ → `Y_L`↑ · `Y_R`↓ |
| 우 | `stick/right` | `D` | `x_bias`↓ → `Y_R`↑ · `Y_L`↓ |
| 점프 | `button2` (Face A) | `H` | **자유 점프** (프롬프트 없음) |
| 연습 READY | Start | `B` | — |
| 본게임 전환 | 운영자 Enter | — | — |

키를 떼면 `x_bias`는 **마지막 값 Hold**. 수평(≈0.5)으로 자동 복귀하지 않는다.  
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

### FailFloor (`CoffinDanceFailFloor`)

- **슬롯 프리팹 자식**으로 둔다 (씬 공용 Floor 금지 — 슬롯 X 분리)
- BoxCollider + 마커 컴포넌트 · 관이 넘어지면 닿는 높이

### 시소 제어 (Module · A안)

| 변수 | 역할 |
|------|------|
| `x_bias` (`SeesawBias`) | ←/→로만 변경 · 키 떼면 Hold |
| `DanceWave` | `Sin(2π · danceSineHz · Time.time)` · [-1, 1] |
| `x_target` | `Clamp01(x_bias + DanceWave × noiseAmp)` |
| `x_current` (`SeesawXCurrent`) | `MoveTowards(x_target, maxNoiseSpeed×dt)` |
| `Y_L` / `Y_R` | `x_current` / `1 - x_current` |

시작·SoftReset: `x_bias = x_current = xSeesawNeutral`(기본 0.5).

### 운구인 (`CoffinDancePallbearerPose`)

- Animator **1D Blend**: `Extension` 0=`Crouch_Idle_Loop` · 1=`Idle_Loop`
- Module `SetExtension` → `Animator.SetFloat("Extension")`
- 어깨 지지: **`mixamorig:RightArm` SphereCollider** · 반대편 **Scale X=-1**
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

| 구간 | Phase | 조작·노이즈 | 점수 |
|------|-------|-------------|------|
| 0~20초 | 1 | **고정** (`noiseAmp` / `maxNoiseSpeed`) | ×1 |
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
| `CoffinDanceCoffinBody` | 관 Rigidbody·CoM·착지 Impulse |
| `CoffinDancePallbearerPose` | Animator Extension 블렌드 · 점프 홉 |
| `CoffinDanceFailFloor` | 실패 바닥 마커 |
| `CoffinDanceSceneBootstrap` | Begin/Tick |
| `CoffinDanceSlotBindings` | `Pallbearers[6]` · PrepareAllPoses / ApplySideExtension |
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
| `initialTiltDegrees` | 6 | SoftReset 초기 기울기 |
| `initialAngularSpeed` | 25 | SoftReset 초기 각속도 |
| `xSeesawNeutral` | 0.5 | 시작·SoftReset 시소 x |
| `danceSineHz` | 1.2 | DanceWave Sine 주파수 |
| `noiseAmp` | 0.12 | 고정 노이즈 진폭 |
| `maxNoiseSpeed` | 0.8 | 고정 초당 x 변화 상한 |
| `jumpLockoutSeconds` | 0.35 | 점프 중 조작 불능 |
| `hpLowScoreThreshold` | 3000 | 1P 저점수 컷 |
| `presentationYawDegrees` | 22 | TiltRoot Y 회전 |
| `slotWorldSpacing` | 40 | 슬롯 X 분리 |
| `disableMainCameraOnBegin` | true | Main Camera off |
| `bindSlotCanvasesToSlotCamera` | true | 슬롯 Canvas → SSC |
| `exitScreenFader` | — | FadeOverlay |
| `coffinDanceSceneName` (GameFlow) | `Minigame_CoffinDance` | 로드 씬명 |

**제거됨**: `shoulderReturnSpeed` · `shoulderRaiseSpeed` · `neutralExtension` · `phase2/3/4ShoulderMul` · `noiseAmpPhase1~4` · `maxNoiseSpeedPhase1~4` · `dancePerlinHz`.

### SlotBindings

| 필드 | 용도 |
|------|------|
| `TiltRoot` | yaw(Y)만 |
| `Coffin` / `CoffinBody` | 관 Transform · `CoffinDanceCoffinBody` |
| `Pallbearers[6]` | [0..2]=좌 · [3..5]=우 |
| `SlotCamera` | 세로 1/4 |
| HUD | Score · PracticeReady · Eliminated |

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

문서 갱신: **2026-08-05** (시소 단일 x · Sine 고정 씰룩임 · Phase 조작 난이도 제거 · 중립 복귀 제거)
