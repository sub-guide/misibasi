# 05_Rhythm_Button_Challenge

> **문서 기준일**: 2026-06-21 — `Assets/`·씬 YAML·`GameFlowDirector.cs` 직접 분석 (추측 없음).  
> Unity Editor에서 실제 플레이로 확인이 필요한 항목은 **「확인 필요」** 로 표시한다.

---

## 0. 새 채팅용 — 현재 상태 스냅샷

> 이전 작업에서 **코드 구현은 끝났고**, Unity Editor는 **Module·Bootstrap 부착(2026-06-21)까지 완료**, UI·오디오는 **미착수** 상태다.  
> 새 채팅·새 세션은 **섹션 0 → 섹션 16「다음 작업 순서」** 부터 읽으면 된다.

### 진행도 한눈에

| 영역 | 상태 | 비고 |
|------|------|------|
| C# 게임 로직 | **완료** | `Assets/_Project/Scripts/Minigames/RhythmButtonChallenge/` 19파일 |
| Flow·Result 연동 | **완료** | `GameFlowDirector`, `ResultFlowController` 수정됨 |
| 씬 파일 존재 | **부분** | `Assets/Scenes/Minigame_RhythmButtonChallenge.unity` 있음 |
| 씬 스크립트 부착 | **부분 완료** | Module·Bootstrap MonoBehaviour 부착 + `module` 참조 (2026-06-21) |
| UI (보드·점수) | **골격+프리팹 완료** | `Board_8Cells`·`Panel_RBC_Score_4Way` + `RBC_BoardCell`·`RBC_ScorePanel` prefab (2026-06-21) |
| Canvas | **완료** | Canvas Scaler 1920×1080, Inspector Scale 1 (Canvas 구동) |
| 오디오 에셋 | **임시 연결** | `MusicSource` + Module 클립 24칸 연결 (2026-07-18). RBC 전용 0_0~2_7 미제작 |
| 판정 Sprite | **연결 완료** | `Assets/RhythmButtonChallenge/Sprites/` Perfect/Fast/Slow/Miss/Wrong → Module (2026-07-10) |
| 버튼 Sprite | **연결 완료** | SNES Pixel Pack Unpressed → Module `spriteA`~`spriteRight` (2026-07-10) |
| Build Settings | **미등록** | RBC 씬 빌드에 포함 안 됨 |
| 메뉴 진입 테스트 | **막힘** | MainMenu `debugRouteAllToOiia = true` |

### 실제 씬 Hierarchy (2026-06-21 YAML 기준)

```
Minigame_RhythmButtonChallenge
├── Main Camera          (+ AudioListener)
├── EventSystem
├── RBC_Root             (Position 0,0,0)
│   ├── RhythmButtonChallengeMinigameModule   (+ MonoBehaviour ✓)
│   └── RhythmButtonChallengeSceneBootstrap   (+ MonoBehaviour ✓, module 연결됨)
└── Canvas               (Canvas Scaler 1920×1080, Scale 1 — Canvas 구동)
    ├── PlayArea → Board_8Cells → Cell_0~7 (RBC_BoardCell prefab)
    └── Panel_RBC_Score_4Way → ScorePanel_1P~4P (RBC_ScorePanel prefab)
(+ 씬 루트 `MusicSource` AudioSource — Module 연결, 2026-07-18)
```

### Build Settings (현재 등록된 씬)

1. `MainMenu`
2. `Minigame_O.I.I.A.`
3. `Results`  
→ **`Minigame_RhythmButtonChallenge` 없음**

### 메인 메뉴 (`MainMenu.unity` Inspector 기준)

| 항목 | 씬에 저장된 값 | 영향 |
|------|----------------|------|
| `catalog` | **빈 배열 `[]`** | 런타임 fallback 사용 → RBC 카탈로그 항목 포함 |
| `debugRouteAllToOiia` | **`true`** | RBC를 골라도 **OIIA 씬**으로 감 |
| `rhythmButtonChallengeSceneName` | **미저장** | 코드 기본값 `Minigame_RhythmButtonChallenge` 사용 |
| `oiiaSceneName` | `Minigame_O.I.I.A.` | — |

### 코드 ↔ 씬 연결 요약

```
[MainMenu] GameFlowDirector
    id == "rhythm_button_challenge" && debugRouteAllToOiia == false
        → LoadScene("Minigame_RhythmButtonChallenge")
            → RhythmButtonChallengeSceneBootstrap.Start()
                → RhythmButtonChallengeMinigameModule.Begin/Tick
                    → (현재) 스크립트 없어서 동작 불가
```

### 멈춘 지점 (추정)

1. Cursor가 **C# 전부** + 씬 **빈 GameObject 이름** + **Canvas 생성**까지 진행
2. 사용자 Unity 작업 **미진행**:
   - `Add Component`로 Module / Bootstrap 부착
   - Canvas Scale (0,0,0) 수정
   - 8칸 보드·4점수 패널·FadeOverlay·MusicSource 배치
   - AudioClip·Sprite Inspector 연결
   - Build Settings 등록

### 다음 작업 순서 (에디터 — 사용자 직접)

아래 순서대로 하면 **플레이 가능한 최소 상태**까지 갈 수 있다.

1. **MainMenu** → `GameFlowDirector` → `Debug Route All To Oiia` **체크 해제**
2. **RBC 씬** → `RhythmButtonChallengeMinigameModule` 오브젝트에 스크립트 **Add Component**
3. **RBC 씬** → `RhythmButtonChallengeSceneBootstrap` 오브젝트에 스크립트 **Add Component** → Module 참조 드래그
4. **Canvas** → `RectTransform` Scale **(1,1,1)** 로 수정
5. **UI 구축** — 섹션 11「목표 Hierarchy」·섹션 12 Inspector 표 참고
6. **MusicSource** + AudioClip 24개 연결 (없으면 임시 OIIA 패턴 SFX로 테스트 가능 — **확인 필요**)
7. **`Assets/Sprites/Gamepad/`** 스프라이트를 Module Inspector `spriteA`~`spriteRight`에 매핑
8. **판정 Sprite 5종** 제작·연결 (없으면 임시 단색 Image로 대체 가능)
9. **FadeOverlay** — OIIA 씬 `FadeOverlay` 구조 복사 권장
10. **File → Build Settings** → RBC 씬 Add Open Scenes
11. Play Mode: MainMenu → RBC 카탈로그 선택 → READY → Enter

---

## 1. 한 줄 요약

화면에 순서대로 나타나는 **8박자 버튼 패턴**을 보고, 음악 박자에 맞춰 **A/B/X/Y/L/R/방향키**를 눌러 점수를 겨루는 리듬 게임이다.

---

## 2. 게임 목적

### 플레이어가 해야 할 일

- 게임이 **패턴을 보여주는 구간(Stage Reveal)** 에서 어떤 버튼을 어떤 순서로 눌러야 하는지 외운다.
- 이어지는 **입력 구간(Stage Input)** 에서 박자에 맞춰 같은 버튼을 누른다.
- **Phase 1** (보통 속도) 5스테이지 → **SPEED UP!** → **Phase 2** (2배속) 5스테이지를 완료한다.
- 스테이지가 올라갈수록 사용할 수 있는 버튼 종류가 늘어난다.

### 승리(유리한 결과)

- 1:1 탈락 경쟁이 아니라 **최종 점수 경쟁**이다.
- Result 화면에서 **높은 점수 = 높은 등수**.
- HP 감소 조건에 해당하지 않으면 **연승 +1**.

### 실패(불리한 결과)

- 판 안에서 Miss/Wrong 입력 → 점수 감소 (탈락은 아님).
- 본게임 종료 후 HP 감소 조건:
  1. 최종 점수 **500,000점 미만** (499,999 이하)
  2. 참가자 2명 이상일 때 **하위 50%**
- 500,000점 **정확히** 달성 시 저점수 규칙은 해당 없음 (`<` 비교).
- HP 0 → GAME OVER.

### 연습 모드

- 코드상 `MinigameContext.IsPractice`를 읽을 수 있으나,
- **`GameFlowDirector`는 RBC 진입 시 항상 `practice = false`** 로 설정한다.
- 따라서 **현재 메뉴에서 연습 모드로 진입하는 경로는 없다.**

---

## 3. 플레이 흐름

### 전체 파티 흐름

```
메인 메뉴 (READY)
    ↓
[Rhythm Button Challenge 선택 + 운영자 Enter]
    ↓  (debugRouteAllToOiia가 꺼져 있어야 함)
Minigame_RhythmButtonChallenge 씬
    ↓
Phase 1: Intro → Stage1~5 (Reveal+Input 반복)
    ↓
SPEED UP! (2초, BGM 피치 2배)
    ↓
Phase 2: Intro → Stage1~5 (2배속)
    ↓
Result 씬 → 메인 메뉴
```

> **주의 (2026-06-21 확인)**: `MainMenu.unity`에 `debugRouteAllToOiia: 1`(true)로 **저장되어 있음**. RBC 테스트 전 반드시 Inspector에서 해제할 것.

### 씬 내부 상태 머신

```
Begin()
    ↓
PhaseIntro (8박, phaseIntroClips 0~7)
    ↓
StageReveal (스테이지 1, revealClips, 패턴 한 박자씩 공개)
    ↓
StageInput (inputClips, 플레이어 입력·판정)
    ↓
StageReveal (스테이지 2) → StageInput → ... → Stage 5 Input
    ↓
SpeedUp (2초, "SPEED UP!" 표시, pitch=2)
    ↓
PhaseIntro (Phase 2)
    ↓
Stage 1~5 반복 (2배속)
    ↓
CompleteSession() → Result 씬
```

### 텍스트 플로우 차트

```
[메뉴] READY
    ↓
[P1 Intro] 8박
    ↓
[P1 Stage1 Reveal] → [P1 Stage1 Input] → ... → [P1 Stage5 Input]
    ↓
[SPEED UP!] 2초
    ↓
[P2 Intro] → [P2 Stage1~5]
    ↓
[Result] → [메뉴]
```

---

## 4. 화면 구성

코드가 기대하는 UI 레이아웃. **현재 씬에는 아래 오브젝트가 없음** (섹션 11·16 참고).

### 목표 화면 (개념도)

```
┌──────────────────────────────────────────────────────────────┐
│                    (SPEED UP! 오버레이 — 페이즈 사이만)          │
├──────────────────────────────────────────────────────────────┤
│  [1] [2] [3] [4] [5] [6] [7] [8]   ← 8칸 공용 보드            │
│   각 칸: 버튼 아이콘 + 현재 박 하이라이트                        │
│   각 칸 아래/옆: 1P~4P 판정 아이콘 (Perfect/Miss 등)            │
├──────────────────────────────────────────────────────────────┤
│  P1 점수    P2 점수    P3 점수    P4 점수   ← 하단 4패널       │
└──────────────────────────────────────────────────────────────┘
```

### 보드 칸 하나 (`BoardCell`)

```
┌─ Cell ─────────────┐
│  ActiveHighlight  │  ← 현재 박자 강조
│  ButtonIcon       │  ← A/B/X/... 스프라이트
│  Judgment1P       │
│  Judgment2P       │  ← 슬롯별 판정 결과
│  Judgment3P       │
│  Judgment4P       │
└───────────────────┘
```

---

## 5. UI 구성 요소

| UI 오브젝트 | 역할 | 사용자 |
|-------------|------|--------|
| `ButtonIcon` (8칸) | 해당 박에 눌러야 할 버튼 그림 | 모두 |
| `ActiveHighlight` (8칸) | Reveal/Input 중 **현재 박** 강조 | 모두 |
| `Judgment1P`~`4P` (8칸×4) | 슬롯별 판정 아이콘 (Perfect/Fast/Slow/Miss/Wrong) | 플레이어·관중 |
| `ScoreText` (4패널) | 누적 점수 (`N0` 포맷) | 모두 |
| `PlayerLabel` (4패널) | 플레이어 이름/번호 | **확인 필요** (코드에서 갱신 안 함) |
| `speedUpText` | "SPEED UP!" 오버레이 | 모두 |
| `FadeOverlay` | 종료 페이드 | 코드에서 Find — **씬에 없음** |

### 구간별 보드 동작

| 구간 | ButtonIcon | ActiveHighlight |
|------|------------|-----------------|
| PhaseIntro | 비표시 | 현재 박만 |
| StageReveal | 0~현재박까지 순차 공개 | 현재 박 |
| StageInput | 8칸 전부 표시 | 현재 박 |
| SpeedUp | 이전 상태 유지 | 갱신 안 됨 |

---

## 6. 플레이어 입력

### 부스 USB 패드 → RBC 버튼 매핑

| RBC 버튼 | 플레이어 이름 | Unity 경로 |
|----------|--------------|------------|
| A | A (Button 2) | `button2` |
| B | B (Button 3) | `button3` |
| X | X (Trigger) | `Joystick.trigger` |
| Y | Y (Button 4) | `button4` |
| Lb | L (Button 5) | `button5` |
| Rb | R (Button 6) | `button6` |
| Up | Stick Up | `stick/up` |
| Down | Stick Down | `stick/down` |
| Left | Stick Left | `stick/left` |
| Right | Stick Right | `stick/right` |

슬롯 `i` = `Joystick.all[i]`.

### 입력이 처리되는 경우

- `_flowState == StageInput` 이고
- `_inputBeatWindow.Active == true` 이고
- 해당 슬롯이 참가 중 (`_aliveMask[i]`)
- 패드가 연결됨

### 입력 동작

| 상황 | 결과 |
|------|------|
| 첫 입력 + 정답 버튼 + 타이밍 구간 내 | Perfect / Fast / Slow |
| 첫 입력 + 오답 버튼 | Wrong (-10,000) |
| 첫 입력 + 타이밍 구간 밖 | Miss (-10,000) |
| 이미 판정된 박에 추가 입력 | Wrong 아이콘 + **-2,000** (ExtraInputPenalty) |
| 박 종료까지 입력 없음 | 자동 Miss |

### 입력이 무시되는 경우

- PhaseIntro, StageReveal, SpeedUp 구간
- 참가하지 않은 슬롯
- 패드 미연결
- 게임 종료 처리 중

### 운영자/디버그 입력

| 키 | 동작 |
|----|------|
| ESC | 즉시 세션 종료 (`CompleteSession`) |

RBC에는 OIIA처럼 운영자 Enter 연습 전환 **없음**.

---

## 7. 게임 규칙

### Phase·Stage 구조

| 개념 | 값 | 설명 |
|------|-----|------|
| Phase | 1 → 2 | Phase 2는 BGM `pitch = 2` (2배속) |
| StagesPerPhase | 5 | 페이즈당 5스테이지 |
| BeatsPerSegment | 8 | 모든 구간 8박자 |
| SpeedUp 표시 | 2초 (기본) | Phase 1 완료 후 |

### 세 구간 종류 (`RbcSegmentKind`)

| 구간 | 오디오 클립 배열 | 플레이어 입력 |
|------|-----------------|--------------|
| PhaseIntro | `phaseIntroClips[0~7]` | 없음 |
| StageReveal | `revealClips[0~7]` | 없음 (패턴만 공개) |
| StageInput | `inputClips[0~7]` | 있음 (판정) |

### 스테이지별 사용 버튼 풀

| Stage | 사용 가능 버튼 |
|-------|---------------|
| 1 | A, B |
| 2 | A, B, X, Y |
| 3 | A, B, X, Y, Lb, Rb |
| 4, 5 | A, B, X, Y, Lb, Rb, Up, Down, Left, Right |

### 패턴 생성 규칙

- 세션 시작 시 `_sessionSeed = Random.Range(...)` — **매 판 랜덤**
- 스테이지별 시드: `_sessionSeed * 397 ^ phaseNumber * 17 ^ stageIndex`
- 8박 각각: 해당 스테이지 풀에서 랜덤 선택
- **연속 3번 같은 버튼 금지** (`WouldViolateConsecutiveRule`) — 최대 32회 재시도

### 타이밍 판정 (StageInput, 박 시작 기준 `deltaMs`)

| 판정 | 조건 (ms) | 점수 |
|------|-----------|------|
| Perfect | \|delta\| ≤ 50 | +10,000 |
| Fast (이른) | -120 ≤ delta < -50 | +5,000 |
| Slow (늦은) | 50 < delta ≤ 120 | +5,000 |
| Miss | 그 외 (너무 이르거나 늦음) | -10,000 |
| Wrong | 잘못된 버튼 | -10,000 |

- `deltaMs = (현재시간 - 박 시작시간) × 1000`
- `Time.unscaledTimeAsDouble` 사용 — **Time.timeScale 영향 없음**

### 8박 올클리어 보너스

- 한 스테이지 Input 구간에서 8박 모두 Perfect/Fast/Slow이면 **+30,000**
- Miss나 Wrong이 하나라도 있으면 보너스 없음

### 실패 조건 (라운드)

- 별도 "게임 오버" 없음. 모든 스테이지 끝까지 진행 후 점수로 평가.

---

## 8. 점수 계산

### 점수표

| 항목 | 점수 |
|------|------|
| Perfect | +10,000 |
| Fast | +5,000 |
| Slow | +5,000 |
| Miss | -10,000 |
| Wrong | -10,000 |
| 추가 입력 (이미 판정된 박) | -2,000 (회당) |
| 8박 올클리어 보너스 | +30,000 |
| 최소 점수 | 0 (음수 불가) |

### 이론적 최대 점수 (참고)

한 스테이지 Input (8박 전부 Perfect + 보너스):

- 8 × 10,000 + 30,000 = **110,000**

한 Phase (5스테이지):

- 5 × 110,000 = **550,000**

전체 (Phase 1 + Phase 2):

- **1,100,000** (전 박 Perfect 가정)

### 계산 예시

**예시 A — Stage 1, 8박 중 6 Perfect + 2 Miss, 보너스 없음**

- 6 × 10,000 - 2 × 10,000 = **40,000**

**예시 B — Stage 3, 전 박 Slow (늦었지만 Good 판정)**

- 8 × 5,000 + 30,000 = **70,000**

**예시 C — HP 경계**

- 최종 499,999 → HP 감소 (저점수)
- 최종 500,000 → 저점수 규칙 해당 없음 (`< 500000`만 해당)
- 4인 중 4위 → 하위 50%로 HP 감소 (OR 조건)

### 오디오·박자 길이

- `_beatDurationSec = clip.length / musicSource.pitch`
- Phase 2에서는 피치 2 → **박자 길이 절반** (2배속)

---

## 9. 내부 시스템 구조

### 오디오·타임라인 시스템

- 각 구간 시작 시 `_segmentStartTime` 기록.
- 매 프레임 경과 시간으로 현재 박 인덱스 계산 (`Floor(elapsed / beatDuration)`).
- 박이 바뀔 때마다 해당 `AudioClip` 재생 (`musicSource.Stop()` 후 Play).
- 8박이 끝나면 `OnSegmentFinished()`로 다음 상태 전환.

### 패턴 시스템

- `_currentPattern[8]`: 현재 스테이지 정답 시퀀스.
- StageReveal: `UpdateBoardForBeat`로 0~현재박까지 아이콘 공개.
- StageInput: 8칸 전부 표시, 박마다 입력 윈도우 오픈.

### 판정 시스템

- 박마다 `BeatWindow` (시작·끝 시각, Active 플래그).
- 다음 박 시작 시 이전 박 `FinalizeInputBeat` — 미입력 Miss 처리.
- 슬롯별 `BeatJudged[]`, `BeatJudgments[]` 배열.

### 점수 시스템

- 슬롯별 `ScoreSum`, 이벤트마다 가산·감산, 0 미만 불가.

### UI 시스템

- `FlushAllUi`: 점수 패널 + 보드 하이라이트.
- `SetJudgmentImage`: 슬롯·박 인덱스로 Judgment 이미지 설정.

### 종료 시스템

- Phase 2 Stage 5 Input 완료 → `CompleteSession()`.
- ESC → 언제든 `CompleteSession()`.

---

## 10. 실제 코드 구조

### 진입점

| 클래스 | 파일 |
|--------|------|
| `RhythmButtonChallengeMinigameModule` | `Assets/_Project/Scripts/Minigames/RhythmButtonChallenge/RhythmButtonChallengeMinigameModule*.cs` |
| `RhythmButtonChallengeSceneBootstrap` | `.../RhythmButtonChallengeSceneBootstrap.cs` |

### partial 파일

| 파일 | 담당 |
|------|------|
| `.cs` | 선언, BuiltInId |
| `.Constants.cs` | 점수, 판정 창, Phase pitch |
| `.Types.cs` | enum, BoardCellBindings, SlotRuntime |
| `.State.cs` | 런타임 필드, 상태 머신 |
| `.Config.cs` | displayName |
| `.Begin.cs` | Begin, Phase/Stage 시작 |
| `.Tick.cs` | 메인 루프, SpeedUp, ESC |
| `.Gameplay.cs` | 입력·판정·보너스 |
| `.Pattern.cs` | 패턴 생성, 버튼 풀 |
| `.AudioFlow.cs` | 박자·세그먼트·SpeedUp |
| `.Input.cs` | 패드 읽기 |
| `.Ui.cs` | 보드·점수·스프라이트 |
| `.SlotHelpers.cs` | ForEachSlot, 점수 유틸 |
| `.ExitSequence.cs` | 종료·Report |
| `RhythmButtonChallengeBoardCellBindings.cs` | 보드 칸 UI 자동 연결 |
| `RhythmButtonChallengeScorePanelBindings.cs` | 점수 패널 자동 연결 |
| `RhythmButtonChallengeHpLossRules.cs` | HP 규칙 |
| `RhythmButtonChallengeResultMinigameFlavor.cs` | Result 훅 (ID만) |

### BuiltInId

- `"rhythm_button_challenge"`

### 공유 의존

- `IMinigameModule`, `PartySession`, `SlotGamepad`, `BoothUsbGamepadLayout`
- `MinigameExitSequence`, `GameFlowDirector` (씬 로드)
- `ResultFlowController` + `RhythmButtonChallengeResultMinigameFlavor`

---

## 11. 씬 구조

**씬 파일**: `Assets/Scenes/Minigame_RhythmButtonChallenge.unity`  
**Build Settings**: **미등록** ✗ (2026-05-31: `EditorBuildSettings.asset`에 MainMenu / OIIA / Results만 있음)

### 현재 Hierarchy (씬 YAML 직접 확인, 2026-05-31)

```
Minigame_RhythmButtonChallenge
├── Main Camera          (Camera + AudioListener)
├── EventSystem          (StandaloneInputModule + EventSystem)
├── RBC_Root             (Transform, 월드 좌표 이상치 있음 — 정리 권장)
│   ├── RhythmButtonChallengeMinigameModule   ← component: Transform만 (MonoBehaviour ✗)
│   └── RhythmButtonChallengeSceneBootstrap   ← component: Transform만 (MonoBehaviour ✗)
└── Canvas               (Canvas + CanvasScaler + GraphicRaycaster)
                         m_Children: []  ← UI 자식 없음
                         m_LocalScale: (0, 0, 0)  ← 화면에 안 보임
```

### 목표 Hierarchy (아직 안 만든 것)

```
Minigame_RhythmButtonChallenge
├── RBC_Root
│   ├── RhythmButtonChallengeMinigameModule   (+ MonoBehaviour)
│   └── RhythmButtonChallengeSceneBootstrap   (+ MonoBehaviour, module 참조)
├── Canvas  (Scale 1,1,1)
│   ├── PlayArea                    Anchor 상단 7/8
│   │   ├── Board_8Cells            ← 이름 고정 (GameObject.Find)
│   │   │   └── Cell_0 … Cell_7     (+ RhythmButtonChallengeBoardCellBindings)
│   │   └── SpeedUpText             TMP, 초기 비활성
│   └── Panel_RBC_Score_4Way        ← 이름 고정
│       └── ScorePanel_1P … 4P      (+ RhythmButtonChallengeScorePanelBindings)
├── MusicSource                     AudioSource
└── FadeOverlay                     ScreenFader
```

### 코드가 기대하지만 씬에 없는 것

| 이름 | 용도 |
|------|------|
| `Board_8Cells` | 8칸 보드 컨테이너 + `RhythmButtonChallengeBoardCellBindings` ×8 |
| `Panel_RBC_Score_4Way` | 4슬롯 점수 + `RhythmButtonChallengeScorePanelBindings` ×4 |
| `FadeOverlay` | 종료 페이드 |
| `speedUpText` | SPEED UP 오버레이 TMP |
| `musicSource` (AudioSource) | 박자 클립 재생 |
| `RhythmButtonChallengeMinigameModule` (MonoBehaviour) | 실제 컴포넌트 부착 |
| `RhythmButtonChallengeSceneBootstrap` (MonoBehaviour) | 실제 컴포넌트 부착 |
| 버튼·판정 Sprite | Inspector `spriteA` 등 |
| `phaseIntroClips` / `revealClips` / `inputClips` | 각 8개 AudioClip |

### 에셋 현황 (2026-05-31)

| 종류 | 경로 | RBC 연동 |
|------|------|----------|
| 버튼 Sprite | `Assets/Sprites/Gamepad/` (A/B/X/Y/L/R/방향 등) | **있음, Inspector 미연결** |
| 판정 Sprite | `Assets/RhythmButtonChallenge/Sprites/` | **연결 완료** Perfect/Fast/Slow/Miss/Wrong (2026-07-10) |
| 버튼 Sprite | SNES Pixel Pack Unpressed | **Module 연결 완료** (2026-07-10) |
| RBC AudioClip | — | **없음** (0_0~2_7, 24개) — 임시 OIIA `Patturn` 가능 |
| RBC 프리팹 | `Assets/RhythmButtonChallenge/Prefabs/` | **`RBC_BoardCell`·`RBC_ScorePanel`** (2026-06-21) |
| RBC 전용 폴더 | — | `Assets/RhythmButtonChallenge/` **미생성** (권장) |
| OIIA 참고 | `Assets/O.I.I.A/Sounds/` | 임시 테스트용으로만 참고 가능 |

---

## 12. Inspector 연결

`RhythmButtonChallengeMinigameModule`이 기대하는 SerializeField (코드 기준).

### 표시·설정

| 필드 | 기본값 | 연결 안 되면 |
|------|--------|-------------|
| `displayName` | "Rhythm Button Challenge" | 이름 표시만 |

### 오디오

| 필드 | 설명 | 연결 안 되면 |
|------|------|-------------|
| `musicSource` | 박자 클립 재생 | 오디오·박자 진행 불가 (`beatDuration` 0.5초 폴백) |
| `phaseIntroClips[8]` | Phase 시작 0_0~0_7 | 해당 구간 무음 |
| `revealClips[8]` | 패턴 공개 1_0~1_7 | 무음 |
| `inputClips[8]` | 입력 2_0~2_7 | 무음 |
| `sessionEndClip` | 종료 효과음 | 무음 |

### UI — 보드

| 필드 | 설명 |
|------|------|
| `boardCells[8]` | 8칸 BoardCellBindings |
| (자동) | `GameObject.Find("Board_8Cells")` 하위 `RhythmButtonChallengeBoardCellBindings` |

### UI — 점수

| 필드 | 설명 |
|------|------|
| `scorePanels[4]` | ScorePanelBindings |
| (자동) | `GameObject.Find("Panel_RBC_Score_4Way")` |

### 스프라이트 (10버튼 + 5판정)

| 필드 | 용도 |
|------|------|
| `spriteA` ~ `spriteRight` | 버튼 아이콘 |
| `spritePerfect` ~ `spriteWrong` | 판정 아이콘 |

### SPEED UP·종료

| 필드 | 기본 | 설명 |
|------|------|------|
| `speedUpText` | - | TMP "SPEED UP!" |
| `speedUpDisplaySeconds` | 2 | 표시 시간 |
| `exitScreenFader` | - | FadeOverlay ScreenFader |
| `sessionEndHoldSeconds` | 0.35 | 종료 대기 |
| `exitFadeOutSeconds` | 1 | 페이드 시간 |

### BoardCellBindings 자식 이름 규칙

| 자식 | 컴포넌트 |
|------|----------|
| `ButtonIcon` | Image |
| `ActiveHighlight` | Image |
| `Judgment1P` ~ `Judgment4P` | Image |

### ScorePanelBindings 자식 이름 규칙

| 자식 | 컴포넌트 |
|------|----------|
| `Score` | TMP_Text |
| `PlayerLabel` | TMP_Text |

---

## 13. 데이터 흐름

```
세션 Begin — Random seed, Phase1, Stage1
    ↓
StartCurrentSegmentAudio — beatDuration 계산
    ↓
매 프레임 Tick
    ├─ SpeedUp 타이머 (해당 시)
    ├─ AdvanceBeatIfNeeded — 박 전환
    │       ├─ PlayClipForCurrentBeat
    │       ├─ OnBeatStarted → 보드 UI
    │       └─ (Input) FinalizeInputBeat(이전 박)
    ├─ TickGameplayInput — 패드 → 판정 → ScoreSum
    └─ FlushAllUi
    ↓
OnSegmentFinished — 다음 Segment/Stage/Phase 결정
    ↓
(전체 완료)
    ↓
BuildSessionReport
    ├─ FinalScore[i] = ScoreSum (0 이상)
    └─ RhythmButtonChallengeHpLossRules → HpLostThisSession
    ↓
PartySession.EndMinigameAndOpenResultScene
    ↓
Result 씬 (등수 → HP → 메인)
```

### 판정 데이터 (한 박)

```
박 N 시작 → BeatWindow.Active = true
    ↓
플레이어 입력 (첫 번째)
    ├─ 버튼 비교 → Wrong 또는 타이밍 → Perfect/Fast/Slow/Miss
    └─ BeatJudged[N]=true, ScoreSum += delta, Judgment 이미지
    ↓
추가 입력 → ExtraInputPenalty -2000
    ↓
박 N+1 시작 시 FinalizeInputBeat(N) — 미판정이면 Miss
```

---

## 14. Result 연동

### MinigameSessionReport

| 필드 | 값 |
|------|-----|
| `MinigameId` | `"rhythm_button_challenge"` |
| `FinalScore` | 본게임 점수 (연습이면 0 — 현재 진입 경로 없음) |
| `HpLostThisSession` | `RhythmButtonChallengeHpLossRules` |
| `Rank` | Result 씬에서 계산 |

### HP 규칙 (`RhythmButtonChallengeHpLossRules`)

1. `FinalScore < 500,000` → HP 감소
2. 참가 2명 이상: 하위 50% → HP 감소
3. OIIA와 동일하게 **OR** 합집합

### OIIA와 다른 점

- RBC 종료 후 **Oiia 연습/본게임 사이클 큐** (`QueueOiiaMainRoundAfterPracticeEnded`) **적용 안 함**
- `GameFlowDirector.OnMinigameComplete`에서 `oiiaSession` 체크만 OIIA에 해당

### Result Flavor

- `RhythmButtonChallengeResultMinigameFlavor`: ID 매칭만, 장식 없음

---

## 15. 코드와 문서의 차이점

| 항목 | 실제 | 비고 |
|------|------|------|
| 씬 완성도 | **스텁** | GameObject 이름만 있고 **MonoBehaviour 미부착** |
| Build Settings | **미등록** | 빌드에 포함되지 않음 |
| UI·오디오 에셋 | **없음** | 코드만 완성 |
| 연습 모드 | 코드는 지원 가능 | 메뉴에서 진입 불가 |
| `PlayerLabel` | 바인딩만 있음 | 갱신 코드 없음 |
| `debugRouteAllToOiia` | MainMenu.unity **true** (코드 기본값은 false) | RBC 선택해도 OIIA로 감 |
| `Assets/Sprites/Gamepad/` | 버튼 아이콘 **존재** | Module Inspector에 아직 미연결 |
| RBC AudioClip | **0개** | OIIA `Sounds/Patturn/` 과 별개 |
| Result 씬 이름 | `Results` (Build) | PartySession 기본 `"Result"` — MainMenu에서 `Results`로 설정됨 |
| Good 판정 | Fast, Slow 포함 | Miss/Wrong만 나쁨 |
| 판정 창 경계 | -50, 50 ms 포함 Perfect | Fast/Slow와 겹치지 않게 설계됨 |

---

## 16. 구현 진행도

### 완성된 부분 (코드 — Cursor)

- [x] `IMinigameModule` 전체 (`RhythmButtonChallengeMinigameModule` + 13 partial)
- [x] Phase 1/2, 5 Stage, 8 Beat 세그먼트 상태 머신
- [x] 패턴 RNG + 3연속 금지
- [x] 타이밍 판정 + 점수 + 8박 보너스 + 추가입력 -2000
- [x] `RhythmButtonChallengeBoardCellBindings` / `ScorePanelBindings`
- [x] `RhythmButtonChallengeHpLossRules` (500,000 미만 + 하위 50%)
- [x] `RhythmButtonChallengeResultMinigameFlavor`
- [x] `RhythmButtonChallengeSceneBootstrap`
- [x] `GameFlowDirector` — id `rhythm_button_challenge` → RBC 씬, OIIA 연습 큐 분리
- [x] `ResultFlowController` — RBC Flavor 기본 등록

### 완성된 부분 (Unity Editor — 사용자, 최소)

- [x] 씬 파일 생성 (`Minigame_RhythmButtonChallenge.unity`)
- [x] `RhythmButtonChallengeMinigameModule` / `RhythmButtonChallengeSceneBootstrap` MonoBehaviour 부착 (2026-06-21)
- [x] `RBC_Root` + 자식 GameObject **이름** 생성
- [x] 빈 `Canvas` + `EventSystem` + `Main Camera`

### 미완성 (Unity Editor — **여기서 중단**)

- [x] **`RhythmButtonChallengeMinigameModule` MonoBehaviour 부착** (2026-06-21)
- [x] **`RhythmButtonChallengeSceneBootstrap` MonoBehaviour 부착 + module 참조** (2026-06-21)
- [x] Canvas Scale **(0,0,0) → (1,1,1)** + Canvas Scaler 1920×1080 (2026-06-21)
- [x] `Board_8Cells` + Cell_0~7 (Bindings 컴포넌트) (2026-06-21)
- [x] `Panel_RBC_Score_4Way` + ScorePanel_1P~4P (2026-06-21)
- [x] Cell·ScorePanel **프리팹** — `Assets/RhythmButtonChallenge/Prefabs/RBC_BoardCell.prefab`, `RBC_ScorePanel.prefab` (2026-06-21)
- [x] `MusicSource` + AudioClip 24개 Inspector 연결 (2026-07-18)
- [x] `spriteA`~`spriteRight` ← SNES Pixel Pack Unpressed (2026-07-10)
- [x] 판정 Sprite 5종 ← `Assets/RhythmButtonChallenge/Sprites/` (2026-07-10)
- [ ] `speedUpText` TMP
- [ ] `FadeOverlay` + `ScreenFader`
- [ ] Module Inspector 전체 SerializeField 연결
- [ ] **Build Settings** RBC 씬 등록
- [ ] MainMenu `debugRouteAllToOiia` **false**
- [ ] 부스 패드 실플레이 검증

### 다음 작업 순서 (재개용 체크리스트)

| # | 작업 | 완료 |
|---|------|------|
| 1 | MainMenu `debugRouteAllToOiia` 해제 | [ ] |
| 2 | RBC 씬 Module·Bootstrap 스크립트 Add Component | [x] |
| 3 | Canvas Scale 1,1,1 | [x] |
| 4 | Board_8Cells 8칸 UI | [x] |
| 5 | Panel_RBC_Score_4Way 4패널 | [x] |
| 5b | Cell·ScorePanel 프리팹화 | [x] |
| 6 | MusicSource + 클립 24 | [x] |
| 7 | Gamepad/SNES Sprite 10 + 판정 Sprite 5 | [x] |
| 8 | SpeedUpText, FadeOverlay | [ ] |
| 9 | Module Inspector 전체 연결 | [ ] |
| 10 | Build Settings 등록 | [ ] |
| 11 | Play Mode 테스트 | [ ] |

### 임시·개발 설정 (현재 값)

- [ ] `MainMenu.unity`: `debugRouteAllToOiia = true` ← **RBC 진입 막음**
- [ ] `Minigame_RhythmButtonChallenge.unity`: Canvas Scale 0
- [x] `RBC_Root` Transform 위치 `(868, 537, -0.98)` → `(0,0,0)` 정리 (2026-06-21)

---

## 17. 알려진 문제점

| 문제 | 심각도 | 설명 |
|------|--------|------|
| 씬 스텁 상태 | **치명** | 플레이 불가. 모듈·부트스트랩 스크립트 미부착 |
| Build Settings 미등록 | **치명** | 빌드된 exe에서 씬 로드 불가 |
| 에셋 전무 | **치명** | UI·사운드 없이는 의미 있는 플레이 불가 |
| debugRouteAllToOiia | 높음 | RBC 테스트 자체가 불가능할 수 있음 |
| `musicSource.PlayOneShot(sessionEndClip)` | 낮음 | 종료 SFX가 musicSource 경유 — BGM과 같은 소스 |
| ExtraInput Wrong 아이콘 | 낮음 | 추가 입력 시 판정 이미지를 Wrong으로 덮어씀 (점수만 -2000) |
| 한 프레임 복수 슬롯 입력 순서 | 낮음 | ForEachSlot 순서대로 처리 — 동시 입력 우선순위는 슬롯 인덱스 순 |

---

## 18. AI 참고 섹션

### 절대 수정하면 안 되는 부분

- `BoothUsbGamepadLayout` 매핑 (A/B/X/Y/L/R/방향) — RBC `ReadAnyGameplayButtonPressed`와 일치
- `RhythmButtonChallengeHpLossRules` 500,000 임계값 (`<` 비교) — 기획 변경 없이 유지
- `BeatsPerSegment = 8`, `StagesPerPhase = 5` — 오디오 배열 길이·상태 머신과 연동
- `IMinigameModule` / `MinigameSessionReport` 계약

### 수정 시 주의할 부분

- `AudioFlow.AdvanceBeatIfNeeded` — 박 건너뜀·세그먼트 종료 타이밍
- `FinalizeInputBeat` 호출 시점 (다음 박 시작 시 이전 박 마감)
- `Time.unscaledTimeAsDouble` — timeScale 독립 의도
- Phase 2 `pitch = 2` → `beatDuration` 절반 — 판정 창도 같이 짧아짐
- partial 파일 구조 유지
- `GameObject.Find` 폴백 이름: `Board_8Cells`, `Panel_RBC_Score_4Way`, `FadeOverlay`

### 재사용 가능한 코드

- OIIA와 동일한 `MinigameExitSequence`, `SceneBootstrap` 패턴
- `RhythmButtonChallengeBoardCellBindings` / `ScorePanelBindings` 자동 연결 패턴
- `RhythmButtonChallengeHpLossRules` (OIIA `OiiaHpLossRules`와 구조 동일)
- 세그먼트 기반 오디오 타임라인 (`AudioFlow.cs`)

### 신규 기능 추가 시 진입 지점

| 기능 | 위치 |
|------|------|
| 스테이지 수·박 수 변경 | `Constants.cs`, `AudioFlow`, `Pattern.cs` |
| 버튼 풀 변경 | `Pattern.cs` `StageButtonPools` |
| 판정 창·점수 | `Constants.cs`, `Gameplay.cs` |
| 새 Phase | `AudioFlow.OnSegmentFinished`, `State.cs` |
| 연습 모드 | `GameFlowDirector.StartSelectedMinigame` + `Begin.cs` |
| UI 요소 | `Ui.cs`, Bindings 클래스 |
| Result 장식 | `RhythmButtonChallengeResultMinigameFlavor.cs` |

### 디버깅 시 먼저 확인할 위치

1. **씬에 MonoBehaviour 부착 여부** (현재 최대 이슈)
2. Build Settings 등록
3. `debugRouteAllToOiia` false 여부
4. `musicSource` + 24 clips 연결
5. `boardCells` / `scorePanels` 자동 수집 성공 여부
6. `Joystick.all` 패드 수
7. `_flowState` / `_segmentKind` (어느 구간인지)
8. `_beatDurationSec` (0.5 폴백이면 클립 미연결)

---

## 19. 사용자 검증 필요 항목

현재 씬·에셋 상태로는 대부분 검증 **불가**. 아래는 씬·에셋 완성 후 확인할 체크리스트.

- [ ] `RhythmButtonChallengeMinigameModule` 컴포넌트 부착·Inspector 전체 연결
- [ ] `RhythmButtonChallengeSceneBootstrap` → module 참조
- [ ] 8칸 보드 레이아웃·하이라이트 동작
- [ ] Stage Reveal 순차 아이콘 공개
- [ ] Stage Input 타이밍 판정 (Perfect/Fast/Slow/Miss)
- [ ] 4슬롯 동시 입력·판정 이미지
- [ ] 8박 올클리어 +30,000 보너스
- [ ] SPEED UP 2초 오버레이 + Phase 2 2배속
- [ ] 24개 박자 클립 길이 동일 여부
- [ ] 부스 패드 10버튼 입력
- [ ] ESC 조기 종료 → Result
- [ ] Build Settings 추가 후 빌드 플레이
- [ ] HP 500,000 미만·하위 50% 규칙
- [ ] `debugRouteAllToOiia` 꺼진 상태 카탈로그 진입
- [ ] FadeOverlay 종료 페이드
- [ ] Canvas 스케일·해상도 (현재 Scale 0 → 수정 필요)

---

## 부록: 전체 세션 타임라인 (코드 기준)

| 순서 | Phase | Segment | Stage | 비고 |
|------|-------|---------|-------|------|
| 1 | 1 | PhaseIntro | - | 8박 |
| 2 | 1 | StageReveal | 1 | 패턴 공개 |
| 3 | 1 | StageInput | 1 | 입력·판정 |
| 4 | 1 | StageReveal | 2 | |
| 5 | 1 | StageInput | 2 | |
| ... | ... | ... | ... | |
| 12 | 1 | StageInput | 5 | |
| 13 | - | SpeedUp | - | 2초 |
| 14 | 2 | PhaseIntro | - | pitch×2 |
| 15~24 | 2 | Reveal/Input ×5 | 1~5 | |
| 25 | - | Complete | - | Result |

총 Input 스테이지: **10회** (Phase당 5 × 2).

---

*관련 문서: `01_프로젝트_개요.md`, `03_Booth_USB_Controller_매핑.md`, `04_메인메뉴.md`*

*코드 위치: `Assets/_Project/Scripts/Minigames/RhythmButtonChallenge/`*  
*씬: `Assets/Scenes/Minigame_RhythmButtonChallenge.unity`*  
*카탈로그 ID: `rhythm_button_challenge`*
