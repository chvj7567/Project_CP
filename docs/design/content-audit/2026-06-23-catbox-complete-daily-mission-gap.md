# Content Audit — 2026-06-23 — CatBox 완성 일일 미션 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 25건 (가장 최근: 2026-06-22)

---

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 Max 84 | 빈 슬롯 25~39(15개), 47~51(5개) 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + collectionType 기반 1종 = 5항목 | Attendance·NormalStageClear·BlockDestroy·AdWatch + CatPang daily |
| 상점 아이템 | 12개 | 고양이 스킨 7개(gold 0~60,000) · IAP 3개(RemoveAD/AddTime/AddMove) · 골드 아이템 2개(5,000씩) |
| 고양이 스킨 | 6종 테마 × Cat1~5 = 30개 EBlockState | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry |

### 미션 계층별 현황

| tapIndex | 역할 | 현재 대상 블록/카운터 | 건수 |
|---|---|---|---|
| 1 (반복 수집) | 블록 누적 수집 — 달성 시 addValue 추가로 무한 반복 | Cat1~5(0~4), Arrow1~6(10~15), CatPang(18), PinkBomb~BlueBomb(19~23) | 17종 |
| 2 (이정표) | 누적 임계값 1회 달성 — addValue=-1이라 반복 없음 | Fish(24), CatBox1(40), WallCreator(45), RainbowPang(52), Ball(53) | 5종 |
| 3 (일일) | 자정 리셋 — dailyCounter 또는 collectionType 스냅샷 차분 | Attendance(0), NormalStageClear(1), BlockDestroy(2), AdWatch(3), CatPang 일일(collectionType 18) | 5항목 |

### 분포 공백

- **CatBox tapIndex 3 일일 미션 = 0건**: Mission.json 전체에서 dailyCounter 또는 collectionType 40~44(CatBox1~5)를 참조하는 tapIndex 3 행이 없음.
- tapIndex 2 이정표에 CatBox1(collectionType 40, clearValue 51)이 있어 CatBox 완성이 Collection에 누적되고 있음은 확인됨.
- GPBossController.BossSkill 로직상 CatBox 스킬이 발동되는 보스 스테이지는 `stage % 10 == 0` 일 때만 (전체 100개 중 10개, 10%) — 플레이어가 CatBox를 만나는 빈도 자체가 적음.
- 일일 미션 5항목 중 보스 스테이지 고유 메커니즘을 측정하는 항목은 0개.

### 과거 감사 후보 (git log 조회 결과)

| 날짜(KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 — 난이도 역전 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(54~83) 수집 미션 완전 공백 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-12 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 |
| 2026-06-11 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-10 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-09 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-08 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-07 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-06-01 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 |
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### CatBox 완성 일일 미션 신설 — tapIndex 3 일일 목표 공백

- **카테고리**: 미션 (tapIndex 3 일일)
- **요지**: CatBox1~5(EBlockState 40~44)를 완성하는 일일 미션이 Mission.json에 전혀 없다. tapIndex 2 이정표(CatBox1, clearValue 51)가 존재하여 완성 누적이 Collection에 기록되고 있으므로, `collectionType: 40` 기반으로 하루 스냅샷 차분 방식의 tapIndex 3 미션 1줄을 추가하면 구현 비용은 JSON 1행 + 문자열 2행 수준이다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/1/4/4 → 종합 **17**
  - 검증가치 4: 일일 미션은 D1/D7 리텐션에 직결 — 보스 스테이지 참여 유도 효과 측정 가능
  - 구현비용 1: Mission.json 1행 추가 + StringKorea/StringEnglish 각 1행 — 코드 변경 없음 (missionID 104 CatPang daily와 동일 collectionType 스냅샷 경로 재사용)
  - 플레이어경험 4: 보스 스테이지 고유 메커니즘(CatBox)을 매일 달성 가능한 하루 목표와 연결 — 고수 플레이어의 일상 루프 강화
  - 데이터근거 4: Mission.json 명세 공백이 코드 1줄로 확인됨; tapIndex 2 이정표 51개가 CatBox 완성 빈도를 간접 증명
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — tapIndex 3 항목 5건 전부 조회: dailyCounter 0/1/2/3, collectionType 18. CatBox collectionType(40~44) 없음.
  - `Assets/AssetBundleResources/json/Mission.json` missionID 13 — `"tapIndex":"2", "collectionType":40, "clearValue":51` → CatBox1 완성이 Collection에 기록됨 확인.
  - `Assets/Scripts/GamePlay/GPBossController.cs` L87~89 — `mod==0`(10%), `mod>=6`(40%), else(50%). CatBox 스킬은 mod==0만(10%), Wall은 100% 발동 → CatBox 접촉 빈도 낮음, 일일 미션으로 노출 기회 확보 가치 있음.
  - `Assets/Scripts/Defines.cs` EDailyCounter — None/Attendance/NormalStageClear/BlockDestroy/AdWatch. CatBox 관련 카운터 없음.
  - `Assets/Scripts/Data.cs` Data.Login.dailyCollectionSnapshotJson — EBlockState int 키로 스냅샷 저장. collectionType 40(CatBox1) 키가 이미 스냅샷 구조에 적재 가능.

#### 유저 플로우

1. **노출 시점·트리거**: 플레이어가 앱 실행 후 UIMission 일일 탭을 열면 "오늘 CatBox 2개 완성하기" 미션이 기존 5개 일일 항목 아래에 표시된다. 탭 진입 전에 DailyMissionService.CheckAndResetIfNeeded()가 먼저 호출되어 자정 이후 첫 진입 시 카운터가 0으로 리셋된 상태다.

2. **화면 변화**: 미션 목록에 새 행이 추가되어 "CatBox 완성 0/2" 형태의 진행도 바와 보상 골드(예: 300) 아이콘이 노출된다. 완성 수가 0이라면 진행도 바가 비어 있고, 달성 시 체크마크가 뜬다. 미션 UI는 기존 UIMission 레이아웃을 그대로 사용한다.

3. **입력 행동**: 플레이어는 CatBox가 배치된 보스 스테이지(특히 stage % 10 == 0인 스테이지)나 일반 스테이지를 선택해 플레이한다. 게임 보드에서 CatBox1~5 위 칸에 같은 색 고양이를 드래그해 빨려들게 만드는 CatInTheBox 동작을 반복한다.

4. **시스템 반응**: CatBox HP가 0이 되어 완성될 때마다 GPGameScene 또는 GPBoard 내 완성 처리 로직이 Data.Collection의 collectionType 40(CatBox1 기준) 카운터를 1 증가시킨다. 이미 tapIndex 2 이정표 카운터(clearValue 51)가 같은 경로를 타고 있으므로, DailyMissionService가 자정 스냅샷 차분을 계산해 일일 카운터를 갱신한다.

5. **반복·재발생 패턴**: NTP 기준 자정마다 DailyMissionService.CheckAndResetIfNeeded()가 dailyCollectionSnapshotJson의 collectionType 40 스냅샷을 갱신하고 일일 차분을 0으로 리셋한다. 플레이어는 매일 2개를 목표로 재도전한다. 일일 목표값(2)이 낮아 보스 스테이지 1회(최소 1개 CatBox 완성 가능) + 추가 1회면 달성 가능하다.

6. **종료·해소 조건**: 하루 CatBox 완성 누적이 2개 이상이 되면 UIMission에서 "완료" 상태로 전환되고, 보상 골드를 수령할 수 있다. 수령 전에는 미션 탭에 알림 배지가 표시된다. 수령 후에는 달성 행이 완료 스타일(회색 처리 등)로 변경된다.

7. **다른 시스템과 상호작용**: Data.Collection의 CatBox1(collectionType 40) 카운터 → dailyCollectionSnapshotJson 스냅샷 → DailyMissionService 차분 → UIMission 진행도 갱신 → Data.Login.gold 증가 → CHMData.SaveData() 순으로 연동된다. tapIndex 2 이정표(clearValue 51)와 동일한 컬렉션 키를 공유하므로 이정표 달성 진행도와 일일 미션 진행도가 함께 오른다.

8. **엣지 케이스**: ① CatBox가 없는 스테이지(group < 100000 일반 스테이지 대부분)만 반복 플레이하면 달성 불가 → 일일 목표를 1~2개로 낮게 설정해 보스 스테이지 1~2회 플레이로 달성 가능하게 해야 함. ② NTP 미수신 상태에서는 DailyMissionService 리셋 불발 — 기존 정책 그대로(리셋 안 함, 위변조 안전). ③ CatBox2~5(41~44)는 별도 collectionType이므로 missionID에서 collectionType 40(CatBox1)만 참조하면 CatBox1 완성 횟수만 카운트됨 — 모든 CatBox 종류를 합산하려면 구현 확장 필요. 단순 MVP로는 collectionType 40 단일 참조로 시작.

9. **유저 정보·피드백**: 미션 달성 시 UIMission에 "미션 완료!" 팝업(기존 UIAlarm 또는 UIMission 달성 이펙트 재사용)이 뜨고 골드 보상 수치가 표시된다. 플레이어는 "CatBox를 완성할수록 골드가 쌓인다"는 인과를 체감하며, 보스 스테이지를 피하지 않고 적극적으로 CatBox 라인을 클리어하는 플레이 패턴이 형성된다.

### 보류 후보

| 후보 | 카테고리 | 점수 | 보류 이유 |
|---|---|---|---|
| Fish 전용 일일 미션 | 미션 tapIndex 3 | 14 | Fish는 맨 아래줄 도달로만 탈출 가능 — 의도적 조작이 어려워 일일 목표 달성 경험이 불쾌할 수 있음; 2026-06-18 Ball 탈출 미션과 유사 카테고리 |
| WallCreator/PotalCreator 파괴 일일 미션 | 미션 tapIndex 3 | 14 | tapIndex 2 이정표(WallCreator, clearValue 71)와 tapIndex 2(PotalCreator·CatBox4) 두 차례 감사(2026-05-31, 2026-05-29)가 이미 존재; 3회 이상 언급된 카테고리 |

---

## 3. 과거 감사 대비 차별성

- git log 25건 검토 완료.
- **가장 유사한 과거 커밋 1**: `aeffad1` (2026-05-29) "PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안" — 차별점: 그 감사는 tapIndex 2 이정표(장기 누적) 공백이었음. 본 감사는 tapIndex 3 일일 미션(자정 리셋) 공백으로, 미션 레이어가 다르다.
- **가장 유사한 과거 커밋 2**: `58c3f65` (2026-05-29) "보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안" — 차별점: 그 감사는 보스 스킬 발동 확률(스테이지 배분) 문제였음. 본 감사는 CatBox 완성 행위에 일일 보상 루프가 없다는 미션 설계 공백이다.
- 카테고리(tapIndex 3 일일 미션) × 대상(CatBox 완성)의 교차는 25건 중 없음 → 중복 없음 확인.

---

## 4. 다음 단계 제안

- 채택 시 Mission.json에 다음 형식으로 1행 추가:
  ```json
  {"missionID":"105", "tapIndex":"3", "descStringID":<신규ID>, "collectionType":40, "dailyCounter":-1, "clearValue":2, "addValue":0, "reward":0, "rewardCount":300}
  ```
- StringKorea.json / StringEnglish.json에 "오늘 CatBox 2개 완성하기" / "Complete 2 CatBoxes today" 문자열 추가.
- DailyMissionService 및 dailyCollectionSnapshotJson 경로는 missionID 104(CatPang daily)와 동일하게 재사용 가능 — 코드 변경 없음.
- clearValue 1 vs 2 중 선택은 보스 스테이지 CatBox 발동 빈도(현재 mod==0 10%) 고려 필요 — 플레이테스트로 검증.

---

## 5. 쉬운 설명 (비개발자 요약)

CatBox는 게임 보드에 등장하는 특별한 상자 블록인데, 같은 색 고양이를 상자 위에 올리면 고양이가 빨려 들어가 상자 HP가 깎이고 결국 사라지는 재미있는 장치다. 현재 게임에는 "오늘 CatBox를 N개 완성하면 골드 보상" 같은 하루 목표가 전혀 없어서, 보스 스테이지에서 열심히 CatBox를 없애도 매일 챙길 수 있는 특별 보상이 없는 상태다. 반면 "광고 1번 보기", "노멀 스테이지 3번 클리어" 같은 다른 하루 목표들은 이미 있어서 CatBox만 빠진 셈이다. 그래서 이번에 제안하는 것은: Mission.json 1줄만 추가해 "오늘 CatBox 2개 완성하기" 하루 목표를 신설하여, 보스 스테이지를 즐기는 플레이어가 매일 골드를 받을 수 있는 작은 보상 고리를 만들자.
