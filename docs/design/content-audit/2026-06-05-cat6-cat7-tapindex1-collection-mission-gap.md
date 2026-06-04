# Content Audit — 2026-06-05 — Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 11건 (가장 최근: 2026-06-04, SHA 1d91056)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39·47~51 + 미정의 7~9 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1건 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7·골드아이템 2), tapIndex 2: 3개(IAP) |
| 고양이 스킨 | 6종 × Cat1~5 = 30슬롯 | Crown / Flowers / Mushroom / Party / Santa / Strawberry |

### 분포 공백

**Mission.json tapIndex 1 — 기본 고양이 블록 수집 현황:**

| EBlockState | collectionType | 블록명 | tapIndex 1 미션 존재 여부 |
|---|---|---|---|
| 0 | 0 | Cat1 | ✅ missionID 18 (clearValue=100, addValue=100) |
| 1 | 1 | Cat2 | ✅ missionID 19 |
| 2 | 2 | Cat3 | ✅ missionID 20 |
| 3 | 3 | Cat4 | ✅ missionID 21 |
| 4 | 4 | Cat5 | ✅ missionID 22 |
| 5 | 5 | Cat6 | **❌ 완전 누락** |
| 6 | 6 | Cat7 | **❌ 완전 누락** |

**핵심 발견**: Cat1~5에 대한 tapIndex 1 수집 미션은 이미 missionID 18~22로 구현되어 있으나, 같은 계열인 Cat6(EBlockState=5)과 Cat7(EBlockState=6)에 대한 항목이 전혀 없다. 7가지 기본 고양이 색상 중 5가지만 미션에 반영되고 나머지 2가지는 아무리 매치해도 수집 미션 보상 루프에 진입조차 되지 않는 비대칭 상태다. Data.Collection에는 key="5", key="6"으로 수집량이 이미 누적되고 있어, 데이터는 있으나 보상이 없는 구조다.

**tapIndex 1 기본 고양이 블록 미션과 타 블록 미션 비교:**

| 블록 계열 | tapIndex 1 항목 수 | collectionType 범위 | 비고 |
|---|---|---|---|
| Cat1~5 | 5개 (mID 18~22) | 0~4 | 구현 완료 |
| Cat6~7 | **0개** | 5~6 | **누락** |
| Arrow1~6 | 6개 (mID 1~6) | 10~15 | 구현 완료 |
| 특수폭탄 5종 | 5개 (mID 8~12) | 19~23 | 구현 완료 |
| CatPang | 1개 (mID 7) | 18 | 구현 완료 |

### 과거 감사 후보 (git log 조회 결과)

| 날짜 (KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 — 일일 미션 최대 600골드/일 대비 최고가 스킨 100일 소요 |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch 1회 보상이 BlockDestroy 100개 보상의 10배 |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 — 화살표 폭탄 5회 일일 미션 추가 제안 |
| 2026-06-01 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 — RainbowPang 반복 미션 누락 제안 |
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 — 최빈출 장애물 장기 미션 추가 제안 |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Cat6·Cat7 tapIndex 1 수집 미션 2개 추가

- **카테고리**: 미션
- **요지**: Cat1~5 수집 미션(missionID 18~22)이 이미 tapIndex 1에 구현된 상태에서, 동일 계열인 Cat6·Cat7(EBlockState 5·6)만 빠져 7색 기본 고양이 중 2색이 보상 루프에서 제외되어 있다. Mission.json에 collectionType=5·6인 항목 2개를 추가하면(Cat1~5와 동일한 clearValue=100, addValue=100 구조) 코드 변경 없이 비대칭이 해소된다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/1/4/5 → 종합 **18**
  - 검증가치 4: Cat1~5와 Cat6~7의 보상 불균형이 수집 진행도 탭에서 명확히 시각화된다. 7가지 색상 중 5가지만 진행바가 채워지는 상황은 컬렉터 플레이어에게 즉각적으로 감지된다.
  - 구현비용 1: Mission.json 2행 추가 + StringKorea/StringEnglish 각 2개 문자열 등록. 코드 변경 전무.
  - 플레이어경험개선 4: 같은 화면(UIMission tapIndex 1)에서 Cat1~5 진행바는 채워지는데 Cat6·Cat7 항목 자체가 없으면 수집형 플레이어의 완성 욕구가 차단된다. 항목 추가 후 즉시 기존 수집 데이터를 반영하여 오래된 플레이어에게 서프라이즈 달성 연출도 기대 가능.
  - 데이터근거 5: `Assets/AssetBundleResources/json/Mission.json` 직접 조회로 collectionType 5(Cat6)·6(Cat7) 완전 부재 확인. Data.Collection 키="5"·"6"은 이미 누적 중.
- **근거**: `Assets/AssetBundleResources/json/Mission.json` — 29개 항목 중 collectionType 0(Cat1)~4(Cat5)는 missionID 18~22로 구현됨. collectionType 5(Cat6)·6(Cat7) 해당 항목 없음. `Assets/Scripts/Data.cs` — Data.Collection은 `key=(int)EBlockState`·`value=누적수` 구조로, Cat6(key="5")·Cat7(key="6")의 수집량이 게임 플레이 중 이미 적재되고 있음. `Assets/Scripts/Defines.cs` — EBlockState.Cat6=5, Cat7=6 명확히 정의됨.

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   플레이어가 FirstScene 하단 내비게이션에서 UIMission을 열고 tapIndex 1(수집 미션) 탭을 선택할 때 항목 목록이 렌더링된다. 현재는 Cat1~5 미션 5개, Arrow1~6 미션 6개, 특수 폭탄 계열 6개 순서로 노출된다. Cat6·Cat7 항목이 추가되면 Cat5 항목 바로 아래(missionID 순 기준)에 두 항목이 삽입되어 기본 고양이 7색 전체가 연속적으로 보이게 된다.

2. **화면 변화**
   UIMission tapIndex 1 리스트에 "주황 고양이 블록 100개 모으기"·"보라 고양이 블록 100개 모으기"(또는 해당 Cat6·Cat7 색상명) 카드가 추가된다. 각 카드에는 Cat6·Cat7에 대응하는 블록 스프라이트 아이콘과 현재 수집량/목표치(0/100 초기값 또는 기존 누적값/100) 게이지가 표시된다. 기존 플레이어는 업데이트 직후 이 항목이 이미 일정 % 채워진 상태로 보이게 되어, 즉각적인 달성 가능성을 암시하는 진입 경험을 얻는다.

3. **입력 행동**
   플레이어는 스테이지에서 Cat6 또는 Cat7 블록을 드래그해 3-매치 이상 맞추면 된다. 별도의 추가 조작이나 의식적인 목표 설정 없이, 기존 플레이와 완전히 동일한 행동이 미션 진행으로 이어진다. 미션이 clearValue(100)에 도달하면 UIMission tapIndex 1 해당 항목에 "수령" 버튼이 활성화되고, 플레이어가 탭하면 보상이 지급된다.

4. **시스템 반응**
   블록 매치 시 GPBoard·GPMatchChecker가 제거된 블록의 EBlockState를 집계하고 `CHMData`를 통해 Data.Collection[key]를 갱신한다(기존 구현). Mission.json에 collectionType=5·6 항목이 추가되면 CHMData.LoadLocalData 시 해당 missionID에 대한 Data.Mission 엔트리가 생성되고, 기존 collectionType 기반 진행도 체크 로직이 Cat6·Cat7 누적값도 자동으로 평가한다. clearValue(100) 달성 시 UIMission에서 달성 UI로 전환되고 보상(Gold 100) 지급 후 다음 목표치(currentTarget+addValue=100)로 갱신된다.

5. **반복·재발생 패턴**
   Cat1~5 미션과 동일하게 `addValue=100` 설정 시 100개 단위로 목표가 갱신되어 무한 반복 달성 구조가 된다. Cat6·Cat7는 스테이지마다 등장 빈도가 Color1~5와 유사하므로(7색 고른 분포 가정), 초반 스테이지에서 빠른 주기(3~5스테이지마다)로 달성 알림이 발생하다가 누적량이 늘수록 간격이 자연스럽게 늘어나는 롱테일 패턴을 형성한다. 플레이어가 앱에 재접속할 때마다 미션 탭에 새 달성 가능 항목이 쌓여 있는 재방문 인센티브로 작용한다.

6. **종료·해소 조건**
   개별 달성 사이클은 clearValue 초과 시 완료되고 즉시 다음 목표치가 갱신된다 — Cat1~5 미션과 동일한 지속형 구조. 미션 항목이 "완료됨" 상태로 영구 고정되는 일은 없으며, 플레이를 계속하는 한 항상 특정 달성률의 Cat6·Cat7 수집 미션이 존재한다. 플레이어 스스로 미션을 닫을 수단은 없고, 원하면 탭 자체를 보지 않으면 된다.

7. **다른 시스템과 상호작용**
   Data.Collection key="5"(Cat6)·"6"(Cat7)에 이미 수집량이 누적되어 있으므로, Mission.json 항목 추가만으로 기존 누적 데이터가 즉시 진행도에 반영된다. 즉, 오래된 플레이어는 업데이트 직후 첫 방문 시 일부 또는 전부 달성된 미션을 발견할 수 있어, 과거 플레이에 대한 소급 보상 서프라이즈 효과가 있다. 고양이 스킨 시스템(CatCrown1~5 등)은 Cat1~5 범위만 커버하므로 Cat6·Cat7 수집 미션과 직접 연계는 없지만, 스킨 확장 기획 시 Cat6·Cat7 테마 추가와 자연스럽게 연결될 수 있다.

8. **엣지 케이스**
   일부 스테이지에서 Cat6·Cat7가 배치되지 않거나 매우 드물게 등장하면, 해당 스테이지가 많은 구간에서는 다른 Cat 미션보다 달성 속도가 느려질 수 있다. 이 경우 플레이어에게 "이 색 고양이가 이 스테이지엔 없다"는 안내가 없으면 미션이 막힌 것처럼 느껴질 수 있으므로, 구현 전 스테이지별 Cat6·Cat7 배치 빈도를 StageBlock.json에서 확인하는 것을 권장한다. 고인물 플레이어(Data.Collection["5"] > 1000 등)는 업데이트 직후 즉시 여러 단계가 달성될 수 있으나, 현재 Cat1~5 미션이 동일 경로로 작동하고 있으므로 기존 로직 검증으로 대응 가능하다.

9. **유저 정보·피드백**
   미션 달성 시 UIAlarm 또는 UIMission 내 Toast 형태로 "주황 고양이 블록 100개 달성! Gold +100" 피드백이 노출된다. 플레이어에게 "내가 화면에서 흔히 보는 이 색 블록도 카운트된다"는 연결감을 주며, 기존 Cat1~5 미션과 함께 기본 고양이 수집 섹션이 완전하게 채워지는 시각적 완성감을 제공한다. 장기적으로 7색 고양이 블록 수집량을 개인 기록으로 프로필 페이지에 노출하는 확장 기획과 연계될 수 있다.

### 보류

- **Cat6·Cat7 tapIndex 2 장기 수집 미션 추가**: tapIndex 2는 Fish·CatBox·WallCreator·RainbowPang·Ball처럼 제거 난이도가 높은 특수 블록에 집중되어 있다. Cat6·Cat7는 일반 매치 블록이므로 tapIndex 2 포맷(addValue=-1, 고정 목표)과 맞지 않는다. 기획 검토 선행 필요. 보류.
- **Cat6·Cat7 tapIndex 3 일일 미션 추가**: 일일 미션은 EDailyCounter 기반으로 코드 수정 없이 JSON만으로 Cat 블록 색상 구분이 불가하다. DailyMissionService 코드 수정 + EDailyCounter enum 확장이 필요해 구현비용이 높다. 보류.
- **ESelect 파워업 스테이지 배분**: Select.json 파일이 AssetBundleResources/json/ 에 존재하지 않음을 Glob으로 확인. 미구현 또는 별도 경로 사용 가능성 있어 현황 파악 불충분. 다음 회차 심층 조사 대상.

## 3. 과거 감사 대비 차별성

git log 11건 검토 완료.

**가장 유사한 과거 커밋**: `b5bcc97` (2026-05-29) "미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안"

| 비교 항목 | 과거 (`b5bcc97`) | 이번 회차 |
|---|---|---|
| 제안 시점 상태 | Cat1~7 전부 tapIndex 1 미션 없음 | Cat1~5는 이미 구현됨(mID 18~22), Cat6·Cat7만 누락 |
| 대상 collectionType | 0~4 (Cat1~5) | 5·6 (Cat6·Cat7) |
| 문제 구조 | 전무(全無) | 비대칭(部分) |
| 추가 항목 수 | 5개 | 2개 |

과거 감사는 "7가지 기본 고양이 색 모두 미션이 없다"는 완전 공백 문제를 다뤘고, 그 결과 Cat1~5 미션(missionID 18~22)이 구현된 것으로 보인다. 이번 감사는 그 이후에 남겨진 **Cat6·Cat7의 편측 누락**을 다루는 것으로, 동일 파일·동일 tapIndex를 다루지만 발견의 성격(전무 vs 비대칭)과 대상 블록 타입이 명확히 다르다.

## 4. 다음 단계 제안

채택 시 구체 구현 계획 수립 필요:

1. `Assets/AssetBundleResources/json/Mission.json`에 Cat6·Cat7 미션 2개 추가
   - Cat6: `{"missionID":"23", "tapIndex":"1", "descStringID":185, "collectionType":5, "clearValue":100, "addValue":100, "reward":0, "rewardCount":100}`
   - Cat7: `{"missionID":"24", "tapIndex":"1", "descStringID":186, "collectionType":6, "clearValue":100, "addValue":100, "reward":0, "rewardCount":100}`
   - missionID 및 descStringID는 기존 최고값 이후로 설정 (충돌 여부 확인 필요)
2. `Assets/AssetBundleResources/json/StringKorea.json` / `StringEnglish.json`에 descStringID 185·186 텍스트 등록
3. `Assets/Scripts/Editor/CHToolString.cs`로 문자열 등록 검증
4. 기존 플레이어 Data.Collection["5"]·["6"] 누적값 → 업데이트 직후 즉시 달성 여부 확인을 위한 단말 테스트
5. StageBlock.json에서 Cat6·Cat7 배치 빈도를 검토해 clearValue 조정 여부 결정 (현재 Cat1~5와 동일한 100 제안)

## 5. 쉬운 설명 (비개발자 요약)

캣팡에는 "고양이 블록 많이 모으기" 미션이 있는데, 지금은 5가지 색깔 고양이에 대해서만 진행 바가 채워진다. 그런데 게임에는 총 7가지 색깔 고양이가 있고, 나머지 2가지(여섯 번째·일곱 번째 색상)도 스테이지에 자주 등장해 플레이어가 매번 맞추고 있다 — 그냥 기록이 되지 않을 뿐이다. 미션 탭을 열면 5가지 색깔 진행바가 나란히 채워지는 것을 보다가, 나머지 2가지 색깔 블록을 아무리 맞춰도 아무것도 안 생긴다는 것을 알게 되면 뭔가 빠진 것 같은 느낌이 든다. 그래서 이번에 제안하는 것은: JSON 파일에 두 줄만 추가해서 나머지 2가지 색 고양이에도 수집 카운터와 보상을 연결하자는 것이다.
