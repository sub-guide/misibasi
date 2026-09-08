# 05_Pigeon (비둘기야 먹자)

> **문서 기준일**: 2026-09-09 — 커서 이동·화면 Clamp **Play 확인**. 쪽기·스폰 미착수.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| 기획·연출 의도 | **확정** | 판정=커서. 비둘기=연출. **연습 라운드 없음** |
| C# (`IMinigameModule`) | **커서 이동 · Clamp Play 확인** | `PigeonMinigameModule` + `PigeonSceneBootstrap`. 쪽기·스폰·종료 없음 |
| 메뉴 카탈로그·씬 로드 | **Play 확인** | catalog `id` = `pigeon` · `pigeonSceneName` = `Minigame_Pigeon` |
| 점수 HUD | **후속** | 이번 배치 없음 |

### 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 화면 | **4인 공용 단일 화면**. Orthographic 1대. 좌표는 카메라 Size **540** · `PlayField` `(960, 540)` |
| 쪽기 | 화면 밖에서 날아오는 `Peck_P*` **아님**. `Cursor_P*` 자식 `Pigeon` |
| 판정 | **커서 ↔ 바닥 면 더미** 겹침. 비둘기는 판정 안 함 |
| 레디 | 씬 안 커서 흰색 레디 **없음**. 메인 메뉴 JOIN/READY만 |
| 연습 | **없음**. `IsPractice == false` 로 진입. OIIA·관짝춤과 다름 |
| 영어 식별 | 씬·폴더 **Pigeon**. 표시명 **비둘기야 먹자** |

---

## 1. 한 줄 요약

유튜브 밈 ‘비둘기야 먹자’를 모티브로, 4인이 **각자 커서**로 바닥에 쏟아진 **라면 면 더미**를 비둘기 우산 손잡이 연출로 쪼아 최다 점수를 겨루는 아케이드 쟁탈전.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`Ctrl` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| 이동 | `StickUp/Down/Left/Right` (8방향, 대각 정규화) | `W` `A` `S` `D` | `cursorSpeed`(기본 **120**)로 로컬 이동. `playCamera` 화면 안으로 Clamp. 참가 슬롯만 |
| 쪼기 | `FaceA` (`button2`) | 키패드 `5` | `Pigeon` 켜고 에디터 애니 재생. 애니 중 **재입력 잠금** |
| 시작 | 메인 메뉴 운영자 Enter | — | 씬 로드와 동시에 본게임 |

---

## 3. 규칙

### 스폰·점수

- 면 더미 **프리팹**. 본게임만: `CupNoodle` 애니 재생 후 지정 좌표에 다수. 제한시간 동안 주기적으로 반복.
- 면 더미 **1개 = 100점**.
- 커서가 면 더미와 **겹친 상태**에서 버튼 **1회당 최대 1개**.
- 연습용 랜덤 스폰 **없음**.

### 쪽기 연출

- `Pigeon`은 커서 자식. 평소 **비활성**. A 시 활성 + Animator(에디터 제작, 위치 키). 끝나면 다시 비활성.
- `Pigeon.png` 시트는 자동 프레임 재생이 아님. 포즈 스프라이트.
- **허공**: 입 라면 끄고 복귀.
- **적중**: 바닥 더미 1개 제거 + **돌아가는 애니 동안만** 자식 입 `Noodle`(`PickedNoodle`) 활성.
- 발출마다 **"구~"** (클립은 `Audio_Sfx`, 없어도 됨).

### 흐름 (연습 없음)

프로젝트 기본(연습→본)의 **Pigeon 예외**.

- 메인 메뉴에서 참가자가 READY → 운영자 Enter → `PrepareRound(false)` → `Minigame_Pigeon` 로드.
- 씬 `Begin` 시점이 곧 본게임: 타이머·쏟기·점수. 씬 안 Start 레디·`Begin` 재호출 없음.
- 제한시간 종료 → `FadeOverlay` Fade Out → `Results` (본게임 리포트, 연습 Result 축소 경로 안 탐).

### 예외

1. 같은 더미 동시 쪽기: 먼저 판정. 같은 프레임이면 낮은 `playerIndex`. 나머지는 허공.
2. 겹친 더미: 가장 최근 스폰(최상단) 1개.
3. 동점: 공동 순위 (`ResultRankingUtility`).

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
│   │   └── CupNoodle          **비활성**. CupNoodle.controller
│   └── Cursors
│       └── Cursor_P1 … P4     CircleCollider2D Is Trigger
│           └── Pigeon         4마리 비활성 + Pigeon.controller
│               └── Noodle     입 라면. **비활성**
├── Canvas
│   └── FadeOverlay
└── EventSystem
```

| 오브젝트 | 역할 |
|----------|------|
| `PigeonRoot` | `PigeonSceneBootstrap` + `PigeonMinigameModule` (에디터 연결) |
| `Audio_Sfx` | `AudioSource`. Play On Awake 꺼짐. 클립 미연결 |
| `Cursor_P*` | 조준·판정. CircleCollider2D. 틴트 1P 빨강 / 2P 파랑 / 3P 초록 / 4P 마젠타 |
| `NoodlePosition` | 쏟기 위치 부모. Sprite 없음. 활성(자식 컵은 꺼 둠) |
| `CupNoodle` | 쏟기 애니. **시작 비활성** |
| `Pigeon/Noodle` | 적중 복귀 때만. **시작 비활성** |
| `FadeOverlay` | `Canvas` 자식. `ScreenFader.canvasGroup` 연결됨 |

점수 TMP는 **아직 없음**(후속). `FadeOverlay`의 `ScreenFader.canvasGroup` 은 연결됨.

---

## 5. 코드

경로: `Assets/_Project/Scripts/Minigames/Pigeon/`

| 심볼 | 역할 |
|------|------|
| `PigeonMinigameModule` | `IMinigameModule` + `partial`. `BuiltInId` = `pigeon` |
| `PigeonSceneBootstrap` | `PartySession` → `Begin` / `Tick` |
| `TickCursorMove` | D-Pad 홀드 → `localPosition`. 대각 `normalized` |
| `ClampToCamera` | `playCamera` Orthographic 뷰를 커서 부모 로컬로 Clamp |

Inspector: `cursors[4]` · `cursorSpeed` 기본·씬 **120** · `playCamera` = `Main Camera` (**Play 확인**).

**이번 슬라이스에 없음**: 쪽기, 면 스폰, 타이머, Result 종료.

---

## 6. 열린 결정

| 주제 | 상태 |
|------|------|
| HP −1 규칙 | 미정 |
| 본게임 제한시간 · 쏟기 주기 · 한 번에 깔 면 개수 | 미정 |
| 본게임 스폰 좌표 | 미정 (마커 여러 개 vs `NoodlePosition` 1점+오프셋) |
| 4P 커서 마젠타 | 씬 값 |
| 비둘기 좌측 오프셋만 | 에디터 애니 |

문서 갱신: **2026-09-09** (playCamera·id Play 확인) · **2026-09-09** (커서 이동 Play 확인) · **2026-09-09** (커서 이동) · **2026-09-09** (연습 없음) · **2026-09-07** (사용자 씬이 계약) · **2026-09-06** (초안 Hierarchy, 폐기)
