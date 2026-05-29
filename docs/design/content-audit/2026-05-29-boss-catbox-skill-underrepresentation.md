# Content Audit — 2026-05-29 — 보스 스테이지 CatBox 스킬 극소 배분 문제

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 4건 (가장 최근: 2026-05-29)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 21종 / 전체 84 | StageBlock.json 실사용 ID 집계, 빈 슬롯 25~39·47~51 제외 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance/NormalStageClear/BlockDestroy/AdWatch |
| 상점 아이템 | 12개 | 스킨 7종 + IAP 3종 + 골드 소비 2종 |
| 고양이 스킨 | 6종 | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |

### 분포 공백

**보스 스테이지 EBossSkillType 실사용 분포 (GPBossController.cs:87~89 분석)**

보스 HP 50% 미만 진입 시 `stage % 10` 값(mod)으로 스킬 조합이 고정된다:

| mod 범위 | 스킬 조합 | 해당 스테이지 수 |
|---|---|---|
| 1~5 | Wall 단독 | 50개 (50%) |
| 6~9 | Wall + Creator | 40개 (40%) |
| 0 (=10, 20, …100) | Wall + Creator + CatBox | 10개 (10%) |

- **Wall**: 100/100 스테이지 (100%)에 등장 — 사실상 필수 스킬
- **Creator**: 50/100 스테이지 (50%)에 등장
- **CatBox**: 10/100 스테이지 (10%)에만 등장 — 그룹당 마지막 스테이지(stage % 10 = 0)에만 출현

각 그룹(100001~100010) 내 1~5번째 스테이지는 반드시 Wall 단독 구간이며, CatBox는 그룹 마지막 스테이지에만 나타난다. 신규 보스 진입 플레이어는 최대 5연속 Wall 단독 스테이지를 거쳐야 비로소 Creator를 만난다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### 보스 스테이지 EBossSkillType 배분 재조정 — CatBox 출현 빈도 확대

- **카테고리**: 보스 스테이지
- **요지**: CatBox 스킬은 보스 스테이지 100개 중 10개(10%)에만 등장한다. Wall 단독 구간 50개를 줄이고, 각 그룹 내 CatBox 출현 위치를 분산시켜 플레이어가 다양한 보스 패턴을 더 빠르게 경험하게 한다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 17
  - 종합 = 4 + (6-2) + 4 + 5 = **17**
- **근거**:
  - `Assets/Scripts/GamePlay/GPBossController.cs:87~89` — `stage % StageGroupSize` 로 스킬 조합 결정
  - `StageGroupSize = 10`, `BossMultiSkillModThreshold = 6` 상수(동 파일:35~39)
  - Stage.json: 보스 스테이지 100개 모두 boardSize=9, blockTypeCount=5 동일 포맷 → 스킬 다양성이 유일한 변별 요소임에도 CatBox가 10%에 그침

#### 유저 플로우

1. **노출 시점·트리거**
   현재 플레이어가 보스 스테이지에 처음 진입(그룹 100001, stage 100001)하면 HP 50% 이하에서 Wall 스킬만 발동한다. 제안 시 그룹 1의 4번째 또는 5번째 스테이지부터 CatBox를 만날 수 있게 배분 기준을 조정한다. 예: mod 조건을 `{0,4,9} → Wall+Creator+CatBox`처럼 3구간 이상으로 분산하거나, 스킬 조합을 JSON 테이블로 외부화하여 그룹별로 달리 지정한다.

2. **화면 변화**
   보스 HP가 50% 미만으로 떨어지는 순간 보스 이미지가 분노(Angry) 상태로 전환되며, 스킬 종류에 따라 보드에 Wall·CatBox·Potal 블록이 스폰된다. CatBox 스킬 발동 시 CatBox1~5(blockState 40~44) 블록이 랜덤 HP(1~9)로 배치되고, HP 0이 되어야 제거된다는 안내 알림이 표시된다.

3. **입력 행동**
   플레이어는 매치-3 드래그로 Cat 블록을 매치하여 점수를 쌓고 보스 HP를 깎는다. CatBox가 배치된 경우, 해당 셀 주변에서 매치를 반복하여 CatBox HP를 소진시키거나, 특수폭탄(RainbowPang·PinkBomb 등)으로 한 번에 제거하는 전략을 선택한다.

4. **시스템 반응**
   CatBox 스킬 발동 후 `BossSkill(EBossSkillType.CatBox)` 호출 → GPBoard가 랜덤 빈 셀에 CatBox1~5 중 하나를 HP 1~9 값으로 스폰한다. 쿨타임은 `StageGroupSize - mod + BossSkillBaseCooldownSeconds` 계산으로 주기적으로 재발동한다.

5. **반복·재발생 패턴**
   쿨타임마다 보스 스킬이 재발동하므로 CatBox가 여러 번 스폰될 수 있다. 보드가 CatBox로 가득 찰 경우 매치 가능 공간이 줄어드는 압박이 높아진다. 재조정 후에는 이 패턴을 보스 스테이지 초반부터 경험하게 된다.

6. **종료·해소 조건**
   플레이어의 보스 HP가 0이 되면 게임 오버(EFailReason.HpOver), 보스의 targetScore를 채우면 클리어(EGameState.GameClear). CatBox 스킬은 클리어 시점까지 주기적으로 발동하므로, 적절한 CatBox 처리 속도를 유지해야 한다.

7. **다른 시스템과 상호작용**
   - GPBombResolver: 특수폭탄 연쇄 시 CatBox에도 대미지 적용 — CatBox 등장 빈도가 높아질수록 폭탄 전략의 가치가 올라간다.
   - UIShop AddMove 아이템: CatBox 처리에 이동 횟수가 소진될 경우 이동추가 아이템 사용 수요 증가 예상.
   - 일일 미션 BlockDestroy(clearValue=100): CatBox 블록 제거가 집계에 포함되는지 코드 검토 필요 — 포함 시 CatBox 빈도 증가로 BlockDestroy 미션 달성 난이도도 함께 조정 대상.

8. **엣지 케이스**
   - 보드 전체가 CatBox로 포화되어 Cat 블록이 스폰되지 않는 데드록 상황 방지 필요 — 스폰 전 빈 셀 수를 체크하거나 최소 Cat 블록 보장 로직 추가 검토.
   - CatBox HP 최대값(BossSkillBlockMaxHp=10)이 낮을 경우 폭탄 한 방에 즉시 제거돼 도전감이 희석될 수 있음 — 그룹 후반부(group 100007~100010)에서는 HP 상한을 상향 조정 가능.
   - 보스 스킬 쿨타임 계산식이 mod=0일 때 `10 - 0 + 10 = 20초`가 되어 다른 mod보다 오히려 쿨타임이 길어지는 구조 — CatBox 스폰 빈도 실측 검증 필요.

9. **유저 정보·피드백**
   현재 보스 스테이지는 모두 동일한 boardSize(9)·blockTypeCount(5)로 스킬만이 유일한 차별 요소다. CatBox 출현이 10개뿐이면 플레이어가 보스 스테이지를 "항상 Wall만 나옴"으로 인식할 가능성이 높다. 출현 빈도를 높이면 "이번엔 CatBox 폭탄 전략으로 돌파" 같은 전략 선택 재미가 생기며, 보스 스테이지의 반복 피로도를 줄일 수 있다.

### 보류

- 보스 HP 드레인 속도(BossHpDrainIntervalSeconds=1초) 조정 — 현황 파악만으로는 난이도 영향 예측 어려움
- EBossSkillType 신규 스킬 추가 — 코드·에셋 양쪽 변경이 필요한 중규모 작업, 현 단계 우선순위 낮음

## 3. 과거 감사 대비 차별성

git log 4건 검토 완료.

가장 유사할 수 있는 과거 커밋: aeffad1 (PotalCreator·CatBox4 tapIndex2 미션 공백) — 그러나 해당 커밋은 **미션 시스템**에서 PotalCreator·CatBox4 수집 미션이 없다는 점을 지적한 것이며, 본 회차는 **보스 스테이지 게임플레이 내 스킬 발동 분포**를 분석한다. 두 주제는 카테고리(미션 vs. 보스 스킬 로직), 근거 파일(Mission.json vs. GPBossController.cs), 개선 방향(미션 데이터 추가 vs. 스킬 배분 로직 수정)이 모두 다르다.

나머지 3건(a6cb70b·08f1ddb·b5bcc97)은 노멀/하드 스테이지 또는 미션 시스템 영역이므로 중복 없음.

## 4. 다음 단계 제안

채택 시:
1. GPBossController.cs의 스킬 발동 분기(lines 87~89) 리팩터링 — mod 상수 또는 JSON 스킬 테이블로 교체
2. 각 보스 그룹(100001~100010)에서 CatBox 등장 스테이지를 최소 3~4개로 상향 (현재: 1개)
3. QA 시뮬레이션으로 CatBox 스폰 포화·데드록 발생 여부 검증
4. DailyMissionService.BlockDestroy 카운터가 CatBox HP 소진 제거를 포함하는지 코드 확인

## 5. 쉬운 설명 (비개발자 요약)

CatPang의 보스 스테이지는 총 100판인데, 보스가 "고양이 상자"라는 특별한 장애물을 꺼내는 판은 딱 10판뿐이다. 나머지 50판은 항상 "벽" 하나만 나와서, 처음 보스에 도전하는 플레이어는 새로운 패턴을 보기까지 몇 판을 지루하게 반복해야 한다. 벽·생성기·고양이 상자 세 가지 패턴을 고루 섞어 배치하면, 매 판마다 "이번엔 어떤 전략을 쓸까?"를 고민하는 재미가 생겨 보스 스테이지가 훨씬 다채로워진다. 그래서 이번에 제안하는 것은: 보스 스테이지 100판에서 고양이 상자 패턴이 등장하는 판 수를 현재 10판에서 30~40판으로 늘리자는 것이다.
