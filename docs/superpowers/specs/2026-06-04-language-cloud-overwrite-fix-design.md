# 언어 설정 클라우드 덮어쓰기 버그 수정 (Design)

- **날짜**: 2026-06-04
- **단계**: 버그 수정 (start-develop-simple)
- **상태**: 승인됨 (사용자 리뷰 대기)

## 1. 문제 (제보)

언어는 **옵션 설정에서 변경할 때만** 바뀌어야 한다. 최초 실행 시 기기 언어로 시작하고, 한 번 옵션으로 바꾸면 다음 앱 실행에도 그 언어로 시작해야 한다. 그러나 **GPGS 로그인 시 언어가 임의로 바뀌는** 제보가 있었다.

## 2. 근본 원인

`Assets/Scripts/Manager/CHMData.cs` `LoadCloudData()`:

- 충돌 병합 루프(line 186-208)는 **일일 미션 필드만** 로컬 우선으로 보존한다.
- line 211 `loginLocalDataDic = loginCloudDataDic = cloudDict;` 에서 클라우드 데이터가 로컬을 통째로 교체한다.
- 이때 `languageType`은 보존 대상이 아니라 **클라우드에 저장돼 있던 언어가 현재(기기/설정) 언어를 덮어쓴다.** → GPGS 로그인 시 언어 변경.

## 3. 정상 동작 확인 (무수정 대상)

| 흐름 | 코드 | 상태 |
|---|---|---|
| 최초 실행 = 기기 언어 | `LoadLocalData` line 44 / `CreateDefaultLogin` line 301 — **신규 유저(로컬 파일 없음, `data.Item1==true`)에만** 기기 언어 적용 | 정상 |
| 옵션에서 언어 변경 | `UISetting` line 51-68 — `loginData.languageType` 설정 후 `SaveData()` (로컬 저장) | 정상 |
| 표시 언어 | `CHMString.GetString` line 34 — `loginData.languageType` 읽어 표시 | 정상 |
| 다음 실행 영속성 | 기존 유저(`data.Item1==false`)는 line 44 리셋 스킵 → 저장된 `languageType` 사용 | 정상 |

→ 즉 영속성은 이미 동작하며, **GPGS 로그인의 클라우드 덮어쓰기만 막으면** 요구사항 전부 충족된다.

## 4. 수정 (단일 지점)

`CHMData.cs` `LoadCloudData()` 병합 루프 안, `cloudLogin` 획득 직후(line 191 부근), 날짜 비교 if/else와 **무관하게 항상**:

```csharp
var cloudLogin = kvp.Value;

//# 언어는 기기 로컬 전용 — 클라우드 값으로 덮어쓰지 않는다 (옵션에서만 변경)
cloudLogin.languageType = localLogin.languageType;
```

- line 211에서 cloudDict로 교체돼도 `languageType`은 로컬 값 유지.
- `localLogin`은 line 188에서 이미 획득(없으면 `continue`).

## 5. 결정 (Decision Lock)

| 항목 | 결정 |
|---|---|
| languageType 성격 | 기기 로컬 전용. 클라우드에서 **로드 시 무시** |
| 클라우드 저장 | 그대로 저장됨(Data.Login에 포함). 저장은 무해, **로드만 무시** (최소 변경) |
| 멀티 디바이스 | 기기마다 독립 언어 (요구사항과 일치) |
| 최초 실행 | 기기 언어 (기존 동작 유지) |
| 옵션 변경 후 영속 | 로컬 저장 + 로드 경로로 이미 보장, GPGS 로그인에도 불변 |

## 6. 검증 (스모크)

1. **재현/수정 확인**: 클라우드에 영어 저장된 계정을 한국어 기기에서 GPGS 로그인 → 로그인 후에도 **한국어 유지**.
2. **옵션 변경 영속**: 옵션에서 영어로 변경 → 앱 재실행 시 영어로 시작 → GPGS 재로그인해도 영어 유지.
3. **최초 실행**: 신규 유저(로컬 파일 없음)는 기기 언어로 시작.
4. **회귀**: 일일 미션 충돌 병합(날짜 비교)은 기존대로 동작.

## 7. 범위 밖 (YAGNI)

- languageType 클라우드 저장 제거 (불필요, 변경폭만 커짐)
- 언어 종류 추가 (Korea/English 외)
- UISetting UI/UX 변경

## 8. 코딩 룰

- `var` 금지(명시적 타입), `!` 금지, 주석 `//#`, 가드 절 형식 (Rule 02). 단, 수정은 기존 `var cloudLogin` 라인 다음에 한 줄 추가 — 신규 추가 줄은 명시적 타입 불요(대입문). 기존 `var` 라인은 미수정 보존.
