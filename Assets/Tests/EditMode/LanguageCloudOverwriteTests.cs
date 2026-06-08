using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace CatPang.Tests.EditMode
{
    //# 언어 설정 클라우드 덮어쓰기 버그 수정 회귀 스위트.
    //# 대상 production: Assets/Scripts/Manager/CHMData.cs LoadCloudData() 병합 루프 (line 186~211)
    //#   추가된 보존 라인: cloudLogin.languageType = localLogin.languageType;
    //# 관련 spec : docs/superpowers/specs/2026-06-04-language-cloud-overwrite-fix-design.md (§4 수정 지점, §6 스모크)
    //# 관련 plan : docs/superpowers/plans/2026-06-04-language-cloud-overwrite-fix.md (Task 1)
    //# 관련 기획 : docs/design/language-cloud-overwrite-fix.md (§3 검증 C1~C5, §2 엣지 E1~E4)
    //#
    //# ── 테스트 가능성 판정 (접근 B) ───────────────────────────────────────────────
    //#   production 병합 경로(LoadCloudData)는 두 겹으로 EditMode 단위 테스트가 불가능하다:
    //#   (1) #if UNITY_ANDROID 컴파일 게이트 — 본 asmdef 는 includePlatforms:["Editor"] 라
    //#       Windows 에디터에서 UNITY_ANDROID 미정의 → LoadCloudData 자체가 컴파일 제외된다.
    //#   (2) ChvjUnityInfra.CHMGPGS.Instance.LoadCloud() 실 GPGS I/O 에 의존하고, 병합 로직이
    //#       메서드 내부에 인라인(별도 순수 함수 추출 없음) → 주입/추출 seam 부재.
    //#   따라서 production 의 실제 병합 경로를 EditMode 에서 호출할 수 없다. 대신:
    //#   (a) Data.Login.languageType 필드 존재/타입 회귀 가드 (수정의 전제 — 필드가 사라지거나
    //#       타입이 바뀌면 production 의 대입 라인이 깨진다).
    //#   (b) 병합 규칙("cloudLogin.languageType = localLogin.languageType")을 실제 Data.Login 으로
    //#       재현하여 로컬 우선 불변식을 박제하고, spec §4 의 보존 순서·가드(continue)·일일미션
    //#       독립성 계약을 고정한다. 재현 변환은 production 라인과 1:1 동일한 한 줄이다.
    //#
    //# ── 어셈블리 제약 ────────────────────────────────────────────────────────────
    //#   Data.Login (namespace Data) 은 Assets/Scripts/ 에 asmdef 가 없어 Assembly-CSharp 에 속한다.
    //#   본 테스트 asmdef 는 overrideReferences:true 로 Assembly-CSharp 를 직접 참조할 수 없으므로
    //#   (Unity 제약, BlockScaleNormalizeTests 선례), 리플렉션으로 실제 production 타입을 로드해
    //#   필드를 조작/검증한다. production 코드 미수정.
    public class LanguageCloudOverwriteTests
    {
        //# Assembly-CSharp 의 production 타입 (리플렉션 캐시)
        private Type _loginType;
        private FieldInfo _languageTypeField;
        private FieldInfo _lastDailyResetDateKeyField;
        private FieldInfo _stageClearCountTodayField;
        private FieldInfo _blockDestroyCountTodayField;
        private FieldInfo _adWatchCountTodayField;
        private FieldInfo _attendanceTodayDoneField;
        private FieldInfo _dailyCollectionSnapshotJsonField;

        //# Defines.ELanguageType (Assembly-CSharp)
        private Type _languageEnumType;
        private object _korea;
        private object _english;

        [SetUp]
        public void SetUp()
        {
            _loginType = FindType("Data.Login");
            Assert.IsNotNull(_loginType, "Data.Login 타입을 Assembly-CSharp 에서 찾지 못함");

            _languageTypeField = _loginType.GetField("languageType", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(_languageTypeField,
                "Data.Login.languageType 필드가 존재해야 한다 (production 보존 라인의 전제). " +
                "필드가 없으면 CHMData.LoadCloudData 의 'cloudLogin.languageType = localLogin.languageType' 가 깨진다.");

            _lastDailyResetDateKeyField = _loginType.GetField("lastDailyResetDateKey", BindingFlags.Public | BindingFlags.Instance);
            _stageClearCountTodayField = _loginType.GetField("stageClearCountToday", BindingFlags.Public | BindingFlags.Instance);
            _blockDestroyCountTodayField = _loginType.GetField("blockDestroyCountToday", BindingFlags.Public | BindingFlags.Instance);
            _adWatchCountTodayField = _loginType.GetField("adWatchCountToday", BindingFlags.Public | BindingFlags.Instance);
            _attendanceTodayDoneField = _loginType.GetField("attendanceTodayDone", BindingFlags.Public | BindingFlags.Instance);
            _dailyCollectionSnapshotJsonField = _loginType.GetField("dailyCollectionSnapshotJson", BindingFlags.Public | BindingFlags.Instance);

            _languageEnumType = FindType("Defines+ELanguageType");
            Assert.IsNotNull(_languageEnumType, "Defines.ELanguageType 타입을 찾지 못함");
            Assert.AreEqual(_languageEnumType, _languageTypeField.FieldType,
                "languageType 필드 타입은 Defines.ELanguageType 여야 한다 (회귀 가드)");

            _korea = Enum.Parse(_languageEnumType, "Korea");
            _english = Enum.Parse(_languageEnumType, "English");
        }

        private static Type FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(name);
                if (t != null)
                    return t;
            }
            return null;
        }

        //# Data.Login 인스턴스 생성 + 언어 설정
        private object NewLogin(string key, object languageType)
        {
            object login = Activator.CreateInstance(_loginType);
            _loginType.GetField("key", BindingFlags.Public | BindingFlags.Instance).SetValue(login, key);
            _languageTypeField.SetValue(login, languageType);
            return login;
        }

        private object GetLanguage(object login) => _languageTypeField.GetValue(login);

        //# ── production 병합 규칙의 1:1 재현 ─────────────────────────────────────────
        //# CHMData.LoadCloudData line 186~211 의 충돌 병합 루프를 그대로 모사한다.
        //# 이 헬퍼가 production 과 어긋나면 회귀 테스트로서 의미가 없으므로, 라인을
        //# spec §4 / plan Task 1 의 코드와 동일하게 유지한다 (변경 시 동기화 필수).
        private void MergeCloudIntoLocal(
            Dictionary<string, object> loginLocalDataDic,
            Dictionary<string, object> cloudDict)
        {
            //# production line 184: 로컬 dict 가 null 이면 병합 루프 전체 스킵
            if (loginLocalDataDic == null)
                return;

            foreach (KeyValuePair<string, object> kvp in cloudDict)
            {
                //# production line 188~189: 로컬에 키가 없으면 continue
                if (loginLocalDataDic.TryGetValue(kvp.Key, out object localLogin) == false)
                    continue;

                object cloudLogin = kvp.Value;

                //# production line 193~194: 언어는 기기 로컬 전용 — 날짜 비교와 무관하게 항상 실행
                _languageTypeField.SetValue(cloudLogin, _languageTypeField.GetValue(localLogin));

                //# production line 197~210: 날짜 비교 기반 일일 미션 필드 보존
                string localKey = (string)_lastDailyResetDateKeyField.GetValue(localLogin);
                string cloudKey = (string)_lastDailyResetDateKeyField.GetValue(cloudLogin);
                if (string.Compare(localKey, cloudKey, StringComparison.Ordinal) > 0)
                {
                    _lastDailyResetDateKeyField.SetValue(cloudLogin, _lastDailyResetDateKeyField.GetValue(localLogin));
                    _stageClearCountTodayField.SetValue(cloudLogin, _stageClearCountTodayField.GetValue(localLogin));
                    _blockDestroyCountTodayField.SetValue(cloudLogin, _blockDestroyCountTodayField.GetValue(localLogin));
                    _adWatchCountTodayField.SetValue(cloudLogin, _adWatchCountTodayField.GetValue(localLogin));
                    _attendanceTodayDoneField.SetValue(cloudLogin, _attendanceTodayDoneField.GetValue(localLogin));
                    _dailyCollectionSnapshotJsonField.SetValue(cloudLogin, _dailyCollectionSnapshotJsonField.GetValue(localLogin));
                }
            }
        }

        //# ════════════════════════════════════════════════════════════════════════════
        //# (a) 회귀 가드 — production 보존 라인의 전제(필드/타입) 고정
        //#   필드/타입은 SetUp 에서 단언하지만, 명시적 케이스로도 박제해 둔다.
        //# ════════════════════════════════════════════════════════════════════════════

        [Test]
        public void Login_languageType_필드가_존재하고_타입이_ELanguageType이다()
        {
            Assert.IsNotNull(_languageTypeField, "languageType 필드 존재 (보존 라인 전제)");
            Assert.AreEqual(_languageEnumType, _languageTypeField.FieldType, "타입 = Defines.ELanguageType");
        }

        [Test]
        public void ELanguageType은_Korea와_English_두_값을_가진다()
        {
            //# 기획서 §1.1: 2종 언어 정책. 값이 변하면 매핑/보존 가정이 흔들린다.
            string[] names = Enum.GetNames(_languageEnumType);
            CollectionAssert.Contains(names, "Korea", "ELanguageType 에 Korea 존재");
            CollectionAssert.Contains(names, "English", "ELanguageType 에 English 존재");
        }

        //# ════════════════════════════════════════════════════════════════════════════
        //# (b) 병합 규칙 계약 — 로컬 우선 불변식 (spec §4, 기획 C1~C5)
        //# ════════════════════════════════════════════════════════════════════════════

        //# 계약 1 — 언어 보존 (C1 / E1: 한국어 기기에서 영어 클라우드 로그인 → 한국어 유지)
        [Test]
        public void 로컬한국어_클라우드영어_병합후_한국어유지()
        {
            object local = NewLogin("user1", _korea);
            object cloud = NewLogin("user1", _english);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            //# 병합 후 cloudDict 가 로컬을 교체하므로(production line 214) cloud 엔트리의 언어가 결과 언어다.
            Assert.AreEqual(_korea, GetLanguage(cloud),
                "클라우드 영어가 로컬 한국어를 덮어쓰면 안 된다 (버그 수정 핵심)");
        }

        //# 역방향 — 로컬 영어 / 클라우드 한국어 → 영어 유지 (옵션 변경 후 영속, C2/C3/E2)
        [Test]
        public void 로컬영어_클라우드한국어_병합후_영어유지()
        {
            object local = NewLogin("user1", _english);
            object cloud = NewLogin("user1", _korea);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            Assert.AreEqual(_english, GetLanguage(cloud),
                "옵션에서 영어로 바꾼 기기는 클라우드 한국어 로드에도 영어 유지");
        }

        //# 동일 언어 — 자기 자신 보존(영어→영어), 변화 없음 (E2 추가 메모)
        [Test]
        public void 로컬영어_클라우드영어_병합후_영어유지()
        {
            object local = NewLogin("user1", _english);
            object cloud = NewLogin("user1", _english);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            Assert.AreEqual(_english, GetLanguage(cloud), "같은 언어면 그대로 유지");
        }

        //# 계약 2 — 로컬 키 부재 시 continue (보존 미실행, NRE 없음)
        [Test]
        public void 클라우드키가_로컬에없으면_continue로_건너뛴다_NRE없음()
        {
            //# 로컬에는 user1 만, 클라우드에는 user2(로컬에 없는 키)만.
            object local = NewLogin("user1", _korea);
            object cloudOnly = NewLogin("user2", _english);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user2", cloudOnly } };

            //# NRE 없이 통과해야 한다. localLogin 미획득 → 보존 라인 미실행.
            Assert.DoesNotThrow(() => MergeCloudIntoLocal(localDic, cloudDic),
                "로컬에 없는 클라우드 키는 continue — NRE 발생 금지");

            //# 클라우드 전용 엔트리는 손대지 않음 (보존 라인 미실행 → 영어 그대로)
            Assert.AreEqual(_english, GetLanguage(cloudOnly), "로컬 미존재 키는 언어 보존 미실행 → 클라우드 값 유지");
        }

        //# 계약 2b — 여러 키 혼재: 매칭 키만 보존, 비매칭은 continue
        [Test]
        public void 매칭키만_언어보존되고_비매칭키는_건드리지않는다()
        {
            object localA = NewLogin("A", _korea);   //# 매칭
            object cloudA = NewLogin("A", _english);
            object cloudB = NewLogin("B", _english); //# 로컬에 없음

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "A", localA } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object>
            {
                { "A", cloudA },
                { "B", cloudB },
            };

            MergeCloudIntoLocal(localDic, cloudDic);

            Assert.AreEqual(_korea, GetLanguage(cloudA), "매칭 키 A 는 로컬 언어로 보존");
            Assert.AreEqual(_english, GetLanguage(cloudB), "비매칭 키 B 는 보존 미실행 (continue)");
        }

        //# 계약 3 — 일일 미션 병합 무간섭 (C5)
        //#   언어 보존은 날짜 비교 if/else 앞에서 무조건 실행되므로, 날짜 기반 일일 필드 보존과
        //#   languageType 보존이 서로 독립적으로 동작해야 한다.

        //# 3-a: 로컬 날짜가 더 미래(로컬 우선) — 일일 필드는 로컬 값으로, 언어도 로컬 값으로
        [Test]
        public void 로컬날짜미래_일일필드는로컬우선이고_언어도로컬보존_독립동작()
        {
            object local = NewLogin("user1", _korea);
            _lastDailyResetDateKeyField.SetValue(local, "20260604");
            _stageClearCountTodayField.SetValue(local, 7);
            _blockDestroyCountTodayField.SetValue(local, 120);
            _adWatchCountTodayField.SetValue(local, 2);
            _attendanceTodayDoneField.SetValue(local, true);
            _dailyCollectionSnapshotJsonField.SetValue(local, "{\"18\":42}");

            object cloud = NewLogin("user1", _english);
            _lastDailyResetDateKeyField.SetValue(cloud, "20260601"); //# 클라우드가 과거
            _stageClearCountTodayField.SetValue(cloud, 0);
            _blockDestroyCountTodayField.SetValue(cloud, 0);
            _adWatchCountTodayField.SetValue(cloud, 0);
            _attendanceTodayDoneField.SetValue(cloud, false);
            _dailyCollectionSnapshotJsonField.SetValue(cloud, "");

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            //# 일일 필드: 로컬 우선 (기존 동작 — 회귀)
            Assert.AreEqual("20260604", _lastDailyResetDateKeyField.GetValue(cloud), "로컬 날짜 미래 → 날짜 키 로컬 우선");
            Assert.AreEqual(7, _stageClearCountTodayField.GetValue(cloud), "stageClearCountToday 로컬 우선");
            Assert.AreEqual(120, _blockDestroyCountTodayField.GetValue(cloud), "blockDestroyCountToday 로컬 우선");
            Assert.AreEqual(2, _adWatchCountTodayField.GetValue(cloud), "adWatchCountToday 로컬 우선");
            Assert.AreEqual(true, _attendanceTodayDoneField.GetValue(cloud), "attendanceTodayDone 로컬 우선");
            Assert.AreEqual("{\"18\":42}", _dailyCollectionSnapshotJsonField.GetValue(cloud), "스냅샷 로컬 우선");
            //# 언어: 로컬 보존 (독립)
            Assert.AreEqual(_korea, GetLanguage(cloud), "언어도 로컬 보존 (일일 병합과 독립)");
        }

        //# 3-b: 클라우드 날짜가 같거나 미래(클라우드 우선) — 일일 필드는 클라우드 유지, 언어는 그래도 로컬 보존
        [Test]
        public void 클라우드날짜우선_일일필드는클라우드유지여도_언어는로컬보존된다()
        {
            object local = NewLogin("user1", _korea);
            _lastDailyResetDateKeyField.SetValue(local, "20260601"); //# 로컬이 과거
            _stageClearCountTodayField.SetValue(local, 99);          //# 무시되어야 함

            object cloud = NewLogin("user1", _english);
            _lastDailyResetDateKeyField.SetValue(cloud, "20260604"); //# 클라우드가 미래
            _stageClearCountTodayField.SetValue(cloud, 3);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            //# 일일 필드: 클라우드 우선 (날짜 비교 else 분기 — 로컬 값 미반영)
            Assert.AreEqual("20260604", _lastDailyResetDateKeyField.GetValue(cloud), "클라우드 날짜 우선 → 날짜 키 클라우드 유지");
            Assert.AreEqual(3, _stageClearCountTodayField.GetValue(cloud), "클라우드 우선 → stageClearCountToday 클라우드 유지");
            //# 언어: 날짜 비교 분기와 무관하게 무조건 로컬 보존
            Assert.AreEqual(_korea, GetLanguage(cloud),
                "클라우드 날짜 우선이어도 언어는 로컬 보존 (보존 라인이 if/else 앞에서 무조건 실행)");
        }

        //# 3-c: 동일 날짜 경계값 — Compare == 0 은 else(클라우드 우선) 경로, 언어는 여전히 로컬 보존
        [Test]
        public void 동일날짜_경계값에서_else경로여도_언어는로컬보존된다()
        {
            object local = NewLogin("user1", _korea);
            _lastDailyResetDateKeyField.SetValue(local, "20260604");
            _stageClearCountTodayField.SetValue(local, 50);

            object cloud = NewLogin("user1", _english);
            _lastDailyResetDateKeyField.SetValue(cloud, "20260604"); //# 동일 → Compare == 0 → else
            _stageClearCountTodayField.SetValue(cloud, 5);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            //# Compare == 0 은 > 0 이 아니므로 else → 클라우드 일일 값 유지
            Assert.AreEqual(5, _stageClearCountTodayField.GetValue(cloud), "동일 날짜는 else(클라우드 우선) 경로");
            Assert.AreEqual(_korea, GetLanguage(cloud), "경계값에서도 언어 로컬 보존");
        }

        //# 계약 4 — 순서 의존성 고정 (E3 / C4)
        //#   보존은 loginLocalDataDic 가 채워진 상태(LoadLocalData 선행)에 의존한다.
        //#   production line 184 가드(loginLocalDataDic != null)가 null 이면 병합 전체 스킵.

        //# 4-a: 로컬 dict 가 null 이면 병합 스킵 (언어 보존 미실행) — 순서 의존성 회귀 박제
        [Test]
        public void 로컬dict가_null이면_병합스킵되어_언어보존이_실행되지않는다()
        {
            object cloud = NewLogin("user1", _english);
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            //# loginLocalDataDic == null → production line 184 가드로 루프 전체 스킵
            Assert.DoesNotThrow(() => MergeCloudIntoLocal(null, cloudDic),
                "로컬 dict null 이어도 NRE 없이 스킵");
            Assert.AreEqual(_english, GetLanguage(cloud),
                "로컬 dict null 이면 보존 미실행 → 클라우드 값 그대로 (LoadLocalData 선행이 보존의 전제). " +
                "이 케이스가 깨지면 부팅 순서(LoadLocalData→LoadCloudData) 의존성이 바뀐 것.");
        }

        //# 4-b: 로컬 dict 가 채워진 정상 순서 — 보존 실행 (E3 정상 경로)
        [Test]
        public void 로컬dict가_채워진_정상순서면_언어보존이_실행된다()
        {
            object local = NewLogin("user1", _korea);
            object cloud = NewLogin("user1", _english);

            Dictionary<string, object> localDic = new Dictionary<string, object> { { "user1", local } };
            Dictionary<string, object> cloudDic = new Dictionary<string, object> { { "user1", cloud } };

            MergeCloudIntoLocal(localDic, cloudDic);

            Assert.AreEqual(_korea, GetLanguage(cloud),
                "LoadLocalData 선행으로 로컬 dict 채워진 정상 순서 → 보존 실행 (신규 유저 첫 GPGS 로그인 E3 포함)");
        }
    }
}
