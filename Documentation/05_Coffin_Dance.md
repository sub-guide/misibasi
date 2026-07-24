# 05_Coffin_Dance (관짝춤)

> **문서 기준일**: 2026-07-24 — C# 1차 프로토타입 기준.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| C# 게임 로직 | **완료** | `Assets/_Project/Scripts/Minigames/CoffinDance/` |
| Flow·Result 연동 | **완료** | `GameFlowDirector` · `PartySession` 연습 큐 · `ResultFlowController` flavor |
| 씬 파일 | **미착수** | 목표명 `Minigame_CoffinDance` |
| Build Settings | **미등록** | 사용자 에디터 |
| 메뉴 진입 테스트 | **미검증** | 카탈로그 fallback `관짝춤` / `coffin_dance` |

---

## 1. 한 줄 요약

4인 세로 분할 슬롯에서 **관( inverted pendulum )** 균형을 ←/→로 유지하고, 동시 **JUMP!** 이벤트에 A로 반응하는 60초 타임어택.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`1` 토글 1P) |
|------|-------------------------|---------------------------|
| 좌 복원력 | `stick/left` | `A` |
| 우 복원력 | `stick/right` | `D` |
| JUMP | `button2` (Face A) | `H` |
| 연습 READY | Start | `B` |
| 본게임 전환 | 운영자 Enter | — |

---

## 3. 물리

- θ(라디안), ω, 중력 토크 `∝ sinθ` (`gravityTorque` × Phase 가중)
- ←/→ 제어 토크 + 회전 관성(`controlTorque`, `rotationalDamping`, `maxAngularSpeed`)
- `|θ| ≥ 90°` → Stumble Buffer **0.5초** → 실패 시 ELIMINATED(본게임). 연습은 소프트 리셋
- Phase2+ 미세 외력(Perlin), Phase3+ 중력 가중, Phase4 각가속·관성 극대화

---

## 4. 점수

| 항목 | 값 |
|------|-----|
| 생존 | 초당 **100** |
| 중앙 유지 (`|θ| ≤ 10°`) | 초당 **50** |
| JUMP 성공 | **+200** |
| JUMP! JUMP! 성공 | **+450** |
| Phase4 (50~60초) | **전체 ×2.0** |

연습 UI 점수는 `-` 표시, Report `FinalScore`는 0.

---

## 5. JUMP 이벤트

- 4슬롯 **동시** 동일 지시
- Phase1: 6~8초 / Phase2: 4~6초 / Phase3: 2.5~4초 / Phase4: 1.5~2.5초
- **Phase3부터** `JUMP! JUMP!` 가능 (`doubleJumpChanceFromPhase3` 기본 0.4)
- 성공: 점수 + `jumpLockoutSeconds`(기본 0.35) 조작 불능 → 착지 토크 충격
- 미입력: `jumpFailTiltImpulse`로 기울기 충격

---

## 6. Phase (본게임 60초)

| 구간 | Phase |
|------|-------|
| 0~20초 | 1 |
| 20~40초 | 2 |
| 40~50초 | 3 |
| 50~60초 | 4 (×2) |

전원 탈락 또는 60초 → **1초**(`SessionEndDelaySeconds`) 후 종료 시퀀스 → Results.

---

## 7. HP (`CoffinDanceHpLossRules`)

- **1인**: 총점 `< hpLowScoreThreshold`(기본 **3000**) → HP −1
- **2인 이상**: 하위 50%만 −1 (저점수 컷 없음)
- 탈락자: **탈락 시점 점수**로 순위 산정 (`_participatedMask` 유지)

---

## 8. 연습 → 본게임

OIIA와 동일: 씬 내 START READY → 운영자 Enter → `PrepareRound(false)` + `Begin` 재호출.  
메뉴 첫 진입은 `PartySession.TakeCoffinDanceNextRoundIsPractice()`.

---

## 9. 주요 타입·파일

| 파일 | 역할 |
|------|------|
| `CoffinDanceMinigameModule` (+ partial) | `IMinigameModule` |
| `CoffinDanceSceneBootstrap` | Begin/Tick |
| `CoffinDanceSlotBindings` | 슬롯 프리팹 바인딩 |
| `CoffinDanceHpLossRules` | HP 판정 |
| `CoffinDanceResultMinigameFlavor` | Result ID 매칭 |

`BuiltInId` = `"coffin_dance"` · DisplayName 기본 `"관짝춤"`.

---

## 10. Inspector 필드 (Module)

| 필드 | 기본 | 설명 |
|------|------|------|
| `slotBindings[4]` | — | `CoffinDanceSlotBindings` |
| `mainRoundTimerCentralTop` | — | 중앙 타이머 TMP |
| `phaseLabelText` | — | Phase TMP |
| `gravityTorque` | 2.8 | 중력 토크 |
| `controlTorque` | 9.5 | 좌우 복원 |
| `jumpLockoutSeconds` | 0.35 | 점프 중 조작 불능 |
| `doubleJumpChanceFromPhase3` | 0.4 | 연속 JUMP 비율 |
| `hpLowScoreThreshold` | 3000 | 1P 저점수 컷 |
| `presentationYawDegrees` | 22 | 연출용 Y 회전 |
| `exitScreenFader` | — | `FadeOverlay` / `ScreenFader` |
| `coffinDanceSceneName` (GameFlow) | `Minigame_CoffinDance` | 로드 씬명 |

### SlotBindings

| 필드 | 용도 |
|------|------|
| `TiltRoot` | θ(Z) + yaw(Y) 적용 루트 |
| `Coffin` / `Pallbearers[6]` | 시각 참조(선택) |
| `SlotCamera` | 세로 1/4 viewport |
| `BalanceGaugeFill` | 기울기 게이지 Image |
| `JumpPromptText` | JUMP! TMP |
| `ScoreText` / `PracticeReadyText` / `EliminatedText` | HUD |

---

## 11. 목표 Hierarchy (참고명)

```
Minigame_CoffinDance
├── CoffinDance_Root
│   ├── CoffinDanceMinigameModule
│   └── CoffinDanceSceneBootstrap
├── Slot_1P ~ Slot_4P  (CoffinDanceSlotBindings + 3D + Camera)
├── Canvas (게이지·JUMP·Score·Timer·Phase · Overlay)
└── FadeOverlay (ScreenFader)
```

---

문서 갱신: **2026-07-24** (C# 1차)
