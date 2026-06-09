# Google Play In-App Updates 연동 — 설계 (spec)

- **작성일**: 2026-06-09
- **단계**: Beta
- **대상 플랫폼**: Android 전용 (Google Play)
- **방법**: 방법 1 — Google Play In-App Updates API (서버 불필요)

## 1. 목적 / 의도

플레이어가 구버전 앱을 실행할 때, **앱 안에서** 스토어 최신 버전을 감지하여 강제(Immediate) 또는 권장(Flexible) 업데이트 흐름을 띄운다. 별도 운영 서버 없이 Google Play가 보유한 버전 정보를 사용한다.

- 강제/권장 분기는 **출시 시 Play Console(또는 Play Developer API)에서 지정하는 업데이트 우선순위(0~5)** 로 통제 — 코드 재배포 없이 정책 조정 가능.
- In-App Updates의 핵심 이점인 "앱 내 완결"을 유지한다 — 스토어 웹/앱 페이지로 이탈시키지 않는다.

## 2. 범위

### 포함
- 부팅 시 1회 업데이트 체크 + 우선순위 기반 Immediate/Flexible 분기
- Immediate 흐름(구글 전체화면 네이티브 UI) + 유저 취소 시 인앱 재요청
- Flexible 흐름(백그라운드 다운로드) + 다운로드 완료 시 게임 내 재시작 안내(UIConfirm)
- 앱 재개(resume) 시 미완료 업데이트 처리
- 플러그인 미설치 시 빌드 안전을 위한 컴파일 게이팅(`UNITY_INFRA_APPUPDATE`)
- 에디터/사이드로드 환경 무동작 처리

### 범위 밖 (YAGNI)
- iOS (앱스토어는 In-App Update 개념 없음)
- 서버/Remote Config 기반 버전 비교(방법 2)
- 우선순위/staleness 자체를 앱에서 정하는 로직 — 이는 출시 메타데이터의 책임

## 3. 전제 / 의존성

- **Google "In-App Updates" Unity 플러그인(`.unitypackage`)은 사용자가 수동 임포트해야 한다.** Google `play-unity-plugins`로 배포되며 코드/툴로 자동 설치 불가.
  - 제공 어셈블리: `Google.Play.AppUpdate`, `Google.Play.Common`, `Google.Play.Core`
- 기존 GPGS(`GooglePlayGames`)·AdMob과 동일 계열(EDM4U 사용)이라 충돌 없음.
- 활성화 절차(사용자):
  1. In-App Updates `.unitypackage` 임포트
  2. `Tools/ChvjUnityInfra/Settings` → "App Update" 탭 → "Use App Update" 체크 (`UNITY_INFRA_APPUPDATE` 심볼 추가)

## 4. 아키텍처 (접근 1 — 패키지 옵트인 모듈)

기존 옵트인 모듈(`CHMAdmob`/`CHMIAP`/`CHMGPGS`)과 동일한 패턴을 따른다.

### 4.1 신규 — 패키지 모듈
`Packages/com.chvj.unityinfra/Runtime/AppUpdate/`

| 파일 | 내용 |
|---|---|
| `CHMAppUpdate.cs` | `CHSingletonStatic<CHMAppUpdate>`. In-App Updates API 래퍼. 전체 본문을 `#if UNITY_INFRA_APPUPDATE`로 감싼다(`CHMGPGS`가 `#if UNITY_ANDROID`로 감싸는 것과 동일 형태). |
| `com.chvj.unityinfra.appupdate.asmdef` | `references`: `com.chvj.unityinfra`, `Google.Play.AppUpdate`, `Google.Play.Common`, `Google.Play.Core`. `includePlatforms`: `Android`, `Editor`. `defineConstraints`: `["UNITY_INFRA_APPUPDATE"]`. |

> `defineConstraints` 덕분에 심볼이 꺼져 있으면 어셈블리 자체가 컴파일되지 않는다 → 플러그인 미설치 상태에서도 누락 참조 에러가 발생하지 않는다. 이것이 게이팅의 핵심.

### 4.2 수정 — 에디터 설정창
`Packages/com.chvj.unityinfra/Editor/ChvjUnityInfraSettingsWindow.cs`
- `APPUPDATE_DEFINE = "UNITY_INFRA_APPUPDATE"` 상수 추가
- `TabLabels`에 `"App Update"` 추가 + `DrawAppUpdateTab()` 구현 (토글 + `#if`로 갈린 사용 가이드 — 다른 탭과 동일 톤)

### 4.3 수정 — 게임 부팅
`Assets/Scripts/Scenes/ResourceDownload.cs`
- `CHMMain.EnsureInitialized()` 직후 ~ `SceneManager.LoadScene(FirstScene)` 직전에 업데이트 체크 await를 끼운다.
- 호출부 전체를 `#if UNITY_INFRA_APPUPDATE`로 감싼다 → 심볼 꺼지면 기존 부팅 흐름과 동일.
- 체크 시점 근거: 이 지점은 CHMUI가 초기화 완료된 상태라 UIConfirm 사용 가능. 강제 업데이트 시 직전 프리로드가 일부 낭비되지만, 강제 업데이트는 드물어 수용 가능한 트레이드오프.

## 5. 동작 흐름 (CHMAppUpdate)

```
ImmediateThreshold = 4   // 우선순위 4~5 = 강제, 0~3 = 권장 (상수, 추후 조정 가능)

async Task CheckAndRunAsync():
  #if UNITY_EDITOR → 즉시 return (에디터 무동작)
  info = await GetAppUpdateInfo()
  if info.UpdateAvailability != UpdateAvailable → return   // 사이드로드 포함 무동작

  if info.UpdatePriority >= ImmediateThreshold && info.IsImmediateUpdateAllowed:
      await StartImmediate(info)
  else if info.IsFlexibleUpdateAllowed:
      StartFlexible(info)   // await 하지 않음 — 백그라운드, 게임은 계속 진입

StartImmediate(info):
  result = await manager.StartUpdate(info, ImmediateAppUpdateOptions)
  if result == 취소/실패:
      UIConfirm(Confirm, title="업데이트 필요", desc="...",
                onYes = () => StartImmediate(info))   // A-1: 인앱 재요청 (스토어 페이지 X)

StartFlexible(info):
  manager.StartUpdate(info, FlexibleAppUpdateOptions)
  설치 상태 리스너 등록:
      onDownloaded → UIConfirm(YesNo, title="업데이트 준비 완료", desc="지금 재시작?",
                              onYes = () => manager.CompleteUpdate())
```

### 5.1 Resume 처리
앱 재개 시 미완료 업데이트를 재확인한다(구글 권장).
- Immediate가 진행 중(`UpdateAvailability == DeveloperTriggeredUpdateInProgress`)이었으면 → Immediate 재개
- Flexible이 이미 `Downloaded` 상태면 → 완료 안내 UIConfirm

### 5.2 UI — UIConfirm 재사용
기존 `UIConfirm`(`Assets/Scripts/UI/UIConfirm.cs`) 사용. 게임 측에서 `CHMUI.ShowUI(EUI.UIConfirm, UIConfirmArg)`로 띄운다.
- 강제 취소: `EConfirmType.Confirm` (확인 버튼만)
- Flexible 완료: `EConfirmType.YesNo`
- **문구는 하드코딩 금지** — `CHMString` 상수 + `CHMJson` 한/영 사전에 키 추가.

> 패키지 모듈은 게임 UI(`EUI.UIConfirm`)를 직접 알지 못한다(역방향 의존 금지, Rule 03 §1). 따라서 UIConfirm 표시는 **게임 측 부팅 코드에서** 처리하고, `CHMAppUpdate`는 콜백(`Action onForcedUpdateCancelled`, `Action onFlexibleDownloaded` 등) 또는 결과 enum을 게임에 통지하는 방식으로 분리한다. 정확한 콜백 시그니처는 plan 단계에서 확정.

## 6. 정책 파라미터

| 파라미터 | 기본값 | 설명 |
|---|---|---|
| `ImmediateThreshold` | 4 | 이 값 이상 우선순위면 Immediate, 미만이면 Flexible |
| 우선순위(0~5) | 출시 시 지정 | Play Console / Play Developer API `inAppUpdatePriority` |
| staleness days | 출시 시 지정 | 필요 시 추가 분기 조건 (이번 범위에선 우선순위만 사용) |

## 7. 테스트 전략

In-App Updates는 실기기 + Play 트랙(내부 테스트 포함)에서만 실동작하므로:
- **단위 테스트(EditMode)**: 우선순위 → Immediate/Flexible 판정을 **순수 함수**로 분리(`DecideUpdateType(priority, immediateAllowed, flexibleAllowed)` 등)하여 분기 경계값(3/4/5, allowed 조합) 검증.
- **네이티브 호출부**: 자동 테스트 불가 → 수동 QA 체크리스트로 문서화(내부 테스트 트랙 업로드 후 구버전→신버전 시나리오).

## 8. 에러 / 엣지 케이스

- 플러그인 미설치 + 심볼 꺼짐 → 어셈블리 비컴파일, 부팅 흐름 무변경 (정상)
- 네트워크 불가 → `GetAppUpdateInfo` 실패/타임아웃 → 조용히 패스(게임 진입 막지 않음)
- 사이드로드/에디터 → `UpdateNotAvailable` → 무동작
- 강제 업데이트 반복 취소 → 매 취소마다 UIConfirm 재요청(무한 통과 방지)
- Flexible 다운로드 중 게임 종료 → 다음 부팅/재개 시 재확인

## 9. 결정 락 (Locked Decisions)

1. 정책: **우선순위 기반 분기** (≥4 Immediate, 그 외 Flexible)
2. Flexible 완료: **UIConfirm 재사용**으로 재시작 안내
3. 강제 취소: **UIConfirm 안내 후 인앱 업데이트 재요청** (스토어 페이지 이동 안 함)
4. 아키텍처: **ChvjPackage 옵트인 모듈**(`UNITY_INFRA_APPUPDATE` 게이팅)
5. 체크 시점: 부팅 `CHMMain` 초기화 후 ~ `FirstScene` 로드 직전
6. Android 전용, 문구는 로컬라이즈, 플러그인은 사용자 수동 임포트
