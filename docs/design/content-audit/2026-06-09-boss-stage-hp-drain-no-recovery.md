# Content Audit — 2026-06-09 — 보스 스테이지 플레이어 HP 소진·회복 루프 미설계

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StringEnglish, StageBlock, StringKorea, Tutorial)
- 참조 소스 코드: GPBossController.cs, Data.cs
- 과거 감사 이력 (git log): 15건 (가장 최근: 2026-06-07 KST → commit 329cce2)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯(7~9, 25~39, 47~51) 23개 제외 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | 스킨 7개 + RemoveAD + AddTime + AddMove + 골드상품 2개 (모두 IAP 또는 골드) |
| 고양이 스킨 | 6종 테마 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |
| 보스 스테이지 플레이어 HP | 최대 100 (Data.Login.hp 기본값) | 보스전 중 1초마다 -1; 회복 경로 없음 |

### 보스 HP 소진 구조 (GPBossController.cs 기반)

```
BossHpDrainIntervalSeconds = 1  // 1초마다 hp.Value -= 1
보스 스킬 발동 임계: hp <= 50% (BossSkillHpThreshold)
스킬 발동 패턴:
  stage % 10 == 0  → Wall + Creator + CatBox, 쿨타임 10s
  stage % 10 >= 6  → Wall + Creator, 쿨타임 (10 + 10 - mod)s
  stage % 10 < 6   → Wall only, 쿨타임 (10 + 10 - mod)s
```

HP=100에서 시작 시 보스전에서 최대 100초(약 1분 40초) 버팀. 실패하거나 클리어한 뒤 HP가 잔여값으로 저장되면 다음 보스전의 생존 가능 시간이 단축됨.

### EReward / Mission / Shop 현황 — HP 관련 항목

| 소스 | HP 관련 항목 |
|---|---|
| `EReward` | Gold(0), AddTime(1), AddMove(2) — HP 항목 없음 |
| Mission.json (27개) | 보상 타입 0(Gold)/1(AddTime)/2(AddMove) 사용만 확인, HP 없음 |
| Shop.json (12개) | IAP 또는 골드 소비 상품만 존재, HP 회복 없음 |
| ConstValue.json | AddMoveItemValue=1, AddTimeItemValue=10 — HP 관련 상수 없음 |
| Data.Login | `hp = 100`, `attack = 0` 두 필드가 저장 구조에 존재하나 증가 경로 미구현 |

### 과거 감사 후보 (git log 조회 결과)

| 날짜(KST) | SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | tapIndex 1 Cat1~5 수집 미션 공백 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 (10% → 30~40% 확대) |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 누락 |
| 2026-05-31 | 4a09a70 | RainbowPang 반복 미션 누락 제안 |
| 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 누락 |
| 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회가 BlockDestroy 100개의 10배 |
| 2026-06-03 | 1d91056 | 상점 스킨 골드 가격 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-05 | 7d601d5 | ESelect 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-06 | 26ebd5a | 하드 스테이지 해금 임계값 150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 2026-06-07 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### 보스 스테이지 플레이어 HP 회복 경로 신설

- **카테고리**: 보스 스테이지 / 보상 시스템
- **요지**: Data.Login에 hp/attack 저장 필드가 존재하고 GPBossController가 이 값을 보스전 내 시간 기반 소모에 사용하지만, EReward·Mission·Shop 어디에도 HP를 회복하는 경로가 없다. 보스전을 반복할수록 HP가 점점 낮아져 생존 가능 시간이 줄어드는 구조적 공백.
- **점수**: 검증가치 4 / 구현비용 3 / 플레이어경험개선 5 / 데이터근거 5 → **종합 17**
- **근거**:
  - `Assets/Scripts/GamePlay/GPBossController.cs:64` — `hp.Value = loginData.hp` (저장된 HP를 보스전 시작 시 불러옴)
  - `GPBossController.cs:66-68` — `Observable.Timer(1초, 1초).Subscribe(_ => hp.Value -= 1)` (시간 기반 소모)
  - `Assets/Scripts/Data.cs:25` — `public int hp = 100;` (저장 필드, 기본값 100)
  - `Assets/Scripts/Data.cs:26` — `public int attack = 0;` (저장 필드, 사용 경로 없음)
  - `Assets/Scripts/Defines.cs:329-337` — `EReward { Gold, AddTime, AddMove }` (HP 항목 없음)
  - `Assets/AssetBundleResources/json/Mission.json` — 27개 미션 전체 reward 타입 0/1/2만 사용 (HP 없음)
  - `Assets/AssetBundleResources/json/Shop.json` — 12개 상품 중 HP 관련 상품 없음
  - `Assets/AssetBundleResources/json/ConstValue.json` — HP 관련 상수 없음 (AddMoveItemValue=1, AddTimeItemValue=10만 존재)

#### 유저 플로우

1. **노출 시점·트리거**
   하드 스테이지 50개 클리어(BossStage_HardStageLock = 50) 후 UIStageSelect에서 보스 탭이 해금된다. 플레이어가 보스 스테이지를 선택하면 GameScene이 로드되고 GPBossController.Init이 호출되어 저장된 loginData.hp를 현재 HP로 설정한다. 첫 진입 시 hp = 100이므로 100초의 생존 시간이 주어진다.

2. **화면 변화**
   GameScene 상단에 보스 HP 게이지(Image fillAmount)와 플레이어 HP 숫자(_hpText)가 표시된다. 보스 HP는 플레이어가 블록을 매치할수록 감소하며, 플레이어 HP는 시간이 흐를수록 자동으로 1초마다 1씩 줄어드는 텍스트로 실시간 갱신된다. 보스 HP가 50% 이하가 되는 시점에 angryBossObj 스프라이트로 전환되며 스킬 발동 알람(UIAlarm stringID=78)이 뜬다.

3. **입력 행동**
   플레이어는 블록을 드래그해 매치를 만들어 보스 HP(targetScore 기반)를 감소시킨다. 특수 폭탄 조합이나 연쇄 매치를 통해 빠르게 보스 HP를 줄이는 것이 목표다. 플레이어 자신의 HP에 영향을 미치는 입력 행동은 현재 존재하지 않는다(시간만이 HP를 소모).

4. **시스템 반응**
   보스 HP가 50% 이하가 되면 EBossSkillType에 따라 주기적으로 Wall·Creator·CatBox 블록이 보드에 생성된다. stage % 10 값에 따라 스킬 종류와 쿨타임이 달라진다(mod=0이면 세 스킬 모두, 쿨타임 10s; mod<6이면 Wall만, 쿨타임 최대 19s). 플레이어 HP가 0이 되면 즉시 EFailReason.HpOver로 UIGameEnd가 표시된다.

5. **반복·재발생 패턴**
   보스전 성공·실패 시 loginData.hp에 잔여 HP가 저장된다고 가정하면(Data.Login 저장 구조상), 다음 보스전 진입 시 이 낮아진 HP에서 시작한다. 100스테이지를 진행하는 동안 반복적으로 소모가 누적되어, 후반 보스 스테이지에 도달할 즈음 HP가 현저히 낮아질 수 있다. 현재 회복 경로가 없어 소모 일방통행 구조다.

6. **종료·해소 조건**
   플레이어 HP가 0이 되면 HpOver 패배로 종료된다. 보스의 targetScore가 0이 되면(블록 매치로 점수 달성) 클리어로 종료된다. 클리어 시 다음 보스 스테이지가 개방되지만, HP 회복 없이 낮아진 HP 상태로 다음 보스전을 맞이한다. 재시도를 위한 HP 복구 방법이 없어 패배 반복 시 아이템(AddTime, AddMove)으로 대응할 수도 없다.

7. **다른 시스템과 상호작용**
   EFailReason.HpOver → UIGameEnd 표시까지만 연결되어 있다. EReward(Gold/AddTime/AddMove)와 HP는 연결되어 있지 않다. Mission.json의 27개 미션 중 어느 것도 HP를 보상하지 않는다. Shop.json의 12개 상품 중 HP 관련 상품이 없다. DailyMissionService의 일일 리셋도 HP를 재설정하지 않는다. Data.Login.attack = 0 필드가 있으나 보스전 공격력에 활용되지 않아 성장 루프 양 축(공격/방어 모두) 미완성 상태다.

8. **엣지 케이스**
   HP = 1인 상태로 보스전에 진입하면 정확히 1초 후 HpOver가 발생하며 플레이어는 아무것도 할 수 없다. HP = 100이더라도 어려운 스테이지에서 100초 이상 소요되면 반드시 패배한다. 보스 스킬 쿨타임이 최소 10s이므로 빠른 매치로 100초 내 보스를 잡는 것이 유일한 전략이나, HP 회복 없이는 보스 스테이지 후반으로 갈수록 진입 자체가 불가능해질 수 있다. 보스전 도중 앱이 종료되면 HP가 어느 시점 값으로 저장되는지에 따라 비정상적 HP 손실이 발생할 수 있다.

9. **유저 정보·피드백**
   HP 회복 방법이 없다는 사실은 플레이어에게 명시적으로 안내되지 않는다. HP가 줄어드는 이유와 회복 방법을 찾다가 포기하는 이탈이 발생할 수 있다. 보스 스테이지 후반 진입자(장기 유저)일수록 HP가 낮아져 있을 가능성이 높으며, 이들이 이탈하면 고가 스킨·IAP 구매층을 잃는 비즈니스 손실로 이어질 수 있다. HP 회복 보상을 미션 달성이나 보스 클리어 시 제공하면 "보스를 잡을수록 더 강해진다"는 RPG적 성장 피드백이 생겨 재방문율이 높아질 것으로 기대된다.

### 보류

- **보스 스킬 Wall 100% 집중도 재설계**: GPBossController.cs 분석 결과 stage % 10 < 6인 초반 보스들은 Wall 스킬만 사용(50% 이상 스테이지). 과거 audit 58c3f65(CatBox 10%)와 같은 카테고리(보스 스킬 배분)라 보류.
- **색폭탄 5종 tapIndex 2 장기 미션 공백**: PinkBomb~BlueBomb이 tapIndex 1 반복 미션만 있고 tapIndex 2 장기 목표가 없음. 하지만 tapIndex 1 반복 미션이 이미 무한 루프 구조라 장기 목표 추가 시 상대적 우선순위 낮음.

---

## 3. 과거 감사 대비 차별성

git log 15건 검토 완료.

가장 유사했던 과거 커밋: **58c3f65** (2026-05-29 — 보스 스테이지 CatBox 스킬 극소 배분) — 보스 스테이지 관련이나 "스킬 종류 배분"의 문제였다.

**차별점**: 이번 후보는 보스전의 "스킬 배분"이 아니라 "플레이어 자원(HP)의 소모-회복 루프 설계" 문제다. GPBossController.cs의 코드 로직(`hp.Value -= 1`)과 Data.Login 저장 구조, 그리고 EReward·Mission·Shop의 HP 보상 완전 부재를 교차 확인한 새로운 관점이다. 나머지 14건 모두 미션 tapIndex 공백, 스테이지 포맷, 상점 가격, 블록 분포에 집중했으며 보스전 플레이어 HP 지속성은 미분석.

---

## 4. 다음 단계 제안

- **검증 우선**: GPGameScene.cs에서 보스전 종료 시 loginData.hp 저장 여부 확인 (저장되면 소모 누적이 실재함; 리셋되면 HP 시스템이 사실상 무의미한 타이머)
- **채택 시**: EReward에 `HpRestore = 3` 추가 → 보스 스테이지 클리어 미션(tapIndex 2, collectionType = 보스 클리어 횟수)에 reward=3으로 HP 회복 보상 연결
- **대안**: 매일 자정 리셋 시 DailyMissionService에서 hp를 100으로 복구하는 규칙 추가 (구현비용 최소)
- **Attack 활용**: Data.Login.attack = 0 필드 활용 계획도 함께 검토 (보스전 공격력 보너스로 활용 시 성장 루프 양 축 완성)

---

## 5. 쉬운 설명 (비개발자 요약)

보스 스테이지에 들어가면 플레이어의 체력이 시간이 지날수록 조금씩 줄어들어요. 체력이 다 떨어지면 게임에서 지게 됩니다. 그런데 현재 이 체력을 다시 채울 방법이 게임 어디에도 없어요. 미션을 완료해도, 상점에서 뭔가를 사도, 체력은 절대 올라가지 않습니다. 마치 스마트폰 배터리가 충전기도 없이 계속 닳아가는 것처럼, 보스 스테이지를 여러 번 하다 보면 결국 체력이 0이 되어 게임 자체를 시작조차 못 하게 될 수 있어요. 그래서 이번에 제안하는 것은: 보스를 물리칠 때마다 체력을 조금씩 회복해주는 보상을 추가하는 것입니다.
