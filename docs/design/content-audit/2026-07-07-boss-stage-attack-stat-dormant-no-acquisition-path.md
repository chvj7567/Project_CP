# Content Audit — 2026-07-07 — 보스 스테이지 `attack` 스탯 잠든 필드 — 매 턴 +0 가산, 성장 경로 전무

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (StringKorea, StringEnglish, Stage, StageBlock, Mission, Shop, Guide, Tutorial, ConstValue)
- 과거 감사 이력 (git log): 27건 (가장 최근: 2026-07-06)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39, 47~51(20개) 제외 |
| 일일 미션 종류 | 5종 (EDailyCounter 4 + CatPang 일별 collectionType) | Attendance / NormalStageClear / BlockDestroy / AdWatch / CatPang-daily |
| 상점 아이템 | 12개 (tapIndex1: 9개, tapIndex2: 3개) | RemoveAD·AddTime·AddMove IAP 포함 |
| 고양이 스킨 | 6종 (CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry) | EBlockState 54~83 |
| 보스 스탯 bonusScore | 매 턴 `loginData.attack` 가산 (GPGameScene.cs L836) | `attack` 초기값 = 0, 획득 경로 없음 |

### 분포 공백

- **`Data.Login.attack` 필드**: `public int attack = 0;` (Data.cs L26) — 초기값 0, 리셋 없음.
- **GPGameScene.cs L836**: `if (_selectStage == ESelectStage.Boss) bonusScore.Value += _loginData.attack;`
  → 보스 스테이지 매 턴마다 `attack` 값을 bonusScore에 더하지만, attack이 항상 0이므로 가산량 = 0.
- **bonusScore UI 표시 조건**: `if (bonusScore.Value > 0)` (L839) → attack 기여분이 0이면 UI 자체가 표시되지 않음.
- **비교**: `BombClearBonusScore = 20`, `BombMergeBonusScore = 30`(L33·35) — 폭탄 이벤트 시 bonusScore는 정상 표시됨.
- **상점·미션 미연결**: Shop.json 12개 항목 전부 attack 보상 없음. Mission.json EReward 열거형 Gold(0)/AddTime(1)/AddMove(2) — attack 보상 경로 없음.

### 과거 감사 후보 (git log 조회 결과, 최근 27건)

| 날짜 | 커밋 SHA | 설명 요지 |
|---|---|---|
| 2026-07-06 | 80d875f | Wall 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-07-05 | 18dd561 | tapIndex 2 장기 이정표 미션 보상 Gold 단일화 |
| 2026-07-04 | 39d8ced | EBackground 4종 완전 미참조, 배경 커스터마이징 루프 공백 |
| 2026-07-03 | 5c2ba4c | Wall·Potal 초등장 스테이지 Tutorial.json 항목 공백 |
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-28 | 5c06786 | Ball 블록 초등장 stage 131 — 후기 도입 집중 |
| 2026-06-27 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-26 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동) 밀스톤 부재 |
| 2026-06-25 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-24 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-23 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 |
| 2026-06-22 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-21 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-20 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-19 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 공백 |
| 2026-06-18 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-17 | 3670d4f | 고양이 스킨 블록(54~83) 수집 미션 완전 공백 |
| 2026-06-16 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 부재 |
| 2026-06-15 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-14 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 클리어 보너스 없음 |
| 2026-06-13 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-12 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-11 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-10 | 329cce2 | 중반-후반 blockTypeCount=5 고착, 색 복잡도 완급 미활용 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### 보스 스테이지 `attack` 잠든 공격력 스탯 — 매 턴 가산 로직 존재, 성장 경로 전무

- **카테고리**: 보스 / 성장 루프
- **요지**: `Data.Login.attack`은 보스 스테이지 매 턴 `bonusScore`에 더해지도록 설계되어 있으나 초기값이 0이고 획득 경로가 전혀 없어 사실상 비활성 상태다. 상점 또는 미션에 attack 증가 보상 경로를 추가하면 보스 스테이지 전용 성장 루프가 완성된다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 4/2/4/5 → 종합 **17**
  - 종합 = 4 + (6−2) + 4 + 5 = 17
- **근거**:
  - `Assets/Scripts/Data.cs` L26: `public int attack = 0;` — 초기값 0, 리셋 없음
  - `Assets/Scripts/Scenes/GPGameScene.cs` L836: `if (_selectStage == ESelectStage.Boss) bonusScore.Value += _loginData.attack;`
  - `Assets/Scripts/Scenes/GPGameScene.cs` L33/35: `BombClearBonusScore = 20`, `BombMergeBonusScore = 30` — 동일 bonusScore 경로에 비폭탄 보너스는 정의되어 있음
  - `Assets/AssetBundleResources/json/Shop.json`: 12개 항목 전부 attack 보상 없음
  - `Assets/Scripts/Defines.cs` EReward: Gold/AddTime/AddMove 3종만 존재 — attack 리워드 타입 없음

#### 유저 플로우

1. **노출 시점·트리거**: 플레이어가 처음으로 보스 스테이지를 진입할 때(stage 100001~). 스테이지 선택 화면에서 "Boss" 탭 진입, UIGameStart 팝업 후 게임 씬 로드 시 `_loginData.attack` 값이 게임 내 bonusScore 계산에 쓰이기 시작한다.

2. **화면 변화**: 현재 상태에서는 attack=0이므로 bonusScore UI(`bonusScoreText`)가 attack 기여로 표시되는 일이 없다. 폭탄 클리어(+20)·폭탄 합성(+30) 이벤트가 발생해야만 bonusScore 텍스트가 잠깐 나타났다 사라진다. attack이 0이 아닌 값을 갖게 되면 매 드래그마다 bonusScore 숫자가 표시되어 플레이어에게 "공격력이 점수를 올리고 있음"을 시각적으로 전달한다.

3. **입력 행동**: 플레이어가 블록을 드래그해 매치를 발생시킨다. 매 드래그 완료 후 `AfterDrag()`가 호출되고, 보스 스테이지에서는 `bonusScore.Value += _loginData.attack` 연산이 실행된다. 현재 이 연산의 결과는 항상 0이다.

4. **시스템 반응**: bonusScore가 0보다 크면 보너스 텍스트가 0.3~0.5초간 표시된 뒤 사라지며 `curScore.Value`에 합산된다. attack이 유의미한 값(예: 5)이라면 moveCount=30인 보스 스테이지에서 최대 +150 보너스 점수가 쌓이며, targetScore 1600 기준 약 9%의 점수 기여가 발생한다.

5. **반복·재발생 패턴**: 보스 스테이지는 100개이며 매 스테이지마다 이 계산이 실행된다. attack을 올릴 수 있는 경로가 생기면 플레이어는 "더 높은 attack → 더 높은 bonusScore → 보스 스테이지 클리어 용이"라는 수직 성장 루프를 경험하게 된다. 상점에서 attack 아이템을 구매할수록 누적 이득이 커진다.

6. **종료·해소 조건**: 플레이어가 보스 스테이지를 클리어하거나 실패하면 `bonusScore`는 0으로 리셋된다. `loginData.attack` 자체는 영구 저장되므로 한 번 증가시킨 attack은 이후 모든 보스 스테이지에 지속 적용된다.

7. **다른 시스템과 상호작용**: attack은 `Data.Login`에 저장되어 GPGS 클라우드 저장 경로를 탄다. `UIRank`의 점수 랭킹도 `curScore` 기반이므로, attack이 높은 플레이어가 보스 랭킹에서 유리해지는 자연스러운 경쟁 레이어가 형성된다. 상점 금화(Gold) 수급과 연결하면 Mission 보상 루프와도 시너지가 생긴다.

8. **엣지 케이스**: ① attack 값이 매우 크면(예: 1000) 매 턴 +1000이 돼 targetScore를 단 한 번의 드래그로 초과하는 밸런스 붕괴 가능성이 있다. 상한값(cap) 설정 또는 단계별 증가폭 제한이 필요하다. ② attack 보상을 미션 달성 시 부여할 경우, 기존 EReward enum에 Attack 타입을 추가해야 해 Mission.json 구조 변경이 필요하다. 상점 Gold 과금 방식이라면 코드 변경 없이 Data.Login.attack++만으로 구현 가능해 구현비용이 더 낮다. ③ 노멀/하드 스테이지에서는 attack이 bonusScore에 반영되지 않으므로 보스 스테이지 전용 스탯임을 플레이어에게 명확히 안내해야 한다.

9. **유저 정보·피드백**: 현재 플레이어는 attack 스탯의 존재를 알 방법이 없다(표시 UI 없음, 획득 경로 없음). 상점 또는 UIGameStart 팝업에 "공격력: N" 표시를 추가해야 플레이어가 성장 목표를 인지할 수 있다. 보스 클리어 시 "이번 턴 공격력 보너스: +N점" 같은 요약 피드백이 있다면 구매·미션 완료 동기가 강화된다.

### 보류

- **CatBox2-5 tapIndex 1 반복 수집 미션 전무**: 과거 2026-07-06 Wall tapIndex 1 미션 공백과 카테고리·요지 구조가 유사(블록 타입 반복 수집 미션 신설). 종합 점수 15로 1위 못함.
- **ESelect 시스템 완전 비활성(LoadSelectInfo 주석처리)**: Power/Delay/Lotto/AddCat/CatPangUpgrade/Speed 6종이 정의되어 있으나 LoadSelectInfo 호출이 주석 처리되어 있고 Select.json이 없음. 코드 아키텍처 활성화가 필요해 구현비용 5 → 종합 15. 제외 기준(코드 아키텍처 변경 필요한 신규 시스템) 경계에 해당해 보류.

## 3. 과거 감사 대비 차별성

git log 27건 검토 완료.

- 가장 유사한 과거 커밋: **ccb2a5d (2026-06-14)** — `Data.Stage.boomAllCount` 잠든 필드(BoomAll 클리어 보너스 없음). 동일한 "잠든 필드" 패턴이나, boomAllCount는 스테이지 클리어 후 통계 필드 미활용이고, 본 제안의 attack은 **게임 플레이 중 매 턴 bonusScore 가산 로직에 직접 사용**되는 수치형 스탯이라는 점에서 다르다. 파급 효과도 차이가 있다: boomAllCount는 클리어 보너스 시스템 신설이 필요한 반면, attack은 **Shop.json에 항목 1개 추가(gold 비용 + attack 증가)**만으로 즉시 활성화 가능하다.
- 2026-06-11 HP 회복 루프 제안: 보스 HP 필드 미회복이 주제. 본 제안은 별개 스탯(attack)이며 클리어 조건이 아닌 **점수 기여** 루프 설계 공백이다.
- 2026-06-20 AddTime/AddMove 보스 스테이지 무효: 아이템 효과 적용 범위의 문제. 본 제안은 스탯 성장 경로 자체가 없다는 구조적 공백이다.

## 4. 다음 단계 제안

채택 시 구체 구현 계획 수립 필요:

1. **Shop.json 항목 추가 (최소 구현)**: 새 shopID에 `gold: N, attack 증가폭: M` 형태 추가. Data.Login.attack 직접 증가. 코드 변경은 Shop 구매 처리 로직 1곳.
2. **attack 획득 상한 설계**: 보스 스테이지 targetScore 대비 최대 기여율을 10~20%로 제한 — 예: attack 최대 20, 매 턴 +20, 30턴 기준 +600 (targetScore 3000 기준 20%).
3. **UIGameStart·UIGameEnd 표시**: 보스 스테이지 시작 팝업에 "공격력: N" 표시, 클리어 결과 화면에 "공격력 보너스 합계: +N점" 추가.
4. **EReward 확장 검토**: 미션 보상으로 attack을 주려면 `EReward.Attack` 추가 + Mission.json 수정이 필요하다. 상점 Gold 구매 경로만 사용하면 Enum 변경 없이 최소 구현 가능.

## 5. 쉬운 설명 (비개발자 요약)

CatPang 보스 스테이지에는 숨겨진 "공격력" 수치가 있다. 원래 설계대로라면 블록을 옮길 때마다 이 공격력만큼 점수가 더 쌓여야 한다. 그런데 지금은 그 공격력이 항상 0이라서, 보스 스테이지에서 아무리 많이 플레이해도 이 보너스 점수는 단 1점도 더해지지 않는다. 마치 게임에 "공격력 강화 슬롯"은 있는데, 강화 재료를 살 수 있는 상점이 없는 것과 같다. 그래서 이번에 제안하는 것은: 상점에서 골드로 공격력을 올릴 수 있는 아이템을 추가해, 보스 스테이지를 많이 할수록 점수가 더 잘 나오는 성장 루프를 완성하는 것이다.
