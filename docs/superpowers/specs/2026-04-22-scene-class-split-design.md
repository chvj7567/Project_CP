# Scene Class Split Design

**Date:** 2026-04-22  
**Branch:** refactor/scene-class-split

## 목표

`Game.cs`(~3000줄)와 `First.cs`(~457줄)에 집중된 책임을 목적별 순수 C# 클래스로 분리한다.  
배경 루프(`ChangeBackgroundLoop`, `ChangeBackground`)는 두 씬 모두에서 제거하고 배경을 고정으로 전환한다.

## 접두사 규칙

| 접두사 | 의미 | 적용 씬 |
|--------|------|---------|
| `GP` | GamePlay | GameScene |
| `LB` | LoBby | LobbyScene (First) |

## 클래스 구조

### GameScene

| 클래스 | 파일 | 책임 |
|--------|------|------|
| `GPGameScene` | `GPGameScene.cs` | MonoBehaviour 진입점, 씬 오케스트레이터. `Start`, `Update`, `OnDestroy`, `AfterDrag`, `GameEnd` |
| `GPBoard` | `GPBoard.cs` | `boardArr` 소유 및 캡슐화. `ChangeBlock`, `DownBlock`, `IsValidIndex`, `IsNormalBlock`, `CreateNewBlock` |
| `GPMatchChecker` | `GPMatchChecker.cs` | 매치 감지. `CheckMap`, `Check3Match`, `CheckSquareMatch`, `CanPlay`, `CanDragBlock` |
| `GPBombResolver` | `GPBombResolver.cs` | 폭탄 패턴 12종 + 특수 폭탄. `Bomb1`~`Bomb12`, `BoomAll`, `Boom3`, `RainbowPang`, `CatPang`, `CreateBombBlock`, `BlockCreatorBlock` |
| `GPTutorial` | `GPTutorial.cs` | 튜토리얼/가이드 UI 흐름. `StartGuide`, `StartTutorial`, `NormalStageGuideStart`, `BossStageGuideStart`, `FingerMoveRepeat`, `CheckTutorial`, `TutorialBlockSetting`, `GetTutorialStageImgSettingValue` |
| `GPBossController` | `GPBossController.cs` | 보스 전용 로직. `BossSkill`, HP Observable 타이머, 보스 HP UI 연동 |

### LobbyScene

| 클래스 | 파일 | 책임 |
|--------|------|------|
| `LBLobbyScene` | `LBLobbyScene.cs` | MonoBehaviour 진입점. `Start`, 버튼 초기화, 다운로드 상태 구독, `StageSelect` |
| `LBLoginHandler` | `LBLoginHandler.cs` | GPGS 로그인/로그아웃. `SetGPGSLogin`, `GetGPGSLogin`, `GetPhoneLoginState` |
| `LBTutorial` | `LBTutorial.cs` | 로비 최초 튜토리얼. `TutorialStart` |

## 제거 항목

| 제거 대상 | 위치 | 대체 |
|-----------|------|------|
| `ChangeBackgroundLoop()` | Game.cs, First.cs | 제거 — 배경 고정 |
| `ChangeBackground()` | Game.cs, First.cs | 제거 — 배경 고정 |
| 배경 관련 `PlayerPrefs` 저장/불러오기 | Game.cs backBtn 핸들러, InitData | 제거 |

## 설계 원칙

- `GPGameScene`이 모든 GP 클래스 인스턴스를 소유하고 생성자로 필요한 참조를 주입
- `GPBombResolver`의 Bomb 메서드는 셀 마킹만 하고 `AfterDrag` 체인은 `GPGameScene`이 단독 책임
- `boardArr`는 `GPBoard`가 소유, 외부는 `GPBoard`를 통해서만 접근
- SerializeField UI 참조는 `GPGameScene`/`LBLobbyScene`이 보유, 각 클래스에 필요한 참조만 생성자로 전달

## 파일 위치

```
Assets/Scripts/Scenes/
  GPGameScene.cs     (Game.cs 대체)
  LBLobbyScene.cs    (First.cs 대체)
Assets/Scripts/GamePlay/
  GPBoard.cs
  GPMatchChecker.cs
  GPBombResolver.cs
  GPTutorial.cs
  GPBossController.cs
Assets/Scripts/Lobby/
  LBLoginHandler.cs
  LBTutorial.cs
```
