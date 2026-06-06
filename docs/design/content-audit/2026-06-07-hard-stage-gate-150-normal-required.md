# Content Audit — 2026-06-07 — 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Stage / Guide / Mission / Shop / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 13건 (가장 최근: 2026-06-05 UTC = 2026-06-06 KST)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39·47~51(23개) 제외; 스킨 변형 30종 포함 |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1 스킨 8종·아이템 2종, tapIndex 2 광고 제거 1종 |
| 고양이 스킨 | 6종 × Cat1~5 = 30개 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |
| **하드 스테이지 해금 조건** | **normalStage ≥ 150** | **ConstValue.json variable 3 = 150, UIStageSelect.cs:41-46** |
| 보스 스테이지 해금 조건 | hardStage ≥ 50 | ConstValue.json variable 4 = 50 |

### 분포 공백

- **하드 해금 임계값 = 노멀 전체 수(150)** — 노멀 스테이지가 총 150개이므로 모든 노멀 스테이지를 완료해야 하드 스테이지 접근 가능. 타 모바일 매치-3 장르 통상 기준(전체 20~40% 클리어 후 신규 모드 개방)과 비교 시 극단적으로 높은 진입 문턱.
- 보스 해금(50/150 = 33%)과의 내부 불균형 — 하드(100%)·보스(33%) 간 해금 기준이 역설적으로 보스 모드가 더 접근하기 쉬운 비율을 시사.
- 노멀 스테이지 중후반(그룹 10~15, 스테이지 100~150)에서 이탈한 플레이어는 영구적으로 하드·보스 콘텐츠 접근 불가.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 (KST) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 추가 제안 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 (10%→30~40% 확대) |
| 2026-05-30 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 (EDailyCounter.HardStageClear 신설) |
| 2026-05-31 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-06-01 | 4a09a70 | RainbowPang tapIndex 1 반복 미션 누락 |
| 2026-06-02 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-06-03 | 41f1f81 | 일일 미션 보상 불균형 — AdWatch vs BlockDestroy |
| 2026-06-04 | 1d91056 | 상점 스킨 골드 가격 선형 계단 (최고가 100일 소요) |
| 2026-06-05 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-06 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### [권장] HardStage_NormalStageLock 임계값 완화 — 150 → 50

- **카테고리**: 스테이지 잠금 밸런스 / 진입 장벽
- **요지**: ConstValue.json의 HardStage_NormalStageLock 값이 150으로 설정되어 있어 노멀 스테이지 150개 전체를 클리어해야 하드 스테이지가 해금된다. 보스 스테이지 해금(hardStage ≥ 50, 전체 33%)과 비교해 하드 해금 조건(100%)이 비정상적으로 높아 중반 이탈 유저의 콘텐츠 다양성이 차단된다.
- **점수**: 검증가치 5 / 구현비용 1 / 플레이어경험개선 5 / 데이터근거 5 → 종합 **20**
- **근거**:
  - `Assets/AssetBundleResources/json/ConstValue.json` line 5: `{"variable":3, "value":150}` — EConstValue.HardStage_NormalStageLock = 150
  - `Assets/Scripts/UI/UIStageSelect.cs` line 41-46: `if (hardLockValue > loginData.normalStage)` → 150 > normalStage 시 하드 버튼 잠금
  - `Assets/Scripts/Data.cs` line 16-18: Data.Login.normalStage / hardStage / bossStage 각각 독립 추적
  - `Assets/AssetBundleResources/json/ConstValue.json` line 6: variable 4 (BossStage_HardStageLock) = 50 — 보스 해금 임계값 50 (비율 33%)
  - 노멀 스테이지는 총 150개(Stage.json group < 100000 행 수 = 150)이므로 임계값 150 = 100% 완료 필수

#### 유저 플로우

1. **노출 시점·트리거**: 로비에서 UIStageSelect를 열 때마다 `UIStageSelect.Start()`가 실행되어 normalStage 카운트와 임계값을 즉시 비교한다. 현재 기준으로는 노멀 스테이지 1~149를 클리어한 유저가 하드 버튼을 탭하면 UIAlarm이 팝업되며 "150스테이지 클리어 필요" 안내가 표시된다. 임계값을 50으로 완화하면 노멀 스테이지 50개 클리어 직후(대략 1~2주 캐주얼 플레이)부터 하드 버튼이 활성화된다.

2. **화면 변화**: UIStageSelect 프리팹 내 `hardLockObj`(자물쇠 UI)가 normalStage < 임계값일 때 활성화된다. 임계값 완화 후에는 더 이른 시점에 `hardLockObj.SetActive(false)`가 호출되어 자물쇠 아이콘이 사라지고 하드 스테이지 버튼 라벨이 정상 표시된다. 보스 잠금(`bossLockObj`)은 별도 조건(hardStage ≥ 50)이므로 영향 없음.

3. **입력 행동**: 잠금 해제 전 하드 버튼을 탭하면 UIAlarm(stringID=110, intValue=hardLockValue)이 표시된다. ConstValue.json의 값만 변경하면 stringID=110의 메시지가 `{intValue}스테이지 클리어` 형식으로 자동 갱신되므로 UI 코드 수정 없이 안내 문구가 "50스테이지 클리어"로 바뀐다. 잠금 해제 후에는 하드 버튼 탭 → `arg.stageSelect?.Invoke(ESelectStage.Hard)` → UIStageSelect 닫힘 → 하드 스테이지 목록 진입.

4. **시스템 반응**: `CHMJson.Instance.GetConstValueInfo(EConstValue.HardStage_NormalStageLock)` 반환값이 50으로 줄어들면 `UIStageSelect.cs:42`의 조건 `if (hardLockValue > loginData.normalStage)`가 normalStage ≥ 50인 유저에 대해 false가 되어 hardStageLock=false, hardLockObj 비활성화가 이루어진다. Data.Login.normalStage는 노멀 스테이지 클리어 시 자동 증가하며 로컬·클라우드 저장되므로 변경 즉시 기존 유저에게도 소급 적용된다.

5. **반복·재발생 패턴**: 노멀 스테이지를 클리어할 때마다 normalStage가 누적된다(재클리어 포함). 유저가 노멀 50을 클리어하는 순간 다음 번 UIStageSelect 오픈 시 자물쇠가 해제된다. 하드 스테이지를 플레이한 뒤 노멀로 돌아가는 것도 자유이므로 두 모드를 번갈아 플레이하는 패턴이 자연스럽게 형성된다.

6. **종료·해소 조건**: normalStage가 임계값 이상이 되면 잠금이 영구 해제된다. 이후 UIStageSelect를 열 때마다 하드 버튼은 항상 활성 상태. 보스 잠금(hardStage ≥ 50)은 별도로 유지되어 하드 스테이지 진행 없이는 보스 모드를 진입할 수 없다는 단계적 구조는 보존된다.

7. **다른 시스템과 상호작용**: 일일 미션 NormalStageClear 목표(3회/일, missionID=101)는 변경 없이 유지. 하드 스테이지가 열리면 과거 감사(2026-05-30, d819b99)에서 제안된 EDailyCounter.HardStageClear 미션 신설과 연계 가능성이 생긴다. 상점 AddMove/AddTime 아이템(shopID 4·5)의 실용 범위도 늘어나 구매 동기가 자연스럽게 증가한다. ESelect 스킬 선택(Power/Delay 등)도 하드 스테이지에서 더 자주 활용되어 어제 감사(2026-06-06, 7d601d5)의 ESelect 미션 제안과 시너지가 발생한다.

8. **엣지 케이스**: Data.Login.normalStage가 재클리어 포함 누적 카운트인지 최대 달성 스테이지 번호인지에 따라 50 달성 속도가 달라진다. 재클리어 누적이라면 같은 스테이지를 50번 반복해도 해금될 수 있으므로 "고유 스테이지 클리어 수" 기준으로 추가 확인이 필요하다. 또한 기존에 normalStage ≥ 150인 유저는 임계값 완화 후에도 이미 해금 상태이므로 영향 없음.

9. **유저 정보·피드백**: UIAlarm stringID=110은 이미 "{intValue}스테이지를 클리어하세요" 형태의 안내를 제공한다. 임계값 변경 시 추가 UI 작업 없이 숫자만 바뀐다. 하드 스테이지 해금 시점에 "하드 스테이지 해금!" UIAlarm 또는 연출을 추가하면 중간 목표 달성감을 강화할 수 있다. 현재 잠금 해제 전용 피드백(해금 순간 연출)은 구현되지 않아 유저가 자연스럽게 인지하지 못할 수 있다.

### 보류

- **AddMove 아이템 효율 (AddMoveItemValue=1) 개선**: ConstValue.json variable 5 = 1. 이동 제한 스테이지에서 아이템 1개 소모 시 +1회만 추가되어 효과가 미미할 수 있다(AddTime=10초 대비). 데이터 근거는 충분하나 이번 회차 채택 스킵.
- **Data.Login.attack 미활용 필드**: GPBossController.cs에서 loginData.hp는 사용하지만 loginData.attack=0은 참조하지 않음. 플레이어 공격력 업그레이드 시스템의 미완성 흔적으로 보이나 별도 설계 검토 필요.

## 3. 과거 감사 대비 차별성

git log 13건 검토 완료.

가장 유사했던 과거 커밋: `d819b99` (2026-05-30) "하드 스테이지 클리어 일일 미션 없음 — EDailyCounter.HardStageClear 신설 제안" — 하드 스테이지 관련이지만 해당 감사는 하드 스테이지 진입 이후의 일일 미션 공백을 다룬다. 이번 감사는 하드 스테이지에 **진입하기 전** 해금 조건 자체의 과도함을 다루므로 계층이 다르다.

두 번째로 유사한 커밋: `08f1ddb` (2026-05-28) "하드 스테이지 100개 완전 균일 포맷" — 하드 스테이지 내부 다양성 제안이며 잠금 조건과 무관.

이번 감사의 차별점: ConstValue.json의 수치 1줄 변경만으로 해결 가능한 가장 낮은 구현비용(1점)과 가장 높은 플레이어 영향(5점)의 조합. 과거 13건 중 진입 장벽(게이트 임계값)에 직접 집중한 감사는 없었다.

## 4. 다음 단계 제안

- 채택 시 ConstValue.json variable 3 값 150 → 50 변경 및 검증
- 하드 스테이지 해금 연출(UIAlarm 또는 전용 팝업) 추가 검토
- normalStage 카운트 방식(재클리어 중복 허용 여부) 코드 확인 후 50 적정성 재평가
- 일일 미션 HardStageClear 신설(과거 감사 d819b99)과 패키지로 기획 검토

## 5. 쉬운 설명 (비개발자 요약)

캣팡에는 세 가지 난이도가 있다: 노멀, 하드, 보스. 지금은 노멀 스테이지 150개를 전부 다 깨야만 하드 스테이지 버튼이 열린다. 150개를 모두 깨는 건 아주 오래 걸리는 일이라, 중간에 게임을 그만두는 플레이어는 하드 스테이지를 한 번도 구경하지 못한 채 떠나게 된다. 반면 보스 스테이지는 하드 스테이지 50개(전체 3분의 1)만 깨면 열린다. 그래서 이번에 제안하는 것은: 하드 스테이지 해금 조건을 150개에서 50개로 낮춰 더 많은 플레이어가 다양한 콘텐츠를 경험할 수 있게 하는 것이다.
