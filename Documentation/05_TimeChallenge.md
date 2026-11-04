# 05_TimeChallenge (스톱워치)

> **문서 기준일**: 2026-11-05 — 팀원 스냅샷의 전용 씬·코드·에셋을 현재 프로젝트에 합침. 공통 입력·종료·HP 연결 및 파일·코드 정리. 메인 진입·3라운드·적중 영상/음향·공통 페이드·Results·복귀·HP·기존 5게임 회귀 **Play 확인**.
> 씬·Inspector 연결값이 기준이다. 본 문서는 규칙·구조·참조를 설명하며 에디터 클릭 절차를 두지 않는다.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| 출처 | 팀원 제작본 선택 이식 | 스냅샷 `C:\Users\PC_1M\Desktop\misibasi`. 기존 게임·폰트·팀원 공통 코드·일지는 복사하지 않음 |
| 모듈·메뉴 | **Play 확인** | `IMinigameModule`, 카탈로그 여섯 번째, 아이콘 지정 |
| 종료 | **Play 확인** | `MinigameExitSequence` → 리포트 → `Results` |
| HP | **Play 확인** | 1인 skip, 2인+ 하위 50% |
| 정리 | 적용됨 | 런타임 탐색·자동 생성·에디터 미리보기 제거. 미사용 음원 파일 보관 |

### 헷갈리기 쉬운 점

| 항목 | 현재 규약 |
|------|-----------|
| 이름 | 표시명 **스톱워치**. 기술명 `TimeChallenge`, ID `time_challenge`. 요청 초안의 `TimeChallange` 철자는 사용하지 않기로 확정 |
| 시작 | READY·운영자 Enter로 진입. 안내와 카운트다운 후 시계가 **자동 시작** |
| 정지 | Face A 한 번. 먼저 멈춘 사람의 기록도 참가자 전원 정지 또는 타임아웃까지 가려짐 |
| 시계 표시 | `03:00`은 **3초 00**, 분:초 형식이 아님 |
| 라운드 결과 | 게임 안 기록 공개·점수 상승·적중 영상은 중간 연출. 최종 결과창은 공통 `Results` |
| 키보드 | 원본 숫자 6~9·숫자패드 6~9 제거. 공통 Ctrl 토글 후 **1P 숫자패드 5** |
| 참가자 | `PartySession`의 PLAYING 슬롯만 참가. 참가자가 없으면 경고 후 시작하지 않음. 가짜 1P 폴백 제거 |
| 디버그 | `debugForce1PRound1Perfect = 0`. 단독 가짜 참가 모드는 없음 |
| 영상 | `VideoClip`·`VideoPlayer`·오디오·RenderTexture를 씬에 직접 연결. StreamingAssets URL 폴백 없음 |

---

## 1. 한 줄 요약

가려진 스톱워치를 목표 시간에 맞춰 멈추는 1~4인 게임. 세 라운드 동안 목표와의 오차 순위로 점수를 받고, 마지막 라운드와 정확 적중 보너스로 합산 점수를 겨룬다.

---

## 2. 입력

| 조작 | 경로 | 효과 |
|------|------|------|
| STOP | `BoothUsbGamepadLayout.FaceA` → `BoothUsbSlotInput.WasPathPressed` | Running 중 해당 참가자의 첫 입력으로 기록 확정 |
| 누름 연출 | 같은 경로의 `IsPathHeld` | 책상 버튼이 아래로 눌리고 Y축 축소·틴트 적용 |
| 개발 키보드 | 공통 Ctrl 토글 → 1P 숫자패드 5 | Face A와 같은 경로 |
| 운영자 시작 | MainMenu READY → Enter | `GameFlowDirector`가 새 씬 로드 |
| 조기 종료 | Escape 또는 `RequestEarlyExit` | 공통 퇴장 후 지금까지 합산한 점수 보고 |

`SlotGamepad.Get`은 공통 입력 함수에 장치를 전달할 때만 사용한다. 게임 코드가 장치 버튼을 직접 읽지 않는다.

---

## 3. 규칙

### 흐름 (연습 없음)

1. 메인에서 `PrepareRound(false)` → `Minigame_TimeChallenge` 로드.
2. Bootstrap이 `PartySession.Slots`로 컨텍스트를 구성하고 모듈 `Begin`·`Tick` 호출.
3. **Briefing**: 첫 라운드 안내 3.5초, 이후 목표 강조 1.5초. 첫 목표 3초, 2·3라운드 목표 1~7초 중 0.01초 단위 추첨. 직전 목표와 같게 추첨되면 다음 0.01초 값(최댓값에서는 최솟값)으로 이동.
4. **Countdown**: 3→2→1, 각 1초. 목표 강조 위치는 0.5초 동안 원래 위치로 돌아감.
5. **Running**: 참가자 시계 동시 시작, GO 0.45초. STOP 입력으로 기록 저장. 심장박동 효과음은 경과 초마다 재생. 전원 STOP 또는 목표+5초가 지나면 라운드 종료.
6. **Result**: 기록 공개, 점수판 0.85초 상승. 기본 4.5초 유지. 정확 적중자가 있으면 기록 공개 1초 후 해당 슬롯 영상 5초 재생하며 전체 유지시간은 최소 6초.
7. 1·2라운드 뒤 다음 라운드. 3라운드 뒤 **0.35초 유지 + 1초 공통 페이드** → 리포트 생성 → `OnComplete` → `Results`.

### 점수

- 기록과 목표를 각각 0.01초 표시 단위로 반올림한 뒤 차이의 절댓값으로 순위를 정한다.
- STOP한 참가자의 순위점수: **1000 / 700 / 500 / 300**.
- 같은 오차는 공동 순위·같은 점수이며 다음 순위를 건너뛴다(예: 1·1·3·4).
- **3라운드는 순위점수 ×2**. 미STOP은 라운드와 무관하게 **300점**.
- STOP했고 표시 오차가 0이면 해당 라운드 점수 **추가 ×2**. 마지막 라운드 정확 적중 1위는 4000점.
- 최종 점수는 세 라운드 합계. 조기 종료는 이미 종료된 라운드의 합산 점수만 보고한다.

### HP

- **1인 감점 없음**. 2인 이상 하위 절반만 1 HP: **2→1명, 3→1명, 4→2명**.
- 합산 저점수부터 선택하며 동점이면 낮은 `playerIndex`부터 감점 대상.
- `TimeChallengeHpLossRules`가 `HpLostThisSession`만 채운다. 실제 HP는 **Results의 HpProcess**에서만 감소.
- 원본의 3·4등 및 공동 3등 예외 규칙은 제거.

---

## 4. 씬

- 파일: `Assets/Scenes/Minigame_TimeChallenge.unity` (Build Settings 등록)
- 로드 이름: `Minigame_TimeChallenge`
- 메뉴: ID `time_challenge`, 제목 `스톱워치`, `Icon_TimeChallenge`

```text
Minigame_TimeChallenge
├── TC_Root                  Bootstrap · Module · AudioSource ×2
│   └── PerfectHitVideo_1P … 4P   World Canvas · VideoPlayer · AudioSource · RectMask2D
│       └── Video                RawImage · AspectRatioFitter
├── Main Camera
├── EventSystem
├── 배경 스프라이트
├── 시계 스프라이트 ×4       TimeChallengeLedClock · SpriteRenderer
│   └── ClockCover
├── 네온 그린 버튼 테이블 ×4 TimeChallengeTableButton · SpriteRenderer
├── Canvas                   Screen Space Camera
│   ├── 시계 TMP ×4 · 자리 1P~4P TMP
│   ├── TimeChallengeCanvas  RectTransform 그룹
│   │   ├── RoundLabel · TargetLabel
│   │   └── CountdownLabel · StatusLabel(비활성)
│   └── FadeOverlay          Image · CanvasGroup · ScreenFader · Canvas
├── ScoreBoard_1P … 4P        World Canvas · TimeChallengeScoreBoard
│   └── ScoreText             (1P에는 원본 안내 TMP도 존재)
└── RoundSting                World Canvas · CanvasGroup · TimeChallengeRoundSting
    ├── Flash
    └── Title
```

| 참조 | 씬 저장 연결 |
|------|--------------|
| `playerClocks[0..3]` | 화면 왼쪽부터 시계 1~4P. 각 `readout`·`cover`·`clockRenderer` 직접 연결 |
| `playerTables[0..3]` | 화면 왼쪽부터 책상 1~4P. 각 `tableRenderer` 직접 연결 |
| `playerScoreBoards[0..3]` | `ScoreBoard_1P`~`4P`; 각 `label`은 자식 `ScoreText` |
| `playerSeatLabels[0..3]` | 원본 Canvas의 1P~4P TMP |
| `roundSting` / `roundTitle` | `RoundSting` / `RoundSting/Title`. 원본 `subtitle`은 의도적으로 미연결 |
| `hudCamera` / `targetLabelCanvas` | `Main Camera` / `TargetLabel`에 저장한 Canvas |
| `stopwatchClickSource` / `heartbeatSource` | `TC_Root`의 별도 AudioSource 2개. Play On Awake 꺼짐 |
| `perfectHitVideos[0..3]` | `PerfectHitVideo_1P`~`4P`. 슬롯마다 전용 720×720 RenderTexture |
| `exitScreenFader` | `Canvas/FadeOverlay`. 검은 Image, CanvasGroup alpha 0, override sorting 100 |

영상 Canvas sorting 80, 목표 강조 Canvas 50, RoundSting 40, FadeOverlay 100. 영상 영역은 원본 시계·책상·가림 스프라이트의 합친 영역으로 저장했고, 재생 시 에셋이나 오브젝트를 검색·생성하지 않는다. 기존 배치값과 게임 중 연출은 유지한다.

---

## 5. 코드

경로: `Assets/_Project/Scripts/Minigames/TimeChallenge/`

| 심볼·파일 | 역할 |
|-----------|------|
| `TimeChallengeMinigameModule` | `IMinigameModule`; `BuiltInId = time_challenge`, 표시명 스톱워치 |
| `.Config / .Constants / .State` | Inspector 참조·조절값, 라운드 상수, 세션 상태 |
| `.Begin / .Tick / .Gameplay` | 참가 슬롯 초기화, 단계 갱신, 목표 추첨·타임아웃·점수 합산 |
| `.Input` | 공통 Face A 입력, STOP 기록, 기본 꺼진 강제 적중 디버그 |
| `.Hud` | 시계·목표·카운트다운·자리 표시·점수판 및 목표 강조 연출 |
| `.Tables` | 버튼 연출, 연결된 오디오 재생, 연결된 영상 정지 |
| `.SceneReferences` | 누락 Inspector 참조 경고. 자동 보정 없음 |
| `.ExitSequence` | 공통 퇴장 후 리포트 생성·OnComplete |
| `TimeChallengeSceneBootstrap` | PartySession → 모듈. 모듈 참조는 씬 연결 |
| `TimeChallengeRanking` | 오차 공동 순위, 라운드점수·정확 적중 배수 |
| `TimeChallengeHpLossRules` | 종료 시 하위 절반 HP 대상 판정 |
| `TimeChallengeLedClock` | 시간 표시·ClockCover·비참가 시계 딤 |
| `TimeChallengeTableButton` | 책상 누름 이동·축소·틴트 |
| `TimeChallengeScoreBoard` | 점수 카운트업·펀치 연출 |
| `TimeChallengeRoundSting` | 제목·플래시 연출. ExecuteAlways·OnValidate·에디터 미리보기 제거 |
| `TimeChallengePerfectVideo` | 연결된 VideoPlayer 재생·정지. 영상 준비 실패 시 경고. URL 폴백 없음 |

전용 에디터 메뉴 스크립트는 없다. 미사용 `TimeChallengeReadyPopup`·`TimeChallengeResultMinigameFlavor`는 통합본에서 제외했다. 자동 검색·생성·에디터 미리보기와 사용하지 않는 HUD 바인딩을 제거했다.

### Inspector 저장값

| 필드 | 코드 기본 | 씬 저장값 |
|------|-----------|-----------|
| `round1Seconds` | 3 | 3 |
| `randomTargetMinSeconds / randomTargetMaxSeconds` | 1 / 7 | 1 / 7 |
| `timeoutExtraSeconds` | 5 | 5 |
| `briefingSeconds / countdownStepSeconds` | 3.5 / 1 | 3.5 / 1 |
| `goVisibleSeconds / resultHoldSeconds` | 0.45 / 4.5 | 0.45 / 4.5 |
| `scoreCountDuration` | 0.85 | 0.85 |
| `perfectHitRevealSeconds / perfectHitVideoSeconds` | 1 / 5 | 1 / 5 |
| `sessionEndHoldSeconds / exitFadeOutSeconds` | 0.35 / 1 | 0.35 / 1 |
| `stopwatchClickVolume / heartbeatVolume / perfectHitVolume` | 1 / 2.2 / 2.4 | 1 / 2.2 / 2.4 |
| `debugForce1PRound1Perfect` | false | 0 |
| 책상 `pressDepth / pressedScaleY` | 22 / 0.86 | 22 / 0.86 |
| 점수판 `countDuration / punchScale` | 0.85 / 1.16 | 0.85 / 1.16 |

---

## 6. 에셋

| 종류 | 경로 | 사용 | 미사용(보관) |
|------|------|------|--------------|
| 음원 | `Assets/Minigames/TimeChallenge/Sounds/` | `StopwatchOn.wav`, `Heartbeat.wav` | `StopwatchOn.mp3` (코드·씬 참조 없음) |
| 스프라이트 | `Assets/Minigames/TimeChallenge/Sprites/` | 대칭형 실내 트레이닝 터프 스튜디오, 네온 그린 버튼 테이블, 꺼진 디지털 LED 스코어보드 시계, 스톱워치 가림 물음표 | 없음 |
| 캐릭터 폴더 | `Sprites/Characters/` | 전용 캐릭터 없음 | 빈 표준 폴더 |
| 영상 | `Assets/Minigames/TimeChallenge/Videos/` | `perfect_hit.mp4`, `PerfectHit_1P`~`4P.renderTexture` | 없음 |
| 메뉴 아이콘 | `Assets/Mainmenu, Result/Sprites/Icon_Minigame/` | `Icon_TimeChallenge.png` | 없음 |
| 글꼴 | 기존 `Assets/Fonts/` | `KERISKEDU_B SDF`, `Minecraft SDF`, `NanumMyeongjoExtraBold SDF` | 기존 파일 그대로 사용 |

음원·스프라이트·영상은 원본 .meta GUID를 유지했다. 영상은 StreamingAssets의 DefaultImporter에서 **VideoClipImporter**로 바꾸되 GUID는 유지했다. TimeChallenge 파일을 Assets/Resources나 StreamingAssets에 두지 않는다. 미사용 에셋 파일은 삭제하지 않았다.

---

## 7. 열린 결정

현재 미확정 기획 결정은 없다. Play 검증 상세는 `02_개발_진행_일지.md`에 기록한다.

문서 갱신: **2026-11-05** (팀원 제작본 통합·코드/파일 정리 · Play 확인)
