# Content Audit — 2026-06-10 — 아이템 사용(AddTime·AddMove) 미션 연계 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StringEnglish, StageBlock, StringKorea, Tutorial)
- 참조 소스 코드: Data.cs, Defines.cs
- 과거 감사 이력 (git log): 16건 (가장 최근: 2026-06-08 KST → commit 85ac09c)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯(25~39, 47~51) 23개 제외 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | 스킨 7개 + RemoveAD + AddTime + AddMove + 골드상품 2개 (IAP 또는 골드 소비) |
| 고양이 스킨 | 6종 테마 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |
| 아이템 사용 추적 | Data.Login: useTimeItemCount / useMoveItemCount 존재 | 미션 연계 0건 |

### 아이템 사용 현황 분석

ConstValue.json 기준:
- `AddMoveItemValue = 1` (variable=5) — 이동 추가 아이템 1개 사용 시 이동 횟수 +1
- `AddTimeItemValue = 10` (variable=6) — 시간 추가 아이템 1개 사용 시 시간 +10초

Shop.json 기준:
- `shopID=4` AddTime — `productName="AddTime"`, `gold=-1` (IAP 실결제 상품)
- `shopID=5` AddMove — `productName="AddMove"`, `gold=-1` (IAP 실결제 상품)

Data.Login 기준 (Assets/Scripts/Data.cs:22-24):
```
public int addTimeItemCount = 0;   // 보유 수량
public int addMoveItemCount = 0;   // 보유 수량
public int useTimeItemCount = 0;   // 누적 사용 횟수 ← 미션 연계 없음
public int useMoveItemCount = 0;   // 누적 사용 횟수 ← 미션 연계 없음
```

Mission.json의 27개 미션 전체를 검토한 결과: `collectionType` 또는 `dailyCounter` 어느 쪽에도 아이템 사용 횟수를 참조하는 항목이 없다. `useTimeItemCount`/`useMoveItemCount` 필드가 저장·갱신되고 있음에도 어떤 보상 루프와도 연결되지 않은 상태.

### Mission.json 일일 미션(tapIndex 3) 현황

| missionID | dailyCounter | clearValue | 보상 |
|---|---|---|---|
| 100 | 0 (Attendance) | 1 | Gold 100 |
| 101 | 1 (NormalStageClear) | 3 | Gold 300 |
| 102 | 2 (BlockDestroy) | 100 | AddTime ×1 |
| 103 | 3 (AdWatch) | 1 | AddMove ×10 |
| 104 | collectionType=18(CatPang) | 3 | Gold 200 |

**아이템 사용(ItemUse) = EDailyCounter 미정의 상태. tapIndex 3에 5개 항목이 있으며 아이템 관련 항목 없음.**

### 과거 감사 후보 (git log 조회 결과)

| 날짜(KST) | SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | tapIndex 1 Cat1~5 수집 미션 공백 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 (10% → 30~40% 확대) |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 누락 |
| 2026-05-31 | 4a09a70 | RainbowPang 반복 미션 누락 제안 |
| 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 누락 |
| 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회가 BlockDestroy 100개의 10배 |
| 2026-06-03 | 1d91056 | 상점 스킨 골드 가격 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-05 | 7d601d5 | ESelect 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-06 | 26ebd5a | 하드 스테이지 해금 임계값 150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 2026-06-07 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 |
| 2026-06-08 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### 아이템 사용(AddTime·AddMove) 일일 미션 신설

- **카테고리**: 일일 미션 / 수익화 루프
- **요지**: Data.Login에 `useTimeItemCount`/`useMoveItemCount` 누적 사용 횟수 필드가 이미 저장되고 있으나, Mission.json의 27개 미션 어디에도 아이템 사용에 대한 보상이 없다. IAP로 구매한 AddTime·AddMove를 사용할 때마다 어떤 달성 보상도 없어 아이템 구매자의 재활성화 루프가 단절된 상태.
- **점수**: 검증가치 4 / 구현비용 3 / 플레이어경험개선 4 / 데이터근거 5 → **종합 16**
- **근거**:
  - `Assets/Scripts/Data.cs:22-24` — `addTimeItemCount`, `addMoveItemCount`, `useTimeItemCount`, `useMoveItemCount` 네 필드가 존재. 보유량과 사용량 양쪽이 분리 저장됨.
  - `Assets/Scripts/Defines.cs:310-326` — `EDailyCounter`에 `None(-1)`, `Attendance(0)`, `NormalStageClear(1)`, `BlockDestroy(2)`, `AdWatch(3)` 5개만 정의. `ItemUse`에 해당하는 값 없음.
  - `Assets/AssetBundleResources/json/Mission.json` — tapIndex=3 항목 5개 전체의 `dailyCounter` 값이 0~3 범위이며, 아이템 사용(useTimeItemCount / useMoveItemCount)을 트리거로 하는 항목 없음.
  - `Assets/AssetBundleResources/json/Shop.json:shopID=4,5` — AddTime, AddMove가 `gold=-1`(IAP 실결제 상품). 비용이 발생하는 상품임에도 사용 시 보상 루프가 없음.
  - `Assets/AssetBundleResources/json/ConstValue.json:variable=5,6` — `AddMoveItemValue=1`, `AddTimeItemValue=10`. 아이템 하나의 효과가 매우 작음(이동 +1, 시간 +10초). 사용 보상 없이는 구매 동기가 낮음.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   플레이어가 GameScene에서 이동 횟수를 소진하거나 시간이 거의 다 됐을 때, UIGameEnd 직전에 AddTime·AddMove 아이템 사용 버튼이 활성화된다. 또는 게임 중 UI에서 직접 아이템 버튼을 탭해 사용할 수 있다. 아이템을 1회 사용하는 순간 `useTimeItemCount`/`useMoveItemCount`가 각각 +1 증가한다. 현재는 이 순간 아무런 미션 진행도 갱신되지 않는다.

2. **화면 변화**
   아이템 사용 직후 보드 위에 남은 시간(+10초) 또는 이동 횟수(+1)가 갱신된다. 기존 UIMission(tapIndex 3 일일 탭)에 신규 "아이템 1회 사용" 항목이 추가되면, 아이템 사용 직후 해당 미션 행의 진행 바가 0→1로 채워지고 달성 시 골드 보상 팝업이 표시된다.

3. **입력 행동**
   플레이어는 GameScene 내 아이템 버튼을 탭하거나 UIGameEnd의 연장 버튼을 탭한다. 탭 1회로 AddTime 또는 AddMove 보유량이 -1, 사용량이 +1 변경된다. 하루에 여러 번 사용할 수 있으나, 일일 미션 clearValue를 1로 설정하면 첫 1회 사용만 달성 트리거가 된다.

4. **시스템 반응**
   아이템 사용 시 `DailyMissionService.OnItemUsed()` 훅(신규)이 호출된다. 내부에서 `CheckAndResetIfNeeded()`를 선행 호출한 뒤 `Data.Login.itemUseCountToday++`(신규 필드)를 증가시키고, EDailyCounter.ItemUse(=4) 조건의 일일 미션 달성 여부를 판정한다. clearValue=1을 초과하면 달성 상태로 전환되고 Gold 보상이 저장된다.

5. **반복·재발생 패턴**
   매일 자정(NTP 기준) `DailyMissionService.CheckAndResetIfNeeded()`가 `itemUseCountToday`를 0으로 리셋한다. 따라서 매일 아이템을 1회 이상 사용하면 매일 미션 보상을 받을 수 있다. 아이템을 보유하고 있는 한 매일 반복적으로 소비 루프가 성립하며, 미션 보상 골드로 스킨 구매에 접근할 수 있다.

6. **종료·해소 조건**
   당일 아이템 사용 횟수가 clearValue(예: 1) 이상이 되는 순간 달성 상태로 전환되고 미션 보상이 지급된다. 그날 이후 추가 사용은 미션 달성에 영향을 주지 않으며 다음날 자정까지 대기 상태가 된다. 아이템 보유량이 0인 날에는 미션 달성이 불가능하므로 아이템을 추가로 구매(IAP)하거나 보스 스테이지 클리어 등 다른 경로로 획득해야 한다.

7. **다른 시스템과 상호작용**
   `EReward.Gold(0)` 보상을 지급하므로 기존 골드 → 스킨 구매 루프와 연결된다. `DailyMissionService`의 자정 리셋 체계(NTP 기반)를 그대로 활용하므로 위변조 방어를 별도로 구현할 필요가 없다. Shop.json의 AddTime/AddMove IAP 구매 → 보유 → 사용 → 미션 달성 → 골드 획득 → 스킨 구매 흐름이 완성되어, IAP 구매자의 참여 루프가 게임 내 경제와 연결된다. tapIndex 3 일일 미션 탭에 항목 1개 추가로 UI 레이아웃 변경이 최소화된다.

8. **엣지 케이스**
   아이템 보유량이 0인 상태에서는 아이템 버튼 자체가 비활성화되므로 `OnItemUsed` 훅이 호출될 일이 없어 데이터 오염이 없다. 아이템 사용 도중 앱이 강제 종료되면 useTimeItemCount 또는 useMoveItemCount의 저장 시점에 따라 카운터가 누락될 수 있으나, 미션 쪽 itemUseCountToday와 useXItemCount가 둘 다 갱신되는 트랜잭션 순서를 보장해야 한다. dailyCollectionSnapshotJson의 기존 구조에 아이템 카운터를 추가할 경우 스냅샷 키 충돌 여부를 확인해야 한다.

9. **유저 정보·피드백**
   IAP로 AddTime/AddMove를 구매한 플레이어는 이미 지불 의향이 있는 핵심 과금 유저다. 이들에게 "아이템을 사용하면 골드를 받을 수 있다"는 미션 보상을 제공하면 아이템을 아끼지 않고 적극적으로 사용하게 된다. 아이템을 더 자주 소진할수록 재구매 필요성이 증가하고, 재구매 → 미션 달성 → 골드 획득 → 스킨 구매 → 재방문 의향 증가의 정(正)의 피드백 루프가 형성된다. 현재 아이템 사용에 대한 어떤 시각적·점수적 피드백도 없어 구매 후 아이템을 쌓아두는 행동이 발생할 수 있으며, 미션 연계는 이를 해소하는 가장 낮은 구현 비용의 개입이다.

### 보류

- **보스 스테이지 클리어 일일 미션(EDailyCounter.BossStageClear 신설)**: 유사 과거 커밋 d819b99(HardStageClear 신설)와 카테고리(일일 미션 카운터 확장)·접근 방식이 동일하다. 대상이 하드→보스로 다르지만, 검증가치 3, 구현비용 2, 플레이어경험 3, 데이터근거 3 → 종합 13으로 이번 권장안보다 낮아 보류.
- **Cat6/Cat7 스킨 테마 비대칭(EBlockState Cat6/Cat7 스킨 없음)**: 스킨 테마 6종이 Cat1~5에만 존재하고 Cat6/Cat7은 기본 외형만 남아 있음. 아트 에셋 12종(6테마 × 2) 추가 필요로 구현비용 4, 종합 12. 유사 과거 커밋 158d246(Cat6/Cat7 미션 공백)과 대상이 동일해 혼동 가능하므로 보류.

---

## 3. 과거 감사 대비 차별성

git log 16건 검토 완료.

가장 유사했던 과거 커밋: **d819b99** (2026-05-29 — 하드 스테이지 클리어 일일 미션, EDailyCounter.HardStageClear 신설) — EDailyCounter에 새 카운터를 추가한다는 접근 방식이 같다.

**차별점**: 과거 제안은 "플레이 행동(스테이지 클리어)"을 카운터 트리거로 삼는다. 이번 제안은 "소비재 아이템 사용(IAP 실결제 상품 사용)"을 트리거로 삼으며, 핵심 근거가 `useTimeItemCount`/`useMoveItemCount` 필드가 이미 저장되고 있음에도 미션 연계가 완전히 없다는 "dormant 데이터" 공백이다. 수익화 루프(IAP 구매자 재활성화)라는 비즈니스 관점의 차별성도 있다. 나머지 15건은 블록 수집, 스테이지 포맷, 보스 시스템, 상점 가격에 집중했으며 IAP 아이템 사용 후 보상 루프 단절은 미분석.

---

## 4. 다음 단계 제안

- **검증 우선**: `useTimeItemCount`/`useMoveItemCount`가 실제로 정상 갱신되는지 GameScene 코드에서 확인 (저장 경로가 있는지, 아이템 사용 시점에 increment가 있는지)
- **채택 시 구현 순서**:
  1. `Defines.cs` — `EDailyCounter`에 `ItemUse = 4` 추가
  2. `Data.cs` — `Data.Login`에 `public int itemUseCountToday = 0;` 추가
  3. `DailyMissionService.cs` — `OnItemUsed()` 훅 추가 + 자정 리셋 시 `itemUseCountToday = 0` 포함
  4. `Mission.json` — tapIndex=3 신규 항목 추가 (dailyCounter=4, clearValue=1, reward=0, rewardCount=100)
- **clearValue 조정 권고**: 1로 설정 시 진입 장벽이 낮아 IAP 비구매자도 달성 불가능하지 않으나, 아이템을 보유하지 않으면 달성 자체가 막히므로 구매 동기가 자연스럽게 생김

---

## 5. 쉬운 설명 (비개발자 요약)

캣팡에는 시간을 늘려주거나 이동 횟수를 추가해주는 아이템이 있는데, 이 아이템은 돈을 내고 사야 해요. 그런데 정작 이 아이템을 쓴다고 해서 게임 안에서 어떤 보상이 주어지지는 않아요. 마치 음식점에서 돈을 냈는데 포인트 적립이 전혀 안 되는 것처럼, 구매자 입장에서는 아이템을 쓸수록 그냥 소모되기만 하는 느낌이에요. 게임 내부를 보면 "아이템을 몇 번 썼는지"는 이미 기록하고 있는데, 그 기록이 아무런 보상과도 연결되어 있지 않아 낭비되고 있는 상태입니다. 그래서 이번에 제안하는 것은: 아이템을 하루에 한 번만 사용해도 골드 보상을 주는 일일 미션을 하나 추가해, 아이템 구매자가 게임에 더 자주 들어오고 더 활발하게 즐길 수 있도록 동기를 만들어 주는 것입니다.
