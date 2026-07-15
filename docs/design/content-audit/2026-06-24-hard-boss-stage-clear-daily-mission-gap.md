# Content Audit — 2026-06-24 — HardStageClear·BossStageClear 일일 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-23 KST / commit 6d92d88)

---

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 Max 84 | 빈 슬롯 25~39(15개), 47~51(5개) 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + collectionType 기반 1종 = 5항목 | Attendance·NormalStageClear·BlockDestroy·AdWatch + CatPang daily |
| 상점 아이템 | 12개 | 고양이 스킨 7개 · IAP 3개(RemoveAD/AddTime/AddMove) · 골드 아이템 2개(5,000씩) |
| 고양이 스킨 | 6종 테마 × Cat1~5 = 30개 EBlockState | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |

### EDailyCounter 현황

| 값 | 이름 | Data.Login 필드 | 설명 |
|---|---|---|---|
| 0 | Attendance | attendanceTodayDone | 오늘 출석 완료 여부 |
| 1 | NormalStageClear | stageClearCountToday | 오늘 노멀 스테이지 클리어 횟수 |
| 2 | BlockDestroy | blockDestroyCountToday | 오늘 블록 파괴 개수 |
| 3 | AdWatch | adWatchCountToday | 오늘 보상형 광고 시청 횟수 |
| — | (없음) | (없음) | 오늘 하드 스테이지 클리어 횟수 |
| — | (없음) | (없음) | 오늘 보스 스테이지 클리어 횟수 |

### 분포 공백

- **HardStageClear·BossStageClear 일일 카운터 = 0개**: `Defines.EDailyCounter`에 하드·보스 스테이지 클리어 전용 카운터 값이 없다. `Data.Login.stageClearCountToday`는 주석에 명시적으로 "오늘 노멀 스테이지 클리어 횟수 (재클리어 포함)"라고 기록되어 있어 하드/보스 클리어는 집계 대상이 아니다.
- 게임에는 3개의 독립적인 진행 경로(노멀/하드/보스)가 있고, 각각 `Data.Login.normalStage`, `hardStage`, `bossStage`로 추적되고 있으나, 일일 미션은 오직 노멀 스테이지 클리어만 측정한다.
- `ConstValue.json` variable 3(HardStage_NormalStageLock=150)·variable 4(BossStage_HardStageLock=50)에 따라 노멀 150개를 모두 클리어해야 하드 스테이지에 진입 가능하다. 하드·보스 플레이어는 이미 상당한 진행도를 보인 고급 유저인데, 그들을 위한 전용 일일 목표가 없다.
- `Mission.json` tapIndex 3 항목 5건 전부 조회: dailyCounter 0/1/2/3 및 collectionType 18. HardStageClear·BossStageClear에 해당하는 행 없음.

### 과거 감사 후보 (git log 조회 결과)

| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 — 보스 스테이지 보상 루프 신설 제안 |
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 — 난이도 역전 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(54~83) 수집 미션 완전 공백 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-06-01 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### HardStageClear·BossStageClear 일일 미션 신설 — 하드·보스 모드 전용 일일 보상 루프 부재

- **카테고리**: 미션 (tapIndex 3 일일)
- **요지**: `EDailyCounter`에 하드·보스 스테이지 클리어 카운터가 없어, 하드/보스 모드를 주로 플레이하는 고급 유저가 매일 달성 가능한 전용 일일 목표를 가질 수 없다. `NormalStageClear(dailyCounter:1)` 패턴을 그대로 복제해 `HardStageClear(4)`·`BossStageClear(5)` 카운터와 `Data.Login` 필드를 추가하면 구현 규모는 코드+데이터 합산 소규모다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/3/5/5 → 종합 **17**
  - 검증가치 4: 고급 유저(노멀 150 클리어 이후 진입) 리텐션 효과 측정 가능. D7/D30 고급 유저 retention gap 가설 검증 실험 설계 가능.
  - 구현비용 3: `EDailyCounter` 값 추가 + `Data.Login` 카운터 필드 2개 추가 + `DailyMissionService.OnStageClear` 분기 확장 + `Mission.json` 1~2행. NormalStageClear 구현 패턴 그대로 복제이나 코드 변경이 포함됨.
  - 플레이어경험개선 5: 노멀을 150개 이상 클리어한 고급 유저가 매일 해금 경로에 맞는 목표를 받지 못하는 명백한 경험 공백 해소. 하드/보스 전용 플레이어의 일일 루프 신규 생성.
  - 데이터근거 5: `Defines.EDailyCounter` 코드·주석, `Data.cs Data.Login.stageClearCountToday` 주석, `Mission.json` missionID 101 모두 직접 확인. 공백이 JSON 1개·코드 2파일에 걸쳐 삼중으로 증명됨.
- **근거**:
  - `Assets/Scripts/Defines.cs` EDailyCounter — `NormalStageClear = 1`만 존재. HardStageClear·BossStageClear 값 없음.
  - `Assets/Scripts/Data.cs` Data.Login — `stageClearCountToday` 주석: "오늘 노멀 스테이지 클리어 횟수 (재클리어 포함)". 하드·보스 전용 일일 카운터 필드 없음.
  - `Assets/AssetBundleResources/json/Mission.json` missionID 101 — `"dailyCounter":1, "clearValue":3` = 노멀 스테이지 3회 클리어. 하드·보스 전용 tapIndex 3 행 없음.
  - `Assets/AssetBundleResources/json/ConstValue.json` variable 3 = 150(하드 해금), variable 4 = 50(보스 해금) — 하드 진입자는 최소 노멀 150 클리어 완료자, 보스 진입자는 추가로 하드 50 클리어 완료자. 고급 유저 정의가 명확함.

#### 유저 플로우

1. **노출 시점·트리거**: 노멀 스테이지를 150개 이상 클리어해 하드 모드를 해금한 플레이어가 앱을 실행하고 UIMission 일일 탭을 열면, 기존 5개 일일 항목 아래에 "오늘 하드 스테이지 1회 클리어하기" 미션이 추가로 표시된다. 보스 모드 해금 플레이어(하드 50 클리어 이상)에게는 "오늘 보스 스테이지 1회 클리어하기" 미션도 함께 표시된다. DailyMissionService.CheckAndResetIfNeeded()가 자정 기준으로 카운터를 리셋한 상태다.

2. **화면 변화**: UIMission 일일 탭 미션 목록에 새 행이 추가된다. "하드 클리어 0/1" 또는 "보스 클리어 0/1" 형태의 진행도 바와 보상(예: 골드 300 또는 500)이 표시된다. 하드 미션 해금 조건(normalStage >= 150)과 보스 미션 해금 조건(hardStage >= 50)을 충족하지 못한 플레이어에게는 해당 행이 잠금 상태로 표시되거나 비표시 처리될 수 있으며, 이는 UX 정책 결정이 필요한 항목이다.

3. **입력 행동**: 플레이어는 스테이지 선택 화면(UIStageSelect)에서 하드 또는 보스 탭을 선택한 후 스테이지를 골라 플레이한다. 하드 스테이지는 시간 제한 없음 + 이동 횟수(ConstValue variable 5·6 기반) 조건으로 진행되고, 보스 스테이지는 HP 드레인(매초 -1) + 목표 점수 조건으로 진행된다. 스테이지 클리어 판정(GPGameScene EGameState.GameClear)이 발생하는 순간 DailyMissionService의 신규 hook가 호출된다.

4. **시스템 반응**: DailyMissionService의 신규 hook가 `Data.Login.hardStageClearCountToday` 또는 `bossStageClearCountToday` 카운터를 1 증가시킨다. UIMission이 구독 중인 ReactiveProperty(또는 다음 탭 진입 시 조회)를 통해 진행도 바가 갱신된다. clearValue(1)를 충족하면 미션 행이 "완료" 상태로 전환되고, 수령 버튼이 활성화된다. `CHMData.SaveData()`로 로컬에 즉시 저장된다.

5. **반복·재발생 패턴**: NTP 기준 자정마다 DailyMissionService.CheckAndResetIfNeeded()가 `hardStageClearCountToday`·`bossStageClearCountToday`를 0으로 리셋한다. clearValue를 1로 설정하면 하드/보스 스테이지 1회 플레이로 매일 달성 가능해, 일일 루프로서 과도한 부담 없이 매일 반복된다. 재클리어 포함 정책을 `stageClearCountToday`와 통일하면 이미 클리어한 스테이지도 카운트된다.

6. **종료·해소 조건**: 각 카운터가 clearValue(1 또는 설계값)에 도달하면 UIMission에서 "완료" 상태가 되고 보상 골드를 수령할 수 있다. 수령 후 해당 행은 완료 스타일로 고정되며, 다음 자정에 리셋되어 다시 도전 가능 상태로 돌아온다. 미수령 상태로 자정이 지나면 기존 NormalStageClear 정책과 동일하게 리셋되어 이전 보상은 소멸한다.

7. **다른 시스템과 상호작용**: `GPGameScene.EGameState.GameClear` → `ESelectStage.Hard(1)` 또는 `ESelectStage.Boss(2)` 분기 → `DailyMissionService.OnHardStageClear/OnBossStageClear` → `Data.Login.hardStageClearCountToday/bossStageClearCountToday` 증가 → `CHMData.SaveData()` → UIMission 진행도 갱신 → 보상 골드 지급. `ConstValue.json`의 해금 임계값(variable 3·4)과 연동해 해금 조건 미충족 플레이어에게는 미션 행을 숨기거나 잠금 처리할 수 있다.

8. **엣지 케이스**: ① 하드 스테이지 50개 미클리어 상태(보스 미해금)인 플레이어에게 보스 일일 미션이 표시되면 달성 불가 상태가 발생 — 해금 조건 확인 후 표시/잠금 처리 필요. ② 재클리어 허용 여부 — `stageClearCountToday` 주석이 "재클리어 포함"이므로 같은 정책 적용 권장. ③ NTP 미수신 시 DailyMissionService 리셋 불발 — 기존 정책 그대로 안전. ④ HardStageClear와 BossStageClear를 동시 신설 vs 순차 도입 — MVP로는 HardStageClear 1개만 먼저 테스트하고 보스 클리어는 데이터 확인 후 추가 권장.

9. **유저 정보·피드백**: 미션 달성 시 기존 UIMission 달성 이펙트(UIAlarm 또는 UIMission 팝업)가 재사용된다. "하드/보스 스테이지를 클리어하면 매일 골드를 받을 수 있다"는 인과를 체감하며, 하드/보스 모드 입문 후 이탈하기 쉬운 전환 구간(노멀 150 클리어 직후)에서 하루 목표가 생겨 고급 유저 유지에 기여할 수 있다. 완료 후 UIMission 알림 배지가 노출되어 다음 접속 시 보상 수령을 자연스럽게 유도한다.

### 보류 후보

| 후보 | 카테고리 | 점수 | 보류 이유 |
|---|---|---|---|
| Cat6·Cat7 tapIndex 2 이정표 미션 공백 | 미션 tapIndex 2 | 14 | 2026-06-04(Cat6·Cat7 tapIndex1)·2026-06-20(Cat1~5 tapIndex2) 두 감사의 교차 — 카테고리가 일부 겹침 |
| Fish 탈출 tapIndex 3 일일 미션 공백 | 미션 tapIndex 3 | 14 | 2026-06-17(Ball 탈출·Potal 전환) 감사와 "탈출 블록 일일 미션" 카테고리 동일; Fish 의도적 조작 어려워 달성 경험 불쾌 가능성 높음 |

---

## 3. 과거 감사 대비 차별성

- git log 24건 검토 완료.
- **가장 유사한 과거 커밋 1**: `f58d02d` (2026-06-21) "스테이지 진행 이정표 보상 완전 부재" — 차별점: 그 감사는 normalStage/hardStage/bossStage 필드를 tapIndex 2 이정표(장기 누적 1회) 미션과 연결하는 문제였음. 본 감사는 tapIndex 3 일일 미션(자정 리셋, 매일 도전)에서 하드·보스 클리어 카운터 자체가 없다는 구조적 공백으로, 측정 레이어(일회성 이정표 vs 매일 반복 목표)와 데이터 경로(cumulative progress vs daily counter)가 근본적으로 다르다.
- **가장 유사한 과거 커밋 2**: `6d92d88` (2026-06-23) "CatBox 완성 tapIndex 3 일일 미션 완전 공백" — 차별점: 그 감사는 보스 스테이지 내 특정 메커니즘(CatBox 완성 행위)에 일일 목표가 없다는 것이었음. 본 감사는 하드·보스 스테이지 자체의 클리어 이벤트를 집계하는 EDailyCounter 값 자체가 없다는 더 상위 레이어 공백이다.
- 카테고리(tapIndex 3 일일 미션) × 대상(HardStageClear 또는 BossStageClear 카운터) 교차는 24건 중 없음 → 중복 없음 확인.

---

## 4. 다음 단계 제안

- 채택 시:
  1. `Assets/Scripts/Defines.cs` EDailyCounter에 `HardStageClear = 4`, `BossStageClear = 5` 추가
  2. `Assets/Scripts/Data.cs` Data.Login에 `hardStageClearCountToday = 0`, `bossStageClearCountToday = 0` 추가 (주석 포함)
  3. `DailyMissionService` — `OnStageClear` 분기에 `ESelectStage` 모드별 카운터 업데이트 hook 추가, `CheckAndResetIfNeeded` 리셋 로직 확장
  4. `Assets/AssetBundleResources/json/Mission.json` 추가 예시:
     ```json
     {"missionID":"105", "tapIndex":"3", "descStringID":<신규ID>, "collectionType":-1,"dailyCounter":4, "clearValue":1, "addValue":0, "reward":0, "rewardCount":300}
     {"missionID":"106", "tapIndex":"3", "descStringID":<신규ID>, "collectionType":-1,"dailyCounter":5, "clearValue":1, "addValue":0, "reward":0, "rewardCount":500}
     ```
  5. StringKorea.json / StringEnglish.json에 "오늘 하드 스테이지 1회 클리어하기" / "Clear 1 Hard Stage today" 등 문자열 추가
  6. 하드·보스 미해금 유저에게 미션 행 표시 여부 UX 결정 필요

---

## 5. 쉬운 설명 (비개발자 요약)

매치-3 퍼즐 게임 CatPang은 "일반 모드"를 150판 클리어해야 더 어려운 "하드 모드"를, 하드 모드를 50판 더 클리어해야 "보스 모드"를 열 수 있다. 즉 하드·보스 플레이어는 이미 수백 판을 해온 고수 유저들이다. 그런데 지금은 "오늘 일반 스테이지 3번 클리어하기"라는 하루 목표는 있어도, 하드나 보스 스테이지를 클리어하면 보상을 주는 하루 목표가 전혀 없다. 고수 유저들이 어렵고 재미있는 스테이지를 열심히 해도 매일 받을 수 있는 특별 보상이 없는 셈이다. 그래서 이번에 제안하는 것은: 하드·보스 스테이지 전용 "오늘 1회 클리어" 하루 목표를 각각 추가해, 오래 플레이한 고수 유저도 매일 게임을 켤 이유를 가질 수 있게 하자.
