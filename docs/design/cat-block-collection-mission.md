# 기획서 — Cat1~5 기본 고양이 블록 수집 미션 (미션 탭 tapIndex 1)

> 작성 2026-05-29 · game-designer
> 입력: spec `docs/superpowers/specs/2026-05-29-cat-block-collection-mission-design.md` · plan `docs/superpowers/plans/2026-05-29-cat-block-collection-mission.md` · 감사 `docs/design/content-audit/2026-05-29-cat-block-collection-mission-gap.md`

---

## § 헤더

- **목표**: 미션 탭 tapIndex 1 맨 앞에 Cat1~5 기본 고양이 블록 수집 미션 5개를 추가하고, 일반 고양이 블록 매치가 수집량으로 적립되도록 코드 보완한다.
- **검증 가설**: "매 스테이지 압도적으로 많이 매치되는 기본 고양이 블록에 수집 미션을 붙이면, 신규 플레이어가 미션 탭을 '내 플레이와 연결된 보상처'로 인지하고 초반 리텐션 루프가 강화되는가."
- **현재 단계 범위 적합성**: **범위 내**. 프로젝트 단계 Beta, stage_goal = "콘텐츠 완성 및 일일 미션·광고·IAP 리텐션 루프 안정화". 본 건은 기존 미션·수집 보상 루프의 콘텐츠 공백(Cat1~7 수집 미션 전무)을 메우는 작업으로 단계 목표에 직결한다.
- **핵심 메커니즘**: 매치 제거 지점에서 `Block.GetBaseCat`으로 정규화한 색별 수집량을 `Data.Collection`에 누적 → tapIndex 1에 missionID 18~22 추가 → 기존 `MissionScrollViewItem`(collectionType 기반)이 진행도·아이콘·반복보상을 변경 없이 처리.

---

## § 1. 핵심 제약 — "코드 변경 필수" 전제 (감사 보고서 정정)

감사 보고서(§7, §0 §2 근거란)는 "Cat1~7 수집 누적값이 이미 `Data.Collection`에 기록되고 있어 코드 변경 전혀 없음"이라 단정했으나 **이는 사실이 아니다.** 본 기획은 이 정정 위에 선다.

- 블록 수집 적립의 **유일한 경로**는 `GPBombResolver.SaveBombCollectionData`이며, 첫 줄 가드 `if (!block.IsBombBlock() && !block.IsSpecialBombBlock()) return;` 로 막힌다.
- `Cat1~7`은 `IsNormalBlock()`이라 이 가드를 통과하지 못한다 → **일반 고양이 블록 수집량은 적립된 적이 없다.**
- 기존 tapIndex 1 미션(Arrow·CatPang·특수폭탄)이 작동하는 것은 그 블록들이 모두 `IsBombBlock()/IsSpecialBombBlock()`이라 가드를 통과하기 때문이다.

**결론**: Mission.json만 추가하면 Cat 미션 진행도가 영원히 0에 머문다. `GPGameScene.RemoveMatchBlock`에 일반 고양이 블록 적립 로직을 추가하는 **코드 변경이 반드시 필요**하다. 본 기획서는 이 전제를 명시적으로 채택하며, 감사 보고서의 "코드 변경 없음" 전제를 폐기한다.

---

## § 2. 밸런스 — clearValue=100 / addValue=100 의 페이싱 근거 (결정 락)

### 2.1 결정값

| 항목 | 값 |
|---|---|
| clearValue (첫 목표) | 100 |
| addValue (반복 증가폭) | 100 |
| reward | 0 = `EReward.Gold` |
| rewardCount | 100 (Gold 100) |

### 2.2 페이싱 — 상수형 faucet (롱테일 아님)

미션 진행/반복 공식은 `MissionScrollViewItem`에서 다음과 같이 동작한다:

```
target   = clearValue + repeatCount × addValue   →  100, 200, 300, 400, …
progress = collectionData.value − startValue       (startValue는 미션 최초 노출 시 1회 스냅샷, 수령해도 리셋되지 않음)
```

target은 누적 목표선이고 progress는 누적 수집량이므로, **사이클당 한계 비용 = addValue = 100블록으로 항상 일정하다.** 감사 보고서 §5의 "점점 간격이 늘어나는 롱테일" 서술은 잘못이다 — 목표선은 커지지만 진행도도 같은 속도로 누적되므로, 매 사이클은 정확히 직전 사이클보다 100블록만 더 든다. 시간이 지나도 가속/감속 없는 **정속(定速) faucet**이다.

### 2.3 달성 주기 추정 (검산)

- 스테이지당 특정 색 고양이 블록 매치량 추정치: **색별 약 30~60개/스테이지** (9×9 보드 7색 분포·연쇄 매치 기준 추정. **측정값 아님 — qa-simulator로 확정 필요**).
- 사이클당 비용 100블록 기준 달성 주기:
  - `100 ÷ 60 ≈ 1.7 스테이지/사이클` (블록 빈도 높을 때)
  - `100 ÷ 30 ≈ 3.3 스테이지/사이클` (블록 빈도 낮을 때)
  - → **색별 약 2~3 스테이지마다 1회 달성, 주기 일정.**

### 2.4 왜 Arrow 미션(clearValue=10)과 다른가

| 블록군 | 등장 방식 | 스테이지당 빈도(추정) | clearValue | 달성 주기 목표 |
|---|---|---|---|---|
| Arrow1~6 (collectionType 10~15) | 플레이어가 의도적으로 4매치 등 조합해야 생성 | 색별 약 0~수 개 | 10 | 수 스테이지 |
| **Cat1~5 (collectionType 0~4)** | 기본 3매치로 항상 등장 | 색별 약 30~60개 | **100** | 약 2~3 스테이지 |

clearValue는 **블록 등장 빈도에 비례해 스케일링**한다. Cat 블록은 Arrow보다 약 10~20배 자주 매치되므로, clearValue를 10으로 두면 첫 스테이지 한 판에 5개 색 미션이 동시 수회 달성되어 보상 알림이 폭주(UX 저해)한다. clearValue=100은 빈도 차이를 흡수해 **"스테이지 단위 달성 주기"를 Arrow 미션과 비슷한 대역(수 스테이지)에 맞추기 위한 값**이다. 이것이 "왜 100이고 10이 아닌가"의 답이다.

### 2.5 골드 유입 모니터링 (데이터 우선)

5개 미션 × Gold 100, 각 색별 약 2~3 스테이지마다 달성 → 초반 골드 유입 추정:

```
검산: 5색 × (100 gold ÷ 2.5 스테이지) ≈ 200 gold/스테이지 (정속 주입)
범위: 5색 × (100 ÷ 1.7 ~ 100 ÷ 3.3) ≈ 약 150~290 gold/스테이지
```

이는 결정 락(100/100) 하에서 발생하는 **신규 정속 골드 소스**다. 수치 자체는 사용자 확정값이므로 변경하지 않으나, 초반 골드 인플레이션이 상점 가격·기존 골드 소스 균형에 미치는 영향은 경제 데이터 없이 단정할 수 없다.

→ **상점 경제 영향은 qa-simulator 검증 후 모니터링.** 결정 메트릭: ① 스테이지당 평균 골드 유입량, ② 기존 골드 소스(스테이지 클리어 보상·일일 미션·광고) 대비 본 미션의 비율, ③ 골드 사용처(상점 아이템 가격) 도달까지 스테이지 수 변화. 임계: 본 미션 골드가 기존 총 유입의 30%를 초과하면 후속 밸런스 사이클에서 clearValue 상향(예: 150~200) 재검토 — 단 본 기획 범위 밖, 별도 밸런스 조정 흐름으로 진행.

---

## § 3. 리스트 위치 / 노출 순서 / UX

### 3.1 위치 결정 — tapIndex 1 맨 앞 (missionID 18~22)

리스트 순서는 `CHMJson.GetMissionInfoList(tapIndex)`가 Mission.json 행 순서를 그대로 반환하므로, **배열 맨 앞(기존 missionID 1 앞)에 missionID 18~22를 두면 미션 탭 진입 시 Cat1~5 미션이 최상단 5개로 노출**된다.

### 3.2 기존 12개와의 노출 순서 영향

| 순서 | 미션군 | 비고 |
|---|---|---|
| 1~5 (신규) | **Cat1~5 기본 고양이** | 맨 위. 첫 스테이지부터 진행감 발생 |
| 6~11 | Arrow1~6 | 기존 |
| 12 | CatPang | 기존 |
| 13~17 | 5색 특수폭탄 | 기존 |

- **의도**: 신규/초반 플레이어가 탭을 열면 가장 먼저 보이는 5개가 "스테이지를 그냥 플레이만 해도 차오르는" 미션이어야 미션 탭이 자신의 플레이와 연결됨을 즉시 학습한다. Arrow·특수폭탄 미션은 의도적 조합이 필요해 신규에겐 즉각 달성이 어렵다 — 이들을 맨 위에 두면 "미션 탭 = 나랑 무관한 곳" 인상이 굳는다.
- **UX 영향(긍정)**: 첫 진입 시 상위 5개가 모두 진행 중(0/100에서 시작이나 첫 스테이지 직후 즉시 수치 상승)이라 진행감·재방문 동기 강화.
- **UX 영향(중립/관리 대상)**: 기존 Arrow·폭탄 미션의 화면 우선순위가 5칸 아래로 밀린다. 단 tapIndex 1은 스크롤 리스트(`MissionScrollView`, 풀링)이므로 스크롤로 모두 접근 가능 — 기능적 손실 없음. 헤비 플레이어가 Arrow 미션을 찾는 빈도가 신규 학습 가치보다 낮다고 판단해 신규를 위에 둔다.
- **달성 알림 빈도**: § 2.3 기준 색별 약 2~3 스테이지마다, 5색이 시차를 두고 달성되므로 초반 약 1 스테이지당 1~2회 알림. 과도하지 않은 수준(clearValue=10이면 알림 폭주). 알림은 수령 버튼 탭 시점의 `UIAlarm`(stringID 57, Gold 획득)으로 표시 — 자동 팝업이 아닌 능동 수령이므로 플레이 흐름 방해 없음.

---

## § 4. 시각 — 아이콘 / 진행 게이지

### 4.1 아이콘

`MissionScrollViewItem.GetMissionSpriteName`은 `collectionType != None`이면 `collectionType.ToString()`을 어드레서블 스프라이트 이름으로 로드한다.

- collectionType 0~4 → `EBlockState.Cat1~Cat5` → 스프라이트 이름 `"Cat1"` ~ `"Cat5"`.
- **신규 에셋 불필요**: `Cat1`~`Cat5` 스프라이트는 이미 게임 블록 렌더링에 쓰이며 어드레서블 등록되어 있다. 추가 검증으로, BlockDestroy 일일 미션이 이미 `"Cat3"` 아이콘을 로드하고 있어(`GetMissionSpriteName` default 분기) 로드 가능성이 실증됨.
- 각 미션 항목은 해당 색 고양이 블록 아이콘으로 표시되어 "어떤 색을 모으는 미션인지" 한눈에 구분된다.

### 4.2 진행 게이지 / 텍스트

- 진행 텍스트: `missionValueText.SetText(collectionData.value − startValue, clearValue)` → 예 "37 / 100". 라벨 prefix는 stringID 20 사용(기존 Arrow 미션과 동일).
- 수령 버튼: 목표 미달이면 비활성, `value − startValue >= target`이면 활성(`SetBtnInteractable`). 수령 시 `repeatCount++`로 다음 목표 200으로 갱신, 텍스트도 즉시 갱신.
- **신규 UI 작업 없음** — 기존 tapIndex 1(MissionTabNormal) 분기가 그대로 처리.

---

## § 5. 시너지 / 중복 점검

| 점검 항목 | 결과 |
|---|---|
| tapIndex 3 missionID 104 (CatPang 수집, collectionType 18, dailyCounter -1, clearValue 3) | **중복 없음.** 대상 블록이 `CatPang`(18, 특수 블록)이고 본 미션은 `Cat1~5`(0~4, 기본 블록). collectionType·tapIndex 모두 다름. 적립 경로도 분리(§6 이중 카운트 회피) |
| tapIndex 3 BlockDestroy 일일 미션 | 일일 미션은 `DailyMissionService` 카운터 기반(`Data.Collection` 아님). 본 미션은 `Data.Collection["Cat1~5"]` 기반. 적립 지점은 `RemoveMatchBlock`로 같으나 **저장소가 다른 dict**라 상호 간섭 없음 |
| tapIndex 1 Arrow/특수폭탄 미션 | 같은 탭·같은 반복 구조에 자연 편입. collectionType 0~4(신규) vs 10~23(기존) 겹치지 않음 |
| tapIndex 2 장기 수집 미션(Fish/CatBox 등) | collectionType 24·40·45·52·53 — 겹치지 않음 |

**시너지(긍정)**: 본 미션은 "스테이지 플레이 → 즉시 미션 진행 → 골드 → (장기적으로) 상점 스킨/아이템 구매" 루프의 가장 앞 단을 채운다. 스킨 고양이를 써도 § 6.1 합산 덕에 미션이 정상 진행되어, 스킨 구매 동기와 충돌하지 않고 오히려 "스킨 써도 미션 손해 없음"을 보장한다.

---

## § 6. 엣지 케이스

### 6.1 스킨 고양이 합산
`Block.GetBaseCat`은 스킨 고양이(CatCrown1~CatStrawberry5, 54~83)를 기본 색 Cat1~5로 환산한다(`Cat1 + (v − CatCrown1) % SkinCatCount`). 따라서 플레이어가 어떤 스킨을 장착해도 매치된 블록은 해당 기본 색 미션에 합산되어 정상 진행한다. 스킨 사용이 미션 진행을 막지 않는다.

### 6.2 Cat6 / Cat7 미션 부재
`GetBaseCat`은 Cat1~7 전부를 자기 자신으로 반환하므로 Cat6/Cat7 매치도 `Data.Collection["Cat6"/"Cat7"]`에 적립된다. 그러나 본 기획은 **Cat1~5 미션만** 추가한다(결정 락). Cat6/7 적립은 대응 미션이 없어 dict 엔트리만 생성될 뿐 무해하며, 추후 Cat6/7 미션 추가 시 누적값을 즉시 활용 가능(차순위, 본 범위 밖).

### 6.3 기존 플레이어 — 0에서 시작 (감사 §7/§8 무효화)
일반 고양이 블록 수집은 § 1대로 그간 추적된 적이 없다. 따라서 **모든 플레이어(고인물 포함)는 본 업데이트 직후 Cat1~5 수집량 0에서 시작**한다.

→ 감사 보고서 §7 "오래된 플레이어가 즉시 높은 달성률/즉시 달성", §8 "고인물 즉시 여러 단계 달성·과도한 골드 폭주", "긍정적 서프라이즈"는 **모두 '수집량이 이미 기록됨'이라는 잘못된 전제 위에 선 서술이므로 무효**다. 업데이트 직후 다단 자동 완료/보상 폭주 리스크는 존재하지 않는다.

### 6.4 startValue 스냅샷
미션 최초 노출 시(`clearState == NotDoing`) 현재 수집량을 `startValue`로 스냅샷하는 기존 동작 그대로(Arrow 미션과 동일). § 6.3대로 최초 노출 시점 수집량은 사실상 0이므로 스냅샷도 0 근처에서 시작한다.

### 6.5 알림 빈도 하한
clearValue=100은 § 2.4대로 알림 폭주를 막는 하한이기도 하다. 만약 향후 더 자주 매치되는 색(보드 분포 편향)이 발견되어 특정 색만 1 스테이지에 다회 달성되면 § 2.5 모니터링 메트릭으로 포착해 후속 조정.

---

## § 구현 요청사항 (gameplay-programmer 용)

> 본 기획은 신규 Enum/Interface/프리팹/SO를 만들지 않는다. 아래에 N/A 항목도 명시적으로 기록한다(누락 아님).

### 7.1 Enum
- **신규 Enum 불필요.** collectionType은 기존 `Defines.EBlockState.Cat1~Cat5`(정수 0~4)를 재사용한다.
- reward는 기존 `Defines.EReward.Gold`(정수 0)를 재사용한다.

### 7.2 Interface
- **신규 Interface 없음.**

### 7.3 에셋 키
- **신규 프리팹/SO/스프라이트 없음.** 미션 아이콘은 기존 어드레서블 스프라이트 `Cat1` ~ `Cat5`(= `EBlockState.Cat1~5`.ToString())를 재사용한다. (Rule 03 §2 — enum 값명 = 에셋 파일명 일치, 이미 충족)

### 7.4 적립 로직 (GPGameScene)
- 위치: `Assets/Scripts/Scenes/GPGameScene.cs` — `RemoveMatchBlock()` 루프 내, 매치 1회/블록 게이트(`!block.remove` 가 true로 전환되는 분기) 안, 기존 `DailyMissionService.OnBlockDestroyed(1)` 호출 지점과 같은 곳.
- 동작: 매치 제거되는 블록의 `EBlockState`를 `Block.GetBaseCat`으로 정규화 → 결과가 `EBlockState.None`이 아닐 때만 `CHMData.Instance.GetCollectionData(baseCat.ToString()).value` 를 1 증가.
- **이중 카운트 회피**: 폭탄/특수폭탄/Wall/Potal 등은 `GetBaseCat`이 `None`을 반환해 이 경로에서 제외된다. 폭탄류는 기존 `GPBombResolver.SaveBombCollectionData`에서만 적립되고, 일반 고양이는 그 게이트를 통과 못 하므로 이 경로에서만 적립된다 → 각 블록 정확히 1회 적립.
- 영속화: 별도 저장 코드 불필요 — 기존 `Data.Collection` 저장 메커니즘 사용.
- (코드 스타일은 Rule 02 준수: `//#` 주석, 명시적 타입, `var`/`!` 금지, 가드 절 외 분기 중괄호 — 구체 구현은 gameplay-programmer 판단 영역)

> **검증-후-수정 주의 (§7.5와 동일 — 이중 적립 방지)**: 현재 이 적립 로직은 이미 `GPGameScene.cs`의 `RemoveMatchBlock()`(대략 line 636~642, `DailyMissionService.OnBlockDestroyed(1)` 직후)에 선구현되어 있다(plan 선반영). gameplay-programmer는 **두 번째 적립 블록을 추가하지 말고**, 기존 코드가 위 명세(GetBaseCat 정규화 → None 아닐 때만 `value += 1`)와 정확히 일치하는지 **검증만** 한다. 불일치 시 위 명세를 진실로 보정. 명세를 그대로 다시 추가하면 블록당 +2 적립 → 수집량·골드 2배의 무성(silent) 밸런스 버그가 발생한다.

### 7.5 SO 스키마 / 데이터 필드 — Mission.json
- 위치: `Assets/AssetBundleResources/json/Mission.json` 배열 **맨 앞**(기존 missionID 1 행 앞)에 5행 추가.
- 스키마(필드 = 기존 행과 동일 형식):

| missionID | tapIndex | descStringID | collectionType | clearValue | addValue | reward | rewardCount |
|---|---|---|---|---|---|---|---|
| 18 | 1 | 180 | 0 (Cat1) | 100 | 100 | 0 (Gold) | 100 |
| 19 | 1 | 181 | 1 (Cat2) | 100 | 100 | 0 (Gold) | 100 |
| 20 | 1 | 182 | 2 (Cat3) | 100 | 100 | 0 (Gold) | 100 |
| 21 | 1 | 183 | 3 (Cat4) | 100 | 100 | 0 (Gold) | 100 |
| 22 | 1 | 184 | 4 (Cat5) | 100 | 100 | 0 (Gold) | 100 |

> **검증-후-추가 주의**: 현재 Mission.json에 이미 missionID 18~22 행이 존재함을 확인했다(plan 선반영 추정). gameplay-programmer는 **재추가하지 말고**, 위 스키마와 값이 정확히 일치하는지 검증만 한다. 불일치 시 위 표를 진실로 보정.

### 7.6 String JSON — descStringID 180~184
- 위치: `Assets/AssetBundleResources/json/StringKorea.json` / `StringEnglish.json` — 기존 마지막 항목(stringID 179) 뒤. UTF-8(BOM 없음) 유지.
- 문구(결정 락 — 기존 "화살표 블록 N 터트리기" 양식과 일관):

| stringID | 한국어 (StringKorea) | 영어 (StringEnglish) |
|---|---|---|
| 180 | 고양이 블록 1 터트리기 | Pop Cat Block 1 |
| 181 | 고양이 블록 2 터트리기 | Pop Cat Block 2 |
| 182 | 고양이 블록 3 터트리기 | Pop Cat Block 3 |
| 183 | 고양이 블록 4 터트리기 | Pop Cat Block 4 |
| 184 | 고양이 블록 5 터트리기 | Pop Cat Block 5 |

### 7.7 collectionType ↔ EBlockState ↔ 에셋 키 매핑 (단일 진실)

| missionID | collectionType (int) | EBlockState | 아이콘 스프라이트 키 | descStringID |
|---|---|---|---|---|
| 18 | 0 | Cat1 | `Cat1` | 180 |
| 19 | 1 | Cat2 | `Cat2` | 181 |
| 20 | 2 | Cat3 | `Cat3` | 182 |
| 21 | 3 | Cat4 | `Cat4` | 183 |
| 22 | 4 | Cat5 | `Cat5` | 184 |

### 7.8 미션 UI
- **변경 없음.** `MissionScrollViewItem`의 tapIndex 1(MissionTabNormal) 분기가 collectionType 기반으로 진행도·아이콘·반복보상을 이미 처리한다.

### 7.9 배포 주의
- Mission.json / StringKorea.json / StringEnglish.json은 `"Resource"` 라벨 Local 그룹. **APK 빌드 전 `Window > Asset Management > Addressables > Groups > Build` 필수.** 에디터 플레이 테스트는 Addressables Play Mode Script가 "Use Asset Database"일 때만 실시간 반영.

---

## § Self-Review

- **Placeholder 잔존 (5 카테고리)**: 0건.
  - 미정 마커: 수치 미확정 항목(스테이지당 블록 빈도·골드 경제 영향)은 비우지 않고 "qa-simulator 검증 후 결정/모니터링 + 결정 메트릭"으로 명시(§2.3, §2.5).
  - 애매한 권유/두 갈래 위임: 0건. clearValue/addValue/문구/위치/대상 모두 단일값 확정.
  - 본문 비움 참조·검산 누락: 페이싱·골드 유입에 검산식 명시(§2.3, §2.5).
- **스펙 커버리지**: spec §1(의도)→헤더, §2(범위)→전체, §3(코드 변경 필수+결정 락)→§1·§2.1, §4(메커니즘)→§7.4·§7.5·§7.8, §5(데이터 흐름)→§7.4, §6(엣지)→§6, §7(테스트 대상)→test-engineer 단계 위임(범위 외 명시), §8(영향 파일)→§7, §9(배포)→§7.9. 갭 0건.
- **내부 일관성**: clearValue=100·addValue=100·Gold 100·descStringID 180~184·missionID 18~22·collectionType 0~4가 본문/표/구현 요청사항 전체 동일.
- **시그니처/명명 일관성**: `GetBaseCat`·`RemoveMatchBlock`·`GetCollectionData`·`MissionScrollViewItem`·`EBlockState.Cat1~5`·`EReward.Gold`·`Cat1`~`Cat5`(스프라이트 키) 글자 그대로 일관(Grep 확인).
- **모호 표현**: 0건.
- **스코프**: 단일 구현 단위(코드 1지점 + JSON 3파일). 분할 불필요.
- **구현 요청사항 완전성**: Enum/Interface/에셋 키는 N/A로 명시, 적립 로직·Mission.json 스키마·String 표·매핑 표 완비.

**Self-Review: 통과** (페이싱 서술을 감사 §5의 "롱테일"에서 "정속 faucet"으로 정정, 골드 유입 모니터링 메트릭 추가, 구현 요청사항 N/A 슬롯 명시 — advisor 교정 반영).
