# 05_OIIA

> 문서 기준일: 코드·씬 파일 직접 분석 (추측 없음). **에디터 조작 가이드·플레이 검증 체크리스트는 본 문서에 두지 않는다** (`Project_Master_Context.md` §2·§3). 검증 타임라인은 `02_개발_진행_일지.md`.  
> **갱신**: 2026-07-11 — 디제잉 레이브 개편 **1단계** (UI 바인딩·데이터 구조만). 아래 §1~§18 본문은 아직 **레거시 `oiiaiooiiiai` 루프** 기준. 개편 진행은 §0·`02_개발_진행_일지.md`.

---

## 0. 개편 중 — OIIA 디제잉 레이브 (Rave)

> **상태**: 1단계 바인딩 + **1.5단계 레거시 제거·에디터 정리·Play 검증 완료** (2026-07-12). 다음 = **2단계(10키 판정)**.  

### 목표 컨셉 (기획)

- 고정 문자 패턴·게이지 → **시간 기반 글로벌 티어** + SNES **10키** 디제잉 박스.
- 상시 **활성 타겟 3개** (`SnesControllerButtonVisual.SetHighlighted`). 순서 무관 성공 → 해당 키 끄고 비활성 중 1개 즉시 보충.
- 60초 글로벌 티어로 전 슬롯 배경(크로마키→우주→클럽)·BGM 동기화. **30콤보** 시 3초 피버(전 버튼 정답).

### 1단계 바인딩 (유지)

| 심볼 | 위치 | 역할 |
|------|------|------|
| `OiiaDjPadButtonId` · `DjPadButtonCount`(10) | `Types.cs` · `Constants.cs` | A…Right 인덱스 |
| `DjBoxRoot` · `DjFaceButtons` · `DjDpadButtons` · `DjShoulderButtons` · `DjPadButtons[]` | `SlotUiBindings` / `OiiaSlotPanelBindings` | SNES Prefab |
| `HudScoreText` · `HudComboText` · `HudFeverText` | 동일 | Hud (Combo는 선택) |
| `SubPatternGuideText` | 동일 | 가사 흐름 |
| `StageScreenRoot` · `StageBackgroundChromaKey/Space/Club` | 동일 | 전광판 |

### 1.5단계에서 코드 제거한 레거시

| 제거 | 비고 |
|------|------|
| `BurstText` partial · 풀 바인딩 | NRE 원인. Hierarchy `BurstText*` 는 에디터에서 삭제/비활성 |
| `ShuffleEffect` · `ButtonShuffle` | O/I/A 매핑 셔플 |
| `GuideFeedback` · Y/X/A/B 가이드 바인딩 | SNES 10키로 대체 |
| `GaugeSlider` · 게이지 드레인 | 룰 폐기 |
| `SequenceText` 커서 UI · `_patternLower` 입력 루프 | `TickGameplay` 스텁 |
| `OiiaPhysicalButton` MapO/I/A | 삭제 |

**유지(골격)**: Begin/Tick/Exit · Practice READY · Timer · Cat/Blur/Waiting · TierBgm·CatMovement(티어 연동은 후속 개조) · Dj 바인딩.

---

## 1. 한 줄 요약

~~4명이 각자 패드로 **O · I · I · A** 글자 순서를 맞추며…~~ → **개편 중**: SNES 10키 디제잉 레이브(§0). 아래 §2~는 레거시 서술이며 1.5단계 이후 코드와 불일치할 수 있음.

---

## 2. 게임 목적

### 플레이어가 해야 할 일

- 화면에 보이는 패턴 순서대로 버튼을 누른다.
- 패턴은 항상 **`oiiaiooiiiai`** (12글자)이며, 끝까지 맞추면 처음부터 다시 반복한다.
- 본게임에서는 **시간 제한 안에 최대한 높은 점수**를 낸다.
- 연습 모드에서는 규칙을 익히고, 모두 준비되면 본게임으로 넘어간다.

### 승리(유리한 결과)

- 이 게임은 1:1 대전 승패가 아니라 **점수 경쟁**이다.
- 본게임 종료 후 **점수가 높을수록** Result 화면에서 **높은 등수**를 받는다.
- HP 감소 조건에 해당하지 않으면 **연승(Win Streak)** 이 1 올라간다.

### 실패(불리한 결과)

- **한 판 안에서의 실패**: 잘못 누르거나, 본게임에서 게이지가 바닥나면 해당 입력이 실패 처리된다. (즉시 탈락은 아님. 계속 플레이 가능)
- **라운드 종료 후 HP 감소** (본게임만, **OIIA 전용 판정**):
  1. 최종 점수가 **저점수 컷(`HpLowScoreThreshold`, 코드 기본 8,000점)** 이하이거나
  2. 참가자가 2명 이상일 때 **하위 50%** (2명→1명, 3명→1명, 4명→2명)
- 위 조건에 걸리면 Result 씬에서 **HP가 1 감소**한다. (HP는 **플레이어 번호**에 귀속·판 중에는 안 깎임·본게임 1판 최대 −1 — `01_프로젝트_개요.md` §파티·세션)
- HP가 0이 되면 **GAME OVER** 상태가 된다.

### 연습 vs 본게임

| 구분 | 연습 | 본게임 |
|------|------|--------|
| 점수 | 표시 안 함 (`-`) | 실시간 누적 |
| 타이머 | 없음 | 중앙 상단 `TIME xx.x` |
| 게이지 바 | 숨김 | 표시, 실수 시 감소 |
| 고양이 애니메이션 | 숨김 | 표시 |
| HP 규칙 | 적용 안 함 | 적용 |
| 종료 후 | 본게임으로 이어질 수 있음 | Result 씬으로 이동 |

---

## 3. 플레이 흐름

> 메인 메뉴 → Result → 메뉴 복귀 등 **파티 전체 흐름**·연습→본게임 **공통 원칙**은 `01_프로젝트_개요.md`, 메뉴 UI는 `04_메인메뉴.md` 참고. 아래는 **OIIA만** 해당하는 흐름이다.

### OIIA 진입·사이클 (요약)

```
[메뉴] OIIA 선택 → 연습(또는 본, PartySession 큐) → 본게임 → Result → [메뉴]
         ↑________________ 연습 끝 후 다음 Enter는 본 1회 ________________|
         |________________ 본 끝 후 다음 Enter는 다시 연습 _______________|
```

### OIIA 씬 내부 흐름 (연습)

```
Begin() — 슬롯·UI 초기화
    ↓
매 프레임 Tick()
    ├─ 참가 슬롯: 패턴 입력 연습 (점수·게이지 없음)
    ├─ START 버튼: READY 토글
    └─ 운영자 Enter + 전원 READY → TransitionPracticeToMainRound()
            ↓
        PartySession 갱신 후 Begin(본게임 컨텍스트) — 씬 재로드 없이 같은 씬에서 본게임 시작
```

### OIIA 씬 내부 흐름 (본게임)

```
Begin() — 점수·게이지·타이머 초기화
    ↓
매 프레임 Tick()
    ├─ 타이머 감소 (0이 되면 종료)
    ├─ 참가 슬롯: 패턴 입력 + 게이지 감소 + 점수 계산
    ├─ 티어 BGM·고양이 애니메이션·UI 갱신
    └─ ESC → 즉시 종료
    ↓
CompleteSession()
    ├─ 종료 효과음 (연결되어 있을 때)
    ├─ 0.35초 대기 + 1초 페이드 아웃
    └─ MinigameSessionReport 생성 → Result 씬 로드
```

### 텍스트 플로우 차트

```
[메뉴] READY
   ↓
[연습] 패턴 익히기 → START×N → 운영자 Enter
   ↓
[본게임] 60초 점수 올리기
   ↓
[Result] 등수 공개 → HP 감소 연출
   ↓
[메뉴] 복귀
```

---

## 4. 화면 구성

### 플레이어 번호 vs OIIA 슬롯 패널

- **플레이어 데이터**(HP·점수·참가 여부)는 `PartySession`의 **플레이어 번호 0~3**(화면 표기 1P~4P)에 귀속된다. 슬롯 패널 자체에 데이터가 붙는 것이 아니다.
- OIIA는 참가 중인 플레이어를 **세로 4분할 슬롯 패널**(`OiiaSlotPanel` × 4)로 **보여 주는** 게임이다. 이 레이아웃은 **OIIA 전용**이며, 다른 미니게임은 배치가 다르거나 슬롯 UI가 없을 수 있다.
- `slotPanels[i]` = **플레이어 번호 i**의 화면 칸. **1P(0)가 맨 왼쪽** → 4P(3)가 맨 오른쪽 (세로 4분할 공통 규칙). 비참가(`SlotState.EMPTY`)면 `WAITING`·검은 Blur만 표시.
- **패드 맞춤**: 부스에서는 어떤 패드가 연결됐는지 보고 **플레이어 자리에 맞게 패드를 옮기면** 된다. USB 꽂는 순서를 맞출 필요는 없다.

### OIIA 화면 레이아웃 (세로 4분할)

씬 `Minigame_O.I.I.A..unity` 기준. 화면은 **세로 4분할**(가로로 나란한 4칸)로 4개의 슬롯 패널이 배치된다. 2×2 격자가 아니다.

`Panel_O.I.I.A._4Way` 아래 슬롯 패널은 화면 너비를 25%씩 차지하며, **왼쪽부터** `SlotPanel_1`(1P) → `SlotPanel_2`(2P) → `SlotPanel_3`(3P) → `SlotPanel_4`(4P) 순이다. (씬 RectTransform 앵커: 0~0.25 / 0.25~0.5 / 0.5~0.75 / 0.75~1.0)

각 칸은 위아래로 화면 전체 높이를 쓰고, 칸 안에서 **왼쪽에 세로 게이지 바**, **가운데에 고양이**, **위·아래에 텍스트**가 배치된다.

### 본게임 화면 (개념도)

에디터 Game 뷰 기준 레이아웃. (참가하지 않은 슬롯은 고양이 위에 `WAITING`이 겹쳐 보인다.)

```
┌────────────┬────────────┬────────────┬────────────┐
│  슬롯 1P   │  슬롯 2P   │  슬롯 3P   │  슬롯 4P   │
│ [상단 TXT] │ [상단 TXT] │ [상단 TXT] │ [상단 TXT] │
│ │게이지│🐱│ │ │게이지│🐱│ │ │게이지│🐱│ │ │게이지│🐱│ │
│ │ (세로) │ │ │ (세로) │ │ │ (세로) │ │ │ (세로) │ │
│ [하단 TXT] │ [하단 TXT] │ [하단 TXT] │ [하단 TXT] │
└────────────┴────────────┴────────────┴────────────┘
              TIME 25.3  ← 화면 최상단 중앙 (4칸 위에 겹침)
```

- **상단 TXT**: 패턴 글자(`Sequence`) — 본게임에서 맞춘 글자 + 다음 글자(빨간색)
- **세로 게이지**: `Gauge` Slider — 칸 왼쪽에 세로로 표시
- **고양이**: `Cat` — 칸 중앙. 1티어 정지; 2·3티어 유지 중 슬롯 패널 안 직선 바운스·Z 회전(3티어 2배). 정답·Animator SpinOnce/Loop는 `CatAnimator.cs`
- **하단 TXT**: 점수(`Score`) 등
- **비참가(EMPTY)**: 고양이 위에 **`WAITING`** 문구 + Blur 검은막 (에디터 스크린샷과 동일)

### 연습 화면 (개념도)

레이아웃은 본게임과 같이 **세로 4칸**이다. 차이점만 아래와 같다.

```
┌────────────┬────────────┬────────────┬────────────┐
│  슬롯 1P   │  슬롯 2P   │  슬롯 3P   │  슬롯 4P   │
│            │            │            │            │
│   OIIA     │   OIIA     │  WAITING   │  WAITING   │
│  (중앙)    │  (중앙)    │  (비참가)   │  (비참가)   │
│  READY?    │            │            │            │
│            │            │            │            │
└────────────┴────────────┴────────────┴────────────┘
        (타이머 없음 · 게이지·고양이 숨김)
```

- 참가 슬롯: `Sequence` 텍스트가 **칸 중앙**에 크게 표시 (`ApplyPracticeCenteredSequenceText`)
- 준비 완료 시: `READY` 문구 표시
- 게이지·고양이: 비표시

### 슬롯 패널 하나의 구조 (`OiiaSlotPanel` 프리팹)

한 칸(세로 열) 안의 자식 배치 개념:

```
┌─ OiiaSlotPanel (칸 전체, 흰 배경 Image) ────────┐
│  [상] Sequence  — 패턴 글자 (TMP)                  │
│  ┌──┬──────────────────────────────┐              │
│  │게│  Cat — 고양이 (Image+Animator)  │              │
│  │이│  O / I / A — 입력 플래시 (겹침)  │              │
│  │지│  Waiting — "WAITING" (EMPTY)   │              │
│  │( │  Blur — 실패/티어/EMPTY 막      │              │
│  │세│                                │              │
│  │로)│                               │              │
│  └──┴──────────────────────────────┘              │
│  [하] Score — 점수 (TMP)                          │
│  ControllerGuide — 반원 바디 + Y/X/A 가이드 버튼   │
│  ShuffleEffect — 루프 완주 셔플 스프라이트 (중앙)   │
│  Ready — "READY" (연습, 칸 안 배치)                │
└──────────────────────────────────────────────────┘
```

---

## 5. UI 구성 요소

| UI 오브젝트 | 역할 | 누가 보는가 | 연습 | 본게임 | 비참가(EMPTY) |
|-------------|------|-------------|------|--------|---------------|
| `Sequence` (SequenceText) | 맞춘 글자(대문자) + 다음 글자(빨간색) 표시 | 해당 슬롯 플레이어·관중 | ○ 중앙 배치 | ○ 원래 위치 | Blur 뒤 숨김 |
| `Gauge` (GaugeSlider) | 입력 유예 시간. 1=여유, 0=실패. **칸 왼쪽 세로 바** | 플레이어 | 숨김 | ○ | 숨김/무의미 |
| `Score` (ScoreText) | 누적 점수 | 플레이어·관중 | `-` 고정 | 숫자 표시 | - |
| `Cat` (CatAnimator) | 1티어: 중앙 정지. 2·3티어: UI 바운스 이동+Z 회전(`CatMovement`). 정답 SpinOnce / 2티어+ SpinLoop | 관중·플레이어 | 숨김 | ○ | 숨김 |
| `O` / `I` / `A` (InputFlash) | 버튼 누를 때 글자 번쩍임 | 플레이어 | ○ | ○ | - |
| `Ready` (PracticeReadyText) | "READY" 문구 | 플레이어 | READY 시만 | 항상 숨김 | 숨김 |
| `Waiting` (WaitingText) | "WAITING" 깜빡임 | 관중 | EMPTY일 때 | EMPTY일 때 | ○ |
| `Blur` (Image) | 실패 빨강, 티어 경고 흰색, 3티어 무지개, EMPTY 검은막 | 모두 | EMPTY 막 | 연출용 | ○ 검은 오버레이 |
| `SlotPanelBackgroundImage` | 슬롯 흰 배경. 2티어 이상 유지 시 투명 | 시각 연출 | - | 2티어+ 투명 | - |
| `Timer` (mainRoundTimerCentralTop) | `TIME xx.x` | 모두 | 숨김 | ○ | - |
| `FadeOverlay` | 종료 시 페이드 아웃 | 모두 | 종료 시 | 종료 시 | - |
| `ControllerGuide` | 슬롯 하단 반원 바디 + **Y / X / A / B** 버튼. **다음 타겟** 네온·홀드·쇼크웨이브 | 플레이어·관중 | ○ | ○ | 숨김 |
| `ShuffleEffect` | 12글자 루프 완주 **셔플 이펙트** — 슬롯 중앙 스프라이트 소→대 확대 + sin 알파 페이드 (**약 1초**) | 모두 | ○ | ○ | 숨김 |
| `VideoEffectAnchor` | (레거시·미사용) 런타임 스폰은 `ControllerGuide` + 해당 `GuideButtonY/X/A` 좌표 | - | - | - | - |

### 성공 MP4 스폰 위치 (런타임)

| 맞춘 패턴 | 가이드 버튼 (위치 기준) | 스폰 부모 | 그리기 순서 |
|-----------|------------------------|-----------|-------------|
| **O** | `BtnX` | `ControllerGuide` | 해당 버튼 **좌표** + sibling **0** (Y/X/A **전부** 앞) |
| **I** | `BtnA` | `ControllerGuide` | 동일 |
| **A** | `BtnY` | `ControllerGuide` | 동일 |

버튼 `RectTransform` 레이아웃을 복사한 뒤 `successVideoEffectScale` 배율을 적용한다. `Body`는 **선택**(제거 예정) — 이펙트·가이드 로직은 버튼만 사용한다.

### 컨트롤러 가이드 — 다이아몬드 패드 (Y/X/A/B)

| UI 위치 | 가이드 Image | 기본 물리 버튼 | 비고 |
|---------|-------------|---------------|------|
| 상단 | `GuideButtonX` (`BtnX`) | X (Trigger) | 루프 완주 후 셔플 시 매핑 변경 |
| 좌측 | `GuideButtonY` (`BtnY`) | Y (Button 4) | |
| 우측 | `GuideButtonA` (`BtnA`) | A (Button 2) | |
| 하단 | `GuideButtonB` (`BtnB`) | B (Button 3) | 4번째 — 셔플 시 O/I/A 중 하나에 매핑되거나 미사용 |

- **타겟 표시**: 다음 입력 버튼 자식 `Neon_Outline` — 상시 scale 1·α 1, 정답 직후에도 **즉시** 다음 타겟에 표시 (`UpdateGuideNeonTarget`, 쇼크웨이브와 독립).
- **정답 쇼크웨이브** (`GuideFeedback.cs`): 별도 자식 `Neon_Shockwave`(없으면 런타임에 `Neon_Outline` 1회 복제). 버튼당 독립 타이머(4개) — 연타 시 겹쳐 확대·페이드. 상수: `NeonShockwaveDuration` 0.22s, scale 2.0~2.5 (`Constants.cs`).
- **Hold 명암**: Idle 0.2 / Hold 1.0 RGB (`GuideButtonIdleBrightness`·`GuideButtonHoldBrightness`).
- **Hold 피드백**: 누르는 동안 버튼 `localScale` 0.87 + 밝기 상승.
- **티어 진동**: `UiShake.cs`와 동일 주파수·진폭으로 Y/X/A/B **동기 흔들림**. `GuideButtonShake.cs` **삭제**.

---

## 6. 플레이어 입력

### 부스 USB 패드 매핑 (OIIA 전용)

| 물리 버튼 | Unity 경로 | 사용자 표기 |
|-----------|-----------|------------|
| Trigger | `Joystick.trigger` | X |
| Button 2 | `button2` | A |
| Button 4 | `button4` | Y |
| Button 3 | `button3` | B |

**기본 매핑** (첫 루프·게이지 바닥 리셋 후): O→X, I→A, A→Y. **12글자 루프 완주**마다 4개 중 무작위 3개를 O/I/A에 1:1 재배치 (`ButtonShuffle.cs`). 안내 텍스트는 **방향어 없이 X/Y/A/B 이름만** 사용.

### 버튼별 역할

| 버튼 | 연습 | 본게임 | 비고 |
|------|------|--------|------|
| X / A / Y / B | 패턴에 매핑된 경우만 정답 후보 | 동일 | 셔플 후 **`ShuffleEffect`** 스프라이트 연출 (~1초, 게임플레이 비정지) |
| L / R / Select | **오답** | **오답** | |
| Start | **READY 토글** | **오답** | 연습에서 START 직후 같은 프레임 오답 방지 |
| 방향키 | 무시 | 무시 | OIIA에서 읽지 않음 |

### 운영자 키보드 입력

| 키 | 역할 |
|----|------|
| Enter | 연습 → 본게임 전환 (전원 READY일 때) |
| ESC | 본게임·연습 모두 즉시 세션 종료 |
| Backspace | **Dev God Mode** 토글 (Editor·Development Build만). ON 시 **1P** A=항상 정답·타 버튼 무시·**본게임 타이머 정지** |

### 입력이 무시되는 경우

- 슬롯이 참가 중이 아닐 때 (`_aliveMask[i] == false`)
- 패드가 연결되지 않았을 때 (`SlotGamepad.Get(i) == null`)
- 실패 직후 **0.5초** 입력 잠금 (`InputLockTimer > 0`) — **Dev God Mode 1P는 예외**
- 연습 중 해당 슬롯이 **READY 상태**일 때 (본게임 전환 대기) — **Dev God Mode 1P는 예외**
- 게임 종료 처리 중 (`_completing == true`)
- **Dev God Mode ON + 1P**: A(Face A) 외 모든 패드 입력 (오타·게이지 감소 없이 무시)

### 입력이 허용되는 경우

- 참가 중(`SlotState.PLAYING`)이고, 입력 잠금이 없고, 연습 READY 대기가 아닐 때
- O/I/A 단독 입력 시 정답 처리
- 본게임에서 아무 입력 없이 게이지가 줄어드는 것은 "입력 없음"이지 "입력 무시"는 아님

---

## 7. 게임 규칙

### 핵심 규칙

1. **패턴은 고정**: `oiiaiooiiiai` (12글자). `SequenceText`에 **12글자 상시 노출** — 지난 글자 `#555555`, 타겟 형광 연두+볼드+120%, 앞 글자 `#FFFFFF`.
2. **한 글자씩 순서대로** 맞춘다. 12글자 완주 시 커서 0 + **셔플** + **`ShuffleEffect`** (~1초, 입력·게이지 **계속**).
3. **정답 시**: 커서 +1, 점수 가산(본게임), 네온 쇼크웨이브, 고양이 위치 BurstText, 패턴 SFX. **게이지 완충은 루프 완주 직후** (`BeginShuffleEffect`).
4. **오타 시** (`OnTypo`): 티어·연속 루프·커서·고양이 **유지**. 타겟 글자만 **빨간 깜빡임**. 게이지 감소는 **독립 구동**(소프트락 방지).
5. **게이지 바닥** (`OnGaugeDepleted`): 커서·연속 루프·매핑 리셋, buzz + 빨간 Blur.

### 난이도 티어 (본게임, `ConsecutiveLoopSuccesses` 기준)

| 티어 | 연속 루프 완주 수 | 게이지 1→0 시간 | 글자당 점수 | 루프 보너스 |
|------|------------------|----------------|------------|------------|
| 1티어 | 0 ~ 1 | **10초** | **300** | **1,000** |
| 2티어 | 2 | **7초** | **500** | **2,000** |
| 3티어 | 3 이상 | **5초** | **800** | **4,000** |

- 오타는 티어·연속 루프를 **강등하지 않음**. 게이지 바닥에서만 연속 루프 0.
- 본게임 제한 시간 **60초** (`MainRoundDurationSeconds`). 셔플 이펙트 **~1초** (`shuffleEffectDuration`).
- 기획 목표 점수 **100,000** (`TargetSuccessScore`).

### 티어 시각·청각 연출

| 조건 | 연출 |
|------|------|
| 연속 2·3루프 **진입 직후** (루프 완주 시) | Blur 흰색 깜빡임 **0.5초** (`TierBumpBlurRemaining`) |
| 연속 3루프+ 플레이 중 (게이지 유지) | Blur 무지개 + 알파 깜빡임, 슬롯 배경 투명 |
| 2티어+ 게이지 유지 | 고양이 SpinLoop + 슬롯/화면 바운스 (`CatMovement`) |
| 2·3티어 정답 | `UiShake` — 패턴·게이지·점수·가이드 Y/X/A/B 동기 흔들림 |
| 3티어 | 바운스·흔들림 진폭 2배 |
| 정답 | 고양이 SpinOnce + **BurstText** (± `burstTextRandomOffset`, **생성축** `burstTextSpawnRotation` + **스윙** `burstTextSwingMin~Max` @ `burstTextSwingFrequency` Hz, **티어별** Perlin 위치 진동 `burstTextShakeAmplitudeTier1~3`·`burstTextShakeFrequencyTier1~3`, **티어별 fontSize** `burstTextFontSizeTier1~3`, P5 아웃라인). Inspector **코믹스 BurstText** |
| 셔플 이펙트 ~1초 | `ShuffleEffect` — 슬롯 중앙 스프라이트 확대·페이드. 입력·게이지·고양이·UI 흔들림 **정지 없음** |
| 참가 슬롯 최대 연속 루프 기준 | 티어 BGM (`TierBgm.cs`) |

### 특수 규칙

- **동시에 여러 O/I/A 버튼**을 누르면 오답.
- **점수는 0 미만으로 내려가지 않음** (`ApplyScoreDeltaNonNegative`).
- **연습 → 본게임** 전환은 씬을 다시 로드하지 않고 `Begin()`을 다시 호출한다.
- **ESC**는 운영자/개발용 조기 종료.
- **Backspace (Dev God Mode)**: Editor·Development Build 한정. 1P A만 항상 정답·나머지 입력 무시·**본게임 `_remainingMainTime` 감소 정지**.

### 예외·경계

- 패턴 인덱스 범위 밖 문자는 코드상 O(Trigger)로 fallback (`MapPatternToPhysical` default).
- `mainRoundSeconds`가 0 이하로 설정되면 **최소 1초**로 강제 (`MainRoundMinSeconds`).
- 비참가 슬롯(`SlotState.EMPTY`)은 `_aliveMask`가 false → 게임플레이·점수 없음, WAITING 표시.

---

## 8. 점수 계산

### 점수 이벤트 (본게임만)

| 이벤트 | 점수 | 조건 |
|--------|------|------|
| 정답 1스텝 | +300 / +500 / +800 | 1·2·3티어 (`Balance.cs` 상수) |
| 패턴 1바퀴 완료 | +1,000 / +2,000 / +4,000 | 루프 보너스 (완주 직전 티어 기준) |
| 오타 | 0 | 점수·티어 페널티 없음 |
| 게이지 바닥 | 0 | 연속 루프 리셋 (점수 차감 없음) |

기획 **목표 점수 100,000** (`TargetSuccessScore`). HP 저점수 컷은 `HpLowScoreThreshold` (**8,000**, `Balance.cs`).

### 연습 모드

- `ScoreSum`은 내부적으로 0 유지, UI에는 `-` 표시.
- `MinigameSessionReport.FinalScore`도 0.

---

## 9. 내부 시스템 구조

코드를 모르는 사람을 위한 설명.

### 입력 시스템

- **플레이어 번호 i**의 패드 입력을 `SlotGamepad`로 읽는다 (내부적으로 `Joystick.all[i]`).
- 부스에서는 **화면 왼쪽 1P 자리부터** 패드를 맞춰 두면 된다.
- OIIA 전용 3버튼(X, A, Y)만 정답 후보이고, 나머지는 오답 또는 특수(START) 처리.

### 게이지(유예) 시스템

- 본게임에서 "얼마나 빨리 다음 입력을 해야 하는가"를 보여준다.
- 맞추면 게이지 100% 충전.
- 티어가 올라갈수록 게이지가 더 빨리 줄어든다.

### 점수 시스템

- 슬롯마다 `ScoreSum` 정수 하나로 관리.
- 이벤트마다 가산·감산, 0 아래로는 내려가지 않음.

### 티어(난이도) 시스템

- `AliveTierTimer`: 실수 없이 게이지를 유지한 시간.
- 이 시간으로 게이지 감속·유지 보너스·BGM·고양이·Blur·**UI 바운스** 연출이 결정된다.

#### 고양이 UI 바운스 (`OiiaMinigameModule.CatMovement.cs`)

물리 엔진(Rigidbody/Collider) 없이 **`Cat` RectTransform**의 `anchoredPosition`·`localEulerAngles.z`만 갱신한다. 경계는 **`Cat`의 부모(`OiiaSlotPanel` 루트) RectTransform** 크기와 고양이 half-extents(Pivot 0.5)로 계산.

| 티어 | 조건 | 거동 |
|------|------|------|
| 1 | `AliveTierTimer` &lt; 4s 또는 게이지 유지 실패 직후 | 중앙 (0,0), 회전 0 |
| 2 | 4s ~ 8s 미만, 게이지 유지 중 | 진입 시 무작위 360° 직진 + Z 회전. **슬롯 패널** 경계 · 160° 부채꼴 반사 |
| 3 | 8s+, 게이지 유지 중 | **`catTier3MoveSpeed`·`catTier3RotateSpeed`**(Inspector) · 화면 전체 경계 · **`localScale` 2배** · collision scale 2티어와 동일 |

- 실패(`OnFail`)·`Begin()` 본게임: `ResetCatMovementImmediate` — 이동 상태·좌표·회전·draw order 즉시 초기화.
- **3티어 화면 전체 이동**: `EnterCatTier3ScreenMode` — `Cat`를 런타임 `CatScreenMovementOverlay`(캔버스 풀스트retch)로 reparent, 경계=화면 전체. 실패·1티어 시 슬롯 패널·원래 scale 복귀.
- **3티어 크기**: `localScale` × **`CatTier3ScaleMultiplier`(2)**.
- **2·3티어 draw order**: `SetCatDrawOnTop` — Canvas `overrideSorting`, `sortingOrder = catMovementDrawSortOrderBase + 슬롯 번호`.
- Inspector: `catTier2MoveSpeed`(씬 **400**), `catTier2RotateSpeed`(씬 **270**), `catTier3MoveSpeed`(씬 **2000**), `catTier3RotateSpeed`(씬 **1080**), `catBoundaryPadding`(씬 **0**), `catBoundaryCollisionScale`(씬 **0.4**), `catMovementDrawSortOrderBase`(씬 **100**).

#### 슬롯 UI 흔들림 (`OiiaMinigameModule.UiShake.cs`)

2·3티어에서 **패턴 한 글자 정답**(`OnCorrectInput`)마다 아래 UI의 `anchoredPosition`을 짧게 진동시킨다. 1티어·연습 모드는 변화 없음.

| 대상 | 바인딩 |
|------|--------|
| 게이지 | `GaugeSlider` |
| 패턴 텍스트 | `SequenceText` |
| 점수 | `ScoreText` |
| 가이드 버튼 | `GuideButtonY` / `GuideButtonX` / `GuideButtonA` |
| 입력 플래시 | `InputFlashO` / `InputFlashI` / `InputFlashA` |

| 티어 | 진폭 |
|------|------|
| 2 | `uiShakeAmplitudeTier2` (코드 기본 10px) |
| 3 | 위 값 ×2 (`UiShakeTier3IntensityMultiplier`) |

- 실패·`Begin()`: rest 좌표 즉시 복원 (`StopSlotUiShake` / `ResetAllSlotUiShake`).
- **UI별 독립 진동**: 대상마다 `PhaseX`/`PhaseY` Perlin 위상을 두고, 정답마다 `RerollSlotUiShakeTargetPhases`로 재랜덤 — 같은 프레임에도 요소마다 다른 방향·궤적.
- Inspector: `uiShakeAmplitudeTier2`, `uiShakeDuration`(0.25s), `uiShakeFrequency`(28Hz).

### 상태 흐름 (슬롯 런타임)

각 참가 슬롯은 매 프레임 대략 다음 순서로 처리된다:

```
입력 잠금·실패 플래시·애니메이션 타이머 감소 (TickMeta)
    ↓
참가 중이면 입력 판정 + 게이지 감소 (TickGameplay)
    ↓
고양이 Animator 모드 갱신 (본게임, CatAnimator)
    ↓
고양이 UI 바운스·회전 (본게임, CatMovement)
    ↓
슬롯 HUD 흔들림 갱신 (본게임, UiShake)
    ↓
화면 UI 반영 (FlushUi)
```

### 오디오 시스템

- **패턴 SFX**: 정답 시 스텝 인덱스별 `patternStepSfx[]` OneShot
- **실패 buzz**: `buzzClip` (씬에서 **미연결** — 확인 필요)
- **티어 BGM**: 별도 AudioSource, 참가자 중 최고 유지 티어 기준 루프
- **세션 종료**: `sessionEndClip` (씬에서 **미연결** — 확인 필요)

### 연습 전환 시스템

- 플레이어 START → `_practiceReady` 토글
- 운영자 Enter → `PartySession.PrepareRound(false, played)` 후 `Begin(본게임)`

### 종료 시스템

- `MinigameExitSequence`: 0.35초 대기 → 1초 페이드 → Report 콜백

---

## 10. 실제 코드 구조

### 진입점·인터페이스

| 클래스 | 파일 | 역할 |
|--------|------|------|
| `IMinigameModule` | `Assets/_Project/Scripts/Minigames/IMinigameModule.cs` | 모든 미니게임 공통 규칙 (`Begin`, `Tick`, `RequestEarlyExit`) |
| `OiiaSceneBootstrap` | `Assets/_Project/Scripts/Minigames/Oiia/OiiaSceneBootstrap.cs` | 씬 시작 시 PartySession에서 컨텍스트 만들고 모듈 구동 |
| `OiiaMinigameModule` | `Assets/_Project/Scripts/Minigames/Oiia/OiiaMinigameModule*.cs` | OIIA 전체 로직 (partial class) |

### partial 파일 역할 분리

| 파일 | 담당 |
|------|------|
| `OiiaMinigameModule.cs` | 클래스 선언, `Id`, `DisplayName` |
| `OiiaMinigameModule.Constants.cs` | 슬롯 수, 게이지 감소(C-5), 실패 락(C-6) |
| `OiiaMinigameModule.Balance.cs` | 티어 경계·점수·유지 보너스·HP 저점수 컷 (Inspector) |
| `OiiaMinigameModule.Types.cs` | `SlotRuntime`, `SlotUiBindings` 구조체 |
| `OiiaMinigameModule.State.cs` | 런타임 필드, 패턴 문자열 |
| `OiiaMinigameModule.Config.cs` | `displayName` SerializeField |
| `OiiaMinigameModule.Begin.cs` | `Begin()`, 슬롯 초기화 |
| `OiiaMinigameModule.Tick.cs` | `Tick()`, ESC, 타이머, 루프 |
| `OiiaMinigameModule.Gameplay.cs` | 입력 판정, 정답/실패, 게이지 |
| `OiiaMinigameModule.PracticeFlow.cs` | 연습 READY, 본게임 전환 |
| `OiiaMinigameModule.Ui.cs` | 패턴 텍스트, 점수, 게이지 UI |
| `OiiaMinigameModule.GuideUi.cs` | 슬롯 하단 Y/X/A 가이드 버튼 밝기 |
| `OiiaMinigameModule.VideoEffects.cs` | 정답 MP4 Instantiate·틴트 색 |
| `OiiaMinigameModule.Timer.cs` | 본게임 타이머 |
| `OiiaMinigameModule.PatternAudio.cs` | 패턴 SFX, buzz |
| `OiiaMinigameModule.TierBgm.cs` | 티어 BGM |
| `OiiaMinigameModule.SlotPanels.cs` | 슬롯 패널 자동 수집 |
| `OiiaMinigameModule.SlotHelpers.cs` | 유틸 함수 |
| `OiiaMinigameModule.BlurFx.cs` | Blur·WAITING 연출 |
| `OiiaMinigameModule.InputLetterFlashes.cs` | O/I/A 플래시 |
| `OiiaMinigameModule.CatAnimator.cs` | 고양이 Animator (SpinOnce/SpinLoop) |
| `OiiaMinigameModule.CatMovement.cs` | 티어별 UI 바운스 이동·Z 회전·160° 부채꼴 경계 반사 |
| `OiiaMinigameModule.UiShake.cs` | 2·3티어 정답 시 슬롯 HUD anchoredPosition 흔들림 |
| `OiiaMinigameModule.ExitSequence.cs` | 종료·Report 생성 |
| `OiiaSlotPanelBindings.cs` | 프리팹 UI 자동 연결 |
| `OiiaVideoEffectController.cs` | MP4 재생·Vertex Color 틴트·종료 시 Destroy |
| `OiiaHpLossRules.cs` | HP 감소 판정 (순수 C#) |
| `OiiaResultMinigameFlavor.cs` | Result 씬 장식 훅 (현재 ID 매칭만) |

### 공유 의존 코드

| 클래스 | 파일 | 역할 |
|--------|------|------|
| `PartySession` | `Assets/_Project/Scripts/Flow/PartySession.cs` | 슬롯·라운드·씬 전환 |
| `SlotGamepad` | `Assets/_Project/Scripts/Input/SlotGamepad.cs` | 슬롯별 패드 |
| `BoothUsbGamepadLayout` | `Assets/_Project/Scripts/Input/BoothUsbGamepadLayout.cs` | 버튼 경로 상수 |
| `OperatorInputService` | `Assets/_Project/Scripts/Input/OperatorInputService.cs` | 운영자 Enter |
| `MinigameExitSequence` | `Assets/_Project/Scripts/Flow/MinigameExitSequence.cs` | 페이드 종료 |
| `GameFlowDirector` | `Assets/_Project/Scripts/Flow/GameFlowDirector.cs` | 메뉴→미니게임 진입 |

### BuiltInId

- `"oiia"` (`OiiaMinigameModule.BuiltInId`)

---

## 11. 씬 구조

**씬 파일**: `Assets/Scenes/Minigame_O.I.I.A..unity`  
**Build Settings**: 등록됨 ✓

### Hierarchy (루트)

```
Minigame_O.I.I.A.
├── Main Camera
├── EventSystem
├── O.I.I.A._Root                    ← OiiaMinigameModule + OiiaSceneBootstrap + AudioSource
├── Canvas_Minigame
│   ├── Panel_O.I.I.A._4Way          ← 세로 4분할 컨테이너 (가로 4열)
│   │   ├── SlotPanel_1              ← 1P (앵커 0.00~0.25)
│   │   ├── SlotPanel_2              ← 2P (앵커 0.25~0.50)
│   │   ├── SlotPanel_3              ← 3P (앵커 0.50~0.75)
│   │   └── SlotPanel_4              ← 4P (앵커 0.75~1.00)
│   ├── Timer                        ← mainRoundTimerCentralTop
│   └── FadeOverlay                  ← ScreenFader (종료 페이드)
```

### 중요 GameObject

| 오브젝트 | 컴포넌트 | 설명 |
|----------|----------|------|
| `O.I.I.A._Root` | `OiiaMinigameModule`, `OiiaSceneBootstrap`, `AudioSource` | 게임 로직 루트 |
| `Panel_O.I.I.A._4Way` | 자식 4개 `OiiaSlotPanelBindings` | 세로 4분할(가로 4열) 슬롯 UI |
| `Timer` | `TMP_Text` | 본게임 타이머 |
| `FadeOverlay` | `ScreenFader` | 검은 페이드 |

### Canvas

- `Canvas_Minigame`: Screen Space Overlay
- 슬롯 패널: 화면을 **가로 4등분**한 세로 열 (부스 1920×1080 기준)

### 프리팹

- `Assets/O.I.I.A/Prefabs/OiiaSlotPanel.prefab`
- 고양이 애니메이션: `Assets/O.I.I.A/Animation_SpiningCat/` (`SpiningCat_UI.anim`, `SpiningCat_UI_Loop.anim`)
- 크로마키 셰이더: `Assets/O.I.I.A/Shaders/OiiaUIChromaKey.shader` (`Shader` 이름: `UI/OiiaChromaKey`)

---

## 12. Inspector 연결

`O.I.I.A._Root` → `OiiaMinigameModule` 기준 (씬에 저장된 값).

### SerializeField 목록

| 필드 | 씬 연결 상태 | 연결 안 되면 |
|------|-------------|-------------|
| `displayName` | "OIIA" | Result 등 표시 이름만 영향 |
| `mainRoundTimerCentralTop` | Timer 연결됨 | 본게임 타이머 안 보임 (경고 로그) |
| `mainRoundSeconds` | **30** | 기본 60이 아님. 0 이하면 1초 |
| `slotPanels[4]` | 4개 패널 연결됨 | 자동 수집 시도, 실패 시 Error 로그 |
| `slotPanelsContainer` | Panel_O.I.I.A._4Way | 자동 Find |
| `sfxSource` | 같은 GO AudioSource | 패턴 SFX·buzz 무음 |
| `buzzClip` | **비어 있음** | 실패 buzz 무음 |
| `tier2DrumLoop` | drum.MP3 연결됨 | 2티어 BGM 없음 |
| `tier3BeatLoop` | oiia beat.MP3 연결됨 | 3티어 BGM 없음 |
| `tierBgmSource` | 비어 있음 | 런타임 2번째 AudioSource 자동 생성 |
| `patternStepSfx[12]` | 12개 연결됨 | 패턴 12글자와 일치. 부족 시 경고 |
| `sequenceTextPulseBoostPoints` | 22 | 정답 시 글자 커짐 크기 |
| `sequenceTextNextHintColor` | 빨간색 | 다음 글자 색 |
| `emptySlotBlurAlpha` | 0.9 | EMPTY 슬롯 어둡기 |
| `inputLetterBurstDuration` | 0.3 | O/I/A 플래시 시간 |
| `inputLetterBurstFontGrowth` | 1000 | 플래시 확대량 (씬 값, 코드 기본 36과 다름) |
| `catAnimatorIdleState` 등 | Idle/SpinOnce/SpinLoop | Animator 상태 이름 불일치 시 애니 안 됨 |
| `catTier2MoveSpeed` | 120 (코드 기본) | **400** (씬) | 2티어 UI 직선 이동(anchoredPosition 단위/초) |
| `catTier2RotateSpeed` | 90 (코드 기본) | **270** (씬) | 2티어 Z 회전(도/초) |
| `catTier3MoveSpeed` | 240 (코드 기본) | **2000** (씬) | 3티어 화면 전체 직선 이동 |
| `catTier3RotateSpeed` | 180 (코드 기본) | **1080** (씬) | 3티어 Z 회전(도/초) |
| `catBoundaryPadding` | 8 (코드 기본) | **0** (씬) | 추가 여백(px). **음수**면 범위 확대 |
| `catBoundaryCollisionScale` | 0.35 (코드 기본) | **0.4** (씬) | **2·3티어** 경계 half-size 배율 |
| `catMovementDrawSortOrderBase` | 100 (코드 기본) | **100** (씬) | 2·3티어 Cat Canvas.sortingOrder = base + 슬롯 번호 |
| `uiShakeAmplitudeTier2` | 10 (코드 기본) | **10** (씬) | 2티어 정답 UI 흔들림 진폭(px). 3티어 2배 |
| `uiShakeDuration` | 0.25 (코드 기본) | **0.25** (씬) | 정답 1회당 흔들림 시간(초) |
| `uiShakeFrequency` | 28 (코드 기본) | **28** (씬) | 흔들림 진동 Hz |
| `burstTextDuration` | 0.55 (코드 기본) | — | BurstText 표시·페이드(초) |
| `burstTextRandomOffset` | **50** (코드 기본) | — | 고양이 중심 ±스폰(px) |
| `burstTextSpawnRotationMin` / `Max` | -30 / 30 | — | 생성 시 중심 Z 회전(도) |
| `burstTextSwingMin` / `Max` | -30 / 30 | — | 중심축 대비 스윙 범위(도) |
| `burstTextSwingFrequency` | 0.45 (코드 기본) | — | 스윙 속도(Hz). 작을수록 느림 |
| `burstTextShakeAmplitudeTier1` / `Tier2` / `Tier3` | 8 / 10 / 14 (코드 기본) | **64 / 80 / 112** (씬) | 티어별 Perlin 진동 진폭(px). 0=고정. 남은 시간 비례 감쇠 |
| `burstTextShakeFrequencyTier1` / `Tier2` / `Tier3` | 32 / 36 / 40 (코드 기본) | **8 / 9 / 10** (씬) | 티어별 BurstText 진동 Hz |
| `burstTextFontSizeTier1` | **200** (코드 기본) | — | 1티어 BurstText fontSize (프리팹 BurstText0~5 = 200) |
| `burstTextFontSizeTier2` | **240** (코드 기본) | — | 2티어 BurstText fontSize |
| `burstTextFontSizeTier3` | **300** (코드 기본) | — | 3티어 BurstText fontSize |
| `shuffleEffectSprite` | — (코드 기본) | — | 셔플 이펙트 스프라이트. 비면 `OiiaSlotPanel/ShuffleEffect` Image.sprite |
| `shuffleEffectDuration` | **1** (코드 기본) | — | 셔플 이펙트 총 시간(초) |
| `shuffleEffectStartScale` | **0.25** (코드 기본) | — | 시작 크기 (localScale) |
| `shuffleEffectEndScale` | **2.2** (코드 기본) | — | 최대 크기 (localScale) |
| `shuffleEffectScaleGrowSpeed` | **1** (코드 기본) | — | 확대 속도 배율. 클수록 빨리 최대 크기 도달 |
| `tier2StartSeconds` | 4 (코드 기본) | **4** (씬) | 2티어 AliveTier 경계(초) |
| `tier3StartSeconds` | 8 (코드 기본) | **8** (씬) | 3티어 경계(초) |
| `scorePerCorrectStepTier1` | 50 | **500** (씬) | 1티어 정답 1스텝 |
| `scorePerCorrectStepTier2` | 65 | **1000** (씬) | 2티어 |
| `scorePerCorrectStepTier3` | 80 | **2000** (씬) | 3티어 |
| `scorePerLoopComplete` | 600 | **1000** (씬) | 패턴 1바퀴 |
| `scorePerFail` | −400 | **−1000** (씬) | 오답·게이지 바닥 |
| `sustainBonusTier1PerSecond` | 250 | **500** (씬) | 1티어 유지 보너스/초 |
| `sustainBonusTier2PerSecond` | 350 | **1500** (씬) | 2티어 |
| `sustainBonusTier3PerSecond` | 450 | **2500** (씬) | 3티어 |
| `hpLowScoreThreshold` | 8000 | **100000** (씬) | Result HP 저점수 컷 |
| `exitScreenFader` | FadeOverlay 연결됨 | `GameObject.Find("FadeOverlay")` 폴백 |
| `sessionEndHoldSeconds` | 0.35 | 종료 전 대기 |
| `exitFadeOutSeconds` | 1 | 페이드 시간 |
| `sessionEndClip` | **비어 있음** | 종료 효과음 없음 |
| `successVideoEffectPrefab` | **비어 있음** | 없으면 MP4 이펙트 생략 |
| `successVideoEffectScale` | 10 (코드 기본) | **2** (씬) | 스폰 시 `localScale` 배율 |
| `successVideoEffectPlaybackSpeed` | 1.5 (코드 기본) | **4** (씬) | `VideoPlayer.playbackSpeed` |
| `successVideoEffectColorO` | Blue (0.15, 0.45, 1) | **(0.67, 0.78, 1)** (씬) | O 정답 MP4 틴트 |
| `successVideoEffectColorI` | Red (1, 0.2, 0.2) | **(1, 0.74, 0.74)** (씬) | I 정답 MP4 틴트 |
| `successVideoEffectColorA` | Green (0.18, 0.58, 0.22) | **(0.78, 1, 0.80)** (씬) | A 정답 MP4 틴트 |
| `guideButtonShakeAmplitude` | 8 (코드 기본) | — | 정답 **임펄스** 진폭(px) |
| `guideButtonShakeDuration` | 0.2 (코드 기본) | — | 임펄스 지속(초) |
| `guideButtonShakeFrequency` | 32 (코드 기본) | — | 힌트·임펄스 Hz |
| `guideButtonHintShakeAmplitude` | 4 (코드 기본) | — | **다음 타겟** 상시 힌트 진폭(px) |

### OiiaSlotPanelBindings (프리팹 자식 이름 규칙)

| 자식 이름 | 컴포넌트 |
|-----------|----------|
| `Sequence` | TMP_Text |
| `Gauge` | Slider |
| `Score` | TMP_Text |
| `Blur` | Image |
| `Ready` | TMP_Text |
| `Cat` | Animator |
| `O`, `I`, `A` | TMP_Text |
| `Waiting` | TMP_Text |
| `ControllerGuide` | (루트 GameObject) |
| `ControllerGuide/Body` | Image — (선택·**제거 예정**) 반원 바디. 코드 미참조 |
| `ControllerGuide/BtnY`, `BtnX`, `BtnA`, `BtnB` | Image — Y / X / A / B 버튼 |
| `ShuffleEffect` | Image — 루프 완주 셔플 스프라이트 (슬롯 중앙) |
| `VideoEffectAnchor` | RectTransform — (레거시·미사용) |
| (루트) | Image → SlotPanelBackgroundImage |

---

## 13. 데이터 흐름

### 본게임 입력 → 결과

```
USB 패드 버튼 (슬롯 i)
    ↓
SlotGamepad.Get(i) → Joystick
    ↓
TickGameplay(i) — 정답/오답 판정
    ↓
OnCorrectInput → SpawnSuccessVideoEffect (해당 BtnY/X/A 뒤·색상별 MP4)
    ↓
SlotRuntime 갱신 (Cursor, Gauge01, ScoreSum, AliveTierTimer)
    ↓
FlushUi(i) — SequenceText, Gauge, Score, GuideUi(Y/X/A 밝기), Blur, Cat...
    ↓
(타이머 종료 또는 ESC)
    ↓
BuildSessionReport()
    ├─ FinalScore[i] = Max(0, ScoreSum)
    └─ OiiaHpLossRules.FillHpLost() → HpLostThisSession[]
    ↓
PartySession.EndMinigameAndOpenResultScene(report)
    ↓
Results 씬 → ResultFlowController
    ├─ 등수 계산·공개
    ├─ HpLost → HP -1 연출
    └─ 메인 메뉴 복귀
```

### 연습 → 본게임 전환

```
START (슬롯 i) → _practiceReady[i] 토글
    ↓
운영자 Enter + AllAlivePracticeReady()
    ↓
PartySession.PrepareRound(false, playedMask)
    ↓
Begin(MinigameContext(..., IsPractice: false))
    ↓
동일 씬에서 본게임 UI·로직으로 재시작
```

---

## 14. Result 연동

> Result 씬 **공통 연출**(등수·HP·Ready·메인 복귀)은 `01_프로젝트_개요.md` §Results 씬 참고.

### MinigameSessionReport (OIIA가 채우는 값)

| 필드 | OIIA에서 채우는 값 |
|------|-------------------|
| `MinigameId` | `"oiia"` |
| `FinalScore[i]` | 본게임 점수 (0 이상). 연습이면 0 |
| `HpLostThisSession[i]` | `OiiaHpLossRules` 결과. 연습이면 계산 안 함 |
| `Rank[i]` | 미니게임에서는 비움 → Result 씬에서 계산 |

### OIIA HP 감소 판정 (`OiiaHpLossRules`)

1. 참가자 중 `FinalScore <= hpLowScoreThreshold`(씬 **100,000**) → `HpLostThisSession` true
2. 참가자 2명 이상: 점수 오름차순 **하위 50%** → true
3. 두 조건은 **OR**. 실제 HP −1은 Result `CoHpProcess()`에서 1회만

### OIIA 메뉴 사이클 (`PartySession` + `GameFlowDirector`)

- 연습 종료 후: `QueueOiiaMainRoundAfterPracticeEnded()` → 다음 메뉴 Enter 진입은 **본게임 1회**
- 본게임 종료 후: `ResetOiiaCycleAfterMainSession()` → 다음 Enter는 **다시 연습**

### OIIA Result 장식

- `OiiaResultMinigameFlavor`: 현재 ID 매칭만. **전용 비주얼 구현 예정** (§15-C-11)

---

## 15. 코드와 문서의 차이점

### 원칙

1. **`Minigame_O.I.I.A..unity` 씬 Inspector 값 = 정답** (플레이 테스트 후 적용한 값).
2. **아직 넣지 않은 효과음**: `buzzClip`, `sessionEndClip`만 의도적 미연결 (§15-B).
3. **코드만 있는 값·기획 의도**: §15-C. **2026-06-19** 확정·반영.

### 기획·밸런스 확정 사항 (2026-06-19)

| ID | 항목 | 상태 | 비고 |
|----|------|------|------|
| C-1 | 패턴 `oiiaiooiiiai` | **확정** | 12글자 최종안 |
| C-2 | 점수 티어별 **+500/+1000/+2000**, 바퀴 +1000, 실패 −1000 | **Inspector·씬 확정** | 구 단일 +60 |
| C-3 | 유지 보너스 **+500/+1500/+2500**/초 | **Inspector·씬 확정** | 구값 +300~500 |
| C-4 | 티어 경계 **4s / 8s** | **Inspector·씬 확정** | 구값 5s/10s |
| C-5 | 게이지 감소 1.5 / 0.7 / 0.3s | **확정** | |
| C-6 | 실패 락 0.5s | **확정** | |
| C-7 | Blur·WAITING (`BlurFx.cs`) | **현행 OK** | Inspector 노출 희망 |
| C-8 | `hpLowScoreThreshold` **100,000**·하위 50% | **Inspector·씬 확정** | 구값 10,000 · `ExitSequence`→`FillHpLost` |
| C-9 | OIIA 연습→본 | **확정** | 씬 내 `Begin` 재호출. 전 게임 연습 필수는 `01` §연습 |
| C-10 | `debugRouteAllToOiia`·카탈로그 | **보류** | `04_메인메뉴.md` |
| C-11 | `OiiaResultMinigameFlavor` | **구현 예정** | |
| C-12 | OIIA 세로 4분할 UI | **확정** | 1P~4P **왼쪽부터** (`slotPanels[0]`…`[3]`). 패드는 부스에서 현장 맞춤 |

---

### 15-A. OIIA 씬에서 설정·테스트 완료 (씬 값 우선)

`Minigame_O.I.I.A..unity` → `OiiaMinigameModule`

| Inspector 필드 | 코드 기본값 | 씬 저장값 | 게임에서 하는 일 |
|----------------|------------|-----------|------------------|
| `mainRoundSeconds` | 60 | **30** | 본게임 제한 시간(초) |
| `emptySlotBlurAlpha` | 0.8 | **0.9** | EMPTY 슬롯 검은 막 농도 |
| `inputLetterBurstDuration` | 0.2 | **0.3** | O/I/A 입력 플래시 유지 시간 |
| `inputLetterBurstFontGrowth` | 36 | **1000** | 플래시 시 글자 커지는 양(포인트) |
| `patternStepSfx` 배열 길이 | 13 (기본) | **12개 연결** | 패턴 12글자와 일치 |

씬 값과 **동일**하여 유지 중: `displayName`, `sequenceTextPulse*`, `tierBgmVolume`=0.85, `tier2BgmPitchScale`=2, 종료 페이드 타이밍, `tier2DrumLoop`·`tier3BeatLoop`·`patternStepSfx` 12개, `exitScreenFader`·`slotPanels` 4개. `tierBgmSource` 비어 있음 → 런타임 보조 `AudioSource` (정상).

---

### 15-B. 아직 미연결 (의도적 · 효과음만)

| 필드 | 씬 상태 | 비고 |
|------|---------|------|
| `buzzClip` | null | 실패 buzz 무음 |
| `sessionEndClip` | null | 종료 SFX 무음 |

패턴 SFX 12개·티어 BGM은 **연결 완료**.

---

### 15-C. OIIA 코드만 있는 값 (상세)

§15 기획·밸런스 표의 항목별 코드 위치·현재 값.

#### C-1. 패턴 — **확정**

| 항목 | 값 | 위치 |
|------|-----|------|
| `_patternLower` | `"oiiaiooiiiai"` (12글자) | `OiiaMinigameModule.State.cs` |

#### C-2. 점수 — **Inspector** (`OiiaMinigameModule.Balance.cs`)

| 필드 | 코드 기본값 | **씬 저장값** |
|------|------------|--------------|
| `scorePerCorrectStepTier1` | +50 | **+500** |
| `scorePerCorrectStepTier2` | +65 | **+1000** |
| `scorePerCorrectStepTier3` | +80 | **+2000** |
| `scorePerLoopComplete` | +600 | **+1000** |
| `scorePerFail` | −400 | **−1000** |

#### C-3. 유지 보너스 — **Inspector**

| 필드 | 코드 기본값 | **씬 저장값** |
|------|------------|--------------|
| `sustainBonusTier1PerSecond` | +250/초 | **+500/초** |
| `sustainBonusTier2PerSecond` | +350/초 | **+1500/초** |
| `sustainBonusTier3PerSecond` | +450/초 | **+2500/초** |

#### C-4. 티어 경계 — **Inspector**

| 필드 | 코드 기본값 | 구값 |
|------|------------|------|
| `tier2StartSeconds` | **4** | 5 |
| `tier3StartSeconds` | **8** | 10 |

#### C-5 · C-6 — **확정**

게이지 감소 1.5 / 0.7 / 0.3s · 실패 락 `InputLockAfterFailSeconds` 0.5s.

#### C-7 — **현행 OK**, Inspector 노출 희망

`BlurFx.cs` 상수(깜빡 속도·3티어 무지개 등).

#### C-8. HP 감소 판정 — **Inspector** (`hpLowScoreThreshold`)

| 항목 | 코드 기본값 | **씬 저장값** | 위치 |
|------|------------|--------------|------|
| `hpLowScoreThreshold` | 8,000 | **100,000** | `Balance.cs` → `FillHpLost(..., hpLowScoreThreshold)` |
| 하위 50% | 2명↑ 시 `count/2`명 | — | `OiiaHpLossRules.cs` |

판 내 실패는 HP 무관. Result `CoHpProcess`에서 `HpLost`당 `ApplyHpDelta(-1)` 1회.

#### C-9. 연습→본 — **확정**

`TransitionPracticeToMainRound()` → `PartySession.PrepareRound(false, …)` → `Begin(본게임)`. 씬 재로드 없음.

#### C-12. OIIA 세로 4분할 — **확정**

| 플레이어 번호 | OIIA 화면 칸 (왼쪽→오른쪽) |
|---------------|---------------------------|
| 0 (1P) | `SlotPanel_1` (맨 왼쪽 열) |
| 1 (2P) | `SlotPanel_2` |
| 2 (3P) | `SlotPanel_3` |
| 3 (4P) | `SlotPanel_4` (맨 오른쪽) |

**기획 규칙**: 세로 4분할이면 **1P부터 왼쪽에 차례로**만 맞으면 된다.  
**코드**: 입력은 `SlotGamepad.Get(i)` (`Joystick.all[i]`). **부스**에서는 연결 상태를 보고 패드를 옮겨 자리와 맞춘다.

입력 O/I/A 매핑은 §6. 다른 미니게임은 이 레이아웃이 **적용되지 않을 수 있음**.

상수 파일: `OiiaMinigameModule.Constants.cs`, `State.cs`, `BlurFx.cs`, `OiiaHpLossRules.cs`.

---

### 15-D. OIIA 한정 메모

| 항목 | 설명 |
|------|------|
| `MainRoundMinSeconds` | `mainRoundSeconds`≤0이면 최소 1초 |
| `patternStepSfx` 코드 기본 Size 13 | 패턴 12글자. 씬 12개면 충분 |
| 판 내 실패 | HP 무관. OIIA는 점수·커서·게이지만 영향 |

---

## 16. 구현 진행도

### 완성된 부분

- [x] IMinigameModule 구현 (Begin/Tick/Exit)
- [x] 연습 모드 + 본게임 전환
- [x] 4슬롯 패턴 입력·게이지·티어·점수
- [x] UI 바인딩 자동화 (`OiiaSlotPanelBindings`)
- [x] 티어 BGM·고양이·Blur 연출
- [x] Result 씬 연동·HP 규칙
- [x] 씬·프리팹·오디오 대부분 연결
- [x] Build Settings 등록
- [x] 컨트롤러 가이드 UI + 크로마키 MP4 성공 이펙트

### 미완성·임시

- [ ] `buzzClip`, `sessionEndClip` — **의도적으로 미연결** (§15-B)
- [ ] `OiiaResultMinigameFlavor` 장식 미구현
- [ ] `debugRouteAllToOiia = true` (개발 편의, 출시 전 해제 필요)
- [ ] placeholder 미니게임 6개 (카탈로그만 존재)

### 추후 개선 예정

- Blur·WAITING 연출 상수 Inspector 노출 (C-7)
- OIIA Result 전용 비주얼 (`OiiaResultMinigameFlavor`, C-11)
- RBC 등 미완성 미니게임 연습 — `01` §연습
- `debugRouteAllToOiia` — `04_메인메뉴.md` (보류)

---

## 17. 알려진 문제점

| 문제 | 심각도 | 설명 |
|------|--------|------|
| `debugRouteAllToOiia` 활성 | 중 | `04_메인메뉴.md` — 개발 중 OIIA 강제 라우팅 |
| `buzzClip`·`sessionEndClip` 없음 | - | **의도적 미연결**. 버그 아님 (§15-B) |
| 패드 미연결 (해당 번호) | 중 | 입력 무시. 부스에서는 패드 연결·자리 맞춤으로 해결 |
| RBC 등 다른 게임 | - | 별도 문서 참고 |

---

## 18. AI 참고 섹션

### 절대 수정하면 안 되는 부분

- OIIA 입력 매핑: **O=Trigger, I=Button2, A=Button4** (§6, `03_Booth_USB_Controller_매핑.md`)
- OIIA 화면: **1P~4P 왼쪽부터** (§15 C-12)
- 패턴 `oiiaiooiiiai` — 변경 시 `patternStepSfx`·UI 동기화 필수
- `OiiaHpLossRules` — `hpLowScoreThreshold`는 **`Balance.cs` Inspector·씬 값**(현재 **100,000**)을 `FillHpLost`에 전달. 하위 50% 구조 유지

### 수정 시 주의할 부분

- partial 파일 분리 구조 유지 (`OiiaMinigameModule.*.cs`)
- `Begin()` 재호출 경로 (연습→본게임) — 씬 리로드 없음
- `_completing` / `_running` 플래그 — 이중 Complete 방지
- `FlushUi`와 `TickGameplay` 순서
- Inspector 자동 수집 (`CollectSlotPanels`, `DefaultSlotPanelsContainerName`)
- 티어 시간 상수 변경 시 BGM·Blur·고양이·게이지 모두 연동됨

### 재사용 가능한 코드

- `OiiaSlotPanelBindings` + 자식 이름 규칙 패턴
- `MinigameExitSequence` + `ScreenFader`
- `OiiaHpLossRules` 패턴 (다른 미니게임 HP 규칙 참고)
- `OperatorInputService` (운영자 Enter)
- partial class 역할 분리 패턴

### 신규 기능 추가 시 진입 지점

| 기능 | 수정 위치 |
|------|-----------|
| 새 입력 버튼 | `Gameplay.cs` `TickGameplay`, `MapPatternToPhysical` |
| 점수 규칙 변경 | `Constants.cs`, `OnCorrectInput` / `OnFail` |
| 새 UI 요소 | `Types.cs` SlotUiBindings, `OiiaSlotPanelBindings`, `Ui.cs` / `GuideUi.cs` |
| 정답 MP4 연출 | `VideoEffects.cs`, `OiiaVideoEffectController.cs`, `OnCorrectInput` |
| 연습 흐름 변경 | `PracticeFlow.cs` |
| Result 장식 | `OiiaResultMinigameFlavor.cs` |
| 메뉴 진입 | `GameFlowDirector.StartSelectedMinigame` |

### 디버깅 시 먼저 확인할 위치

1. `Joystick.all` 개수·연결 (Input Debugger) — 부스에서는 패드 자리 맞춤
2. `OiiaSceneBootstrap` — PartySession 존재 여부
3. `slotPanels` / `Panel_O.I.I.A._4Way` 바인딩
4. `_aliveMask` — 슬롯이 PLAYING인지
5. `_ctx.IsPractice` — 연습/본게임 모드
6. `mainRoundTimerCentralTop` 연결
7. `GameFlowDirector.debugRouteAllToOiia`

---

*관련 문서: `01_프로젝트_개요.md`, `03_Booth_USB_Controller_매핑.md`, `04_메인메뉴.md`*
