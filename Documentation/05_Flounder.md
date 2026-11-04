# 05_Flounder (물 만난 광어)

> **문서 기준일**: 2026-11-04 — 팀원 제작본을 `Fish-Merge`에 합침. 메인 진입 · 후원 연출 · 카운트다운 · 25초 연타 · Fade → Results **Play 확인**. HP −1: 1인 없음 · 2인+ 하위 50%.  
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

### 헷갈리기 쉬운 점

| 항목 | 현재 진실 |
|------|-----------|
| 시작 | 씬 로드 직후 **바로 진행 안 함**. 참가자 누군가 ↑/↓를 **한 번 눌러야** 시작 후원창이 뜬다 |
| 점수 연타 | ↑ ↓ **번갈아** 눌러야 +10. 같은 방향 반복은 캐릭터만 움직이고 점수·게이지 없음 |
| 키보드 | 1P 전용 F/J **삭제**. 개발 키보드는 공통 `Ctrl` 토글 W/S |
| 종료 연출 | 광어 자체 페이드 **삭제**. 공통 `MinigameExitSequence` + `Canvas/FadeOverlay` |
| `ResultPanel` | 팀원 버전의 씬 안 결과창. **비활성으로 남아 있고 사용 안 함**. 결과는 공통 `Results` 씬 |
| `debugSoloWithoutPartySession` | **0 유지**. 1이면 가짜 4인으로 시작하고 Results로 안 넘어감. 에디터 메뉴 「씬 구성」을 다시 돌리면 1로 바뀜 |
| 캐릭터 | 4슬롯 모두 `PlayerFlop_Frames` 프레임 + 슬롯별 틴트. 2P·3P 전용 GIF(`Player2P`/`Player3P`)는 씬에서 **안 씀** |
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

입력은 `FlounderInput` → `BoothUsbSlotInput.WasPathPressed` / `IsPathHeld` (다른 게임과 동일 경로). 슬롯 번호는 `Input_1P`…`Input_4P`의 `playerSlotIndex` **0~3**.

---

## 3. 규칙

### 흐름 (연습 없음)

1. 메인 메뉴 READY → 운영자 Enter → `PrepareRound(false)` → `Minigame_Flounder` 로드.
2. **StartDonation**: 참가 슬롯 중 누군가 ↑/↓ 입력 → `startDonationDelayAfterInputSeconds`(**0.65**초) 뒤 중앙 시작 후원창(`startDonationAmount` **50,000**원). 창 표시 `centerPopupSeconds` **7**초, 페이드 `fadeSeconds` **0.3**. 음원: intro(`BGM_[cut_3sec]`) → TTS(`광어_tts`).
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
│   │   │   ├── GameDonationPanel   게임 중 별풍선 알림 (시작 비활성)
│   │   │   ├── Countdown           (시작 비활성)
│   │   │   └── StartDonationPanel  중앙 시작 후원창
│   │   ├── ResultPanel        **미사용**. 비활성
│   │   └── FadeOverlay        ScreenFader (2026-11-04 추가)
│   ├── Character_1P … 4P      GifVisual (SpriteRenderer)
│   └── Input_1P … Input_4P    FlounderInput
├── EventSystem
├── Header · Footer · Feed     팀원 버전 잔여 UI. **비활성**
```

| 오브젝트 | 역할 |
|----------|------|
| `Flounder` | `FlounderSceneBootstrap` + `FlounderMinigameModule` + `FlounderGameManager`(+`StartDonationBgm` partial) + `FlounderDonationManager` |
| `Slot_*P` | `FlounderPlayerSlotView` + `FlounderGauge`. 비참가 슬롯은 `SetActiveSlot(false)` |
| `DonationUI` | `FlounderDonationUI` (partial: `StartPopup` · `GameplayPopup` · `SlotPlacement` · `Sprites` · `Typography` · `SceneLayout` · `Setup` · `EditorPreview`) |
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
| `FlounderGameManager.StartDonationBgm` | intro → TTS 순서 재생, 카운트다운 BGM |
| `FlounderInput` | 슬롯별 ↑/↓ (`BoothUsbSlotInput`) |
| `FlounderGauge` | 게이지 증가·감소·`IsFull`·`%` 표시 |
| `FlounderDonationManager` | 시작 금액 · 게임 중 후원 가중치 추첨 · 닉네임 |
| `FlounderDonationUI` (partial) | 중앙 시작 후원창 · 슬롯 위 별풍선 알림 큐 · 글꼴·배치 |
| `FlounderCountdown` | 5→START! |
| `FlounderLiveUI` | `LIVE  12,000` 시청자 수 연출 |
| `FlounderDance` · `FlounderPlayerSlotView` · `FlounderPlayer` | 캐릭터 애니 · 슬롯 UI · 플레이어 점수 데이터 |
| `FlounderHpLossRules` | 1인 skip. 2인+ `count/2`명 `HpLostThisSession` |
| `FlounderSlotGifAutoSetup` | 2P·3P 프레임을 `Resources`에서 불러 붙이는 컴포넌트. **씬에 없음(미사용)** |
| `FlounderResultUI` | 씬 안 결과창. **미사용** |

에디터 메뉴 (`Assets/_Project/Scripts/Editor/Flounder*.cs`, 메뉴 `Mini Party/물 만난 광어/…`): 씬 생성·구성, 후원 UI, 게이지 Hierarchy, 슬롯 캐릭터, 슬롯 Score. `FlounderDonationUIScenePreview`는 씬을 열 때 자동으로 후원창 미리보기를 갱신.

Inspector (씬 저장값): `roundDurationSeconds` **25** · `scorePerMash` **10** · `startDonationDelayAfterInputSeconds` **0.65** · `gifCharacterOnlyOnFirstSlot` **0** · `startDonationBgmVolume` **1** · `startDonationAmount` **50000** · `gainPerMash` **0.01** · `decayPerSecond` **0.005** · `countdownStart` **5** · `stepSeconds` **1** · `startDisplaySeconds` **0.35** · `centerPopupSeconds` **7** · `fadeSeconds` **0.3** · `gameplayAlertSeconds` **2.5** · `baseViewerCount` **12000** · `exitScreenFader` · `sessionEndHoldSeconds` **3** · `exitFadeOutSeconds` **0.85**.

런타임에 `Find` / `AddComponent` / `AssetDatabase.LoadAssetAtPath`(에디터 한정)로 보정하는 코드가 남아 있다 (팀원 원본 유지, 정리 여부 미결정).

---

## 6. 에셋

| 종류 | 경로 | 비고 |
|------|------|------|
| 스프라이트 | `Assets/Minigames/Flounder/Sprites/` | 배경 · 게이지 · 별풍선 6종 · 시작 후원창 |
| 캐릭터 프레임 | `Assets/Minigames/Flounder/Characters/PlayerFlop_Frames/` | 4슬롯 공용 |
| 글꼴 | `Assets/Minigames/Flounder/Fonts/NanumGothic*` · `Assets/Fonts/NanumBarunpenB*` · `Assets/Fonts/온글잎 박다현체 SDF` | |
| 음원 | `Assets/Audio/` | intro `BGM_[cut_3sec]` · TTS `광어_tts` · 카운트다운 `BGM_[cut_30sec]`. **다른 게임과 달리 게임 폴더 밖** |
| 런타임 로드 | `Assets/Resources/Flounder/` | 코드상 `FlounderSlotGifAutoSetup` 전용. 현재 미사용 |

---

## 7. 열린 결정

| 주제 | 상태 |
|------|------|
| 에셋 폴더 정리 (게임 폴더 밖 음원 · 미사용 파일) | 미결정 |
| 런타임 자동 세팅 코드 정리 | 보류 |
| 카탈로그 아이콘 | 미지정 |
| `ResultPanel` · `Header`/`Footer`/`Feed` 잔여 오브젝트 삭제 | 미결정 |

문서 갱신: **2026-11-04** (최초 작성 · 팀원 제작본 이식 · Play 확인)
