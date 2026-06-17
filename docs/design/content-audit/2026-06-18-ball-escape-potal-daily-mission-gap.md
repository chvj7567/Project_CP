# Content Audit — 2026-06-18 — Ball 탈출·Potal 전환 일일 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 20건 (가장 최근: 2026-06-17)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39(15개), 47~51(5개) 제외. 스킨 테마 30개 포함 |
| 일일 미션 종류 | EDailyCounter 4종 + collectionType 기반 1종 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch / CatPang(daily) |
| 상점 아이템 | 12개 | tapIndex 1 스킨 9개, tapIndex 2 IAP 3개 |
| 고양이 스킨 | 6종 × Cat1~5 = 30 블록 변형 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |

### 분포 공백 — tapIndex 3 일일 미션 현황

| missionID | dailyCounter / collectionType | 조건 | 보상 |
|---|---|---|---|
| 100 | dailyCounter=0 (Attendance) | 1회 | Gold 100 |
| 101 | dailyCounter=1 (NormalStageClear) | 3회 | Gold 300 |
| 102 | dailyCounter=2 (BlockDestroy) | 100개 | AddTime ×1 |
| 103 | dailyCounter=3 (AdWatch) | 1회 | AddMove ×10 |
| 104 | collectionType=18 (CatPang) | 3개/일 | Gold 200 |

**Ball (collectionType=53) tapIndex 3 일일 미션: 존재하지 않음.**

Ball은 tapIndex 2 장기 이정표(missionID=17, collectionType=53, clearValue=131)만 있고, 매일의 달성 목표가 없다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 — 보스 스테이지 조기 진입 허용 설계 재검토 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 — 스킨 장착 보상 루프 신설 제안 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 — PinkBomb~BlueBomb 50개 이정표 신설 제안 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절 제안 |
| 2026-06-13 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 진입 장벽 완화 제안 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 — 100일 소요 |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch vs BlockDestroy |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-05-31 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### Ball 탈출·Potal 전환 일일 미션 (tapIndex 3) — Mission.json 1줄 추가

- **카테고리**: 일일 미션 (tapIndex 3 / collectionType 기반)
- **요지**: Ball 블록은 맨 아래줄 도달 시 Potal로 전환·좌우 확산하는 독특한 제거 메커니즘을 갖고 있음에도 tapIndex 3 일일 미션이 전혀 없다. CatPang daily(missionID 104)가 collectionType=18 + dailyCollectionSnapshotJson 패턴으로 구현된 선례가 있어, 동일 방식으로 collectionType=53(Ball) 일일 미션을 **Mission.json 1줄 추가**만으로 구현할 수 있다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/1/4/5 → 종합 **18점**
  - 검증가치 4: Ball은 드래그 가능한 유일한 특수블록이며 탈출·Potal 전환이라는 고유 행동이 있으나, 이 행동을 매일 유도하는 목표가 없어 전략적 가치 검증 기회 자체가 없음
  - 구현비용 1: Mission.json에 tapIndex 3 항목 1줄 추가. 코드 변경 불필요. missionID 104 패턴(collectionType + dailyCollectionSnapshotJson)을 그대로 재사용
  - 플레이어경험 4: Ball→Potal 변환은 좌우 확산이라는 시각적 피드백이 강하고, 일일 목표로 설정 시 플레이어가 Ball을 의도적으로 배치·드래그하는 전략적 행동을 매일 경험하게 됨
  - 데이터근거 5: Mission.json에 Ball tapIndex 2 (missionID=17, collectionType=53, clearValue=131)가 명확히 존재하며, 동일 collectionType=53으로 tapIndex 3 공백이 데이터로 확인됨. missionID 104 구현 선례로 추가 개발 비용 없음
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 17 (tapIndex=2, collectionType=53, clearValue=131): 누적 131개 달성 장기 미션은 있으나 tapIndex 3 미션 없음
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 104 (tapIndex=3, collectionType=18, clearValue=3, reward=0, rewardCount=200): Ball daily 구현의 직접 선례
  - `Assets/Scripts/Defines.cs` — `EBlockState.Ball = 53`: collectionType=53이 Ball을 정확히 가리킴
  - `Assets/Scripts/Data.cs` — `Login.dailyCollectionSnapshotJson`: 자정 스냅샷으로 daily collection 카운트를 추적하는 기존 인프라 완비

#### 제안 Mission.json 항목

```json
{"missionID":"105", "tapIndex":"3", "descStringID":172, "collectionType":53, "dailyCounter":-1, "clearValue":2, "addValue":0, "reward":0, "rewardCount":150}
```

- `clearValue=2`: 하루 2회 Ball 탈출. CatPang daily(3회)보다 낮게 설정 — Ball 등장 빈도가 CatPang보다 낮음
- `rewardCount=150`: Gold 150 — Attendance(100)보다 높고 CatPang daily(200)보다 낮아 난이도 대비 균형

#### 유저 플로우

1. **노출 시점·트리거**: 플레이어가 UIMission을 열고 tapIndex 3(일일 미션) 탭을 선택하면 새 미션 항목 "오늘 Ball 탈출 2회"가 기존 5개 미션 아래에 노출된다. 자정(NTP 기준) 이후 첫 접속 시 미션 진행도가 0으로 리셋된다.

2. **화면 변화**: 미션 리스트에 Ball 블록 아이콘과 "0/2" 진행 바가 표시된다. GameScene에서 Ball이 맨 아래줄에 도달할 때마다 진행도가 1 증가하며, UIMission을 열면 실시간 반영된 값을 확인할 수 있다.

3. **입력 행동**: 플레이어는 GameScene 내에서 Ball 블록을 아래 방향으로 드래그하거나(Ball은 EDrag 가능), 인접 블록을 매치해 Ball 위·옆 공간을 만들어 Ball이 중력으로 내려가도록 유도한다. 폭탄 범위로도 Ball을 직접 제거할 수 있으나, 탈출(Potal 전환)에는 맨 아래줄 도달이 필요하다.

4. **시스템 반응**: Ball이 9×9 그리드 최하단 행(row 8)에 도달하면 GPGameScene의 `SetDissapearBlock` 계열 처리가 Ball→Potal 전환을 수행하고, Data.Collection[key="53"]의 누적값이 1 증가한다. DailyMissionService는 이 누적값과 `dailyCollectionSnapshotJson`의 자정 스냅샷 차분으로 오늘의 Ball 탈출 횟수를 계산한다. clearValue=2 달성 시 클리어 처리 후 Gold 150이 즉시 지급된다.

5. **반복·재발생 패턴**: 자정(NTP) 이후 DailyMissionService.CheckAndResetIfNeeded() 호출 시 clearState가 NotDoing으로 리셋된다. 스냅샷도 갱신되므로 전날 탈출 기록은 오늘 카운트에 포함되지 않는다. 플레이어는 매일 동일 미션을 반복 수행한다.

6. **종료·해소 조건**: 당일 Ball 탈출이 2회 누적되면 미션이 Clear 상태로 전환되고 Gold 150이 지급된다. 이후 같은 날 추가 Ball 탈출이 발생해도 보상이 중복 지급되지 않는다. 자정 이후 다음 날 미션이 새로 시작된다.

7. **다른 시스템과 상호작용**: Ball 탈출은 스테이지 클리어 판정(`checkHp && IsBallBlock()`)에도 영향을 준다. 즉 Ball을 탈출시키면 스테이지 클리어 목표와 일일 미션 목표를 동시에 달성할 수 있어, 스테이지 클리어와 일일 미션 간 시너지가 생긴다. 또한 Ball이 Potal로 전환되면 좌우로 Potal이 확산되어 인접 블록에 추가 피해를 줄 수 있다.

8. **엣지 케이스**: ① NTP 미수신 상태에서는 리셋이 발생하지 않으므로(DailyMissionService 기존 안전 처리), 스냅샷 기반 카운트도 그대로 유지된다. ② Ball이 없는 스테이지에서는 탈출이 불가능하므로, 해당 스테이지에서는 미션 진행이 0 유지된다 — 플레이어가 의도적으로 Ball이 등장하는 스테이지를 선택해 플레이하도록 유도하는 간접 스테이지 선택 동기가 생긴다. ③ Ball이 폭탄 범위로 제거되면 Potal 전환이 아니라 직접 소멸이므로 탈출 카운트에 포함되지 않는다(기존 GPMatchChecker `ChangeMatchState` 처리 그대로).

9. **유저 정보·피드백**: 미션 설명 텍스트(descStringID=172)에 "오늘 Ball을 2번 탈출시키세요 — Ball이 맨 아래에 닿으면 Potal로 변해요!"와 같이 Ball→Potal 전환 메커니즘을 함께 안내하면 튜토리얼 효과도 겸한다. 탈출 성공 시 Potal 확산 이펙트가 시각적 보상 피드백을 제공하므로 별도 UI 연출 없이도 달성감이 자연스럽게 전달된다.

### 보류

- **BossStage 클리어 일일 미션**: 2026-05-29 HardStageClear 신설 제안과 카테고리(EDailyCounter 신설) 및 요지(모드별 클리어 미션 부재)가 유사해 보류.
- **WallCreator tapIndex 1 반복 수집 미션**: 2026-05-29 PotalCreator·CatBox tapIndex 2 공백 제안과 영역이 근접해 보류.

---

## 3. 과거 감사 대비 차별성

git log 20건 검토 완료.

가장 유사할 수 있는 과거 커밋:
- `4a09a70` (2026-05-29) "PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백" — tapIndex가 2(장기)이고 대상 블록도 다름. Ball tapIndex 3 일일 미션과 카테고리 다름.
- `1ea1f97` (2026-06-02) "Arrow 폭탄 tapIndex 3 일일 미션 누락" — tapIndex 3이지만 대상이 Arrow 폭탄(직접 매치 기반 카운터). Ball은 collectionType 기반 스냅샷 차분 방식으로 구현 패턴이 다름.

차별점: ① tapIndex 3 일일 미션이면서 ② collectionType(스냅샷 차분) 방식이고 ③ Ball의 맨 아래줄 탈출이라는 고유 메커니즘에 집중한다는 세 가지 조합은 20건 어디에도 없음.

---

## 4. 다음 단계 제안

- Mission.json에 missionID=105 항목 1줄 추가 (위 제안 JSON)
- StringKorea.json / StringEnglish.json에 descStringID=172 텍스트 추가
- DailyMissionService 기존 missionID 104 처리 경로에서 missionID 105 자동 지원 확인 (collectionType 기반이므로 추가 코드 불필요할 가능성 높음)
- 채택 시 QA: Ball이 없는 스테이지에서 미션 카운트가 변경되지 않는지 확인

---

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 매일 할 수 있는 작은 도전 과제들이 있다. 예를 들어 "오늘 광고 한 번 보기"나 "오늘 블록 100개 없애기" 같은 것들이다. 그런데 게임에 나오는 특별한 블록 중에 '공(Ball)' 블록이 있다 — 이 공은 판의 맨 아래까지 내려가면 '포탈'로 변해서 좌우로 펼쳐지는 멋진 효과를 내는 유일한 블록이다. 그런데 "오늘 공을 2번 탈출시키기" 같은 일일 도전 과제는 아직 없다. 그래서 이번에 제안하는 것은: 게임 파일 한 줄만 추가해서 "공을 오늘 2번 맨 아래까지 보내기" 도전 과제를 만드는 것이다 — 코딩 없이 데이터만 바꾸면 돼서, 만들기 가장 쉬운 신규 컨텐츠다.
