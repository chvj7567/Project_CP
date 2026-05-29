# Content Audit — 2026-05-29 — PotalCreator·CatBox2~5 tapIndex 2 장기 수집 미션 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 3건 (가장 최근: 2026-05-29, SHA b5bcc97)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 / 전체 84 | Cat1~5·Arrow1~6·Wall·Portal·CatPang·5색폭탄·Fish·CatBox1~5·WallCreator·PotalCreator·RainbowPang·Ball만 실제 배치됨 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1건 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7 + 골드 2), tapIndex 2: 3개(IAP) |
| 고양이 스킨 | 6종 | Crown / Flowers / Mushroom / Party / Santa / Strawberry × Cat1~5 = 30 슬롯 |

### 분포 공백

**Mission.json tapIndex 2 (장기 1회 달성) 미션 vs StageBlock 출현 수 비교:**

| EBlockState (int) | 블록명 | StageBlock 출현 수 | tapIndex 2 미션 | 비고 |
|---|---|---|---|---|
| Fish (24) | 물고기 | 258 | ✅ missionID=13, clearValue=31 | |
| CatBox1 (40) | 고양이 상자 1단계 | 75 | ✅ missionID=14, clearValue=51 | |
| **CatBox2 (41)** | **고양이 상자 2단계** | **57** | **❌ 없음** | |
| **CatBox3 (42)** | **고양이 상자 3단계** | **57** | **❌ 없음** | |
| **CatBox4 (43)** | **고양이 상자 4단계** | **152** | **❌ 없음** | CatBox1보다 2배 출현, 미션 부재 |
| **CatBox5 (44)** | **고양이 상자 5단계** | **67** | **❌ 없음** | |
| WallCreator (45) | 벽 생성기 | 162 | ✅ missionID=15, clearValue=71 | |
| **PotalCreator (46)** | **포탈 생성기** | **200** | **❌ 없음** | WallCreator(162)보다 많은데 미션 부재 |
| RainbowPang (52) | 무지개 팡 | 139 | ✅ missionID=16, clearValue=91 | |
| Ball (53) | 공 | 923 | ✅ missionID=17, clearValue=131 | |

**핵심 발견 — 대칭 파괴와 규모 역설**:

1. **WallCreator(45) ↔ PotalCreator(46) 불균형**: WallCreator는 tapIndex 2 미션(missionID=15)이 있는데, 대응 쌍인 PotalCreator는 없다. 더욱이 PotalCreator는 스테이지 내 출현 횟수가 200으로 WallCreator(162)보다 많다 — 더 자주 등장하는 블록이 오히려 미션 트래킹 대상에서 빠진 역설.

2. **CatBox1만 있고 CatBox2~5는 없음**: CatBox는 HP가 1→5단계로 증가하는 계단식 블록이다. CatBox1(40, 75회)에는 미션이 있지만, 더 높은 단계인 CatBox4(43, 152회)는 CatBox1보다 두 배 많이 등장함에도 미션 트래킹이 전혀 없다. 플레이어 입장에서 "어려운 상자를 깼는데 보상 루프가 없다"는 단절감이 발생한다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### tapIndex 2 장기 수집 미션에 PotalCreator(46) + CatBox4(43) 추가 (우선 2종)

- **카테고리**: 미션
- **요지**: Mission.json의 tapIndex 2 장기 1회 달성 미션에 WallCreator(45)의 대칭 쌍인 PotalCreator(46, 출현 200회)와 CatBox 계열 최고 출현 단계인 CatBox4(43, 출현 152회)에 대한 항목이 없다. 기존 tapIndex 2 미션의 clearValue 패턴(31→51→71→91→131)에 맞춰 PotalCreator(clearValue=111)와 CatBox4(clearValue=61)를 추가하면 코드 변경 없이 Mission.json 2행만으로 중후반 플레이어의 장기 목표를 보완할 수 있다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 4/1/4/5 → 종합 **18**
  - 검증가치 4: WallCreator-PotalCreator 대칭 파괴와 CatBox4 출현 역설은 기존 미션 설계의 맹점을 직접 드러내므로, 추가 시 중후반 미션 완료율이 올라가는지 A/B 검증 가치가 높다.
  - 구현비용 1: Mission.json에 2행 추가 + StringKorea/StringEnglish에 각 2개 descStringID 등록. 코드 변경 전혀 없음. Data.Collection 구조는 이미 해당 EBlockState 키를 수집하고 있어 신규 미션이 즉시 동작한다.
  - 플레이어경험개선 4: 중후반(그룹 7 이상) 스테이지에서 포탈 생성기와 4단계 상자를 반복 소파하면서도 아무 장기 목표가 없던 플레이어에게 구체적 진척 지표가 생긴다.
  - 데이터근거 5: `Assets/AssetBundleResources/json/Mission.json` 전체 23건 직접 조회 — collectionType 43·46 해당 항목 없음 확인. `Assets/AssetBundleResources/json/StageBlock.json` blockState별 집계 — CatBox4=152, PotalCreator=200 직접 확인.
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json`: missionID 13~17 (tapIndex 2) 5건 조회. collectionType 40(CatBox1) 있음, 41~44 없음. collectionType 45(WallCreator) 있음, 46(PotalCreator) 없음.
  - `Assets/AssetBundleResources/json/StageBlock.json`: blockState 기준 집계 — CatBox1(40)=75, CatBox2(41)=57, CatBox3(42)=57, CatBox4(43)=152, CatBox5(44)=67, WallCreator(45)=162, PotalCreator(46)=200.
  - `Assets/Scripts/Data.cs`: Data.Collection은 key=(int)EBlockState, value=누적수 구조. CatBox4(43)와 PotalCreator(46) 수집량이 이미 저장되고 있으나, Mission.json 미등록으로 보상 루프 단절.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   플레이어가 스테이지를 클리어하면 매치 중 파괴한 블록 수가 Data.Collection에 누적된다. 그룹 7 이상 스테이지에서 포탈 생성기(PotalCreator)와 4단계 고양이 상자(CatBox4)가 자주 등장하므로, 클리어 후 미션 탭(UIMission)에서 tapIndex 2 탭을 열면 새로 추가된 "포탈 생성기 N개 파괴" 및 "4단계 고양이 상자 N개 파괴" 항목이 이미 수십 % 달성된 상태로 보인다.

2. **화면 변화**
   UIMission tapIndex 2 탭에 기존 5개 항목(Fish·CatBox1·WallCreator·RainbowPang·Ball) 아래 "포탈 생성기 111개 파괴" 및 "4단계 고양이 상자 61개 파괴" 항목이 추가된다. 진행률 바가 현재 Data.Collection 저장값으로 즉시 채워지므로, 첫 진입 시 "이미 진행 중" 감각을 준다.

3. **입력 행동**
   플레이어는 별도 행동 없이 기존처럼 스테이지를 진행한다. 포탈 생성기가 있는 스테이지(주로 그룹 6~15, 보스)에서 매치를 통해 포탈 생성기 블록을 파괴하면 자동 누적된다. 미션 탭에서 항목을 눌러 현재 진척도를 확인하는 것이 유일한 추가 입력이다.

4. **시스템 반응**
   스테이지 클리어 시 GPGameScene이 파괴된 블록 수를 DailyMissionService(또는 Data.Collection 갱신 경로)에 전달한다. Mission 시스템이 clearValue(PotalCreator=111, CatBox4=61)에 도달했는지 체크하고, 달성 시 보상(Gold 1000)을 지급하며 해당 Mission의 clearState를 EClearState.Clear로 변경한다.

5. **반복·재발생 패턴**
   tapIndex 2 미션은 addValue=-1로 1회 달성 후 재발하지 않는다. 달성 후 해당 항목에 "완료" 표시가 붙으며, 동일 collectionType에 대한 추가 보상은 없다. 반복 요소는 없으므로 플레이어가 한 번 보상을 받은 뒤 해당 항목은 트로피처럼 기록으로 남는다.

6. **종료·해소 조건**
   PotalCreator 누적 파괴 수가 111 이상이 되는 순간(또는 이미 초과 상태로 미션이 활성화되는 순간) 미션이 즉시 달성 상태로 전환된다. 기존에 이미 111개 이상 파괴한 고인물 플레이어는 미션 탭 첫 진입 시 바로 보상을 수령할 수 있다.

7. **다른 시스템과 상호작용**
   Data.Collection이 이미 EBlockState 기반으로 누적값을 저장하고 있으므로 PotalCreator(46)와 CatBox4(43)에 대한 수집 데이터는 신규 코드 없이 즉시 조회 가능하다. 구글 클라우드 저장(GPGS)을 통해 진척도가 디바이스 간 동기화되므로 재설치 후에도 달성 이력이 유지된다. 미션 완료 보상(Gold 1000)은 상점(UIShop) 아이템 구매 통화로 연결된다.

8. **엣지 케이스**
   - 이미 clearValue 초과인 플레이어: Mission.json 활성화 시 즉시 달성 처리. 게임 최초 시작일 전 데이터 없음 시 startValue=0으로 정상 동작.
   - PotalCreator 없는 스테이지(그룹 1~5): 해당 스테이지에선 진척 불가이므로 초반 플레이어에게는 미션이 보이되 막막하게 느껴질 수 있음. clearValue 111은 중후반 도달 분량이므로 초반 단계에선 낮은 진척률이 자연스럽다.
   - CatBox4가 포함된 스테이지(그룹 7~15 이상)에서 연쇄 폭발로 CatBox4가 자동 파괴되는 경우: 파괴 출처 무관하게 blockState=43 파괴가 집계되므로 정상 카운트.

9. **유저 정보·피드백**
   플레이어는 미션 설명 텍스트(descStringID 신규 등록 필요)를 통해 "포탈 생성기 111개 파괴 → 골드 1000 보상"임을 인지한다. 달성 직전(~10개 남음)에 진척 바가 거의 가득 찬 상태를 보면서 목표 의식이 생기고, 포탈 생성기가 있는 스테이지를 의식적으로 선택하는 행동 변화가 유도될 수 있다. WallCreator 미션과 PotalCreator 미션을 나란히 보면서 "쌍으로 완료"하는 만족감도 기대할 수 있다.

### 보류 후보

| 후보 | 카테고리 | 점수 | 보류 이유 |
|---|---|---|---|
| Cat6/Cat7 blockTypeCount 확장 (그룹 13~15에 blockTypeCount=6 도입) | 블록/스테이지 | 16 | StageBlock.json 스테이지 재설계 필요 — 구현비용 3으로 점수 낮음 |
| Arrow4(13)·Arrow2(11) 방향별 배치 불균형 개선 | 블록 | 15 | Arrow1 편중은 방향 다양성 문제이나, 스테이지 재배치 비용 대비 체감 차이 불분명 |
| 5색 폭탄 GreenBomb/YellowBomb 스테이지 배치 확대 | 블록 | 14 | 출현 수 7~7로 매우 적지만, 폭탄 블록 배치 밸런스는 별도 폭탄 시스템 검토 필요 |

## 3. 과거 감사 대비 차별성

git log 3건 검토 완료.

- **a6cb70b** (스테이지 후반 시간제한 모드 공백): 카테고리=스테이지, 요지=그룹 13·15 시간제한 추가. 본 제안과 카테고리·요지·근거 모두 다름.
- **08f1ddb** (하드 스테이지 100개 포맷 다양화): 카테고리=스테이지, 요지=boardSize/time/moveCount 균일 문제. 본 제안과 카테고리·요지·근거 모두 다름.
- **b5bcc97** (tapIndex 1 Cat1~5 수집 미션 공백): 카테고리=미션으로 **카테고리 일부 겹침**. 차별점:
  - b5bcc97: tapIndex 1 (반복 달성, addValue=10) → 초반 플레이어의 즉각 보상 루프 부재
  - 본 제안: tapIndex 2 (1회 달성, addValue=-1) → 중후반 플레이어의 장기 목표 부재
  - b5bcc97 대상 블록: Cat1~5 (기본 매치 블록, 모든 스테이지 공통)
  - 본 제안 대상 블록: PotalCreator(46) · CatBox4(43) (중후반 특수 장애물 블록)
  - 핵심 논거의 차별점: 본 제안은 "WallCreator-PotalCreator 대칭 파괴"와 "CatBox1-CatBox4 출현 역전 역설"이라는 기존 데이터 내 내부 모순을 근거로 삼는다. b5bcc97은 "Cat 블록이 미션 전무"라는 절대적 공백 근거였다.

## 4. 다음 단계 제안

채택 시 다음 작업이 필요하다:

1. **descStringID 신규 등록**: StringKorea.json / StringEnglish.json에 "포탈 생성기 N개 파괴" · "4단계 고양이 상자 N개 파괴" 문자열 추가 (각 2개).
2. **Mission.json 추가**: missionID 105(CatBox4) · 106(PotalCreator), tapIndex=2, collectionType=43/46, clearValue=61/111, addValue=-1, reward=Gold, rewardCount=1000.
3. **검증 포인트**: 추가 후 중후반(그룹 7 이상) 도달 플레이어의 tapIndex 2 미션 탭 방문 빈도와 달성률 추적.
4. **선택적 확장**: CatBox2(41)·CatBox3(42)·CatBox5(44)도 같은 패턴으로 추가 가능 — 단, 우선순위는 CatBox4+PotalCreator이며 나머지는 검증 후 결정 권장.

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 스테이지에서 부수면 기록되는 "수집 미션" 탭이 있다. 지금은 "물고기 31개 부수기" "공 131개 부수기" 같은 목표가 있는데, 스테이지에서 200번이나 등장하는 "포탈 생성기"는 이 목록에 아예 없다. 마치 게임에서 가장 흔한 빨간 몬스터가 있는데, 포켓몬 도감에 그 몬스터 칸만 빠진 것과 같다. 게다가 같은 짝인 "벽 생성기"는 목록에 있는데 "포탈 생성기"만 빠져 있어서 불균형하다. 그래서 이번에 제안하는 것은: "포탈 생성기 111개 부수기"와 "4단계 고양이 상자 61개 부수기" 미션 2개를 추가해, 중반 이후 플레이어에게 자연스럽게 달성되는 장기 목표와 골드 보상을 주는 것이다.
