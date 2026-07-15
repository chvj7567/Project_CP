# Content Audit — 2026-07-10 — WallCreator tapIndex 1 반복 수집 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StringEnglish, StageBlock, StringKorea, Tutorial)
- 과거 감사 이력 (git log): 44건 (가장 최근: 2026-07-09, PotalCreator tapIndex 2 이정표 공백)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 약 30종 / 전체 84 | 빈 슬롯 25~39, 47~51 제외 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance(0) / NormalStageClear(1) / BlockDestroy(2) / AdWatch(3) |
| 상점 아이템 | 12개 | 스킨 7개(skinIndex 0~6), RemoveAD, AddTime, AddMove, 기타 2개 |
| 고양이 스킨 테마 | 6종 (CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry) × Cat1~5 = 30 블록 | EBlockState 54~83 |
| WallCreator 배치 | 162셀 / 45 스테이지 | 하드 71~150 (25스테이지·132셀), 보스 100041~100070 (20스테이지·30셀) |

### 분포 공백 — Mission.json tapIndex 별 WallCreator 항목

| tapIndex | WallCreator(45) 항목 | 비고 |
|---|---|---|
| tapIndex 1 (반복 수집) | **❌ 없음** | Arrow1~6·Cat1~5·CatPang·색폭탄5종은 모두 tapIndex 1 존재 |
| tapIndex 2 (생애 이정표) | ✅ missionID 15 (clearValue=71) | collectionType=45, addValue=-1 (1회성) |
| tapIndex 3 (일일 미션) | ❌ 없음 | 현재 일일 미션은 Attendance·NormalStageClear·BlockDestroy·AdWatch·CatPang 5개뿐 |

tapIndex 2 이정표(71회)를 달성한 이후 유저는 WallCreator 파괴가 **어떤 미션 루프에도 연결되지 않는다**. tapIndex 1 반복 수집 미션이 없으므로 파괴 행위가 보상 없는 소모로만 남는다.

### 과거 감사 후보 (git log 조회 결과 — 최근 10건)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-07-09 | 9ae50e1 | PotalCreator tapIndex 2 이정표 미션 완전 공백 — WallCreator 대칭 미션 결여 |
| 2026-07-08 | d371085 | 보스 스테이지 Stage.json 스킬 조율 필드 전무 — 100개 쿨타임 코드 상수 의존 |
| 2026-07-07 | 1324855 | 보스 스테이지 attack 공격력 스탯 잠듦 — 매 턴 +0, 획득 경로 전무 |
| 2026-07-06 | 80d875f | Wall 블록 tapIndex 1 반복 수집 미션 완전 공백 — 1,719셀·123스테이지 |
| 2026-07-05 | 18dd561 | tapIndex 2 이정표 보상 Gold 단일화 — AddTime·AddMove 전무 |
| 2026-07-04 | 39d8ced | EBackground 4종 미참조 — 배경 커스터마이징 보상 루프 공백 |
| 2026-07-03 | 5c2ba4c | Wall·Potal 초등장 스테이지(6·7) Tutorial.json 항목 완전 공백 |
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 13~15 고아 항목 |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 — 100개 전부 tutorialID=-1 |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 |
| (이하 34건 생략 — 2026-05-28부터) | | |

## 2. 추가 컨텐츠 후보 (권장 1개)

### WallCreator tapIndex 1 반복 수집 미션 신설

- **카테고리**: 미션
- **요지**: WallCreator(EBlockState=45)는 tapIndex 2 이정표(clearValue=71)가 존재해 컬렉션 추적이 가능함을 확인했음에도, tapIndex 1 반복 미션이 전혀 없다. 45개 스테이지·162셀에 걸쳐 등장하는 블록이 이정표 달성 이후 보상 루프에서 완전히 이탈한다.
- **점수**: 검증가치/구현비용/플레이어경험개선/데이터근거 = 4/2/4/5 → 종합 **17**
  - 검증가치 4: 하드 중·후반(stage 71+) 핵심 장애물로, 반복 보상이 없으면 중반 이탈 위험
  - 구현비용 2: Mission.json 1줄 추가 + StringKorea/English 1항목 — 코드 변경 불필요
  - 플레이어경험개선 4: tapIndex 2 클리어 이후 WallCreator 파괴가 완전히 무보상 상태, 즉각 체감 개선
  - 데이터근거 5: Mission.json 내 collectionType=45 tapIndex 1 항목 완전 부재, StageBlock.json 162셀·45스테이지 수치 확보
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 15(tapIndex 2, collectionType=45, clearValue=71)만 존재, tapIndex 1 항목 0건
  - `Assets/AssetBundleResources/json/StageBlock.json` — blockState=45: 하드 132셀/25스테이지(stage 71~150), 보스 30셀/20스테이지(100041~100070)
  - `Assets/Scripts/Defines.cs` — EBlockState.WallCreator=45, Mission.json collectionType=45와 정확히 매핑
  - 비교 대상: Arrow1~6(missionID 1~6, collectionType=10~15, tapIndex 1, clearValue=10) — 동급 장애물 블록 계열이 tapIndex 1 보유

#### 유저 플로우

1. **노출 시점·트리거**
하드 스테이지 71 이상 또는 보스 스테이지(group 100041~100070)에서 WallCreator 블록을 인접 매치로 처음 파괴하는 순간, UIMission tapIndex 1 탭 목록에 해당 미션이 진행 상태(Doing)로 활성화된다. tapIndex 2 이정표를 이미 달성한 유저도 tapIndex 1 카운터는 별도로 시작되어 즉시 참여 가능하다.

2. **화면 변화**
WallCreator가 HP 0으로 소멸하면 파괴 이펙트(기존 Damage 이펙트 재사용 가능)와 함께 Data.Collection의 collectionType=45 누적값이 1 증가한다. UIMission 탭 버튼에 진행 변화 뱃지(숫자 갱신)가 표시되며, 10회 달성 직전에는 "N/10" 형태의 카운터가 화면 상단에 잠시 노출된다.

3. **입력 행동**
유저는 WallCreator 블록에 인접한 일반 Cat 블록을 드래그해 3매치 이상을 성사시켜 WallCreator HP를 1씩 감소시킨다. WallCreator는 직접 드래그할 수 없는 고정 블록이므로 주변 매치 전략이 필요하다. clearValue=10 달성 후 UIMission 탭에서 보상 수령 터치를 입력한다.

4. **시스템 반응**
collectionType=45 카운터가 clearValue=10에 도달하면 Data.Mission에서 해당 미션의 clearState가 Clear로 전환되고 Gold rewardCount(100 권장)를 즉시 지급한다. addValue>0 설정으로 카운터를 0으로 리셋한 뒤 동일 미션이 즉시 재시작된다 — Arrow1~6(tapIndex 1, addValue=10) 반복 구조와 동일하다.

5. **반복·재발생 패턴**
WallCreator는 하드 스테이지당 평균 5.3셀, 보스 스테이지당 평균 1.5셀 배치된다. clearValue=10이면 하드 스테이지 2개를 클리어할 때마다 약 1회 완료가 가능한 속도다(하드 1스테이지 ≒ 5.3회 파괴). 보스 스테이지 전용 유저도 20스테이지 × 1.5셀 = 30셀 ÷ 10 = 3회 완료가 가능하다.

6. **종료·해소 조건**
스테이지를 Game Over로 종료해도 카운터는 유지된다. 보상 수령을 미루면 Clear 상태가 유지되다가 다음 접속 시 UIMission에서 즉시 수령 가능하다. WallCreator가 등장하는 스테이지를 더 이상 반복하지 않으면 카운터가 멈추지만 미션 자체는 비활성화되지 않고 대기 상태로 잔류한다.

7. **다른 시스템과 상호작용**
WallCreator 파괴는 DailyMissionService.OnBlockDestroyed(EDailyCounter.BlockDestroy)와 중복 집계된다 — Mission 102(오늘 블록 100개 파괴)에도 동시 기여한다. WallCreator가 소멸하면 해당 칸에서 매 턴 생성되던 Wall 블록 생성이 중단되어 난이도가 즉시 완화된다 — 파괴의 게임플레이 임팩트가 크기 때문에 tapIndex 1 보상과 의미론적으로 잘 어울린다.

8. **엣지 케이스**
WallCreator는 직접 매치가 불가하나 폭탄 범위(CatPang 3×3, Arrow 가로·세로, BoomAll 전체 등)로 HP 0까지 삭감되면 소멸한다. 이 경우 collectionType=45 카운터가 +1되는지 기존 RemoveMatchBlock/DamageBlock 코드 경로에서 확인이 필요하다. 또한 보스 스킬 EBossSkillType.Creator로 동적 생성된 WallCreator를 파괴할 때도 같은 카운터가 증가하므로 tapIndex 2 달성 이후에도 보스 플레이 중 카운터가 계속 쌓인다 — 이 점을 이용해 고득점 유저는 보스 스테이지 반복으로 tapIndex 1을 빠르게 달성할 수 있다.

9. **유저 정보·피드백**
WallCreator 파괴 시 "장벽 생성기 N/10개 제거" 형태의 UIMission 진행 표시가 즉각 피드백을 제공한다. 일반 고양이 블록(Cat1~5, clearValue=100 → Gold 100)에 비해 파괴 단위당 난이도가 높으므로 rewardCount를 100~150 Gold로 상향 책정하는 것이 적절하다. 하드 스테이지 초입(stage 71) 유저와 보스 스테이지 진입 유저 모두 참여 가능해 타깃 구간이 넓다.

### 보류
- **Potal(17) tapIndex 1 미션**: Wall·Potal tapIndex 1 공백은 2026-07-06 감사(80d875f)에서 Wall과 함께 다루어 채택 보류.
- **PotalCreator(46) tapIndex 1 미션**: PotalCreator tapIndex 2 자체가 미존재(2026-07-09 감사, 9ae50e1)로 tapIndex 2 신설이 선행되어야 해 범위 초과.
- **ESound BGM 단일 구조**: 구현비용 5점으로 종합 12점. 오디오 에셋 신규 제작 필요.
- **selectCatShop 고양이 선택 다양성 미션**: 컬렉션 추적 외 신규 행동 추적 로직 필요, 구현비용 3점 → 종합 14점.

## 3. 과거 감사 대비 차별성

git log 44건 검토 완료.

- **가장 유사한 과거 커밋 1**: 9ae50e1 (2026-07-09) — PotalCreator tapIndex 2 이정표 미션 공백. 차별점: 오늘 후보는 WallCreator(45)의 **tapIndex 1** 공백이며, tapIndex 2(missionID 15)가 이미 존재하는 블록에서 반복 루프가 단절된다는 점에서 레이어가 다르다.
- **가장 유사한 과거 커밋 2**: 80d875f (2026-07-06) — Wall(16) tapIndex 1 반복 수집 미션 공백. 차별점: Wall(16)은 장애물 블록, WallCreator(45)는 **생성기** 블록으로 메커니즘이 전혀 다르다. WallCreator는 tapIndex 2가 이미 존재해 추적 가능성이 확인된 상태에서 tapIndex 1만 누락된 구조적 불완전성이다.
- **중복 없음 판정**: 카테고리(WallCreator tapIndex 1), 요지(반복 수집 루프 단절), 근거(missionID 15 tapIndex 2 기존재 + tapIndex 1 부재)가 과거 44건과 모두 다르다.

## 4. 다음 단계 제안

채택 시 구체 구현 계획:
1. `Assets/AssetBundleResources/json/Mission.json` 에 신규 항목 1줄 추가:
   ```json
   {"missionID":"105", "tapIndex":"1", "descStringID":<신규ID>, "collectionType":45, "clearValue":10, "addValue":10, "reward":0, "rewardCount":100}
   ```
2. `Assets/AssetBundleResources/json/StringKorea.json` / `StringEnglish.json` 에 신규 descStringID 항목 추가 (예: "장벽 생성기 {0}개 제거" / "Destroy {0} Wall Creators").
3. StageBlock.json 에서 WallCreator 등장 스테이지(stage 71~150, boss 100041~100070)를 재확인해 clearValue=10 달성 횟수가 충분한지 검증 (162셀 ÷ 10 = 최대 16회 완료 예상).
4. 보스 EBossSkillType.Creator 동적 생성 WallCreator 파괴 시 collectionType=45 카운터 증가 여부를 GPBossController.cs 및 RemoveMatchBlock 코드 경로에서 확인.

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 '장벽 생성기'라고 불리는 특별한 방해 블록이 있다. 이 블록은 매 턴 주변에 벽을 새로 만들어서 플레이어가 맞추기 어렵게 방해한다. 게임의 중반~후반 스테이지 45개에 등장할 만큼 중요한 존재인데, 지금은 처음 71개를 없애면 작은 보상을 한 번 주는 '목표'만 있을 뿐, 그 이후로는 아무리 많이 없애도 아무 보상이 없다. 비유하자면, 장벽 생성기 71개를 깬 순간부터는 "수고했어, 끝!" 이라고 하는 것과 같다. 그래서 이번에 제안하는 것은: 장벽 생성기를 10개 없앨 때마다 금화 100개를 계속 반복해서 받을 수 있는 '반복 미션'을 하나 추가해, 플레이어가 후반 스테이지에서도 꾸준히 보람을 느끼게 만들자는 것이다.
