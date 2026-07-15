# Content Audit — 2026-06-19 — AddTime·AddMove 두 아이템 보스 스테이지 완전 무효

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (Stage, StageBlock, Mission, Shop, ConstValue, Guide, Tutorial, StringKorea, StringEnglish)
- 과거 감사 이력 (git log): 24건 (가장 최근: 2026-06-17 UTC / 2026-06-18 KST)

## 1. 현황
| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 약 28종 / 전체 84 | 빈 슬롯(25~39, 47~51) 제외. Cat1~7, Arrow1~6, Wall, Potal, CatPang, PinkBomb~BlueBomb, Fish, CatBox1~5, WallCreator, PotalCreator, RainbowPang, Ball |
| 일일 미션 종류 | EDailyCounter 4종 | Attendance(1), NormalStageClear(3), BlockDestroy(100), AdWatch(1) |
| 상점 아이템 | 12개 | tapIndex 1: 스킨 7종 + 기타 2종(gold 5000), tapIndex 2: RemoveAD, AddTime(IAP), AddMove(IAP) |
| 고양이 스킨 | 6종 | CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry (각 Cat1~5) |
| 소모성 아이템 | AddTime(+10초), AddMove(+1회) | Shop.json shopID 4, 5. IAP 구매 또는 일일 미션 보상으로 획득 가능 |

### 분포 공백

**보스 스테이지(100개, Stage.json group >= 100000) 전체 구조:**
- `time = -1.0` — 시간 제한 없음 (전 보스 스테이지 일관)
- `moveCount = -1` — 이동 횟수 제한 없음 (전 보스 스테이지 일관)
- 실패 조건: `EFailReason.HpOver` (GPBossController: 플레이어 HP 100에서 1/초 감소)

**소모성 아이템 효과 적용 범위:**
- `AddTime` (EConstValue.AddTimeItemValue = 10초): `time > 0`인 스테이지에만 유효 → 보스 스테이지 100개에서 **완전 무효**
- `AddMove` (EConstValue.AddMoveItemValue = 1회): `moveCount > 0`인 스테이지에만 유효 → 보스 스테이지 100개에서 **완전 무효**

**일일 미션 보상과의 연계 단절:**
- missionID 102 (BlockDestroy 100개 달성): reward=1 (AddTime 1개) → 보스 위주 플레이어는 이 보상이 무용
- missionID 103 (AdWatch 1회): reward=2 (AddMove 10개) → 보스 위주 플레이어는 이 보상이 무용

### 과거 감사 후보 (git log 조회 결과)
| 날짜(UTC) | 커밋 SHA | 설명 |
|---|---|---|
| 2026-06-17 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 |
| 2026-06-16 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 |
| 2026-06-15 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 |
| 2026-06-14 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 |
| 2026-06-13 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 |
| 2026-06-11 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 |
| 2026-06-10 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 |
| 2026-06-09 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 |
| 2026-06-08 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 |
| 2026-06-07 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 |
| 2026-06-06 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 |
| 2026-06-05 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 |
| 2026-06-03 | 1d91056 | 상점 스킨 골드 가격 선형 계단 |
| 2026-06-02 | 41f1f81 | 일일 미션 보상 불균형 |
| 2026-06-01 | 1ea1f97 | Arrow 폭탄 tapIndex 3 일일 미션 완전 누락 |
| 2026-05-31 | 4a09a70 | 특수폭탄 계열 tapIndex 1 불일치 |
| 2026-05-30 | 23f1382 | Wall·Potal tapIndex 2 장기 파괴 미션 완전 누락 |
| 2026-05-29 | d819b99 | 하드 스테이지 클리어 일일 미션 없음 |
| 2026-05-29 | 58c3f65 | 보스 스테이지 CatBox 스킬 극소 배분 |
| 2026-05-29 | aeffad1 | PotalCreator·CatBox4 tapIndex 2 장기 수집 미션 공백 |
| 2026-05-29 | b5bcc97 | 미션 tapIndex 1 Cat1~5 기본 블록 수집 미션 공백 |
| 2026-05-28 | 08f1ddb | 하드 스테이지 100개 완전 균일 포맷 |
| 2026-05-28 | a6cb70b | 스테이지 후반 시간제한 모드 공백 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### AddTime·AddMove 두 소모성 아이템이 보스 스테이지(100개) 전체에서 완전 무효 — 보스 전용 아이템 효과 설계 부재

- **카테고리**: 아이템 / 보스 스테이지
- **요지**: Shop.json의 소모성 아이템 AddTime(+10초)·AddMove(+1회)는 `time > 0` 또는 `moveCount > 0`인 스테이지에서만 효과를 발휘하는데, Stage.json의 보스 스테이지 100개는 전부 `time=-1, moveCount=-1`이므로 두 아이템 모두 보스 스테이지에서 기능하지 않는다. 전체 400스테이지 중 25%에서 구매한 IAP 아이템을 전혀 쓸 수 없는 구조다.
- **점수**: 검증가치/구현비용/플레이어경험/데이터근거 = 5/3/5/5 → 종합 18
- **근거**:
  - `Assets/AssetBundleResources/json/Stage.json` — 보스 스테이지(group >= 100000) 전체: `"time": -1.0, "moveCount": -1` 일관 확인
  - `Assets/AssetBundleResources/json/Shop.json` — shopID 4(AddTime, IAP), shopID 5(AddMove, IAP): tapIndex 2 소모성 아이템
  - `Assets/AssetBundleResources/json/ConstValue.json` — variable 5 (AddMoveItemValue=1회), variable 6 (AddTimeItemValue=10초)
  - `Assets/AssetBundleResources/json/Mission.json` — missionID 102 보상 AddTime 1개(reward=1), missionID 103 보상 AddMove 10개(reward=2) → 보스 위주 플레이어에게 가치 없는 보상 구조
  - `Assets/Scripts/GamePlay/GPBossController.cs` L66-68 — `hp.Value -= 1` per second (HP drain만 존재, 게임 타이머 없음)
  - `Assets/Scripts/Defines.cs` — `EFailReason.HpOver` (보스 실패 조건), `EGameState.BossStagePlay`

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   하드 스테이지 50개를 클리어하면 (ConstValue variable 4=50) 보스 스테이지가 해금된다. 보스 스테이지는 UIStageSelect에서 Boss 탭으로 진입 가능하며, 난이도가 높다고 판단한 플레이어는 Shop에서 AddTime이나 AddMove를 IAP로 구매하거나, 일일 미션(missionID 102·103) 보상으로 보유한 아이템을 들고 보스 스테이지에 진입을 시도한다.

2. **화면 변화**
   보스 스테이지 진입 시 UIGameStart가 열리고 ESelect 스킬 선택 후 게임이 시작된다. GPBossController가 초기화되어 플레이어 HP 바(100)와 보스 HP 게이지가 화면에 표시된다. 노멀/하드와 달리 남은 이동 횟수 카운터와 게임 타이머는 UI에 나타나지 않는다 (moveCount=-1, time=-1).

3. **입력 행동**
   플레이어는 블록을 드래그해 매치를 만들며 보스 HP(targetScore)를 줄이려 한다. 시간이 촉박하거나 이동이 부족하다는 느낌에 보유 중인 AddTime 또는 AddMove를 쓰려 하지만, 아이템 사용 버튼이 비활성화되어 있거나 누르더라도 화면에 아무 반응이 없다.

4. **시스템 반응**
   AddTime: 게임 타이머에 +10초를 더해야 하나 보스 스테이지에는 타이머가 존재하지 않아 효과 발동 불가. AddMove: 이동 횟수 카운터에 +1을 더해야 하나 보스 스테이지에는 이동 제한이 없어 효과 발동 불가. 아이템은 소비되지 않거나 소비돼도 아무 변화도 일어나지 않는다. GPBossController의 HP drain(`hp.Value -= 1` per second)은 계속 진행되어 플레이어 압박은 유지된다.

5. **반복·재발생 패턴**
   보스 스테이지 100개 전체(group 100001~100010, stage 100001~100100)에서 동일하게 발생한다. 일일 미션 missionID 103(AdWatch 보상 AddMove 10개)을 매일 달성하는 플레이어가 보스 스테이지를 주로 플레이하면 AddMove가 매일 쌓이지만 사용처가 없다. 이 상황이 매일 반복되어 아이템 창고가 넘쳐나지만 실질적 가치를 못 느끼게 된다.

6. **종료·해소 조건**
   현재 설계에서는 해소 조건이 없다. 보스 스테이지 클리어 조건은 보스 HP(targetScore)를 매치로 소진하는 것이고, 실패 조건은 EFailReason.HpOver(플레이어 HP 0)다. AddTime·AddMove 아이템이 이 두 조건 어느 쪽에도 개입할 수 없어 보스 스테이지 내내 아이템이 무의미하다.

7. **다른 시스템과 상호작용**
   - **EReward 시스템**: missionID 102(BlockDestroy)의 AddTime 보상과 missionID 103(AdWatch)의 AddMove 보상이 보스 위주 플레이어에게 실질 가치 없는 보상이 되어 미션 완료 만족도가 떨어진다.
   - **IAP 시스템**: AddTime·AddMove는 실 결제 상품(shopID 4, 5)인데 100스테이지에서 효과 없음 → 구매 의욕 훼손, 앱스토어 리뷰 리스크.
   - **Data.Login**: `addTimeItemCount`/`addMoveItemCount`는 증가하지만 보스 스테이지에서 소비되지 않아 카운터가 비정상적으로 누적될 수 있다.
   - **GPBossController HP drain**: 플레이어는 아이템으로 HP 감소를 막을 방법이 없고, 보스 스킬(Wall/Creator/CatBox)이 보드를 어렵게 만들수록 아이템 무효화의 체감 좌절이 커진다.

8. **엣지 케이스**
   - 노멀/하드 스테이지 중 time > 0인 시간제한 스테이지나 moveCount > 0인 이동 제한 스테이지에서는 AddTime/AddMove가 유효하므로, 보스와 교차 플레이 시 아이템 효과 인지가 혼재된다.
   - 하드 50개 미만의 초반 플레이어는 보스 해금 전이므로 이 문제를 인지하지 못한다.
   - 보스 스테이지 중 가장 어려운 구간(`stage % 10 == 0`, GPBossController L87: Wall+Creator+CatBox 3중 스킬 동시 발동)에서 아이템 사용 욕구가 가장 높지만, 이때도 AddTime·AddMove는 무효다.
   - 일부 하드 스테이지(Stage.json에서 time=-1, moveCount > 0인 스테이지)는 AddTime 무효 + AddMove 유효가 섞여 있어 "이동 제한 스테이지"에서만 AddMove 사용 학습이 이뤄진다.

9. **유저 정보·피드백**
   보스 스테이지에서 반복 실패한 플레이어는 "아이템을 샀는데 보스에서 못 쓴다"는 불만을 앱스토어 리뷰나 커뮤니티에 남길 수 있다. IAP 구매 후 기대 효용이 충족되지 않는 경험은 환불 요청이나 구매 중단으로 이어질 수 있다. 반대로 일일 미션 보상으로 AddTime/AddMove를 계속 받지만 쌓이기만 하는 보스 플레이어는 미션 보상 체감 만족도가 낮아져 일일 복귀 동기가 약해진다.

### 보류
- **tapIndex 2 이정표 Ball(131개) 완료 후 장기 목표 고갈 — Arrow 폭탄 이정표 신설**: 검증가치 3, 구현비용 2, 플레이어경험 3, 데이터근거 4 → 종합 15. 과거 2026-06-10, 2026-06-14에 "특정 블록 tapIndex 2 이정표 부재" 카테고리 제안 있어 패턴 중복 우려.
- **보스 스테이지 전용 일일 미션(BossStageClear) 부재**: 검증가치 3, 구현비용 2, 플레이어경험 3, 데이터근거 4 → 종합 15. 과거 2026-05-29 "하드 스테이지 클리어 일일 미션 없음"과 같은 카테고리(스테이지 클리어 일일 미션) 부분 중복.

## 3. 과거 감사 대비 차별성
- git log 24건 검토 완료.
- 가장 유사했던 과거 커밋:
  - `7528a26` (2026-06-09) — "아이템 사용(AddTime·AddMove) 미션 연계 공백": **차별점** = 해당 제안은 "아이템을 사용해도 미션 카운터에 집계되지 않는다"는 gamification 연계 누락이었다. 이번 제안은 "아이템 자체 기능(시간 연장/이동 추가)이 보스 스테이지에서 물리적으로 작동하지 않는다"는 아이템 효과 무효화로, 더 근본적인 설계 구조 결함이다.
  - `85ac09c` (2026-06-08) — "보스 스테이지 플레이어 HP 소진·회복 루프 미설계": **차별점** = 해당 제안은 HP를 회복할 경로(아이템·미션 보상)가 없다는 점이 핵심. 이번 제안은 AddTime·AddMove가 보스에서 효과 없음이 핵심으로, 해결 방향이 다르다 (HP 회복 신설 vs AddTime을 보스에서 HP 회복으로 전환).
- 두 과거 제안과 카테고리 일부 겹침이 있으나 요지·근거·해결 방향이 모두 상이하므로 채택 기준 충족.

## 4. 다음 단계 제안
채택 시 구현 선택지:
1. **(저비용)** 보스 스테이지에서 AddTime 사용 시 플레이어 HP +10 회복, AddMove 사용 시 보스 스킬 쿨타임 +10초 연장으로 분기 처리 (`GPGameScene`의 아이템 사용 핸들러에 `EGameState.BossStagePlay` 분기 추가).
2. **(고품질)** 보스 전용 소모 아이템(예: "HP 포션", "보스 방어막") 신설 — Shop.json 추가, EReward 확장, GPBossController 연동. 기존 AddTime/AddMove는 노멀/하드 전용으로 정리.
3. **(중간)** AddTime을 "유니버설 아이템"으로 재설계: 노멀/하드에서는 +10초, 보스에서는 플레이어 HP +10 회복. 스테이지 모드(EGameState) 자동 감지 적용.

## 5. 쉬운 설명 (비개발자 요약)

이 게임에는 어려운 판을 돌파하기 위해 돈을 내거나 노력으로 얻을 수 있는 아이템이 두 가지 있다. 하나는 "시간을 10초 더 늘려주는 아이템"이고, 다른 하나는 "블록을 한 번 더 움직일 수 있는 아이템"이다. 그런데 게임 전체의 4분의 1인 보스 스테이지 100개를 플레이하면, 이 두 아이템이 모두 아무 효과도 없다 — 보스 스테이지에는 시간제한도, 이동 횟수 제한도 없기 때문에 "늘려줄 시간"도 "늘려줄 횟수"도 애초에 존재하지 않는다. 열심히 모은 아이템이 보스 앞에서 쓸모없어지는 상황이 매일 반복되는 것이다. 그래서 이번에 제안하는 것은: 보스 스테이지에서도 이 아이템들이 실제로 도움이 되도록 효과를 바꾸거나(예: 시간 아이템 → HP 회복), 보스 전용 아이템을 새로 만드는 것이다.
