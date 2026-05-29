# Content Audit — 2026-05-29 — 기본 고양이 블록 수집 미션 공백 (tapIndex 1)

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 2건 (가장 최근: 2026-05-28, SHA 08f1ddb)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39·47~51 + 미정의 7~9 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1건 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7·골드아이템 2), tapIndex 2: 3개(IAP) |
| 고양이 스킨 | 6종 | Crown / Flowers / Mushroom / Party / Santa / Strawberry × Cat1~5 = 30 슬롯 |

### 분포 공백

**Mission.json tapIndex별 미션 구성:**

| tapIndex | 미션 수 | 대상 블록 (collectionType) | 특이사항 |
|---|---|---|---|
| 1 (달성형 수집) | 12개 | Arrow1-6 (10~15), CatPang(18), PinkBomb(19), YellowBomb(20), OrangeBomb(21), GreenBomb(22), BlueBomb(23) | **Cat1~7 (0~6) 전무** |
| 2 (장기 수집) | 5개 | Fish(24), CatBox1(40), WallCreator(45), RainbowPang(52), Ball(53) | CatBox2~5·PotalCreator 없음 |
| 3 (일일) | 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch / CatPang(18) | 보스 스테이지 클리어 없음 |

**핵심 발견**: tapIndex 1 수집 미션 12개는 Arrow 계열과 특수 폭탄 블록(collectionType 10~23)만 포함하고, **게임에서 가장 빈번하게 등장하는 Cat1~7 기본 고양이 블록(collectionType 0~6)에 대한 수집 미션이 하나도 없다.** 플레이어가 스테이지를 클리어할 때마다 수백 개씩 매치하는 블록인데, 이 행동에 대응하는 미션 보상 루프가 전혀 존재하지 않는다.

tapIndex 1 미션 달성 방식 (`addValue=10`): clearValue(10)에 도달하면 보상(Gold 100) 지급 후 startValue에 addValue(10)를 더해 목표 갱신 — 동일 미션이 반복 달성되는 구조. Cat 블록 추가 시 이 반복 구조에 자연스럽게 편입 가능.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### tapIndex 1 미션에 Cat1~5 기본 고양이 블록 수집 미션 추가

- **카테고리**: 미션
- **요지**: Mission.json tapIndex 1의 12개 수집 미션이 Arrow1-6·특수폭탄만 다루고, 스테이지에서 압도적으로 많이 등장하는 Cat1~5 기본 고양이 블록(EBlockState 0~4)에 대한 수집 미션이 전무하다. Cat1~5별 수집 미션(clearValue=50, addValue=50, reward=Gold 100) 5개를 tapIndex 1에 추가하면 초반 플레이어의 미션 탭 진입 동기와 즉각적 보상 루프를 강화할 수 있다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/5 → 종합 **17**
  - 검증가치 4: 초반 플레이어가 미션 탭을 열면 Arrow/특수폭탄 미션만 보이는데, 이 블록들은 기본 매치로는 만들어지지 않아 즉각 달성이 어렵다. Cat 블록 미션이 있으면 첫 스테이지부터 자연스럽게 달성되는 보상 경로가 생긴다.
  - 구현비용 2: Mission.json에 5행 추가 + StringKorea/StringEnglish에 각 5개 descStringID 등록. 코드 변경 전혀 없음.
  - 플레이어경험개선 4: 미션 탭에 "고양이 블록 50개 모으기" 항목이 있으면 스테이지 플레이와 미션이 자연스럽게 연결되어 목표의식이 높아진다.
  - 데이터근거 5: `Assets/AssetBundleResources/json/Mission.json` 전체 조회 결과, collectionType 0~6(Cat1~7) 해당 항목 없음을 직접 확인.
- **근거**: `Assets/AssetBundleResources/json/Mission.json` 23개 항목 전체 조회. collectionType 10~23(Arrow·특수폭탄) 12개, collectionType 24·40·45·52·53(Fish·CatBox 등) 5개, dailyCounter 기반 5개 — collectionType 0~6 해당 항목 없음. `Assets/Scripts/Defines.cs` 기준 Cat1(0)~Cat7(6)이 EBlockState에 명확히 정의되어 있으며, Data.Collection 구조상 key=(int)EBlockState으로 수집량이 이미 저장된다. 즉 Cat1~7의 수집 누적값은 이미 Data.Collection에 기록되고 있으나, 이에 대응하는 Mission.json 항목이 없어 보상 루프가 단절되어 있다.

#### 유저 플로우 (9개 항목 모두)

1. **노출 시점·트리거**  
   플레이어가 FirstScene의 하단 내비게이션에서 미션 아이콘(UIMission)을 탭하면 tapIndex 1 탭이 기본으로 열린다. 현재는 Arrow·특수폭탄 관련 12개 항목만 보이는데, 이 블록들은 스테이지에서 일부러 조합해야 나타나기 때문에 신규 플레이어는 미션이 자신의 플레이와 연결된다는 느낌을 받기 어렵다. Cat1~5 수집 미션이 추가되면 첫 방문 시 1~3개 미션이 이미 수십 % 달성 상태로 노출되어 즉각적인 진행감을 준다.

2. **화면 변화**  
   UIMission tapIndex 1 리스트에 "고양이 블록 1색 N개 모으기" 형태의 항목이 Cat1~5별로 5개 추가된다. 각 항목에는 해당 색상 Cat 블록 스프라이트 아이콘(EBlockState 0~4에 대응하는 에셋)과 진행 게이지(현재 수집량 / clearValue)가 표시된다. 미션 달성 시 우상단에 골드 획득 연출(UIAlarm 계열 팝업)이 재생된다. 리스트 순서는 missionID 기준이므로, 새 Cat 미션을 missionID 1~5 앞에 두거나 뒤에 이어붙이는 것으로 기획 의도에 따라 결정한다.

3. **입력 행동**  
   플레이어가 스테이지에서 평소처럼 Cat 블록을 드래그하여 3-매치 이상 맞추면 Data.Collection에 수집량이 자동 누적된다. 별도의 추가 조작 없이 스테이지 플레이만으로 미션 진행이 이루어지기 때문에, 플레이어는 미션 탭을 열 때마다 자연스럽게 달성에 가까워진 수치를 확인하게 된다. 미션 달성 시 UIMission에서 "수령" 버튼을 탭하여 Gold를 받는다.

4. **시스템 반응**  
   블록 매치 후 GPBoard·GPMatchChecker에서 제거된 블록의 EBlockState 값이 집계되어 `CHMData`를 통해 Data.Collection[key]를 갱신한다(이 누적 로직은 이미 구현된 상태). Mission.json에 새 항목이 추가되면 CHMData.LoadLocalData 시 해당 missionID에 대한 Data.Mission이 생성되고, DailyMissionService 외 별도 코드 없이 collectionType 기반 진행도 체크(CheckData)가 자동 작동한다. 목표(clearValue) 달성 시 UIMission에서 달성 표시로 전환되고, 탭 이후 addValue만큼 다음 목표가 갱신된다.

5. **반복·재발생 패턴**  
   tapIndex 1 미션은 `addValue=10`(또는 50 등 설정 가능) 반복 달성 구조다 — 현재 Arrow/특수폭탄 미션과 동일하게, Cat 블록도 clearValue 달성 후 다음 목표치가 addValue만큼 상승하여 미션이 무한 반복된다. Cat 블록은 매 스테이지에서 수백 개가 매치되므로, 초반에는 빠른 주기(스테이지 2~3개마다)로 달성 알림이 발생하고 이후 점점 간격이 늘어나는 자연스러운 롱테일 반복 패턴이 형성된다.

6. **종료·해소 조건**  
   개별 달성 사이클은 clearValue를 넘어서는 순간 완료되며 Gold 보상이 지급된다. 그 즉시 다음 목표치(currentTarget + addValue)가 설정되므로 항목이 "완료" 상태로 영구 고정되지 않는다 — Arrow 미션과 동일한 지속형 구조. 플레이어가 의도적으로 종료할 수단은 없으며, 플레이를 계속하는 한 항상 특정 달성률의 Cat 블록 미션이 존재한다.

7. **다른 시스템과 상호작용**  
   Data.Collection에 Cat1~7 수집량이 이미 기록되므로, Mission.json 항목만 추가하면 기존 누적 데이터를 즉시 반영한 진행도가 표시된다. 즉, 오래된 플레이어는 업데이트 직후 미션 탭 첫 진입에서 높은 달성률 또는 즉시 달성 가능한 미션을 만날 수 있다 — 기존 플레이어에게 긍정적 서프라이즈로 작용한다. 일일 미션(tapIndex 3)의 missionID 104(CatPang 수집 clearValue=3)와는 대상 블록(Cat1~5 vs CatPang=18) 및 tapIndex가 달라 중복이 없다.

8. **엣지 케이스**  
   이미 Cat 블록 수집량이 매우 많은 고인물 플레이어(예: Data.Collection["0"] > 1000)는 clearValue=50 미션이 업데이트 직후 즉시 여러 단계 달성된다. 이 경우 한 번에 여러 골드 보상이 지급될 수 있으므로, addValue 단계별로 순차 수령 처리가 되는지 확인이 필요하다(현재 Arrow 미션도 동일 경로를 통과하므로 기존 로직 검증으로 충분). clearValue를 너무 낮게(10) 설정하면 초반부터 달성 알림이 과도하게 빈번해 UX를 방해할 수 있으므로, 첫 목표는 50~100 수준을 권장한다.

9. **유저 정보·피드백**  
   미션 달성 시 UIAlarm 또는 UIMission 내 Toast 형태로 "빨간 고양이 블록 50개 달성! Gold +100" 피드백이 뜬다. 플레이어에게 "내가 이미 하고 있는 행동이 보상으로 이어진다"는 연결감을 주며, 이는 미션 탭 재방문 동기를 강화한다. 장기적으로 Cat1~5 수집 누적량을 UIMission에 개인 기록 형태로 노출("지금까지 빨간 고양이 블록 N개 모음")하면 수집형 플레이어의 소유감도 높일 수 있다.

### 보류

- **tapIndex 2에 CatBox2~5 수집 미션 추가**: CatBox1(40)만 있고 CatBox2~5(41~44)가 없어 장기 미션 목표가 단절된다. 검증가치/구현비용/플레이어경험/데이터근거 = 3/2/3/4 → 종합 14. 중장기 플레이어 대상으로 유효하나, 초반 리텐션 영향이 Cat 기본 블록 미션보다 낮아 차순위.
- **보스 스테이지 클리어 일일 미션 추가(BossStageClear EDailyCounter)**: EDailyCounter enum 확장 + DailyMissionService 코드 수정 필요(구현비용 3). 코드 변경을 동반하므로 이번 회차 보류.
- **tapIndex 1에 Fish(24)·Ball(53) 수집 미션 추가**: 이미 tapIndex 2에 있어 미션 중복 우려. 기획 검토 선행 필요. 보류.

## 3. 과거 감사 대비 차별성

git log 2건 검토 완료.

가장 유사했던 과거 커밋: 해당 없음 — 2건 모두 **스테이지** 카테고리(노멀 스테이지 시간제한 모드 공백, 하드 스테이지 포맷 균일성)이며, 이번 제안은 **미션** 카테고리로 카테고리 자체가 다르다.

차별점 상세:
- `a6cb70b`: 스테이지 후반(121~150) 시간제한 모드 부재 → Stage.json time 파라미터 추가 제안
- `08f1ddb`: 하드 스테이지 boardSize/moveCount 완전 균일 → Stage.json 파라미터 다양화 제안
- **이번**: Mission.json tapIndex 1에서 Cat1~5 기본 블록(collectionType 0~4) 수집 미션 전무 → Mission.json 항목 추가 제안
- 수정 파일이 Stage.json이 아니라 Mission.json + StringKorea/StringEnglish로 완전히 다름
- 영향 받는 시스템: 스테이지 플레이 흐름이 아니라 미션·수집 보상 루프

## 4. 다음 단계 제안

채택 시 구체 구현 계획 수립 필요:

1. `Assets/AssetBundleResources/json/Mission.json`에 Cat1~5 미션 5개 추가
   - missionID: 18~22 (또는 기존 missionID 1 앞에 0~4 등 기획 결정)
   - collectionType: 0, 1, 2, 3, 4 (Cat1~Cat5)
   - tapIndex: 1, clearValue: 50, addValue: 50, reward: 0 (Gold), rewardCount: 100
2. `Assets/AssetBundleResources/json/StringKorea.json` / `StringEnglish.json`에 각 missionID 대응 descStringID 텍스트 등록 (예: "빨간 고양이 블록 N개 모으기")
3. `Assets/Scripts/Editor/CHToolString.cs`를 통해 문자열 검증
4. 기존 플레이어의 Data.Collection["0"]~["4"] 기존 누적값 → 즉시 달성 여부 확인을 위한 단말 테스트
5. Addressables 그룹 재빌드 후 UIMission tapIndex 1 리스트 렌더링 확인

## 5. 쉬운 설명 (비개발자 요약)

캣팡에는 "미션"이라는 탭이 있는데, 여기에는 화살표 블록을 10개 모으거나 특수 폭탄을 터뜨리는 도전과제가 들어있다. 그런데 게임에서 가장 많이 보이는 빨강·노랑·파랑·초록·보라색 기본 고양이 블록에 대한 미션은 단 하나도 없다 — 매 스테이지마다 수백 개씩 맞추는 블록인데도 보상이 연결되지 않는 것이다. 반면 화살표나 폭탄 블록은 특별한 방법으로 만들어야 해서 처음 하는 사람한테는 미션 탭이 "나랑은 상관없는 곳" 처럼 느껴질 수 있다. 그래서 이번에 제안하는 것은: 기본 고양이 블록 5가지 색깔 각각 "50개 모으기" 미션을 추가해서, 스테이지를 플레이할수록 자연스럽게 달성되는 보상 경로를 만들어주자는 것이다.
