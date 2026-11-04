# 05_Flounder (물 만난 광어)

> **문서 기준일**: 2026-11-04 — 팀원 제작본을 `Fish-Merge`에 합침. 메인 진입 · 후원 연출 · 카운트다운 · 25초 연타 · Fade → Results **Play 확인**. HP −1: 1인 없음 · 2인+ 하위 50%. 같은 날 파일·코드 정리(에디터 도구·자동 세팅 코드 삭제, 에셋 폴더 표준화) — **정리 후 Play 확인**. 미사용 에셋 파일은 디스크에 보관, **코드·씬 참조만 제거**.  
> 씬·프리팹 조립은 에디터 작업(채팅 Step-by-Step). 본 문서에는 에디터 클릭 절차를 두지 않는다.

---

## 0. 현재 상태 스냅샷

| 영역 | 상태 | 비고 |
|------|------|------|
| 출처 | **팀원 제작** | 2026-09-18 분기본에서 개발 → 2026-11-04 이식. 공통 시스템은 현재 버전 유지 |
| C# (`IMinigameModule`) | **Play 확인** | 후원 연출 → 카운트다운 → 연타 → `MinigameExitSequence` → Results |
| 메뉴 카탈로그·씬 로드 | **Play 확인** | catalog `id` = `flounder` · `flounderSceneName` = `Minigame_Flounder`. 아이콘 미지정 |
| 제한시간 | **Play 확인** | 25초(`roundDurationSeconds`). `Timer` TMP 남은 초 `0.0` 형식 |
| 게이지·후원 보너스 | **Play 확인** | 게이지 100% → 가상 후원 1회 → 보너스 점수 |
| HP −1 | **Play 확인** | 1인 안 깎음. 2인+ 하위 50%. Results에서만 −1 |
| 파일·코드 정리 | **Play 확인** | 에디터 메뉴 7개 · 미사용 클래스 · 레이아웃 보존 장치 · `Find`/`AddComponent`/`AssetDatabase`/`Resources.Load` 보정 삭제. 미사용 에셋은 **파일 유지**. 오프라인 컴파일·GUID 검사 통과 |

### 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 시작 | 씬 로드 직후 **바로 진행 안 함**. 참가자 누군가 ↑/↓를 **한 번 눌러야** 시작 후원창이 뜬다 |
| 점수 연타 | ↑ ↓ **번갈아** 눌러야 +10. 같은 방향 반복은 캐릭터만 움직이고 점수·게이지 없음 |
| 키보드 | 1P 전용 F/J **삭제**. 개발 키보드는 공통 `Ctrl` 토글 W/S |
| 종료 연출 | 광어 자체 페이드 **삭제**. 공통 `MinigameExitSequence` + `Canvas/FadeOverlay` |
| 결과 화면 | 씬 안 결과창 **없음**(삭제). 결과는 공통 `Results` 씬 |
| `debugSoloWithoutPartySession` | **0 유지**. 1이면 가짜 4인으로 시작하고 Results로 안 넘어감. 씬 단독 테스트 때만 켠다 |
| 캐릭터 | 4슬롯 모두 `PlayerFlop_Frames` 프레임. 슬롯별 틴트(`gifCharacterTint`)는 현재 전부 흰색. 2P·3P 전용 GIF·프레임은 씬에서 **안 씀** |
| 씬이 정답 | 배치·글꼴·스프라이트·클립은 **씬 Inspector 값**. 코드가 경로로 에셋을 찾거나 레이아웃을 덮어쓰지 않는다 (2026-11-04 정리) |
| 시작 후원창 | 금액은 이미지(`StartDonation_100000`)에 그려져 있다. 코드에 금액 값 없음 |
| 영어 식별 | 씬·폴더·id **Flounder** / `flounder`. 표시명 **물 만난 광어** |

---

## 1. 한 줄 요약

인터넷 방송 콘셉트로, 4인이 스틱 ↑/↓를 **번갈아 연타**해 광어 캐릭터를 파닥이게 하고, 게이지를 채울 때마다 **가상 별풍선 후원**으로 보너스 점수를 받아 25초 동안 최고 점수를 겨룬다.

---

## 2. 입력

| 조작 | `BoothUsbGamepadLayout` | 개발 키보드(`Ctrl` 토글 1P) | 효과 |
|------|-------------------------|---------------------------|------|
| 위 파닥 | `StickUp` | `W` | 캐릭터 위 동작. 직전과 방향이 다르면 +10점·게이지 +1% |
| 아래 파닥 | `StickDown` | `S` | 캐릭터 아래 동작. 직전과 방향이 다르면 +10점·게이지 +1% |
| 시작 | 메인 메뉴 운영자 Enter | — | 씬 로드. 씬 안에서는 참가자 ↑/↓ 1회로 시작 후원창 |
| 운영자 점수 | 키보드 `1`~`4` / 키패드 `1`~`4` | — | Playing 중 참가 슬롯에 연타 1회분(`scorePerMash`, 기본 10). 게이지·후원은 안 오름. 디버그 ON이면 키패드 4 제외 |

입력은 `FlounderInput` → `BoothUsbSlotInput.WasPathPressed` / `IsPathHeld` (다른 게임과 동일 경로). 슬롯 번호는 `Input_1P`…`Input_4P`의 `playerSlotIndex` **0~3**.

---

## 3. 규칙

### 흐름 (연습 없음)

1. 메인 메뉴 READY → 운영자 Enter → `PrepareRound(false)` → `Minigame_Flounder` 로드.
2. **StartDonation**: 참가 슬롯 중 누군가 ↑/↓ 입력 → `startDonationDelayAfterInputSeconds`(**0.65**초) 뒤 중앙 시작 후원창(이미지 + 「모든 플레이어에게 후원!」 + `centerTaglineText`). 창 표시 `centerPopupSeconds` **7**초, 페이드 `fadeSeconds` **0.3**. 음원: intro(`BGM_[cut_3sec]`) → TTS(`광어_tts`).
3. **Countdown**: `5-4-3-2-1-START!` (`stepSeconds` **1** · START 표시 **0.35**초). 시작과 동시에 `countdownBgmClip`(`BGM_[cut_30sec]`, 카운트 5초 + 플레이 25초 분량) 재생.
4. **Playing**: 25초 연타. `Timer`에 남은 초.
5. **Ended**: 0초 → BGM 정지 · 캐릭터 정지 → `sessionEndHoldSeconds` **3**초 유지 → `exitFadeOutSeconds` **0.85**초 Fade Out → `Results`.

### 점수

- 방향이 바뀐 연타 1회 = `scorePerMash` **10**점. 같은 방향 연속은 0점.
- 최종 점수 = 연타 점수 + 후원 보너스 (`FlounderPlayer.FinalScore`).
- 슬롯 `Score` TMP에 실시간 표시.

### 게이지 · 가상 후원

- 방향이 바뀐 연타마다 게이지 `gainPerMash` **+1%**. 매초 `decayPerSecond` **0.5%** 감소.
- 100% → 게이지 0으로 리셋 + 가상 후원 1회 추첨 → 보너스 점수 가산 + 슬롯 위 별풍선 알림 `gameplayAlertSeconds` **2.5**초.
- 후원 금액은 가중치 추첨 (`FlounderDonationManager.gameplayTiers`):

| 금액(원) | 보너스 | 확률 |
|----------|--------|------|
| 1,000 | 100 | 35% |
| 3,000 | 300 | 25% |
| 5,000 | 500 | 20% |
| 10,000 | 1,000 | 12% |
| 30,000 | 3,000 | 6% |
| 50,000 | 5,000 | 2% |

- 후원자 닉네임은 `donorNicknames` 중 랜덤.

### LIVE 시청자 수 (연출만)

- 시작 `baseViewerCount` **12,000**. 연타 1회 +3, 후원 1회 +120. 점수와 무관.

### HP

- **1인 안 깎음**. **2인 이상** 하위 50%(2→1, 3→1, 4→2). 컷 점수 없음. 동점은 낮은 `playerIndex`가 아래. Results `HpProcess`에서만 −1 (`FlounderHpLossRules`, Pigeon과 동일 규칙).

---

## 4. 씬

- **파일**: `Assets/Scenes/Minigame_Flounder.unity` (Build Settings 등록)
- **로드 이름**: `Minigame_Flounder`

```
Minigame_Flounder
├── Flounder                   Bootstrap · Module · GameManager · DonationManager · AudioSource
│   ├── Main Camera
│   ├── Background             Background_Studio
│   ├── Canvas                 Screen Space Overlay (씬 유일 Canvas)
│   │   ├── LiveUI             LiveBadge · ViewerCount
│   │   ├── Timer              남은 초 TMP
│   │   ├── Slot_1P … Slot_4P  Label · GaugeTrack(GaugeInner·Fill) · Percent · Score
│   │   ├── DonationUI
│   │   │   ├── GameDonationPanel   게임 중 별풍선 알림 (시작 비활성). 자식 Message · Amount(비활성)
│   │   │   ├── Countdown           (시작 비활성)
│   │   │   └── StartDonationPanel  중앙 시작 후원창
│   │   └── FadeOverlay        ScreenFader (2026-11-04 추가)
│   ├── Character_1P … 4P      GifVisual (SpriteRenderer)
│   └── Input_1P … Input_4P    FlounderInput
└── EventSystem
```

2026-11-04 정리로 삭제: 씬 안 결과창 `ResultPanel`, 잔여 UI `Header` · `Footer` · `Feed`, `Flounder`의 `FlounderSceneLayoutPreserve`, `Background`의 `FlounderBackgroundFit`.

| 오브젝트 | 역할 |
|----------|------|
| `Flounder` | `FlounderSceneBootstrap` + `FlounderMinigameModule` + `FlounderGameManager`(+`StartDonationBgm` partial) + `FlounderDonationManager` |
| `Slot_*P` | `FlounderPlayerSlotView` + `FlounderGauge`. 비참가 슬롯은 `SetActiveSlot(false)`. `gifRenderer` = 같은 번호 `Character_*P/GifVisual` |
| `DonationUI` | `FlounderDonationUI` (partial: `StartPopup` · `GameplayPopup` · `SlotPlacement` · `Typography`) |
| `Character_*P` | `FlounderDance` — 프레임 애니 · 위/아래 파닥 · 종료 시 정지 |
| `FadeOverlay` | 검은 Image + CanvasGroup(alpha 0) + `ScreenFader`. `exitScreenFader` 연결 |

---

## 5. 코드

경로: `Assets/_Project/Scripts/Minigames/Flounder/`

| 심볼 | 역할 |
|------|------|
| `FlounderMinigameModule` | `IMinigameModule`. `BuiltInId` = `flounder`. 종료 시 `MinigameExitSequence` → 리포트(`FinalScore` + `FlounderHpLossRules`) → `OnComplete` |
| `FlounderSceneBootstrap` | `PartySession` → `Begin` / `Tick`. `debugSoloWithoutPartySession`은 단독 테스트용 |
| `FlounderGameManager` | 단계 머신 `FlounderGamePhase` (`Idle` · `StartDonation` · `Countdown` · `Playing` · `Ended`). 연타·게이지·후원·타이머 |
| `FlounderGameManager.StartDonationBgm` | intro → TTS 순서 재생, 카운트다운 BGM. `startDonationBgmSource`와 클립 3개는 Inspector 연결 필수 (없으면 경고) |
| `FlounderInput` | 슬롯별 ↑/↓ (`BoothUsbSlotInput`) |
| `FlounderGauge` | 게이지 증가·감소·`IsFull`·`%` 표시 |
| `FlounderDonationManager` | 게임 중 후원 가중치 추첨 · 닉네임 |
| `FlounderDonationEventData` | 알림 1건 (후원자 · 대상 슬롯 · 금액) |
| `FlounderDonationUI` (partial) | 중앙 시작 후원창 · 슬롯 위 별풍선 알림 큐 · 글꼴 적용. 알림 위치는 대상 슬롯 `Label` 위로 매번 계산 (`SlotPlacement`) |
| `FlounderDonationUiConstants` | 후원 금액 단계 `TierAmountWon` (= `gameplayStarBalloonSprites` 순서) |
| `FlounderCountdown` | 5→START! |
| `FlounderLiveUI` | `LIVE  12,000` 시청자 수 연출 |
| `FlounderDance` · `FlounderPlayerSlotView` · `FlounderPlayer` | 캐릭터 애니 · 슬롯 UI · 플레이어 점수 데이터 |
| `FlounderHpLossRules` | 1인 skip. 2인+ `count/2`명 `HpLostThisSession` |

에디터 메뉴 스크립트(`Editor/Flounder*.cs`)는 **없음** (2026-11-04 삭제). 씬은 Inspector에서 직접 고친다.

Inspector (씬 저장값): `roundDurationSeconds` **25** · `scorePerMash` **10** · `startDonationDelayAfterInputSeconds` **0.65** · `startDonationBgmVolume` **1** · `gainPerMash` **0.01** · `decayPerSecond` **0.005** · `countdownStart` **5** · `stepSeconds` **1** · `startDisplaySeconds` **0.35** · `centerPopupSeconds` **7** · `fadeSeconds` **0.3** · `gameplayAlertSeconds` **2.5** · `baseViewerCount` **12000** · `exitScreenFader` · `sessionEndHoldSeconds` **3** · `exitFadeOutSeconds` **0.85**.

---

## 6. 에셋

다른 미니게임과 같은 구조(`Assets/Minigames/Flounder/Sounds` · `Sprites`, 글꼴은 공용 `Assets/Fonts`). 미사용 파일은 지우지 않고 종류별 폴더에 함께 둔다.

| 종류 | 경로 | 씬에서 사용 | 미사용(보관) |
|------|------|-------------|--------------|
| 음원 | `Minigames/Flounder/Sounds/` | `BGM_[cut_3sec]`(intro) · `광어_tts` · `BGM_[cut_30sec]`(카운트다운) | 원본 `광어 (운동 리액션) BGM` · `BGM_[cut_1sec]` · `BGM_[cut_2sec]` · `FlounderStartDonationBgm`(+ 같은 파일 `_copy1` · `_copy2`) |
| 스프라이트 | `Minigames/Flounder/Sprites/` | `Background_Studio` · `GameDonationBanner` · `GaugeRoundRect` · `StarBalloon_*` 6종 · `StartDonation_100000` | `StartDonationPopup` |
| 캐릭터 | `Minigames/Flounder/Sprites/Characters/` | `PlayerFlop_Frames/` (4슬롯 공용) | 원본 GIF 3개 · `Player2P_Frames` · `Player3P_Frames` (+ 같은 파일 `_copy`) |
| 글꼴 | `Assets/Fonts/` | `NanumGothic SDF` · `NanumBarunpenB SDF` · `온글잎 박다현체 SDF` | — |

`Assets/Resources` 폴더는 없다 (2026-11-04 비움 — Resources는 안 써도 빌드에 포함되기 때문).

---

## 7. 열린 결정

| 주제 | 상태 |
|------|------|
| 미사용(보관) 에셋 삭제 | 보관 유지 (사용자 결정) |
| 카탈로그 아이콘 | 미지정 |

문서 갱신: **2026-11-04** (파일·코드 정리 · 에셋 폴더 표준화) · **2026-11-04** (최초 작성 · 팀원 제작본 이식 · Play 확인)
