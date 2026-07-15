# Content Audit — 2026-07-14 — BlockDestroy 일일 미션 보상 단가 비대칭 신설 제안

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StringEnglish, StageBlock, StringKorea, Tutorial)
- 과거 감사 이력 (git log): 33건 (가장 최근: 2026-07-13 — CatBox1~5 tapIndex 1 반복 수집 미션 완전 공백)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행(공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 슬롯 | 빈 슬롯 23개(7~9·25~39·47~51) 제외 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance·NormalStageClear·BlockDestroy·AdWatch |
| 상점 아이템 | 12개 | 스킨 7종 + IAP 3종(RemoveAD·AddTime·AddMove) + 보스 업그레이드 2종 |
| 고양이 스킨 | 6종 | CatCrown·CatFlowers·CatMushroom·CatParty·CatSanta·CatStrawberry (각 Cat1~5) |

### 일일 미션 4종 보상 현황 (Mission.json 전체 tapIndex 3)

| missionID | dailyCounter | clearValue (목표) | reward | rewardCount | 실제 획득 자원 | 추정 소요 시간 |
|---|---|---|---|---|---|---|
| 100 | Attendance (0) | 1회 (앱 열기) | Gold | 100 | 골드 100 | ~5초 |
| 101 | NormalStageClear (1) | 3회 클리어 | Gold | 300 | 골드 300 | ~15~30분 |
| 102 | BlockDestroy (2) | **100개** 파괴 | AddTime (1) | **1** | AddTime × 1 (사용 시 +10초) | **~30~45분** |
| 103 | AdWatch (3) | **1회** 시청 | AddMove (2) | **10** | AddMove × 10 (개당 +1회) | **~30초** |

- ConstValue.json 기준: AddMoveItemValue = **1** (개당 이동횟수 +1), AddTimeItemValue = **10** (개당 시간 +10초)

### 분포 공백

**핵심 비대칭 발견:**
- BlockDestroy 100개 → AddTime **1개** (+10초) → 획득 단가: 블록 100개 = 아이템 1개
- AdWatch 1회 → AddMove **10개** (+총 10이동) → 획득 단가: 광고 1회 = 아이템 10개
- 노력 배율: BlockDestroy가 광고 대비 **약 100배** 어렵지만, 아이템 수령량은 **1/10**
- 결과적으로 블록을 열심히 파괴하는 플레이어가 광고만 보는 유저보다 보상 효율이 100분의 1

### 과거 감사 후보 (git log 조회 결과 — 최근 10건)

| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-07-13 | d2c02fa | CatBox1~5 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-12 | c1bc4d1 | 보스 스탯 업그레이드 아이템 tapIndex 1 혼재·1회 구매 한도 |
| 2026-07-11 | 5a56780 | Cat6·Cat7 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-07-10 | d043062 | WallCreator tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-09 | 9ae50e1 | PotalCreator tapIndex 2 생애 이정표 미션 완전 공백 |
| 2026-07-08 | d371085 | 보스 스테이지 Stage.json 스킬 조율 필드 전무 |
| 2026-07-07 | 1324855 | 보스 스테이지 attack 잠든 공격력 스탯 |
| 2026-07-06 | 80d875f | Wall 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-05 | 18dd561 | tapIndex 2 장기 이정표 미션 보상 Gold 단일화(AddTime·AddMove 전무) |
| 2026-07-04 | 39d8ced | EBackground 4종 Defines.cs 단독 정의·완전 미참조 |

(전체 33건 조회 — 2026-06-08 ~ 2026-07-13)

## 2. 추가 컨텐츠 후보 (권장 1개)

### BlockDestroy 일일 미션 rewardCount 재조정 — 100블록 파괴 보상을 AdWatch 대비 노력 비례로 상향

- **카테고리**: 일일 미션 (tapIndex 3) 보상 밸런스
- **요지**: 매일 블록 100개 파괴(2~3판·약 30~45분)의 일일 미션 보상이 AddTime 1개에 불과해, 광고 1회 시청(30초)으로 AddMove 10개를 받는 것보다 노력 효율이 100배 낮다. missionID 102의 rewardCount를 1→5로 조정하면 플레이 기반 보상 루프가 복원된다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/1/4/5 → 종합 **18**
  - 검증가치 4: A/B 테스트 지표(일일 미션 완료율·블록 파괴 세션 수) 단일 수치 변경으로 즉시 측정 가능
  - 구현비용 1: Mission.json missionID 102의 `rewardCount` 값 1→5 수정 1줄. 코드 변경 없음
  - 플레이어경험 4: 공정한 보상 구조 → 플레이 동기 유지 → 일일 세션 체류 시간 증가 기대
  - 데이터근거 5: Mission.json + ConstValue.json에서 clearValue·rewardCount·AddTimeValue 정확한 수치 확인
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 102: `"dailyCounter":2, "clearValue":100, "reward":1, "rewardCount":1`
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 103: `"dailyCounter":3, "clearValue":1, "reward":2, "rewardCount":10`
  - `Assets/AssetBundleResources/json/ConstValue.json` — variable 5 (AddMoveItemValue): value **1**, variable 6 (AddTimeItemValue): value **10**
  - 한 게임에서 파괴 가능한 블록 수 추정: 9×9=81 최대 블록, 평균 매치당 3~5개 소멸 → 1판 약 30~50개 파괴 → clearValue 100 달성에 2~3판 필요

#### 유저 플로우

1. **노출 시점·트리거**
   플레이어가 UIMission(tapIndex 3 일일 탭)을 열면 `missionID 102` "블록 N개 파괴" 진행 바가 보인다. 세션 시작 전 확인하거나, GPGameScene에서 매치 후 자동 카운터가 갱신될 때 알 수 있다. 현재는 100개 파괴 후 AddTime 1개를 수령하면 루프가 끝나 추가 동기가 소멸한다.

2. **화면 변화**
   개선 후 missionID 102 완료 시 "AddTime × 5" 수령 팝업이 뜬다. 기존 × 1 대비 5배 강조 연출이 가능하다. 보상 팝업에서 아이템 아이콘이 5개 쌓이는 시각 효과를 추가하면 성취감이 증폭된다.

3. **입력 행동**
   플레이어는 평소처럼 게임을 진행하며 블록을 매치·파괴한다. 별도 추가 행동 없이 기존 플레이 흐름이 그대로 트리거가 된다. NormalStageClear(3회)와 병행하면 두 일일 미션을 동시에 달성할 수 있다.

4. **시스템 반응**
   GPGameScene의 RemoveMatchBlock 호출마다 `blockDestroyCountToday++`(Data.Login)가 갱신되고, DailyMissionService.OnBlockDestroyed가 카운터를 체크한다. clearValue(100)에 도달하면 Mission clearState를 Clear로 바꾸고 AddTime × 5를 지급하도록 rewardCount를 5로 설정한다.

5. **반복·재발생 패턴**
   일일 자정(NTP 기준) 리셋 후 blockDestroyCountToday가 0으로 초기화되어 다음 날 재도전 가능하다. 스테이지 1~3판이 하루 블록 파괴 목표에 자연스럽게 기여하므로 강제 파밍 없이 정상 플레이 중 달성된다.

6. **종료·해소 조건**
   blockDestroyCountToday ≥ 100에서 보상을 수령하면 tapIndex 3 탭의 해당 항목이 "완료" 상태로 전환된다. 그날 이후 추가 파괴는 카운터가 올라가지 않는다(이미 Clear). 다음 자정에 리셋.

7. **다른 시스템과 상호작용**
   - AddTime 아이템 5개는 Shop 상품(shopID 4, IAP)과 동일 자원이므로 미션 완료만으로도 시간제한 스테이지 연장 아이템을 획득할 수 있어 IAP 구매 대안 가치가 생긴다.
   - missionID 104(CatPang 블록 3개 일일)와 병행 진행 가능 — 두 미션 모두 정상 플레이 중 달성되는 구조다.
   - GPBombResolver의 폭탄 연쇄(Bomb1~Bomb12)가 한 번에 다수 블록을 파괴하므로 폭탄 스테이지에서 BlockDestroy 달성이 빨라지는 시너지가 있다.

8. **엣지 케이스**
   - 보스 스테이지(BossStagePlay)에서 파괴한 블록이 BlockDestroy 카운터에 포함되는지 검증 필요 — 현재 CLAUDE.md는 "Wall/Locker 등 직접 매치 불가 항목 제외" 명시. 보스 스테이지 HP 기반 블록이 카운터에 합산되면 파밍 루트가 열릴 수 있으므로 제외 여부 명확화 요망.
   - 네트워크 없이 오프라인 플레이 시 blockDestroyCountToday는 로컬 Data.Login에 저장되나 자정 리셋 판정은 NTP 기준이므로, CHMTime.IsAvailable == false면 리셋이 연기된다. 재접속 후 한 번에 보상 수령 가능 여부 확인.
   - rewardCount를 5로 올렸을 때 AddTime 아이템 수급이 IAP 매출에 미치는 영향(상품 shopID 4 대체 효과) — 미션으로 일 5개 획득 vs IAP 대비 전환율 모니터링 필요.

9. **유저 정보·피드백**
   현재 구조에서는 매일 45분을 투자해 블록 100개를 파괴해도 AddTime 1개만 얻는다. 이 사실을 알게 된 플레이어는 "블록 열심히 깨봐야 이득 없다"는 인식을 갖고 일일 미션 자체를 무시하게 된다. 반면 rewardCount를 5로 올리면 "오늘 블록 많이 깼더니 AddTime 5개 생겼네"라는 긍정적 피드백 루프가 형성되어 자발적 플레이 연장이 기대된다.

### 보류

- Potal 블록(EBlockState 17) tapIndex 1·2 미션 완전 공백 — 과거 Wall 블록 tapIndex 1(2026-07-06) 및 PotalCreator tapIndex 2(2026-07-09) 감사와 카테고리 유사(장애물 블록 미션 공백). 다음 회차 후보.
- Shop 스킨 가격(0~60,000 Gold) vs 미션 Gold 보상 수입 격차 — tapIndex 1 미션 반복 보상이 100 gold이므로 최고가 스킨(60,000 gold)까지 600회 반복 필요. 후속 경제 감사 후보.

## 3. 과거 감사 대비 차별성

git log 33건 검토 완료.

- **가장 유사한 과거 커밋**: `18dd561` (2026-07-05) — "tapIndex 2 장기 이정표 미션 보상 Gold 단일화 — AddTime·AddMove 전무"
  - 차별점: `18dd561`은 **tapIndex 2**(생애 이정표) 5개 미션의 **보상 타입**이 모두 Gold뿐이고 AddTime·AddMove가 없다는 issue. 반면 이번 감사는 **tapIndex 3**(일일) 의 기존 AddTime·AddMove 보상은 이미 있지만 **보상 수량(rewardCount)**이 노력 대비 불균형하다는 issue — 구조 부재 vs 분량 비대칭으로 본질적으로 다름.

- **두 번째 유사 후보**: `7528a26` (2026-06-09) — "아이템 사용(AddTime·AddMove) 미션 연계 공백"
  - 차별점: `7528a26`은 "아이템을 **사용**하는 행위"가 미션 목표로 존재하지 않는다는 gap. 이번 감사는 이미 존재하는 BlockDestroy 일일 미션의 rewardCount 수치가 AdWatch 대비 불합리하게 낮다는 calibration 이슈 — 누락 vs 수치 비율 문제.

## 4. 다음 단계 제안

채택 시:
1. `Assets/AssetBundleResources/json/Mission.json` missionID 102: `"rewardCount":1` → `"rewardCount":5`
2. 변경 전후 A/B 측정 지표: 일일 미션 완료율(BlockDestroy), 평균 일일 세션 수, AddTime IAP(shopID 4) 구매 전환율
3. 보스 스테이지 파괴 블록이 카운터에 포함되는지 코드 레벨 확인 (`Assets/Scripts/GamePlay/GPGameScene.cs`)

## 5. 쉬운 설명 (비개발자 요약)

지금 게임에는 "오늘 블록 100개 부숴보세요" 하는 숙제가 있는데, 이걸 다 하면 시간 아이템이 딱 한 개 나온다. 반면에 "오늘 광고 한 번 보세요" 숙제는 30초만 투자하면 이동 아이템이 무려 열 개나 나온다. 2~3판(약 30분 이상)을 꽉 채워 뛰어야 하는 숙제와, 광고 한 편 보면 끝나는 숙제의 보상이 오히려 역전된 셈이다. 마치 1시간 동안 열심히 청소했더니 용돈 500원, 30초 발표 연습했더니 5,000원을 받는 것처럼 앞뒤가 맞지 않는다. 그래서 이번에 제안하는 것은: 블록 100개 파괴 숙제의 보상을 아이템 5개로 올려서, 열심히 플레이한 사람이 적절한 대가를 받도록 바로잡는 것이다.
