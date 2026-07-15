# Content Audit — 2026-07-13 — CatBox1~5 tapIndex 1 반복 수집 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StringEnglish, StageBlock, StringKorea, Tutorial)
- 과거 감사 이력 (git log + docs 폴더): 47건 (가장 최근: 2026-07-12)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 20개(25~39, 47~51) 및 미정의 슬롯 제외 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 (shopID 0~11) | tapIndex 1(스킨+기타) 9개, tapIndex 2(IAP) 3개 |
| 고양이 스킨 | 6종 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry (각 Cat1~5) |

### 미션 tapIndex 분포 현황

**tapIndex 1 (반복 수집 미션)** — Mission.json에 등록된 collectionType 목록:
- Cat1~5 (0~4), Arrow1~6 (10~15), CatPang (18), PinkBomb~BlueBomb (19~23)
- **완전 누락**: Cat6(5), Cat7(6), Wall(16), Potal(17), Fish(24), **CatBox1~5(40~44)**, WallCreator(45), PotalCreator(46), RainbowPang(52), Ball(53), 스킨 블록(54~83)

**tapIndex 2 (장기 이정표 미션)** — Mission.json 등록:
- Fish(24, clearValue=31), **CatBox1(40, clearValue=51)**, WallCreator(45, clearValue=71), RainbowPang(52, clearValue=91), Ball(53, clearValue=131)
- **완전 누락**: CatBox2~5 (41~44) 등

**tapIndex 3 (일일 미션)** — 5건 (Attendance, NormalStageClear, BlockDestroy, AdWatch, CatPang 일일 발동)

### 분포 공백

CatBox1(40)은 tapIndex 2(이정표, clearValue=51)만 존재하고 tapIndex 1(반복 수집)이 없다.
CatBox2~5(41~44)는 tapIndex 1·2 양쪽 모두 미션이 없다.
보스 스테이지 100개에 걸쳐 반복 등장하는 핵심 장애물 5종 전부가 반복 수집 루프와 단절된 상태.

### 과거 감사 후보 (git log 조회 결과 — 최근 30건, 전체 47건)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-07-12 | c1bc4d1 | 보스 스탯 업그레이드 아이템(체력+10·공격력+1) tapIndex 1 혼재 및 1회 구매 한도 |
| 2026-07-11 | 5a56780 | Cat6·Cat7 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-07-10 | d043062 | WallCreator tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-09 | 9ae50e1 | PotalCreator tapIndex 2 생애 이정표 미션 완전 공백 |
| 2026-07-08 | d371085 | 보스 스테이지 Stage.json 스킬 조율 필드 전무 |
| 2026-07-07 | 1324855 | 보스 스테이지 attack 잠든 공격력 스탯 |
| 2026-07-06 | 80d875f | Wall·Potal tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-05 | 18dd561 | tapIndex 2 장기 이정표 미션 보상 Gold 단일화 |
| 2026-07-04 | 39d8ced | EBackground 4종 완전 미참조 — 배경 커스터마이징 보상 루프 공백 |
| 2026-07-03 | 5c2ba4c | Wall·Potal 초등장 스테이지 Tutorial.json 항목 완전 공백 |
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-27 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 87% 시점 후기 도입 |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약 밀스톤 스테이지 완전 부재 |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(54~83) 수집 미션 완전 공백 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 클리어 보너스 없음 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |

*(git log 미노출 17건: 2026-05-28~06-10, docs 폴더에서 확인)*

## 2. 추가 컨텐츠 후보 (권장 1개)

### CatBox1~5 tapIndex 1 반복 수집 미션 완전 공백

- **카테고리**: 미션
- **요지**: 보스 스테이지 핵심 장애물인 CatBox1~5(EBlockState 40~44) 5종 모두 tapIndex 1(반복 수집) 미션이 전무하다. CatBox1은 tapIndex 2 이정표(clearValue=51)만 존재하고, CatBox2~5는 tapIndex 1·2 양쪽 어디에도 없다. 100개 보스 스테이지에서 반복적으로 파괴하는 장애물인데도 점진적 보상 루프가 단절된 상태다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 **17**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — collectionType 40~44 항목이 tapIndex=1에 없음 (CatBox1은 tapIndex=2 missionID=14만 존재)
  - `Assets/Scripts/Defines.cs` — EBlockState.CatBox1(40)~CatBox5(44), EBossSkillType.CatBox
  - `Assets/Scripts/Data.cs` — Data.Collection key=(int)EBlockState, value=누적 파괴 횟수 → 수집 인프라 이미 존재
  - 보스 스테이지 100개에서 EBossSkillType.CatBox 스킬로 CatBox가 생성·반복 등장함

#### 유저 플로우

1. **노출 시점·트리거**
   플레이어가 보스 스테이지 진입 후 CatBox 위 칸에 해당 색 고양이 블록을 드래그해 `CatInTheBox`를 발동, HP를 0으로 만들면 CatBox가 파괴된다. 파괴 직후 `Data.Collection`의 해당 key(CatBox 타입별 int) 값이 1 증가하면서 UIMission tapIndex 1 진행 바가 갱신된다.

2. **화면 변화**
   UIMission 팝업의 첫 번째 탭(tapIndex 1) 목록에 "CatBox1 N개 파괴", "CatBox2 N개 파괴" 등 최대 5개 항목이 신설된다. 각 항목은 프로그레스 바와 현재값/목표값(예: 3/10)을 표시하며, 달성 시 보상 수령 버튼이 활성화된다.

3. **입력 행동**
   플레이어는 보스 스테이지 보드에서 CatBox 위 칸을 확인하고, 해당 CatBox가 받는 색 고양이 블록(Box1=Cat1 또는 Cat6, Box2=Cat2 또는 Cat7, Box3=Cat3, Box4=Cat4, Box5=Cat5)을 드래그로 인접 칸에 배치해 빨려들어가는 연출을 유발한다.

4. **시스템 반응**
   CatBox HP가 0이 되면 `GPBossController` 또는 관련 게임플레이 코드가 해당 `EBlockState`를 인식하고, 기존 `Data.Collection` 인프라를 통해 누적 카운터를 증가시킨다. `clearValue`(예: 10)에 도달하면 보상(Gold 또는 AddTime/AddMove 1개)이 지급되고, `addValue`(예: 10)만큼 다음 목표치가 갱신된다.

5. **반복·재발생 패턴**
   tapIndex 1 반복 수집 구조이므로 10개 파괴 → 보상 → 다음 10개 목표의 사이클이 계속된다. 보스 스테이지를 반복 플레이할수록 카운터가 쌓여 주기적으로 보상이 발생한다. CatBox 종류(1~5)별 카운터가 독립적으로 관리되므로 다양한 보스 스테이지 클리어 경험이 각기 다른 보상 루프에 연결된다.

6. **종료·해소 조건**
   `addValue > 0`으로 설정 시 무한 반복 이정표. `addValue = -1`로 설정 시 한 번만 달성하면 완료. 기존 tapIndex 1 미션(Arrow1~6, Cat1~5 등)이 모두 반복(addValue=10) 방식이므로 동일하게 설정하는 것이 자연스럽다.

7. **다른 시스템과 상호작용**
   `Data.Collection`(CHMData 관리) 저장값을 참조하므로 기존 수집 인프라를 재사용한다. GPGS 연결 시 클라우드 동기화 대상에 포함된다. UIMission의 tapIndex 1 리스트가 최대 5개 추가되므로 스크롤뷰 레이아웃 길이가 늘어난다(`CHPoolingScrollView`가 동적 풀링이므로 추가 비용 없음). 보스 스테이지 진행도(`Data.Login.bossStage`)와는 별개 트래킹이므로 의존성 없음.

8. **엣지 케이스**
   일반/하드 스테이지에서는 CatBox가 배치되지 않아 카운터 증가가 없다. `EBossSkillType.CatBox` 스킬로 WallCreator/PotalCreator가 CatBox를 보드에 생성한 경우, 이 CatBox도 파괴 시 정상 추적 대상이다. 스테이지 실패로 CatBox가 화면에서 사라지는 경우(HP 소진 없이 종료)에는 카운터 미증가. CatBox가 게임 중 CatInTheBox 아닌 폭탄 범위에 걸리는 경우 현재는 장애물 블록으로 `DamageBlock`만 받으므로 HP 소진 시점에서 카운터 증가를 처리해야 한다.

9. **유저 정보·피드백**
   UIMission tapIndex 1 진행 바에서 "CatBox1을 3/10개 파괴했습니다" 형태로 진행 상황을 확인할 수 있다. 보스 스테이지 클리어 직후 미션 완료 알림(UIAlarm 등)을 통해 보상 획득 사실을 인지할 수 있다. CatBox 5종이 각각 독립 미션으로 표시되므로, 어떤 CatBox가 등장하는 보스 스테이지를 더 자주 플레이해야 하는지 전략적 판단 정보가 생긴다.

### 보류

- **PotalCreator tapIndex 1 반복 수집 미션 공백** — WallCreator tapIndex 1(2026-07-10)과 카테고리·요지가 유사. 차별점 있으나 이번 회차는 CatBox가 더 임팩트 크다고 판단해 보류.
- **Ball tapIndex 1 반복 수집 미션 공백** — stage 131 이후 19개 스테이지 한정 등장(플레이어 도달 비율 낮음)으로 플레이어경험 개선폭이 제한적. ball-block-late-intro(2026-06-27)와 소재 부분 겹침.
- **일일 미션 tapIndex 3 출석 보상 Gold 100 — NormalStageClear 300 대비 역전** — daily-mission-reward-imbalance-adwatch(2026-06-03)와 카테고리 겹침. 허용 범위이나 이번 회차 우선순위 낮음.

## 3. 과거 감사 대비 차별성

- git log 30건 + docs 폴더 17건 = **47건** 전부 검토 완료.
- CatBox 관련 과거 감사 3건:
  - SHA 6d92d88 (2026-06-23): **CatBox 완성** tapIndex **3** 일일 미션 공백 — "오늘 CatBox를 완성한 횟수" 카운터 신설 제안
  - `2026-05-29-boss-catbox-skill-underrepresentation.md`: 보스 **스킬(EBossSkillType) 분포** 편중 — CatBox 스킬이 다른 보스 스킬보다 적게 등장
  - `2026-05-29-potalcreator-catbox-tapindex2-mission-gap.md`: CatBox tapIndex **2** 이정표 공백 — CatBox2~5에 tapIndex 2 이정표 없음
- 차별점: 본 회차는 **tapIndex 1(반복 수집)** 공백에 초점. 기존 3건은 각각 tapIndex 3(일일), 스킬 분포, tapIndex 2(이정표)를 다뤘으며, CatBox 파괴가 쌓일 때마다 주기적으로 보상이 발생하는 **반복 루프** 설계 자체가 없다는 점은 아직 다루지 않았다. CatBox1은 tapIndex 2 이정표가 있지만 tapIndex 1이 없어 "51개를 향해 달리는 동안 중간 보상이 0개"인 상황도 이번 분석에서만 드러난다.

## 4. 다음 단계 제안

1. **Mission.json에 5행 추가** — missionID 105~109, collectionType 40~44, tapIndex=1, clearValue=10, addValue=10, reward=0, rewardCount=100 (Gold)
2. **clearValue·rewardCount 밸런스 검토** — 보스 스테이지당 CatBox 등장 횟수 시뮬레이션 필요 (StageBlock.json 정밀 분석)
3. **CatBox2~5 tapIndex 2 추가 여부 검토** — 기존 CatBox1 tapIndex 2(clearValue=51) 대비 CatBox2~5의 적절한 이정표 수치 설계
4. **UIMission 스크롤 길이 UI 테스트** — tapIndex 1 항목 최대 17 → 22개로 증가 시 UX 검증

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 보스 스테이지에서 고양이를 집어넣어야 열리는 "고양이 상자(CatBox)"가 5종류 있다. 게임을 하다 보면 이 상자를 수십 번 이상 열게 되는데, 지금은 "얼마나 많이 열었는지"를 세어주는 반복 미션이 하나도 없다. 다른 블록들(기본 고양이, 화살표 폭탄 등)은 10개 부술 때마다 골드를 주는 미션이 있는데, 가장 까다로운 보스 전용 블록인 CatBox만 반복 보상 없이 빠져 있는 것이다. 그래서 이번에 제안하는 것은: CatBox 5종 각각에 "10개 파괴할 때마다 골드를 주는 미션"을 추가해 보스 스테이지를 열심히 플레이할수록 주기적인 보상을 받을 수 있게 연결하자는 것이다.
