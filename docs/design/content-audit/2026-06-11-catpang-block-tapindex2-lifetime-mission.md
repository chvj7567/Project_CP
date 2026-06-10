# Content Audit — 2026-06-11 — CatPang 블록 tapIndex 2 장기 누적 미션 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 17건 (가장 최근: 2026-06-10, UTC 기준 커밋 2026-06-09)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 31종 / 전체 84 enum 슬롯 | 빈 슬롯 25~39·47~51 15개 제외, 스킨 변종 30종 별도 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance(0), NormalStageClear(1), BlockDestroy(2), AdWatch(3) |
| 상점 아이템 | 12개 | 스킨 7종(tapIndex 1), IAP 3종(tapIndex 2), 골드 아이템 2종(tapIndex 1) |
| 고양이 스킨 | 6종 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |
| tapIndex 2 장기 미션 | 5개 | Fish(31) / CatBox1(51) / WallCreator(71) / RainbowPang(91) / Ball(131) |

### 분포 공백

tapIndex 2 장기 누적 미션(일회성 이정표)이 있는 블록 타입과 없는 주요 블록:

| 블록 | EBlockState | tapIndex 1 | tapIndex 2 | tapIndex 3 |
|---|---|---|---|---|
| CatPang | 18 | ✅ (missionID 7, 10회 반복) | **❌ 없음** | ✅ (missionID 104, 일일 3회) |
| Fish | 24 | ❌ | ✅ (31회) | ❌ |
| CatBox1 | 40 | ❌ | ✅ (51회) | ❌ |
| WallCreator | 45 | ❌ | ✅ (71회) | ❌ |
| RainbowPang | 52 | ❌ (과거 감사 4a09a70 제안) | ✅ (91회) | ❌ |
| Ball | 53 | ❌ | ✅ (131회) | ❌ |

CatPang은 tapIndex 1·3이 모두 있는 유일한 블록임에도 tapIndex 2(장기 이정표)만 비어 있다.
게임 제목 자체인 'CatPang'이 장기 누적 목표 없이 단기 반복·일일 카운터로만 소비된다.

### 과거 감사 후보 (git log 조회 결과)

| # | 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|---|
| 1 | 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2 | 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 3 | 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 4 | 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 5 | 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 6 | 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 7 | 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| 8 | 2026-05-31 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 제안 |
| 9 | 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 — 화살표 폭탄 5회 일일 미션 추가 제안 |
| 10 | 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 11 | 2026-06-03 | 1d91056 | 상점 스킨 골드 가격 선형 계단 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 12 | 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 — Cat1~5 구현 후 2종만 잔여 공백 |
| 13 | 2026-06-05 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 — tapIndex 2 스킬 선택 미션 신설 제안 |
| 14 | 2026-06-06 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 15 | 2026-06-07 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 제안 |
| 16 | 2026-06-08 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |
| 17 | 2026-06-09 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 — IAP 구매자 보상 루프 단절 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### CatPang 블록 tapIndex 2 장기 누적 이정표 미션 신설

- **카테고리**: 미션 (tapIndex 2 장기 일회성 이정표)
- **요지**: 게임 제목과 동일한 CatPang 블록(2×2 정사각형 매치 결과물)이 tapIndex 1 반복·tapIndex 3 일일 미션에는 존재하지만 tapIndex 2 장기 누적 이정표 미션이 유일하게 없다. collectionType=18 항목 하나를 Mission.json에 추가하면 코드 변경 없이 해결된다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 5/1/5/5 → 종합 **20**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json`: tapIndex 2 행(missionID 13~17)에 collectionType=18(CatPang) 항목 없음
  - tapIndex 2 시퀀스는 Fish(31)→CatBox1(51)→WallCreator(71)→RainbowPang(91)→Ball(131)이며 모두 특수 블록인데 CatPang만 누락
  - `Assets/Scripts/Defines.cs` L125: `CatPang = 18` — 다른 tapIndex 2 블록과 동일한 EBlockState 체계 내에 있음
  - `Assets/Scripts/Data.cs` L60-62: Data.Collection은 key-value 구조로 collectionType int 값을 그대로 키로 사용 — "18" 키는 이미 tapIndex 1에서 적산 중
  - 제안 값: `{"missionID":"12-1", "tapIndex":"2", "descStringID":172, "collectionType":18, "clearValue":21, "addValue":-1, "reward":0, "rewardCount":1000}`

#### 유저 플로우

1. **노출 시점·트리거**
   UIMission 화면에서 '장기 미션'(tapIndex 2) 탭을 열면 새 CatPang 이정표 카드가 목록 최상단에 노출된다. clearValue=21이 Fish(31)보다 낮아 가장 먼저 달성 가능한 tapIndex 2 미션으로 배치된다. 게임을 시작한 직후부터 Data.Collection["18"] 누적치가 이미 0 이상이면 진행 바가 채워진 채로 보인다.

2. **화면 변화**
   미션 카드에는 CatPang 블록 아이콘(EBlockState=18의 스프라이트)과 현재 누적 수/목표 21 진행 바가 표시된다. 진행 바가 100%가 되면 카드 우측에 '골드 수령' 버튼이 활성화된다. 달성 전까지는 버튼이 비활성 회색 상태를 유지한다.

3. **입력 행동**
   플레이어는 GameScene에서 2×2 정사각형을 완성시켜 CatPang 블록을 생성하는 행동을 반복한다. 이 행동은 tapIndex 1 미션(10회 반복 골드) 달성과 tapIndex 3 일일 미션(하루 3회) 달성과 동시에 진행되므로 별도 의식적 입력이 필요하지 않다.

4. **시스템 반응**
   CatPang 블록이 보드에 생성될 때마다 GPBombResolver.CreateBombBlock에서 EBlockState.CatPang 생성이 감지되어 Data.Collection["18"] 값이 +1 증가한다. DailyMissionService 또는 동일한 Collection 갱신 경로를 통해 tapIndex 2 진행치도 자동 연동된다. 새로운 코드 경로 없이 기존 컬렉션 증가 로직 그대로 동작한다.

5. **반복·재발생 패턴**
   tapIndex 2 미션은 addValue=-1이므로 일회성이다. 목표 21 달성 후 보상을 수령하면 해당 카드는 '완료' 상태로 바뀌고 더 이상 진행 바가 갱신되지 않는다. 이후 게임에서 CatPang을 추가로 생성해도 이 미션에는 반영되지 않는다. 새 tapIndex 2 이정표가 더 필요하다면 clearValue를 높인 후속 미션을 추가하는 방식으로 확장 가능하다.

6. **종료·해소 조건**
   Data.Collection["18"] 누적값이 clearValue=21 이상이 되는 순간 미션 상태가 달성 가능(Doing→Clear 전환 대기)으로 바뀐다. 플레이어가 UIMission tapIndex 2 탭에서 '골드 수령' 버튼을 탭하면 rewardCount=1000 골드가 Data.Login에 적산되고 미션 clearState가 EClearState.Clear로 확정된다. 보상 수령 이후 카드는 완료 처리되어 목록에서 흐려진다.

7. **다른 시스템과 상호작용**
   - **tapIndex 1 미션(missionID 7)**: 동일한 collectionType=18을 추적하므로 두 미션이 동일한 증가 이벤트를 공유한다. tapIndex 1 달성(10회마다 골드 100) 도중 tapIndex 2 이정표(21회)도 자연스럽게 달성된다.
   - **tapIndex 3 일일 미션(missionID 104)**: 일일 CatPang 3회 카운터는 자정 리셋 스냅샷 기반이라 누적 Collection과 분리된 별도 경로다. 충돌 없다.
   - **상점 골드 경제**: 1000골드 보상은 스킨 구매 경제(최소 10,000골드)의 10%로, 기존 tapIndex 2 미션 보상 수준과 동일하게 책정된다.

8. **엣지 케이스**
   - **신규 유저**: Data.Collection["18"]=0에서 시작하므로 자연 진행. 문제 없다.
   - **기존 유저 소급 적용**: Mission.json에 항목 추가 시, 이미 Collection["18"]≥21인 플레이어는 앱 재시작 직후 미션 탭에서 즉시 보상 버튼이 활성화된다. 소급 달성으로 인한 대규모 골드 지급이 일시적으로 발생할 수 있다. clearValue를 더 높게(예: 50~100) 설정하면 소급 충격을 완화할 수 있다.
   - **스킨 테마 CatPang**: CatCrown 등 스킨이 적용된 경우 Cat1~7 대신 CatCrown1~5 등이 보드에 배치되지만 2×2 매치 결과로 생성되는 CatPang 블록의 EBlockState는 항상 CatPang(18)이다. 스킨과 무관하게 동일하게 카운트된다.
   - **보스 스테이지**: 보스 스테이지에서도 CatPang 블록 생성이 가능하므로 카운트된다. 보스 스테이지 플레이어는 이 미션을 더 빠르게 달성할 수 있다.

9. **유저 정보·피드백**
   미션 카드는 "CatPang 블록 XX / 21회 생성" 형식으로 진행 수치를 실시간 표기한다. 게임 중 CatPang이 생성될 때 기존 특수블록 생성 이펙트(FireCracker 등)가 재생되어 달성이 임박했음을 감각적으로 인지할 수 있다. 보상 수령 시 골드 획득 연출(Gold 사운드 + 상단 골드 카운터 증가 트윈)이 즉시 재생된다. 플레이어는 게임 이름과 동일한 블록을 '21번 만들었다'는 성취감을 정량적으로 확인하게 된다.

### 보류
- **CatBox2/CatBox3/CatBox5 tapIndex 2 공백**: 과거 감사 aeffad1(CatBox4·PotalCreator)과 카테고리(tapIndex 2 누락) 및 근거(Mission.json 미등재) 구조가 동일. 차별화 가능하나 임팩트가 낮음.
- **Ball 블록 일일 미션(tapIndex 3) 신설**: Ball의 독특한 탈출 메커니즘(맨 아래줄 도달 시 Potal 변환)을 일일 목표로 활용하는 아이디어. 하지만 EDailyCounter 신설 및 이벤트 추적 코드 변경 필요 → 구현비용 3점. 추후 검토.
- **보스 스테이지 누적 클리어 tapIndex 2 미션**: collectionType이 EBlockState 외 범위를 사용해야 해 기존 Data.Collection 키 체계와 정합성 검토 필요. 구현비용 높음.

## 3. 과거 감사 대비 차별성

git log 17건 전체 검토 완료.

가장 유사한 과거 커밋:
- **4a09a70** (2026-05-31): "특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락" — **tapIndex 1** 부재가 근거였고 대상은 RainbowPang. 본 제안은 **tapIndex 2** 부재, 대상은 CatPang.
- **aeffad1** (2026-05-29): "PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백" — tapIndex 2 누락이라는 구조는 동일. 단, 대상 블록(PotalCreator/CatBox4)과 근거(장애물 블록 종류 불완전성)가 다르다. 본 제안은 CatPang이 tapIndex 1·3을 **모두 갖추고 있음에도** tapIndex 2만 누락된 구조적 비대칭이 근거이며, '게임 제목 동명 블록의 장기 이정표 부재'라는 플레이어 경험 임팩트가 뚜렷이 다르다.

다른 17건의 커밋은 카테고리(스테이지, 보스, 스킨, 보상 밸런스, EDailyCounter 신설, IAP 연계)가 모두 이 제안과 다르다.

## 4. 다음 단계 제안

채택 시:
1. Mission.json에 신규 항목 추가 — missionID·descStringID 할당, clearValue 결정(소급 충격 고려)
2. StringKorea.json / StringEnglish.json에 descStringID에 해당하는 미션 설명 문자열 추가
3. 기존 유저 소급 골드 지급 규모 시뮬레이션 (현재 평균 CatPang 생성 수 데이터 없으면 보수적으로 clearValue=50 권장)
4. QA: tapIndex 2 미션 카드 렌더링 및 골드 지급 정상 동작 확인

## 5. 쉬운 설명 (비개발자 요약)

이 게임의 이름은 "CatPang"이고, 게임 안에도 2×2 네모 블록을 맞추면 생기는 특별한 블록이 바로 "CatPang 블록"이다. 그런데 이 게임을 대표하는 블록인데도, "지금까지 CatPang 블록을 21번 만들었어요!" 같은 장기 목표가 없다. 마치 게임 이름이 적힌 블록이 가장 중요한 성취 목록에서 빠져 있는 셈이다. 다른 특별한 블록들(물고기, 고양이 상자, 무지개 폭탄 등)은 모두 이런 장기 목표가 있는데 말이다. 그래서 이번에 제안하는 것은: **CatPang 블록을 살면서 21번 만들면 골드 1000개를 주는 이정표 미션을 추가하자**는 것이며, 코드를 건드리지 않고 설정 파일 한 줄만 추가하면 구현된다.
