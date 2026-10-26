# 05_Pigeon (비둘기야 먹자)

> **문서 기준일**: 2026-10-26 — 하단 바·`Circle` X·쪽기 `goo`·쏟기 `LetsEat`·라운드 `Delicious` **1P Play 확인**. HP −1: 1인 없음 · 2인+ 하위 50%. 2~4P Play 미검증.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| 기획·연출 의도 | **확정** | 판정=커서. 비둘기=연출. **연습 라운드 없음** |
| C# (`IMinigameModule`) | **1P Play 확인** | 커서·쏟기·쪽기·HUD 슬롯·30초 바·Fade→Results. 2~4P 미검증 |
| 메뉴 카탈로그·씬 로드 | **Play 확인** | catalog `id` = `pigeon` · `pigeonSceneName` = `Minigame_Pigeon` |
| 점수 HUD | **1P Play 확인** | 비참가는 `Player1`…`Player4` 슬롯 전체 Off |
| 제한시간 | **1P Play 확인** | 30초. 빨간 경과 + 하얀 로드 + `0:00/0:30` + `Circle` X `-900`→`900` |
| 쪽기 사운드 | **1P Play 확인** | 등장마다 `goo0`…`goo4` 중 하나. `peckSfxSource` = `Audio_Sfx` |
| 쏟기 사운드 | **1P Play 확인** | 쏟기 시작마다 `LetsEat` 1회. 같은 `Audio_Sfx` |
| 라운드 랜덤 사운드 | **1P Play 확인** | `Delicious` 하나. 끝난 뒤 **4~8**초 랜덤. 같은 `Audio_Sfx` |
| HP −1 | **구현** | 1인 안 깎음. 2인+ 하위 50%. Results에서만 −1. 2~4P 미검증 |

### 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 화면 | **4인 공용 단일 화면**. Orthographic 1대. 좌표는 카메라 Size **540** · `PlayField` `(960, 540)` |
| 쪽기 | 화면 밖에서 날아오는 `Peck_P*` **아님**. `Cursor_P*` 자식 `Pigeon` |
| 판정 | **커서 ↔ 바닥 면 더미** 겹침. 비둘기는 판정 안 함 |
| 레디 | 씬 안 커서 흰색 레디 **없음**. 메인 메뉴 JOIN/READY만 |
| 연습 | **없음**. `IsPractice == false` 로 진입. OIIA·관짝춤과 다름 |
| `NoodlePosition` | 매 쏟기 화면 안 무작위. 면 부모 아님. 컵은 자식이라 같이 이동 |
| 영어 식별 | 씬·폴더 **Pigeon**. 표시명 **비둘기야 먹자** |
| 컵 역재생 | 컨트롤러 상태 `CupNoodleReverse` (Speed -1). 코드 `Play(..., 0)`. 컴포넌트 `Animator.speed = -1` **아님** |
| 쪽기 역재생 | 왼쪽 `PeckReverse`, 오른쪽 `PeckRightReverse` (각 클립, Speed -1). `Play(..., 0)`. 컴포넌트 speed −1 **아님** |
| 쪽기 좌우 | 커서 월드 X가 `playCamera.position.x`보다 크면 오른쪽 클립. 같거나 작으면 왼쪽. 런타임 스케일 반전 **아님** |
| 국물 | 쏟기당 **한 번**. 컵 정방향이 끝난 뒤 **첫 면**과 함께 `Play(Soup)`. 더미 5개마다 아님 |
| 제한시간 바 | `BG` 위 `TimerBuffer`(하양 로드) 위 `TimerFill`(빨간 경과). 둘 다 Image **Filled**, 왼쪽부터. 하양은 경과보다 앞서 멈췄다 점프 |
| 제한시간 글자 | `Timer` 자식 `Text`. `0:00/0:30`처럼 경과 초(내림)/`roundDuration`. 슬래시 앞뒤 공백 없음 |
| 재생 머리 | `Bar` 자식 `Circle`. `roundTimerPlayhead`. 게임 시간 비율로 Anchored X **`-900`→`900`**. Y는 씬 값 |
| HP −1 | 1인 없음. 2인+ 하위 50%. 미니게임 안에서는 하트 안 깎음 |

---

## 1. 한 줄 요약

유튜브 밈 ‘비둘기야 먹자’를 모티브로, 4인이 **각자 커서**로 바닥에 쏟아진 **라면 면 더미**를 비둘기 우산 손잡이 연출로 쪼아 최다 점수를 겨루는 아케이드 쟁탈전.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`Ctrl` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| 이동 | `StickUp/Down/Left/Right` (8방향, 대각 정규화) | `W` `A` `S` `D` | `cursorSpeed`(기본 **120**)로 로컬 이동. `playCamera` 화면 안으로 Clamp. 참가 슬롯만 |
| 쪼기 | `FaceA` (`button2`) | 키패드 `5` | Peck 정방향 후 판정, 역재생. 그 동안 이동·A 잠금 |
| 시작 | 메인 메뉴 운영자 Enter | — | 씬 로드와 동시에 본게임 |

---

## 3. 규칙

### 스폰·점수

- 면 더미 **프리팹**. 매 쏟기 시작 시 `NoodlePosition`을 `playCamera` 화면 안 무작위 **월드** 좌표로 옮김(컵도 자식이라 같이 이동). 이때 `pourSfxClip`을 `peckSfxSource`로 한 번 `PlayOneShot`(비면 생략). 정방향 끝 → **국물 `Soup` 1회**(연결돼 있으면) + 지터를 가까운 순으로 월드 `Instantiate` (`pileParent` 없으면 `NoodlePosition.parent`). 기존 면은 따라가지 않음. → 역재생 → `pourCooldown`.
- 면 더미 **1개 = 100점**.
- 커서가 면 더미와 **겹친 상태**에서 버튼 **1회당 최대 1개**.
- 연습용 랜덤 스폰 **없음**.

### 쪽기 연출

- `Pigeon`은 커서 자식. 평소 **비활성**. A 시점에 커서 월드 X가 화면 중앙보다 크면 `PeckRight`→`PeckRightReverse`, 아니면 `Peck`→`PeckReverse`. 정방향 **끝난 뒤** 커서↔면 검사. `peckSpeed` **1**이면 클립 속도. 정방향·역재생의 애니와 대기 시간을 같이 나눈다. 쪽기가 끝나면 Animator `speed`는 1.
- 판정은 A를 누른 프레임이 아님. 비둘기 그림은 판정 안 함.
- **허공**: 입 라면 끄고 역재생.
- **적중**: 최상단 더미 1개 `Destroy` + 100점 + **역재생 동안** 입 `Noodle` 활성.
- 발출 시작마다 `peckSfxClips` 중 비지 않은 칸을 하나 `PlayOneShot`. 칸이 전부 비면 생략. 적중 여부와 무관.
- 라운드 중 `randomSfxClip` 하나. 시작 후 4~8초(`randomSfxIntervalMinSeconds` / `Max`) 중 랜덤으로 쉬었다가 재생하고, 클립이 끝난 뒤 같은 방식으로 다시. `peckSfxSource`. 비면 생략. 쪽기·쏟기와 겹칠 수 있음.

### 흐름 (연습 없음)

프로젝트 기본(연습→본)의 **Pigeon 예외**.

- 메인 메뉴에서 참가자가 READY → 운영자 Enter → `PrepareRound(false)` → `Minigame_Pigeon` 로드.
- 씬 `Begin` 시점이 곧 본게임: 쏟기·쪽기. 점수는 `_score`와 `Score_P*` `{n}점`. 씬 안 Start 레디·`Begin` 재호출 없음.
- 제한시간 **30초**(`roundDuration`). 빨강 `roundTimerFill.fillAmount` = 경과/30. 하양 `roundTimerBuffer`는 그보다 앞선 로드 초/30. 글자 `roundTimerLabel`은 `0:00/0:30`에서 시작해 초마다 올라가고, 끝나면 `0:30/0:30`. `Circle` Anchored X는 같은 시간 비율로 **`-900`에서 `900`**. Y는 유지. 0초 → 쪽기 중단 · `FadeOverlay` Fade Out → `Results`.
- HP −1: **1인 안 깎음**. **2인 이상** 하위 50%(2→1, 3→1, 4→2). 컷 점수 없음. 미니게임 안이 아니라 Results `HpProcess`.

### 예외

1. 같은 더미 동시 쪽기: 먼저 판정. 같은 프레임이면 낮은 `playerIndex`. 나머지는 허공.
2. 겹친 더미: 가장 최근 스폰(최상단) 1개.
3. 동점 순위: 공동 순위 (`ResultRankingUtility`). HP 하위 50% 자를 때는 점수 오름차순 후 낮은 `playerIndex`가 아래.

---

## 4. 씬 (2026-09-09 YAML 기준)

- **파일**: `Assets/Scenes/Minigame_Pigeon.unity` (Build Settings 등록)
- **로드 이름**: `Minigame_Pigeon`
- **에셋**: `Assets/Pigeon/Sprites/` · 프리팹 `Assets/Pigeon/Prefabs/Noodle.prefab` · 애니 `Assets/Pigeon/Animations/`

```
Minigame_Pigeon
├── PigeonRoot
├── Audio_Sfx
├── Main Camera
├── PlayField
│   ├── Floor
│   ├── NoodlePosition         Transform만. 활성. 자식 CupNoodle
│   │   ├── CupNoodle          **비활성**. CupNoodle.controller
│   │   └── Soup               에디터. 첫 면과 함께 Play. **시작 비활성 권장**
│   └── Cursors
│       └── Cursor_P1 … P4     CircleCollider2D Is Trigger
│           └── Pigeon         4마리 비활성 + Pigeon.controller
│               └── Noodle     입 라면. **비활성**
├── Canvas
│   ├── PlayerSlot
│   │   └── Player1 … Player4     비참가 시 슬롯 전체 Off
│   │       └── Score_P1 … Score_P4   TMP `{n}점`
│   ├── YoutubeOverlay
│   │   ├── Timer
│   │   │   └── Text           TMP. `roundTimerLabel`
│   │   └── Bar
│   │       ├── Blur0
│   │       ├── Blur1
│   │       ├── BG             트랙
│   │       ├── TimerBuffer    하얀 로드. Image Filled
│   │       ├── TimerFill      빨간 경과. Image Filled
│   │       └── Circle         재생 머리. `roundTimerPlayhead`
│   └── FadeOverlay
└── EventSystem
```

| 오브젝트 | 역할 |
|----------|------|
| `PigeonRoot` | `PigeonSceneBootstrap` + `PigeonMinigameModule` (에디터 연결) |
| `Audio_Sfx` | `peckSfxSource`. Play On Awake 꺼짐. 소스 Clip 슬롯은 비어 있음. 재생은 `peckSfxClips` `PlayOneShot` |
| `Cursor_P*` | 조준·판정. CircleCollider2D. 틴트 1P 빨강 / 2P 파랑 / 3P 초록 / 4P 마젠타 |
| `NoodlePosition` | 쏟기 기준점. **매 사이클 화면 안 무작위 월드 좌표**. 면의 부모 아님 |
| `CupNoodle` | 쏟기 애니. **시작 비활성** |
| `Soup` | 국물. 첫 면과 함께 상태 `Soup` 1회. **시작 비활성**. 위치는 에디터 |
| `Pigeon/Noodle` | 적중 후 **PeckReverse 동안만**. **시작 비활성** |
| `Score_P*` | 슬롯 점수 TMP. 형식 `{점수}점` |
| `Player1`…`Player4` | 점수 HUD 슬롯. 비참가 시 **슬롯 전체** 비활성 |
| `TimerFill` | 빨간 경과. `roundTimerFill`. 시작 fill 0. `Bar` 자식. 씬 **1800×10**, Y **-410** |
| `Circle` | 재생 머리. `roundTimerPlayhead`. X는 `playheadStartX` **-900** → `playheadEndX` **900**. Y는 유지 |
| `Text` | `Timer` 자식 TMP. `roundTimerLabel`. `m:ss/m:ss` |
| `TimerBuffer` | 하얀 로드. `roundTimerBuffer`. `BG`와 `TimerFill` 사이. 씬 색 약 **0.887** 회색, Anchored Y **-410**, **1800×10**. 시작은 첫 덩어리 **3**초 |
| `FadeOverlay` | `Canvas` 자식. `ScreenFader.canvasGroup` 연결됨 |

`FadeOverlay`의 `ScreenFader.canvasGroup` 은 연결됨.

---

## 5. 코드

경로: `Assets/_Project/Scripts/Minigames/Pigeon/`

| 심볼 | 역할 |
|------|------|
| `PigeonMinigameModule` | `IMinigameModule` + `partial`. `BuiltInId` = `pigeon` |
| `PigeonSceneBootstrap` | `PartySession` → `Begin` / `Tick` |
| `TickCursorMove` | D-Pad 홀드 → `localPosition`. 대각 `normalized`. Peck 중 잠금 |
| `TickPour` / `SpawnNextPile` | 화면 안 무작위 `NoodlePosition` → 정방향 시작에 `PlayPourSfx` → 첫 면 때 `PlaySoupOnce` → 월드 스폰 → 역재생 |
| `PigeonMinigameModule.Peck.cs` | `TickPeck` · 좌 `Peck` / 우 `PeckRight` · `ResolvePeckHit` |
| `PigeonMinigameModule.Hud.cs` | `{n}점`. 비참가는 `scoreHudSlots`(Player1…4) Off |
| `PigeonMinigameModule.RandomSfx.cs` | 라운드 중 `randomSfxClip` 1개. 끝난 뒤 간격 랜덤 |
| `PigeonMinigameModule.Timer.cs` | 빨강 경과/30. 하양 로드 점프. 글자 `FormatRoundClock`. `Circle`은 `MovePlayheadAlongRound`. `roundDuration` **30** |
| `PigeonMinigameModule.ExitSequence.cs` | hold 0.35 + Fade Out 1 → `OnComplete` |
| `PigeonHpLossRules` | 1인 skip. 2인+ `count/2`명 `HpLostThisSession` |
| `ClampToCamera` | `playCamera` Orthographic 뷰를 커서 부모 로컬로 Clamp |

Inspector: `cursors[4]` · `cursorSpeed` **120** · `playCamera`. 스폰: `noodlePrefab` · `cupNoodle` · `cupAnimator` · `noodlePosition` · `pileParent` · `pilesPerPour` **5** · `spawnJitterRadius` **30** · `pileSpawnStagger` **0.15** · `pourCooldown` **4** · `cupPourDuration` **0** · `soup` · `soupAnimator` · `pourSfxClip` · `randomSfxClip` · `randomSfxIntervalMinSeconds` **4** · `randomSfxIntervalMaxSeconds` **8**. 쪽기: `peckPigeons[4]` · `peckAnimators[4]` · `mouthNoodles[4]` · `peckCursorColliders[4]` · `peckDuration` **0** · `peckSpeed` **1** · `scorePerPile` **100** · `peckSfxSource` · `peckSfxClips` **5칸**. HUD: `scoreLabels[4]` · `scoreHudSlots[4]`. 타이머: `roundDuration` **30** · `roundTimerFill` · `roundTimerLabel` · `roundTimerPlayhead` · `playheadStartX` **-900** · `playheadEndX` **900** · `roundTimerBuffer` · `bufferFirstChunkSeconds` **3** · `bufferChunkMinSeconds` **1.5** · `bufferChunkMaxSeconds` **4** · `bufferJumpIntervalMinSeconds` **0.6** · `bufferJumpIntervalMaxSeconds` **1.8** · `exitScreenFader` · `sessionEndHoldSeconds` **0.35** · `exitFadeOutSeconds` **1**.

**이번 슬라이스에 없음**: 2~4P 부스 검증.

---

## 6. 열린 결정

| 주제 | 상태 |
|------|------|
| 4P 커서 마젠타 | 씬 값 |
| 비둘기 진입 방향 | 에디터 `Peck` / `PeckRight` |

문서 갱신: **2026-10-26** (라운드 `Delicious` 1P Play 확인) · **2026-10-26** (라운드 랜덤 클립 1개) · **2026-10-26** (쪽기 `peckSpeed`) · **2026-10-26** (쏟기 `LetsEat` 1P Play 확인) · **2026-10-26** (쏟기 시작 `pourSfxClip` 1회) · **2026-10-26** (쪽기 `goo` 랜덤 1P Play 확인) · **2026-10-26** (쪽기 등장 사운드 5클립 랜덤) · **2026-10-26** (`Circle` X 1P Play 확인) · **2026-10-26** (`Circle` X `-900`→`900`) · **2026-10-26** (`Circle` X `-410`→`410`) · **2026-10-26** (`Circle`이 빨간 끝 추적) · **2026-10-26** (제한시간 글자 1P Play 확인) · **2026-10-26** (제한시간 글자 `m:ss/m:ss`) · **2026-10-26** (하단 바 하얀 로드 1P Play 확인) · **2026-10-26** (하단 바 하얀 로드 점프) · **2026-09-13** (HP −1 1인 skip·2인+ 하위 50%) · **2026-09-13** (타이머·HUD·Results 1P Play 확인) · **2026-09-13** (HUD 슬롯 단위 On/Off) · **2026-09-13** (제한시간 바·Results) · **2026-09-13** (1P Play 확인) · **2026-09-13** (점수 HUD) · **2026-09-12** (국물 Soup 1회) · **2026-09-12** (좌 Peck / 우 PeckRight) · **2026-09-10** (쪽기 Peck→판정→PeckReverse) · **2026-09-10** (NoodlePosition 화면 무작위·면 월드 스폰) · **2026-09-10** (중앙부터 차례 스폰) · **2026-09-10** (역재생 정규화 0) · **2026-09-09** (`CupNoodleReverse` Play) · **2026-09-09** (면 스폰) · **2026-09-09** (playCamera·id Play 확인) · **2026-09-09** (커서 이동 Play 확인) · **2026-09-09** (커서 이동) · **2026-09-09** (연습 없음) · **2026-09-07** (사용자 씬이 계약) · **2026-09-06** (초안 Hierarchy, 폐기)
