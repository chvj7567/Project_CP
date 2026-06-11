# Content Audit — 2026-06-12 — Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 미설계

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StageBlock, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 18건 (가장 최근: 2026-06-10)

## 1. 현황
| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 31종 / 전체 84 | 빈 슬롯 7~9·25~39·47~51 제외, 스킨 30종(54~83) 별도 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | 스킨 7 + IAP 3 + 골드 소모 2 |
| 고양이 스킨 | 6종 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry (각 Cat1~5) |

### 분포 공백
- `Data.Stage.boomAllCount` 필드가 기본값 `-1`로 정의되어 있으나, `GPBombResolver.BoomAll()` 내부 포함 게임플레이 코드 전체에서 **단 한 군데도 쓰지 않음** (grep 확인: Data.cs 정의 1건만 존재).
- BoomAll은 두 색폭탄(EBlockState 19~23)을 스왑할 때 발동하며 `BoomAllBonusScore = 5` 보너스 점수를 주고 전체 보드를 폭발시킨다. 강력한 아이템임에도 사용 여부를 사후에 조회할 수단이 없다.
- 현재 스테이지 클리어 후 UIGameEnd에는 성공/실패(EFailReason)만 표시된다. 별점·플레이 품질 평가가 존재하지 않는다.

### 과거 감사 후보 (git log 조회 결과)
| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-10 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-09 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-08 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-07 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-06 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 |
| 2026-06-05 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 공백 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-03 | 1d91056 | 상점 스킨 골드 가격 선형 계단 |
| 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 |
| 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-05-31 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### BoomAll 미사용 클리어 — "BoomAll 없이 클리어" 보너스 시스템
- **카테고리**: 스테이지 / 진행 보상
- **요지**: `Data.Stage.boomAllCount` 필드가 이미 존재하지만 코드 어디서도 기록하지 않는다. BoomAll(전체 폭발 아이템) 없이 스테이지를 클리어했을 때 추가 보상(골드·컬렉션 수치 보너스)을 지급하는 "순수 클리어" 마킹 시스템을 신설하면, 기존 필드를 활성화하고 리플레이 동기를 동시에 만든다.
- **점수**: 검증가치 4 / 구현비용 2 / 플레이어경험개선 4 / 데이터근거 5 → **종합 17**
- **근거**:
  - `Assets/Scripts/Data.cs:54` — `public int boomAllCount = -1;` 기본값 -1, 정의만 있고 write 없음
  - `Assets/Scripts/GamePlay/GPBombResolver.cs:86-88` — `BoomAll()` 메서드가 boomAllCount를 업데이트하지 않음
  - `grep -r "boomAllCount" Assets/Scripts` 결과: Data.cs 정의 1건 외 0건
  - BoomAll 발동 시 `BoomAllBonusScore = 5`만 추가되고 별도 클리어 품질 평가 없음
  - UIGameEnd는 `EFailReason`(TimeOver/MoveOver/HpOver)과 GameClear 상태만 표시

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   스테이지 클리어 직후 UIGameEnd가 뜨는 시점에, 해당 판에서 BoomAll을 한 번도 발동하지 않았다면 "BoomAll 없이 클리어!" 뱃지와 추가 골드 보상이 팝업 내에 표시된다. BoomAll을 한 번이라도 발동했으면 뱃지 없이 일반 클리어 UI가 표시된다.

2. **화면 변화**
   UIGameEnd 클리어 영역에 별(★) 아이콘 또는 "순수 클리어" 텍스트 레이블이 추가된다. 스테이지 선택 화면(UIStageSelect)에서도 해당 스테이지 슬롯 우측 상단에 작은 왕관 아이콘이 표시되어 "BoomAll 없이 클리어" 기록이 남아 있음을 알린다.

3. **입력 행동**
   플레이어는 BoomAll 콤보(색폭탄 두 개 스왑)를 의도적으로 피하면서 일반 매치·화살표 폭탄·CatPang 블록만으로 스테이지를 클리어하려 시도한다. 실수로 색폭탄 두 개를 인접시키지 않도록 블록 배치에 더 신중하게 드래그한다.

4. **시스템 반응**
   GPBombResolver.BoomAll()이 호출되는 순간, Data.Stage.boomAllCount를 1 이상으로 설정(첫 발동 시 0→1, 이후 매 발동마다 +1)한다. 스테이지 클리어 판정 시 boomAllCount == 0이면 추가 골드 보상(예: 기본 보상 × 1.5)을 지급하고 clearState에 "순수 클리어" 플래그를 추가로 마킹한다.

5. **반복·재발생 패턴**
   이미 순수 클리어를 달성한 스테이지라도 재플레이해 BoomAll을 사용하면 순수 클리어 기록이 취소된다. 반대로 BoomAll을 사용했던 스테이지를 재플레이해 BoomAll 없이 클리어하면 순수 클리어 기록이 새로 적힌다. 이를 통해 반복 플레이를 유도한다.

6. **종료·해소 조건**
   전체 150개 하드·노멀 스테이지를 모두 순수 클리어 달성하면 UIStageSelect에 "전 스테이지 순수 클리어" 특별 뱃지가 표시되고, 한 번만 지급되는 대형 골드 보상(예: 50,000골드)이 지급된다. 보스 스테이지는 BoomAll 발동 구조가 동일하므로 선택적으로 포함 가능하다.

7. **다른 시스템과 상호작용**
   - UIGameEnd: 클리어 결과에 순수 클리어 뱃지 UI 추가
   - UIStageSelect: 스테이지 슬롯에 순수 클리어 아이콘 렌더링
   - Data.Stage: boomAllCount 쓰기 활성화 (기존 필드 재사용)
   - GPBombResolver.BoomAll(): boomAllCount 증가 한 줄 추가
   - CHMData: 클리어 보상 골드 지급 경로 연계 (기존 gold 지급 로직 재사용)

8. **엣지 케이스**
   - 스테이지 도중 앱 종료 후 재시작: boomAllCount는 클리어 시점에만 저장하므로 중간 집계는 휘발된다. GPGameScene 세션 단위 카운터를 별도 로컬 변수로 유지하다 클리어 확정 시 Data.Stage에 쓴다.
   - 네트워크 오류로 클라우드 저장 실패: boomAllCount는 로컬 JSON에 먼저 저장하며 GPGS 동기화는 최선 노력(best-effort) 방식이므로 기존 CHMData 흐름과 동일.
   - RainbowPang 발동(보드에 색폭탄 살포): 색폭탄 2개 스왑이 아니라 RainbowPang 자체 발동이므로 BoomAll 경로가 아님 → boomAllCount에 영향 없음.

9. **유저 정보·피드백**
   UIGameEnd에서 "BoomAll 없이 클리어!" 텍스트와 추가 골드 수치가 명시되어 플레이어가 보상 근거를 즉시 이해한다. UIStageSelect의 아이콘은 스테이지 진입 전 "아직 순수 클리어 못 한 스테이지"를 한눈에 식별하게 해주어 자연스럽게 리플레이 목표를 제시한다.

### 보류
- **EBackground 진행 해금 시스템** (종합 13): EBackground 1~4가 enum으로 존재하지만 unlock 메커니즘 없음. 유효한 공백이나, boomAllCount 활성화보다 신규 UI/로직이 더 필요해 구현 비용이 높음.
- **EDailyCounter.BossStageClear 신설** (종합 12): HardStageClear(2026-05-29)와 카테고리가 동일해 중복 유사도 높음.
- **스테이지 그룹 완료 보너스** (종합 16): group 1~15 각 10스테이지 완주 보상. 유효하나 Data.Stage에 group-level 집계 필드가 없어 신규 구조 추가 필요. boomAllCount 활성화보다 점수 낮음.

## 3. 과거 감사 대비 차별성
git log 18건 검토 완료.

- **가장 유사한 과거 커밋**: 없음 — 기존 감사 모두 미션 JSON 공백, 일일 카운터 신설, 스테이지 포맷, 보상 불균형, 스킬 연계 등을 다루었다. **`Data.Stage.boomAllCount`라는 코드 레벨 잠든 필드를 근거로 삼은 감사는 18건 중 단 한 건도 없다.**
- 차별점: 이번 후보는 새 데이터 구조 추가나 JSON 항목 추가 없이 **기존 필드를 활성화**하는 방향으로, 구현 비용이 가장 낮은 동시에 데이터 근거가 가장 명확하다.

## 4. 다음 단계 제안
- 채택 시: `GPBombResolver.BoomAll()` 내부에 boomAllCount 증가 로직 한 줄 추가 → UIGameEnd에 순수 클리어 뱃지 표시 → UIStageSelect 슬롯에 아이콘 추가 → 클리어 시 골드 지급 분기 추가 순서로 진행.
- 기획 검토 포인트: 순수 클리어 보상 골드 배율(×1.5?), 재클리어로 기록 갱신 가능 여부, 보스 스테이지 포함 범위.

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 "BoomAll"이라는 강력한 아이템이 있다. 두 개의 특별한 폭탄 블록을 서로 맞바꾸면 보드 전체가 한번에 폭발하는 기술인데, 쓰면 쉽게 이길 수 있지만 그 기술 없이 이기면 오히려 더 뿌듯하다. 게임 코드에는 "BoomAll을 몇 번 썼는지" 기록하는 칸이 이미 만들어져 있는데, 지금은 그 칸이 텅 비어 있다. 아무것도 기록하지 않고 있는 것이다. 그래서 이번에 제안하는 것은: 이 빈 기록칸을 실제로 사용해서, BoomAll 없이 스테이지를 깨면 "순수 클리어" 뱃지와 추가 골드를 주는 시스템을 만들자는 것이다.
