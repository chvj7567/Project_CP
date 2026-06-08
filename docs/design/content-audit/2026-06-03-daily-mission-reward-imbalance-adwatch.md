# Content Audit — 2026-06-03 — 일일 미션 보상 불균형: AdWatch 1회 보상이 BlockDestroy 100회 보상의 10배

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Guide, Mission, Shop, Stage, StageBlock, StringEnglish, StringKorea, Tutorial)
- 과거 감사 이력 (git log): 9건 (가장 최근: 2026-06-02)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / Max=84 | 빈 슬롯 7~9·25~39·47~51(23개) 제외 후 실제 정의 타입 수 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7 + 기타 2), tapIndex 2: 3개(RemoveAD / AddTime / AddMove) |
| 고양이 스킨 | 6종 테마 × Cat1~5 = 30 EBlockState | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |

### 분포 공백 — 일일 미션(tapIndex 3) 보상 비교

| missionID | dailyCounter | 목표 | 보상 타입 | 보상 수량 | Shop 골드 환산 |
|---|---|---|---|---|---|
| 100 | 0 Attendance | 출석 1회 | Gold | 100 | 100골드 |
| 101 | 1 NormalStageClear | 노멀 스테이지 3회 클리어 | Gold | 300 | 300골드 |
| 102 | 2 BlockDestroy | 블록 **100개** 파괴 | AddTime | **1** | ~5,000골드 상당 |
| 103 | 3 AdWatch | 광고 **1회** 시청 | AddMove | **10** | ~50,000골드 상당 |
| 104 | -1 (snapshot 기반) | CatPang 3회 생성 | Gold | 200 | 200골드 |

- Shop.json shopID 7(AddTime, gold=5,000), shopID 8(AddMove, gold=5,000) 기준 환산
- **투자 대비 수익 비율**: AdWatch가 BlockDestroy 대비 골드 환산 **10배** 이상 유리

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 — 화살표 폭탄 5회 일일 미션 추가 제안 |
| 2026-06-01 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 제안 |
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### 일일 미션 보상 불균형 — AdWatch 보상이 BlockDestroy 대비 10배 이상

- **카테고리**: 일일 미션 (tapIndex 3) 밸런스
- **요지**: missionID 103(AdWatch 1회)이 AddMove×10을 지급해 missionID 102(BlockDestroy 100개 → AddTime×1) 대비 골드 환산 10배 우위를 점한다. 광고 시청이 능동 플레이보다 훨씬 효율적인 구조가 되어, 플레이어의 게임 내 동기를 광고 쪽으로 과도하게 유도하고 핵심 플레이 루프(매치-3)의 보람을 하락시킨다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 5/1/4/5 → 종합 **19**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 102: `clearValue=100, reward=1(AddTime), rewardCount=1` vs missionID 103: `clearValue=1, reward=2(AddMove), rewardCount=10`
  - `Assets/AssetBundleResources/json/Shop.json` — shopID 7: `gold=5000` (AddTime 1개), shopID 8: `gold=5000` (AddMove 1개) → AddMove×10 ≈ 50,000골드, AddTime×1 ≈ 5,000골드

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**: 플레이어가 UIMission 탭 3(일일)을 열면 5개의 일일 미션이 나열된다. BlockDestroy(블록 100개 파괴 → AddTime×1)와 AdWatch(광고 1회 → AddMove×10)가 나란히 위치하며, 보상 아이콘 옆 수량 텍스트가 시각적으로 바로 비교된다. 두 미션 모두 동일한 '일일' 슬롯이므로 중요도가 동등하게 인식된다.

2. **화면 변화**: AdWatch 미션 완료 시 AddMove 아이템 10개 획득 애니메이션이 재생되고 미션 슬롯이 '완료' 상태로 바뀐다. BlockDestroy 미션 완료 시에는 AddTime 아이템 1개 획득 연출만 표시된다. 같은 탭에서 확인하는 두 보상의 수량 차이(1개 vs 10개)가 플레이어 눈에 즉시 들어온다.

3. **입력 행동**: 효율을 인지한 플레이어는 게임 시작 전 UIMission에서 AdWatch를 먼저 완료하기 위해 보상형 광고 시청 버튼을 선택한다. BlockDestroy 미션은 정상 플레이를 3~5판 진행해야 100개 목표에 도달하므로 자연 완료까지 시간이 걸린다.

4. **시스템 반응**: `DailyMissionService.OnAdWatched()`가 호출되어 `Data.Login.adWatchCountToday`가 1 증가하며 clearValue=1 조건을 즉시 충족한다. BlockDestroy는 `DailyMissionService.OnBlockDestroyed(count)`가 매 매치마다 누적 호출되어 `blockDestroyCountToday`가 100에 달해야 완료된다. 자정(NTP 기준)에 `DailyMissionService.CheckAndResetIfNeeded()`로 두 카운터가 동시에 0으로 리셋된다.

5. **반복·재발생 패턴**: 보상 효율 차이를 학습한 플레이어는 매일 로그인 → AdWatch 1회 → AddMove×10 수령 패턴을 고정화한다. BlockDestroy 100개는 능동적 플레이가 필요하지만 보상 매력이 AdWatch의 1/10에 불과하므로 후순위로 밀리거나 무시된다. 시간이 지날수록 AddMove 보유량이 크게 쌓여 이동 제한 실패가 거의 발생하지 않는 상태가 된다.

6. **종료·해소 조건**: 두 미션 모두 rewardCount 수령 즉시 `EClearState.Clear`로 전환되어 당일 중복 수령이 차단된다. 다음 자정(NTP UTC 기준)에 `EClearState.NotDoing`으로 초기화되며 다시 도전 가능 상태가 된다.

7. **다른 시스템과 상호작용**: AddMove 대량 축적 시 UIShop tapIndex 2의 AddMove IAP(shopID 5, 실결제) 구매 동기가 소멸한다. 또한 AddMove가 과잉 공급되면 이동 제한 모드 스테이지(moveCount > 0 을 가진 하드 스테이지 89개)의 체감 난이도가 크게 낮아져 중·후반 스테이지 도전 의욕이 감소할 수 있다. 반대로 AddTime은 수급이 제한적이어서 시간 제한 모드 스테이지(time > 0인 하드 스테이지 73개)는 난이도 차이가 부각된다.

8. **엣지 케이스**: NTP 미수신(`CHMTime.IsAvailable == false`) 상태에서는 자정 리셋이 발생하지 않으므로 AdWatch 보상을 다음날에도 이미 완료 상태로 인식한다. 네트워크 차단으로 광고가 로드되지 않는 환경에서는 AdWatch 미션을 완료할 수 없어, BlockDestroy가 유일한 아이템 경로가 되는데 현재 보상(AddTime×1)이 극히 적어 불만이 증폭될 수 있다.

9. **유저 정보·피드백**: 보상 수량 비대칭을 인지한 플레이어는 "광고 한 번 보면 게임 다 했다"는 인식을 형성하고 매치-3 코어 루프 자체의 보람이 희석된다. 장기적으로는 앱 스토어 리뷰에 "광고를 보면 아이템이 너무 많이 온다"는 긍정/부정이 혼재하는 피드백이 등장할 수 있으며, AddMove 과잉 지급으로 인한 스테이지 클리어율 왜곡이 밸런스 지표를 오염시킬 수 있다.

### 보류

- **Both-mode 스테이지(시간+이동 동시 제한) 분포 불균형**: 그룹 1~6에만 12개 존재, 그룹 7~15에는 0개. 흥미로운 발견이나 과거 감사 a6cb70b(그룹 13~15 시간제한 공백)와 카테고리 부분 겹침으로 이번 회차 보류.
- **보스 스테이지 완전 무제한 모드**: 100개 전체가 time=-1·moveCount=-1이며 targetScore만으로 진행. 과거 58c3f65(보스 스킬 분포)와 카테고리 중복 없으나 점수(종합 16)에서 우선순위 밀림.

## 3. 과거 감사 대비 차별성

git log 9건 검토 완료. 가장 유사했던 과거 커밋: **1ea1f97** (Arrow 폭탄 tapIndex 3 일일 미션 완전 누락) — 동일하게 tapIndex 3 일일 미션 영역을 다루지만, 1ea1f97는 **존재하지 않는 미션 추가**를 제안한 반면 본 회차는 **기존 미션 간 보상 수량의 불균형 조정**을 제안한다는 점에서 뚜렷이 다르다. 과거 9건 중 보상 수량 밸런스(rewardCount 비율)를 직접 분석한 제안은 없음.

## 4. 다음 단계 제안

- 채택 시 Mission.json missionID 102의 `rewardCount`를 1 → 3으로 상향, 또는 missionID 103의 `rewardCount`를 10 → 3으로 하향하여 비율 재균형 검토
- AddMove 단가(shopID 8: gold=5,000) 대비 AdWatch 일일 획득량이 수익화 전략과 부합하는지 기획·사업 담당자 연계 검토
- BlockDestroy 목표치(100개) 난이도를 감안한 적정 보상 범위 표 작성 (목표치 조정 또는 보상 타입 변경도 선택지)

## 5. 쉬운 설명 (비개발자 요약)

게임 안에서 매일 할 수 있는 미션 중에 "광고 영상 딱 1번 보기"와 "블록 100개 부수기"가 있다. 광고 영상을 1번 보면 이동 아이템을 무려 10개나 받는데, 블록 100개를 힘들게 부수면 시간 아이템 고작 1개만 받는다. 두 아이템의 가게 가격은 똑같이 5,000골드인데 받는 개수가 10배나 차이 나니, 누구라도 광고만 보고 나머지는 건너뛰고 싶어진다. 그래서 이번에 제안하는 것은: 열심히 게임을 한 사람과 광고를 본 사람이 비슷한 보상을 받도록 숫자를 조정하자는 것이다.
