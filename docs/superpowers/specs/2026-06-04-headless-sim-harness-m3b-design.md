# 헤드리스 시뮬 하니스 M3b — Fish/Ball 이동블록 설계

작성일: 2026-06-04
선행: M1(일반매치), M2(정적 특수블록+드래그규칙), M3a(폭탄 시스템). 현재 supported 103/150.

## 1. 목표

CatPang 의 **이동블록 Fish/Ball** 을 헤드리스 시뮬에서 측정 가능하게 한다. 이로써 남은 47개 미지원 stage(전부 Fish/Ball 만으로 막힘 — 검증 완료)가 unlock → **노멀모드 150/150 완전 커버리지**(보스 group≥100000 은 M4 별도).

**범위 밖**: 보스(M4), 이펙트/사운드/애니메이션 타이밍.

## 2. 메커니즘 (실게임 충실)

### Fish (EBlockState.Fish=24)
- 드래그 불가, 폭탄/매치로 **제거 불가**.
- 매 턴 gravity 로 하강. **유일 탈출 = 맨아래줄 도달 시 색폭탄 변환**(실 `SetDissapearBlock` L697~701: `changeBlockState = Random(PinkBomb..BlueBomb)`). 변환되면 Fish 목표 제거됨.

### Ball (EBlockState.Ball=53)
- 드래그 **가능**, 폭탄 범위로 **직접 제거 가능**(실 `ChangeMatchState` 가 Ball 을 제외 안 함).
- 또는 맨아래줄 도달 시 **Potal 변환**(hp `PortalBlockHp=5`) + 좌우 확산: 인접 일반블록/제거대상을 Potal 로(hp `BallPortalStartHp=4` 부터 1씩 감소, 비일반 만나면 정지). 우측 루프는 `ballHp<=0` break, 좌측은 없음(실게임 비대칭 — 충실 포팅). Ball→Potal 은 **목표 전환**(Ball 목표→Potal 목표), 이후 Potal 을 깨야 클리어.

### 승패
Fish/Ball 은 hp 무관 **목표블록**(실 Update L190·L1005: `GetHp()>0 || IsFishBlock() || IsBallBlock()`). 보드에 남아있으면 미클리어.

## 3. 아키텍처 (실게임 구조 미러 + 통합부 단위)

| 파일 | 변경 | 책임 |
|---|---|---|
| `SimDisappearResolver` (신규) | 맨아래줄(row=Size-1) 스캔: Fish→색폭탄(시드 RNG), Ball→Potal+좌우 확산. 실 `SetDissapearBlock` 포팅. ChangeBlockState/ChangeHp 예약(ApplyChanges 가 적용) |
| `SimBlock.cs` 확장 | `IsFish()`(State==Fish), `IsBall()`(State==Ball). `CanNotDrag()` 에 **Fish 추가**(Ball 미추가 — 드래그 가능) |
| `SimMatchChecker.cs` 확장 | `ChangeMatchState` 에 **Fish 제외** 추가(폭탄 제거 불가). Ball 은 제거 가능(유지) |
| `SimGame.cs` 확장 | 턴루프에 `SimDisappearResolver` 호출(매 수 1회, RunCreators 근처), `CheckClear` 의 Fish/Ball 목표 판정 활성화(M3a placeholder), `IsSupported` 에 Fish/Ball 추가 |

`SimDisappearResolver` 는 시드 RNG 주입(`new System.Random(seed+4)` — 기존 damage/creator/bomb/combo RNG 와 분리).

### 낙하 (변경 없음)
Fish/Ball 은 `IsWallLike`(Wall/CatBox/Creator) 가 아니므로 기존 `SimGravity.CompactSegment` 가 매 턴 하강시킨다. M2 gravity 수정으로 State/Hp/ChangeBlockState 가 함께 이동하므로 추가 작업 불필요.

## 4. 턴루프 통합 순서 (실 AfterDrag)

실게임 AfterDrag: `... → BlockCreatorBlock(creator) → SetDissapearBlock() → UpdateMap() → CheckMap()`. Sim 메인 루프의 한 수 처리에서 `RunCreators` 직후, `ResolveCascades` 전에 `SimDisappearResolver.Resolve` 호출(예약 후 ApplyChanges). 단, **맨아래줄 변환은 그 칸이 맨아래줄에 "도달"했을 때** 발생 — gravity 후 검사이므로 ResolveCascades 의 gravity 적용 흐름과 정합되게 배치한다(상세는 plan).

## 5. AI 동작 (MVP: 수동)

Fish/Ball 은 매 턴 낙하해 결국 맨아래줄에서 변환되므로 **수동 플레이로 측정 가능**. Ball 은 드래그 가능 + M3a 폭탄 수 탐색이 Ball 인접 스왑을 자연히 포함. 별도 Fish/Ball 타게팅 정책 불필요(YAGNI). cap-hit stage 는 M3a 처럼 "하드 stage" 후보로 기록(sim 버그 아님 가정, 정밀 정책 검증은 선택).

## 6. 비결정성

Fish→색폭탄 색 선택만 비결정 → 시드 RNG(seed+4). Ball 확산은 결정적.

## 7. 골든/테스트 전략 (하이브리드)

`SetDissapearBlock` 은 GPGameScene 내부(비순수)라 직접 런타임 골든 불가(M3a 조합과 동일) → **포팅 + 단위 테스트**:
- Ball→Potal 좌우 확산: 결정적이라 정확한 hp 분포·정지 조건 단위 골든(실 L708~722 대조).
- Fish→색폭탄: 변환 발생 + 시드 결정성 단위.
- ChangeMatchState Fish-immunity: 폭탄 범위에 Fish 넣어도 match 안 됨 단위.
- CheckClear Fish/Ball: 보드에 Fish/Ball 남으면 미클리어, 변환/제거 후 클리어 단위.
- 낙하: Fish/Ball 이 매 턴 한 칸씩 떨어져 맨아래줄 도달하는지 단위.

회귀 PASS 62 유지. `CountSupported` 재측정 → **150/150 확인**. 진단(150판) 무한루프·크래시 0 확인.

## 8. IsSupported 갱신

`SimGame.IsSupported` 화이트리스트에 Fish(24)/Ball(53) 추가. → 노멀모드 전 블록 지원(보스 제외).

## 9. 산출물

- 신규: `Assets/Scripts/Sim/SimDisappearResolver.cs`
- 확장: `SimBlock.cs`, `SimMatchChecker.cs`, `SimGame.cs`
- 테스트: `Assets/Tests/EditMode/Sim/SimDisappearTests.cs`, `SimGameTests.cs`(확장)
- 측정: `docs/qa-reports/` M3b 리포트 (150/150)
