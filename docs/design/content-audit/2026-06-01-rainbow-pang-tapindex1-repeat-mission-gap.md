# Content Audit — 2026-06-01 — RainbowPang tapIndex 1 반복 미션 완전 누락

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Guide, Mission, Shop, Stage, StageBlock, StringEnglish, StringKorea, Tutorial)
- 과거 감사 이력 (git log): 7건 (가장 최근: 2026-05-31)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 / 전체 84 (빈 슬롯 25~39·47~51 = 20개 제외 시 61종 정의) | 스킨 테마(54~83) 제외 기준 |
| 일일 미션 종류 | EDailyCounter 4종 + EBlockState 기반 1종 = tapIndex 3 총 5건 | Attendance/NormalStageClear/BlockDestroy/AdWatch + CatPang daily |
| 상점 아이템 | 12개 (tapIndex 1 코인 구매: 9개 / tapIndex 2 IAP: 3개) | 스킨 7종 + 아이템 2종 + IAP 3종 |
| 고양이 스킨 | 6테마 × Cat1~5 = 30종 정의 (샵 판매: 7개) | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |
| tapIndex 1 반복 미션 | 17건 | Cat1~5 수집 5건 + Arrow1~6 생성 6건 + CatPang 1건 + 특수폭탄 5건 |
| tapIndex 2 장기 미션 | 5건 | Fish/CatBox1/WallCreator/RainbowPang/Ball |

### 분포 공백

**특수폭탄(EBlockState 19~23, 52) tapIndex 1 미션 커버리지:**

| 블록 | EBlockState | 스테이지 배치 수 | tapIndex 1 반복 미션 | tapIndex 2 장기 미션 |
|---|---|---|---|---|
| PinkBomb  | 19 |  23 | ✅ missionID 8  (clearValue 10) | ❌ |
| YellowBomb| 20 |   7 | ✅ missionID 9  (clearValue 10) | ❌ |
| OrangeBomb| 21 |  13 | ✅ missionID 10 (clearValue 10) | ❌ |
| GreenBomb | 22 |   7 | ✅ missionID 11 (clearValue 10) | ❌ |
| BlueBomb  | 23 |   8 | ✅ missionID 12 (clearValue 10) | ❌ |
| **RainbowPang** | **52** | **139** | **❌ 없음** | ✅ missionID 16 (clearValue 91) |

RainbowPang(52)은 PinkBomb~BlueBomb 5종보다 **스테이지 배치 횟수가 6~20배 많음**(139회 vs 7~23회)에도 불구하고 **tapIndex 1 단기 반복 미션이 전혀 없다.** 5색 특수폭탄과 대조적으로 완전한 구조적 불일치다.

**tapIndex 1 미션 reward 패턴:**
- Cat1~5: reward=0(Gold), rewardCount=100
- Arrow1~6: reward=0(Gold), rewardCount=100
- CatPang: reward=0(Gold), rewardCount=100
- PinkBomb/OrangeBomb/BlueBomb: reward=1(AddTime), rewardCount=1
- YellowBomb/GreenBomb: reward=2(AddMove), rewardCount=1

RainbowPang tapIndex 1은 특수폭탄 패턴(reward=1 또는 2)에 따라 자연스럽게 배치 가능하다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### RainbowPang tapIndex 1 반복 미션 신설

- **카테고리**: 미션
- **요지**: 5색 특수폭탄(PinkBomb~BlueBomb)에는 tapIndex 1 반복 미션(missionID 8~12)이 있는데, 더 강력하고 더 자주 등장하는 RainbowPang(52)에만 tapIndex 1 미션이 없다. Mission.json에 missionID 105 항목 하나를 추가해 구조적 불일치를 해소하는 것을 제안한다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 3/2/3/4 → 종합 **14**
  - 검증가치 3: RainbowPang 생성률 데이터 확보 → 추후 난이도 조정 기준 마련
  - 구현비용 2: Mission.json 1행 추가만 필요, 코드 변경 불필요 (collectionType 52는 tapIndex 2에서 이미 추적 중)
  - 플레이어경험개선 3: 단기 반복 목표가 생겨 강력 폭탄 생성 동기 부여
  - 데이터근거 4: PinkBomb~BlueBomb 5종 tapIndex 1 있음 / RainbowPang tapIndex 1 없음 = Mission.json 내 명백한 구조적 불일치. StageBlock.json 배치 수 RainbowPang 139회 (비교 블록 대비 최대 20배)
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` L19: missionID 16, collectionType 52 (tapIndex 2 존재 확인)
  - `Assets/AssetBundleResources/json/Mission.json` L14~18: missionID 8~12, collectionType 19~23 (PinkBomb~BlueBomb tapIndex 1 모두 커버됨)
  - `Assets/AssetBundleResources/json/StageBlock.json` 분석: blockState=52 배치 139회 (blockState=19~23 합계 58회 대비 2.4배)
  - `Assets/Scripts/Defines.cs` L147: `RainbowPang = 52`

#### 제안 추가 항목 (Mission.json)

```json
{"missionID":"105", "tapIndex":"1", "descStringID":172, "collectionType":52,
 "clearValue":5, "addValue":5, "reward":1, "rewardCount":1}
```

- clearValue 5: PinkBomb~BlueBomb(10)보다 낮게 설정 — RainbowPang이 조건부 생성이라 달성 빈도가 낮음
- reward=1(AddTime), rewardCount=1: 특수폭탄 패턴 일관성 유지
- descStringID 172: 다음 빈 문자열 ID (StringKorea.json 확인 후 확정 필요)

#### 유저 플로우

1. **노출 시점·트리거**: 사용자가 UIMission을 열어 tapIndex 1 탭으로 진입할 때 기존 BlueBomb 미션(missionID 12) 아래에 신규 "RainbowPang 5개 생성" 미션 항목이 표시된다. 첫 플레이 직후부터 노출되며 특별한 진입 조건이 없다.

2. **화면 변화**: UIMission tapIndex 1 탭 스크롤 목록 하단에 새 미션 행이 추가된다. RainbowPang 블록 아이콘(보라색 폭탄류 이미지)과 "N/5 생성" 진행 바, AddTime 보상 아이콘이 기존 PinkBomb~BlueBomb 미션과 동일한 레이아웃으로 표시된다.

3. **입력 행동**: 사용자는 게임 플레이 중 가로 또는 세로 방향의 매치 3개 이상이 동시 달성되거나 기존 보드의 RainbowPang을 발동시켜 카운터를 올린다. 특별한 UI 입력 없이 자동 집계되며, 사용자는 미션 탭으로 돌아와 진행도를 확인하거나 달성 시 수령 버튼을 누른다.

4. **시스템 반응**: RainbowPang 생성(GPBombResolver.CreateBombBlock 경로)이 발생할 때마다 Collection 데이터(collectionType 52)가 +1 카운트된다. clearValue(5, 10, 15, …) 달성 시 DailyMissionService 또는 미션 평가 로직이 EClearState.Clear로 전환하고 보상(AddTime ×1)을 수령 가능 상태로 전환한다. 수령 후 clearValue가 addValue(5)만큼 증가하여 다음 반복 목표가 자동 갱신된다.

5. **반복·재발생 패턴**: tapIndex 1의 addValue(5) 방식에 따라 목표가 5→10→15→20으로 무한 반복 증가하며, 달성할수록 AddTime을 지속적으로 획득한다. RainbowPang 생성이 스테이지 클리어 보상처럼 느껴지도록 설계되어 강력 폭탄 생성을 적극 추구하는 플레이 패턴을 유도한다.

6. **종료·해소 조건**: tapIndex 1 미션은 명시적 종료 없이 무한 반복된다. clearValue 달성마다 UIMission 탭 배지(알림 표시)가 갱신되고 사용자가 수령 버튼을 탭하면 해당 회차가 완료 처리된다. 사용자가 미션을 의도적으로 무시해도 카운터는 계속 쌓이므로 나중에 몰아서 수령 가능하다.

7. **다른 시스템과 상호작용**: collectionType 52는 tapIndex 2(missionID 16, clearValue 91)와 동일 카운터를 공유한다. tapIndex 1 달성으로 카운터가 쌓이면 tapIndex 2 진행도도 동시에 증가하는 시너지 효과가 생긴다. 또한 RainbowPang 발동이 GPBombResolver에서 보드 전체에 색폭탄을 살포하므로 연쇄 폭발이 증가하여 BlockDestroy 일일 미션(missionID 102) 달성에도 간접 기여한다.

8. **엣지 케이스**: (a) tapIndex 2(clearValue 91)와 collectionType 52 공유이므로 누적 카운터가 91을 초과해도 tapIndex 2 달성 판정에 문제가 없는지 확인 필요 — 카운터가 단조 증가이면 정상이지만 addValue 반복 후 카운터 초과 처리를 검증해야 한다. (b) RainbowPang이 보스 스테이지 보드에서도 생성되는지 여부에 따라 카운터 기여 범위가 달라진다. (c) descStringID 172가 StringKorea.json / StringEnglish.json에 각각 추가되지 않으면 텍스트 누락으로 표시 오류가 발생한다.

9. **유저 정보·피드백**: 미션 달성 시 UIMission 탭 아이콘에 배지 알림이 표시되고 사용자가 수령 버튼을 누르면 AddTime 획득 피드백(팝업 또는 플로팅 텍스트)이 노출된다. 연속 달성(카운터가 한 게임에서 clearValue를 두 단계 이상 초과)인 경우 수령 대기 상태가 중첩되지 않도록 UI 스택 처리가 필요하다.

### 보류

- **CatBox2·3·5 tapIndex 2 장기 미션 신설** (과거 audit #4 aeffad1과 카테고리 일부 중복 — 같은 CatBox 계열 tapIndex 2 공백 논지)
- **보스 스테이지 boardSize 다양화** (과거 audit #2 08f1ddb와 카테고리 중복 — 스테이지 포맷 균일성 논지)
- **시즌 한정 스킨 도입** (종합 14점 동률이나 구현비용 3으로 RainbowPang 구현비용 2에 열위)
- **BossStageClear 일일 미션** (과거 audit #6 d819b99의 HardStageClear 신설과 카테고리·요지 과반 중복)

## 3. 과거 감사 대비 차별성

git log 7건 검토 완료.

가장 유사했던 과거 커밋: **b5bcc97** ("미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안")
- 공통점: tapIndex 1 미션 구조 공백 논지
- **차별점**: b5bcc97은 Cat1~5(기본 매치 블록) 수집 미션 부재가 주제. 본 감사는 **특수폭탄 계열 내부의 불일치** — PinkBomb~BlueBomb 5종 tapIndex 1 있음 vs. RainbowPang tapIndex 1 없음. 블록 카테고리(기본 블록 vs. 특수폭탄)와 근거(수집 공백 vs. 동종 블록군 내 불일치)가 모두 다르다.

나머지 6건(a6cb70b/08f1ddb/aeffad1/58c3f65/d819b99/23f1382)은 스테이지 배분·보스 스킬·일일 미션·장애물 블록 tapIndex 2 등 본 제안과 카테고리·요지·근거가 겹치지 않는다.

## 4. 다음 단계 제안

채택 시:
1. `StringKorea.json` / `StringEnglish.json`에 descStringID 172 문자열 추가 ("레인보우팡 N개 생성" / "Generate N RainbowPangs")
2. `Mission.json` missionID 105 항목 추가 (위 JSON 초안 참조)
3. RainbowPang이 보스 스테이지 보드에서도 생성 가능한지 GPBombResolver 확인
4. collectionType 52 카운터 공유로 인한 tapIndex 1·2 동시 집계 엣지 케이스 테스트

---

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 특별한 폭탄을 모으면 보상을 주는 미션이 있는데, 색폭탄 5종(분홍·노랑·주황·초록·파랑)은 미션 목록에 들어가 있지만 그것들보다 훨씬 강하고 게임에서 더 자주 등장하는 '레인보우팡' 폭탄만 빠져있다. 마치 라면 5개 메뉴에는 "10개 먹으면 쿠폰 증정" 이벤트가 있는데 제일 인기 많은 짜장면에만 이벤트가 없는 것과 같다. 레인보우팡은 보드 전체에 폭탄을 뿌리는 초강력 폭탄이라 유저가 만들면 짜릿함을 느끼는데, 미션 보상이 없으니 그 성취감이 보상으로 연결되지 않는다. 그래서 이번에 제안하는 것은: 레인보우팡을 5개 만들 때마다 시간 추가 아이템을 주는 반복 미션을 새로 추가하는 것이다.
