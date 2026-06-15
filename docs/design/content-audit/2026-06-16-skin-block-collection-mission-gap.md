# Content Audit — 2026-06-16 — 고양이 스킨 블록 수집 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 21건 (가장 최근: 2026-06-15)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 25종 / EBlockState.Max=84 | 빈 슬롯(25~39, 47~51) 및 스킨 블록(54~83) 제외 실 게임플레이 블록 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 (skinIndex 0~6 스킨 7종 + RemoveAD + AddTime + AddMove + 골드상품 2종) | tapIndex 1: 스킨·골드, tapIndex 2: 인앱결제 상품 |
| 고양이 스킨 | 6종 × Cat1~5 = 30개 EBlockState | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry (54~83) |
| 블록타입 수 분포 | 5종 209스테이지(83.6%), 4종 23, 3종 17, 2종 1 | 대다수 스테이지가 5색 최대 복잡도 |

### 미션 tapIndex별 collectionType 현황

| tapIndex | 커버 블록 (collectionType = EBlockState int) | 공백 |
|---|---|---|
| 1 (반복 수집) | Cat1~5(0~4), Arrow1~6(10~15), CatPang(18), PinkBomb~BlueBomb(19~23) | Cat6(5), Cat7(6), Wall(16), Potal(17), **스킨 블록 54~83 전체** |
| 2 (장기 이정표) | Fish(24), CatBox1(40), WallCreator(45), RainbowPang(52), Ball(53) | Cat1~7, Arrow, CatBox2~5, PotalCreator(46), **스킨 블록 54~83 전체** |
| 3 (일일) | Attendance·NormalStageClear·BlockDestroy·AdWatch(dailyCounter), CatPang 일일(collectionType:18) | HardStageClear, Arrow 일일, **스킨 블록 전체** |

### 분포 공백
- **EBlockState 54~83 (스킨 블록 30개) 에 해당하는 collectionType 을 사용하는 미션이 tapIndex 1·2·3 어디에도 존재하지 않는다.**
- 상점 tapIndex 1(스킨 구매)은 있으나, 구매 후 해당 스킨 장착·플레이에 연계된 미션 보상 루프가 단절된 상태.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | SHA | 설명 |
|---|---|---|
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 — PinkBomb~BlueBomb 50개 이정표 신설 제안 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절 제안 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 — 게임 동명 블록 장기 목표 신설 제안 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 — IAP 구매자 보상 루프 단절 제안 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 제안 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 — tapIndex 2 스킬 선택 미션 신설 제안 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 — Cat1~5 구현 후 2종만 잔여 공백 |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 — 화살표 폭탄 5회 일일 미션 추가 제안 |
| 2026-06-01 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 제안 |
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 — 스킨 장착 보상 루프 신설 제안

- **카테고리**: 미션 (스킨·상점 연계)
- **요지**: EBlockState에 30개의 스킨 블록(CatCrown1~5, CatFlowers1~5, CatMushroom1~5, CatParty1~5, CatSanta1~5, CatStrawberry1~5)이 정의되어 있고 상점에 6종의 유료 스킨(gold 10000~60000)이 있지만, Mission.json의 collectionType 목록에 54~83 항목이 단 하나도 없어 스킨을 구매하거나 장착해도 미션 보상이 없는 상태다. tapIndex 1 반복 미션으로 "스킨 테마 블록 N개 수집" 미션을 신설하면 스킨 장착 플레이 동기를 부여하고 구매 전환율을 높일 수 있다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 **17**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — collectionType 목록: 0,1,2,3,4,10,11,12,13,14,15,18,19,20,21,22,23,24,40,45,52,53 (총 22개). EBlockState 54~83 범위 항목 0개.
  - `Assets/Scripts/Defines.cs` — EBlockState.CatCrown1(54)~EBlockState.CatStrawberry5(83): 30개 스킨 블록 정의됨.
  - `Assets/AssetBundleResources/json/Shop.json` — skinIndex 1~6 스킨 6종, gold 10000~60000. 구매 후 Mission 연계 없음.
  - `Assets/Scripts/Data.cs` — Data.Collection.value(int)로 개별 EBlockState 카운터를 저장하는 구조가 이미 존재 — 스킨 블록 collectionType 추가 시 기존 집계 로직 재사용 가능.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**: 플레이어가 UIShop에서 첫 번째 유료 스킨(예: CatCrown, 10000골드)을 구매하는 순간, 또는 UIMission tapIndex 1 탭을 스크롤하다 "CatCrown 블록 10개 수집"이라는 새 항목을 처음 발견할 때 미션이 노출된다. 스킨을 하나도 사지 않은 플레이어에게는 잠금 아이콘과 함께 "상점에서 스킨 구매 후 활성화"라는 힌트 문구로 호기심을 유도한다.

2. **화면 변화**: UIMission tapIndex 1 탭의 기존 Cat1~5 미션 목록 아래에 구매한 스킨 테마별 미션 행이 추가된다. 각 행은 해당 스킨의 고양이 아이콘, 누적 수집 진행 바, 현재 목표 수량(예: 10개)을 표시한다. 미구매 스킨의 미션 행은 잠금 상태로 회색 처리되어 구매를 유도하는 상점 이동 버튼이 함께 표시된다.

3. **입력 행동**: 플레이어는 UIShop에서 원하는 스킨을 골드로 구매하고 "장착" 버튼을 누른다. 이후 GameScene에서 스테이지를 선택해 플레이하면, 보드 위의 고양이 블록이 장착한 스킨 테마(예: CatCrown1~5)로 표시된다. 플레이어는 해당 블록을 드래그·매치로 제거한다.

4. **시스템 반응**: 매치로 스킨 블록이 제거될 때마다 CHMData의 Collection 카운터(collectionType = (int)EBlockState.CatCrown1 등 테마별 대표값)가 1씩 증가한다. 누적값이 목표(10개)에 도달하면 UIMission의 해당 미션 행에 "수령" 버튼이 활성화되고, 탭 아이콘에 빨간 알림 뱃지가 표시된다. 보상(골드 100)을 수령하면 addValue:10이 적용되어 다음 목표가 20개로 자동 갱신된다.

5. **반복·재발생 패턴**: tapIndex 1 기존 구조(addValue 반복)를 그대로 따르므로 10개 → 20개 → 30개 순으로 목표가 누적된다. 스킨을 장착한 채 스테이지를 클리어할 때마다 카운터가 쌓이며, 스킨 교체 시 새 테마 카운터가 활성화된다. 이전 테마의 누적 진행도는 보존되어 스킨을 재장착하면 이어서 진행할 수 있다.

6. **종료·해소 조건**: 반복 미션이므로 상한선이 없다. 플레이어가 해당 스킨을 계속 장착하고 플레이하는 한 무한히 골드를 얻을 수 있다. "스킨을 상점에서 구매하지 않은 상태"가 유일한 잠금 조건이며, 구매 즉시 미션이 활성화된다.

7. **다른 시스템과 상호작용**: UIShop의 스킨 구매(Data.Shop.buy)와 직접 연동되어 구매 전환율을 높인다. 기존 Cat1~5 tapIndex 1 미션(missionID 18~22)과 충돌 없이 병렬 운영되며, 스킨 장착 시 Cat1~5 블록이 스킨 블록으로 교체되므로 Cat1~5 미션 카운터는 스킨 장착 중 증가하지 않는 대신 스킨 테마 미션 카운터가 활성화된다. 이로써 두 미션 계열이 상호 보완 구조를 이룬다.

8. **엣지 케이스**: 기본 스킨(skinIndex 0, 무료)은 Cat1~5 기존 미션으로 이미 커버되므로 스킨 테마 미션을 추가하지 않는다. 여러 스킨을 보유한 플레이어가 스킨을 자주 교체하면 어느 미션도 빠르게 진행되지 않을 수 있으나, 이는 플레이어의 선택이므로 별도 제약 불필요. 스킨 블록 5종(Cat1~5에 대응하는 CatCrownN 등)을 하나의 collectionType으로 통합 집계할지, 각 색상별로 분리할지는 구현 시 결정 필요 — 통합 방식(테마당 미션 1개)이 UI 복잡도 면에서 권장된다.

9. **유저 정보·피드백**: 미션 달성 시 UIMission에서 골드 보상 수령 팝업이 표시된다. 플레이어는 "내가 산 스킨으로 특별한 골드를 얻었다"는 이중 보람(외형 + 보상)을 느낀다. 미구매 스킨 미션의 잠금 UI는 "얼마나 더 필요하지?"라는 궁금증보다는 "구매하면 이런 보상이 생긴다"는 명확한 인센티브 안내로 설계한다.

### 보류

- **Ball 블록 tapIndex 1 반복 미션 공백** — Ball(EBlockState 53)은 tapIndex 2(missionID 17, clearValue:131)에 장기 이정표로 이미 있다. 폭탄 제거 또는 맨 아래줄 탈출이라는 특수 메커니즘 때문에 탈출 속도가 느려 tapIndex 1 단위(10개 반복)에 적합한지 검토 필요. 카테고리가 이미 커버된 블록이므로 보류.
- **보스 EBossSkillType.Wall/Creator 배분 분석** — 05-29 감사에서 CatBox 배분을 다뤘으므로 같은 카테고리(보스 스킬 배분)의 중복 우려. 차별점이 있으나 이번 회차 채택 후 다음 회차로 이월.

---

## 3. 과거 감사 대비 차별성

git log 21건 검토 완료. 

가장 유사한 과거 커밋을 두 건 검토:
- **2026-06-04 (1d91056)** — "상점 스킨 골드 가격 선형 계단" — 스킨 가격 구조 개선 제안으로 카테고리(상점)가 일부 겹치지만, 본 회차는 **미션 시스템 공백(collectionType 54~83 미존재)**에 집중한다. 가격 문제가 아닌 "구매 후 보상 루프 단절" 문제로 근거·요지가 다르다.
- **2026-06-05 (158d246)** — "Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락" — tapIndex 1 반복 미션 공백이라는 카테고리가 유사하나, 본 회차의 대상은 Cat6/7이 아닌 **스킨 테마 블록(54~83) 30개 전체**로, 상점 구매 연동이라는 별도 경제 시스템과의 연계가 핵심 차별점이다.

→ 카테고리·요지·근거가 모두 겹치는 과거 감사 없음. 채택 확정.

---

## 4. 다음 단계 제안

- 채택 시 Mission.json에 missionID 105~110 (또는 다음 가용 ID) 형태로 스킨 테마 6종의 tapIndex 1 미션 추가
- collectionType 필드: 각 스킨 테마의 Cat1 EBlockState 값 (CatCrown1=54, CatFlowers1=59, CatMushroom1=64, CatParty1=69, CatSanta1=74, CatStrawberry1=79)을 대표값으로 사용하고, 게임 로직에서 테마 내 5색을 묶어 집계
- StringKorea.json / StringEnglish.json에 미션 설명 문자열 추가 필요 (예: "CatCrown 블록 {0}개 수집")
- UIMission 잠금 UI 기획 및 상점 연동 구현 검토

---

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 귀여운 고양이 캐릭터들이 6가지 코스튬(왕관, 꽃, 버섯, 파티, 산타, 딸기)으로 갈아입을 수 있는 스킨 상점이 있다. 각 스킨은 골드를 모아서 사야 하는데, 지금은 스킨을 사고 나서 그 스킨을 입고 게임을 열심히 해도 "잘했어요!" 하는 추가 보상이 하나도 없다. 스킨을 산 사람과 안 산 사람이 게임 보상 면에서 아무 차이가 없는 것이다. 그래서 이번에 제안하는 것은: 스킨을 입고 플레이해서 그 스킨의 고양이 블록을 N개 모으면 골드 보너스를 받는 미션을 추가하자는 것이다.
