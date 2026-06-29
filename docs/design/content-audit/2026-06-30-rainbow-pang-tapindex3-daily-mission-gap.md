# Content Audit — 2026-06-30 — RainbowPang tapIndex 3 일일 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringEnglish / StringKorea / Tutorial)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-29)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84슬롯 | 빈 슬롯 25~39·47~51 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집형 1종 = 총 5개 | tapIndex 3 |
| 상점 아이템 | 12개 | 스킨 7·IAP 3·골드 아이템 2 |
| 고양이 스킨 테마 | 6종 × Cat1~5 = 30슬롯 | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |

### RainbowPang 미션 커버리지 vs 유사 특수 블록 비교 (Mission.json 기준)

| 블록 (EBlockState) | 등장 셀 수 | tapIndex 1 | tapIndex 2 | tapIndex 3 |
|---|---|---|---|---|
| CatPang (18) | 43셀 | ✅ mID 7 | ❌ (제안: 2026-06-11) | ✅ mID 104 (3개/일) |
| Fish (24) | 258셀 | ❌ (제안: 2026-06-29) | ✅ mID 13 (31개) | ❌ |
| **RainbowPang (52)** | **139셀** | **❌ (제안: 2026-06-01)** | **✅ mID 16 (91개)** | **❌ 완전 공백** |
| Ball (53) | 923셀 | ❌ | ✅ mID 17 (131개) | ❌ (제안: 2026-06-17) |

**핵심 발견**: RainbowPang은 tapIndex 2 이정표(mID 16, 생애 91회 발동)가 이미 존재하고 Collection 시스템이 RainbowPang 수집 횟수를 추적하고 있다. CatPang과 동일한 collectionType 기반 일일 미션 인프라(mID 104, dailyCollectionSnapshotJson)가 이미 구현되어 있음에도 tapIndex 3(일일) 항목이 Mission.json에 단 한 줄도 없다. CatPang과 RainbowPang은 모두 특수 생성 블록이지만 일일 목표 대상은 CatPang뿐이어서 RainbowPang이 등장하는 스테이지의 일일 플레이 동기가 약하다.

### 분포 공백

- 현재 tapIndex 3 구성:
  - EDailyCounter 기반 4종: 출석(0), 노멀클리어(1), 블록파괴(2), 광고시청(3)
  - collectionType 기반 1종: CatPang(18) 3개/일 — missionID 104 (rewardCount=200 Gold)
- RainbowPang(52)·Fish(24)·Ball(53)·Arrow1~6(10~15)·5색폭탄(19~23) 등 주요 특수 블록은 tapIndex 3 대상에서 전면 누락
- RainbowPang은 tapIndex 2(clearValue=91) 데이터까지 갖춰져 있어 tapIndex 3 추가에 필요한 Collection 추적 인프라가 이미 존재

### 과거 감사 후보 (git log 조회 결과 — 최근 24건)

| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-27 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 시점 후기 도입, 19개 스테이지 집중 |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 — 화살표 폭탄 생애 이정표 신설 제안 |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 스테이지 완전 부재 |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 — EDailyCounter HardStageClear·BossStageClear 없음 |
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 — 보스 스테이지 보상 루프 신설 제안 |
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 — 난이도 역전 stage 137 이동 100회·목표 900점 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 — normalStage/hardStage/bossStage 필드 미션 미연결 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 — 기본 블록 생애 1회 달성 이정표 신설 제안 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 — 보스 전용 아이템 효과 설계 부재 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 — Mission.json 1줄 추가로 신설 제안 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 — 보스 스테이지 조기 진입 허용 설계 재검토 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 — 스킨 장착 보상 루프 신설 제안 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 — PinkBomb~BlueBomb 50개 이정표 신설 제안 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절 제안 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 — 게임 동명 블록 장기 목표 신설 제안 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 — IAP 구매자 보상 루프 단절 제안 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 제안 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 — tapIndex 2 스킬 선택 미션 신설 제안 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 — Cat1~5 구현 후 2종만 잔여 공백 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### RainbowPang tapIndex 3 일일 미션 신설 — "오늘 RainbowPang 2번 발동" 보스·중반 플레이 유인

- **카테고리**: 미션 (tapIndex 3 일일 미션 공백)
- **요지**: RainbowPang(EBlockState 52)은 보드에 색폭탄을 랜덤 살포하는 특수 블록으로, tapIndex 2 이정표(mID 16, 생애 91회 발동)가 이미 존재하고 Collection 시스템이 발동 횟수를 추적하고 있다. dailyCollectionSnapshotJson 메커니즘(mID 104의 CatPang 일일 미션과 동일 인프라)이 이미 구현되어 있어 코드 변경 없이 Mission.json 1줄 추가로 tapIndex 3을 신설할 수 있다. "오늘 RainbowPang 2번 발동" 일일 목표를 추가하면 RainbowPang이 등장하는 중반~후반 및 보스 스테이지의 매일 플레이 동기가 강해지며, 현재 CatPang에만 집중된 tapIndex 3 수집형 미션의 다양성이 확대된다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 4/2/4/5 → 종합 **17**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — tapIndex 3에 collectionType 52 항목 0건 확인
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 104: `{"tapIndex":"3","collectionType":18,"dailyCounter":-1,"clearValue":3,"rewardCount":200}` → 동일 패턴 존재
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 16: tapIndex 2, collectionType=52, clearValue=91 → Collection[52] 추적 인프라 존재
  - `Assets/Scripts/Defines.cs` — EBlockState.RainbowPang = 52, EDailyCounter.None = -1
  - `Assets/Scripts/Data.cs` — Data.Login.dailyCollectionSnapshotJson: Collection 기반 일일 미션 스냅샷 필드 이미 존재

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   RainbowPang 블록이 처음 등장하는 중반 스테이지 구간에 진입한 플레이어가 UIMission(tapIndex 3, 일일 탭)을 열면 "RainbowPang 2번 발동" 항목이 목록에 새롭게 표시된다. 이 목표는 자정(NTP 기준, CHMTime.UtcNow)에 자동 리셋되어 매일 초기화된 상태로 노출된다. RainbowPang이 배치되지 않는 초반 스테이지(stage 1~30 구간)를 플레이 중인 플레이어에게는 미션 진행도가 0/2로 표시되어 달성을 위해 더 앞선 스테이지가 필요하다는 신호를 자연스럽게 제공한다.

2. **화면 변화**
   UIMission 일일 탭 스크롤 목록에 기존 5개 미션 아래 또는 CatPang 미션(mID 104) 인접 위치에 "RainbowPang 2번" 항목이 추가된다. 진행도 바(0/2), 블록 아이콘(RainbowPang 스프라이트), 보상 아이콘(Gold 200)이 기존 mID 104 행과 동일한 레이아웃으로 렌더링된다. 게임 중 RainbowPang 블록이 발동되는 순간 Collection[52] 카운터가 증가하고 자정 스냅샷과의 델타가 일일 진행도에 실시간 반영되어 진행 바가 0→1→2로 갱신된다.

3. **입력 행동**
   플레이어는 RainbowPang 블록이 배치된 스테이지에 입장하여 RainbowPang을 매치 범위에 포함시키거나 폭탄(Arrow·CatPang 등) 범위 안에 넣어 발동한다. RainbowPang 발동 시 보드에 색폭탄이 랜덤 살포되므로 색폭탄이 유리한 위치에 뿌려지는 타이밍을 노려 발동하려는 전략적 판단이 발생한다. 일일 목표(clearValue=2)는 RainbowPang이 포함된 스테이지를 1~2회 정상 플레이하는 과정에서 자연스럽게 달성 가능한 수준으로 설계된다.

4. **시스템 반응**
   RainbowPang 블록이 발동되면 GPBombResolver.RainbowPang()이 보드에 색폭탄을 살포하고, CHMData의 Collection[EBlockState.RainbowPang](키=52) 카운터가 +1 증가한다. DailyMissionService는 자정 초기화 시 dailyCollectionSnapshotJson에 `{"52": <현재 누적값>}` 스냅샷을 저장한다. 일일 진행도는 Collection[52] 현재값 - 스냅샷[52] 델타로 계산되어 mID 104와 동일한 집계 로직을 코드 변경 없이 재활용한다. clearValue=2 달성 시 UIAlarm·UIMission 완료 알림과 함께 EReward.Gold 200개가 지급된다.

5. **반복·재발생 패턴**
   addValue=0이므로 일일 목표는 매일 RainbowPang 2회로 고정된다(tapIndex 1처럼 누적 상승하지 않음). 매일 자정 리셋 후 다음 날 동일한 "RainbowPang 2번" 목표가 부활한다. RainbowPang이 등장하는 스테이지를 포함한 중반 이후 콘텐츠를 꾸준히 플레이하면 매일 달성 가능한 난이도이므로 일일 접속을 자연스럽게 유도한다. Collection[52] 누적은 tapIndex 2 이정표(mID 16, clearValue=91)와 공유되어 일일 달성이 장기 이정표 진행도를 동시에 앞당기는 이중 보상 구조가 된다.

6. **종료·해소 조건**
   당일 RainbowPang 2회 발동 달성 및 보상 수령 탭으로 해당 일의 미션이 완료된다. 자정이 되면 DailyMissionService.CheckAndResetIfNeeded()가 스냅샷을 갱신하고 진행도가 0으로 초기화되어 다음 날 다시 달성 가능해진다. NTP 미수신 상태(CHMTime.IsAvailable = false)에서는 DailyMissionService가 리셋을 보류하여 진행도가 유지됨으로써 시간 위변조를 통한 중복 달성이 방지된다. 달성 미완료 상태로 자정을 넘기면 진행도가 리셋되어 해당 날의 달성은 소멸한다.

7. **다른 시스템과 상호작용**
   RainbowPang 발동 시 색폭탄(PinkBomb~BlueBomb)이 보드에 살포되므로, 일일 목표 달성 과정에서 부가적인 폭탄 연쇄가 유발된다. 이 연쇄로 인해 BlockDestroy 카운터(dailyCounter=2)가 동시에 누적되어 일일 미션 mID 102("블록 100개 파괴")와 시너지가 발생한다. RainbowPang이 다수 배치된 보스 스테이지에서는 GPBossController의 EBossSkillType.Creator·Wall 스킬과 교차하는 상황에서 RainbowPang을 전략적으로 발동하면 CatBox·Wall 같은 목표 블록도 함께 정리할 수 있다. 한편 Collection[52] 증가는 tapIndex 2 이정표(clearValue=91)와 공유 집계되어 단일 발동이 두 미션 진행도를 동시에 올린다.

8. **엣지 케이스**
   RainbowPang이 배치되지 않은 초반 스테이지에서는 일일 목표 달성이 불가하며, 플레이어는 스테이지 선택 화면에서 RainbowPang 블록이 있는 스테이지를 직접 탐색해야 하는 마찰이 발생한다. CLAUDE.md에서 "RainbowPang은 무조건 목표 제외(가드 continue)"로 명시된 것은 스테이지 클리어 목표 집계(GPGameScene checkHp)에서의 제외를 의미하며, Collection 시스템의 카운팅과는 별개이므로 일일 미션 집계에는 영향이 없다. 게임 중 앱이 강제 종료되면 마지막 CHMData.SaveData() 호출 시점까지의 Collection 값만 반영되어 미저장 발동이 누락될 수 있다. 보스 스테이지에서 GPBossController의 RainbowPang 스킬로 생성된 블록이 즉시 발동되는 경우에도 동일하게 Collection[52]+1이 증가하므로 일반 스테이지와 구분 없이 집계된다.

9. **유저 정보·피드백**
   RainbowPang 발동 시 기존 Collection 토스트("RainbowPang +1")가 화면에 출력되고 UIMission 일일 탭 진행 바가 0→1→2로 실시간 갱신된다. clearValue=2의 낮은 목표치는 "오늘 한 판만 더 하면 달성" 심리를 유도해 플레이 지속 시간을 자연스럽게 연장한다. 보상 200 Gold는 CatPang 일일(100 Gold)보다 2배 높아 RainbowPang의 상대적 희소성을 반영하며, 달성 시 "잘했다"는 피드백을 강화한다. 미달성 상태에서 자정을 넘기면 리셋 알림 없이 조용히 초기화되므로 플레이어는 다음 날 다시 도전 기회를 얻는다.

### 보류

- **Arrow bomb 조합(ArrowBomb+ArrowBomb → 색폭탄 업그레이드) 일일 미션**: GPGameScene.AfterDrag 조합 이벤트에 카운터 연결이 전무하고 새 EDailyCounter 추가 및 hook 구현이 필요하다. 검증가치 4, 구현비용 3, 플레이어경험 5, 데이터근거 4 → 종합 16. 구현비용이 더 높고 RainbowPang daily(17)보다 종합점수가 낮아 보류.
- **WallCreator·PotalCreator tapIndex 1 반복 수집 미션**: WallCreator(45)는 tapIndex 2(mID 15) 존재, PotalCreator(46)는 tapIndex 2 제안(2026-05-29 58c3f65) 있으나 두 블록 모두 tapIndex 1이 없다. 검증가치 3, 구현비용 2, 플레이어경험 3, 데이터근거 4 → 종합 14. 생성기 블록 파괴 카운터 직관성이 Fish 이탈보다 낮고 종합점수 차이가 커 보류.

## 3. 과거 감사 대비 차별성

git log 24건 전수 검토 완료.

- 가장 유사한 과거 커밋: **2026-06-01 "Rainbow Pang tapIndex 1 반복 수집 미션 완전 공백"** (파일: `docs/design/content-audit/2026-06-01-rainbow-pang-tapindex1-repeat-mission-gap.md`) — 동일하게 RainbowPang(52)을 대상으로 하지만, 해당 제안은 **tapIndex 1 반복 수집 미션** 공백(장기 반복 누적: "RainbowPang N개 모아 반복 보상")을 다뤘다. 오늘 제안은 **tapIndex 3 일일 미션** 공백(단기 일일 루프: "오늘 RainbowPang 2번 발동")으로 타겟 계층이 다르다. 심리적 보상 주기(주 단위 반복 vs 일일 재설정), 구현 방식(Collection 기반 반복 addValue vs 스냅샷 델타 기반 일일 리셋), 플레이어 행동 유인(장기 수집 vs 매일 접속) 모두 상이하다.
- 카테고리(RainbowPang 미션)가 동일하나 **요지(반복 수집 vs 일일 목표)와 근거(tapIndex 1 공백 vs tapIndex 3 공백)가 다르므로** 중복 회피 기준("카테고리·요지·근거가 모두 겹칠 때")을 충족하지 않는다. 24건 중 RainbowPang tapIndex 3을 직접 타겟으로 한 제안은 0건.

## 4. 다음 단계 제안

- Mission.json에 다음 줄 추가:
  ```json
  {"missionID":"105", "tapIndex":"3", "descStringID":<신규ID>, "collectionType":52, "dailyCounter":-1, "clearValue":2, "addValue":0, "reward":0, "rewardCount":200}
  ```
- StringKorea.json / StringEnglish.json에 `<신규ID>` 문자열 추가 (예: "무지개팡 2번 발동" / "Activate RainbowPang 2 times")
- DailyMissionService.CheckAndResetIfNeeded() 내에서 dailyCollectionSnapshotJson에 key=52 항목이 포함되는지 확인 — mID 104(CatPang, key=18)와 동일 로직이면 코드 변경 없이 JSON 1줄 추가만으로 동작 가능
- 채택 시 RainbowPang이 배치된 스테이지 목록을 StageBlock.json에서 확인하여 일일 2회 달성이 가능한 스테이지가 충분한지(10개 이상 스테이지 권장) 사전 검증 권장

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 "무지개팡"이라는 특별한 블록이 있는데, 이 블록이 터지면 보드 전체에 색깔 폭탄이 팍팍 뿌려지는 화려한 연출이 나온다. 비슷하게 특별한 "캣팡" 블록은 이미 "오늘 하루 3번 만들어봐!" 같은 매일 달성 목표가 있어서 하루하루 작은 성취감을 주는데, 무지개팡은 "평생 91번 터뜨리기"라는 아주 먼 목표만 있고 "오늘 2번 터뜨리기" 같은 가까운 하루짜리 목표가 없다. 이 때문에 무지개팡이 나오는 중반~후반 스테이지를 오늘 꼭 해야 할 이유가 하나 부족하다. 그래서 이번에 제안하는 것은: 무지개팡을 오늘 딱 2번만 터뜨리면 금화 200개를 주는 일일 목표 1줄을 추가하는 것이다.
