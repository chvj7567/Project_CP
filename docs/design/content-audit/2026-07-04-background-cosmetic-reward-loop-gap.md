# Content Audit — 2026-07-04 — EBackground 4종 미연동: 게임판 배경 커스터마이징 보상 루프 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 27건 (가장 최근: 2026-07-02, 5c2ba4c)
  - 참고: 2026-05-28~2026-06-03 기간의 구 포맷 커밋(11건)은 grep 미매칭, docs/ 파일로 별도 확인

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 / 전체 84 | 빈 슬롯 25~39·47~51 제외 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1건 = 5개 | Attendance / NormalStageClear / BlockDestroy / AdWatch |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7 + gold 2), tapIndex 2: 3개(IAP) |
| 고양이 스킨 | 6종 × 5마리 = 30 슬롯 | EBlockState 54~83 |
| **게임판 배경** | **EBackground 4종 정의** | **Background1~4 — 플레이어 해금 루프 없음** |

### 분포 공백

**EBackground enum (Defines.cs) vs 코드 참조 현황:**

| enum 값 | int | Defines.cs 정의 | 게임 코드 참조 | Mission.json | Shop.json |
|---|---|---|---|---|---|
| Background1 | 0 | ✅ | ❌ | ❌ | ❌ |
| Background2 | 1 | ✅ | ❌ | ❌ | ❌ |
| Background3 | 2 | ✅ | ❌ | ❌ | ❌ |
| Background4 | 3 | ✅ | ❌ | ❌ | ❌ |

- `grep -r "EBackground" Assets/Scripts/` 결과: **Defines.cs 1파일만 매칭** — 나머지 코드에서 완전 미참조
- `ResourceDownload.cs`는 로딩 화면 배경 순환에 `List<Image> backgroundList` (Inspector 직접 직렬화)를 사용하며 EBackground enum을 거치지 않음
- Mission.json: 27개 미션 중 collectionType 또는 설명으로 "배경"을 언급하는 항목 **0건**
- Shop.json: 12개 항목 중 배경 관련 항목 **0건**

### 과거 감사 후보 (git log 조회 결과 — 최근 10건)

| 날짜 | SHA | 설명 |
|---|---|---|
| 2026-07-02 | 5c2ba4c | Wall·Potal 초등장 스테이지(6·7) Tutorial.json 항목 완전 공백 — 장벽 블록 안내 신설 |
| 2026-07-01 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 13~15 고아 항목 |
| 2026-06-30 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 — 100개 전부 tutorialID=-1 |
| 2026-06-29 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 |
| 2026-06-28 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 |
| 2026-06-26 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 87% 시점 후기 도입 |
| 2026-06-25 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 |
| 2026-06-24 | fcebcd0 | 후반 그룹 10~15 복합 제약 밀스톤 스테이지 부재 |
| 2026-06-23 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 |
| 2026-06-22 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 |

---

## 2. 추가 컨텐츠 후보 (권장 1개)

### EBackground 4종 게임판 배경 커스터마이징 보상 루프 신설

- **카테고리**: 배경 / 수집 / 커스터마이징
- **요지**: `EBackground { Background1~4 }` 가 Defines.cs에 정의되어 있으나 게임 코드에서 완전 미참조이고, 미션·상점과의 연결 없이 플레이어가 배경을 해금하거나 선택할 방법이 없다. 기존 고양이 스킨 블록(EBlockState 54~83)이 블록 외관을 바꾸는 것과 달리, 게임판 전체 배경을 바꾸는 보상 루프를 신설하면 장기 플레이 동기가 생긴다.
- **점수**: 검증가치 4 / 구현비용 2 / 플레이어경험개선 4 / 데이터근거 5 → 종합 **17점**
  - `4 + (6-2) + 4 + 5 = 17`
- **근거**:
  - `Assets/Scripts/Defines.cs` L243~253: `EBackground { Background1=0, Background2=1, Background3=2, Background4=3, Max }` — 4종 정의
  - `grep -r "EBackground" Assets/Scripts/ --include="*.cs" -l` → Defines.cs **1개만** 매칭 (완전 미참조)
  - `Assets/Scripts/Scenes/ResourceDownload.cs` L12: `[SerializeField] List<Image> backgroundList` — 로딩 화면 배경 순환은 CHMResource/EBackground 없이 inspector 직접 참조
  - `Assets/AssetBundleResources/json/Mission.json` 27줄 전체 조회: Background 관련 collectionType 항목 **0건**
  - `Assets/AssetBundleResources/json/Shop.json` 12줄 전체 조회: 배경 관련 항목 **0건**
  - `Assets/Scripts/Data.cs` Data.Login: `selectedBackground` 또는 배경 선택 필드 **없음**

#### 유저 플로우

1. **노출 시점·트리거**: 플레이어가 노멀 스테이지 50·100·150번을 클리어하거나, 상점 tapIndex 1에서 골드로 구매할 때 배경 해금 알림이 표시된다. 스테이지 클리어 연출이 끝난 직후 "새 배경 해금!" 팝업이 한 화면에 등장해 흐름을 끊지 않고 인지시킨다.

2. **화면 변화**: UIMission 또는 UIShop에서 배경 항목을 탭하면 프리뷰 창이 열려 현재 게임판 위에 해당 배경이 반투명 오버레이로 미리 보인다. 해금 전 항목은 회색 잠금 아이콘이 표시되고, 해금 조건(예: "노멀 스테이지 50 클리어")이 아래에 표기된다.

3. **입력 행동**: 해금된 배경 중 원하는 것을 탭 → "적용" 버튼을 누른다. 골드 구매형이면 구매 확인 팝업(UIConfirm)이 뜬 뒤 처리된다. 기본 배경(Background1)은 해금 없이 초기부터 자유롭게 선택 가능하다.

4. **시스템 반응**: `Data.Login.selectedBackground` 필드에 선택값이 저장되고, 다음 GameScene 로드 시 GPGameScene 초기화 단계에서 해당 EBackground 값으로 CHMResource를 통해 배경 Sprite를 로드·적용한다. 선택은 게임 재시작 후에도 유지된다.

5. **반복·재발생 패턴**: 총 4개 배경이 있어 해금 횟수는 최대 3회(Background1은 기본 제공). 노멀 스테이지 클리어 단계마다 해금되는 구조라면 스테이지 50·100·150이 자연스러운 체크포인트가 된다. 상점 구매형을 병행하면 빠른 해금을 원하는 유저를 위한 골드 소비 경로가 생긴다.

6. **종료·해소 조건**: 4개 배경을 모두 해금하면 미션 탭에서 "모든 배경 수집 완료" 표시가 나타나고, 이후에는 선택 전환만 가능하다. 컬렉션 진행률(예: 3/4)이 UIMission tapIndex 2 이정표 영역에서 표시되면 단계적 달성감이 지속된다.

7. **다른 시스템과 상호작용**: Mission.json에 tapIndex 2 이정표 항목(collectionType=신규 Background 카운터)을 추가하면 DailyMissionService와 CHMData가 진행도를 추적한다. Shop.json에 tapIndex 1 항목(gold=30000~60000)을 추가하면 스킨 구매와 동일한 골드 소비 루프를 탄다. Data.Login에 `selectedBackground` int 필드를 추가하면 로컬/클라우드 저장이 자동으로 적용된다.

8. **엣지 케이스**: ① 이미 해금된 배경을 상점에서 다시 구매 시도 → 구매 버튼 비활성화 + "이미 보유 중" 문구 표시. ② 로컬 저장에는 해금 기록이 있으나 클라우드에는 없는 경우 → CHMData의 기존 로컬/클라우드 병합 로직에 selectedBackground 필드 병합 규칙 추가(더 큰 값 우선). ③ Background asset이 Addressables에 미등록된 상태에서 로드 시도 → CHMResource가 null 콜백 처리하므로 기본 배경으로 폴백.

9. **유저 정보·피드백**: 스테이지 클리어 시 해금된 배경 이름이 잠깐 토스트 알림으로 표시되고, 다음 GameScene 진입 시 선택된 배경이 즉시 적용되어 변화가 눈에 띈다. UIMission tapIndex 2에서 "배경 3/4" 식의 수집 진행률이 표시되면 플레이어가 나머지 해금 조건을 자연스럽게 확인하게 된다.

### 보류

- **WallCreator·PotalCreator tapIndex 1 반복 수집 미션 공백** (16점): Mission.json에서 직접 확인 가능한 공백이나, 2026-05-29 커밋(구 포맷)이 PotalCreator tapIndex 2 공백을 이미 다뤘으며 카테고리 부분 중복. 구현비용 1로 낮지만 직접 매치 불가 블록의 반복 수집 미션이 유저에게 명확한 행동 유도를 주는지 불확실.
- **tapIndex 3 일일 미션 WallCreator 제거 카운터 공백** (13점): EDailyCounter 확장이 필요해 코드 변경 비용이 높고, 보스 스테이지 특정 블록에 묶인 좁은 범위.

---

## 3. 과거 감사 대비 차별성

git log 27건 + 구 포맷 11건 = 38건 전체 검토 완료.

가장 유사했던 과거 커밋: **3670d4f (2026-06-15)** — "고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백"

차별점:
- 2026-06-15 대상: **EBlockState 54~83** (고양이 스킨 *블록* — 게임 중 블록 외관 변경, 매치 게임플레이와 직결)
- 이번 대상: **EBackground** (게임판 *배경* — 매치 외 시각 커스터마이징, enum이 코드에서 완전 미참조라는 기술적 근거 추가)
- 카테고리 차이: 블록 수집 미션 vs 게임판 배경 해금 루프
- 근거의 독창성: `grep`으로 EBackground가 Defines.cs 외 어디서도 참조되지 않음을 코드 레벨에서 직접 입증 — 단순 JSON 공백이 아니라 미이행 설계 의도 추정 가능

---

## 4. 다음 단계 제안

채택 시 구체 구현 계획 수립 필요:

1. `Data.Login`에 `selectedBackground` int 필드 추가 (기본값 0 = Background1)
2. `Shop.json`에 tapIndex 1 배경 구매 항목 3건 추가 (Background2~4, 각 30000~60000 gold)
3. `Mission.json`에 tapIndex 2 배경 이정표 항목 추가 (노멀 스테이지 50·100·150 클리어와 연동)
4. `CHMResource`의 LoadSprite 또는 string 오버로드로 `"Background2"` 등 Addressables 에셋 로드
5. `GPGameScene` 초기화 단계에서 `Data.Login.selectedBackground`를 읽어 배경 적용
6. `UISetting` 또는 `UIShop`에 배경 선택 UI 추가

---

## 5. 쉬운 설명 (비개발자 요약)

지금 CatPang에는 게임판 뒤에 깔리는 배경 그림이 무려 4종류나 준비되어 있다. 마치 방에 놓을 벽지를 4개 사두고도 모두 창고에 처박아 둔 상태다. 게임 코드에서도 이 배경들이 사용될 자리가 비어 있고, 미션이나 상점을 통해 플레이어가 획득하거나 교체하는 방법이 전혀 없다. 스킨을 바꾸는 것보다 훨씬 눈에 확 띄는 변화가 배경인데, 지금은 아무도 경험할 수 없다. 그래서 이번에 제안하는 것은: 특정 스테이지를 클리어하거나 골드로 구매하면 배경 그림을 하나씩 해금하는 '배경 수집 루프'를 만들자는 것이다.
