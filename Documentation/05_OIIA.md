# 05_OIIA

> 문서 기준일: 코드·씬 파일 직접 분석 (추측 없음). **에디터 조작 가이드·플레이 검증 체크리스트는 본 문서에 두지 않는다** (`Project_Master_Context.md` §2·§3). 검증 타임라인은 `02_개발_진행_일지.md`.  
> **갱신**: 2026-10-26 — 현재 규칙만 남김. 씬 저장값은 T1 **6.5**초 · BGM 클립 **27**초 지점. 본게임 시간 필드는 씬에 아직 없어 코드 **26**초.

---

## 0. 현재 상태

> **상태**: T1 빌드업 후 T2 연타 **Play 확인**(2026-10-26). 그 뒤 미사용 코드 삭제는 일지에 **미검증**. 2~4P 미검증.

### 한 줄 요약

4명이 세로 4분할 슬롯에서 SNES **10키**를 연타한다. **T1**은 우주 배경만 있는 빌드업이고, **T2**에서 클럽·전 키 정답·관중·스핀·빔·Scream으로 점수를 쌓는다. 입력마다 `oiiaiooiiiai`가 진행되고 12글자마다 보너스가 붙는다.

| 항목 | 값 |
|------|-----|
| T1 | 씬 `tier1DurationSeconds` **6.5** (코드 기본 5). `Bg_Space`만. 입력·Highlight·고양이·관중·빔·Scream·흔들림 없음. TIME은 본게임 초에 멈춤 |
| T2 | `mainRoundDurationSeconds`. 씬에 필드 없음 → 코드 **26**. 클럽 · 10키 Highlight · 전부 정답 · 관중 · SpinLoop · Beam · Scream · 떼창 |
| BGM | 라운드 시작과 함께 `mainBgmClip` 루프. 씬 `bgmStartTimeSeconds` **27** (코드 기본 0). 한 바퀴 뒤 곡의 처음으로 복귀 |
| 입력 | 10키 아무거나. 한 프레임에 여러 키가 눌려도 **1회** |
| 점수 | 입력 +100 · 12글자 완성 추가 +500 (그 입력은 +600) 후 진행도 0 |
| Dev God | Backspace. `_roundElapsed`와 T2 타이머가 멈춤. BGM은 계속 |

`Fever*` 코드 이름은 게이지·Scream·떼창에 남아 있다. 화면의 피버 모드 글자는 없다.

---

## 1. 게임 목적

- T1이 끝난 뒤 T2 동안 10키를 최대한 많이 누른다.
- 입력마다 스피커·가이드·떼창이 반응한다.
- 종료 후 점수가 높을수록 Result 등수가 높다.
- HP (`OiiaHpLossRules`): 최종 점수 ≤ **8000**, 또는 참가 2명 이상일 때 하위 50%(2→1, 3→1, 4→2). Result에서만 HP −1. HP 0 → GAME OVER.
- HP 컷 8000은 조정 보류 (`Project_Master_Context.md` §6).
- 씬 안 연습 없음. 메인 JOIN/READY/Enter 후 바로 본게임.

---

## 2. 플레이 흐름

```
메인 Enter → Minigame_O.I.I.A.
  OiiaSceneBootstrap → Begin
    TIME = mainRoundDurationSeconds (코드 26)
    T1: 우주, 입력 없음
    tier1DurationSeconds <= 0 이면 그 프레임에 EnterTier2

Tick (Dev God이 꺼져 있을 때 _roundElapsed 증가)
  elapsed >= T1 → EnterTier2
    10키 Highlight · 관중 상승 · Scream · 고양이 SpinLoop · 클럽
  T2에서만 TIME 감소 → 0 또는 Escape → CompleteSession → Results
```

슬롯 Tick: `TickGameplay`(T2만) · 고양이 · UiShake · Spotlight · 배경 · 스피커 · 관중 · HUD. 그다음 `UpdateTierBgm` · TIME 표시.

BGM은 T1을 포함해 라운드가 돌아가는 동안 재생된다. 시작 위치는 클립의 `bgmStartTimeSeconds`이다.

---

## 3. 화면

데이터는 `PartySession` 플레이어 0~3. 화면은 세로 4분할 (왼쪽이 1P). 씬 `Minigame_O.I.I.A..unity` · `Panel_O.I.I.A._4Way`. 슬롯 루트 `RectMask2D`로 옆 슬롯을 자른다. 비참가는 `WAITING`.

| 이름 | 역할 |
|------|------|
| `StageScreen` | 전광판. 자식 `Mask` 안에 `Bg_Space` / `Bg_Club`. Cat · SpotlightL/R |
| `DjBox` | Score · Combo · FeverGauge×2(패턴) · SubPatternGuide · Speakers |
| `SpeakerL` / `SpeakerR` | Body · WooferTop · WooferBottom |
| `Crowd` | T2에 상승 후 유지 |
| `Waiting` | 비참가 |

개편 전 캡처 `Documentation/images/oiia/` 는 현재 화면이 아니다.

---

## 4. UI 바인딩

`OiiaSlotPanelBindings` → `SlotUiBindings`.

| 필드 | 역할 |
|------|------|
| `WaitingText` · `SlotPanelBackgroundImage` · `CatAnimator` | 공통 |
| `DjBoxRoot` · Face/Dpad/Shoulder · `DjPadButtons[10]` | SNES 10키 |
| `HudScoreText` | 점수. 항상 표시 |
| `HudComboText` | `{n}<size=55%> COMBO</size>`. 0이면 숨김 |
| `FeverGaugeImage` · `FeverGaugeImageB` | `SubPatternMatched / 12`. 완성 시 0. 프리팹명 `FeverGauge_L` / `FeverGauge_R` |
| `SubPatternGuideText` | 맞춘 접두만 대문자 |
| `SpeakerL/R` Root·Body·WooferTop·WooferBottom | 스피커 |
| `StageScreenRoot` · `StageBackgroundSpace` · `StageBackgroundClub` | 배경은 `Mask` 아래에서 찾음 |
| `CrowdRoot` · `CrowdImage` | 관중 |
| `SpotlightL/R` Root·Fixture·Beam | T2만 표시 |

---

## 5. 입력

| Id | 부스 경로 |
|----|-----------|
| A/B/Y | Face A/B/Y |
| X | Primary Trigger |
| L/R | Shoulder L/R |
| Up/Down/Left/Right | Stick 방향 |

| 입력 | 동작 |
|------|------|
| T2의 10키 | 전부 정답. 같은 프레임은 첫 키 1회 |
| T1 | 입력 무시. Highlight 없음 |
| Escape | 세션 종료 |
| Backspace | Dev God. 에디터·Development 빌드 |

EMPTY 슬롯은 무시한다.

---

## 6. 연출

| 시스템 | 동작 |
|--------|------|
| UiShake | T2. 정답 `uiShakeTier2` + 상시 `uiShakeIdleTier2`. 대상은 HUD·DjBox·`Mask`의 `Bg*`. `StageScreen`과 `Mask`는 고정 |
| Speakers | 패턴 스텝마다 우퍼 펄스 · O/I/A 글자 · `speakerTier2` |
| Spotlight | T2만 Fixture·Beam |
| StageBackground | T1 Space · T2 Club |
| Crowd | T2 시작 시 Rising → Active. 퇴장 페이드 없음 |
| CatAnimator | T1 숨김 · T2 `SpinLoop` |

---

## 7. 점수·HP

| 이벤트 | 점수 |
|--------|------|
| 입력 1회 | +100 (`DjHitScore`) |
| 12글자 완성 | 추가 +500. 그 입력은 +600. 진행도 0 |

상수: `DjHitScore` 100 · `PatternCompleteBonusScore` 500 · `HpLowScoreThreshold` 8000.  
본게임 초는 상수가 아니라 Inspector `mainRoundDurationSeconds` (최소 1).

---

## 8. 오디오

| 채널 | 동작 |
|------|------|
| `mainBgmClip` | `UpdateTierBgm`. `AudioSource.time = bgmStartTimeSeconds` 후 루프. 볼륨 `tierBgmVolume` |
| `patternStepSfx[12]` | `PlayFeverChantStep`. 12번째는 리셋 전 `steppedPosition` |
| `feverScreamClip` | T2 시작부터 루프. 재시작·종료 시 정지 |
| `sessionEndClip` | `ExitSequence`에서 재생. 씬 클립은 **비어 있음** (연결 미결정) |

`buzzClip` 필드는 삭제했다.

---

## 9. 코드

| 파일 | 역할 |
|------|------|
| `OiiaSceneBootstrap.cs` | PartySession → `Begin` / `Tick` |
| `OiiaMinigameModule.cs` | `IMinigameModule` · `BuiltInId = "oiia"` |
| `OiiaSlotPanelBindings.cs` | 프리팹 AutoWire |
| `OiiaHpLossRules.cs` | Result HP 대상 |
| `Begin` / `Tick` / `ExitSequence` | 수명 |
| `PracticeFlow` | Escape만 |
| `Gameplay` / `DjPadInput` / `DjPadVisual` | 판정 · L/R · ABXY 명도 |
| `Fever` / `FeverAudio` / `FeverChant` | T2 진입 · Scream · 떼창 |
| `SubPatternGuide` | 패턴 · 가이드 |
| `Speakers` / `Crowd` / `UiShake` | 연출 |
| `Spotlight` / `StageBackground` / `CatAnimator` | 전광판 |
| `TierBgm` / `PatternAudio` | BGM · 스텝 SFX. T1 길이·본게임 초·BGM 위치 필드 |
| `Ui` / `SlotPanels` / `SlotEmptyUi` / `SlotHelpers` | HUD · 클리핑 |
| `Timer` / `Balance` / `Constants` / `Types` / `Config` / `State` / `DevGodMode` | 공통 |

삭제된 것: 연습 큐 · 3키 · 3초 피버 · `GlobalTier.cs` · `buzzClip` · `OiiaVideoEffectController` · `OiiaUIChromaKey` · `Mat_OiiaChromaKeyVideo` · `PracticeReadyText` · `HudFeverText` · `StageBackgroundChromaKey` · SpinOnce · 관중 퇴장 페이드. 경과는 `02_개발_진행_일지.md`.

---

## 10. 씬 · Inspector

씬: `Assets/Scenes/Minigame_O.I.I.A..unity`. 루트 `OiiaMinigameModule` + `OiiaSceneBootstrap`. 컨테이너 `Panel_O.I.I.A._4Way`. `FadeOverlay` · `Timer`.

씬 YAML (2026-10-26 파일 기준):

| 필드 | 씬 | 코드 기본 |
|------|----|-----------|
| `tier1DurationSeconds` | **6.5** | 5 |
| `bgmStartTimeSeconds` | **27** | 0 |
| `mainRoundDurationSeconds` | **없음** (저장 전). 런타임은 코드 값 | **26** |
| `uiShakeTier2` | Amp 20 · 0.25s · 64Hz | 같음 |
| `uiShakeIdleTier2` | Amp 8 · 32Hz | 같음 |
| `speakerTier2` | 우퍼 스케일 2 · 0.1s · 글자 100 · 비행 200 | — |
| `crowdRiseDistance` / `crowdRiseSeconds` | 120 · 0.35 | 같음 |
| `crowdShakeAmplitude` / `Frequency` | **10** · **3** | 8 · 18 |
| `spotlightBeamAlpha` | 0.15 | — |
| `tierBgmVolume` | 0.85 | — |
| `feverChant*` | Layer 12 · Pool 32 · Pitch 0.98~1.02 · Vol 0.5~1 · Delay 0.15 | — |
| `sessionEndClip` | 비어 있음 | — |
| `oiiaDjPadUnpressedBrightness` | 100 | 100 |

필드가 삭제된 뒤 씬에 남은 `crowdFadeSeconds` · `catAnimatorSpinOnceState` 는 스크립트가 읽지 않는다. Unity가 씬을 다시 저장하면 빠진다.

`uiShakeTier2` · `uiShakeIdleTier2` · `speakerTier2` 는 예전 `*Tier3` 키를 `FormerlySerializedAs`로 읽는다.

---

## 11. 데이터 흐름

```
Begin → TIME 세팅 · T1 배경 · (T1 길이 0이면 EnterTier2)
EnterTier2 → BeginBaseFever → Highlight · Crowd · Scream · SpinLoop · Club

OnDjHit
  → Combo++ · AdvanceSubPatternOnHit
  → +100 · (12글자면 +500, 진행도 0)
  → UiShake · Speakers · PlayPatternStepSfx(steppedPosition)
```

---

## 12. Result

| 항목 | 내용 |
|------|------|
| Report | `MinigameId = "oiia"` · `FinalScore` · `HpLostThisSession` |
| HP | `OiiaHpLossRules.FillHpLost` |
| UI | 공통 `Results` 씬 |

---

## 13. AI 참고

- 패턴 문자열과 `patternStepSfx` 길이는 12로 맞춘다.
- 12번째 SFX는 리셋 전 `steppedPosition`을 쓴다.
- T2 연출은 `EnterTier2`에서 켠다. `Begin`은 T1이면 켜지 않는다.
- 점수 상수는 `DjHitScore`와 `PatternCompleteBonusScore`뿐이다.
- 에디터 조작 순서는 채팅에만 적는다.

| 증상 | 확인 |
|------|------|
| T1인데 입력이 됨 | `_tier2Active` · `TickGameplay` |
| T2 연출이 안 켜짐 | `EnterTier2` · `BeginBaseFever` |
| 보너스 없음 | `AdvanceSubPatternOnHit`의 `patternCompleted` |
| 마지막 글자 무음 | `steppedPosition` → `PlayPatternStepSfx` |
| 배경이 안 흔들림 | `StageScreen/Mask/Bg_*` |
| 옆 슬롯 침범 | `clipSlotContentToPanel` |
| Scream/떼창 | `FeverAudio` · `FeverChant` |
