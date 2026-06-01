# Content Audit — 2026-06-02 — Arrow 폭탄 tapIndex 3 일일 미션 완전 누락

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 10개 (ConstValue, Guide, Mission, Shop, Stage, StageBlock, StringEnglish, StringKorea, Tutorial, Select)
- 과거 감사 이력 (git log): 8건 (가장 최근: 2026-05-31)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 (StageBlock 배치 기준) | 빈 슬롯 25~39·47~51 제외. Cat6·7은 동적 생성 블록으로 미배치 |
| 일일 미션 종류 | tapIndex 3: 5종 | Attendance / NormalStageClear×3 / BlockDestroy×100 / AdWatch×1 / CatPang×3 |
| 상점 아이템 | 12개 | tapIndex 1 (골드구매) 9개 · tapIndex 2 (IAP) 3개 |
| 고양이 스킨 | 6종 × Cat1~5 = 30개 (EBlockState 54~83) | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |

### 분포 공백

**tapIndex 3 (일일 미션) — collection-based 추적 현황**

| missionID | collectionType | 블록 | clearValue | tapIndex 3 유무 |
|---|---|---|---|---|
| 7 (tapIndex 1) | 18 = CatPang | CatPang | 10 (long-term) | ✓ (missionID 104) |
| 1~6 (tapIndex 1) | 10~15 = Arrow1~6 | Arrow 폭탄 | 10 (long-term) | **✗ 없음** |
| 8~12 (tapIndex 1) | 19~23 = PinkBomb~BlueBomb | 색폭탄 | 10 (long-term) | **✗ 없음** |

**스테이지 내 Arrow 배치 수 (StageBlock.json 전체 기준)**

| 블록 | EBlockState | 배치 수 |
|---|---|---|
| Arrow1 | 10 | 64 |
| Arrow2 | 11 | 6 |
| Arrow3 | 12 | 50 |
| Arrow4 | 13 | 8 |
| Arrow5 | 14 | 37 |
| Arrow6 | 15 | 14 |
| **합계 Arrow1~6** | — | **179** |
| CatPang (비교) | 18 | 43 |

Arrow1~6 합계 179개로, tapIndex 3 일일 추적이 유일하게 존재하는 CatPang(43개)의 **4.2배** 배치.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 (KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| 2026-05-31 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Arrow 폭탄 tapIndex 3 일일 미션 추가 — "오늘 화살표 폭탄 5개 발동"

- **카테고리**: 미션 (tapIndex 3 일일)
- **요지**: tapIndex 3 일일 미션에서 collection-based 블록 추적이 CatPang 단 1종뿐이다. Arrow1~6은 스테이지 전체에 179개 배치(CatPang의 4.2배)이며 4매치·교차매치 때마다 동적으로 추가 생성되는 핵심 블록임에도 tapIndex 3 일일 미션이 전혀 없어 플레이어의 "오늘 잘 한 매치"가 보상 루프로 연결되지 않는다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 5/1/4/5 → 종합 **19**
  - 종합 = 5 + (6-1) + 4 + 5 = **19**
- **근거**:
  - `Assets/AssetBundleResources/json/Mission.json` — tapIndex 3 missionID 100~104 중 collection-based(dailyCounter=-1)는 missionID 104 (collectionType 18=CatPang, clearValue 3)뿐. Arrow(10~15), 색폭탄(19~23) 항목 없음.
  - `Assets/AssetBundleResources/json/StageBlock.json` — Arrow1~6 합계 179 배치 (Cat1~5 합계 200과 동급, CatPang 43의 4.2배).
  - `Assets/Scripts/Defines.cs` — `EDailyCounter` 에는 Attendance(0)/NormalStageClear(1)/BlockDestroy(2)/AdWatch(3) 4종만 존재. Arrow 전용 카운터 없음.
  - `Assets/Scripts/Data.cs` — `dailyCollectionSnapshotJson` 필드가 `{"(int)EBlockState": count}` 형식으로 모든 EBlockState 수집 일일 스냅샷을 지원하므로 코드 변경 없이 JSON 1행만 추가해 Arrow1(10)을 대표 collectionType으로 추적 가능.

#### 유저 플로우 (9항목)

1. **노출 시점·트리거**  
   UIMission의 tapIndex 3(일일 미션) 탭에 "오늘 화살표 폭탄 5개 발동" 미션이 새 항목으로 표시된다. 자정 NTP 리셋 시 매일 초기화되어 플레이어가 앱을 열 때마다 새 목표로 제시된다. 기존 5종 일일 미션 아래 6번째 항목으로 배치되며, 특별한 언락 조건 없이 모든 플레이어에게 처음부터 노출된다.

2. **화면 변화**  
   미션 항목에 "오늘 N/5 화살표 폭탄 발동" 진행 바가 표시된다. Arrow 폭탄을 발동할 때마다 카운터가 실시간 갱신되고, 5회 달성 시 항목이 "완료" 상태(골드 보상 수령 버튼)로 전환된다. 미션 뱃지(미완료 수 표시)도 함께 갱신된다.

3. **입력 행동**  
   플레이어는 평소처럼 게임을 플레이하며 4매치 이상(가로·세로·교차)을 유도해 Arrow 폭탄을 생성하고 발동한다. 별도의 UI 조작이나 특수 행동 없이 자연스러운 플레이 중 발생한다.

4. **시스템 반응**  
   Arrow 폭탄이 `Block.Bomb()` 내의 Bomb4·5·7·8 경로로 발동될 때, `CHMData`의 `dailyCollectionSnapshotJson`에 해당 EBlockState(10~15) 수집 카운터가 증가한다. missionID 104(CatPang 일일)와 동일 로직으로 `DailyMissionService`가 clearValue(5)에 도달했는지 판정한다.

5. **반복·재발생 패턴**  
   매일 자정(NTP 기준) 리셋되어 다음날 다시 0회부터 시작한다. 스테이지마다 4매치 기회가 반복적으로 발생하므로 일반적인 3~5스테이지 플레이 세션에서 5회 달성이 가능하다. 일일 완료 후에는 "완료" 상태로 고정되고 추가 보상은 없다.

6. **종료·해소 조건**  
   당일 Arrow 폭탄 발동이 누적 5회에 도달하면 미션이 완료 상태로 전환되고, UIMission에서 보상(골드 200 제안)을 수령할 수 있다. 수령 후 항목은 "완료" 뱃지로 표시되며 다음 자정 전까지 재진입 불가하다.

7. **다른 시스템과 상호작용**  
   Arrow 폭탄이 다른 폭탄과 조합되는 경우(Arrow + Arrow → 색폭탄 승급, `GPGameScene.AfterDrag`)에도 개별 발동으로 카운팅된다. EGameState가 BossStagePlay인 경우도 동일하게 카운팅된다. `DailyMissionService.CheckAndResetIfNeeded()`가 호출 전 NTP 수신 확인을 선행하므로 시각 위변조 방어가 유지된다.

8. **엣지 케이스**  
   스테이지 클리어 실패(시간·이동 소진, EFailReason)로 게임이 강제 종료되어도 발동된 Arrow 폭탄 카운터는 보존된다 — `dailyCollectionSnapshotJson`이 매 발동 시점에 갱신되기 때문이다. 연속 콤보로 Arrow 폭탄이 연쇄 발동되는 경우 각각 1회로 카운팅된다. Arrow1~6 전부가 아닌 Arrow1(10)만 추적하는 최소 구현이라면, Arrow3·5 등 다른 방향 화살표는 카운팅에서 제외됨을 UI에 명시해야 한다.

9. **유저 정보·피드백**  
   발동 즉시 미션 카운터가 N/5로 증가해 플레이어가 진행 상황을 실시간으로 인지한다. 5회 완료 시 화면 내 알림(UIAlarm)으로 "일일 미션 완료" 팝업이 뜬다. 기존 BlockDestroy×100 미션과 달리 "큰 매치를 의도적으로 만들면 보상이 주어진다"는 직관적인 피드백 루프가 형성되어 플레이어의 전략적 매치 시도를 자연스럽게 유도한다.

### 보류

- **CatBox2/3/5 tapIndex 2 미션 추가**: StageBlock 배치 각 57·57·67개로 미션 없음이 확인되나, 2026-05-29 aeffad1 (PotalCreator·CatBox4 tapIndex 2 제안)과 카테고리·구현 방식이 강하게 겹쳐 본 회차에서 기각. 차기 회차 재검토 가능.
- **보스 스테이지 클리어 tapIndex 3 일일 미션**: EDailyCounter 신규 추가 필요(코드 변경)로 구현비용이 높아 본 회차에서 기각.

## 3. 과거 감사 대비 차별성

git log 8건 전체 검토 완료.

- **가장 유사한 과거 커밋**: `4a09a70` (RainbowPang tapIndex 1 반복 미션 누락) — tapIndex 1 long-term 범위이며, 본 제안의 tapIndex 3 daily 범위와 계층이 다름. RainbowPang은 tapIndex 2(장기)에만 있고 tapIndex 3(일일) 부재를 다루지 않았음.
- **본 제안의 차별점**: 기존 8건 모두 tapIndex 1·2 장기 수집 미션 또는 EDailyCounter 기반 일일 카운터 관련 제안이었다. 본 제안은 **tapIndex 3 내 collection-based 추적 편중**(CatPang 1종 독점)을 최초로 지적한다. 구현 경로도 코드 변경 없이 `Mission.json` 1행 추가로 완결되어 과거 어느 제안과도 구현 메커니즘이 겹치지 않는다.

## 4. 다음 단계 제안

- **채택 시**: Mission.json에 missionID 105~110 범위에서 신규 tapIndex 3 항목 추가 (Arrow1 대표 1종 또는 Arrow1~6 6종 개별). descStringID 172+ 번대 신규 문자열 등록(StringKorea.json / StringEnglish.json). 보상은 Gold 200 제안 (기존 missionID 104 동일 수준).
- **확장 시**: Arrow1~6 각각에 tapIndex 3 미션을 부여하거나, 새 EDailyCounter.ArrowBombActivate(4)를 추가해 6종 합산 추적으로 확장 가능 (코드 변경 수반).
- **검증 방법**: 추가 후 1주일 일일 미션 완료율 비교 (BlockDestroy×100 대비 달성 난이도 밸런스 확인 필요).

## 5. 쉬운 설명 (비개발자 요약)

CatPang 게임에는 "오늘의 숙제"처럼 매일 리셋되는 임무가 있어서, 출석하거나 스테이지를 3번 클리어하거나 광고를 보면 보상을 받을 수 있다. 그런데 이 오늘의 숙제 중 "블록을 맞춰서 화살표 폭탄을 만들면 보상"이라는 항목이 없다. 화살표 폭탄은 게임에서 가장 자주 만드는 특수 블록으로, 전체 스테이지에 179개나 미리 배치되어 있을 정도로 중요한데 말이다. 이미 비슷한 방식으로 "CatPang 폭탄을 오늘 3번 쓰면 보상"은 있는데, 화살표 폭탄은 빠져있는 셈이다. 그래서 이번에 제안하는 것은: 오늘 화살표 폭탄을 5번 발동시키면 보상(골드)을 주는 일일 미션을 추가해, 플레이어가 큰 매치를 노릴 이유를 하나 더 만들자는 것이다.
