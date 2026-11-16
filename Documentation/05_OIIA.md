# 05_OIIA

> 문서 기준일: 코드·씬 파일 직접 분석 (추측 없음). **에디터 조작 가이드·플레이 검증 체크리스트는 본 문서에 두지 않는다** (`Project_Master_Context.md` §2·§3). 검증 타임라인은 `02_개발_진행_일지.md`.  
> **갱신**: 2026-11-16 — 티어별 게이지·입력 점수. `playerTier1/2/3` 기본은 최대 **100** · 입력 **+8** · 초당 **20** 감소 · 점수 **100**. 빌드업 **6.5**초 · BGM **27**초 · 본게임 **26**초.

---

## 0. 현재 상태

> **상태**: 개인 티어, 게이지·스피커·점수 이펙트, 흔들림 제외(Score·HudDisplay·SubPatternGuide·Guage) **Play 확인**(2026-11-17).

### 한 줄 요약

4명이 세로 4분할 슬롯에서 SNES **10키**를 연타한다. 빌드업은 우주 배경만 있고 입력이 없다. 연타가 시작되면 슬롯마다 게이지로 개인 티어 1·2·3이 오르내린다. 입력마다 `oiiaiooiiiai`가 진행되고 12글자마다 보너스가 붙는다. 화면 게이지는 그 패턴이 아니라 티어 게이지다.

| 항목 | 값 |
|------|-----|
| 빌드업 | 씬 `tier1DurationSeconds` **6.5** (코드 기본 5). `Bg_Space`만. 입력·Highlight·고양이·관중·빔·Scream·흔들림 없음. 개인 티어는 1, 게이지 0, 감소 없음 |
| 연타 | `mainRoundDurationSeconds`. 씬 **26**. 10키 Highlight · 전부 정답. 티어는 슬롯마다 따로 |
| 게이지 | 티어별 `playerTier1/2/3`. 기본 최대 **100** · 입력 **+8** · 초당 **20** 감소. 그 티어 최대를 넘긴 양만 다음 티어로 이월. 0이면 한 단계 하락 후 **아래 티어** 최대에서 다시 감소. 3도 하락. 1에서 0이면 1에 머무름. 하락으로 채운 최대는 승급이 아님. 막대는 `Gauge / 그 티어 GaugeMax` |
| 개인 티어 1 | `StageScreen` 기본 이미지. Space·Club 끔. 빔·관중·Scream 없음. 입력마다 `SpinOnce`. 흔들림 없음 |
| 개인 티어 2 | `Bg_Space`. 빔·관중·Scream 없음. `SpinOnce`. 흔들림·스피커는 티어 2 수치 |
| 개인 티어 3 | `Bg_Club`. 네온 Beam · 관중 상승 · Scream · `SpinLoop`. 씬에 저장된 강한 흔들림·스피커. 0이면 티어 2로 내려가고 관중은 즉시 숨김 |
| BGM | 라운드 시작과 함께 `mainBgmClip` 루프. 씬 `bgmStartTimeSeconds` **27** (코드 기본 0). 한 바퀴 뒤 곡의 처음으로 복귀 |
| 입력 | 10키 아무거나. 한 프레임에 여러 키가 눌려도 **1회** |
| 점수 | 누르기 전 티어의 `HitScore`. 세 티어 기본 **100**. 12글자 완성 추가 +500은 티어와 무관. 그 입력으로 티어가 올라도 점수는 누르기 전 티어. 운영자 점수는 현재 티어 `HitScore`만. 게이지·패턴은 올리지 않음 |
| Dev God | Backspace. `_roundElapsed`·연타 타이머·게이지 감소가 멈춤. 입력 게이지 증가는 유지. BGM은 계속 |

`Fever*` 코드 이름은 게이지·Scream·떼창에 남아 있다. `IsFeverActive`는 개인 티어 3이다. 화면의 피버 모드 글자는 없다.

---

## 1. 게임 목적

- 빌드업이 끝난 뒤 연타 동안 10키를 최대한 많이 누른다. 게이지를 채워 개인 티어를 올린다.
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
    빌드업: 우주, 입력 없음, 개인 티어 1, 게이지 0
    tier1DurationSeconds <= 0 이면 그 프레임에 EnterTier2

Tick (Dev God이 꺼져 있을 때 _roundElapsed 증가)
  elapsed >= 빌드업 → EnterTier2
    10키 Highlight. 관중·Scream·SpinLoop는 아직 없음
  연타에서만 남은 시간 감소 → 0 또는 Escape → CompleteSession → Results
  입력(게이지 증가·승급) 다음 같은 프레임에 게이지 감소·하락
```

슬롯 Tick: `TickGameplay`(연타만) · 운영자 점수 · `TickPlayerTierGauge` · 고양이 · UiShake · Spotlight · 배경 · 스피커 · 관중 · HUD. 그다음 `UpdateTierBgm`.

BGM은 빌드업을 포함해 라운드가 돌아가는 동안 재생된다. 시작 위치는 클립의 `bgmStartTimeSeconds`이다.

---

## 3. 화면

데이터는 `PartySession` 플레이어 0~3. 화면은 세로 4분할 (왼쪽이 1P). 씬 `Minigame_O.I.I.A..unity` · `Panel_O.I.I.A._4Way`. 슬롯 루트 `RectMask2D`로 옆 슬롯을 자른다. 비참가는 `WAITING`.

| 이름 | 역할 |
|------|------|
| `StageScreen` | 전광판. 자식 `Mask` 안에 `Bg_Space` / `Bg_Club`. Cat · SpotlightL/R |
| `DjBox` | Score · Combo · `Gauge_L/R_1~3` · SubPatternGuide · Speakers. 자신은 흔들리지 않음 |
| `SpeakerL_1~3` / `SpeakerR_1~3` | 티어까지 누적. 각 Body · WooferTop · WooferBottom |
| `Crowd` | 개인 티어 3에 상승. 내려가면 즉시 숨김 |
| `Waiting` | 비참가 |

개편 전 캡처 `Documentation/images/oiia/` 는 현재 화면이 아니다.

---

## 4. UI 바인딩

`OiiaSlotPanelBindings` → `SlotUiBindings`.

| 필드 | 역할 |
|------|------|
| `WaitingText` · `SlotPanelBackgroundImage` · `CatAnimator` | 공통 |
| `DjBoxRoot` · Face/Dpad/Shoulder · `DjPadButtons[10]` | SNES 10키 |
| `HudScoreText` | `DjBox/Score`. 점수. 항상 표시. 자식 `T1`~`T3` 각 4개는 입력 점수 이펙트 |
| `HudComboText` | `HudDisplay/Combo`. `{n}<size=55%> COMBO</size>`. 0이면 숨김 |
| `TierGaugeLeft[3]` · `TierGaugeRight[3]` | `Gauge_L_1~3` · `Gauge_R_1~3`. 현재 티어 쌍만 켜고 `fillAmount = Gauge / 그 티어 GaugeMax`. 자식 `Frame`은 채우지 않음 |
| `SubPatternGuideText` | 맞춘 접두만 대문자 |
| `SpeakerLeft[3]` · `SpeakerRight[3]` | `SpeakerL_1~3` · `SpeakerR_1~3`. 티어 n까지 켜짐 |
| `StageScreenRoot` · `StageBackgroundSpace` · `StageBackgroundClub` | 배경은 `Mask` 아래에서 찾음 |
| `CrowdRoot` · `CrowdImage` | 관중 |
| `SpotlightL/R` Root·Fixture·Beam | 연타 중 Fixture. Beam은 개인 티어 3만 |

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
| 연타의 10키 | 전부 정답. 같은 프레임은 첫 키 1회. 게이지는 그 티어 `GaugePerHit` |
| 빌드업 | 입력 무시. Highlight 없음. 게이지 감소 없음 |
| Escape | 세션 종료 |
| Backspace | Dev God. 에디터·Development 빌드 |
| 키보드 `1`~`4` | 운영자 점수. 살아 있는 슬롯의 현재 티어 `HitScore`. 패턴·콤보·12글자 보너스·게이지는 진행하지 않음. 키패드 동일, 디버그 ON이면 키패드 4 제외 |

EMPTY 슬롯은 무시한다.

---

## 6. 연출

| 시스템 | 동작 |
|--------|------|
| UiShake | 연타 중. 정답 `uiShakeTier1/2/3` + 상시 `uiShakeIdleTier1/2/3`. 티어 1은 진폭 0. 대상은 `DjBox` 직속 자식 중 `Score`·`HudDisplay`·`SubPatternGuide`·`Gauge*`·`Guage`·`Speaker*`를 뺀 것과 `Mask`의 `Bg*`. 그 제외 대상과 그 자식, `DjBox`·`StageScreen`·`Mask`는 고정. 스피커 흔들림은 슬라이드 위에 따로 더함 |
| Speakers | `SpeakerL/R_1~3`. 티어 n이면 1~n이 제자리로 들어오고, 더 높은 번호는 나간다. 왼쪽은 왼쪽, 오른쪽은 오른쪽. 거리 `speakerSlideDistance`(기본 400) · 시간 `speakerSlideSeconds`(기본 0.4). 패턴 스텝마다 들어와야 하는 우퍼 펄스 · O/I/A 글자 · 현재 티어 `speakerTier1/2/3` |
| Spotlight | 연타 중 Fixture. Beam 스트로보는 개인 티어 3 |
| StageBackground | 빌드업 Space. 연타 티어 1은 둘 다 끔(StageScreen 기본). 티어 2 Space. 티어 3 Club |
| Crowd | 개인 티어 3 진입 시 Rising → Active. 이탈 시 즉시 숨김. 퇴장 페이드 없음 |
| CatAnimator | 빌드업 숨김. 연타 티어 1·2는 입력마다 `SpinOnce`. 티어 3은 `SpinLoop` |

---

## 7. 점수·HP

| 이벤트 | 점수 |
|--------|------|
| 입력 1회 | 누르기 전 티어 `HitScore`. 기본 100. `Score`의 그 티어 글자 1개가 0.2초 동안 y +40, 알파 0으로 꺼짐 |
| 12글자 완성 | 추가 +500 (`PatternCompleteBonusScore`). 티어와 무관. 진행도 0 |

상수: `PatternCompleteBonusScore` 500 · `HpLowScoreThreshold` 8000. 입력 점수는 `playerTier1/2/3.HitScore`.  
본게임 초는 상수가 아니라 Inspector `mainRoundDurationSeconds` (최소 1).

---

## 8. 오디오

| 채널 | 동작 |
|------|------|
| `mainBgmClip` | `UpdateTierBgm`. `AudioSource.time = bgmStartTimeSeconds` 후 루프. 볼륨 `tierBgmVolume` |
| `patternStepSfx[12]` | `PlayFeverChantStep`. 12번째는 리셋 전 `steppedPosition` |
| `feverScreamClip` | 개인 티어 3 슬롯이 하나라도 있는 동안 루프. 없으면 정지 |
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
| `Fever` / `FeverAudio` / `FeverChant` | 개인 티어 게이지 · 티어 3 Scream · 떼창 |
| `SubPatternGuide` | 패턴 · 가이드 |
| `Speakers` / `Crowd` / `UiShake` | 연출 |
| `Spotlight` / `StageBackground` / `CatAnimator` | 전광판 |
| `TierBgm` / `PatternAudio` | BGM · 스텝 SFX. T1 길이·본게임 초·BGM 위치 필드 |
| `Ui` / `SlotPanels` / `SlotEmptyUi` / `SlotHelpers` | HUD · 클리핑 |
| `Balance` / `Constants` / `Types` / `Config` / `State` / `DevGodMode` | 공통 |

`SpinOnce`는 연타 티어 1·2에서 다시 재생한다. 삭제된 것: 연습 큐 · 3키 · 3초 피버 · 시간 기반 글로벌 티어 · `buzzClip` · `OiiaVideoEffectController` · `OiiaUIChromaKey` · `Mat_OiiaChromaKeyVideo` · `PracticeReadyText` · `HudFeverText` · `StageBackgroundChromaKey` · 관중 퇴장 페이드 · 화면 타이머 TMP(`OiiaMinigameModule.Timer`). 경과는 `02_개발_진행_일지.md`.

---

## 10. 씬 · Inspector

씬: `Assets/Scenes/Minigame_O.I.I.A..unity`. 루트 `OiiaMinigameModule` + `OiiaSceneBootstrap`. 컨테이너 `Panel_O.I.I.A._4Way`. `FadeOverlay`. 화면 타이머 TMP는 없음.

씬 YAML (2026-11-16 파일 기준):

| 필드 | 씬 | 코드 기본 |
|------|----|-----------|
| `tier1DurationSeconds` | **6.5** | 5 |
| `bgmStartTimeSeconds` | **27** | 0 |
| `mainRoundDurationSeconds` | **26** | **26** |
| `playerTier1/2/3` | 각 `GaugeMax` **100** · `GaugePerHit` **8** · `GaugeDrainPerSecond` **20** · `HitScore` **100** | 같음 |
| `uiShakeTier3` | Amp 20 · 0.25s · 64Hz | 같음. 예전 씬 키 `uiShakeTier2`를 이 키로 변경 |
| `uiShakeIdleTier3` | Amp 8 · 32Hz | 같음. 예전 씬 키 `uiShakeIdleTier2` |
| `uiShakeTier1` · `uiShakeIdleTier1` | 씬에 없음 | 진폭 0 |
| `uiShakeTier2` | 씬에 없음 | Amp 8 · 1s · 5Hz |
| `uiShakeIdleTier2` | 씬에 없음 | Amp 10 · 1Hz |
| `speakerTier3` | 우퍼 스케일 2 · 0.1s · 글자 100 · 비행 200 | 같음. 예전 씬 키 `speakerTier2` |
| `speakerTier1` | 씬에 없음 | 우퍼 1.2 · 글자 64 · 비행 70 |
| `speakerTier2` | 씬에 없음 | 우퍼 1.35 · 글자 80 · 비행 95 |
| `crowdRiseDistance` / `crowdRiseSeconds` | 120 · 0.35 | 같음 |
| `crowdShakeAmplitude` / `Frequency` | **10** · **3** | 8 · 18 |
| `spotlightBeamAlpha` | 0.15 | — |
| `tierBgmVolume` | 0.85 | — |
| `feverChant*` | Layer 12 · Pool 32 · Pitch 0.98~1.02 · Vol 0.5~1 · Delay 0.15 | — |
| `sessionEndClip` | 비어 있음 | — |
| `oiiaDjPadUnpressedBrightness` | 100 | 100 |

필드가 삭제된 뒤 씬에 남은 `crowdFadeSeconds` · `catAnimatorSpinOnceState` 는 스크립트가 읽지 않는다. Unity가 씬을 다시 저장하면 빠진다.

씬의 강한 흔들림·스피커 값은 `*Tier3` 키에 있다. `*Tier1`·`*Tier2`는 씬에 없으면 코드 기본값이다. Unity가 씬을 저장하면 그 기본값이 YAML에 들어간다.

---

## 11. 데이터 흐름

```
Begin → 빌드업 Space · 개인 티어 1 · 게이지 0 · (빌드업 길이 0이면 EnterTier2)
EnterTier2 → Highlight. 관중·Scream·SpinLoop는 티어 3부터

OnDjHit
  → Combo++ · 누르기 전 티어 HitScore
  → AddPlayerTierGaugeOnHit (그 티어 GaugePerHit, 그 티어 GaugeMax를 넘기면 이월)
  → AdvanceSubPatternOnHit
  → (12글자면 +500, 진행도 0)
  → UiShake · 티어 1·2면 SpinOnce · Speakers · PlayPatternStepSfx
TickPlayerTierGauge
  → 그 티어 초당 감소. 0이면 티어 -1, 게이지 = 아래 티어 GaugeMax. 티어 3을 벗어나면 관중 즉시 숨김
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
- 연타 시작은 `EnterTier2`다. 티어 3 연출은 게이지 승급 때 켠다. `Begin`은 빌드업이면 켜지 않는다.
- 12글자 보너스는 `PatternCompleteBonusScore`. 입력 점수·게이지 최대·충전·감소는 `playerTier1/2/3`.
- 에디터 조작 순서는 채팅에만 적는다.

| 증상 | 확인 |
|------|------|
| 빌드업인데 입력이 됨 | `_tier2Active` · `TickGameplay` |
| 티어 3 연출이 안 켜짐 | `AddPlayerTierGaugeOnHit` · `ApplyPlayerTierTransition` · `IsFeverActive` |
| 보너스 없음 | `AdvanceSubPatternOnHit`의 `patternCompleted` |
| 마지막 글자 무음 | `steppedPosition` → `PlayPatternStepSfx` |
| 배경이 안 흔들림 | `StageScreen/Mask/Bg_*` |
| 옆 슬롯 침범 | `clipSlotContentToPanel` |
| Scream/떼창 | `FeverAudio` · `FeverChant` |
