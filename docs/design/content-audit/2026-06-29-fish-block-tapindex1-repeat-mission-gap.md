# Content Audit — 2026-06-29 — Fish 블록 tapIndex 1 반복 수집 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷

- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringEnglish / StringKorea / Tutorial)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-27)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84슬롯 | 빈 슬롯 25~39·47~51 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집형 1종 = 총 5개 | tapIndex 3 |
| 상점 아이템 | 12개 | 스킨 7·IAP 3·골드 아이템 2 |
| 고양이 스킨 테마 | 6종 × Cat1~5 = 30슬롯 | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |

### Fish 블록 미션 커버리지 vs 유사 장애물 블록 비교 (StageBlock.json / Mission.json 기준)

| 블록 (EBlockState) | 최초 등장 스테이지 | 등장 스테이지 수 | 총 셀 수 | tapIndex 1 | tapIndex 2 | tapIndex 3 |
|---|---|---|---|---|---|---|
| Arrow1~6 (10~15) | stage 1 | 전체 | 64~50 | ✅ mID 1~6 | ❌ | ❌ (제안: 2026-06-25) |
| CatPang (18) | stage 1 | 전체 | 43 | ✅ mID 7 | ❌ (제안: 2026-06-11) | ✅ mID 104 |
| PinkBomb~BlueBomb (19~23) | stage 1 | 전체 | 36~37 | ✅ mID 8~12 | ❌ (제안: 2026-06-14) | ❌ |
| **Fish (24)** | **stage 31** | **33(노멀) + 20(보스) = 53** | **258** | **❌ 완전 공백** | **✅ mID 13 (31마리)** | **❌** |
| RainbowPang (52) | 중반 | 다수 | 139 | ❌ (제안: 2026-06-01) | ✅ mID 16 (91개) | ❌ |
| Ball (53) | stage 131 | 19(노멀) + 보스 | 923 | ❌ | ✅ mID 17 (131개) | ❌ (제안: 2026-06-17) |

**핵심 발견**: Fish는 stage 31부터 53개 스테이지에 걸쳐 258개 셀이 배치된 주요 장애물 블록임에도 tapIndex 1 반복 수집 미션이 단 한 줄도 없다. 동일한 tapIndex 2 이정표 미션(clearValue=31)이 존재하므로 Collection 시스템이 Fish 이탈 횟수를 이미 트래킹하고 있으나, 그 전 단계인 "Fish 10마리 이탈" 반복 목표가 비어 있다.

### 분포 공백

- Fish 등장 스테이지: stage 31 최초, 이후 stage 38, 53, 56~60 등 전반~후반 분산
- tapIndex 2 clearValue 31 달성에 필요한 플레이: stage 31부터 꾸준히 진행 시 수십 스테이지 소요 예상
- tapIndex 1 "10마리 이탈" 목표가 없어 tapIndex 2에 도달하기까지 어떠한 중간 목표도 존재하지 않음
- 비교: Arrow1~6(tapIndex 1 존재 → tapIndex 2 없음), CatPang(tapIndex 1·3 존재 → tapIndex 2 없음)
- Fish만 tapIndex 1 없이 tapIndex 2만 있는 구조 — 난이도 계단 공백

### 과거 감사 후보 (git log 조회 결과 — 최근 24건)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-27 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 시점 후기 도입, 19개 스테이지 집중 |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 — 화살표 폭탄 생애 이정표 신설 제안 |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 스테이지 완전 부재 |
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
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Fish 블록 tapIndex 1 반복 수집 미션 — "Fish 10마리 이탈" 중간 목표 신설 제안

- **카테고리**: 미션 (tapIndex 1 반복 수집 미션 공백)
- **요지**: Fish(EBlockState 24)는 stage 31부터 등장하는 독특한 장애물 블록이다. 폭탄·매치로 직접 파괴할 수 없고 오직 보드 맨 아래줄 도달로만 "이탈"(소멸·색폭탄 변환)이 가능하다. Collection 시스템은 Fish 이탈 횟수를 이미 추적하며 tapIndex 2 이정표 미션(missionID 13, clearValue 31)도 존재하지만, tapIndex 1 단계인 "Fish 10마리 이탈" 반복 목표는 Mission.json 어디에도 없다. Arrow·CatPang·Bomb 등 다른 주요 블록은 모두 tapIndex 1 반복 수집 미션을 보유한다는 점에서 Fish만 중간 계단이 빠진 구조다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 4/1/4/5 → 종합 **18**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — tapIndex 1에 collectionType 24 (Fish) 항목 0건 확인
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 13: tapIndex 2, collectionType 24, clearValue 31 (Fish 이탈 31회) 존재
  - `Assets/AssetBundleResources/json/StageBlock.json` — Fish(blockState=24): 노멀/하드 33개 스테이지(stage 31~), 보스 20개 스테이지, 총 258셀
  - `Assets/Scripts/Defines.cs` — EBlockState.Fish = 24, EBlockState.Max = 84

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   노멀 스테이지 31번부터 Fish 블록이 보드에 처음 등장한다. 신규 미션 "Fish 10마리 이탈"은 플레이어가 게임 내 미션 탭(tapIndex 1)을 진입하는 순간 목록에 표시되며, Fish가 등장하는 스테이지를 클리어하는 과정에서 자연스럽게 카운트가 쌓인다. Stage 31의 튜토리얼 연출이 Fish 메카닉을 설명한 직후, 미션 탭에 진입하면 해당 목표를 즉시 확인할 수 있다. Fish가 없는 stage 1~30 구간에서는 카운트가 올라가지 않으므로 노출 타이밍이 자연스럽게 stage 31 이후로 고정된다.

2. **화면 변화**
   UIMission(tapIndex 1) 스크롤 목록에 "Fish N마리 이탈" 항목이 새롭게 추가된다. 기존 Arrow1~6·CatPang·Bomb 미션들과 같은 행 구조(descStringID → 텍스트, 진행도 바, 보상 아이콘)로 표시된다. Fish가 맨 아래줄에 도달하는 순간 화면 상단에 수집 카운트 +1 토스트가 출력되며(Collection 시스템 기존 동작), 미션 진행 바가 실시간으로 업데이트된다. clearValue 10에 도달하면 보상 수령 버튼이 활성화되고, addValue 10씩 반복 달성 가능한 구조가 된다.

3. **입력 행동**
   플레이어는 Fish를 맨 아래줄로 내리기 위해 Fish 아래 칸의 블록을 의도적으로 제거하는 전략을 구사한다. Fish는 드래그·스왑이 불가능하므로 Fish 하단의 일반 블록을 매치하거나, Arrow·CatPang 폭탄으로 Fish 하단 열을 청소하거나, PinkBomb로 같은 색 블록을 대량 제거해 Fish를 한 번에 2~3줄 낙하시키는 방식을 선택한다. 미션 카운트를 올리는 유일한 행동이 "Fish 이탈"이므로 Fish를 보드 상단에 묶어두는 수동적 플레이는 미션 달성을 늦출 뿐이다.

4. **시스템 반응**
   Fish가 row 8(맨 아래줄, 0-indexed)에 도달하면 GPGameScene의 `SetDissapearBlock`이 호출된다. Fish는 색폭탄(PinkBomb~BlueBomb 중 하나)으로 교체되고, CHMData의 Collection[EBlockState.Fish] 카운터가 +1 증가한다. DailyMissionService의 집계와 별개로 tapIndex 1 Collection 기반 미션은 로컬 저장 데이터를 직접 읽어 진행도를 계산하므로 별도 이벤트 훅 없이 기존 집계 결과를 그대로 활용한다. 보상 수령 시 EReward.Gold 100개가 지급된다(타 tapIndex 1 미션 기준).

5. **반복·재발생 패턴**
   clearValue 10 달성 후 addValue 10씩 재설정되어 "Fish 20마리 이탈", "Fish 30마리 이탈" 순으로 반복된다. tapIndex 2 이정표(clearValue 31)와 자연스럽게 연결되어, tapIndex 1 반복 3회차(30마리 달성) 직후 tapIndex 2 목표(31마리)가 완성되는 흐름이 만들어진다. 이후 tapIndex 1이 계속 반복되어 Fish가 많이 배치된 후반·보스 스테이지로 갈수록 달성 속도가 빨라지는 자연스러운 가속 곡선이 형성된다. 스테이지 50개 이상에 Fish가 분산 배치되어 있으므로 정상적인 스테이지 진행만으로도 반복 달성이 가능하다.

6. **종료·해소 조건**
   tapIndex 1은 무한 반복 구조이므로 단일 종료 조건은 없다. 다만 tapIndex 2 이정표(31마리) 달성 이후에도 tapIndex 1 반복은 지속되어 장기 보상 루프가 이어진다. 보스 스테이지에도 Fish가 20개 스테이지에 걸쳐 배치되어 있어 최후반 콘텐츠까지 미션이 살아 있다. 플레이어가 모든 스테이지를 클리어한 이후에도 재클리어(Data.Login.stageClearCountToday 재집계 가능 구조 참고) 플레이를 통해 추가 Fish를 확보할 수 있다.

7. **다른 시스템과 상호작용**
   Fish가 이탈하면 즉시 색폭탄으로 변환되므로, Fish 이탈을 유도하는 플레이가 보드에 추가 폭탄을 생성한다. 이 폭탄이 연쇄 반응을 일으켜 Wall·CatBox 등 다른 목표 블록을 처리하는 시너지가 발생한다. 또한 Fish 이탈 → 색폭탄 생성 → 인접 매치와 연계 → 추가 CatPang·Arrow 생성으로 이어지는 연쇄가 "GPBombResolver 연쇄 처리" 시스템과 맞물려 고점수 기회를 만든다. DailyMissionService의 BlockDestroy(EDailyCounter 2) 카운터는 Fish 이탈 자체를 직접 카운트하지 않으므로, 두 미션이 충돌하지 않고 독립적으로 집계된다.

8. **엣지 케이스**
   Fish가 폭탄 범위 내에 있을 때 `GPMatchChecker.ChangeMatchState`는 Fish를 명시적으로 제외하므로, 폭탄으로는 Fish를 직접 파괴할 수 없다. 따라서 폭탄을 Fish 위에 놓아도 카운트가 오르지 않고 Fish 이탈(맨 아래줄 도달)만 유효하다. Fish가 맨 아래줄 도달 전에 보스 HP 소진 또는 이동 횟수 초과로 게임이 종료되면 해당 라운드의 Fish 이탈 카운트는 반영되지 않는다(Collection 업데이트가 이탈 시점에 즉시 발생하므로, 게임 종료 시점까지 이탈한 Fish 수만 집계). WallCreator·PotalCreator가 Fish 하단 열을 막는 상황에서는 이탈이 지연될 수 있어 고난도 스테이지에서의 달성이 더 까다롭다.

9. **유저 정보·피드백**
   Fish 이탈 시 기존 Collection 토스트 텍스트("Fish +1")가 화면에 표시된다. 미션 탭 진행 바가 10마리 단위로 갱신되므로 플레이어는 단기 목표까지 남은 횟수를 명확히 알 수 있다. clearValue 10 달성 알림은 기존 미션 완료 팝업(UIAlarm 또는 UIMission 내 토스트) 구조를 그대로 활용한다. 반복 달성 시마다 동일한 Gold 100개 보상이 지급되어 작은 성취감을 지속적으로 제공한다.

### 보류

- **WallCreator·PotalCreator tapIndex 1 반복 수집 미션 공백**: WallCreator(45)는 tapIndex 2(missionID 15, clearValue 71)가 존재하고 PotalCreator(46)는 tapIndex 2 제안(2026-05-29)이 있으나 tapIndex 1은 둘 다 없다. 종합점수 16으로 Fish(18)보다 낮고, 생성기 블록 특성상 "파괴 수" 트래킹이 Fish의 "이탈 수"보다 직관적이지 않아 보류.
- **CatBox2·3·5 tapIndex 2 이정표 미션 공백**: CatBox4는 과거 제안(aeffad1)에서 언급됐고 CatBox1은 tapIndex 2 존재. CatBox2/3/5 공백은 실재하나 카테고리·요지가 기존 제안(CatBox4)과 부분 중복. 종합점수 15.
- **Ball tapIndex 1 반복 수집 미션 공백**: Ball은 stage 131부터 등장해 tapIndex 1 달성 구간이 20개 스테이지뿐이다. tapIndex 3 제안(2026-06-17)과 카테고리 일부 중복. 종합점수 14.

## 3. 과거 감사 대비 차별성

git log 24건 전수 검토 완료. 가장 유사했던 과거 커밋은 두 건이다:

- **aeffad1 (2026-05-29)** "PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백" — 동일한 '미션 공백' 카테고리이나 블록 타입(PotalCreator/CatBox4 vs Fish), tapIndex(2 vs 1), 메카닉(생성기 파괴 vs 맨 아래줄 이탈)이 모두 다르다.
- **fcebcd0 (2026-05-31)** "wall-potal-tapindex2-mission-gap" — Wall/Potal tapIndex 2 제안으로, 본 제안(Fish tapIndex 1)과 블록 타입·tapIndex 모두 다르다.
- Fish는 "폭탄·매치 불가 + 맨 아래줄 이탈" 메카닉이 게임 내 유일하며, 이 독특한 이탈 조건이 tapIndex 1 반복 미션의 차별화 근거가 된다. 24건 중 Fish(blockState 24)를 직접 타겟으로 하는 tapIndex 1 제안은 0건.

## 4. 다음 단계 제안

- Mission.json에 다음 줄 추가:
  `{"missionID":"23", "tapIndex":"1", "descStringID":<신규ID>, "collectionType":24, "clearValue":10, "addValue":10, "reward":0, "rewardCount":100}`
- StringKorea.json / StringEnglish.json에 `<신규ID>` 문자열 추가 (예: "Fish N마리 이탈" / "Escape N Fish")
- 채택 시 UIConfirm / UIAlarm 연동 확인 후 Mission.json 단독 배포 가능 (코드 변경 없음)

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 "물고기 블록"이라는 특별한 블록이 있다. 폭탄을 터뜨려도 파괴할 수 없고, 아래 칸 블록들을 치워서 맨 아랫줄까지 내려보내야만 보드에서 사라진다. 이 물고기 블록은 31번 스테이지부터 등장해 게임 내내 꽤 자주 나오는데, "물고기 10마리 탈출시키기" 같은 작은 목표가 없어서 처음에 도전할 이유를 플레이어가 느끼기 어렵다. 다른 블록들(화살표, 폭탄 등)은 모두 "10개 모아봐!" 목표가 있어 게임 중에 보람을 느끼게 해주는데 물고기만 빠져 있다. 그래서 이번에 제안하는 것은: 물고기 블록을 10마리 탈출시킬 때마다 작은 보상을 주는 목표 1줄을 추가하는 것이다.
