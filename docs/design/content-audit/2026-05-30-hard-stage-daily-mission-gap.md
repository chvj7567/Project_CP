# Content Audit — 2026-05-30 — 하드 스테이지 클리어 일일 미션 없음 — EDailyCounter 공백 보완 제안

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 10개 (Mission.json, Shop.json, Stage.json, StageBlock.json, ConstValue.json, Guide.json, Tutorial.json, StringKorea.json, StringEnglish.json, Select.json)
- 과거 감사 이력 (git log): 5건 (가장 최근: 2026-05-29)

---

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 61종 / 전체 84 | 빈 슬롯 25~39(15개), 47~51(5개) = 20개 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + collectionType 기반 1종 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch + CatPang 생성 |
| 상점 아이템 | 12개 | 스킨 7, 소모성 IAP 2, 골드 소비 2, 광고 제거 1 |
| 고양이 스킨 | 6종 (Crown/Flowers/Mushroom/Party/Santa/Strawberry) | EBlockState 54~83, 각 테마 Cat1~5 |

### 분포 공백

#### 게임 모드 시간 vs 이동 전환 추세 (하드 기준)
| 그룹 | 시간제한 스테이지 수 | 이동제한 스테이지 수 | Both |
|---|---|---|---|
| 1 (stage 1~10) | 8 | 1 | 1 |
| 6 (stage 51~60) | 6 | 1 | 3 |
| 11 (stage 101~110) | 3 | 7 | 0 |
| 13 (stage 121~130) | 0 | 10 | 0 |
| 15 (stage 141~150) | 0 | 10 | 0 |

초반 그룹(1~6): 시간제한 지배 / 후반 그룹(11~15): 이동제한 완전 지배 — 후반에서 시간제한 모드 전면 소멸.

#### StageBlock 목표 블록 비중 (hp>0 기준)
| 블록 타입 | 목표 배치 수 | 비율 |
|---|---|---|
| Wall (16) | 504 | ~51% |
| Potal (17) | 168 | ~17% |
| PotalCreator (46) | 139 | ~14% |
| RainbowPang (52) | 139 | ~14% |
| CatBox 1~5 합산 | 154 | ~15% |
| WallCreator (45) | 48 | ~5% |
| 기타 (Arrow/Cat/Bomb 등) | 22 | ~2% |

Wall 블록이 전체 목표 블록의 절반을 차지.

#### 일일 미션 현황 (tapIndex=3)
| missionID | 카운터 종류 | 목표값 | 보상 |
|---|---|---|---|
| 100 | Attendance (출석) | 1 | Gold 100 |
| 101 | NormalStageClear | 3 | Gold 300 |
| 102 | BlockDestroy | 100 | AddTime 1 |
| 103 | AdWatch | 1 | AddMove 10 |
| 104 | CatPang 생성 (collectionType 18) | 3 | Gold 200 |

**HardStageClear 카운터 없음** — EDailyCounter에 `HardStageClear` 항목이 존재하지 않아, 하드 모드만 플레이하는 플레이어는 missionID 101 달성 불가.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 |
|---|---|---|
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 — 그룹 13·15 시간제한 스테이지 추가 제안 |
| 2026-05-29 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 — 보드 크기·이동제한 다양화 제안 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 제안 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 제안 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 — 10%→30~40% 확대 제안 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### 하드 스테이지 클리어 일일 미션 추가 (EDailyCounter.HardStageClear 신설)

- **카테고리**: 미션
- **요지**: EDailyCounter에 `HardStageClear = 4` 항목을 추가하고, 하루 2회 하드 스테이지 클리어를 목표로 하는 일일 미션(missionID 105)을 신설. 하드 모드만 플레이하는 고참 플레이어가 현재 "일일 스테이지 클리어" 미션을 달성할 방법이 없는 리텐션 공백을 보완한다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 4/2/4/4 → 종합 **16**
- **근거**:
  - `Assets/Scripts/Defines.cs` 382행 — `EDailyCounter` 열거형에 `HardStageClear` 항목 없음. `NormalStageClear = 1`만 존재.
  - `Assets/Scripts/Data.cs` 31행 — `stageClearCountToday`(노멀 전용). `hardStageClearCountToday` 필드 없음.
  - `Assets/AssetBundleResources/json/Mission.json` 25행 — missionID 101 `dailyCounter:1` = NormalStageClear, clearValue:3. 하드 전용 미션 행 없음.
  - `Assets/AssetBundleResources/json/Stage.json` — 하드·노멀 공유 150 스테이지 존재 (group 1~15). 하드 진행도(`Data.Login.hardStage`)는 이미 추적 중이므로 클리어 판정 로직이 존재함.

#### 유저 플로우

1. **노출 시점·트리거**
   UIMission 화면 tapIndex 3(일일 미션 탭) 진입 시, 기존 5개 미션 하단에 새 항목 "하드 스테이지 2판 클리어"가 노출된다. 일일 리셋 자정(NTP 기준)마다 진행 카운터가 0으로 초기화된다. 하드 모드 첫 클리어 직후 푸시 알림(UIMission 뱃지)이 표시되어 진행 현황을 알린다.

2. **화면 변화**
   미션 항목이 "0/2" → "1/2" → "2/2" 로 실시간 업데이트된다. 달성 시 체크 표시와 함께 보상 수령 버튼이 활성화된다. UIMission 탭 아이콘에 표시되는 완료 카운트가 기존 5개 체계에서 6개로 확장된다.

3. **입력 행동**
   플레이어는 UIStageSelect에서 하드 스테이지를 선택해 게임을 플레이한다. 스테이지 클리어(EGameState.GameClear) 시 자동으로 카운터가 증가한다. 보상 수령은 UIMission에서 버튼 탭 1회로 완료된다.

4. **시스템 반응**
   GPGameScene에서 GameClear 판정 시 ESelectStage.Hard(=1) 조건 분기를 추가해 `DailyMissionService.OnHardStageClear()`를 호출한다. 이 메서드는 `Data.Login.hardStageClearCountToday`를 +1 하고 CHMData.SaveData()를 트리거한다. `clearValue:2` 도달 시 EClearState.Clear로 전환, 보상 수령 가능 상태가 된다.

5. **반복·재발생 패턴**
   매일 자정 NTP 기준으로 DailyMissionService.CheckAndResetIfNeeded() 호출 시 `hardStageClearCountToday = 0` 리셋. 클리어된 스테이지를 재도전해도 카운트 증가(재클리어 포함 — NormalStageClear와 동일 정책). 하루 2판 목표이므로 과제 부담이 낮아 매일 반복 유도가 쉽다.

6. **종료·해소 조건**
   하루 2회 하드 스테이지 클리어 완료 후 보상 수령 버튼 탭으로 미션 종료. 보상 수령 전 자정을 넘기면 보상 기회는 소멸되고 카운터는 0 리셋된다(기존 일일 미션과 동일 정책). 보상 미수령 경고 알림은 별도 기획 범위.

7. **다른 시스템과 상호작용**
   - **CHMTime**: 자정 리셋 판정은 기존 DailyMissionService 인프라를 그대로 활용.
   - **ESelectStage**: GPGameScene이 이미 ESelectStage.Hard/Boss/Normal을 분기 처리하므로 Hard 분기에 카운터 훅만 추가.
   - **GPGS 클라우드 저장**: Data.Login에 `hardStageClearCountToday` 필드 추가 시 기존 클라우드 동기화 흐름에 자동 포함.
   - **UIMission UI**: 기존 MissionScrollViewItem 재사용, missionID 105 데이터 행만 추가.

8. **엣지 케이스**
   - NTP 미수신 상태에서는 리셋을 수행하지 않으므로(기존 DailyMissionService 보호) 시각 위변조로 인한 반복 수령 불가.
   - 보스 스테이지 클리어는 HardStageClear에 포함하지 않는다(보스는 별도 모드, ESelectStage.Boss=2).
   - 노멀 모드(ESelectStage.Normal=3) 클리어는 기존 NormalStageClear 카운터만 증가, HardStageClear에는 포함하지 않는다.
   - clearValue를 3(NormalStageClear와 동일)으로 설정하면 하드 모드가 더 어려우므로 불균형. 2회가 적절.

9. **유저 정보·피드백**
   진행 중 카운트는 MissionScrollViewItem 내 텍스트로 실시간 표시된다. 클리어 후 GPGameScene의 UIGameEnd에 "일일 미션 달성!" 토스트 메시지를 추가하면 즉각 피드백 제공 가능(별도 기획 검토 사항). 보상 Gold 양은 NormalStageClear(300) 대비 하드가 난이도 높으므로 400~500 Gold 설정 검토를 권장한다.

### 보류

- **Wall 블록 목표 과점유(51%) 완화**: Wall이 전체 목표 블록의 51%를 차지해 후반 스테이지가 단조롭다는 분석 가능. 그러나 구현비용(스테이지 데이터 대규모 수정)이 높아 이번 회차 채택 보류.
- **AdWatch 일일 보상 리밸런싱**: AddMove×10이 PassiveIncome 성격으로 과하다는 판단 가능. 그러나 광고 수익 모델과 직결되어 기획 민감도가 높아 보류.

---

## 3. 과거 감사 대비 차별성

git log 5건 전체 검토 완료.

| 과거 커밋 | 내용 요지 | 이번 제안과의 차별점 |
|---|---|---|
| a6cb70b | 그룹 13·15 시간제한 스테이지 추가 | 스테이지 데이터 구조 변경 제안. 이번 제안은 미션 시스템 EDailyCounter 신설. |
| 08f1ddb | 하드 스테이지 균일 포맷 다양화 | 스테이지 보드·이동제한 다양화. 이번 제안은 플레이어 행동 보상 시스템(미션). |
| b5bcc97 | tapIndex 1 Cat1~5 수집 미션 공백 | 장기 수집 미션 추가. 이번 제안은 일일(tapIndex 3) 미션 추가로 탭·주기 다름. |
| aeffad1 | tapIndex 2 PotalCreator/CatBox4 수집 미션 공백 | 장기 수집 미션 추가. 이번 제안은 일일 미션이며 카테고리 완전 다름. |
| 58c3f65 | 보스 스테이지 CatBox 배분 확대 | 보스 AI 스킬 밸런스. 이번 제안은 모드별 일일 미션 리텐션 구조. |

가장 유사한 과거 커밋: **b5bcc97** (미션 공백 제안) — 차별점: b5bcc97은 tapIndex 1 장기 수집 미션이고, 이번 제안은 tapIndex 3 일일 카운터 기반 미션 신설이며 EDailyCounter enum 확장을 포함한다는 점에서 구현 범위와 리텐션 목적이 다르다.

---

## 4. 다음 단계 제안

채택 시 아래 3가지 작업이 필요하다 (구현비용 낮음, 예상 총 1~2일):

1. `Assets/Scripts/Defines.cs` — `EDailyCounter`에 `HardStageClear = 4` 추가
2. `Assets/Scripts/Data.cs` — `Data.Login`에 `public int hardStageClearCountToday = 0;` 추가
3. `Assets/AssetBundleResources/json/Mission.json` — missionID 105 행 추가 (`tapIndex:3, dailyCounter:4, clearValue:2, reward:0(Gold), rewardCount:400`)
4. `Assets/Scripts/GamePlay/GPGameScene.cs` — GameClear 판정 시 ESelectStage.Hard 분기에 `DailyMissionService.OnHardStageClear()` 훅 추가
5. 선택: StringKorea.json / StringEnglish.json에 missionID 105 설명 문자열 추가

---

## 5. 쉬운 설명 (비개발자 요약)

캣팡에는 매일 주어지는 숙제 5가지가 있다 — 출석, 스테이지 3판 깨기, 블록 100개 깨기, 광고 보기, 고양이 폭탄 3번 만들기. 그런데 "스테이지 3판 깨기"는 쉬운 노멀 모드만 인정된다. 어려운 하드 모드를 열심히 하는 골수 팬은 같은 스테이지를 열 번 깨도 이 숙제가 안 끝난다. 노멀 모드로 잠깐 갔다가 다시 하드로 와야 하는 번거로움이 생기는 것이다. 그래서 이번에 제안하는 것은: 하드 모드 2판 클리어를 새 일일 숙제로 추가해, 하드 팬들도 매일 도장 받을 수 있게 하자.
