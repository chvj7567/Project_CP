# Content Audit — 2026-07-03 — Wall·Potal 초등장 스테이지(6·7) 튜토리얼 완전 공백

> 자동 생성 (매일 07:01 KST) — CatPang Daily Content Audit 루틴 (Rule 01 자동화 예외).
> 이 보고는 제안이며, 정식 기획화는 별도 검토가 필요하다.

## 0. 입력 스냅샷
- 참조 스테이지 수: 노멀 150 / 하드 150 / 보스 100 (플레이 모드 기준) — Stage.json 행 수: 250 (하드·노멀 공유 150 + 보스 100)
- 참조 JSON 파일 수: 9개 (ConstValue / Guide / Mission / Shop / Stage / StageBlock / StringKorea / StringEnglish / Tutorial)
- 과거 감사 이력 (git log): 25건 (가장 최근: 2026-07-02)

## 1. 현황

| 카테고리 | 현황 | 비고 |
|---|---|---|
| 스테이지 수 | 노멀 150 / 하드 150 / 보스 100 | Stage.json 250행 (공유 150 + 보스 100), 노멀은 하드 테이블 공유 |
| 활용 블록 타입 | 29종 / 전체 84 (빈 슬롯 제외 61종 정의) | StageBlock.json 실배치 기준, 스킨·Cat6·Cat7 스테이지 미배치 |
| 일일 미션 종류 | EDailyCounter 4종 + CatPang 수집 1건 = 5건 | Attendance / NormalStageClear / BlockDestroy / AdWatch + CatPang daily |
| 상점 아이템 | 12개 | tapIndex 1: 9개(스킨 7 + 골드 2), tapIndex 2: 3개(IAP) |
| 고양이 스킨 | 6테마 × Cat1~5 = 30종 정의 | CatCrown / CatFlowers / CatMushroom / CatParty / CatSanta / CatStrawberry |

### 분포 공백 — Tutorial.json 초기 장벽 블록 미커버

Tutorial.json는 특정 `tutorialStageID`에 대응하는 안내 항목을 담는다. StageBlock.json 기준 각 블록의 **첫 등장 스테이지**를 확인하면:

| 블록 | 첫 등장 스테이지 | Tutorial.json 항목 | 비고 |
|---|---|---|---|
| Wall (16) | **stage 6** | **❌ 없음** | 인접 매치로만 제거 가능 |
| Potal (17) | **stage 7** | **❌ 없음** | HP 소진 시 일반 블록 변환 |
| Fish (24) | stage 31 | ✅ tutorialStageID=31 | |
| CatBox1 (40) | stage 51 | ✅ tutorialStageID=51 | |
| WallCreator (45) | stage 71 | ✅ tutorialStageID=71 | |
| PotalCreator (46) | **stage 73** | **❌ 없음** | WallCreator 쌍 블록, 누락 |
| RainbowPang (52) | stage 91 | ✅ tutorialStageID=91 (CatPang 연결) | |
| Ball (53) | stage 131 | ✅ tutorialStageID=131 | |

**핵심 발견**: Wall이 stage 6, Potal이 stage 7에 등장하는데 두 블록 모두 Tutorial.json에 항목이 없다. 이 두 블록은 **"직접 매치 불가 → 인접 칸 매치로 HP 감소 → HP=0에서 제거·변환"** 이라는 게임 내 가장 핵심 장벽 메커니즘의 첫 접촉 지점이다. Tutorial.json의 가장 이른 장애물 안내(stage 31, Fish)보다 25스테이지나 앞서 나타남에도 아무런 안내가 없다.

### 과거 감사 후보 (git log 조회 결과)

| 날짜 | 커밋 SHA | 설명 요약 |
|---|---|---|
| 2026-07-02 | 4de882b | Guide.json 하드 스테이지 가이드 완전 공백 + guideIndex 13~15 고아 항목 — HardStageGuideMaxIndex 상수 신설 제안 |
| 2026-07-01 | 013d928 | 보스 스테이지 Tutorial.json 항목 완전 부재 — 100개 전부 tutorialID=-1, HP·보스 스킬 맥락 안내 신설 제안 |
| 2026-06-30 | f9451b8 | RainbowPang tapIndex 3 일일 미션 완전 공백 — 오늘 2번 발동 일일 목표 신설 제안 |
| 2026-06-29 | dd924d0 | Fish 블록 tapIndex 1 반복 수집 미션 완전 공백 — stage 31 등장 블록 중간 목표 신설 제안 |
| 2026-06-28 | 5c06786 | Ball 블록 초등장 stage 131 — 노멀 스테이지 87% 시점 후기 도입, 19개 스테이지 집중 |
| 2026-06-26 | 54006f8 | Arrow1~6 tapIndex 2 장기 이정표 미션 완전 공백 — 화살표 폭탄 생애 이정표 신설 제안 |
| 2026-06-25 | fcebcd0 | 후반 그룹 10~15 복합 제약(시간+이동 동시) 밀스톤 스테이지 완전 부재 — stage 91~150 이중 실패 조건 소멸 |
| 2026-06-24 | 3009996 | 하드·보스 스테이지 클리어 일일 미션 완전 공백 — EDailyCounter HardStageClear·BossStageClear 카운터 없음 |
| 2026-06-23 | 6d92d88 | CatBox 완성 tapIndex 3 일일 미션 완전 공백 — 보스 스테이지 보상 루프 신설 제안 |
| 2026-06-22 | 962c93d | 후반 스테이지 moveCount 1~100 100배 격차 — 난이도 역전 stage 137 이동 100회·목표 900점 |
| 2026-06-21 | f58d02d | 스테이지 진행 이정표 보상 완전 부재 — normalStage/hardStage/bossStage 필드 미션 미연결 |
| 2026-06-20 | 5f57450 | Cat1~5 tapIndex 2 장기 이정표 미션 완전 공백 — 기본 블록 생애 1회 달성 이정표 신설 제안 |
| 2026-06-19 | feddcde | AddTime·AddMove 아이템 보스 스테이지 완전 무효 — 보스 전용 아이템 효과 설계 부재 |
| 2026-06-18 | 6bac443 | Ball 탈출·Potal 전환 tapIndex 3 일일 미션 완전 공백 — Mission.json 1줄 추가로 신설 제안 |
| 2026-06-17 | 4c65699 | 하드→보스 해금 임계값 50/150 비대칭 — 보스 스테이지 조기 진입 허용 설계 재검토 |
| 2026-06-16 | 3670d4f | 고양이 스킨 블록(EBlockState 54~83) 수집 미션 완전 공백 — 스킨 장착 보상 루프 신설 제안 |
| 2026-06-15 | b0ee2e0 | 5색 특수폭탄 tapIndex 2 장기 이정표 미션 완전 부재 — PinkBomb~BlueBomb 50개 이정표 신설 제안 |
| 2026-06-14 | ee1fc5b | 연속 출석 스트릭 미션 완전 부재 — 장기 리텐션 루프 단절 제안 |
| 2026-06-13 | ccb2a5d | Data.Stage.boomAllCount 잠든 필드 — BoomAll 없이 클리어 보너스 시스템 신설 제안 |
| 2026-06-12 | 80f90cc | CatPang 블록 tapIndex 2 장기 누적 이정표 미션 공백 — 게임 동명 블록 장기 목표 신설 제안 |
| 2026-06-11 | 7528a26 | 아이템 사용(AddTime·AddMove) 미션 연계 공백 — IAP 구매자 보상 루프 단절 제안 |
| 2026-06-10 | 85ac09c | 보스 스테이지 플레이어 HP 소진·회복 루프 미설계 — HP 회복 경로 신설 제안 |
| 2026-06-09 | 329cce2 | 중반-후반 110 스테이지 blockTypeCount=5 고착 — 색 복잡도 완급 조절 레버 미활용 제안 |
| 2026-06-08 | 26ebd5a | 하드 스테이지 해금 임계값 150/150 — 노멀 전량 완료 강제 진입 장벽 완화 제안 |
| 2026-06-07 | 7d601d5 | ESelect 게임 시작 스킬 6종 미션·보상 연계 완전 공백 — tapIndex 2 스킬 선택 미션 신설 제안 |
| 2026-06-04 | 158d246 | Cat6·Cat7 tapIndex 1 수집 미션 비대칭 누락 — Cat1~5 구현 후 2종만 잔여 공백 |

## 2. 추가 컨텐츠 후보 (권장 1개)

### Wall·Potal 초등장 스테이지(6·7) 튜토리얼 신설

- **카테고리**: 튜토리얼 / 초기 온보딩
- **요지**: Wall(stage 6)·Potal(stage 7) 첫 등장 시 Tutorial.json 항목이 전무하다. 두 블록은 "직접 매치 불가" 장벽 메커니즘의 첫 접촉이므로, 안내 없이 만나는 플레이어는 진행 불가 이유를 직관적으로 파악하기 어렵다.
- **점수**: 검증가치 4 / 구현비용 2 / 플레이어경험개선 5 / 데이터근거 5 → 종합 **18**
  - 검증가치 4: 초반(그룹 1) 이탈 방지 효과를 직접 측정 가능
  - 구현비용 2: Tutorial.json 2행 추가 + StringKorea·StringEnglish 2항목 추가로 완료. 코드 변경 없음
  - 플레이어경험개선 5: Wall/Potal 규칙 미이해는 초반 막힘의 직접 원인 — 인접 매치 필요성 안내 한 줄이 스테이지 클리어율에 즉각 영향
  - 데이터근거 5: Tutorial.json·StageBlock.json·Stage.json 3중 교차 검증 완료
- **근거**:
  - `Assets/AssetBundleResources/json/StageBlock.json` — Wall(blockState=16) 최초 등장 stage=6, Potal(blockState=17) 최초 등장 stage=7
  - `Assets/AssetBundleResources/json/Tutorial.json` — tutorialStageID 목록: [1,2,3,4,5,8,31,51,71,91,131]. 6과 7 없음
  - `Assets/AssetBundleResources/json/Stage.json` — stage 6: group=1(초기 그룹), time=10.0, boardSize=5. stage 7: group=1, boardSize=5
  - 비교: Fish는 stage 31 첫 등장에 tutorialStageID=31 항목 존재. Ball은 stage 131 첫 등장에 tutorialStageID=131 존재. Wall·Potal만 공백

#### 유저 플로우 (9개 항목)

1. **노출 시점·트리거**
   Tutorial.json에 `tutorialStageID=6`(Wall 안내)와 `tutorialStageID=7`(Potal 안내) 항목이 추가된 뒤, 플레이어가 해당 스테이지에 처음 진입하는 순간 GPTutorial이 tutorialStageID를 조회해 팝업을 트리거한다. 스테이지 진입은 Stage 5 클리어 → Stage 6 진입 자동 플로우로 이루어지며, 튜토리얼 항목 유무가 진입 조건을 바꾸지는 않는다.

2. **화면 변화**
   Stage 6 진입 직후 보드가 초기화된 상태에서 팝업 오버레이(기존 Fish·CatBox 안내와 동일 레이아웃)가 등장한다. 팝업 내부에는 "Wall 블록은 직접 매치할 수 없어요. 인접한 칸을 매치해서 깨세요!" 같은 1~2줄 설명과 닫기 버튼이 표시된다. Stage 7 진입 시에도 동일 패턴으로 "Potal 블록은 인접 매치로 체력을 0으로 만들면 일반 블록으로 바뀌어요!" 안내가 나온다.

3. **입력 행동**
   플레이어는 팝업 텍스트를 읽고 닫기(확인) 버튼을 탭한다. 기존 Tutorial.json 기반 안내의 입력 구조와 동일 — 추가 조작 없이 1탭으로 종료된다.

4. **시스템 반응**
   GPTutorial이 팝업 닫힘 신호를 받은 뒤 정상 보드 입력을 활성화한다. 이후 플레이어는 Wall 블록이 포함된 stage 6 보드를 직접 플레이한다. 안내가 방금 설명한 인접 매치 규칙을 머릿속에 넣은 상태로 첫 시도를 시작하게 된다.

5. **반복·재발생 패턴**
   해당 튜토리얼은 각 스테이지당 1회만 표시된다. Stage 6을 클리어하지 못하고 재도전해도 이미 본 튜토리얼은 재표시되지 않는다(기존 Fish·CatBox 안내와 동일 동작). 노멀 모드에서도 Stage 6·7은 하드와 동일 StageBlock 데이터를 공유하므로 동일 튜토리얼이 적용된다.

6. **종료·해소 조건**
   닫기 버튼 탭으로 팝업이 닫히며 튜토리얼이 완료된다. Data.Login.guideIndex 또는 별도 플래그에 해당 tutorialStageID 열람 완료가 기록되어 이후 동일 스테이지 진입 시 재표시하지 않는다.

7. **다른 시스템과 상호작용**
   StringKorea.json·StringEnglish.json에 새 descStringID 2개가 추가되어야 한다(Wall 설명 1개, Potal 설명 1개). CHMString의 키 정의가 있으면 함께 추가. 이외 GPBoard, GPMatchChecker, Block.cs 등 기존 게임 로직은 변경하지 않는다 — 코드 수정 없이 데이터 파일 2개(Tutorial.json, String 파일)만 변경으로 완료된다.

8. **엣지 케이스**
   ① 앱을 Stage 6 팝업이 뜨는 도중 종료하면 재진입 시 팝업이 다시 표시되어야 한다(열람 완료 기록이 없으므로). ② Stage 6을 처음부터 클리어한 경험이 있는 기존 플레이어(이미 guideIndex가 진행된 유저)에게는 소급 표시하지 않는다 — 신규 유저 대상만. ③ Stage 5를 클리어하지 않은 상태에서 Stage 6에 진입할 경로는 없으므로(선형 진행) 순서 충돌 없음.

9. **유저 정보·피드백**
   안내 전까지 플레이어는 "왜 이 블록을 눌러도 매치가 안 되지?"라는 의문을 가지게 된다. 팝업 1개가 "인접 칸을 매치하라"는 핵심 규칙을 명시하면 그 의문이 즉시 해소된다. Potal의 경우 "일정 횟수 인접 매치 후 일반 블록으로 전환된다"는 변환 메커니즘도 함께 전달해야 재도전 전략이 생긴다. 기존 fish/CatBox 튜토리얼 안내 문구 톤 앤 매너(짧고 직접적인 2줄 이내)를 그대로 따르는 것이 권장된다.

### 보류

| 후보 | 카테고리 | 점수 | 보류 이유 |
|---|---|---|---|
| CatBox2~5 tapIndex 2 장기 미션 공백 | 장기 미션 | 17 | 유사 내용 docs 파일 존재(2026-05-29). 튜토리얼 갭(18점)에 밀림 |
| PotalCreator tapIndex 2 미션 공백 | 장기 미션 | 15 | Wall/Potal 튜토리얼이 더 직접적 UX 문제 |
| 보스 스테이지 boardSize 9×9 고착 | 스테이지 구조 | 13 | 보드 크기 변경은 보스 AI 재설계 연동 필요, 구현비용 高 |

## 3. 과거 감사 대비 차별성

git log 25건 검토 완료.

- 가장 유사한 과거 커밋: 013d928 (2026-07-01) "보스 스테이지 Tutorial.json 항목 완전 부재"
  - 차별점: 이전 감사는 **보스 스테이지 100개 전체** tutorialID=-1 공백(tutorialStageID 미설계)을 다뤘다. 이번 제안은 **노멀·하드 스테이지 그룹 1(stage 6·7)** 의 장벽 블록 첫 접촉 안내 부재다 — 대상이 초반 일반 스테이지이며 Wall·Potal의 "간접 제거" 메커니즘 이해를 목표로 한다.
- 4de882b (2026-07-02) "Guide.json 하드 스테이지 가이드 완전 공백": Guide.json과 Tutorial.json은 별개 시스템이다. Guide.json은 게임 내 업적·가이드 포인터(guideIndex), Tutorial.json은 특정 스테이지 진입 시 표시하는 인라인 팝업이다. 카테고리·근거 모두 다르다.
- Wall·Potal 관련 기존 docs 파일(2026-05-29, 2026-05-31)은 **mission 공백**(tapIndex 2) 제안이었다. 이번 제안은 **tutorial 공백**이다 — 시스템과 레이어가 다르다.

## 4. 다음 단계 제안

채택 시 아래 순서로 진행한다:
1. StringKorea.json / StringEnglish.json에 Wall·Potal 설명 문자열 추가 (descStringID 신규 2개 배정)
2. Tutorial.json에 `{"tutorialStageID":6, "descStringID":X, "connectNextBlock":-1, "descNextBlockStringID":-1}` 항목 추가
3. Tutorial.json에 `{"tutorialStageID":7, "descStringID":Y, "connectNextBlock":-1, "descNextBlockStringID":-1}` 항목 추가
4. GPTutorial.cs에서 해당 tutorialStageID를 정상 처리하는지 동작 검증
5. 신규 설치 플레이테스트(Stage 6·7 진입 시 팝업 표시 확인)

## 5. 쉬운 설명 (비개발자 요약)

CatPang에는 일반 블록처럼 직접 매치할 수 없는 특별한 벽 블록(Wall)과 포탈 블록(Potal)이 있다. 이 블록들은 옆 칸의 일반 블록을 맞춰야만 조금씩 깎여 사라진다. 그런데 이 게임은 이 블록들이 처음 나타나는 6번째, 7번째 스테이지에서 아무런 설명도 안 해주고 있다 — 마치 새로운 규칙을 아무 말 없이 갑자기 가르치는 것처럼. 경험 많은 플레이어는 몇 번 시도하면 파악하겠지만, 처음 하는 사람은 "왜 이게 안 눌러지지?" 하고 게임을 그냥 닫아버릴 수 있다. 그래서 이번에 제안하는 것은: 6번·7번 스테이지에 처음 들어갈 때 딱 한 번 짧은 안내 문구("이 블록은 옆 칸을 맞춰서 깨세요!")가 뜨게 만들자는 것이다.
