# Content Audit — 2026-07-01 — 보스 스테이지 Tutorial.json 항목 완전 부재 — 최초 진입 시 HP·보스 스킬 맥락 안내 없음

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, Guide, Tutorial, ConstValue, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 25건 (가장 최근: 2026-06-30, f9451b8)

## 1. 현황
| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 Max=84 | 빈 슬롯 25~39(15개)·47~51(5개) 제외. 스킨 30종 포함 |
| 일일 미션 종류 | EDailyCounter 4종 + 특수 1종 = 5개 | Attendance/NormalStageClear/BlockDestroy/AdWatch + 오늘 CatPang 3개 |
| 상점 아이템 | 12개 | tapIndex1(유료/골드 9개), tapIndex2(IAP 3개) |
| 고양이 스킨 | 6종 × 5 Cat = 30개 | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |
| Tutorial.json 항목 | 11개 (전부 non-boss) | tutorialStageID: 1·2·3·4·5·8·31·51·71·91·131 |
| Guide.json 항목 | 15개 | guideIndex 1~15. ConstValue상 NormalGuideMax=6, BossGuideMax=12 |
| 보스 스테이지 tutorialID | -1 (100개 전부) | Stage.json 상 tutorialID=-1 = 인게임 튜토리얼 미발동 |

### 분포 공백

#### Tutorial.json 커버리지
Tutorial.json의 11개 항목은 다음 비-보스 스테이지만 커버한다:

| tutorialStageID | 대응 스테이지 | 추정 안내 내용 | connectNextBlock |
|---|---|---|---|
| 1 | stage 1 | 시간 제한 기본 조작 | — |
| 2 | stage 2 | 이동 횟수 제한 조작 | 18 (CatPang) |
| 3 | stage 3 | 보드 크기 확대 | — |
| 4 | stage 4 | 추가 안내 | — |
| 5 | stage 5 | 추가 안내 | — |
| 8 | stage 8 | 미상 | — |
| 31 | stage 31 | Fish 블록 첫 등장 | — |
| 51 | stage 51 | CatBox 첫 등장 | — |
| 71 | stage 71 | WallCreator 첫 등장 | — |
| 91 | stage 91 | RainbowPang 첫 등장 | 18 (CatPang) |
| 131 | stage 131 | Ball 첫 등장 | 14 (Arrow5) |

**보스 스테이지(stage 100001~100100): tutorialStageID 0건. 100개 전부 tutorialID=-1.**

#### 보스 스테이지 구조 특이점
- time: -1, moveCount: -1 → 시간/이동 제한 둘 다 없음 (non-boss와 완전 다른 실패 조건)
- targetScore: 1000~2800 (비-보스 평균과 다른 목표 체계)
- blockTypeCount: 5 (100개 전부 동일 — 가장 복잡한 색 구성)
- EFailReason.HpOver → 기본 게임 EFailReason.TimeOver/MoveOver와 다른 3번째 실패 유형
- GPBossController 가동: EBossSkillType.Wall / Creator / CatBox 중 하나 이상이 매 턴 작동

Guide.json은 guideIndex 7~12가 보스 가이드(ConstValue BossStageGuideMaxIndex=12)로 추정되나, Guide는 일반적 팁 팝업(게임 진행 단계별 1회성 안내)이고 Tutorial은 스테이지 플레이 중 특정 블록·UI 요소를 가리키며 맥락 안내를 주는 별개 시스템이다. 인게임 블록 하이라이트·connectNextBlock 기능은 Tutorial만 제공한다.

### 과거 감사 후보 (git log 조회 결과, 최근 25건)
| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 — 오늘 2번 발동 일일 목표 신설 |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-27 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 시점 후기 도입 |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 스테이지 완전 부재 |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(54~83) 수집 미션 완전 공백 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 미구현 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-05-30 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### [권장] 보스 스테이지 Tutorial.json 항목 완전 부재 — 최초 진입 시 HP 시스템·보스 스킬 맥락 안내 신설
- **카테고리**: 스테이지 / 온보딩 / 튜토리얼
- **요지**: Tutorial.json에 보스 스테이지 항목이 단 1개도 없어, hardStage 50을 달성하고 처음 보스 스테이지에 진입한 플레이어가 시간·이동 제한이 없는 HP 소진 방식(EFailReason.HpOver)·보스 스킬(Wall/Creator/CatBox 자동 생성) 등 완전히 다른 게임 규칙을 설명 없이 마주한다. stage 100001 진입 시 Tutorial.json에 새 항목 1~3개를 추가해 HP 바·보스 스킬 발동·클리어 조건을 맥락 안내하는 것을 제안한다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 **17**
- **근거**:
  - `Assets/AssetBundleResources/json/Tutorial.json` — 11항목 전부 tutorialStageID ≤ 131 (non-boss)
  - `Assets/AssetBundleResources/json/Stage.json` — boss 100개 전부 `tutorialID: -1` (확인: `Boss tutorialID counter: {-1: 100}`)
  - `Assets/AssetBundleResources/json/ConstValue.json` — `variable:2, value:12` = BossStageGuideMaxIndex=12 (Guide 시스템은 보스 커버하지만 Tutorial 시스템은 비어있음을 대조)
  - `Assets/Scripts/Defines.cs` — `EFailReason`: None/TimeOver/MoveOver/**HpOver** — 보스 전용 실패 유형이 별도 존재함에도 안내 부재
  - `Assets/Scripts/GamePlay/GPBossController.cs` — 보스 AI (EBossSkillType.Wall/Creator/CatBox) 작동 로직 존재

#### 유저 플로우

1. **노출 시점·트리거**
   플레이어가 hardStage 카운터가 50에 도달해 보스 스테이지 해금 직후 처음으로 stage 100001을 선택하는 시점. 현재 Stage.json에서 stage 100001의 `tutorialID=-1`이므로 GPTutorial은 아무것도 로드하지 않는다. 새 Tutorial.json 항목(예: tutorialStageID=100001)을 추가하고 Stage.json의 tutorialID 필드를 업데이트하면, 첫 보스 스테이지 시작 직전에 안내 팝업이 발동된다.

2. **화면 변화**
   보스 스테이지 보드가 로드되면서 기존 non-boss 스테이지에는 없던 HP 바(Data.Login.hp=100 기반)와 보스 체력 UI가 나타난다. 튜토리얼 미적용 현재: 플레이어는 HP 바가 왜 있는지, 적이 왜 매 턴 벽을 생성하는지 알 수 없다. 튜토리얼 추가 시: 말풍선이 HP 바를 가리키며 "HP가 0이 되면 실패!" 안내 → 다음 탭에서 보스 AI가 생성하는 Wall/CatBox를 connectNextBlock으로 하이라이트 → 마지막 탭에서 targetScore(클리어 목표)를 가리키는 3단계 안내가 표시된다.

3. **입력 행동**
   플레이어는 말풍선을 탭하여 다음 안내 항목으로 넘어간다. 기존 stage 1~131 튜토리얼 UX와 동일하므로 새로운 인터랙션 학습이 필요 없다. 총 1~3회 탭으로 안내 완료 후 자동으로 스테이지 플레이가 시작된다.

4. **시스템 반응**
   GPTutorial.cs가 Stage.json의 tutorialID 값을 읽어 Tutorial.json에서 대응 항목을 로드한다. `connectNextBlock` 필드에 Wall(16) 또는 CatBox1(40) 등 보스 스킬 블록 ID를 지정하면 해당 블록을 화면에서 하이라이트하는 기존 연동 로직이 동일하게 동작한다. StringKorea.json / StringEnglish.json에 새 descStringID만 추가하면 i18n도 자동 처리된다.

5. **반복·재발생 패턴**
   최초 1회만 발동(기존 tutorial 패턴과 동일). 이후 같은 보스 스테이지 재도전 시에는 발동하지 않는다. 선택적 확장으로, 새 보스 그룹(100001→100002 등 10개 그룹) 첫 진입 시마다 신규 스킬 유형(Creator, CatBox 등)을 소개하는 그룹별 튜토리얼을 추가할 수 있다.

6. **종료·해소 조건**
   안내 팝업의 모든 항목을 탭 완료하면 자동 종료. Data.Login.guideIndex 필드(현재 0으로 초기화)를 활용하거나 별도 boolean 필드를 추가해 "보스 튜토리얼 완료" 상태를 저장, 재발동을 방지한다. 안내가 완료되면 보드 인터랙션이 활성화되며 플레이어는 즉시 게임을 시작한다.

7. **다른 시스템과 상호작용**
   Tutorial.json(신규 항목 추가) → Stage.json(stage 100001의 tutorialID 업데이트) → GPTutorial.cs(기존 로직으로 자동 처리) → StringKorea.json / StringEnglish.json(새 descStringID 추가). 안내 완료 여부를 Data.Login.guideIndex 또는 신규 필드에 저장 → CHMData가 로컬/클라우드 저장 담당. 추가 코드 변경 없이 JSON 데이터 수정만으로 구현 가능한 최소 경로가 존재한다.

8. **엣지 케이스**
   이미 보스 스테이지를 클리어한 기존 유저가 업데이트 후 재진입할 경우: tutorialID 체크 시점에 "이미 클리어된 스테이지 = 튜토리얼 건너뜀" 처리가 필요하다(Data.Stage.clearState를 활용 가능). tutorialStageID 값 충돌 방지: Tutorial.json 현재 최대 ID는 131이므로 새 ID를 100001 이상으로 지정하면 충돌이 없다. 다국어 누락: 새 descStringID를 StringKorea.json와 StringEnglish.json에 동시 추가해야 한다.

9. **유저 정보·피드백**
   보스 스테이지 최초 진입 실패("왜 갑자기 체력이 닳아?", "적이 왜 매 턴 벽을 놓는지 몰랐다")는 대표적 이탈 시점이다. 안내 추가 후 보스 스테이지 1판 클리어율 변화를 측정하면 튜토리얼 효과를 정량적으로 검증할 수 있다. 기존 non-boss 튜토리얼이 stage 31(Fish), 51(CatBox), 131(Ball) 첫 등장 시 유저 이탈을 줄였다는 전례를 같은 방식으로 보스에 적용한다.

### 보류
- **Potal 블록 Tutorial 부재** (Wall과 다른 Potal 전환 메커니즘 미안내): 점수 14. 구현비용 동일하나 검증가치·데이터근거가 보스 스테이지 갭보다 낮음
- **특수폭탄 조합 Tutorial 부재** (Arrow+Arrow → 색폭탄 업그레이드 미안내): 점수 13. 비직관적이나 게임 내 발견 가능성 있음
- **EBackground 시각 단조로움** (4개 배경 × 100스테이지 분량): 점수 12. 아트 에셋 추가 필요로 구현비용 높음

## 3. 과거 감사 대비 차별성
- git log 25건 검토 완료.
- 가장 유사한 과거 커밋: `85ac09c` (2026-06-09 KST) — "보스 스테이지 HP 소진·회복 루프 미설계". 해당 감사는 HP 회복 경로 부재(시스템 설계 갭)를 다뤘고, 본 감사는 그 HP 시스템 자체를 처음 마주하는 플레이어에게 안내가 없다는 온보딩 갭을 다룬다. 각도가 다르다.
- `58c3f65` (2026-05-30 KST) — "보스 스테이지 CatBox 스킬 극소 배분". 해당 감사는 보스 스킬 분포 불균형(콘텐츠 구성)을 다뤘고, 본 감사는 어떤 스킬이 등장하든 설명이 없다는 안내 부재를 다룬다. 관점이 다르다.
- 과거 25건 중 Tutorial.json 또는 Guide.json 시스템을 직접 분석한 감사는 0건.

## 4. 다음 단계 제안
- 채택 시: Tutorial.json에 tutorialStageID 100001 항목 1~3개 추가(HP 바, 보스 스킬, targetScore 안내) + Stage.json stage 100001 tutorialID 업데이트 + StringKorea/English.json 문자열 추가. 코드 변경 없이 JSON 수정만으로 최소 구현 가능.
- 검증: 보스 스테이지 첫 1회 클리어율 A/B 테스트 (튜토리얼 유/무 비교).
- 확장: 보스 그룹별(100001→100002 등) 신규 스킬 등장 시 추가 안내 엔트리 삽입 계획 수립.

## 5. 쉬운 설명 (비개발자 요약)

CatPang은 레벨이 400개(실질 기준)인 고양이 퍼즐 게임이에요. 처음 150개 레벨은 "3개 맞추기" 기본 규칙이라서 처음엔 짧은 설명 팝업이 하나씩 나와줬어요 — Fish 블록이 처음 나올 때, CatBox가 처음 나올 때처럼요. 근데 '보스 스테이지'라는 100개짜리 특별 구역에 들어가면, 갑자기 체력 막대가 생기고 적이 매 턴 방해 블록을 심어놓는 완전히 다른 게임이 펼쳐지는데 — 설명이 단 한 마디도 없어요. 마치 3년간 축구만 하다가 갑자기 "이제부터 농구 시합이에요"라는 말도 없이 농구 코트에 세워진 것과 같아요. 그래서 이번에 제안하는 것은: 보스 스테이지에 처음 들어갈 때 "이 스테이지는 체력이 0이 되면 져요", "적이 매 턴 방해블록을 놓아요" 같은 짧은 안내 팝업 1~3개를 추가하는 것이에요.
