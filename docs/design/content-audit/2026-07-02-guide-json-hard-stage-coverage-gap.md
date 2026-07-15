# Content Audit — 2026-07-02 — Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 13~15 고아 항목

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-30 UTC / 2026-07-01 KST)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39(15개)·47~51(5개) = 20개 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + 컬렉션 기반 1종 | Attendance / NormalStageClear / BlockDestroy / AdWatch + CatPang 일일 발동 |
| 상점 아이템 | 12개 | tapIndex 1(스킨 8종 포함) / tapIndex 2(광고제거·AddTime·AddMove) |
| 고양이 스킨 테마 | 6종 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |
| Guide.json 항목 | 15개 (guideIndex 1~15) | ConstValue 기준 최대 도달 가능 인덱스=12 → 13~15 고아 |
| ConstValue 가이드 상수 | 2개 (Normal=6, Boss=12) | Hard 스테이지 대응 상수 없음 |

### 분포 공백

Guide.json은 15개의 진행형 가이드 항목(guideIndex 1~15)을 가지며, ConstValue.json은 두 개의 가이드 최대 인덱스 상수를 정의한다.

- `NormalStageGuideMaxIndex` (variable=1) = **6** → 노멀 스테이지 가이드 1~6 커버
- `BossStageGuideMaxIndex` (variable=2) = **12** → 보스 스테이지 가이드 7~12 커버

공백 1 — **하드 스테이지(150개) 가이드 전무**: `HardStageGuideMaxIndex` 상수가 ConstValue.json에 없다. 하드 스테이지는 시간 제한이 있고 노멀 대비 이동 횟수가 절반으로 줄지만, 첫 진입 플레이어에게 이 규칙 변화를 알려주는 가이드 단계가 존재하지 않는다.

공백 2 — **guideIndex 13~15 고아 항목**: Guide.json에 `guideIndex` 13·14·15 (descStringID 71·72·73)가 존재하나, ConstValue에서 참조하는 최대 인덱스가 12이므로 게임 내에서 절대 노출되지 않는다. 3개 항목이 데이터로만 존재하는 완전한 데드 콘텐츠 상태다.

공백 3 — **Hard 전환 순간 안내 없음**: 노멀 → 하드 해금(150스테이지 완료) 직후 첫 하드 스테이지 진입 시, 규칙 변경(시간제한 ON, 이동 횟수 정상화)을 안내하는 팝업·가이드 경로가 없다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜(KST) | 커밋 SHA | 주요 설명 |
|---|---|---|
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 — 100개 전부 tutorialID=-1 |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 — 오늘 2번 발동 목표 신설 제안 |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 — stage 31 등장 블록 중간 목표 신설 |
| 2026-06-28 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 87% 시점 후기 도입, 19개 스테이지 집중 |
| 2026-06-27 | fcebcd0 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 — 화살표 폭탄 생애 이정표 신설 |
| 2026-06-26 | 3009996 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 스테이지 완전 부재 |
| 2026-06-25 | 6d92d88 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-24 | 962c93d | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-23 | f58d02d | 후반 스테이지 moveCount 1~100 100배 격차 — stage 137 이동 100회·목표 900점 |
| 2026-06-22 | 5f57450 | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-21 | feddcde | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-20 | 6bac443 | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-19 | 4c65699 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-18 | 3670d4f | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-17 | b0ee2e0 | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 |
| (이하 9건 생략 — 2026-06-14 ~ 2026-05-28) | … | … |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 13~15 고아 항목

- **카테고리**: 온보딩 / 게임 모드
- **요지**: ConstValue.json에 Normal(max=6)과 Boss(max=12) 가이드 상수만 존재하며 Hard 스테이지용 가이드 상수가 없다. Guide.json의 guideIndex 13~15 (3항목)는 어느 상수로도 도달 불가한 고아 상태로, 하드 스테이지 150개 전체가 가이드 공백 구간이다.
- **점수**: 검증가치 5 / 구현비용 2 / 플레이어경험개선 4 / 데이터근거 5 → 종합 **18**
- **근거**:
  - `Assets/AssetBundleResources/json/Guide.json` — guideIndex 1~15 (15항목), guideIndex 13·14·15의 descStringID = 71·72·73
  - `Assets/AssetBundleResources/json/ConstValue.json` — variable 1(=6, Normal guide max) / variable 2(=12, Boss guide max). **Hard 가이드 MaxIndex 상수 전무**
  - `Assets/Scripts/Defines.cs` EConstValue — `NormalStageGuideMaxIndex=1`, `BossStageGuideMaxIndex=2`. HardStageGuideMaxIndex enum 항목 없음
  - `Assets/Scripts/Data.cs` Data.Login — `guideIndex` 필드 존재. 현행 코드 최대 도달값=12

#### 유저 플로우

1. **노출 시점·트리거**  
   현재 가이드는 `Data.Login.guideIndex`가 `NormalStageGuideMaxIndex(6)` 이하일 때 노멀 스테이지 진입 시마다 순서대로 노출된다. 노멀 가이드 완료(guideIndex≥6) 후 하드 스테이지로 전환하면 더 이상 가이드 트리거가 없다. 제안: 하드 스테이지 첫 진입 시(`Data.Login.hardStage==0 → 1`) `HardStageGuideMaxIndex` 범위(guideIndex 13~15)의 가이드를 순서대로 표시한다.

2. **화면 변화**  
   기존 가이드 팝업(UIAlarm 또는 전용 오버레이)이 게임 화면 위에 반투명 배경과 함께 나타난다. 하드 스테이지 안내 가이드가 guideIndex 13·14·15 순서대로 표시된다. 노멀과 동일한 가이드 UI 패턴을 재사용하므로 신규 화면 설계가 불필요하다.

3. **입력 행동**  
   유저가 가이드 팝업을 탭하거나 닫기 버튼을 누르면 다음 가이드(또는 게임 시작)로 진행된다. 기존 노멀·보스 가이드와 완전히 동일한 인터랙션 패턴을 따른다. 가이드 3단계를 모두 닫으면 `Data.Login.guideIndex`가 15로 갱신되고 저장된다.

4. **시스템 반응**  
   `guideIndex`가 `HardStageGuideMaxIndex(15)`에 도달하면 하드 스테이지 가이드 완료로 판정되며, 이후 하드 스테이지 재진입 시 가이드가 재표시되지 않는다. ConstValue.json에 `{"variable": 7, "value": 15}` 항목을 추가하고 EConstValue에 `HardStageGuideMaxIndex = 7`을 추가하면 기존 로직과 동일한 방식으로 처리 가능하다.

5. **반복·재발생 패턴**  
   가이드는 `guideIndex` 조건을 충족할 때 1회만 노출된다(재표시 없음 — 노멀·보스 가이드와 동일). 하드 스테이지 150개 중 첫 번째 진입에서만 발동된다. guideIndex 13·14·15 각각이 하드 스테이지 진입 직후 순서대로 1회씩 노출된다.

6. **종료·해소 조건**  
   guideIndex가 15(HardStageGuideMaxIndex)에 도달하면 더 이상 가이드가 노출되지 않는다. Data.Login.guideIndex=15가 GPGS 클라우드 저장에 포함되므로 재설치 후에도 재노출이 없다. 기존 유저 중 하드 스테이지를 이미 진행 중인 경우(`hardStage≥1 && guideIndex<13`)는 다음 하드 진입 시 가이드를 소급 제공하고, `guideIndex≥13`이면 스킵한다.

7. **다른 시스템과 상호작용**  
   `ConstValue.json` — `{"variable": 7, "value": 15}` (HardStageGuideMaxIndex) 추가. `Assets/Scripts/Defines.cs` EConstValue enum — `HardStageGuideMaxIndex = 7` 추가. `Data.Login.guideIndex` 필드 — 기존 필드 그대로 활용하며 최대값이 15로 확장됨. `Guide.json` guideIndex 13·14·15(descStringID 71·72·73)의 문자열을 하드 스테이지 규칙 설명 내용으로 확인 및 수정.

8. **엣지 케이스**  
   현재 최대 도달값이 12이므로 guideIndex≥13 저장 데이터는 존재하지 않지만, 데이터 부정합 방어로 guideIndex>15이면 가이드 스킵 처리. 노멀 가이드 미완(guideIndex<6) 유저는 HardStage_NormalStageLock=150으로 인해 하드 스테이지 진입 자체가 불가하므로 노멀 가이드 미완→하드 가이드 진입 경우가 발생하지 않는다. GPGS 클라우드 동기화 시 서버 guideIndex와 로컬 guideIndex 중 큰 값을 채택하는 기존 정책이 그대로 적용된다.

9. **유저 정보·피드백**  
   하드 스테이지 첫 진입 유저에게 즉각적 피드백 제공: 규칙 변경 요약("시간 제한이 있어요!", "이동 횟수가 더 빡빡해요!" 등)과 함께 기대감 조성. 유저가 가이드 문구를 읽고 닫으면 게임이 시작되므로 흐름 방해가 최소화된다. Guide.json descStringID 71·72·73의 실제 문자열(StringKorea.json 참조)을 하드 스테이지 맥락에 맞게 확인·수정하면 구현 완료다.

### 보류

- **EBackground 4종 해금 시스템 부재** (종합 16): EBackground enum 4종이 있지만 상점·미션 어디에도 배경 해금·선택 항목이 없음. 과거 2026-06-12, 06-21, 07-01 감사에서 차순위 보류 기록. 새 아트 에셋 없이 선택 UI만 추가 가능하나 Shop UI 변경 범위가 있어 구현비용 3으로 평가, 이번에도 보류.
- **Data.Login.attack 미사용 필드** (종합 13): attack=0 기본값이 현행 게임 어디에도 연결되지 않음. 신규 공격력 시스템 설계가 필요해 구현비용이 높음.
- **Fish 블록 tapIndex 3 일일 미션 공백** (종합 14): Fish tapIndex 1 반복 미션은 2026-06-29에 다뤘으나 tapIndex 3(오늘 Fish 탈출 N회) 일일 미션은 미다뤄짐. 유사 패턴(CatBox·Ball·RainbowPang tapIndex 3) 다수 기존 감사가 있어 차별성 약함.

## 3. 과거 감사 대비 차별성

git log 24건 검토 완료. 가장 유사했던 과거 커밋: `013d928` (2026-07-01 KST — 보스 스테이지 Tutorial.json 항목 완전 부재). Tutorial.json은 스테이지별 1:1 페어 개별 튜토리얼 데이터인 반면, **Guide.json은 `Data.Login.guideIndex` 카운터 기반 진행형 누적 가이드 시스템**으로 별개의 시스템이다. 또한 이번 주제는 기 존재하는 guideIndex 13~15 항목이 완전한 고아 상태라는 데이터 불일치 이슈를 포함하며, Hard 스테이지 전체(400스테이지 중 37.5%) 온보딩 공백을 다룬다.

## 4. 다음 단계 제안

- `ConstValue.json`에 `{"variable": 7, "value": 15}` 추가 (HardStageGuideMaxIndex)
- `Assets/Scripts/Defines.cs` EConstValue enum에 `HardStageGuideMaxIndex = 7` 추가
- Guide.json guideIndex 13~15의 descStringID 71·72·73 → StringKorea.json에서 해당 문자열 확인 및 하드 스테이지 안내 내용으로 수정 또는 재활용 여부 결정
- 게임 로직(LBLobbyScene 또는 GPGameScene)에서 하드 스테이지 첫 진입 시 guideIndex 13~15 순서 재생 트리거 추가
- 기존 유저 마이그레이션: `hardStage≥1 && guideIndex<13`인 유저는 다음 하드 진입 시 소급 가이드 노출

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 처음 게임을 시작하는 사람에게 규칙을 차례대로 알려주는 "가이드 카드" 시스템이 있다. 쉬운 스테이지(노멀)와 가장 어려운 스테이지(보스)에는 이 카드가 잘 연결되어 있지만, 그 중간인 "하드 스테이지" 150개에는 아무런 안내 카드도 없다. 하드 스테이지는 갑자기 시간 제한이 생기고 이동 횟수도 빡빡해지는데, 아무 설명 없이 진입하면 유저가 당황해서 그냥 게임을 닫아버릴 수 있다. 게다가 게임 안에 이미 만들어져 있지만 한 번도 유저에게 보여진 적 없는 가이드 카드가 3장 (13번·14번·15번)이나 있다. 그래서 이번에 제안하는 것은: 하드 스테이지에 처음 진입하는 순간, 그 잠들어 있던 3장의 카드를 꺼내 "지금부터 하드 모드예요! 이렇게 달라졌어요"라고 알려주는 것이다.
