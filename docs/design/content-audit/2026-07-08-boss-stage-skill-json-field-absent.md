# Content Audit — 2026-07-08 — 보스 스테이지 Stage.json 스킬 조율 필드 전무 — 100개 스테이지 쿨타임 코드 상수 단일 의존

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 27건 (가장 최근: 2026-07-07)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 25종 / 전체 84 | 빈 슬롯 25~39·47~51(15개) 제외, Cat1~7·Arrow1~6·Wall·Potal·CatPang·5색폭탄·Fish·CatBox1~5·WallCreator·PotalCreator·RainbowPang·Ball·스킨고양이 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1(스킨 8개) + tapIndex 2(RemoveAD·AddTime·AddMove·기타 4개) |
| 고양이 스킨 | 6종 (Cat1~5 × 테마 6) | CatCrown·CatFlowers·CatMushroom·CatParty·CatSanta·CatStrawberry |

### 분포 공백 — 보스 스테이지 Stage.json 스킬 조율 필드 분석

**Stage.json 보스 스테이지 전체 필드 목록** (실측):
```
blockTypeCount, boardSize, group, moveCount, stage, targetScore, time, tutorialID
```
스킬 관련 필드: **전무**. `bossSkillCooldown`, `bossSkillType`, `bossSkillCount` 등 어떤 스킬 조율 필드도 존재하지 않는다.

**GPBossController 스킬 로직** (`Assets/Scripts/GamePlay/GPBossController.cs`):
- 보스 스킬 발동은 모두 코드 상수(`BossSkillBaseCooldownSeconds = 10`, `StageGroupSize = 10`)와 `stage % 10` 공식으로만 결정된다.
- 쿨타임 공식: `mod==0` → 10초, `mod>=6` → (10-mod+10)초, `mod 1~5` → (10-mod+10)초
- **그룹 번호(group 필드)는 쿨타임 공식에 전혀 참여하지 않는다.**

| 보스 mod (stage % 10) | 스킬 종류 | 쿨타임 | 해당 스테이지 수 |
|---|---|---|---|
| 1~5 | Wall 단독 | 19s / 18s / 17s / 16s / 15s | 50개 (50%) |
| 6~9 | Wall + Creator | 14s / 13s / 12s / 11s | 40개 (40%) |
| 0 | Wall + Creator + CatBox | 10s | 10개 (10%) |

**그룹별 targetScore 대 쿨타임 불일치**:
| 그룹 | targetScore | 보스 스킬 쿨타임 (mod==0 기준) | 비고 |
|---|---|---|---|
| Group 1 (stage 100001~100010) | 1,000 | 10초 | 기준 |
| Group 5 (stage 100041~100050) | 1,800 | 10초 | targetScore 1.8배, 쿨타임 동일 |
| Group 10 (stage 100091~100100) | 2,800 | 10초 | targetScore 2.8배, 쿨타임 동일 |

→ targetScore가 그룹 1→10으로 2.8배 선형 증가하지만, 보스 스킬 쿨타임은 상수 고정이다.  
→ 스킬 빈도가 증가하지 않으므로, 후반 그룹에서 체감 난이도 상승은 "더 많은 점수를 내야 한다"는 양적 부담뿐이며 전술적 긴장감은 그룹 1과 동일하다.

### 하드·노멀 스테이지 제약 모드 분포 (참고)
- 시간제한 단독: 61개 (41%) / 이동 제한 단독: 77개 (51%) / 복합(양쪽): 12개 (8%)

### 과거 감사 후보 (git log 조회 결과 — 27건)

| 날짜 | SHA | 설명 요약 |
|---|---|---|
| 2026-07-07 | 1324855 | 보스 스테이지 attack 잠든 공격력 스탯 — 매 턴 +0 가산, 획득 경로 전무 |
| 2026-07-06 | 80d875f | Wall 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-05 | 18dd561 | tapIndex 2 장기 이정표 미션 보상 Gold 단일화 — AddTime·AddMove 전무 |
| 2026-07-04 | 39d8ced | EBackground 4종 게임판 배경 커스터마이징 보상 루프 공백 |
| 2026-07-03 | 5c2ba4c | Wall·Potal 초등장 스테이지(6·7) Tutorial.json 항목 완전 공백 |
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 고아 항목 |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-27 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 87% 후기 도입 |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 완전 부재 |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-13 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 클리어 보너스 미설계 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### 보스 스테이지 Stage.json 스킬 조율 필드 신설 — 그룹별 스킬 쿨타임·종류 데이터 기반 조율 가능화

- **카테고리**: 보스 / 스테이지 데이터 설계
- **요지**: 보스 100개 스테이지 전체의 스킬 쿨타임과 스킬 종류가 GPBossController의 코드 상수 1개(`BossSkillBaseCooldownSeconds = 10`)로만 결정된다. Stage.json에 스킬 조율 필드가 없으므로, 게임 디자이너는 JSON 편집만으로 보스 난이도를 조율할 수 없으며 매 튜닝 시마다 C# 코드를 수정해야 한다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 5/3/4/5 → 종합 **17점**
  - 검증가치 5: 디자이너가 보스 스킬을 조율하려면 코드를 반드시 고쳐야 하는 운영 블로커가 명확히 존재
  - 구현비용 3: Stage.json 필드 추가 + GPBossController에서 해당 필드 참조로 교체, 중간 비용
  - 플레이어경험 4: 그룹별 스킬 빈도 조율 가능 → 후반 보스 단조로움 개선, 그룹 10이 그룹 1보다 확실히 다른 체감 제공
  - 데이터근거 5: Stage.json 필드 목록 실측(8개 필드, 스킬 관련 0개) + GPBossController 코드 직접 확인

- **근거**:
  - `Assets/AssetBundleResources/json/Stage.json` — 보스 스테이지 필드: `blockTypeCount, boardSize, group, moveCount, stage, targetScore, time, tutorialID`. 스킬 관련 필드 **0개**.
  - `Assets/Scripts/GamePlay/GPBossController.cs:34` — `const int BossSkillBaseCooldownSeconds = 10;` 전체 100개 보스 스테이지에 동일 적용.
  - `GPBossController.cs:86-89` — 쿨타임 분기가 `stage % StageGroupSize` (mod)에만 의존하며 `group` 값은 쿨타임 계산에 미사용.
  - targetScore: Group 1 = 1,000 → Group 10 = 2,800 (+200/그룹, 2.8배). 스킬 쿨타임 배율: 1.0배(고정). 난이도 구성 요소 간 비례 불일치.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**  
   보스 스테이지 선택 후 GameScene이 로드되고 GPBossController가 Init()되는 순간부터 스킬 쿨타임이 결정된다. 플레이어가 보스 스테이지 100010(Group 1 마지막)과 100100(Group 10 마지막)에 진입할 때 각각 완전히 동일한 10초 쿨타임으로 시작된다. 현재 구조에서는 디자이너가 Group 10의 스킬 빈도를 높이고 싶어도 Stage.json 수정으로는 불가능하다.

2. **화면 변화**  
   보스 HP가 50% 임계값(`BossSkillHpThreshold = 0.5f`) 이하로 떨어지는 순간 보스 이미지가 분노 상태(`_angryBossObj`)로 전환되고 스킬 타이머가 시작된다. 이 전환 연출은 Group 1이든 Group 10이든 동일하다. 제안 시: Group별로 다른 타이머 설정이 가능해지면, 후반 그룹에서는 보스 분노 전환 후 더 빠른 주기로 장애물이 쏟아지는 압박감을 연출할 수 있다.

3. **입력 행동**  
   플레이어는 매치-3 드래그로 점수를 누적시켜 보스의 targetScore × 50%를 초과하는 것을 목표로 한다. 스킬 발동 단계에서는 장애물 블록이 주기적으로 보드에 추가되므로, 플레이어는 장애물을 우선 제거하는 드래그와 점수 누적 드래그 사이에서 선택해야 한다. Group 10에서 targetScore가 2,800이면 스킬 단계 진입 후 1,400점을 더 내야 하지만, 장애물 출현 빈도는 Group 1(targetScore 1,000, 스킬 단계 후 500점 추가)과 동일하다.

4. **시스템 반응**  
   `Observable.Timer(간격=coolTime초)` 구독으로 쿨타임마다 `BossSkill(EBossSkillType)` 호출 → GPBoard.boardArr에서 무작위 일반 블록 위치를 선택해 장애물 블록으로 교체(`changeBlockState`, `changeHp` 설정). mod==0 스테이지는 Wall·Creator·CatBox 세 종류의 스킬을 하나의 타이머로 한꺼번에 실행한다. Stage.json에 필드가 생기면 각 스테이지별로 이 값들을 독립 제어할 수 있다.

5. **반복·재발생 패턴**  
   스킬 타이머는 보스 분노 전환 후 Owner(GPGameScene)가 소멸될 때까지 계속 반복된다. Group 10 mod==0 스테이지(stage 100100, targetScore 2,800)에서 스킬 단계(1,400점 이후)의 지속 시간은 Group 1 mod==0(stage 100010, targetScore 1,000, 스킬 단계 후 500점 추가)보다 평균적으로 약 2배 이상 길다. 그러나 10초 쿨타임이 같으므로 단위 시간당 장애물 등장 횟수는 동일하고, 단지 그 상태가 더 오래 지속된다.

6. **종료·해소 조건**  
   플레이어가 targetScore를 달성하면 GPGameScene에서 GameClear 판정이 나고 보스 스킬 타이머가 소멸된다. 실패 조건은 `hp.Value <= 0` (플레이어 HP 소진, 1초마다 자동 감소) 또는 시간/이동 제한 소진. 보스 스테이지는 시간(time=-1)·이동(moveCount=-1) 제한이 없으므로 실질적 실패 원인은 HP 소진뿐이다. 후반 그룹에서 목표 점수가 높아 싸움이 길어질수록 HP 자연 소진 위험이 커지지만, 스킬 빈도는 동일하므로 직접적인 압박 가중은 없다.

7. **다른 시스템과 상호작용**  
   `BossSkill`이 설치한 Wall/Potal은 인접 매치로 HP를 깎아야 제거되고, WallCreator/PotalCreator는 매 턴 주변 블록을 새 장애물로 변환한다. CatBox는 맞는 색의 고양이 블록이 박스 위 칸에 놓여야 HP가 줄어드는 별도 메커니즘이다. 이 세 스킬의 상호작용 복잡도가 다름에도 불구하고 Stage.json 필드가 없어 동일 스테이지에 세 종류를 동시에 넣을지, 하나만 넣을지를 JSON 레벨에서 선택할 수 없다.

8. **엣지 케이스**  
   mod==0 스테이지는 쿨타임 10초(최단)에 3종 스킬이 동시 실행되므로, 보드가 단시간에 복수의 장애물 유형으로 가득 찰 수 있다. 특히 CatBox가 생성된 상황에서 RainbowPang이 발동하면(다른 요인에 의해) 색폭탄이 무작위 살포되어 CatBox의 HP가 예기치 않게 감소하는 시너지가 발생할 수 있다. 반대로 mod==1 스테이지는 19초라는 긴 쿨타임 때문에 보스 스킬이 한 번도 발동되지 않고 클리어될 수 있다(빠른 플레이어의 경우).

9. **유저 정보·피드백**  
   현재 보스 스킬 쿨타임이나 스킬 종류에 대한 UI 정보가 전혀 없다. 플레이어는 "언제 다음 장애물이 오는가"를 알 방법이 없고, 보스 분노 후 UIAlarm(stringID=78)으로 경고 팝업이 한 번 뜰 뿐이다. Stage.json에 필드를 추가해 그룹별로 튜닝이 가능해지더라도, 플레이어에게 스킬 예고 정보를 노출하는 추가 UX를 고려할 수 있다(예: 보스 HP 게이지 옆 스킬 타이머 표시).

### 보류
- **보스 스킬 쿨타임 역전 현상** (mod==0 최단 10초·3종 vs mod==1 최장 19초·1종): 내부 설계 불일치이지만 위 제안의 하위 항목으로 포함되므로 별도 우선 후보에서 제외.
- **보스 스테이지 boardSize 전수 9** (모든 보스 스테이지가 동일한 9×9 보드): 스킬 조율 필드 신설로 해결 가능한 범위를 벗어나므로 독립 후보로 보류.

## 3. 과거 감사 대비 차별성

git log 27건 검토 완료.

**가장 유사했던 과거 커밋**: `1324855` (2026-07-07) "보스 스테이지 attack 잠든 공격력 스탯 — 매 턴 +0 가산, 획득 경로 전무"  
→ 차별점: 해당 감사는 `Data.Login.attack` 필드(플레이어 공격력)가 사용되지 않는 점에 초점. 본 감사는 Stage.json의 보스 스킬 제어 필드 부재라는 **데이터 설계 구조** 문제에 초점. 대상 시스템(플레이어 스탯 vs. 스테이지 데이터 스키마)과 개선 방향이 상이하다.

**관련 파일로 보이나 범위 다른 것**: `2026-05-29-boss-catbox-skill-underrepresentation.md` (git log 이전 파일)  
→ 해당 파일은 CatBox 스킬이 100개 보스 스테이지 중 10개(10%)에만 등장한다는 **비율 문제**에 초점. 본 감사는 그 근본 원인인 Stage.json 스킬 필드 부재 — 즉 **설계자가 JSON으로 스킬을 조율할 구조 자체가 없음** — 이라는 상위 구조 문제에 초점. 처방도 다름: 해당 파일은 "CatBox 더 많이 넣기", 본 제안은 "스킬 종류·쿨타임을 JSON 필드로 제어하는 구조 신설".

## 4. 다음 단계 제안

1. **Stage.json 스키마 확장** — `bossSkillCooldown: number`, `bossSkillTypes: string[]` 필드 추가
   - 예: `{"stage": 100010, ..., "bossSkillCooldown": 10, "bossSkillTypes": ["Wall","Creator","CatBox"]}`
2. **GPBossController 수정** — `BossSkillBaseCooldownSeconds` 상수 참조를 `_stageInfo.bossSkillCooldown` (StageInfo 파싱 필드) 참조로 교체
3. **StageBlock.json 연동** — 실제 보드에서 어떤 블록 타입과 조합되는지 검토하여 그룹별 스킬 타입을 결정
4. **그룹별 쿨타임 가이드라인 수립** — 예: Group 1 = 18초, Group 5 = 14초, Group 10 = 10초 등 선형 감소 초안 수립 후 QA 시뮬레이션으로 검증

## 5. 쉬운 설명 (비개발자 요약)

CatPang의 보스 스테이지는 총 100개인데, 보스가 함정 블록을 심는 속도(주기)가 첫 번째 보스 스테이지나 마지막 보스 스테이지나 완전히 똑같다. 즉, 가장 어려운 보스 스테이지(100개 중 100번째)를 만나도 보스가 함정을 심는 박자는 처음 보스를 만날 때와 다를 게 없어서, 후반부 보스가 더 강한 느낌이 목표 점수가 높다는 것 말고는 잘 안 든다. 현재 게임 설계 파일(Stage.json)에는 이 속도를 스테이지마다 다르게 조정할 칸 자체가 없어서, 기획자가 바꾸고 싶을 때는 프로그래머를 불러 코드를 직접 수정해야만 한다. 그래서 이번에 제안하는 것은: Stage.json에 "보스 함정 주기" 항목을 추가해서, 후반 그룹일수록 보스가 더 빠르고 다양한 방식으로 함정을 심도록 기획자가 직접 수치를 조절할 수 있게 만드는 것이다.
