# Content Audit — 2026-06-21 — 스테이지 진행 이정표 보상 완전 부재 — normalStage/hardStage/bossStage 필드 미션 미연결

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 25건 (가장 최근: 2026-06-20)

## 1. 현황
| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행, 노멀은 하드 테이블 공유 (시간제한 제거 + 이동횟수 2배) |
| 활용 블록 타입 | 약 24종 / 전체 84 | 빈 슬롯 25~39·47~51 제외, 스킨(54~83) 별도 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | 스킨 7, IAP 3, 골드 소모 2 |
| 고양이 스킨 | 6종 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |

### 분포 공백
- **blockTypeCount 포화**: 150개 노멀·하드 스테이지 중 109개(72.7%)가 blockTypeCount=5. 3·4색 스테이지는 초반 41개뿐.
- **이동제한 vs 시간제한 배분**: 노멀·하드 공유 스테이지 150개 중 이동제한 89개, 시간제한 61개. 두 모드 간 차이는 노멀에서 이동횟수 2배만 적용.
- **tapIndex 2 이정표 미션 커버리지**: Fish, CatBox1, WallCreator, RainbowPang, Ball — 5개 특수 블록만 연결. **Data.Login의 normalStage / hardStage / bossStage 숫자는 단 하나의 미션과도 연결되지 않는다.**
- **ConstValue.json 잠재 기준값**: HardStage_NormalStageLock=150, BossStage_HardStageLock=50 — 이 두 값은 해금 게이트에만 쓰이고, 이정표 달성 보상으로는 전혀 활용되지 않는다.

### 과거 감사 후보 (git log 조회 결과)
| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 — 기본 블록 생애 1회 달성 이정표 신설 제안 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 — 보스 전용 아이템 효과 설계 부재 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 — Mission.json 1줄 추가로 신설 제안 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 — 보스 스테이지 조기 진입 허용 설계 재검토 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 — 스킨 장착 보상 루프 신설 제안 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 — PinkBomb~BlueBomb 50개 이정표 신설 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절 제안 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 — 게임 동명 블록 장기 목표 신설 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 — IAP 구매자 보상 루프 단절 제안 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 — tapIndex 2 스킬 선택 미션 신설 |
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

## 2. 추가 컨텐츠 후보 (권장 1개)

### 스테이지 진행 이정표 보상 완전 부재 — normalStage/hardStage/bossStage 미션 미연결
- **카테고리**: 미션 / 스테이지
- **요지**: Data.Login에 normalStage·hardStage·bossStage 진행도 필드가 명시적으로 추적되지만, Mission.json에서 이 값을 clearValue 기준으로 쓰는 미션이 단 하나도 없다. "N번째 스테이지 클리어" 달성 순간의 이정표 보상이 존재하지 않아 플레이어가 스테이지 50·100·150을 달성해도 아무런 보상감 없이 다음 스테이지로 넘어간다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/4 → 종합 **16**
- **근거**:
  - `Assets/Scripts/Data.cs` — `Data.Login.normalStage`, `Data.Login.hardStage`, `Data.Login.bossStage` 진행도 필드가 존재 (line 16~18)
  - `Assets/AssetBundleResources/json/ConstValue.json` — `variable:3, value:150` (HardStage_NormalStageLock), `variable:4, value:50` (BossStage_HardStageLock): 진행도 값이 해금 게이트 판정에만 쓰임
  - `Assets/AssetBundleResources/json/Mission.json` — tapIndex 2(이정표) 5개 항목 전부 `collectionType`이 EBlockState 계열(24·40·45·52·53). `normalStage`·`hardStage`·`bossStage`에 대응하는 collectionType 없음
  - Stage.json 분석: 노멀·하드 공유 150개, 보스 100개, 총 250행 확인

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   플레이어가 노멀 스테이지 10·25·50·100·150번째를 클리어하는 순간(또는 하드 25·50·100·150, 보스 25·50·100)에 미션 탭에 '스테이지 이정표 달성!' 토스트와 함께 보상 대기 뱃지가 점등된다. 기존 스테이지 클리어 연출(UIGameEnd) 직후 이정표 달성 연출이 추가로 재생된다.

2. **화면 변화**
   UIMission의 tapIndex 2(장기 이정표) 탭에 "노멀 50스테이지 클리어" 항목이 생기고, 진행 바가 채워진 상태로 표시된다. 기존 Fish·CatBox1·WallCreator·RainbowPang·Ball 이정표 아래 노멀/하드/보스 각 단계별 이정표 항목이 나열된다.

3. **입력 행동**
   유저가 UIMission tapIndex 2 탭으로 진입한 뒤 달성된 이정표 항목을 탭한다. 기존 수집 미션 보상 수령 UI와 동일한 골드·아이템 획득 팝업이 표시된다. 탭 1회로 수령 완료, 재수령 불가(addValue=-1 정책 유지).

4. **시스템 반응**
   CHMData가 normalStage 값이 이정표 clearValue(예: 50)에 처음 도달한 시점에 해당 Data.Mission.clearState를 Doing→Clear로 갱신하고 rewardCount(금화)·EReward(AddTime/AddMove 등) 지급 처리한다. DailyMissionService.CheckAndResetIfNeeded와 무관하게 1회성 지급이므로 타이머 리셋과 무관하다.

5. **반복·재발생 패턴**
   addValue=-1로 설정해 1회만 달성 가능하다. 노멀 10·25·50·100·150 / 하드 25·50·100·150 / 보스 25·50·100 등 여러 단계를 설계하면, 플레이어는 게임 수명 전반에 걸쳐 총 10~14회의 이정표 보상 획득 기회를 얻는다. 이미 해당 스테이지를 넘어선 기존 유저는 게임 첫 로그인 시 Data.Login.normalStage 값을 역산해 일괄 지급 처리한다.

6. **종료·해소 조건**
   보상을 수령하면 항목이 '완료' 상태로 고정되고 진행 바가 만재 표시된다. 마지막 이정표(노멀 150 / 하드 150 / 보스 100 완료)까지 수령하면 해당 게임 타입의 이정표 섹션이 '전부 달성' 상태로 표시된다.

7. **다른 시스템과 상호작용**
   ConstValue.json의 HardStage_NormalStageLock(150)·BossStage_HardStageLock(50) 게이트와 타이밍이 겹친다. 예를 들어 "노멀 150 이정표 달성" 보상을 받는 시점이 곧 하드 해금 시점이므로, 해금 연출과 이정표 보상 연출을 순서대로 표시해야 한다. 이중 연출 순서 정의가 구현 시 핵심 주의사항이다.

8. **엣지 케이스**
   - 기존 유저가 이미 노멀 150을 초과한 상태로 업데이트된 경우: 첫 앱 실행 시 normalStage≥clearValue인 항목을 모두 즉시 Doing→Clear로 전환하고 UIMission 뱃지를 표시한다.
   - 오프라인 플레이 후 NTP 미수신 상태에서 이정표 달성: 스테이지 클리어 저장은 localData 기반이므로 NTP와 무관하게 즉시 반영 가능하다. DailyMissionService의 NTP 리셋 로직과 독립적으로 동작한다.
   - 보스 스테이지 bossStage=53(max 100 미만)에서 앱 강제 종료 후 재접속: Data.Login은 persistentDataPath JSON 기반이므로 클리어 값이 보존된다. clearState 갱신 누락 없이 정상 체크된다.

9. **유저 정보·피드백**
   이정표 수령 UI에 "지금까지 노멀 스테이지를 50개 클리어했어요!" 메시지를 CHTMPro로 표시하고 미션 설명 StringID를 신설한다. 보상 획득 후 소셜 공유 버튼(GPGS)을 선택적으로 노출해 "스테이지 50 달성!" 외부 공유를 유도할 수 있다.

### 보류
- **Arrow 폭탄 tapIndex 2 이정표 공백**: tapIndex 2 이정표 영역은 2026-06-15(특수폭탄), 2026-06-11(CatPang), 2026-06-20(Cat1~5)에서 반복 제안. 같은 카테고리·요지·근거 구조가 누적 25건 중 4건을 점유하므로 이번 회차 보류.
- **EBackground 4종 vs 400스테이지 시각 단조로움**: 배경 다양화는 새 아트워크(구현비용 3) 필요. 종합점수 13으로 이번 회차 차순위.

## 3. 과거 감사 대비 차별성
git log 25건 검토 완료.

가장 유사한 과거 커밋:
- **2026-06-20 (5f57450)**: "Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백" — collectionType(블록 파괴 누적 횟수)을 clearValue 기준으로 쓰는 이정표 미션 공백
- **2026-06-07 (26ebd5a)**: "하드 스테이지 해금 임계값 150/150" — normalStage 진행도 값을 게이트에 쓰는 것을 다룸

**차별점**:
1. 과거 tapIndex 2 이정표 제안들은 모두 **블록 파괴·생성 누적 횟수**(EBlockState collectionType)를 기준으로 삼는다. 이번 제안은 **스테이지 번호 도달 진행도**(normalStage/hardStage/bossStage 필드)를 clearValue 기준으로 쓰는 완전히 다른 데이터 축이다.
2. 2026-06-07 제안은 해금 임계값(gate) 자체의 비대칭성을 지적했지만, 이번 제안은 같은 진행도 값을 **보상 이정표(reward milestone)**로 재활용하는 미연결 상태를 지적한다. 같은 필드를 다른 목적으로 활용하는 제안이다.
3. Data.Login 3개 필드 모두 추적 중이면서 Mission.json에 연결이 0개인 점은 이번 감사에서 처음 정량 확인됐다.

## 4. 다음 단계 제안
채택 시 구체 구현 계획 수립 필요:
- Mission.json에 새 collectionType 차원 정의 필요 (현재 EBlockState int 값과 충돌하지 않는 별도 namespace 고려)
- 또는 Data.Login.normalStage/hardStage/bossStage 직접 폴링하는 별도 MilestoneService 설계 검토
- 기존 유저 마이그레이션 처리 로직 (첫 업데이트 실행 시 일괄 체크) 선행 설계 필요
- StringKorea.json / StringEnglish.json에 미션 설명 문자열 추가 (descStringID 신규 할당)

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 총 400개의 스테이지가 있고, 게임이 플레이어가 몇 번째 스테이지까지 왔는지 숫자를 꼼꼼하게 기록하고 있다. 그런데 이 숫자는 다음 스테이지를 잠금 해제하는 데만 쓰이고, 플레이어가 50번째·100번째·150번째 스테이지를 깰 때 "축하 선물"을 주는 데는 전혀 활용되지 않는다. 마치 마라톤에서 10km, 21km, 42km 지점마다 물과 간식을 나눠줘야 하는데 결승선에 도착할 때까지 아무것도 없는 것과 같다. 그래서 이번에 제안하는 것은: 노멀·하드·보스 스테이지 각각 25·50·100 달성 지점마다 골드나 아이템을 한 번씩 지급하는 "스테이지 이정표 달성 보상" 미션을 추가하자는 것이다.
