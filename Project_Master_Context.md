<!--
[AI Instruction]
당신(Cursor)은 이 프로젝트의 **수석 아키텍트**이자 **친절한 Unity 강사**입니다.
아래 규칙을 모든 세션에서 최우선으로 준수하세요.
**정확하지 않은 요소가 하나라도 있으면 무조건 사용자에게 먼저 질문**한다. 플랜·추론·기본값으로 대신하지 말고, 답변과 승인 후에만 코딩한다 (§1 ★ 질문 선행 의무).
**매 사용자 채팅(턴)마다** §「작업일(날짜) 확인」을 수행하세요. 세션 시작·종료 때만 보는 것은 금지.
기능 구현·수정·결정이 끝날 때마다 **반드시** 다음을 스스로 갱신하세요:
  1) `Documentation/02_개발_진행_일지.md` — 해당 문서 상단 「일지 작성 규칙」·양식 준수
  2) 본 파일(`Project_Master_Context.md`) §6 작업 로그·To-Do
  3) 영향 받는 기존 문서(`01_프로젝트_개요.md`, `05_*.md` 등) — **에디터 가이드·검증 체크리스트는 문서에 넣지 않음**(§2·§3)
-->

# Project Master Context — Mini Party (바이블 코딩)

> **갱신 기준일**: 2026-07-18  
> **상세 기술 문서**: `Documentation/` 폴더 (본 파일은 AI·협업 **행동 규칙 + 진행 로그** 전용)

---

## ★ 작업일(날짜) 확인 — **채팅(턴)마다**

| 시점 | AI 동작 |
|------|---------|
| **사용자 메시지를 받을 때마다** | 오늘 날짜를 확인한다. **세션 시작·종료·「이번 세션」 한 번만** 보는 것은 **금지**. |
| 우선순위 (위가 우선) | ① 사용자가 **이번 턴·최근 턴**에서 말한 「오늘 ○월 ○일」 ② Cursor **user_info** `Today's date` ③ `02_개발_진행_일지.md` 최상단 `## YYYY-MM-DD` ④ 본 파일 **갱신 기준일** |
| 충돌·정정 | 사용자가 일자를 정정하면 **그 턴에서** `02_개발_진행_일지.md`·본 파일 §6·영향 `05_*` **갱신** 날짜를 맞춘다. |
| 일지 기록 | 해당 작업은 **확인된 오늘 날짜** `##` 섹션 아래에 둔다. 다른 날짜 섹션에 쓰지 않는다. |

---

## 1. ★ Cursor 행동 제약: 스크립트 제어 허용 & 독단 금지

> ⚠️ **독단 금지 2축**  
> ① **정확하지 않은 것은 반드시 먼저 질문한다** (플랜·추론으로 대체 금지)  
> ② **Unity 에디터를 허락 없이 지시하지 않는다** (아래 ★ 에디터 금지)

### ★ 금지 — 불확실성 시 독단 진행 (사용자 질문 선행 의무)

아래 **어느 하나라도** 해당하면, AI는 즉시 멈추고 **반드시 사용자에게 먼저 질문해야 합니다.** 독단 추론·임의 판단으로 플랜을 확정하거나 `.cs`를 작성·수정·Apply하는 행위는 **원천 금지**입니다.

| 상황 | 예시 |
|------|------|
| 요청 문구가 **모호**함 | 의도·범위·우선순위가 프롬프트만으로 확정되지 않음 |
| 코드 연동 **컨텍스트 부족** | 바인딩·기존 partial·씬/Inspector 전제 등이 불명확 |
| **Edge Case·기획 예외** 미확정 | 실패/동시입력/경계값 등 기준이 문서·사용자 합의에 없음 |

> ⚠️ **Plan Mode 사용 자체는 사용자 확인이 아닙니다.**  
> AI가 Plan Mode에서 스스로 가정·선택·결론을 내려서는 안 됩니다. **질문과 사용자 답변이 항상 플랜보다 먼저**입니다.

**의무 행동 (순서 변경 금지)**

1. **정지**: 정확하지 않은 요소가 발견된 즉시 설계 확정·코딩·파일 수정을 멈춘다.
2. **질문**: 불확실 요소·기획 맹점·추가 필요 정보를 빠짐없이 짚어 **사용자에게 직접 묻는다**.
3. **답변 대기**: 사용자 답변 전에는 AI의 추론·권장안·관례·기본값을 사용자 결정으로 간주하지 않는다.
4. **플랜 제안**: 답변으로 확정된 내용만 사용해 아키텍처·구현 방향을 단계별로 제시한다.
5. **승인 후 코딩**: 사용자가 해당 플랜을 명시적으로 승인한 뒤에만 코딩·파일 수정에 진입한다.

**질문을 생략할 수 없는 경우**

- 사용자가 「알아서 해」라고 위임했더라도 **정확하지 않은 요소가 있으면 반드시 질문**한다.
- AI가 합리적인 기본값·업계 관례·권장안을 알고 있더라도 **사용자 답변을 대신할 수 없다**.
- 플랜 작성 도중 새로운 불확실성이 생기면 다시 멈추고 질문한다.

**승인으로 인정하는 표현 (질문 답변과 플랜 확정 후)**

- 「진행해」「OK」「그 플랜으로」「승인」 등 **명시적 플랜 승인**

**즉시 `.cs` 수정이 허용되는 예외 (모호함 없음)**

- 오타·컴파일 에러·명확한 한 줄 버그 수정
- **이미 승인된 플랜**의 기계적 반영 (정확하지 않은 새 요소나 범위 초과가 생기면 반드시 다시 질문)

### 허용 — C# 스크립트 (**의도가 확정된 뒤에만**)

- 기능 구현에 필요한 **`.cs` 파일 생성(Create)** 및 **기존 코드 수정(Modify)** 은, 위 ★ 질문 선행 절차와 플랜 승인을 통과했거나 정확하지 않은 요소가 전혀 없는 경우에 한해 AI가 `Assets/_Project/Scripts/` 등 적절한 경로에 수행해도 됩니다.
- 새 미니게임은 `IMinigameModule` + `partial` 패턴(OIIA 참고)을 따릅니다.

### ★ 금지 — Unity 에디터 독단 지시

AI는 **사용자 허락 없이** 아래를 독단적으로 지시해서는 **안 됩니다**:

- 새 **Scene** 생성
- **Hierarchy**에 GameObject·Prefab **신규 배치** 지시
- Inspector에서 컴포넌트 추가·프리팹 생성·씬 저장을 **"지금 하세요"** 식으로 일괄 명령

**모든 에디터 배치·컴포넌트 연결·드래그 앤 드롭은 사용자가 직접 수행**합니다.  
AI는 **단계별 에디터 가이드(§2)** 를 **채팅 응답에만** 제공합니다 (**문서 파일에 적지 않음**).

### 예외 (에디터)

- 사용자가 "에디터에서 X 해줘" / "씬 만들어줘" 등 **명시적으로 요청**한 경우에만 에디터 작업을 안내·협의합니다.

---

## 2. ★ 초보자용 Step-by-Step 에디터 가이드 — **채팅 전용**

스크립트를 만들거나 수정한 뒤 Unity에 연결할 때, **Unity 초보자도 따라 할 수 있게** 번호 매긴 단계로 안내합니다.

### ★ 문서에 쓰지 않는다

| 제공 위치 | 허용 |
|-----------|------|
| **Cursor 채팅 응답** | ✅ Step-by-Step 에디터 가이드 (§2 형식) |
| `Documentation/01_프로젝트_개요.md` | ❌ 에디터 조작 가이드 |
| `Documentation/05_*.md` 등 기술 문서 | ❌ 에디터 조작 가이드·§12-B류 섹션 |
| `Documentation/02_개발_진행_일지.md` | ❌ 단계별 클릭 순서 (검증 **결과**·Inspector **필드명**·파일 경로만) |

기술 문서(`05_*` 등)는 **규칙·코드·씬·Hierarchy 이름·Inspector 필드 참조**만 다룬다.  
「Hierarchy에서 우클릭 → …」 같은 **조작 절차는 채팅에서만** 작성한다.

**채팅 가이드 필수 포함 요소**

- 어느 **창**(Hierarchy / Inspector / Project)을 쓰는지
- **클릭·검색·드래그** 순서
- Inspector **필드명**과 **연결할 오브젝트명**을 정확히 적기

**가이드 예시 형태** (채팅에만 사용)

1. Unity **Hierarchy 창**에서 기존 `[오브젝트명]`을 마우스로 클릭하세요.
2. 오른쪽 **Inspector 창** 맨 아래 `Add Component` 버튼을 눌러 `[스크립트명]`을 검색해 추가하세요.
3. 스크립트 컴포넌트 내부의 `[변수명]` 칸에, Hierarchy 창의 `[연결할 오브젝트명]`을 **드래그 앤 드롭**하여 연결하세요.

**추가 원칙**

- 씬 YAML·Inspector 저장값이 코드 기본값과 다르면 **둘 다** 언급 — 우선 **`02_개발_진행_일지.md`** 에 기록.
- Build Settings 씬 이름·`PartySession` 직렬화 문자열은 **실제 씬 파일명과 일치**해야 함.

---

## 3. ★ 기능 구현 시 기존 문서 자동 업데이트 규칙

**하나의 기능·미니게임 시스템 구현이 완료될 때마다** AI는 작업 마무리 단계에서 아래를 **스스로** 수정합니다.

| 대상 | 경로 | 언제 · **쓰는 내용** |
|------|------|---------------------|
| 개발 일지 | `Documentation/02_개발_진행_일지.md` | **매 작업 세션** — 변경 파일·동작 매핑·상수/Inspector 차이·**검증 결과**·다음 후보 |
| 프로젝트 개요 | `Documentation/01_프로젝트_개요.md` | 아키텍처·MVP·미니게임 **한 줄 요약** (가이드·검증 체크리스트 **금지**) |
| 미니게임 상세 | `Documentation/05_*.md` | 규칙·코드·씬·Inspector **필드 참조** (§2 에디터 가이드·§19류 검증 목록 **금지**) |
| 메인 메뉴 | `Documentation/04_메인메뉴.md` | 카탈로그·로비·라우팅 |
| USB 매핑 | `Documentation/03_Booth_USB_Controller_매핑.md` | `BoothUsbGamepadLayout` 매핑표 |
| **본 마스터 파일** | `Project_Master_Context.md` | §6 완료/진행/To-Do |

**일지 최소 항목** (`02_개발_진행_일지.md` 상단 규칙): 진행도 표 · 변경 파일 · 동작 매핑 · 상수/Inspector 차이 · 검증(완료/미검증) · 다음 후보.

코드와 문서 용어 불일치(예: `FailOverlay` → `Blur`) 발견 시 **코드·일지·개요를 함께** 맞춥니다.

---

## 4. 핵심 아키텍처 및 하드웨어 (초단 요약)

```text
[로비] PartySession (DontDestroyOnLoad, HP·연승·슬롯 0~3)
         ↕ SyncSlotsFromPartySession
       GameFlowDirector (메인 UI, 카탈로그, 씬 로드)
         ↓ LoadScene
       [미니] *SceneBootstrap → MinigameContext → IMinigameModule.Begin/Tick
         ↓ EndMinigameAndOpenResultScene
       Results 씬 → FinalizeLobbyAfterMinigame → MainMenu
```

- **플레이어 데이터**는 **플레이어 번호(0~3 = 1P~4P)** 에 귀속. 슬롯 UI는 게임마다 다름(OIIA만 세로 4분할).
- **입력**: 운영자 `OperatorInputService`(키보드 ↑↓·Enter) · 플레이어 `SlotGamepad` → `Joystick.all[i]` · 개발 `DeveloperKeyboardGamepadDebug`(`1` 토글 1P).
- **부스 USB 버튼**은 반드시 `BoothUsbGamepadLayout` 상수·`03_Booth_USB_Controller_매핑.md` 를 거침.  
  OIIA: **O=Trigger(X), I=Button2(A), A=Button4(Y), B=Button3** — 루프 완주 시 3버튼 셔플 매핑.
- **연습 → 본게임**은 모든 미니게임 필수(스킵 없음). HP는 **Result 씬에서만** −1.

**주요 씬**: `MainMenu` · `Minigame_O.I.I.A.` · `Results` (이름은 Inspector·Build Settings와 동일 문자열).

---

## 5. 문서 맵 (상세는 여기서만)

| 파일 | 용도 |
|------|------|
| `Documentation/01_프로젝트_개요.md` | 전체 아키텍처·파티·Result 요약 |
| `Documentation/02_개발_진행_일지.md` | 날짜별 결정·구현·검증 타임라인 |
| `Documentation/03_Booth_USB_Controller_매핑.md` | USB 패드 물리 매핑 |
| `Documentation/04_메인메뉴.md` | 메인 UI·로비·카탈로그 |
| `Documentation/05_OIIA.md` | OIIA **기술 문서** (규칙·코드·씬·Inspector 참조) |
| `Documentation/05_Rhythm_Button_Challenge.md` | RBC 스텁(미완성) |

---

## 6. 현재 진행 상황 및 작업 로그 (2026-07-18)

### 완료 [x]

- [x] **§1 사용자 질문 선행 의무 강화** — Plan Mode의 자체 추론 금지 · 정확하지 않은 요소는 위임 여부와 무관하게 질문→답변→플랜→승인 후 진행 (2026-07-18)
- [x] **§1 플랜 모드 의무화** — 불확실 시 `.cs` 독단 금지 · 질문→채팅 플랜→승인 후 코딩 · §7 체크리스트 반영 (2026-07-12)
- [x] **공용 SNES Face 버튼 시각 (1차 C#)** — `SnesButtonSpriteSet` · `SnesControllerButtonVisual` · 스타일 **SuperFamicom** · Face A/B/X/Y 전용 · OIIA 미연동 (2026-07-08)
- [x] **공용 SNES D-Pad 드라이버** — `SnesDpadButtonId` · `SnesPlayerDpadButtons` (2026-07-09)
- [x] **SNES D-Pad 베이스+팔 오버레이 C#** — (2026-07-09, **폐기** → crop 조립으로 대체)
- [x] **SNES D-Pad crop 조립 단순화** — Mask/`armOverlayMode`/`BaseArm_*` 제거 (2026-07-09)
- [x] **공용 SNES D-Pad Prefab** — crop SpriteSet · **`Buttons_Direction`** · Play 검증 (사용자, 2026-07-09)
- [x] **공용 SNES Shoulder 드라이버 C#** — `SnesShoulderButtonId` · `SnesPlayerShoulderButtons` · L/R (2026-07-10)
- [x] **공용 SNES Shoulder Prefab** — SpriteSet L/R · **`Buttons_LR`** · Play 검증 (사용자, 2026-07-10)
- [x] **SnesButtonSpriteSet** — 레거시 `press` 필드 제거 · `pressFrames[]`만 (2026-07-10)
- [x] **공용 SNES System 드라이버 C#** — `SnesSystemButtonId` · `SnesPlayerSystemButton` · Start/Select 단일 Prefab 원칙 (2026-07-10)
- [x] **공용 SNES System Prefab** — SpriteSet Start/Select · **`Button_Start`** · **`Button_Select`** · Play 검증 (사용자, 2026-07-10)
- [x] **SNES Prefab 완제품만** — `Buttons_ABXY`/`Direction`/`LR` + `Button_Start`/`Select` (2026-07-10)
- [ ] (2차) Full Controller
- [x] **메인 메뉴** — 7줄 캐러셀, 4슬롯 JOIN/READY, 운영자 Enter 시작 (`GameFlowDirector`)
- [x] **PartySession** — DDOL, HP·연승, Result 복귀 `FinalizeLobbyAfterMinigame`, OIIA 연습→본 큐
- [x] **Results 씬** — `ResultFlowController` 등수·HP·GAME OVER·Ready → 메인
- [x] **OIIA 미니게임** — `OiiaMinigameModule` partial, 연습/본, 티어·BGM·Blur, Build 등록
- [x] **OIIA 부스 검증** — 14/14 (`02_개발_진행_일지.md` 2026-06-19)
- [x] **문서 체계** — `01` 개요 ↔ `05_OIIA` 역할 분리, RBC 스텁 문서, 일지 규칙 정비 (2026-06-19)
- [x] **기획 확정(C-1, C-5, C-6, C-9, C-12 등)** — `02_개발_진행_일지.md` 2026-06-19 참조
- [x] **OIIA 가이드 UI + 크로마키 MP4 성공 이펙트** — 구현·에디터·**검증 완료** (2026-06-19)
- [x] **OIIA 티어별 고양이 UI 바운스** — `CatMovement.cs` … **검증 완료** (2026-06-20) → **2026-07-12 삭제** (중앙 고정 · CatAnimator만)
- [x] **OIIA 티어별 슬롯 UI 흔들림** — `UiShake.cs` … **검증 완료** (2026-06-20) → **2026-07-12 글로벌 티어(`ResolveGlobalTier`) 계승** · Tick `UpdateSlotUiShake` 복구 · **Play 검증 완료**
- [x] **OIIA 점수·티어·HP 저점수 컷 밸런스** — `Balance.cs` Inspector·**2차 씬 튜닝·검증 완료** (2026-06-20)
- [x] **OIIA 성공 MP4 O/I/A 틴트** — `VideoEffects.cs` Inspector·씬 색상·**검증 완료** (2026-06-20)
- [x] **OIIA 가이드 버튼 진동** — `GuideButtonShake.cs` … **2026-06-20 검증** → **2026-06-24 폐지** (`GuideFeedback` 네온 2레이어로 대체)
- [x] **OIIA 아케이드 조작감·코믹스 연출·셔플·밸런스 개편** — C#·프리팹·**1P Play 검증 완료** (2026-06-24)
- [x] **OIIA 네온 2레이어** — `Neon_Outline` / `Neon_Shockwave`·연타 겹침·타겟 즉시 표시·**1P 검증 완료** (2026-06-24)
- [x] **OIIA BurstText·WAITING 대기 UI 숨김** — **1P 검증 완료** (2026-06-28)
- [x] **개발자 키보드 디버그 (1P)** — `DeveloperKeyboardGamepadDebug` · `BoothUsbSlotInput` · 메뉴 W/S 제거 (2026-07-10)
- [x] **OIIA Dev God Mode** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA BurstText 3티어 draw order** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA BurstText Inspector·±50·진동** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA BurstText 스윙 회전** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA BurstText 티어별 fontSize** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA BurstText 티어별 진동** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA Dev God Mode 타이머 정지** — **1P 검증 완료** (2026-06-28)
- [x] **OIIA 셔플 이펙트 Inspector** — 확대 속도·크기 **1P 검증 완료** (2026-06-28)
- [x] **OIIA 본게임→Result·HP·Fade** — **1P 통합 검증 완료** (2026-06-28)
- [x] **OIIA 셔플 이펙트** — `ShuffleEffect` 1초·**1P 검증 완료** (2026-06-28)
- [x] **에디터 Play Mode MainMenu 시작** — **1P 검증 완료** (2026-06-28)

### 진행 중 / 미완 · To-Do [ ]

**OIIA 디제잉 레이브 개편**

- [x] **1단계: UI 바인딩·데이터 구조** — `OiiaDjPadButtonId` · `DjPadButtons[10]` · Hud · SubPatternGuide · StageScreen (2026-07-11)
- [x] **1.5단계: 레거시 제거** — Burst/Shuffle/Guide/ButtonShuffle 삭제 · 게이지·문자패턴·가이드 바인딩 제거 · `TickGameplay` 스텁 · **에디터 정리·Play 검증 완료** (사용자, 2026-07-12)
- [x] **2단계: 활성 타겟 3개 + 10키 판정** — `DjActive` · `OnDjHit`/`OnDjMiss` · L/R black Highlight(Displayed 스프라이트만 흰색) · Dev God 전버튼 Highlight · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **Blur 제거** — `BlurFx`·바인딩 삭제 · WAITING만 유지 · 스포트라이트 대체 예정 · **에디터·Play 검증 완료** (사용자, 2026-07-12)
- [x] **3-A: 피버** — 패턴 12완성 진입 · 게이지=`matched/12` · 종료 시 리셋 · 전 키 정답 모드 (2026-07-12, 자동재생은 2026-07-18 제거)
- [x] **3-B: 스포트라이트** — 글로벌 티어 · L/R Fixture+Beam · Beam만 색/스트로보 · `spotlightBeamAlpha` Inspector · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **3-C: 전광판·BGM** — Chroma→Space→Club · 단일 `mainBgmClip` · 티어 27/33.5 · Beam 예고·클립 · T3 고양이 상시회전 · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **CatMovement 삭제 · UiShake 계승** — 고양이 바운스 제거 · `ResolveGlobalTier` T2+/T3×2 · Tick `UpdateSlotUiShake` · 패널 배경 투명화 제거 · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **SubPatternGuide 접두 표시** — `oiiaiooiiiai` · 12완성 시 피버 · 피버 진입/종료/미스 시 초기화 · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **스피커 시스템** — TMP 템플릿·티어 튜닝·대소문자 랜덤 · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **OIIA ABXY Unpressed 명도 100** — DjBox A/B/X/Y만 · **Play 검증 완료** (사용자, 2026-07-12)
- [x] **관중 이펙트** — 피버 상승·진동·페이드 · 에디터 Rest 위치 · `crowdShakeAmplitude` · **Play 검증 완료** (사용자, 2026-07-13)
- [x] **슬롯 패널 클리핑** — `clipSlotContentToPanel` · 패널 루트 `RectMask2D` · 이펙트가 옆 슬롯 위에 겹치지 않음 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **UiShake 티어별·StageScreen·상시** — `uiShakeTier*` + `uiShakeIdleTier*` · 씬 튜닝 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **피버 패턴 자동재생 제거** — 자동 접두 진행·스텝 SFX·타이머 제거 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **피버 입력 기반 수동 패턴** — 버튼 입력마다 OIIA 스텝 SFX·SubPatternGuide·스피커 글자 진행 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **피버 진입 직전 마지막 I SFX 누락 수정** — `AdvanceSubPatternOnHit` 진행 위치 반환 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **피버 Scream 루프 사운드** — `Scream.mp3` · 피버 슬롯 존재 동안 반복 · 마지막 피버/세션 종료 시 정지 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **글로벌 티어별 피버 개편** — T1 패턴 완성 3초 피버 · T2 패턴 반복/게이지 0/피버 없음 · T3 모든 참가 슬롯 전체 강제 피버 · **Play 검증 완료** (사용자, 2026-07-18)
- [x] **피버 OIIA 떼창 변조** — T1/T3 피버 패턴 스텝 · 씬 튜닝(레이어12·피치0.98~1.02·지연0.15) · **Play 검증 완료** (사용자, 2026-07-18)

**OIIA 후속 (레거시 루프)**

- [ ] OIIA **2~4P·부스 다패드** 재검증 (개편 후)
- [ ] `OiiaResultMinigameFlavor` Result 전용 비주얼 (C-11, 현재 ID 매칭만)
- [ ] `buzzClip`, `sessionEndClip` 연결 여부 결정 (현재 의도적 미연결)
- [ ] Result `rankRevealClip` / `hpHitClip` Inspector 오디오 (선택)

**공용 UI — SNES 컨트롤러 버튼**

- [x] SuperFamicom Face SpriteSet 4 + Prefab (`SNES_FaceButton`, `Button_A/B/X/Y`, 완제품 **`Button_ABXY`**) (사용자 에디터, 2026-07-08)
- [x] Face `*Press` 멀티프레임 C# (`pressFrames`) · 앞2 눌림/뒤2 해제 · **Play 검증 완료** (사용자, 2026-07-08)
- [x] Sprite Editor 슬라이스·SpriteSet `pressFrames` 연결 · **`Button_ABXY` Play 검증 완료** (사용자, 2026-07-08)
- [ ] (선택) 씬별 1P~4P `playerIndex` · Face `Buttons_ABXY` / D-Pad `Buttons_Direction` 다패드 교차 검증
- [x] **SNES D-Pad 드라이버 C#** — `SnesDpadButtonId` · `SnesPlayerDpadButtons` · stick/up|down|left|right (2026-07-09)
- [x] **SNES D-Pad crop 조립 단순화** — Mask/`armOverlayMode`/`BaseArm_*` 제거 (2026-07-09)
- [x] **공용 SNES D-Pad Prefab** — crop SpriteSet · **`Buttons_Direction`** · Play 검증 (사용자, 2026-07-09)
- [x] **SNES Shoulder 드라이버 C#** — `SnesShoulderButtonId` · `SnesPlayerShoulderButtons` · `ShoulderL`/`ShoulderR` (2026-07-10)
- [x] **SNES Shoulder Prefab** — `Button_L`/`Button_R` · **`Buttons_LR`** · Play 검증 (사용자, 2026-07-10)
- [x] **SnesButtonSpriteSet** — 레거시 `press` 제거 (2026-07-10)
- [x] **SNES System 드라이버 C#** — `SnesSystemButtonId` · `SnesPlayerSystemButton` · 버튼당 완제품 Prefab (2026-07-10)
- [x] **SNES System Prefab** — `Button_Start` · `Button_Select` · Play 검증 (사용자, 2026-07-10)
- [x] **SNES Controller Prefab 정리** — 완제품 5종만 · `SNES_Button`/단품 삭제 (사용자, 2026-07-10)
- [x] **SNES 2D 무애니 C#** — `instantHoldVisual` (2026-07-10)
- [ ] (2차) Full Controller

**메인·카탈로그**

- [ ] `debugRouteAllToOiia` 정책 확정 — 출시 전 false + id별 씬 분기 (C-10, **보류**)
- [ ] 카탈로그 placeholder 6종 → 실제 미니게임 (미착수)
- [ ] 메인 UI 폴리시 (아이콘·BGM·선택 강조 등, 와이어프레임 수준)

**Rhythm Button Challenge (RBC)**

- [x] RBC 씬 Module·Bootstrap MonoBehaviour 부착 + `module` 참조 (2026-06-21, 사용자 에디터)
- [x] RBC 씬 UI 골격·프리팹·Canvas (2026-06-21)
- [x] RBC Module Sprite 15종 연결 — SNES Unpressed + `RhythmButtonChallenge/Sprites` 판정 (2026-07-10)
- [x] RBC `MusicSource` + AudioClip 24 연결 (2026-07-18)
- [ ] RBC SpeedUpText·FadeOverlay·Build (`05_Rhythm_Button_Challenge.md` §16 #8~)

**운영·품질**

- [ ] 메인 슬롯 HUD vs Inspector vs 문서 교차 검증 (일지 2026-05-10 후속 메모)

---

## 7. AI 작업 체크리스트 (매 채팅 턴 · 작업 종료 시)

0. [ ] **이번 사용자 메시지 기준** 오늘 날짜를 확인했는가? (§「작업일(날짜) 확인」 — 세션 1회만 확인 **금지**)
1. [ ] **정확하지 않은 요소가 하나라도 있으면**, Plan Mode의 자체 추론·위임·기본값으로 넘기지 않고 **먼저 사용자에게 질문한 뒤 답변을 기다렸는가?** (§1 ★ 질문 선행 의무)
2. [ ] C# 변경만 수행했는가? (에디터 독단 지시 없음)
3. [ ] 에디터 연결 필요 시 §2 가이드를 **채팅에만** 제공했는가? (`01`·`05_*`에 Step-by-Step **미작성**)
4. [ ] `02_개발_진행_일지.md` 에 **확인된 오늘 날짜** 섹션을 추가·갱신했는가?
5. [ ] 영향 문서(`01`, `04`, `05_*`, `03`) 및 **본 파일 §6** To-Do를 갱신했는가? (`05_*`에 검증 체크리스트·에디터 가이드 **미추가**)

---

*마스터 파일 갱신: 2026-07-18 — 피버 OIIA 떼창 변조 Play 검증·씬 튜닝 반영*
