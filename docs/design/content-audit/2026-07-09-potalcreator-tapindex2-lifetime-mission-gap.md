# Content Audit — 2026-07-09 — PotalCreator tapIndex 2 생애 이정표 미션 완전 공백 — WallCreator 대칭 미션 결여

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 28건 (가장 최근: 2026-07-07)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 30종 / 전체 84 | 빈 슬롯 25~39·47~51 제외 기준 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1(스킨 8개 포함) 9개 + tapIndex 2(IAP) 3개 |
| 고양이 스킨 테마 | 6종 | CatCrown·CatFlowers·CatMushroom·CatParty·CatSanta·CatStrawberry |

### Creator 블록 쌍 미션 분포 (핵심 분석)

StageBlock.json 실측치:

| 블록 | EBlockState | 셀 수 | 등장 스테이지 수 | tapIndex 2 미션 |
|---|---|---|---|---|
| WallCreator | 45 | 162셀 | 45스테이지 | missionID=15, clearValue=71 **✓** |
| PotalCreator | 46 | **200셀** | **47스테이지** | **없음 ✗** |

- **PotalCreator는 WallCreator보다 배치 셀이 23% 많고(200 vs 162), 등장 스테이지도 더 많다(47 vs 45).**
- Mission.json 전체 29개 항목 중 collectionType=46(PotalCreator)에 해당하는 항목이 단 하나도 없다.
- WallCreator와 PotalCreator는 동일한 Creator 메커니즘(매 턴 인접 일반 블록을 Wall/Potal로 변환, 인접 매치로 HP 감소 제거)을 공유하는 대칭 블록이다.

### Mission.json tapIndex 2 전체 현황

| missionID | tapIndex | collectionType | 대상 블록 | clearValue | 비고 |
|---|---|---|---|---|---|
| 13 | 2 | 24 | Fish | 31 | 있음 |
| 14 | 2 | 40 | CatBox1 | 51 | 있음 |
| **15** | 2 | 45 | **WallCreator** | **71** | **있음** |
| 16 | 2 | 52 | RainbowPang | 91 | 있음 |
| 17 | 2 | 53 | Ball | 131 | 있음 |
| — | — | **46** | **PotalCreator** | — | **없음** |

tapIndex 2(생애 이정표) 미션이 있는 5개 블록 중 WallCreator와 대칭 쌍인 PotalCreator만 유일하게 누락되어 있다.

### 분포 공백
- PotalCreator는 47개 스테이지에서 200개 셀로 등장하는 주요 장애물 블록임에도 어떤 tapIndex에도 미션이 없다.
- 유저가 PotalCreator를 제거해도 Collection 카운터(collectionType=46)가 증가하지만, 이 데이터를 소비하는 Mission.json 항목이 전무하다.
- WallCreator를 71개 처치하면 Gold 1000 보상이 지급되지만, PotalCreator를 어떤 수만큼 처치해도 이정표 보상이 없다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 (커밋 기준) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-07-07 | d371085 | 보스 스테이지 Stage.json 스킬 조율 필드 전무 — 쿨타임 코드 상수 단일 의존 |
| 2026-07-06 | 1324855 | 보스 스테이지 attack 잠든 공격력 스탯 — 매 턴 +0 가산, 획득 경로 전무 |
| 2026-07-05 | 80d875f | Wall 블록 tapIndex 1 반복 수집 미션 완전 공백 — 1,719셀·123스테이지 |
| 2026-07-04 | 18dd561 | tapIndex 2 장기 이정표 미션 보상 Gold 단일화 — AddTime·AddMove 전무 |
| 2026-07-03 | 39d8ced | EBackground 4종 Defines.cs 단독 정의·완전 미참조 |
| 2026-07-02 | 5c2ba4c | Wall·Potal 초등장 스테이지(6·7) Tutorial.json 항목 완전 공백 |
| 2026-07-01 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 고아 항목 |
| 2026-06-30 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 |
| 2026-06-29 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 |
| 2026-06-28 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-26 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 후기 도입 |
| 2026-06-25 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-24 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 스테이지 부재 |
| 2026-06-23 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-22 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-21 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 |
| 2026-06-20 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-19 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-18 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-17 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-16 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-15 | 3670d4f | 고양이 스킨 블록(54~83) 수집 미션 완전 공백 |
| 2026-06-14 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-13 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-11 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 |
| 2026-06-10 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-09 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-08 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### PotalCreator tapIndex 2 생애 이정표 미션 완전 공백 — WallCreator 대칭 미션 결여

- **카테고리**: 미션
- **요지**: WallCreator(45)는 tapIndex 2 이정표 미션(missionID=15, clearValue=71)을 보유하고 있으나, 동일 Creator 메커니즘을 공유하는 PotalCreator(46)는 배치량이 더 많음에도(200셀·47스테이지 vs 162셀·45스테이지) Mission.json에 단 1개의 항목도 없다. WallCreator-PotalCreator 쌍의 완전한 대칭 균열이다.
- **점수**: 검증가치=4 / 구현비용=1 / 플레이어경험개선=4 / 데이터근거=5 → **종합 18**
- **근거**: `Assets/AssetBundleResources/json/Mission.json` — collectionType=46 항목 전무, collectionType=45 missionID=15 확인. `Assets/AssetBundleResources/json/StageBlock.json` — blockState=46 200셀/47스테이지(Python 실측), blockState=45 162셀/45스테이지.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**: 노멀·하드 스테이지 중 PotalCreator가 처음 등장하는 스테이지(StageBlock.json 실측 기준 stage 73 근방)에 진입할 때가 첫 트리거다. 이후 47개 스테이지에 걸쳐 반복 등장하므로 노멀 기준 스테이지 중반~후반 전반에 걸쳐 지속적으로 만난다.

2. **화면 변화**: 현재는 PotalCreator를 매치로 제거해도 미션 탭(UIMission)에 아무런 진행 바가 추가되지 않는다. 유사 블록인 WallCreator를 제거하면 tapIndex 2 미션 진행 바가 71/71로 채워지는 것과 대조된다. 유저 입장에서 PotalCreator를 공략해도 미션 UI 변화가 전혀 없다.

3. **입력 행동**: 유저는 PotalCreator 블록이 놓인 칸 인접 영역에서 고양이 블록 3-매치를 만들어 PotalCreator의 HP를 1씩 감소시킨다. PotalCreator는 매 턴 주변 일반 블록을 Potal 블록으로 변환하므로, 유저는 타이밍을 잡아 여러 번 인접 매치를 연속으로 성공시켜야 한다.

4. **시스템 반응**: GPGameScene이 인접 매치 발생 시 `CheckArround → DamageBlock`을 호출해 PotalCreator HP를 감소시킨다. HP가 0이 되면 PotalCreator가 소멸한다. Data.Collection에 collectionType=46 카운터가 1 증가하지만, 이 카운터와 연결된 Mission.json 항목이 없으므로 CHMData는 어떤 미션 갱신도 트리거하지 않는다.

5. **반복·재발생 패턴**: 47개 스테이지에서 총 200개 셀의 PotalCreator가 등장한다. 유저가 모든 스테이지를 클리어하면 최소 200회 이상 PotalCreator를 제거하게 된다. WallCreator 71개 이정표는 달성 가능한 수치지만, PotalCreator는 같은 수를 훨씬 초과 달성하더라도 아무런 보상이 지급되지 않는다.

6. **종료·해소 조건**: Mission.json에 PotalCreator tapIndex 2 항목(collectionType=46, clearValue=71, addValue=-1, reward=0, rewardCount=1000)을 1행 추가하면 해소된다. 71개 달성 시 미션 클리어 알림과 Gold 1000 보상이 지급되며, 이후 tapIndex 2 미션은 1회성(addValue=-1)이므로 반복 부담 없이 루프가 완성된다.

7. **다른 시스템과 상호작용**: PotalCreator 소멸은 Potal 생성을 중단시켜 스테이지 클리어에 직접 기여한다. 미션 연계 시 Mission 시스템(Data.Mission, UIMission)과 즉각 연동되며, StringKorea/StringEnglish.json에 descStringID 문자열을 1행씩 추가하는 것이 전부다. 기존 WallCreator 미션 로직(missionID=15)과 완전히 동일한 경로를 재사용하므로 코드 변경 없이 데이터만으로 구현 가능하다.

8. **엣지 케이스**: clearValue=71로 WallCreator와 동일하게 설정하면 PotalCreator가 더 많이 등장하므로(200 vs 162) 달성 난이도가 WallCreator보다 낮아진다. 균형을 맞추려면 clearValue를 90 내외로 조정할 수 있다. 또한 노멀 모드는 하드 스테이지 동일 테이블을 공유하므로 노멀·하드 모드 양쪽에서 PotalCreator 처치가 카운터에 반영된다.

9. **유저 정보·피드백**: 현재 유저는 UIMission tapIndex 2 탭을 열면 Fish/CatBox1/WallCreator/RainbowPang/Ball 이정표 진행 바를 볼 수 있다. PotalCreator 항목이 없으므로 "PotalCreator를 얼마나 제거했는지" 파악할 방법이 없다. 미션 추가 후에는 진행 바가 표시되어 유저가 자신의 장애물 공략 누적치를 인식하고 이정표 달성을 목표로 삼을 수 있다.

### 보류

- **CatBox2~5 tapIndex 2 생애 이정표 미션 공백**: CatBox1만 미션 보유, CatBox2~5(333셀·최대 38스테이지) 없음. 구현비용이 4행으로 높아 2위 보류.
- **ESelect 선택 스킬 Mission.json 미연계**: ESelect 6종(Power/Delay/Lotto/AddCat/CatPangUpgrade/Speed) 미션 없음. Select.json 파일 부재로 데이터 근거가 간접적이라 3위 보류.

## 3. 과거 감사 대비 차별성

git log 28건 검토 완료.

가장 유사했던 과거 커밋:
- **80d875f** (2026-07-05) "Wall 블록 tapIndex 1 반복 수집 미션 완전 공백" — 차별점: 해당 커밋은 Wall **블록(blockState=16)**의 tapIndex **1** 반복 수집 미션 부재를 다뤘다. 본 제안은 WallCreator·PotalCreator라는 별도 **Creator 블록(45·46)**의 tapIndex **2** 생애 이정표 미션의 비대칭 구조를 다룬다. Wall ≠ WallCreator, tapIndex 1 ≠ tapIndex 2.
- **6d92d88** (2026-06-22) "CatBox 완성 tapIndex 3 일일 미션 완전 공백" — 차별점: tapIndex 3(일일 미션)이며 CatBox 관련. 본 제안은 tapIndex 2(생애 이정표)이며 PotalCreator 관련으로 블록 타입도 tapIndex도 다르다.

두 과거 커밋과 **카테고리·블록 타입·tapIndex 모두 다름** — 중복 없음.

## 4. 다음 단계 제안

- 채택 시 `Assets/AssetBundleResources/json/Mission.json`에 1행 추가 (missionID 신규 할당, tapIndex=2, collectionType=46, clearValue=71 또는 90, addValue=-1, reward=0, rewardCount=1000)
- `Assets/AssetBundleResources/json/StringKorea.json` 및 `StringEnglish.json`에 descStringID 설명 문자열 1개씩 추가
- 코드 변경 없음 — 기존 WallCreator 미션 처리 로직 재사용

## 5. 쉬운 설명 (비개발자 요약)

게임에는 매 턴 주변 블록을 장애물로 바꿔 방해하는 '생성기 블록'이 두 종류 있다. 벽을 만드는 WallCreator와, 구멍(포탈)을 만드는 PotalCreator다. 둘 다 똑같이 여러 번 인접 매치를 성공시켜야 없앨 수 있는 어려운 블록인데, WallCreator를 71개 없애면 금화 1000개를 주는 보상이 있고, PotalCreator는 200개나 없애도 아무 보상이 없다. 심지어 PotalCreator는 WallCreator보다 게임에 더 많이 등장한다. 그래서 이번에 제안하는 것은: PotalCreator도 비슷한 수만큼 없앴을 때 동일한 보상을 받을 수 있는 이정표 미션을 추가해, 두 종류의 생성기 블록을 공평하게 인정해 주자.
