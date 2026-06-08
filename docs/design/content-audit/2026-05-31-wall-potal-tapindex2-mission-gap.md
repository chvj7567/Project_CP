# Content Audit — 2026-05-31 — Wall·Potal 장기 수집 미션 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Guide, Mission, Shop, Stage, StageBlock, StringEnglish, StringKorea, Tutorial)
- 과거 감사 이력 (git log): 6건 (가장 최근: 2026-05-30)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 / 전체 84 (빈 슬롯 제외 61종 정의) | Cat6·Cat7·스킨계열(54~83)은 스테이지 미배치 |
| 일일 미션 종류 | EDailyCounter 4종 + EBlockState 기반 1종 = 5건 | Attendance/NormalStageClear/BlockDestroy/AdWatch + CatPang daily |
| 상점 아이템 | 12개 (tapIndex 1: 9개 / tapIndex 2 IAP: 3개) | 스킨 7종 + 아이템 2종 + IAP 3종 |
| 고양이 스킨 | 6테마 × Cat1~5 = 30종 정의 (샵 판매: 7개) | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |
| tapIndex 2 장기 미션 | 5건 | Fish(24)/CatBox1(40)/WallCreator(45)/RainbowPang(52)/Ball(53) |

### 분포 공백

**tapIndex 2 장기 미션 블록 vs. StageBlock.json 배치 수 비교:**

| 블록 | EBlockState | 스테이지 배치 수 | tapIndex 2 미션 여부 |
|---|---|---|---|
| Wall | 16 | **1,719** | ❌ 없음 |
| Potal | 17 | **787** | ❌ 없음 |
| Ball | 53 | 923 | ✅ missionID 17 (clearValue 131) |
| Fish | 24 | 258 | ✅ missionID 13 (clearValue 31) |
| PotalCreator | 46 | 200 | ❌ 없음 (past audit #3에서 제안됨) |
| WallCreator | 45 | 162 | ✅ missionID 15 (clearValue 71) |
| RainbowPang | 52 | 139 | ✅ missionID 16 (clearValue 91) |
| CatBox1 | 40 | 75 | ✅ missionID 14 (clearValue 51) |

Wall은 전체 스테이지에서 **1,719회 배치 = 단일 최다 장애물 블록**이며, Potal도 787회로 2위 수준이다.
tapIndex 2 미션이 있는 블록 5종(Ball·Fish·WallCreator·RainbowPang·CatBox1)보다 Wall·Potal이 훨씬 많이 배치되어 있지만, **장기 미션이 전혀 없다.**

Wall·Potal은 tapIndex 1(반복 달성) 미션도 존재하지 않는다 — 즉 두 종류 모두 미션 시스템에서 완전히 누락된 상태다.

**스테이지 존재 범위:**
- Wall 포함 스테이지: 123개 / 238개 (51.7%)
- Potal 포함 스테이지: 87개 / 238개 (36.6%)
- Wall 또는 Potal 포함: 156개 / 238개 (65.5%)
- 보스 스테이지 내 Wall: 50개, Potal: 30개

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### [권장] Wall·Potal tapIndex 2 장기 파괴 미션 추가

- **카테고리**: 미션 (tapIndex 2 장기)
- **요지**: 전체 스테이지에서 가장 많이 배치된 장애물인 Wall(1,719회)과 Potal(787회)에 tapIndex 2 장기 누적 미션이 없다. 다른 장애물 5종은 모두 tapIndex 2 미션을 보유하고 있어 Wall·Potal만 명백한 공백으로 남아 있다.
- **점수**: 검증가치=4 / 구현비용=2 / 플레이어경험개선=4 / 데이터근거=5 → **종합 17**
- **근거**: `Assets/AssetBundleResources/json/StageBlock.json` — Wall 1,719건(123스테이지), Potal 787건(87스테이지). `Assets/AssetBundleResources/json/Mission.json` — collectionType=16(Wall), 17(Potal) 해당 항목 전무.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   플레이어가 UIMission 화면의 두 번째 탭(tapIndex 2, 장기 미션)을 열면 새로운 행 "Wall 블록 N개 파괴"와 "Potal 블록 N개 파괴"가 목록에 표시된다. 미션은 게임 최초 설치 시점부터 자동 집계가 시작되며 플레이어의 별도 수락 없이 카운트가 누적된다.

2. **화면 변화**
   tapIndex 2 미션 목록(현재 5행)에 Wall·Potal 항목이 각 1행씩 추가되어 총 7행이 된다. 각 행에는 Wall/Potal 블록 아이콘, 현재 파괴 누적 수, 목표 수(예: Wall 200개 / Potal 100개), 달성 보상(Gold)이 표시된다. 달성 시 완료 표시와 보상 수령 버튼이 활성화된다.

3. **입력 행동**
   플레이어는 별도 액션 없이 기존 방식대로 스테이지를 플레이하면 된다. Wall 블록 인접 칸에서 매치가 발생할 때마다 DamageBlock → HP 0 → 소멸 경로가 실행되고, 이 시점에 파괴 카운트가 +1 증가한다. Potal도 동일한 경로로 집계된다.

4. **시스템 반응**
   CHMData의 Collection 데이터 내 key="16"(Wall) 및 key="17"(Potal) 항목의 value가 블록 소멸 시 +1 갱신된다. 기존 tapIndex 2 미션 판정 로직(missionID별 collectionType과 Collection.value 비교)이 그대로 적용되어 clearValue 도달 시 EClearState.Clear로 전환된다.

5. **반복·재발생 패턴**
   tapIndex 2 미션의 addValue 필드를 -1(단발 달성)로 설정하면 목표 달성 후 미션이 완료 처리된다. addValue를 양수로 설정하면 연속 목표 갱신형이 된다. Wall은 65.5%의 스테이지에 분포하므로 적극적인 플레이어라면 노멀 모드 클리어 중 자연스럽게 2~3회 달성 구간을 경험할 수 있다.

6. **종료·해소 조건**
   단발형(addValue=-1)이면 목표 달성과 보상 수령이 완료 조건이다. 반복형이면 미션 화면에서 다음 누적 목표가 자동 갱신된다. 어느 방식이든 중도 이탈 시 누적 카운트는 영구 보존되어 재접속 후 이어진다.

7. **다른 시스템과 상호작용**
   Wall 파괴는 GPMatchChecker/GPGameScene의 CheckArround → DamageBlock → 블록 제거 경로에서 발생한다. WallCreator(45)가 생성한 Wall 블록도 동일 경로로 제거되므로, WallCreator 스테이지에서도 카운트가 자동 집계된다. 보스 스테이지 GPBossController에서 Wall 스킬을 사용하는 경우(EBossSkillType.Wall), 해당 Wall 파괴도 동일 Collection 키로 집계된다.

8. **엣지 케이스**
   Wall이 HP > 1인 상태에서 여러 차례 피격 후 최종 소멸하는 경우, 파괴 카운트는 HP=0 이 되는 시점 1회만 집계해야 한다(중간 피격 시 중복 집계 금지). Potal이 일반 블록으로 변환되는 경우(HP=0 → 일반 블록 전환 스테이지 설계 시)와 완전 소멸하는 경우의 카운트 기준을 사전에 정의해야 한다. GPGS 클라우드 저장 동기화 시 Collection dict의 key="16", "17" 항목이 정합성 있게 병합되는지 확인 필요.

9. **유저 정보·피드백**
   UIMission tapIndex 2 목록에서 "Wall N개 파괴 (달성: M/N)"와 진행 바가 상시 확인 가능하다. 목표 달성 시 UIMission 탭 진입 알림 뱃지가 표시된다(기존 미션 완료 알림 패턴 동일). 게임플레이 중에는 별도 HUD 표시 없이 배경 집계만 진행된다. 과거 스테이지를 재플레이해도 누적 카운트는 정상 추가되어 초보자·복귀 유저 모두 달성 경로를 갖는다.

### 보류

- **Arrow1~6 tapIndex 2 장기 미션**: Arrow 블록은 런타임 생성(4+매치 콤보)이며 tapIndex 1 반복 미션이 이미 존재함. 장기 미션 공백이지만 Wall·Potal보다 데이터 근거(배치 수)가 없어 2순위로 보류. 종합점수 13.
- **보스 스테이지 보드 크기 단일화**: 전체 100스테이지가 boardSize=9로 고정. 과거 감사 #1(하드 스테이지 포맷 단일화)과 카테고리 겹침. 보류. 종합점수 15.
- **Cat6·Cat7 tapIndex 1 미션 공백**: Cat6·Cat7(EBlockState=5,6)은 StageBlock.json에 배치 사례 0건 — 스테이지에 실제 등장하지 않으므로 미션화 근거 부족. 보류. 종합점수 11.

## 3. 과거 감사 대비 차별성

git log 6건 검토 완료. 가장 유사한 과거 커밋: `aeffad1` (PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안).

**차별점:**
- 과거 #3 대상: PotalCreator(46) = 블록 생성기, CatBox4(43) = 고양이 상자 컨테이너 → 특수 메커니즘 블록
- 본 회차 대상: Wall(16) = 기본 장애물, Potal(17) = 기본 포탈 장애물 → 매치 인접 피격으로 제거되는 가장 원초적이고 빈출하는 장애물

과거 #3의 블록들은 배치 수가 각 200개 이하인 특수 블록이었으나, Wall(1,719)·Potal(787)은 전체 스테이지 배치 합산 1위·2위로 규모 차이가 3~10배 이상이다. 또한 Wall·Potal은 tapIndex 1 미션도 전혀 없어 미션 시스템 내 완전 누락 상태인 반면, CatBox1(tapIndex 2 존재)처럼 일부 연계 블록은 이미 커버되어 있다.

## 4. 다음 단계 제안

- 채택 시 Mission.json에 tapIndex=2, collectionType=16(Wall), 17(Potal) 행 추가 필요
- clearValue 초안 제안: Wall 200개(현 Ball 131보다 높은 배치 빈도 반영) / Potal 100개
- addValue=-1 (단발형)로 우선 검증 후 반복형 전환 고려
- 보스 스테이지 내 Wall/Potal 파괴 집계 경로(GPBossController ↔ Collection) 정합성 확인 필요
- 기존 tapIndex 2 미션 UI(UIMission)가 추가 행을 정상 렌더링하는지 스크롤 동작 확인 필요

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 스테이지를 어렵게 만드는 장애물 블록들이 있는데, 그중 "벽(Wall)"이라는 블록이 전체 맵에서 가장 많이 등장하는 방해 요소다 — 게임 맵 데이터를 세어보니 무려 1,719번이나 배치되어 있어 다른 어떤 장애물보다 압도적으로 많다. 그런데 정작 이 벽을 많이 부쉈을 때 플레이어에게 특별한 성취나 보상을 주는 장기 미션이 하나도 없다. 비슷하게 자주 등장하는 물고기(Fish)나 공(Ball) 같은 블록에는 "평생 N개 파괴" 목표 미션이 있는데도 말이다. 그래서 이번에 제안하는 것은: 가장 흔하게 마주치는 벽과 포탈 장애물을 "누적 N개 파괴" 장기 목표로 연결해, 매일 스테이지를 클리어할수록 목표에 조금씩 가까워지는 재미를 추가하는 것이다.
