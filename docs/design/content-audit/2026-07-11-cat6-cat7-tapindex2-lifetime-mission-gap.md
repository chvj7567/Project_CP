# Content Audit — 2026-07-11 — Cat6·Cat7 tapIndex 2 장기 이정표 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue, Stage, Guide, Mission, Shop, StringKorea, StringEnglish, StageBlock, Tutorial)
- 과거 감사 이력 (git log): 30건 (가장 최근: 2026-07-10 KST — WallCreator tapIndex 1 반복 수집 미션 공백)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 활성 28종 / 전체 84 | 빈 슬롯(25~39, 47~51) 및 고양이 스킨(54~83) 제외 핵심 블록 기준 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1종 = 5건 | tapIndex 3: missionID 100~104 |
| 상점 아이템 | 12개 (tapIndex 1: 9개, tapIndex 2: 3개) | skinIndex 기반 고양이 7종 + RemoveAD/AddTime/AddMove + HP/공격력 |
| 고양이 스킨 | 6종 30블록 (EBlockState 54~83) | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry × Cat1~5 |

### 분포 공백 — Mission.json 미션 구조

Mission.json 29개 미션을 tapIndex 기준으로 분류하면:

**tapIndex 1 (반복 수집 미션, addValue > 0)**

| 블록 타입 | collectionType | 미션 존재 여부 |
|---|---|---|
| Cat1 | 0 | ✅ missionID 18, clearValue 100, addValue 100 |
| Cat2 | 1 | ✅ missionID 19 |
| Cat3 | 2 | ✅ missionID 20 |
| Cat4 | 3 | ✅ missionID 21 |
| Cat5 | 4 | ✅ missionID 22 |
| **Cat6** | **5** | **❌ 완전 공백** |
| **Cat7** | **6** | **❌ 완전 공백** |
| Arrow1~6 | 10~15 | ✅ missionID 1~6 |
| CatPang | 18 | ✅ missionID 7 |
| PinkBomb~BlueBomb | 19~23 | ✅ missionID 8~12 |

**tapIndex 2 (장기 이정표 미션, addValue = -1)**

| 블록 타입 | collectionType | clearValue | 미션 존재 여부 |
|---|---|---|---|
| Fish | 24 | 31 | ✅ missionID 13 |
| CatBox1 | 40 | 51 | ✅ missionID 14 |
| WallCreator | 45 | 71 | ✅ missionID 15 |
| RainbowPang | 52 | 91 | ✅ missionID 16 |
| Ball | 53 | 131 | ✅ missionID 17 |
| **Cat6** | **5** | — | **❌ 완전 공백** |
| **Cat7** | **6** | — | **❌ 완전 공백** |
| Cat1~5 | 0~4 | — | ❌ 공백 (2026-06-19 감사 기록) |

Cat6·Cat7는 tapIndex 1과 tapIndex 2 모두 미션이 없는 유일한 정규 고양이 블록이다.  
Cat1~5는 tapIndex 1 존재 / tapIndex 2 공백이지만, Cat6·Cat7는 tapIndex 1·2 모두 공백이다.

### 과거 감사 후보 (git log 조회 결과, 최근 10건)

| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-07-10 | d043062 | WallCreator tapIndex 1 반복 수집 미션 공백 |
| 2026-07-09 | 9ae50e1 | PotalCreator tapIndex 2 생애 이정표 미션 공백 |
| 2026-07-08 | d371085 | 보스 Stage.json 스킬 조율 필드 전무 |
| 2026-07-07 | 1324855 | 보스 스테이지 attack 잠든 공격력 스탯 |
| 2026-07-06 | 80d875f | Wall 블록 tapIndex 1 반복 수집 미션 공백 |
| 2026-07-05 | 18dd561 | tapIndex 2 이정표 미션 보상 Gold 단일화 |
| 2026-07-04 | 39d8ced | EBackground 4종 완전 미참조 — 배경 커스터마이징 공백 |
| 2026-07-03 | 5c2ba4c | Wall·Potal 초등장 Tutorial.json 항목 공백 |
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 공백 |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 전무 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### [권장] Cat6·Cat7 tapIndex 2 장기 이정표 미션 완전 공백

- **카테고리**: 미션
- **요지**: Cat6(주황 고양이)·Cat7(하얀 고양이)는 Cat1~5와 동등하게 스테이지에 등장하는 정규 블록임에도 tapIndex 1(반복 수집)·tapIndex 2(장기 이정표) 미션이 모두 부재하다. 특히 tapIndex 2 이정표는 Cat6·Cat7를 생애 목표로 삼을 경로를 완전히 차단한다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/5/4/5 → 종합 14
  - 종합 = 4 + (6-5) + 4 + 5 = **14**
- **근거**: `Assets/AssetBundleResources/json/Mission.json` — collectionType 5(Cat6), 6(Cat7)에 대응하는 missionID가 존재하지 않음. Cat1~5(collectionType 0~4)는 tapIndex 1 미션 5건(missionID 18~22, clearValue 100)이 있으나, Cat6·Cat7는 tapIndex 1·2 어디에도 없음.

#### 유저 플로우 (9항목)

1. **노출 시점·트리거**  
   UIMission을 열어 tapIndex 2(이정표) 탭을 선택하면 현재 Fish/CatBox1/WallCreator/RainbowPang/Ball 5개만 표시된다. Cat6·Cat7 이정표 미션이 신설되면 같은 탭에 2개 항목이 추가로 노출된다. 트리거는 최초 UIMission 오픈이며, 별도 해금 조건 없이 즉시 진행 상태를 보여준다.

2. **화면 변화**  
   이정표 탭에 Cat6(주황 고양이) 아이콘과 "주황 고양이 N마리 수집" 텍스트, Cat7(하얀 고양이) 아이콘과 "하얀 고양이 N마리 수집" 텍스트가 각각 진행 바와 함께 표시된다. clearValue 수치는 기존 Fish(31)·CatBox1(51) 사이 구간인 약 40~45를 권장한다.

3. **입력 행동**  
   유저는 별도 행동 없이 일반 스테이지 플레이 중 Cat6 또는 Cat7 블록을 3-매치로 제거하면 카운터가 자동 증가한다. 스테이지 선택·플레이·클리어 루틴 외 추가 UI 조작이 필요 없다.

4. **시스템 반응**  
   블록 제거 시 `CHMData`의 Collection 데이터(collectionType 5/6 key)가 +1 증가하고, DailyMissionService 또는 Mission 체크 로직이 clearValue 달성 여부를 판정한다. clearValue 도달 시 보상(Gold 혹은 AddTime/AddMove)이 지급되고 미션이 완료 상태로 변경된다.

5. **반복·재발생 패턴**  
   tapIndex 2 이정표는 addValue=-1(비반복) 구조이므로 clearValue를 1회 달성하면 영구 완료된다. 반복 참여는 tapIndex 1 신설 미션(clearValue/addValue 설정)을 별도 추가해야 하며, 본 제안은 tapIndex 2 1회 달성 이정표에 집중한다.

6. **종료·해소 조건**  
   Cat6 총 수집 누적이 clearValue(예: 40)에 도달하면 해당 이정표 미션이 `EClearState.Clear`로 전환되어 보상 수령 가능 상태가 된다. Cat7도 동일 흐름. 두 미션은 독립 판정이며 Cat6를 먼저 달성해도 Cat7 진행에 영향 없다.

7. **다른 시스템과 상호작용**  
   `Data.Collection`의 collectionType 5/6 누적값을 읽어 미션 진행률을 표시하므로, 기존 tapIndex 1 수집 미션 채점 로직(`CHMData` → Mission clearState 갱신)과 동일한 경로를 사용한다. 별도 카운터나 새 필드가 필요 없다.

8. **엣지 케이스**  
   고양이 스킨 테마 블록(CatCrown1~CatStrawberry5, EBlockState 54~83)이 스테이지에 배치된 경우, 해당 블록 제거가 Cat6·Cat7 collectionType 5/6 카운터에 포함되는지 확인 필요하다. 스킨 블록은 별도 collectionType이 없으므로 기본 Cat6·Cat7 수집에 합산되는지, 아니면 별도 처리되는지 GPGameScene의 수집 집계 코드를 검토해야 한다.

9. **유저 정보·피드백**  
   현재 Cat6·Cat7에 대한 미션 안내가 전혀 없어 유저는 이 두 고양이 블록을 "의미 없이 사라지는 블록"으로 인식할 수 있다. 이정표 미션 추가 후에는 Mission UI에서 진행률이 표시되어 Cat6·Cat7를 목적 있게 매치하는 동기가 생기고, "N/40 달성" 형태의 피드백으로 장기 참여 의사를 유지할 수 있다.

### 보류
- CatBox2~5 tapIndex 2 이정표 공백: 2026-05-29 potalcreator-catbox-tapindex2 감사와 카테고리 근접
- ESelect JSON 파일 미존재(Select.json 없음): 코드 아키텍처 문제로 컨텐츠 감사 범위 이탈

---

## 3. 과거 감사 대비 차별성

git log 30건 검토 완료.

- **가장 유사한 과거 커밋**: d043062 (WallCreator tapIndex 1, 2026-07-10) 및 9ae50e1 (PotalCreator tapIndex 2, 2026-07-09)
  - 공통점: 특정 블록 타입의 특정 tapIndex 미션 공백
  - **차별점**: 본 제안은 정규 고양이 블록(Cat6·Cat7)으로, 장애물/생성기 계열(Wall/Creator)과 달리 유저가 매 스테이지 직접 드래그·매치하는 블록이다. Cat1~5는 tapIndex 1 존재하지만 Cat6·Cat7는 tapIndex 1·2 모두 없어 미션 연결이 완전히 단절되어 있다는 점에서 심각도가 다르다.
- **2026-06-05 cat6-cat7-tapindex1**: tapIndex 1(반복 수집) 공백을 다뤘고, 본 제안은 tapIndex 2(장기 이정표) 공백을 별도 지적한다.
- **2026-06-19/20 cat1-5-tapindex2**: Cat1~5 한정이었으며 Cat6·Cat7는 명시적으로 제외됨.

---

## 4. 다음 단계 제안

채택 시:
1. `Mission.json`에 missionID 105·106(Cat6 tapIndex 2), missionID 107·108(Cat7 tapIndex 2) 2~4행 추가
2. clearValue 40~45 범위로 Fish(31)·CatBox1(51) 사이 배치
3. 고양이 스킨 블록(CatCrown Cat6 등 EBlockState 없음 확인 → 스킨 테마는 Cat1~5 기반이므로 Cat6·Cat7 스킨 미정의)을 감안해 collectionType 5·6 집계 범위 코드 검토 필요

---

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 고양이 블록이 7가지 색(Cat1~Cat7)이 있다. 1번부터 5번 고양이는 "몇 마리 모았어요" 같은 목표가 있어서 많이 모으면 보상을 받는다. 그런데 6번(주황 고양이)과 7번(하얀 고양이)은 게임판에 똑같이 등장하는데도 목표가 하나도 없어서, 이 두 고양이를 열심히 모아도 아무런 칭찬이나 보상이 없다. 게임을 200번 하면서 6번 고양이를 수백 마리 없앴지만 아무도 그걸 기록해주지 않는 셈이다. 그래서 이번에 제안하는 것은: 6번·7번 고양이를 일정 수 이상(약 40마리) 모으면 깨지는 목표를 추가해, 이 두 고양이도 플레이어에게 의미 있는 블록으로 느껴지게 하자는 것이다.
