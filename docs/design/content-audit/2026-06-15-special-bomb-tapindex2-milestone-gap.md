# Content Audit — 2026-06-15 — 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 20건 (가장 최근: 2026-06-14 KST, SHA ee1fc5b)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / EBlockState Max 84 | 빈 슬롯 25~39(15개)·47~51(5개) 제외, 스킨 테마 30종 포함 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| tapIndex 3 미션 | 5개 | missionID 100~104 |
| 상점 아이템 | 12개 | 스킨 7종, 소모성 IAP 2종, 골드 구매 2종, 광고 제거 1종 |
| 고양이 스킨 | 6종 테마 × Cat1~5 = 30 EBlockState | CatCrown/Flowers/Mushroom/Party/Santa/Strawberry |

### tapIndex 별 미션 구조 (Mission.json 기준)

| tapIndex | 개수 | 특징 | 포함 블록 타입 |
|---|---|---|---|
| 1 (반복 수집) | 17개 | addValue > 0, 목표마다 반복 지급 | Cat1~5, Arrow1~6, CatPang(18), **PinkBomb(19)~BlueBomb(23)** |
| 2 (장기 이정표) | 5개 | addValue = -1, 누적 1회 달성 | Fish(24), CatBox1(40), WallCreator(45), RainbowPang(52), Ball(53) |
| 3 (일일) | 5개 | dailyCounter 기반 | Attendance, NormalStageClear, BlockDestroy, AdWatch, CatPang 일일 3개 |

### 특수폭탄 5종 미션 현황

| 블록 | EBlockState | collectionType | tapIndex 1 | tapIndex 2 |
|---|---|---|---|---|
| PinkBomb | 19 | 19 | ✓ clearValue=10, reward=AddTime×1 | **없음** |
| YellowBomb | 20 | 20 | ✓ clearValue=10, reward=AddMove×1 | **없음** |
| OrangeBomb | 21 | 21 | ✓ clearValue=10, reward=AddTime×1 | **없음** |
| GreenBomb | 22 | 22 | ✓ clearValue=10, reward=AddMove×1 | **없음** |
| BlueBomb | 23 | 23 | ✓ clearValue=10, reward=AddTime×1 | **없음** |

비교: RainbowPang(52)은 tapIndex 1 없이 tapIndex 2(91개 이정표)가 있음.
특수폭탄 5종은 tapIndex 1만 있고 tapIndex 2가 전혀 없어 단기 반복 수집 이후 장기 목표가 단절됨.

### 분포 공백
- 특수폭탄을 10개씩 여러 번 수집해도 tapIndex 1 보상만 반복 지급되고, 총 수집량에 대한 이정표 달성감·고보상이 없음.
- 동일하게 생성 계열인 CatPang 블록(18)은 06-11 감사에서 tapIndex 2 이정표 부재가 지적되었고, 특수폭탄 5종은 이와 별개로 tapIndex 2 자체가 없음.
- tapIndex 2에 있는 5개 블록(Fish/CatBox1/WallCreator/RainbowPang/Ball)은 모두 장애물 계열이며, 생성계 특수폭탄 계열은 tapIndex 2 이정표가 존재하지 않음.

### 과거 감사 후보 (git log 조회 결과)

| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 균일 포맷 다양화 제안 |
| 2026-05-29 | b5bcc97 | Cat1~5 tapIndex 1 수집 미션 공백 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 공백 |
| 2026-05-29 | 58c3f65 | 보스 CatBox 스킬 극소 배분 10%→30~40% 확대 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 |
| 2026-05-31 | 4a09a70 | Wall·Potal tapIndex 2 장기 파괴 미션 누락 |
| 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 누락 |
| 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 (AdWatch vs BlockDestroy) |
| 2026-06-03 | 1d91056 | 상점 스킨 가격 선형 계단 vs 일일 미션 획득량 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 누락 |
| 2026-06-05 | 7d601d5 | ESelect 6종 스킬 미션·보상 연계 완전 공백 |
| 2026-06-06 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 진입 장벽 완화 |
| 2026-06-07 | 329cce2 | 중반-후반 110스테이지 blockTypeCount=5 고착 |
| 2026-06-08 | 85ac09c | 보스 스테이지 HP 소진·회복 루프 미설계 |
| 2026-06-09 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-10 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 공백 |
| 2026-06-11 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 클리어 보너스 신설 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### 5색 특수폭탄 tapIndex 2 장기 이정표 미션 신설

- **카테고리**: 미션 (tapIndex 2 장기 이정표)
- **요지**: PinkBomb~BlueBomb 5종이 tapIndex 1 단기 반복 수집(10개마다 아이템 보상)만 있고, 총 누적 수집에 대한 장기 이정표가 없어 특수폭탄 수집 동기가 초반 이후 급감함. 이정표 추가로 중장기 수집 목표를 부여한다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 **17**
- **근거**: Mission.json — missionID 8~12에 collectionType 19~23의 tapIndex 1이 있으나, tapIndex 2 항목이 전혀 없음. 동일 수집 계열 RainbowPang(52)은 missionID 16에 tapIndex 2(91개 이정표)가 있어 직접 비교 가능.

#### 제안 이정표 설계 (안)

| 블록 | missionID (신규) | clearValue | reward | rewardCount |
|---|---|---|---|---|
| PinkBomb(19) | 105 | 50 | 골드(0) | 500 |
| YellowBomb(20) | 106 | 50 | 골드(0) | 500 |
| OrangeBomb(21) | 107 | 50 | 골드(0) | 500 |
| GreenBomb(22) | 108 | 50 | 골드(0) | 500 |
| BlueBomb(23) | 109 | 50 | 골드(0) | 500 |

(clearValue 50은 tapIndex 1×5회 달성에 해당하며, RainbowPang 이정표 91의 절반 수준으로 첫 단계 설정)

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   유저가 UIMission을 열어 tapIndex 2(이정표 탭)를 선택했을 때, 기존 Fish/CatBox1/WallCreator/RainbowPang/Ball 아래에 PinkBomb~BlueBomb 5개 새 이정표 항목이 표시된다. 또는 tapIndex 1 미션(10개)을 3회 이상 반복 달성한 유저가 UI를 열었을 때 "이제 큰 목표가 생겼다"는 시각으로 발견된다.

2. **화면 변화**
   tapIndex 2 미션 목록에 각 특수폭탄 아이콘과 "PinkBomb 50개 수집" 형태의 설명 텍스트, 진행 바(현재 누적치 / 50)가 추가된다. 기존 tapIndex 1 UI는 변경 없이 그대로 유지되며, 유저는 두 탭에서 각각 단기·장기 진행도를 확인할 수 있다.

3. **입력 행동**
   유저는 스테이지 플레이 중 3~4개 매치 이상(또는 2×2 정사각형 이외 교차·가로·세로 초과)으로 특수폭탄을 생성한다. 별도 UI 조작 없이 스테이지를 정상 플레이하기만 하면 Data.Collection 카운터(collectionType 19~23)가 자동 증가한다.

4. **시스템 반응**
   GPBombResolver.CreateBombBlock 에서 특수폭탄(EBlockState 19~23)이 보드에 배치될 때 collectionType 해당 값의 Data.Collection.value가 1씩 증가한다. 이미 tapIndex 1이 같은 카운터를 사용하고 있으므로, 동일 Collection 값이 tapIndex 2 clearValue(50)에 도달하면 미션 clearState → Clear 전환, 골드 500 지급, 팡 효과 재생.

5. **반복·재발생 패턴**
   tapIndex 2는 addValue=-1로 1회성 이정표다. 50개 이정표 1개만 설계할 경우 클리어 후 종료. 단계형(50→100→200)으로 설계 시 addValue 대신 다음 missionID 자동 unlock 방식이 필요하므로 첫 버전은 단일 이정표(50개) 1회 달성으로 단순 구현한다.

6. **종료·해소 조건**
   5종 각각 독립으로 clearValue=50에 도달하면 해당 미션 완료(clearState → Clear). 총 5개 모두 달성하면 tapIndex 2 특수폭탄 섹션 전체가 완료 상태로 표시된다. 보상은 골드로, EReward.Gold 지급 흐름을 따른다.

7. **다른 시스템과 상호작용**
   - **tapIndex 1과 카운터 공유**: 같은 collectionType을 사용하므로 별도 저장 필드 없이 tapIndex 2 달성 판정이 자동으로 이뤄진다.
   - **GPBombResolver 연계**: 특수폭탄 생성 로직에 Collection 증가 hook이 이미 있다면 추가 코드 불필요.
   - **상점 스킨**: 스킨 착용 여부와 관계없이 특수폭탄은 EBlockState 19~23로 생성되므로 카운터 집계에 영향 없음.
   - **보스 스테이지**: 보스 스킬(Wall/Creator/CatBox)로 생성되는 블록이 간접적으로 특수폭탄 생성을 유도할 수 있어 보스 플레이 유도 효과도 기대된다.

8. **엣지 케이스**
   - **복수 동시 생성**: 한 번의 매치에서 여러 특수폭탄이 동시 생성되는 경우(교차 매치 등), 각각 별도 collectionType 카운터를 올려야 한다. 동일 폭탄 복수 생성 시 카운트 중복 방지 기준 명확화 필요.
   - **PinkBomb + 일반블록 조합(Boom3)**: PinkBomb이 조합에 사용될 때 소멸하는데, 이 시점에 생성 집계인지 소멸 집계인지 혼동 방지 필요. 생성 시점 집계로 통일 권장.
   - **골드 인플레이션**: 5종 모두 500골드씩 지급하면 최대 2500골드 추가. 일일 미션 최대 약 600골드/일 대비 일회성 2500은 큰 편이므로 최초 이정표 보상을 200~300골드로 낮출 수도 있다.
   - **신규 유저 탭 노출**: tapIndex 2 탭에 항목이 많아지면 스크롤 UX 확인 필요. CHPoolingScrollView 기반이면 자동 처리되나, 항목 순서(기존 5개 + 신규 5개)를 missionID 순 또는 그룹화 순으로 정렬 방침 결정 필요.

9. **유저 정보·피드백**
   이정표 달성 시 팡 효과(EPangEffect.Explosion)와 골드 획득 팝업이 표시된다. UIMission tapIndex 2 탭의 진행 바가 실시간 갱신되어 "PinkBomb 32/50"처럼 달성 근접 여부를 피드백한다. 미션 설명(descStringID)에 "평생 PinkBomb N개 생성 달성" 형태의 문구가 표시되어 장기 목표임을 명확히 안내한다.

### 보류

- **일일 미션 전체 동시 완료 보너스** (tapIndex 3 5개 동시 달성 시 추가 보상): 점수 15, 일일 미션 카테고리 내에서 보상 불균형(06-02)·아이템 사용(06-09)·출석 스트릭(06-14) 등 연속 감사 피로도 고려해 보류.
- **Ball→Potal 변환 성공 횟수 전용 미션**: 신규 EDailyCounter 추가 및 GPGameScene 변환 이벤트 hook이 필요해 구현비용이 높음.

## 3. 과거 감사 대비 차별성

- git log 20건 검토 완료.
- 가장 유사했던 과거 커밋 3건:
  - **aeffad1** (05-29): PotalCreator·CatBox4 tapIndex 2 공백 → **장애물 생성 블록** 기반, 현재 제안은 **폭탄 조합 생성 블록** 기반
  - **4a09a70** (05-31): Wall·Potal tapIndex 2 장기 파괴 미션 누락 → **장애물 파괴** 카운터, 현재 제안은 **특수폭탄 생성** 카운터
  - **80f90cc** (06-10): CatPang 블록 tapIndex 2 이정표 → **CatPang(18) 단일 블록**, 현재 제안은 **5색 특수폭탄(19~23) 5종 세트** 이정표
- 차별점: 기존 tapIndex 2 감사들은 tapIndex 1이 없는 블록의 이정표 신설이었으나, 이번은 tapIndex 1이 이미 구현된 블록에 대해 tapIndex 2 이정표가 비대칭으로 누락된 구조 문제를 지적함. "단기 반복은 있는데 장기 이정표가 없다"는 점이 신규 관점.

## 4. 다음 단계 제안

- 채택 시 Mission.json에 missionID 105~109 추가 (descStringID는 다음 미사용 ID 배정 필요)
- StringKorea.json / StringEnglish.json에 설명 문자열 추가
- GPBombResolver 또는 GPMatchChecker에서 특수폭탄 생성 시 Collection 카운터 증가 hook 유무 확인 (tapIndex 1과 동일 경로 사용 시 추가 코드 불필요)
- UIMission 스크롤 정렬 정책 결정 (missionID 순 권장)

## 5. 쉬운 설명 (비개발자 요약)

CatPang 게임에는 퍼즐을 풀 때 폭탄처럼 터지는 특별한 블록 5가지(분홍·노랑·주황·초록·파란 폭탄)가 있다. 지금은 이 폭탄을 10개씩 모을 때마다 소소한 보상을 받을 수 있지만, 100개나 200개 같은 "큰 목표"는 존재하지 않아서 어느 정도 하고 나면 모을 이유가 사라진다. 마치 달리기 대회에서 10미터마다 스티커는 주는데 완주 메달이 없는 것처럼, 장기 목표가 빠져 있다. 그래서 이번에 제안하는 것은: 5가지 폭탄 블록 각각에 "50개 모으면 골드 보상" 같은 큰 이정표를 추가해 오래 즐기는 유저에게도 의미 있는 달성감을 주자는 것이다.
