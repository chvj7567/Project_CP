# Spec — Cat1~5 기본 고양이 블록 수집 미션 (tapIndex 1)

> 작성일 2026-05-29 · 출처 감사 보고서 `docs/design/content-audit/2026-05-29-cat-block-collection-mission-gap.md`
> 단계: brainstorming(spec) — 의도·범위·메커니즘 윤곽 + 결정 락

## 1. 의도 (Why)

미션 탭 tapIndex 1("달성형 수집")은 현재 Arrow1~6·CatPang·5색 특수폭탄(collectionType 10~23)만 다룬다. 이 블록들은 플레이어가 의도적으로 조합해야 등장하므로, 신규 플레이어에게 미션 탭이 "내 플레이와 무관한 곳"으로 느껴진다. 반면 매 스테이지 수백 개씩 매치되는 **기본 고양이 블록(Cat1~7, collectionType 0~6)에 대응하는 수집 미션이 하나도 없다.**

Cat1~5 수집 미션 5개를 추가해, 스테이지 플레이가 곧바로 미션 진행·보상으로 이어지는 초반 보상 루프를 만든다.

## 2. 범위 (Scope)

**포함:**
- 일반 고양이 블록(Cat1~5) 수집량을 매치 제거 시점에 누적하는 로직 추가
- Mission.json tapIndex 1 **맨 앞**에 Cat1~5 미션 5개 추가
- StringKorea/StringEnglish 미션 설명 문자열 5개 추가

**제외 (YAGNI / 차순위):**
- Cat6/Cat7 수집 미션 (적립은 되나 미션 항목은 만들지 않음)
- tapIndex 2 CatBox2~5 수집 미션 (감사 보고서 §보류)
- 보스 스테이지 클리어 일일 미션 (EDailyCounter 확장 필요 — 별도 건)
- 수집 누적량 개인 기록 UI 노출 (감사 §9 장기 제안 — 별도 건)

## 3. 핵심 제약 — 감사 보고서 전제 정정 (Critical)

감사 보고서는 "Cat1~7 수집 누적값이 이미 Data.Collection에 기록되고 있어 코드 변경 전혀 없음"이라 했으나, **이는 거짓이다.**

- 블록 수집 적립의 유일한 경로는 `GPBombResolver.SaveBombCollectionData`이며, 첫 줄에서 `if (!block.IsBombBlock() && !block.IsSpecialBombBlock()) return;` 가드로 막힌다.
- `Cat1~7`은 `IsNormalBlock()` 이라 이 가드를 통과하지 못한다 → **일반 고양이 블록은 수집량이 적립된 적이 없다.**
- 기존 tapIndex 1 미션(Arrow·CatPang·특수폭탄)이 작동하는 이유는 그 블록들이 모두 `IsBombBlock()/IsSpecialBombBlock()` 이라 가드를 통과하기 때문.

→ Mission.json만 추가하면 Cat 미션 진행도가 영원히 0에 머문다. **코드 변경이 반드시 필요하다.**

### 결정 락 (사용자 확정)
| 항목 | 값 |
|---|---|
| 밸런스 | clearValue = 100, addValue = 100 (반복 시 100→200→300…) |
| 보상 | Gold ×100 (reward=0, rewardCount=100) |
| 문구 | "고양이 블록 N 터트리기" / "Pop Cat Block N" |
| 리스트 위치 | tapIndex 1 **맨 앞** (기존 missionID 1 이전) |
| 대상 | Cat1~5 (collectionType 0~4), 5개 |

## 4. 메커니즘 (How — 윤곽)

### 4.1 수집 적립
`GPGameScene.RemoveMatchBlock()` — 매치된 모든 블록이 정확히 1회 통과하는 루프(`if (block.IsMatch() && !block.remove)` → `block.remove = true`). 기존 일일 미션 카운트(`DailyMissionService.OnBlockDestroyed(1)`)와 같은 지점에서 적립한다.

- `Block.GetBaseCat(block.GetBlockState())` 로 정규화:
  - Cat1~7 → 자기 자신
  - 스킨 고양이(CatCrown1~CatStrawberry5, 54~83) → 기본 색 Cat1~5
  - 그 외(폭탄·특수폭탄·Wall·Potal 등) → `EBlockState.None`
- 결과가 `None` 이 아닐 때만 `CHMData.Instance.GetCollectionData(baseCat.ToString()).value += 1`

**이중 카운트 회피:** 폭탄/특수폭탄은 `GetBaseCat` 이 `None` 을 반환해 이 경로에서 제외되고, 기존대로 `SaveBombCollectionData` 에서만 적립된다. 일반 고양이 블록은 `SaveBombCollectionData` 게이트를 통과하지 못하므로 이 경로에서만 적립된다 → 각 블록은 정확히 1회 적립.

### 4.2 미션 데이터
Mission.json tapIndex 1 맨 앞에 missionID 18~22 추가. 각 항목: `collectionType` 0~4, `clearValue` 100, `addValue` 100, `reward` 0(Gold), `rewardCount` 100.

### 4.3 미션 UI — 변경 없음
`MissionScrollViewItem` (tapIndex 1 분기)이 이미:
- `GetCollectionData(collectionType.ToString())` 로 진행도(`value - startValue`) 계산
- `GetMissionSpriteName` → `collectionType.ToString()` = `"Cat1"~"Cat5"` 스프라이트를 어드레서블에서 로드 (기존 BlockDestroy 일일 미션이 `"Cat3"` 아이콘을 쓰고 있어 로드 가능성 검증됨)
- `addValue` 반복 달성(`repeatCount`) 구조 처리

리스트 순서 = Mission.json 행 순서(`CHMJson.GetMissionInfoList(tapIndex)` 필터). 맨 앞 행에 두면 미션 탭 진입 시 최상단 노출.

## 5. 데이터 흐름

```
스테이지 플레이 → 고양이 블록 3-매치
  → GPGameScene.RemoveMatchBlock (block.remove 게이트, 1회/블록)
    → GetBaseCat 정규화 → Data.Collection["Cat1".."Cat5"].value += 1
  → (영속화: 기존 폭탄 수집과 동일 메커니즘 — 신규 저장 코드 불필요)

미션 탭 진입 → MissionScrollViewItem.Init (tapIndex 1)
  → GetCollectionData("CatN") 읽어 진행도 표시 / 아이콘 로드 / 반복보상 처리
```

## 6. 엣지 케이스

- **스킨 고양이**: `GetBaseCat` 으로 기본 색에 합산 → 스킨을 써도 Cat1~5 미션이 정상 진행.
- **Cat6/Cat7**: 적립은 되나 미션 없음 → `Data.Collection["Cat6"/"Cat7"]` dict 엔트리만 생성, 무해.
- **기존 플레이어**: 일반 고양이 수집이 그간 추적된 적 없어 모두 0에서 시작 → 즉시 다단 완료/과도한 보상 알림 리스크 없음. (감사 §7/§8 "고인물 즉시 달성·긍정적 서프라이즈"는 잘못된 전제 기반이므로 **무효**.)
- **startValue 스냅샷**: 미션 첫 노출 시 현재 수집량을 startValue로 잡는 기존 동작 그대로 — Arrow 미션과 동일.

## 7. 테스트 대상 (test-engineer 단계)

- 일반 고양이 블록(Cat1~5) 매치 시 해당 색 수집량 +1
- 스킨 고양이(예: CatCrown1) 매치 시 기본 색(Cat1) 수집량에 합산
- 폭탄/특수폭탄/Wall 매치 시 이 경로에서 적립되지 않음 (이중 카운트 회피)
- tapIndex 1 Cat 미션 진행도 표시 및 반복 보상(100→200→300) 동작

## 8. 영향 파일

| 파일 | 변경 |
|---|---|
| `Assets/Scripts/Scenes/GPGameScene.cs` | `RemoveMatchBlock` 에 수집 적립 추가 |
| `Assets/AssetBundleResources/json/Mission.json` | missionID 18~22 추가 (tapIndex 1 맨 앞) |
| `Assets/AssetBundleResources/json/StringKorea.json` | descStringID 180~184 |
| `Assets/AssetBundleResources/json/StringEnglish.json` | descStringID 180~184 |

## 9. 배포 주의

세 JSON은 `"Resource"` 라벨 Local 그룹으로 로드된다. APK 빌드 전 `Window > Asset Management > Addressables > Groups > Build` 필요. 에디터 플레이 테스트는 Addressables Play Mode Script가 "Use Asset Database" 일 때만 실시간 반영(=Existing Build면 옛 데이터).
