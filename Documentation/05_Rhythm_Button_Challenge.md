# 05_Rhythm_Button_Challenge

> **문서 기준일**: 2026-09-13 — 목표 스펙. C# 스텁. 씬 보드·장식·`Phase` 배치됨. **플레이어 슬롯은 사용자 추가 중**.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| 목표 기획 | **확정** | 무페이즈 5스테이지 · 연습 없음 · 성공/실패 · 4×2 · 슬롯 박별 테두리 |
| C# (`IMinigameModule`) | **스텁** | 구 페이즈·정확도·박마다 클립·Find UI **삭제**. Begin 후 ESC→Result. 클록·보드·입력 미구현 |
| 메뉴 카탈로그·씬 로드 | **진입 OK** (2026-07-22) | `id` = `rhythm_button_challenge` · 씬 `Minigame_RhythmButtonChallenge` · `PrepareRound(false)` |
| 오디오 | **후속** | 에셋 미준비. 목표 클록은 오디오 없이 진행 |
| 화면 장식 | **에디터** | 로직 없음. AI 비범위 |
| 씬 Hierarchy | **배치 완료** | 보드·장식·Phase·PlayerSlot. Module은 사용자가 `RBC_Root`에 붙임 |

### 헷갈리기 쉬운 점

| 항목 | 진실 |
|------|------|
| 이 문서 | **목표 스펙**. 구 5단 판정·페이즈2·SPEED UP은 **폐기** |
| 레포 플레이 | Begin 후 **정지**. ESC로 Result (점수 0) |
| 보드 | 목표 **4열×2행**. 구씬은 가로 1줄 8칸 |
| 판정 그림 | 보드 칸 위 이펙트 **없음**. 슬롯 테두리만 |
| 연습 | **없음** (Pigeon과 같은 기획 예외). 미완성이 아님 |
| 페이즈 | **게임 페이즈 없음**. 오브젝트 `Phase`는 스테이지 HUD `n/5` |
| HUD | `Phase` TMP `{n}/5` |
| 슬롯 테두리 | 슬롯당 Outline **1개**. 그 박 결과만 녹/빨. **8박 이력 없음** |

---

## 1. 한 줄 요약

4인이 공용 **4×2 보드**에 공개된 8박 버튼 패턴을 외운 뒤, 같은 순서를 박 안에 눌러 **성공/실패**로 점수를 겨루는 리듬 게임.

---

## 2. 입력

Input 구간·해당 박 윈도우가 열린 참가 슬롯만 읽는다. Reveal·Intro에서는 무시.

| RBC 버튼 | `BoothUsbGamepadLayout` | 개발 키보드(`Ctrl` 토글 1P) |
|----------|-------------------------|------------------------------|
| A | `FaceA` | 키패드 `5` |
| B | `FaceB` | `B` |
| X | Trigger (`PrimaryTriggerWasPressed`) | `V` |
| Y | `FaceY` | (매핑표 `03`) |
| L / R | `ShoulderL` / `ShoulderR` | `Q` / `E` |
| 방향 | `StickUp/Down/Left/Right` | `W` `A` `S` `D` |

운영자 **ESC**: 세션 종료 → Result. 연습 전환 Enter **없음**.

---

## 3. 규칙

### 흐름 (연습 없음)

프로젝트 기본(연습→본)의 **RBC 예외**. Pigeon과 같음.

```
메인 JOIN/READY/Enter → PrepareRound(false) → Minigame_RhythmButtonChallenge
  → Intro 8박 (입력 없음)
  → Stage 1 Reveal → Stage 1 Input
  → … Stage 5 Reveal → Stage 5 Input
  → CompleteSession → Results
```

- 페이즈 · SPEED UP · pitch 배속 **없음**.
- HUD TMP: `{stageIndex}/5` (1부터). Intro 중 표기는 미정이면 `1/5` 전 또는 Intro 전용 공란 — **구현 슬라이스에서 HUD를 붙일 때 씬 값 우선**. 목표는 스테이지 진행 중 `n/5`.

### 보드

```
[0] [1] [2] [3]     ← 1~4박 (위줄 왼쪽→오른쪽)
[4] [5] [6] [7]     ← 5~8박 (아래줄 왼쪽→오른쪽)
```

칸 안: 각 `Square`의 `Icon` 아래 A/B/X/Y/LB/RB/방향 자식. **정답 버튼만 SetActive(true)**, 나머지 Off. 스프라이트 교체 아님.

| 구간 | ButtonIcon | 하이라이트 |
|------|------------|------------|
| Intro | 비표시 | 현재 박 |
| Reveal | 0~현재박 순차 공개 | 현재 박 |
| Input | 8칸 전부 | 현재 박 |

### 패턴 (레거시)

세션 시드 랜덤. 스테이지 시드: `_sessionSeed * 397 ^ stageIndex` (페이즈 항 없음).

| Stage | 풀 |
|-------|----|
| 1 | A, B |
| 2 | A, B, X, Y |
| 3 | + L, R |
| 4, 5 | + Up, Down, Left, Right |

**연속 3박 같은 버튼 금지** (최대 32회 재시도).

### 판정

한 박 = 비트클록 구간. **성공 / 실패**만. Perfect·Fast·Slow·Miss·Wrong·ms 창 없음.

- **성공**: 그 박이 끝나기 전에 **정답 버튼**.
- **실패**: **오답**, 또는 **박 종료까지 입력 없음**.
- 성공 후 같은 박 추가 입력: 레거시 **Extra −2000**. 테두리는 이미 성공(녹)이면 유지할지 덮을지는 구현 시 레거시 Wrong 덮어쓰기와 맞출지 질문. **기본 합의는 점수 Extra만 레거시**.

### 점수 · HP (레거시 숫자)

| 항목 | 값 |
|------|----|
| 성공 | +10,000 |
| 실패 | −10,000 |
| Extra | −2,000 |
| 8박 전부 성공 | +30,000 |
| 점수 하한 | 0 |

이론 최대 (5 Input × (80,000+30,000)) = **550,000**.

HP (`RhythmButtonChallengeHpLossRules`, Result에서만 −1):

1. `FinalScore < 500000`
2. 참가 2명 이상 **하위 50%** (OR)

500000 정확은 저점수 규칙 아님.

### 판정 UI

보드 칸의 Judgment Image · 성공 이펙트 **없음**.

`PlayerSlot` / `1P`~`4P`: Unity `Outline` **슬롯당 1개**.

- 성공 → 테두리 **녹색**
- 실패 → 테두리 **빨간**

**그 박의 최신 결과만** 보인다. 이전 박 이력은 남기지 않음. 비참가 슬롯은 숨김(Pigeon과 같은 취지).

### 비트클록

경과 `Time.unscaledTimeAsDouble` → 박 인덱스. **박마다 `AudioSource.Stop`/`Play`로 시간을 만들지 않음.**

오디오 전에는 비트 길이 `[SerializeField]`. 오디오는 이후 클록에 구독.

### 화면 장식

확성기·번개 등 씬 장식은 사용자 에디터. 규칙·코드 계약 없음.

---

## 4. 씬

- **파일**: `Assets/Scenes/Minigame_RhythmButtonChallenge.unity` (Build 등록)
- **로드 이름**: `Minigame_RhythmButtonChallenge`
- **에셋(현)**: `Assets/Minigames/RhythmButtonChallenge/` (씬이 쓰는 쪽). `Assets/RhythmButtonChallenge/` 는 중복 복사 — 정리 후속.

**목표 Hierarchy (이름은 구현 슬라이스·에디터에서 확정, 코드는 SerializeField)**

```
Minigame_RhythmButtonChallenge
├── Main Camera
├── EventSystem
├── RBC_Root                 사용자가 Module + Bootstrap 부착 예정
├── MusicSource
└── Canvas  1920×1080
    ├── Background
    ├── Decoration           Speaker · Electric · ExclamationMark · Bulb
    ├── Cells                Square_1 … Square_8
    ├── Phase                TMP `n/5`
    ├── PlayerSlot           1P … 4P (각 Outline 1 + P*_Score)
    └── FadeOverlay
```

`1P`~`4P`는 왼쪽부터. 각 자식 `P1_Score` … `P4_Score`. Module·Bootstrap은 `RBC_Root` 같은 오브젝트. Find 없음.

`RhythmButtonChallengeSceneBootstrap`: `PartySession` 없으면 `Begin` 안 함. 메뉴 경유 진입.

---

## 5. 코드

경로: `Assets/_Project/Scripts/Minigames/RhythmButtonChallenge/`

| 심볼 | 역할 |
|------|------|
| `RhythmButtonChallengeMinigameModule` | `BuiltInId`. Begin/Tick 스텁. ESC → Result |
| `.Pattern.cs` | 스테이지 풀 · 3연속 금지. 시드에 페이즈 항 없음. 아직 Begin에서 호출 안 함 |
| `.Input.cs` | 10키 `ReadAnyGameplayButtonPressed`. Tick에서 아직 안 읽음 |
| `.ExitSequence.cs` | `exitScreenFader` Inspector. Find 없음 |
| `RhythmButtonChallengeSceneBootstrap` | `PartySession` → `Begin`/`Tick`. `FindObjectOfType` 없음 |
| `RhythmButtonChallengeHpLossRules` | 50만 + 하위 50% |
| `RhythmButtonChallengeResultMinigameFlavor` | ID 매칭만 |
| `*BoardCellBindings` / `*ScorePanelBindings` | Inspector 필드만. 이름 AutoWire 없음 |
| `GameFlowDirector` | id → 씬, `practice = false` |

**삭제됨**: `.AudioFlow.cs` · `.Gameplay.cs` · `.Ui.cs` (박마다 클립, 5단 판정, `GameObject.Find`)

유지 계약: `IMinigameModule` · `MinigameSessionReport` · `BoothUsbGamepadLayout` 10키 · 씬 이름.

**없음**: 비트클록 C# · 칸 아이콘 On/Off 구동 · 슬롯 Outline 색 · 오디오.

---

## 6. 열린 결정

| 주제 | 상태 |
|------|------|
| 오디오 클립 나누기 | **후속**. 에셋 미준비 |
| Intro 중 `n/5` 표시 | HUD 슬라이스에서 씬 값 |
| Extra 시 테두리 | Outline 1개(최신). Extra −2000일 때 색 덮기 여부 |
| 화면 장식 | 에디터. 문서화 안 함 |
| 비트 길이 기본값 | C# 클록 슬라이스에서 질문 |

문서 갱신: **2026-09-13** (구 로직 삭제·스텁) · **2026-09-13** (목표 스펙 전면 재작성)
