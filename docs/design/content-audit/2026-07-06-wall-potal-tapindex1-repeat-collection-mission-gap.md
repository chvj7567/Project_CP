# Content Audit — 2026-07-06 — Wall/Potal tapIndex 1 반복 수집 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-28)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 실사용 30종 / 전체 84 | 빈 슬롯 25~39·47~51 제외 기준 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1(스킨 등) 9개, tapIndex 2(IAP) 3개 |
| 고양이 스킨 테마 | 6종 | CatCrown·CatFlowers·CatMushroom·CatParty·CatSanta·CatStrawberry |

### Mission.json 탭별 미션 분포

| tapIndex | 미션 수 | 대상 블록(collectionType) |
|---|---|---|
| 1 (반복 수집) | 17건 | Cat1~5(0~4), Arrow1~6(10~15), CatPang(18), 5색 폭탄(19~23) |
| 2 (생애 이정표) | 5건 | Fish(24), CatBox(40), WallCreator(45), RainbowPang(52), Ball(53) |
| 3 (일일) | 5건 | dailyCounter 0~3, CatPang 일일(18) |

### 장애물 블록 — StageBlock.json 배치 현황

| 블록 | blockState | 등장 스테이지 수 | 배치 셀 수 | 첫 등장 | Mission.json 존재 여부 |
|---|---|---|---|---|---|
| Wall | 16 | **123** | **1,719** | stage 6 | tapIndex 1·2 **없음** |
| Potal | 17 | 87 | 787 | stage 7 | tapIndex 1·2 **없음** |
| Fish | 24 | 53 | 258 | stage 31 | tapIndex 1 없음(2←이전 감사), tapIndex 2 있음 |
| WallCreator | 45 | 45 | — | stage 71 | tapIndex 2만 있음(missionID 15) |
| PotalCreator | 46 | 47 | — | stage 73 | **없음** |
| Ball | 53 | 49 | — | stage 131 | tapIndex 2만 있음(missionID 17) |

Wall 블록은 **게임 내 가장 많이 배치된 장애물(1,719 셀, 123 스테이지)**임에도  
tapIndex 1(반복 수집 보상 루프)과 tapIndex 2(생애 이정표) 모두 Mission.json에 존재하지 않는다.  
Potal(787 셀, 87 스테이지) 역시 동일하게 두 탭 모두 부재다.

### 분포 공백
- tapIndex 1 미션은 고양이·화살표 폭탄 계열(Cat1~5, Arrow1~6, CatPang, 5색 폭탄)에만 존재.  
  가장 자주 제거하는 장애물 블록(Wall, Potal)은 수집 루프에서 완전히 배제되어 있다.
- Wall은 stage 6부터 보스 stage 100090까지 등장 — 플레이 전 구간에 걸쳐 있다.
- tapIndex 2에 WallCreator(collectionType=45)는 있지만, 그 산물인 Wall 자체에는 어떤 미션도 없다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-28 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-26 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 시점 후기 도입 |
| 2026-06-25 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-24 | fcebcd0 | 후반 그룹 10~15 복합 제약 밀스톤 스테이지 완전 부재 |
| 2026-06-23 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-22 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-21 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 — 난이도 역전 |
| 2026-06-20 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-19 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-18 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-17 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-16 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-15 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 |
| 2026-06-14 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-13 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 진입 장벽 완화 제안 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 확대 제안 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### Wall 블록 tapIndex 1 반복 수집 미션 신설

- **카테고리**: 미션 (tapIndex 1 반복 수집 루프)
- **요지**: Wall 블록(EBlockState=16)은 게임 내 배치량 1위(1,719 셀, 123 스테이지)임에도 Mission.json에 수집 미션이 전혀 없다. 기존 Cat1~5·Arrow1~6·5색 폭탄 계열과 동일한 tapIndex 1 패턴으로 "Wall N개 제거"마다 Gold를 지급하는 반복 루프를 추가하면 플레이 전 구간에서 지속적 성취감을 제공할 수 있다.
- **점수**: 검증가치 4 / 구현비용 1 / 플레이어경험 5 / 데이터근거 5 → 종합 **19**
- **근거**:
  - `Assets/AssetBundleResources/json/StageBlock.json` — Wall(blockState=16): 1,719 셀, 123 스테이지(stage 6~100090). 전 구간 최대 배치 장애물.
  - `Assets/AssetBundleResources/json/Mission.json` — collectionType=16 항목 없음. tapIndex 1·2 모두 부재 확인.
  - `Assets/Scripts/Defines.cs` EBlockState.Wall=16, EBlockState.Potal=17.
  - 기존 tapIndex 1 패턴: collectionType 0~4(Cat1~5)는 clearValue=100/addValue=100/rewardCount=100(Gold). 동일 패턴으로 Wall(16)을 추가하면 코드 변경 없이 Mission.json 1줄 + 문자열 테이블 1줄로 완성된다.

#### 유저 플로우

1. **노출 시점·트리거**  
   스테이지에서 플레이어가 Wall 블록 인접 셀을 3-매치하면 GPGameScene의 `RemoveMatchBlock` → `CheckArround` → `DamageBlock`이 호출되어 Wall HP가 0이 된다. 이 순간 CHMData의 Collection 딕셔너리 key "16" 값이 1 증가한다. 플레이어가 UIMission tapIndex 1 탭을 열면 새로 추가된 "Wall N개 제거" 카드가 진행 바와 함께 표시된다.

2. **화면 변화**  
   Wall이 파괴될 때는 기존 블록 소멸 이펙트(EEffect.Damage 또는 FireCracker)가 재생된다. UIMission에서는 tapIndex 1 탭의 마지막 위치에 Wall 미션 카드가 추가되며, clearValue 도달 시 골드 수령 팝업(현재 Cat 미션과 동일 UI)이 표시된다.

3. **입력 행동**  
   Wall은 드래그 불가이므로 별도 입력 없다. 플레이어는 Wall 인접 고양이 블록을 드래그하여 매치시키는 기존 행동만으로 자동으로 카운터가 쌓인다. 미션 존재를 인지한 플레이어는 Wall 옆에 우선적으로 매치를 유도하려는 전략적 드래그를 취하게 된다.

4. **시스템 반응**  
   CHMData의 `Collection` 딕셔너리에서 key="16"을 찾아 value를 1 증가시킨다. clearValue(예: 10)에 도달하면 EReward.Gold rewardCount(예: 100)를 지급하고, clearValue를 addValue(예: 10)만큼 올려 다음 목표를 설정한다. repeatCount(예: 100) 한도까지 반복된다. 별도 코드 변경 없이 Mission.json 파싱 로직이 그대로 처리한다.

5. **반복·재발생 패턴**  
   clearValue=10, addValue=10, repeatCount=100으로 설정하면 총 1,000개 Wall 파괴까지 Gold 보상이 반복된다. stage 6부터 등장하는 Wall의 평균 배치 밀도(약 14 셀/스테이지)를 고려하면, 중반 플레이어는 70~80 스테이지 진행 시 첫 100개 목표를 달성할 수 있어 자연스러운 루프 속도를 갖는다.

6. **종료·해소 조건**  
   repeatCount=100 도달 시 미션 카드가 "완료" 상태로 전환되고 더 이상 보상이 지급되지 않는다. 이후에도 Collection 딕셔너리의 누적값은 유지된다(tapIndex 2 이정표용 소재 또는 추후 확장 시 재사용). 완료 후에는 tapIndex 2에 Wall 이정표 미션(현재 부재)을 신설하면 자연스러운 연속 루프를 형성한다.

7. **다른 시스템과 상호작용**  
   WallCreator 블록(missionID 15, collectionType=45)이 주변 일반 블록을 Wall로 변환하는 경우, 그 Wall이 나중에 파괴될 때도 Wall 수집 카운터에 포함되므로 WallCreator가 많이 등장하는 후반·보스 스테이지에서 Wall 미션 진행 속도가 가속된다. 보스 스킬 `EBossSkillType.Wall`로 생성된 Wall도 동일하게 카운터에 포함되어, 보스 스테이지 플레이어에게 추가 보상 동기를 제공한다.

8. **엣지 케이스**  
   CLAUDE.md에 따르면 Wall은 직접 드래그 불가이며, `ChangeMatchState`에서 폭탄 범위 대상에서 명시 제외된다. 따라서 폭탄으로 직접 제거되는 경우는 카운터 집계에서 제외해야 한다(인접 `DamageBlock` 경로만 유효). HP가 2 이상인 Wall을 두 번에 나눠 깎아도 최종 파괴 시 카운터를 1 증가시켜야 한다(중간 데미지 때 카운터 증가 방지). WallCreator에 의해 변환된 블록과 보스 스킬 생성 Wall의 카운터 포함 여부는 명세 확정 필요.

9. **유저 정보·피드백**  
   미션 탭의 진행 바 외에, 스테이지 클리어 결과 화면(UIGameEnd/UIGameStart 영역)에서 "이번 스테이지 Wall N개 파괴" 미니 통계를 표시하면 플레이어가 미션과 플레이를 연결짓도록 유도할 수 있다. 골드 지급 시에는 기존 Cat 미션과 동일한 플로팅 텍스트 피드백을 사용한다. repeatCount 종료 이후에도 tapIndex 2 이정표 미션 신설을 안내하는 힌트 문구를 넣으면 장기 목표 인식을 높일 수 있다.

### 보류
- **Potal tapIndex 1 신설** — Wall과 동일 카테고리이지만 배치량(787 셀)이 Wall(1,719)의 절반 수준이고 등장 시점(stage 7)도 비슷하다. Wall 미션 효과를 검증한 뒤 이어서 추가하는 것이 합리적이므로 보류.
- **Cat6/Cat7 tapIndex 2 이정표 미션** — 2026-06-05(Cat6/Cat7 tapIndex 1 공백)와 2026-06-19(Cat1~5 tapIndex 2 공백)의 연장선상이나, Cat 블록 계열 미션은 다수 감사에서 다뤄졌으므로 보류.
- **PotalCreator tapIndex 1** — 2026-05-29 파일(potalcreator-catbox-tapindex2-mission-gap)과 유사 카테고리. 보류.

---

## 3. 과거 감사 대비 차별성

git log 24건 검토 완료.  
가장 유사했던 과거 커밋:

- **2026-05-31 (Wall/Potal tapIndex 2 gap)** — Wall·Potal의 생애 이정표(tapIndex 2, 일회성 달성) 부재를 다뤘다.  
  **차별점**: 오늘 제안은 tapIndex 1(반복 수집 루프, 매 N개마다 Gold 지급). tapIndex 2는 "평생 한 번 달성하는 마일스톤"인 반면, tapIndex 1은 "매일 쌓이는 Gold 루프"다. 동기 구조가 다르고, 구현도 clearValue/addValue 반복 패턴으로 별도 항목이 필요하다.
- **2026-06-28 (Fish tapIndex 1 gap)** — Fish 블록 반복 수집 미션 공백을 다뤘다.  
  **차별점**: Fish는 53 스테이지·258 셀에 stage 31부터 등장하는 특수 탈출 블록이다. Wall은 123 스테이지·1,719 셀로 stage 6부터 전 구간에 배치된 기본 장애물이며, 배치 규모가 Fish의 약 6.7배다. 미션 부재의 영향 범위와 플레이어 체감 빈도가 완전히 다르다.

---

## 4. 다음 단계 제안

- 채택 시 Mission.json에 다음 1줄 추가 후 문자열 테이블(StringKorea/StringEnglish.json)에 descStringID 신규 항목 등록:  
  `{"missionID":"23", "tapIndex":"1", "descStringID":185, "collectionType":16, "clearValue":10, "addValue":10, "reward":0, "rewardCount":100}`  
  (missionID·descStringID는 기존 최대값 이후 다음 값으로 할당 조정 필요)
- 미션 문구(안): "Wall 블록 N개 제거하기" (한국어), "Break N Wall blocks" (영어)
- 효과 검증: 미션 추가 전후 일평균 스테이지 플레이 수 및 UIMission 탭 진입률 비교
- 후속 확장: Wall tapIndex 1 완료 후 tapIndex 2 이정표(예: Wall 500개 누적 제거) 신설 연계

---

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 고양이 블록 주변에 있는 "벽돌"(Wall) 블록이 있다. 플레이어가 벽돌 옆에서 매치를 하면 벽돌이 깨진다. 그런데 벽돌은 게임 전체에서 가장 많이 등장하는 장애물인데도, 지금까지 "벽돌 몇 개 깼는지" 세주는 미션이 하나도 없다. 고양이 블록이나 화살표 폭탄은 몇 개 부쉈는지 보상받을 수 있는데, 훨씬 자주 만나는 벽돌은 아무리 많이 깨도 아무 보상이 없다. 그래서 이번에 제안하는 것은: 벽돌 10개 깰 때마다 골드를 받는 수집 미션을 추가해, 평소 플레이하면서 자연스럽게 보상이 쌓이는 루프를 만들자는 것이다.
