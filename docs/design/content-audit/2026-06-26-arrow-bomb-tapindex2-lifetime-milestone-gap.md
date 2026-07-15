# Content Audit — 2026-06-26 — Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 30건 (가장 최근: 2026-06-25 / 후반 복합 제약 밀스톤)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 30종 / 전체 84 | 빈 슬롯(25~39, 47~51) 제외 실사용: Cat1~7(7), Arrow1~6(6), Wall/Potal(2), CatPang(1), 5색폭탄(5), Fish(1), CatBox1~5(5), WallCreator/PotalCreator(2), RainbowPang(1), Ball(1), 스킨 30종 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | 스킨 7개(tapIndex 1), RemoveAD/AddTime/AddMove(tapIndex 2), 골드 교환 2개(tapIndex 1) |
| 고양이 스킨 | 6종 × 5 = 30 블록 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |

### 분포 공백

**blockTypeCount 고착**
- Stage.json 250개 중 blockTypeCount=5: 209개 (83.6%)
- blockTypeCount=3: 17개 / 4: 23개 / 2: 1개 → 초반만 낮은 색 수를 사용하고 스테이지 5 이후 사실상 5색 고정

**시간·이동 제한 분포**
- time>0: 73스테이지 / moveCount>0: 89스테이지 / 둘 다 양수: 12스테이지
- 이동 제한과 시간 제한이 겹치는 스테이지가 전체의 4.8%에 불과

**미션 tapIndex 분포**
| tapIndex | 설명 | 미션 수 | 대상 블록 |
|---|---|---|---|
| 1 | 반복 수집 (addValue>0) | 17 | Cat1~5(5), Arrow1~6(6), CatPang(1), 5색폭탄(5) |
| 2 | 일회성 이정표 (addValue=-1) | 5 | Fish(24), CatBox1(40), WallCreator(45), RainbowPang(52), Ball(53) |
| 3 | 일일 미션 | 5 | Attendance, NormalStageClear, BlockDestroy, AdWatch, CatPang(수집) |

**핵심 공백: Arrow1~6(10~15)은 tapIndex 1만 있고 tapIndex 2가 전혀 없다.**
- Mission.json: missionID 1~6 → tapIndex=1, clearValue=10, addValue=10 (10개마다 100골드 반복)
- Fish·CatBox1·WallCreator·RainbowPang·Ball은 각각 tapIndex 2 이정표 미션이 존재
- Arrow 폭탄 6종(Arrow1~6)만 장기 이정표 목표 완전 부재

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-25 | fcebcd0 | 후반 복합 제약(시간+이동) 밀스톤 완전 부재 |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 공백 |
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 공백 |
| 2026-06-22 | 962c93d | 후반 moveCount 1~100 100배 격차 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 부재 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 공백 |
| 2026-06-19 | feddcde | AddTime·AddMove 보스 스테이지 무효 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 공백 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-16 | 3670d4f | 스킨 블록 수집 미션 공백 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 이정표 공백 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 부재 |
| 2026-06-12 | ccb2a5d | boomAllCount 잠든 필드 — BoomAll 클리어 보너스 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | AddTime·AddMove 아이템 사용 미션 공백 |
| 2026-06-09 | 85ac09c | 보스 HP 회복 루프 미설계 |
| 2026-06-08 | 329cce2 | blockTypeCount=5 고착 제안 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150 완화 |
| 2026-06-06 | 7d601d5 | ESelect 스킬 미션 연계 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 공백 |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 선형 계단 — 일일 보상 대비 소요 시간 |
| 2026-06-03 | 41f1f81 | AdWatch 1회 보상이 BlockDestroy 100개의 10배 불균형 |
| 2026-06-02 | 58c3f65 | **Arrow 화살표 폭탄 tapIndex 3 일일 미션 공백** |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 미션 공백 |
| 2026-05-29 | 58c3f65 | 보스 CatBox 스킬 극소 배분 |
| 2026-05-30 | (이전) | 하드 스테이지 일일 미션 공백 |
| 2026-05-31 | (이전) | Wall·Potal tapIndex 2 미션 공백 |
| 2026-06-01 | (이전) | RainbowPang tapIndex 1 반복 미션 공백 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백

- **카테고리**: 미션
- **요지**: 화살표 폭탄(Arrow1~6) 6종은 tapIndex 1 반복 수집 미션(10개마다 100골드)만 존재하고 tapIndex 2 장기 이정표 미션이 전혀 없다. Fish/CatBox1/WallCreator/RainbowPang/Ball 등 비교군 블록들이 모두 tapIndex 2를 가진 데 비해 화살표 폭탄만 누락되어 있으며, 이는 게임의 핵심 특수블록(6종 대응 Bomb4/5/2/6/7/8)에 대한 장기 플레이 동기를 끊는 공백이다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 **17**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json`: missionID 1~6 (collectionType 10~15, tapIndex 1, clearValue=10, addValue=10) — Arrow1~6 tapIndex 2 항목 없음 확인
  - missionID 13~17 (tapIndex 2 이정표): collectionType=24(Fish), 40(CatBox1), 45(WallCreator), 52(RainbowPang), 53(Ball) — Arrow 폭탄 계열 없음
  - `Assets/Scripts/Defines.cs`: EBlockState.Arrow1=10 ~ Arrow6=15 (6종), EReward.Gold=0 / AddTime=1 / AddMove=2

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   UIMission의 tapIndex 2 탭(이정표)에 새 목표가 추가된다. 유저가 UIMission을 처음 열거나 스테이지 클리어 후 미션 탭을 확인하는 시점에 "화살표 폭탄 50개 수집" 형태의 이정표 카드가 표시된다. 기존 tapIndex 1 반복 미션(10개 달성마다 소량 골드)과 나란히 보이므로 유저는 자연스럽게 장기 목표를 인지하게 된다.

2. **화면 변화**
   UIMission tapIndex 2 탭의 이정표 리스트 하단에 Arrow 폭탄 이정표 카드 6개(Arrow1~6 각각)가 추가된다. 각 카드는 현재 수집 수 / clearValue를 프로그레스 바 형태로 보여주고, 미달성 상태에서는 잠금 아이콘과 함께 목표 수치가 표시된다. 달성 즉시 카드가 활성화되며 "보상 받기" 버튼이 나타난다.

3. **입력 행동**
   유저는 스테이지 내에서 4개 이상의 같은 색 고양이를 가로/세로로 정렬하거나 가로+세로 동시 매치를 만들어 Arrow 폭탄을 생성한다(GPBombResolver.CreateBombBlock 기준: hScore>3 → Arrow1/Arrow4, vScore>3 → Arrow3/Arrow2, 교차 → Arrow5/Arrow6). 유저 입력은 기존 드래그 스왑과 동일하며 별도 입력 행동이 불필요하다.

4. **시스템 반응**
   GPBombResolver가 화살표 폭탄을 생성하는 시점에 Data.Collection의 해당 collectionType(10~15) 카운터를 1 증가시킨다. 이 처리는 기존 tapIndex 1 반복 수집 카운팅 로직과 동일 경로를 공유하므로 추가 구현 비용이 낮다. clearValue(예: 50개) 도달 시 UIMission에 알림 뱃지가 표시된다.

5. **반복·재발생 패턴**
   tapIndex 2 이정표는 addValue=-1(1회성)이므로 달성 후 재반복되지 않는다. 단, Arrow 6종(Arrow1~6)이 개별 이정표 항목으로 등록되면 유저는 6개 이정표를 순차 달성하는 중기 목표를 가지게 된다. 스테이지 진행에 따라 등장하는 Arrow 폭탄 종류가 달라지므로 자연스러운 진행 쐐기가 생긴다.

6. **종료·해소 조건**
   각 Arrow 종류별 clearValue(예: Arrow1 50개, Arrow2~6 동일 또는 차등) 달성 시 이정표 미션이 완료된다. 보상은 EReward.Gold(금액 TBD) 또는 tapIndex 2 패턴(missionID 13~17 참고: rewardCount=1000골드)으로 설정한다. 6종 모두 달성하면 UIMission tapIndex 2 탭의 Arrow 이정표 섹션이 완료 상태로 표시된다.

7. **다른 시스템과 상호작용**
   Data.Collection(key=collectionType int)에 카운터를 저장하므로 클라우드 저장(GPGS)과 자동 동기화된다. UIMission의 ScrollView 풀링 구조(CHPoolingScrollView)가 새 항목을 자동 수용한다. 기존 tapIndex 1 Arrow 미션(missionID 1~6)과 같은 collectionType을 공유하므로 별도 카운팅 필드 없이 동일 컬렉션 값을 참조한다.

8. **엣지 케이스**
   Arrow5(가로+세로 교차 4매치 이상)와 Arrow6(같은 교차 조건 분기)는 `_arrowPangIndex` 플래그로 교대 생성된다. 한 매치에서 Arrow5·Arrow6 중 하나만 생성되므로 이중 카운팅 우려는 없다. 폭탄 조합(Arrow+Arrow → 색폭탄 승급) 시 원본 Arrow는 소멸하므로, 수집 카운팅을 소멸 시점이 아닌 생성 시점에 처리해야 승급 후 카운터 누락이 발생하지 않는다.

9. **유저 정보·피드백**
   UIMission tapIndex 2 탭에서 각 Arrow 이정표의 실시간 진행도를 확인할 수 있다. 달성 직후 스테이지 결과 화면(UIGameEnd 이후)에 "미션 달성" 팝업 또는 뱃지가 표시된다. 유저는 "Arrow4를 50개 모으려면 세로 5매치를 더 많이 만들어야 한다"는 구체적인 전략 목표를 인지하게 되어 스테이지 내 의도적 패턴 형성을 유도한다.

### 보류

- **Fish 블록 tapIndex 3 일일 미션 공백** — Fish는 tapIndex 2 이정표(missionID 13)가 있으나 tapIndex 3 일일 카운터가 없음. Ball 탈출 일일 미션(2026-06-18)과 유사하여 카테고리·근거 일부 겹침. 점수: 검증가치 3 / 구현비용 2 / 플레이어경험 3 / 데이터근거 4 → 종합 12.

## 3. 과거 감사 대비 차별성

git log 30건 검토 완료.

가장 유사했던 과거 커밋:
- **2026-06-02 (58c3f65)** — "Arrow 화살표 폭탄 tapIndex 3 일일 미션 공백" 제안. **차별점**: 그 감사는 tapIndex 3(일일 미션, EDailyCounter 기반)이었으나 이번은 tapIndex 2(생애 1회 이정표, addValue=-1)다. 매일 리셋되는 단기 목표와 누적 카운트 기반 장기 이정표는 보상 구조·달성 주기·유저 동기가 완전히 다르다.
- **2026-06-15 (b0ee2e0)** — "5색 특수폭탄(PinkBomb~BlueBomb) tapIndex 2 이정표 공백" 제안. **차별점**: 대상 블록이 EBlockState 19~23(색 폭탄 5종)인 반면 이번은 EBlockState 10~15(방향성 화살표 폭탄 6종)다. 생성 조건(매치 수 4↑ vs 매치 패턴)·폭탄 발동 범위(한 줄/십자/대각)·미션 수(5개 vs 6개)가 모두 다르다.

## 4. 다음 단계 제안

- **채택 시**: Mission.json에 missionID 200~205 (또는 미사용 ID)로 Arrow1~6 tapIndex 2 항목 6줄 추가. clearValue 값은 50(Arrow1·3), 50(Arrow2·4), 30(Arrow5·6 — 생성 조건이 더 까다로운 교차형) 차등 권장. StringKorea/English.json에 descStringID 추가. 별도 게임 코드 수정 불필요.
- **구현 비용 예상**: JSON 편집 1~2시간. 테스트 2~3판으로 카운터 정상 증가 확인.

## 5. 쉬운 설명 (비개발자 요약)

CatPang 게임에는 고양이 블록을 4개 이상 한 줄로 맞추면 나타나는 "화살표 폭탄" 블록이 있다. 화살표 폭탄에는 가로로 한 줄 날리는 것, 세로로 날리는 것, 십자로 터지는 것 등 6가지 종류가 있어서 게임의 핵심 재미를 담당한다. 지금은 화살표 폭탄을 10개 모을 때마다 소량의 골드를 주는 단기 목표만 있고, "평생 50개 모으면 큰 보상" 같은 장기 목표가 하나도 없다. 비슷한 블록인 물고기·고양이 상자·무지개 폭탄은 모두 장기 목표가 있는데 화살표 폭탄만 빠져 있어서, 열심히 폭탄을 만들어도 "뭔가 쌓이는 느낌"이 없다. 그래서 이번에 제안하는 것은: 화살표 폭탄 6가지 각각에 대해 "평생 50개 모으기" 같은 장기 이정표 목표를 미션 탭에 추가하자는 것이다.
