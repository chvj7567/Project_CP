# Content Audit — 2026-06-20 — Cat1~5 tapIndex 2 장기 누적 이정표 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-19 KST — feddcde)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39·47~51(20개) 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + collection snapshot 1종 = 5종 | Attendance/NormalStageClear/BlockDestroy/AdWatch + CatPang 생성 |
| 상점 아이템 | 12개 | 스킨 7종 + IAP 3종 + 골드 구매 아이템 2종 |
| 고양이 스킨 | 6종 × Cat1~5 = 30 EBlockState | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry, shopID 1~11에서 skinIndex 0~6 판매 |

### 분포 공백 — Mission tapIndex 2 (장기 이정표)

| collectionType | 블록 이름 | tapIndex 2 여부 | clearValue |
|---|---|---|---|
| 0~4 (Cat1~5) | 기본 고양이 | ❌ **없음** | — |
| 5~6 (Cat6~7) | 기본 고양이 (tapIndex 1도 없음) | ❌ 없음 | — |
| 10~15 (Arrow1~6) | 화살표 폭탄 | ❌ 없음 | audit #17이 tapIndex 3 제안 |
| 18 (CatPang) | 캣팡 | ❌ 없음 | audit #8 tapIndex 2 제안됨 |
| 19~23 (5색 폭탄) | 특수 색폭탄 | ❌ 없음 | audit #5 제안됨 |
| 16~17 (Wall/Potal) | 장애물 | ❌ 없음 | audit #19 제안됨 |
| 24 (Fish) | 물고기 | ✅ clearValue 31 | missionID 13 |
| 40 (CatBox1) | 고양이 상자 1 | ✅ clearValue 51 | missionID 14 |
| 45 (WallCreator) | 벽 생성기 | ✅ clearValue 71 | missionID 15 |
| 52 (RainbowPang) | 무지개팡 | ✅ clearValue 91 | missionID 16 |
| 53 (Ball) | 볼 | ✅ clearValue 131 | missionID 17 |

**핵심 발견**: tapIndex 2가 구현된 5종은 모두 특수·장애물 블록이다. 게임 핵심 메커니즘인 **기본 고양이 블록(Cat1~5)에는 tapIndex 2 이정표가 단 한 개도 없다.** Cat1~5는 tapIndex 1 반복 미션(missionID 18~22, clearValue 100, addValue 100)이 있어 100개마다 골드 100을 반복 지급하지만, 생애 1회 달성하는 장기 목표가 전무하다. 희귀 블록(Fish 31개, CatBox1 51개)보다 매 판 수십 배 더 자주 파괴하는 기본 블록에 오히려 장기 이정표가 없는 역전 현상이다.

### 과거 감사 후보 (git log 조회 결과, 24건)

| SHA | KST 날짜 | 설명 |
|---|---|---|
| feddcde | 2026-06-19 | AddTime·AddMove 아이템 보스 스테이지 완전 무효 — 보스 전용 아이템 효과 설계 부재 |
| 6bac443 | 2026-06-18 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 — Mission.json 1줄 추가로 신설 제안 |
| 4c65699 | 2026-06-17 | 하드→보스 해금 임계값 50/150 비대칭 — 보스 스테이지 조기 진입 허용 설계 재검토 |
| 3670d4f | 2026-06-16 | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 — 스킨 장착 보상 루프 신설 제안 |
| b0ee2e0 | 2026-06-15 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 — PinkBomb~BlueBomb 50개 이정표 신설 제안 |
| ee1fc5b | 2026-06-14 | 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절 제안 |
| ccb2a5d | 2026-06-12 | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 80f90cc | 2026-06-11 | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 — 게임 동명 블록 장기 목표 신설 제안 |
| 7528a26 | 2026-06-10 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 — IAP 구매자 보상 루프 단절 제안 |
| 85ac09c | 2026-06-09 | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |
| 329cce2 | 2026-06-08 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 제안 |
| 26ebd5a | 2026-06-07 | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 7d601d5 | 2026-06-06 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 — tapIndex 2 스킬 선택 미션 신설 제안 |
| 158d246 | 2026-06-05 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 — Cat1~5 구현 후 2종만 잔여 공백 |
| 1d91056 | 2026-06-04 | 상점 스킨 골드 가격 선형 계단 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 41f1f81 | 2026-06-03 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 1ea1f97 | 2026-06-02 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 — 화살표 폭탄 5회 일일 미션 추가 제안 |
| 4a09a70 | 2026-06-01 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 제안 |
| 23f1382 | 2026-05-31 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| d819b99 | 2026-05-30 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 58c3f65 | 2026-05-29 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| aeffad1 | 2026-05-29 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| b5bcc97 | 2026-05-29 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 08f1ddb | 2026-05-28 | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Cat1~5 tapIndex 2 장기 누적 이정표 미션 신설

- **카테고리**: 미션
- **요지**: 게임의 핵심 매치-3 액션인 Cat1~5 기본 블록 파괴에 생애 최초 달성형 장기 이정표(tapIndex 2)가 전혀 없다. tapIndex 1은 100개마다 반복 지급되지만 "평생 Cat1을 500개 매치했다"는 이정표가 없어 중기 이탈 구간에서 목표감이 단절된다.
- **점수**: 검증가치 4 / 구현비용 1 / 플레이어경험개선 4 / 데이터근거 5 → 종합 **18**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 18~22: Cat1~5 tapIndex 1 (collectionType 0~4, clearValue 100, addValue 100) 존재 확인. tapIndex 2 항목 중 collectionType 0~6 해당 항목 **0건** 확인.
  - `Assets/Scripts/Defines.cs` EBlockState — Cat1=0, Cat2=1, Cat3=2, Cat4=3, Cat5=4 확인.
  - `Assets/Scripts/Data.cs` Data.Collection — key=string(int), value=int 구조로 Cat1 수집량을 이미 key `"0"`으로 추적 중. 별도 신규 필드·코드 변경 없이 Mission.json 5줄 추가만으로 구현 가능.
  - 기존 tapIndex 2 (missionID 13~17): 특수·장애물 블록(Fish/CatBox1/WallCreator/RainbowPang/Ball) 5종만 이정표 보유. 기본 고양이 블록은 전무.

#### 유저 플로우 (9항목)

1. **노출 시점·트리거** — 패치 적용 후 최초 앱 실행 시 UIMission의 tapIndex 2 탭(장기 목표)에 Cat1~5 각각의 이정표 미션 5개가 자동 노출된다. 기존 Fish·CatBox1·WallCreator·RainbowPang·Ball 이정표 아래에 순서대로 추가된다. 이미 Cat1을 수백 개 이상 매치한 기존 플레이어는 첫 접속 순간 진행 바가 채워진 상태로 표시되며, clearValue 초과 시 즉시 "수령 가능" 상태로 뜬다.

2. **화면 변화** — UIMission tapIndex 2 탭에 "Cat1 블록 500개 달성", "Cat2 블록 500개 달성" 형태로 5개 항목이 추가된다. 각 항목은 현재 누적량 / 목표값(예: 237 / 500)과 진행 바를 노출한다. 달성 직전·달성 시 진행 바 색상과 "수령" 버튼 활성 여부가 전환된다. 미달성 항목은 회색, 달성 항목은 강조 색상, 수령 완료 항목은 완료 도장 표시로 구분된다.

3. **입력 행동** — 유저는 게임 보드에서 평소대로 Cat1 블록이 포함된 3매치(또는 이상)를 이어 제거한다. 별도 UI 인터랙션 없이 자동 집계된다. 진행도 확인을 원하면 UIMission → tapIndex 2 탭을 탭한다. clearValue 도달 후에는 해당 미션 항목을 탭하면 보상 수령 확인 다이얼로그(또는 즉시 수령)가 표시된다.

4. **시스템 반응** — GPMatchChecker·GPBombResolver가 Cat1 블록 파괴를 감지할 때마다 `CHMData.Instance`를 통해 `Data.Collection["0"] += 파괴 개수`가 누적된다. 누적값이 clearValue(예: 500)에 도달하면 미션 상태가 `Doing → Clear 대기`로 전환되고 UIAlarm 또는 상단 배너로 "Cat1 이정표 달성! 수령 가능" 알림이 표시된다. 보상 수령 시 골드 1,000이 지급되고 미션 상태는 `Clear`로 영구 고정된다(addValue=-1, 비반복).

5. **반복·재발생 패턴** — tapIndex 2는 1회성(addValue=-1)이므로 달성 후 재발동하지 않는다. tapIndex 1(missionID 18~22)은 완전히 독립적으로 계속 반복 진행된다. 두 미션은 동일 `Data.Collection["0"]` 값을 공유하므로 tapIndex 1(100개 단위) 달성과 tapIndex 2(500개 이정표) 달성이 같은 게임 세션에서 동시에 발생할 수 있다.

6. **종료·해소 조건** — Cat1~5 각 항목별로 누적값 ≥ clearValue 도달 후 UIMission tapIndex 2 탭에서 보상을 수령하면 해당 미션이 영구 완료 처리된다. 5개 미션 모두 완료하면 tapIndex 2 탭 내 Cat 계열 항목이 전부 완료 스탬프 상태로 변경되며 이후 별도 인터랙션 없이 고정된다.

7. **다른 시스템과 상호작용** — `Data.Collection["0"]` 값은 tapIndex 1 미션(missionID 18)과 공유 읽기된다. GPGS 클라우드 저장 및 Local 저장에 동기화되므로 기기 재설치 후에도 누적이 유지된다. 고양이 스킨 테마 블록(CatCrown1 = collectionType 54 등)으로 매치할 경우 Cat1 카운트 집계 포함 여부는 별도 설계 결정이 필요하다(현재 스킨 테마는 별도 EBlockState로 분리되어 있어 기본 Cat1과 collectionType이 다름). DailyMissionService.CheckAndResetIfNeeded()의 자정 리셋과는 무관하다(tapIndex 2는 일일 카운터 아님).

8. **엣지 케이스** — ① **기존 플레이어 소급**: 패치 적용 즉시 `Data.Collection["0"]` 값이 500 이상인 플레이어는 보상 대기 상태로 시작한다. `Data.Mission.startValue` 필드를 활용해 패치 시점 스냅샷을 찍어 소급 적용을 차단할지, 아니면 즉시 보상을 허용할지 정책 결정 필요(현재 tapIndex 2 미션들은 소급 차단 미적용). ② **Cat6·Cat7 제외**: Cat6(collectionType 5)·Cat7(collectionType 6)은 tapIndex 1 미션도 없으므로(audit #14 제안 미구현) 이번 제안에서 제외. 추후 tapIndex 1·2 동시 신설 검토 권장. ③ **중복 달성**: Cat1 tapIndex 1(missionID 18)이 clearValue 300 시점에 3회째 반복 달성될 때 Cat1 tapIndex 2도 clearValue 500 근처면 거의 동시에 두 보상 팝업이 발생할 수 있다. UI 큐 처리로 순차 표시 권장.

9. **유저 정보·피드백** — UIMission tapIndex 2 탭 내 진행 바로 현재 누적량과 목표값을 실시간 표시한다. 달성 시 상단 UIAlarm 배너로 "Cat1 블록 500개 달성! 골드 1,000 수령 가능" 메시지를 노출한다. 보상 수령 후 완료 스탬프 애니메이션으로 성취감을 강화한다. 신규 플레이어는 초반 50~100판 내 달성 가능한 clearValue를 설정해 "처음 달성하는 큰 보상" 경험을 제공해야 리텐션 효과가 최대화된다. 통계 확인을 위해 Quest 완료 이벤트에 "tapIndex2_cat1_500" 등의 애널리틱스 로그를 추가하는 것을 권장한다.

### 보류

- **Arrow1~6 tapIndex 2 장기 이정표 미션** — 종합 16 (검증가치 3 + 구현비용 5 + 플레이어경험개선 3 + 데이터근거 5). audit #17(Arrow tapIndex 3 일일 미션)과 카테고리 부분 겹침. Cat1~5 tapIndex 2보다 검증가치·경험개선이 낮아 2위.
- **보스 스테이지 클리어 일일 미션(BossStageClear EDailyCounter 신설)** — 종합 15 (검증가치 4 + 구현비용 3 + 플레이어경험개선 4 + 데이터근거 4). EDailyCounter 신설과 DailyMissionService 코드 수정이 필요해 구현비용 3. audit #20(HardStageClear)와 카테고리 부분 겹침.
- **Fish 탈출 tapIndex 3 일일 미션** — 종합 12 (검증가치 3 + 구현비용 3 + 플레이어경험개선 3 + 데이터근거 3). audit #2(Ball·Potal tapIndex 3)와 카테고리 부분 겹침.

## 3. 과거 감사 대비 차별성

git log 24건 검토 완료.

**가장 유사한 과거 커밋 2건**:
- `b5bcc97` (2026-05-29) "미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안" — 같은 블록(Cat1~5), 다른 tapIndex(**1**). 해당 제안이 수용되어 현재 missionID 18~22가 구현된 것으로 보인다.
- `158d246` (2026-06-05) "Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락" — Cat6·Cat7 tapIndex **1** 제안으로 블록 그룹 일부 겹치나, 대상(Cat6/7)과 tapIndex(1)가 모두 다르다.

**차별점 3가지**:
1. **tapIndex 자체가 다름**: b5bcc97은 tapIndex **1**(반복 누적)을 신설 제안 → 이미 구현됨. 본 제안은 tapIndex **2**(생애 1회 이정표) 신설로 완전히 다른 탭·다른 보상 로직·다른 UX다.
2. **과거 제안 수용 이후 필연적 후속 공백**: tapIndex 1이 구현된 Cat1~5에 tapIndex 2가 없는 구조적 비대칭을 처음으로 지적하는 제안이다. Fish·CatBox1 등 5개 블록은 tapIndex 2가 있으나 Cat1~5는 없다 — 기본 블록이 특수 블록보다 장기 목표가 빈약한 역전 현상이 이번에 처음 정량화됐다.
3. **게임 코어 메커니즘 최초 적용**: 기존 tapIndex 2 제안(audit #8 CatPang, audit #5 5색폭탄, audit #19 Wall/Potal)은 모두 특수·희귀 블록 대상이었다. 매 판 수십 개씩 매치되는 기본 고양이 블록을 tapIndex 2 대상으로 삼은 제안은 24건의 감사 이력 중 이번이 최초다.

## 4. 다음 단계 제안

채택 시 구체 구현 계획 수립 필요:
1. **clearValue 결정**: 헤드리스 시뮬레이션(`docs/qa-reports/`) 또는 노멀 스테이지 1판당 Cat1 평균 파괴 수 분석 후 "50~100판이면 달성 가능한" 초기값 설정. 예시: Cat1~5 각 500개(또는 빈도별 차등).
2. **신규 missionID 할당**: 현재 1~22, 100~104 사용 중 → 23~27 범위 제안. 충돌 없음.
3. **descStringID 추가**: `StringKorea.json` / `StringEnglish.json`에 설명 문자열 5건 추가.
4. **기존 플레이어 소급 적용 정책 결정**: startValue 기반 소급 차감 여부(전통적으로 현재 구조에서는 소급 허용 방향).
5. **스킨 테마 블록(CatCrown1 등)의 Cat1 집계 포함 여부** 설계 결정: 포함 시 스킨 활성 플레이어 달성 가속화, 제외 시 형평성 우선.

## 5. 쉬운 설명 (비개발자 요약)

CatPang에서 플레이어가 매일 가장 많이 하는 행동은 같은 색 고양이 블록 3개 이상을 줄 맞춰 없애는 것이다. 지금은 고양이 블록 100개를 없앨 때마다 소량의 금화를 받는 소소한 반복 보상만 있고, "평생 처음으로 500개를 달성했다!"는 큰 이정표 보상은 존재하지 않는다 — 마치 매일 조깅을 해도 "누적 100km 완주" 메달은 없는 셈이다. 그런데 아이러니하게도, 물고기·상자처럼 가끔 등장하는 특이한 블록들에는 이런 "처음 달성" 이정표 보상이 이미 있어서, 훨씬 더 자주 하는 기본 행동이 더 적은 장기 보상을 받는 이상한 역전이 일어나고 있다. 그래서 이번에 제안하는 것은: 가장 자주 만지는 기본 고양이 블록 5가지(Cat1~Cat5) 각각에 "평생 500개 없애면 금화 1,000 보상"을 Mission.json 파일 5줄만 추가해서 만들어 보자는 것이다.
