# Content Audit — 2026-06-14 — 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 19건 (가장 최근: 2026-06-12)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 31종 / 전체 84 | 빈 슬롯 7~9·25~39·47~51 제외, 스킨 30종(54~83) 별도 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | 스킨 7 + IAP 3 + 골드 소모 2 |
| 고양이 스킨 | 6종 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry (각 Cat1~5) |

### 분포 공백

- `Data.Login.attendanceTodayDone` 필드는 **bool** 타입이며 오늘 출석 여부만 기록한다.
- **연속 출석일수(streak) 카운터 필드가 존재하지 않는다** — `lastDailyResetDateKey`는 자정 리셋 판정용이며 연속 추적용이 아니다.
- `EDailyCounter.Attendance(0)` 미션(missionID 100)은 당일 출석 1회 = 100골드 보상이며, 3일·7일·30일 연속 달성 마일스톤 미션은 정의되지 않아 장기 리텐션 루프가 완전히 비어 있다.
- 모바일 매치-3 장르에서 streak 기반 연속 출석 보상은 D7·D14·D30 리텐션에 직접 영향을 미치는 핵심 메커니즘이지만, CatPang에는 tapIndex 1/2/3 중 어느 탭에도 streak 개념을 활용한 미션이 없다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 진입 장벽 완화 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 vs 일일 미션 보상 gap |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 (AdWatch vs BlockDestroy) |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-06-01 | 4a09a70 | RainbowPang tapIndex 1 반복 미션 누락 |
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 (HardStageClear 신설) |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 균일 포맷 — 보드 크기·이동제한 다양화 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 추가 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### 연속 출석 스트릭 마일스톤 미션 신설

- **카테고리**: 일일 미션 / 리텐션
- **요지**: `Data.Login.attendanceTodayDone`은 당일 출석 여부(bool)만 추적하며 연속 출석일수를 기록하는 필드가 없다. 3일·7일·30일 연속 달성에 대한 마일스톤 미션(tapIndex 2)도 없어 장기 플레이를 이어갈 동기 루프가 완전히 단절되어 있다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 5/3/5/4 → 종합 **17**
- **근거**:
  - `Assets/Scripts/Data.cs:41` — `attendanceTodayDone: bool` 필드만 존재. 연속 카운터(`streakDays` 등) 없음 직접 확인.
  - `Assets/Scripts/Data.cs:29` — `lastDailyResetDateKey`는 자정 리셋 판정 전용. 연속일 추적 목적 아님.
  - `Assets/AssetBundleResources/json/Mission.json:24` — `missionID 100`: `dailyCounter:0` (Attendance), `clearValue:1`, 보상 100골드. streak 마일스톤 행 없음.
  - `Assets/Scripts/Defines.cs:311~326` — `EDailyCounter` 4종 정의(Attendance/NormalStageClear/BlockDestroy/AdWatch). streak 관련 enum 없음.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   UIMission 일일 탭(tapIndex 3)에 진입하는 순간 출석 처리가 발동(`DailyMissionService.MarkAttendance()`)되며, 기존 출석 미션 셀 위에 "연속 N일 출석 중" 배너가 즉시 노출된다. 앱을 처음 실행한 날부터 streak = 1로 시작해 매일 진입 시마다 표시가 업데이트된다.

2. **화면 변화**
   UIMission 일일 탭 상단에 스트릭 전용 배너 행이 추가된다. 배너에는 현재 연속 일수, 다음 마일스톤(3·7·30일)까지 남은 일수, 마일스톤 달성 시 지급될 보상 아이콘이 표시된다. 마일스톤 달성 시 배너가 골드 획득 연출로 전환된다.

3. **입력 행동**
   유저는 별도 버튼을 누를 필요 없이 UIMission 탭을 여는 것만으로 streak이 갱신된다. 기존 출석 미션(missionID 100)과 동일한 트리거를 공유하므로 중복 행동이 불필요하다.

4. **시스템 반응**
   `DailyMissionService.MarkAttendance()`가 기존 `attendanceTodayDone = true` 처리에 더해 `streakDays += 1`을 수행한다. `lastDailyResetDateKey`와 전일 날짜키를 비교해 연속 여부를 판정하고, 하루라도 건너뛰면 `streakDays = 1`로 리셋한다. `CHMTime.UtcNow` 기반 NTP 시각으로 판정하므로 기기 시각 조작에 안전하다.

5. **반복·재발생 패턴**
   매일 반복되는 사이클이다. 3일 마일스톤 달성 후 streak은 계속 누적되어 7일 마일스톤으로 이어지고 30일까지 도달 가능하다. 각 마일스톤 보상은 임계값 최초 도달 시 1회 지급된다. 30일 달성 이후에도 streak은 계속 표시되어 유지 동기를 부여한다.

6. **종료·해소 조건**
   자정(NTP 기준) 이후 다음날까지 UIMission 탭에 진입하지 않으면 streak이 1로 리셋된다. 마일스톤 보상은 각 임계값(3·7·30일) 최초 도달 시에만 지급되며, 리셋 후 재달성 시에는 지급하지 않는다(tapIndex 2 방식 `addValue:-1`).

7. **다른 시스템과 상호작용**
   - `CHMTime.UtcNow` / `GetUtcDateKey()`: NTP 기반 날짜 판정. NTP 미수신 시 streak 갱신 보류(리셋 없음), 위변조 안전.
   - `CHMData.SaveData()`: streak 갱신 직후 즉시 호출 → GPGS 클라우드 동기화 대상.
   - 기존 EDailyCounter.Attendance 일일 미션(missionID 100)은 변경 없이 병렬 동작. streak과 별개로 당일 100골드는 그대로 지급.
   - UIMission `CHPoolingScrollView` 아이템 렌더링: 기존 tapIndex 3 셀 4개에 streak 배너 1개가 추가 삽입되므로 스크롤뷰 아이템 개수 변화 고려.

8. **엣지 케이스**
   - NTP 미수신 상태: 판정 불가이므로 streak을 변경하지 않는다. 수신 회복 후 다음 정상 출석에서 연속 여부 재판정.
   - 동일 날짜키로 앱 재실행 시 `attendanceTodayDone` 가드로 streak 중복 증가 방지.
   - GPGS 클라우드·로컬 데이터 불일치 시 `streakDays` 큰 값 유지(기존 `CHMData` merge 정책 적용).
   - 앱 최초 설치 직후 `streakDays = 0`, 첫 탭 진입 시 1로 초기화.
   - 마일스톤 보상 재수령 시도: Data.Mission `clearState = Clear`이면 지급 스킵(기존 tapIndex 2 미션 처리와 동일).

9. **유저 정보·피드백**
   UIMission 배너에 현재 연속 일수와 진행 바가 표시된다. 마일스톤 달성 시 기존 FireCracker 이펙트(`EEffect.FireCracker`) 재생 + 골드·아이템 팝업. streak이 끊긴 첫 진입 시 "하루 쉬어서 연속 기록이 리셋됐어요" 소프트 알림을 UIMission 배너에 1회 표시해 재시작 동기를 부여한다.

### 보류

- **고양이 스킨 테마 블록 tapIndex 1 미션 신설**: `collectionType` 54~83(스킨 블록)이 Mission.json에서 전혀 미사용. 구현 단순하나 2026-06-05 Cat6/Cat7 tapIndex 1 미션 감사와 카테고리가 일부 겹침(tapIndex 1 수집 미션). 차회 검토.
- **Ball 블록 일일 탈출 미션(tapIndex 3)**: Ball tapIndex 2(131개 누적)만 있고 일일 목표가 없음. 종합 점수 13으로 streak 17 대비 낮아 보류.

## 3. 과거 감사 대비 차별성

git log 19건 검토 완료.

가장 유사했던 과거 커밋:
- `d819b99` (2026-05-30): 하드 스테이지 클리어 일일 미션 없음 → HardStageClear EDailyCounter 신설. **차별점**: 그 제안은 새 EDailyCounter 값을 추가해 하드 스테이지 플레이 행동을 당일 횟수로 추적하는 것. 본 제안은 날짜 간 연속성(streak)을 추적하는 완전히 다른 자료 구조(`streakDays: int`)이며, 대상 메커니즘은 리텐션·습관 형성이고 tapIndex 2 마일스톤으로 구현한다.

streak 기반 연속성 개념(consecutive days)은 19건 전체 과거 감사에서 단 한 번도 다루지 않은 영역이다.

## 4. 다음 단계 제안

채택 시 구현 순서:
1. `Data.Login`에 `streakDays: int` 필드 추가 (`Assets/Scripts/Data.cs`)
2. `DailyMissionService.MarkAttendance()`에 전일 날짜키 비교 → streak 증가/리셋 로직 추가 (`Assets/Scripts/`)
3. `Mission.json`에 tapIndex 2 streak 마일스톤 행 3개 추가 (3일/7일/30일, collectionType 비사용 고유 key)
4. UIMission 일일 탭에 streak 배너 UI 추가 (기존 `MissionScrollView` 또는 별도 행)
5. A/B 검증 지표: 7일 리텐션(streak 도입 전 기준선 대비), 일일 활성 세션 비율

## 5. 쉬운 설명 (비개발자 요약)

지금 고양이팡에서는 매일 미션 탭을 열면 골드 100개를 받는다. 그런데 "3일 연속으로 접속했다"거나 "7일을 하루도 빠짐없이 들어왔다"는 걸 게임이 전혀 기억하지 않는다. 매일 들어와도 특별히 더 큰 선물을 받을 수 없어서, 하루 빠진다고 아쉬울 이유가 없다. 마치 도장 10개를 모으면 음료 한 잔을 주는 스탬프 카드처럼, 연속으로 접속한 날들이 쌓여서 3일·7일·30일 목표를 채우면 특별 보상을 주는 기능이 있으면 유저가 게임을 매일 습관처럼 열게 된다. 그래서 이번에 제안하는 것은: 매일 접속이 "연속 N일 째"로 쌓이고, 3일·7일·30일을 채우면 큰 보상을 주는 '연속 출석 스트릭 미션'을 추가하는 것이다.
