# Content Audit — 2026-07-05 — tapIndex 2 장기 이정표 미션 보상 Gold 단일화: AddTime·AddMove 전무

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log, main 브랜치 기준): 22건 (가장 최근: 2026-06-29 KST, dd924d0 — Fish 블록 tapIndex 1 반복 수집 미션 공백)
  - 주의: 2026-06-30~2026-07-04 KST 기간 5개 커밋이 detached HEAD에서 생성되어 main에 미병합 상태. 해당 주제(RainbowPang daily·Boss tutorial·Guide.json gap·Wall/Potal tutorial·EBackground)도 중복 회피 검토에 반영.

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 / 전체 84 | 빈 슬롯 25~39·47~51 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1건 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch + CatPang daily |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7 + gold 2), tapIndex 2: 3개(IAP) |
| 고양이 스킨 | 6종 × 5마리 = 30 슬롯 | EBlockState 54~83 |

### 분포 공백 — tapIndex별 보상 유형 비율

| tapIndex | 미션 수 | Gold 보상 수 | AddTime 수 | AddMove 수 | Gold 비율 |
|---|---|---|---|---|---|
| 1 (반복 수집) | 17개 | 12개 | 3개 | 2개 | 71% |
| 2 (장기 이정표) | 5개 | **5개** | **0개** | **0개** | **100%** |
| 3 (일일) | 5개 | 3개 | 1개 | 1개 | 60% |

**tapIndex 2 미션 전체 상세 (Mission.json missionID 13~17)**:

| missionID | collectionType | 블록 타입 | clearValue | addValue | reward | rewardCount |
|---|---|---|---|---|---|---|
| 13 | 24 | Fish | 31 | -1 | 0 (Gold) | 1000 |
| 14 | 40 | CatBox1 | 51 | -1 | 0 (Gold) | 1000 |
| 15 | 45 | WallCreator | 71 | -1 | 0 (Gold) | 1000 |
| 16 | 52 | RainbowPang | 91 | -1 | 0 (Gold) | 1000 |
| 17 | 53 | Ball | 131 | -1 | 0 (Gold) | 1000 |

**핵심 발견**: tapIndex 2 이정표 미션 5개는 달성하는 데 수십~수백 판의 플레이가 필요하다 (Fish 31마리부터 Ball 131개까지). 달성 난이도가 tapIndex 1·3보다 압도적으로 높음에도 불구하고, 보상은 Gold 1000으로 단일 고정되어 있으며 AddTime·AddMove가 한 건도 없다. 이는 tapIndex 1(14/17가 Gold만, 5/17가 AddTime/AddMove 포함)·tapIndex 3(3/5가 Gold만, 2/5가 AddTime/AddMove)와 비교하면 구조적 역설이다. 또한 Mission.json 기준 tapIndex 1 Cat 수집 미션(rewardCount=100)보다 10배 높은 1000 Gold지만, Shop.json의 가장 저렴한 gold 상품(shopID 7·8, gold=5000)보다 낮아 단독으로는 상점 구매력이 없다.

### 분포 공백 보조 데이터

| 미션 | 달성 최소 요구 플레이 수 (추정) | 보상 실용성 |
|---|---|---|
| Fish 31마리 (clearValue=31) | Fish는 stage 31부터 등장, 수십 판 | Gold 1000 = 상점 최저가(5000)의 20% |
| Ball 131개 (clearValue=131) | Ball은 stage 131부터 등장, 수백 판 | Gold 1000 = 상점 최저가(5000)의 20% |
| 동일 Gold 1000 보상 vs AdWatch 1회 달성 | 몇 초 | Gold 100 + 즉각적 맥락 없음 |

### 과거 감사 후보 (git log 조회 결과 — 최근 10건)

| 날짜(KST) | 커밋 SHA | 설명 | main 포함 |
|---|---|---|---|
| 2026-07-04 | 39d8ced | EBackground 4종 미참조 — 게임판 배경 커스터마이징 루프 공백 | ⚠️ orphaned |
| 2026-07-03 | 5c2ba4c | Wall·Potal 초등장 스테이지(6·7) Tutorial.json 완전 공백 | ⚠️ orphaned |
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 고아 항목 | ⚠️ orphaned |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 — 100개 전부 tutorialID=-1 | ⚠️ orphaned |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 | ⚠️ orphaned |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 — stage 31 등장 블록 | ✅ main |
| 2026-06-27 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 시점 후기 도입 | ✅ main |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 | ✅ main |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약 밀스톤 스테이지 완전 부재 | ✅ main |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 | ✅ main |

## 2. 추가 컨텐츠 후보 (권장 1개)

### tapIndex 2 장기 이정표 미션 보상 단일화 해소 — Gold 고정 5/5에 AddTime·AddMove 도입

- **카테고리**: 미션 보상 밸런스
- **요지**: tapIndex 2 이정표 미션 5개(missionID 13~17) 전체가 Gold 1000 단독 보상으로 고정되어 있다. tapIndex 1(29%가 AddTime/AddMove)·tapIndex 3(40%가 AddTime/AddMove)과 달리, 달성 난이도가 가장 높은 tapIndex 2에만 게임 플레이를 직접 보조하는 아이템 보상이 전무한 구조적 역설이 존재한다.
- **점수**: 검증가치 3 / 구현비용 1 / 플레이어경험개선 4 / 데이터근거 5 → 종합 **17점**
  - 3 + (6-1) + 4 + 5 = 17
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` missionID 13~17: reward 필드 5건 모두 0 (Gold), rewardCount 5건 모두 1000
  - `Assets/Scripts/Defines.cs` EReward enum: Gold=0, AddTime=1, AddMove=2 — 3종 보상 유형 정의
  - Mission.json tapIndex 1 보상 비교: missionID 8(PinkBomb→AddTime1), 9(YellowBomb→AddMove1), 10(OrangeBomb→AddTime1), 11(GreenBomb→AddMove1), 12(BlueBomb→AddTime1) — 5/17 AddTime/AddMove
  - Mission.json tapIndex 3 보상 비교: missionID 102(BlockDestroy→AddTime1), 103(AdWatch→AddMove10) — 2/5 AddTime/AddMove
  - `Assets/AssetBundleResources/json/Shop.json` shopID 7·8: gold=5000 — tapIndex 2 보상 1000 Gold < 상점 최저가의 20%
  - `Assets/Scripts/Data.cs` Data.Login: addTimeItemCount, addMoveItemCount 필드 존재 — 아이템 지급 인프라 기이식

#### 유저 플로우

1. **노출 시점·트리거**
   플레이어가 스테이지를 플레이하다 특정 블록(예: Fish, Ball)의 수집 누계가 Mission.json 기준 clearValue(31, 51, 71, 91, 131)를 돌파하는 순간, UIMission tapIndex 2 탭 진입 시 해당 항목에 "보상 수령 가능" 표시가 생긴다. 보상 수령은 UIMission 화면에서 해당 미션 항목을 탭해야 이루어지며, 스테이지 종료 후 자동 팝업 방식이 아닌 플레이어 주도 수령 방식이다.

2. **화면 변화**
   UIMission tapIndex 2 탭을 열면 달성된 이정표 항목의 진행 바가 꽉 찬 상태로 바뀌고, 보상 아이콘이 반짝이는 대기 애니메이션을 재생한다. 현재 Gold 코인 아이콘 자리에 AddTime(시계) 또는 AddMove(이동 화살표) 아이콘이 표시되도록 변경하면, 플레이어는 수령 전부터 어떤 종류의 보상이 기다리는지 시각적으로 인지할 수 있다. tapIndex 1·3 미션에서 동일한 아이콘 표시 로직이 이미 작동하므로, 아이콘 변경은 Mission.json 값 수정만으로 자동 반영될 가능성이 높다.

3. **입력 행동**
   플레이어는 보상 대기 중인 이정표 항목을 탭한다. 현재 tapIndex 1·3 미션에서처럼 단순 탭 한 번으로 즉시 보상이 지급되거나, AddTime/AddMove의 경우 UIConfirm 팝업("AddMove 3개를 받으시겠습니까?")을 통해 확인 후 지급하는 두 방식 중 UX 정책에 따라 선택한다. 후자는 플레이어가 보상 내용을 한 번 더 인지하게 해 체감을 높인다.

4. **시스템 반응**
   AddMove 보상 시 `Data.Login.addMoveItemCount += rewardCount` 처리 후 `CHMData.SaveData()`로 로컬 및 클라우드에 즉시 저장된다. AddTime 보상이면 `addTimeItemCount`가 동일하게 증가한다. 해당 미션의 `EClearState`는 `Clear`로 전환되어 이후 재수령이 불가능하다. tapIndex 2의 `addValue=-1` 특성상 이 전환은 플레이어 생애 단 1회만 발생한다.

5. **반복·재발생 패턴**
   tapIndex 2 미션은 `addValue=-1`(비반복형)이므로 각 이정표 보상 수령은 전 플레이어 생애 1회에 한정된다. 5개의 이정표는 블록 누계 31→51→71→91→131 순으로 도달하므로 자연스러운 5단계 성취 루프를 형성한다. 마지막 Ball 이정표(131)는 Ball이 stage 131 이후에만 등장한다는 특성상 노멀·하드 스테이지 후반부를 상당 수 진행한 플레이어만 도달할 수 있어, AddMove 3개 등의 보상이 막힌 후반 스테이지를 돌파하는 데 직접 활용될 가능성이 높다.

6. **종료·해소 조건**
   5개 tapIndex 2 이정표를 모두 수령하면 UIMission tapIndex 2 탭이 "모두 완료" 상태로 고정된다. 각 항목은 수령 직후 "완료" 표시로 전환되고 재탭해도 반응하지 않는다. 이정표 달성 조건인 블록 수집 누계는 `Data.Collection`의 key-value 구조에서 계속 증가하지만, clearState=Clear 상태에서는 미션 로직에 영향을 주지 않는다. 완료 후에는 tapIndex 2 탭에 남은 미션이 없으므로, 추가 이정표 신설(예: Fish 62마리 2차 이정표)이 있어야 탭이 다시 활성화된다.

7. **다른 시스템과 상호작용**
   AddMove는 UIGameStart에서 스테이지 진입 전 이동 횟수를 추가하거나, UIGameEnd에서 이동 소진 직전 아이템을 사용하는 두 시점 모두에서 소비 가능하다. AddTime은 시간 제한 스테이지(Stage.json time>0)에서만 유효하므로, WallCreator 이정표(clearValue=71)에 AddTime을 배정하면 WallCreator가 등장하는 stage 71 전후 시간 제한 스테이지에서 직접 활용 가능한 보상이 된다. 두 아이템 모두 `Data.Login.addMoveItemCount`·`addTimeItemCount` 필드로 관리되므로 기존 Shop IAP 구매(shopID 4·5)와 동일한 재고 풀에서 소비된다.

8. **엣지 케이스**
   ① 이미 tapIndex 2 이정표를 달성·완료한 기존 플레이어(clearState=Clear)는 Mission.json 보상 값 변경 전 Gold 1000을 이미 수령한 상태다 — 소급 지급 여부를 정책적으로 결정해야 하며 권장은 미소급(기존 완료 상태 존중)이다. ② `addMoveItemCount`·`addTimeItemCount`가 int 최대값 근처인 경우 오버플로우가 발생할 수 있으나, 보상 수치(예: 2~5)가 매우 작아 실질적 위험은 낮다. ③ Mission.json 변경 후 강제 업데이트 없이 구버전 클라이언트가 reward=1/2를 알 수 없는 값으로 처리할 경우 보상 지급이 누락될 수 있으므로, 클라이언트 버전 별 reward 타입 파싱 로직을 확인해야 한다. 단, tapIndex 1 미션에서 이미 reward=1/2가 사용되고 있으므로 기존 클라이언트는 해당 값을 올바르게 파싱한다.

9. **유저 정보·피드백**
   플레이어는 Ball 131개 수집이라는 수백 판에 걸친 달성 직후 Gold 1000 코인만 받는 현재 구조에서 체감 보상이 낮다고 느낄 가능성이 높다 — 상점에서 Gold 1000으로는 아무것도 구매할 수 없기 때문이다. AddMove 3개로 변경하면 "막혀 있던 스테이지를 한 번 더 시도할 수 있다"는 즉각적이고 실용적인 동기가 생긴다. UIMission tapIndex 2의 보상 아이콘이 Gold → AddMove/AddTime으로 바뀌는 것 자체가 "이 이정표는 뭔가 다른 보상"이라는 시각적 신호가 되어 탭의 가치를 높인다.

### 보류

- **tapIndex 2 clearValue 단계 간격 불균형** (Fish 31→CatBox1 51→WallCreator 71→RainbowPang 91→Ball 131: 간격 20·20·20·40으로 마지막만 2배) — 종합 점수 14. clearValue 변경은 이미 미션 진행 중인 플레이어의 저장 데이터(startValue) 충돌 가능성이 있어 추가 검증 비용 발생, 이번 보상 단일화 이슈가 더 우선.
- **tapIndex 2 rewardCount 1000 Gold 구매력 부족 단독 제안** — 종합 점수 12. 보상 유형 단일화 문제와 본질적으로 같은 뿌리를 갖고 있어 이번 제안에 포함시킬 수 있으며, 별도 감사로 분리할 필요가 없다.
- **tapIndex 1 Cat1~5 미션 rewardCount=100 Gold 동일화** (Cat5 수집이 Cat1보다 어렵지 않음에도 동일 보상) — 종합 점수 11. 난이도 차별화 근거 데이터가 StageBlock.json 심화 분석 없이는 약하고, tapIndex 2 보상 구조 문제가 더 직접적.

## 3. 과거 감사 대비 차별성

git log main 기준 22건 + detached HEAD orphaned 5건 = 총 27건 검토.

- **2026-06-03 (daily-mission-reward-imbalance-adwatch)**: tapIndex 3 일일 미션의 AdWatch 보상값(AddMove 10) 불균형 — 일일 미션 내부 보상값 비교. 이번 제안은 미션 계층(tapIndex) 간 보상 유형 분포 비교로, 대상 tapIndex과 이슈 유형이 다르다.
- **2026-06-04 (shop-skin-price-vs-daily-reward-gap)**: 상점 스킨 가격 대비 일일 보상 총량 격차 — 상점 vs 일일 미션 간 경제 비교. 이번 제안은 tapIndex 2 미션 내부 보상 유형 단일화 문제다.
- **2026-06-14 (b0ee2e0, 5색 특수폭탄 tapIndex 2 이정표 미션 완전 부재)**: 화살표·특수폭탄 블록 타입에 대한 tapIndex 2 미션이 아예 없다는 "미션 존재 누락" 제안. 이번 제안은 "기존 5개 tapIndex 2 미션의 보상 유형이 Gold 단독"이라는 "보상 설계 단일화" 문제 — 존재 유무가 아닌 보상 구조의 이슈다.
- **2026-06-09 (7528a26, 아이템 사용 미션 연계 공백)**: AddTime·AddMove를 구매(IAP)한 뒤 사용하는 행위가 미션으로 연결되지 않는 루프 단절 — IAP → 사용 행위 미션화 제안. 이번 제안은 이정표 달성 → 아이템 지급이라는 방향이 반대이며, 일반 미션 이정표의 보상 유형 설계가 대상이다.
- orphaned 5건(2026-06-30~07-04): RainbowPang daily / Boss tutorial / Guide.json gap / Wall·Potal tutorial / EBackground — 모두 이번 제안과 카테고리·근거가 다름.
- 27건 전체 중 "tapIndex 2 이정표 미션의 보상 유형(Gold vs AddTime/AddMove) 다양성"을 직접 다룬 커밋: **0건**.

## 4. 다음 단계 제안

채택 시 구체 구현 계획 수립 필요:

1. Mission.json missionID 15 (WallCreator, clearValue=71) → `reward: 1` (AddTime), `rewardCount: 2` 로 변경 — WallCreator 첫 등장(stage 71) 인근 시간 제한 스테이지와 보상 연관성 확보
2. Mission.json missionID 17 (Ball, clearValue=131) → `reward: 2` (AddMove), `rewardCount: 3` 으로 변경 — 가장 어려운 이정표에 가장 유용한 아이템 배정
3. 기존 완료(clearState=Clear) 플레이어 소급 지급 여부 정책 결정 (권장: 미소급)
4. UIMission tapIndex 2 보상 아이콘 표시가 reward=1/2를 올바르게 렌더링하는지 확인 (tapIndex 1에서 이미 동작 예상, 추가 코드 불필요 가능성 높음)
5. StringKorea/StringEnglish 보상 안내 문자열(descStringID 164·166 연관) 업데이트 여부 검토

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 미션이 세 종류 있는데, 그중 "장기 이정표 미션"이 가장 어렵다. 예를 들어 "물고기 블록을 31개 없애라", "볼 블록을 131개 없애라" 같은 것들인데, 이걸 달성하려면 게임을 수십 판에서 수백 판 해야 한다. 그런데 막상 이 힘든 미션들을 달성하면 보상은 동전(Gold) 1000개뿐이다 — 상점에서 가장 싼 물건이 5000개짜리이니, 그 보상으로는 아무것도 살 수 없다. 반면, 매일 광고를 딱 한 번만 봐도 이동권(AddMove)을 주고, 폭탄 블록 10개를 모으면 시간 추가권(AddTime)을 주는데, 정작 제일 힘든 이정표 미션에만 게임에서 실제로 쓸모 있는 아이템이 하나도 없다. 그래서 이번에 제안하는 것은: 5개 이정표 중 최소 2개의 보상을 Gold 동전이 아닌 AddMove(이동 추가)나 AddTime(시간 추가)으로 바꿔서, 오래 플레이한 사람이 진짜로 도움이 되는 보상을 받게 하자는 것이다.
