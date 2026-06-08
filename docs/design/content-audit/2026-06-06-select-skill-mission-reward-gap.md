# Content Audit — 2026-06-06 — ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Stage / Guide / Mission / Shop / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 12건 (가장 최근: 2026-06-04 UTC = 2026-06-05 KST)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 슬롯 | 빈 슬롯 25~39·47~51(15개) 제외; 스킨 테마 30 포함 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1(9) + tapIndex 2 IAP(3) |
| 고양이 스킨 | 기본 1 + 테마 6종 | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |
| ESelect 스킬 | 6종 정의됨 | Power/Delay/Lotto/AddCat/CatPangUpgrade/Speed |

### 분포 공백

**ESelect 스킬 시스템 데이터 연계 현황:**
- `Defines.cs` — `ESelect` 6종 열거형 존재 (Power / Delay / Lotto / AddCat / CatPangUpgrade / Speed)
- `EJsonType` — `Select` 타입이 열거형에 있으나, `Assets/AssetBundleResources/json/` 하위에 **`Select.json` 파일 미존재** (9개 JSON 중 없음)
- `Mission.json` — 27개 미션 항목 중 ESelect 관련 `collectionType` 0건
- `Shop.json` — 12개 항목 중 스킬 구매/업그레이드 항목 0건
- tapIndex 2(장기 수집) 미션 5개: Fish(31) / CatBox1(51) / WallCreator(71) / RainbowPang(91) / Ball(131) — ESelect 없음
- tapIndex 3(일일) 미션 5개: Attendance / NormalStageClear / BlockDestroy / AdWatch / CatPang x3 — ESelect 없음

**결론**: ESelect 스킬 6종이 Defines.cs에 정의되어 있고 `EJsonType.Select`도 존재하지만, 미션·상점 어디에도 스킬 사용/달성 조건이 없어 **유저가 스킬을 의식적으로 다양화할 동기가 없는 상태**.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 (KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-05-31 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 |
| 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 2026-06-03 | 1d91056 | 상점 스킨 골드 가격 선형 계단 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### ESelect 게임 시작 스킬 6종 — 장기 수집 미션(tapIndex 2) 연계 신설

- **카테고리**: 미션 / 스킬 시스템
- **요지**: 게임 시작 전 선택할 수 있는 ESelect 스킬(Power/Delay/Lotto/AddCat/CatPangUpgrade/Speed) 6종이 Defines.cs에 정의되어 있으나, Mission.json·Shop.json에 해당 스킬 사용 조건을 가진 항목이 0건이다. tapIndex 2(장기 수집) 미션에 스킬별 사용 카운터 조건을 추가해 스킬 다양화를 유도하면 장기 리텐션 목표감이 생긴다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/3/4/4 → 종합 **15**
- **근거**:
  - `Assets/Scripts/Defines.cs` L205~212 — `ESelect { Power, Delay, Lotto, AddCat, CatPangUpgrade, Speed }` 6종 정의
  - `Assets/Scripts/Defines.cs` L12 — `EJsonType.Select` 존재하나 `Assets/AssetBundleResources/json/` 에 `Select.json` 미존재
  - `Assets/AssetBundleResources/json/Mission.json` — 전체 27행 중 ESelect 관련 `collectionType` 항목 0건
  - `Assets/AssetBundleResources/json/Shop.json` — 전체 12행 중 스킬 항목 0건

#### 유저 플로우

1. **노출 시점·트리거**
   유저가 노멀 스테이지 20단계 이상 진입한 시점부터 UIMission의 tapIndex 2 탭에 "특정 스킬을 N회 선택하라"는 새 미션 카드가 노출된다. 스킬 종류별(6종) 각각 1개씩 최대 6개의 카드가 단계적으로 잠금 해제되어 노출된다.

2. **화면 변화**
   UIMission tapIndex 2 탭 하단에 ESelect 스킬 아이콘을 가진 미션 카드가 추가된다. 현재 clearValue 31(Fish)~131(Ball) 범위의 기존 5개 카드 뒤에 배치되며, 스킬 아이콘과 "N회 선택" 진행 바가 함께 표시된다.

3. **입력 행동**
   유저는 게임 시작 전 UIGameStart 스킬 선택 화면에서 6종 중 원하는 스킬 1개를 고른다. 선택 후 게임 플레이를 완료하면 해당 스킬의 선택 횟수 카운터가 1 증가한다. 유저는 특정 스킬 미션 달성을 위해 평소 쓰지 않던 스킬을 선택하는 행동 변화가 생긴다.

4. **시스템 반응**
   게임 종료(클리어·실패 무관) 시 스킬 선택 이벤트를 기록해 `Data.Collection`의 해당 ESelect 키 카운터를 1 증가시킨다. Mission 시스템은 `collectionType = (int)ESelect.Power` 형식으로 카운터를 참조하며, clearValue(예: 10회) 도달 시 미션 완료 처리 후 Gold 보상을 지급한다.

5. **반복·재발생 패턴**
   tapIndex 2 미션은 단일 달성형(`addValue = -1`)으로 설계해 첫 10회 선택 시 1회 보상 지급한다. 스킬 6종이 각각 독립 미션으로 존재하므로 유저는 총 6회의 달성 보상 기회를 갖는다. 모든 스킬 미션을 완료한 유저에게는 "스킬 마스터" 칭호 또는 추가 보상 미션을 tapIndex 2에 추가하는 확장도 가능하다.

6. **종료·해소 조건**
   각 스킬별 clearValue 도달 시 해당 미션 카드가 완료 처리된다. `EReward.Gold`와 함께 선택적으로 AddMove 아이템 1개를 추가 보상으로 제공하면 스킬 미션의 가치가 기존 tapIndex 2 수집 미션(보상: Gold 1000)과 균형을 맞춘다. 6종 미션 모두 완료 후에는 더 이상 ESelect 관련 tapIndex 2 미션이 표시되지 않는다.

7. **다른 시스템과 상호작용**
   UIGameStart의 스킬 선택 UI 이벤트가 `CHMData`의 `Collection` 저장 구조와 연결되어야 한다. Mission.json에 행을 추가(`collectionType: 200~205` 신규 범위 부여)하고 CHMData가 해당 ESelect 카운터를 Collection 데이터로 추적하도록 수정한다. 기존 tapIndex 2 UI 렌더링 코드는 변경 없이 JSON 행 추가만으로 카드가 노출된다.

8. **엣지 케이스**
   유저가 매번 동일한 스킬(예: Power)만 선택하면 나머지 5종 스킬 미션이 진척되지 않는다. 이를 방지하려면 UIMission tapIndex 2에서 "가장 낮은 카운터 스킬"을 진행 바 강조 표시로 추천하거나, UIGameStart 스킬 선택 화면에 미션 달성 현황 배지를 노출해 유도한다. 또한 보스 스테이지에서도 스킬 선택이 발생하는 경우 해당 카운터도 집계하는지 정책 결정이 필요하다.

9. **유저 정보·피드백**
   미션 카드 내 진행 바(예: "3/10회")와 스킬 아이콘이 함께 표시되어 유저가 어떤 스킬을 얼마나 더 쓰면 보상을 받는지 한눈에 파악한다. UIGameStart 스킬 선택 화면에서 각 스킬 아이콘 우측 상단에 미션 진행률 배지(예: 소형 원형 프로그레스)를 노출하면 스킬 선택 동기가 강화된다. 최초로 스킬 미션 카드가 해금될 때 UIMission 탭 아이콘에 뱃지 알림이 표시되어 신규 콘텐츠 발견을 유도한다.

### 보류

- **Fish 블록 tapIndex 3 일일 미션 공백** — 종합 12. 일일 미션 카테고리(tapIndex 3)는 이전 5건이 이미 다뤄 포화 위험.
- **CatBox2·3·5 tapIndex 2 미션 공백** — 종합 11. 2026-05-29 audit(aeffad1)에서 CatBox4·PotalCreator tapIndex 2 미션 공백을 다뤄 카테고리(tapIndex 2, CatBox) 중복.

## 3. 과거 감사 대비 차별성

git log 12건 검토 완료. **ESelect 스킬 시스템을 직접 대상으로 삼은 과거 커밋 없음**. 가장 유사한 과거 커밋: `1ea1f97` (2026-06-01, Arrow 폭탄 tapIndex 3 일일 미션 누락) — 모두 '블록/폭발 이벤트'를 조건으로 한 미션이고, 이번 제안은 '스킬 선택 행동'을 조건으로 한 미션이라 근본 대상이 다르다. 차별점: 과거 audit은 블록 타입(EBlockState) 기반 수집 미션 공백에 집중했으나, 이번 audit은 게임 시작 전 선택 행동(ESelect) 기반 연계 공백이라 시스템 레이어가 다르다.

## 4. 다음 단계 제안

- 채택 시: UIGameStart 스킬 선택 이벤트 → CHMData Collection 카운터 연동 설계 → Mission.json 행 추가(collectionType 신규 범위 정의) → UIMission tapIndex 2 미션 카드 노출 확인
- 우선 검토 항목: Select.json 파일 부재 원인 파악 (미구현 vs 삭제된 기능 잔재), ESelect가 GPGameScene에서 실제로 사용되는 경로 확인

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 게임을 시작하기 전에 특별한 능력(스킬) 6가지 중 하나를 고를 수 있는 기능이 있다. 마치 RPG 게임에서 직업을 고르는 것처럼, "빠르게 진행", "폭발 강화" 같은 선택지가 준비되어 있다. 그런데 지금은 어떤 스킬을 골라도 미션 달성이나 보상에 아무런 차이가 없어서, 대부분의 유저가 항상 같은 스킬만 반복 선택한다. 그래서 이번에 제안하는 것은: 각 스킬을 10번 써보면 골드 보상을 받는 미션을 추가해, 유저가 다양한 스킬을 경험하고 게임을 더 오래 즐기도록 유도하는 것이다.
