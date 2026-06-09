using NUnit.Framework;
using ChvjUnityInfra;

namespace CatPang.Tests.EditMode
{
    //# AppUpdatePolicy.Decide 경계값 회귀 스위트.
    //# 대상 production: Packages/com.chvj.unityinfra/Runtime/Core/AppUpdatePolicy.cs
    //# 관련 plan : docs/superpowers/plans/2026-06-09-in-app-updates.md (Task 1)
    //# production AppUpdatePolicy는 패키지 어셈블리(com.chvj.unityinfra)에, 본 테스트는
    //# predefined 테스트 어셈블리에 컴파일된다. Decide는 플러그인 미설치(심볼 OFF)에도 항상 실행 가능.
    public class AppUpdatePolicyTests
    {
        [Test]
        public void 업데이트없음시_None반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(updateAvailable: false, priority: 5, immediateAllowed: true, flexibleAllowed: true);
            Assert.AreEqual(EAppUpdateAction.None, r);
        }

        [Test]
        public void 우선순위4_Immediate허용시_Immediate반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 4, true, true);
            Assert.AreEqual(EAppUpdateAction.Immediate, r);
        }

        [Test]
        public void 우선순위5_Immediate허용시_Immediate반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 5, true, false);
            Assert.AreEqual(EAppUpdateAction.Immediate, r);
        }

        [Test]
        public void 우선순위3_Flexible허용시_Flexible반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 3, true, true);
            Assert.AreEqual(EAppUpdateAction.Flexible, r);
        }

        [Test]
        public void 우선순위0_Flexible허용시_Flexible반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 0, false, true);
            Assert.AreEqual(EAppUpdateAction.Flexible, r);
        }

        [Test]
        public void 우선순위4_Immediate불가_Flexible허용시_Flexible로폴백()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 4, false, true);
            Assert.AreEqual(EAppUpdateAction.Flexible, r);
        }

        [Test]
        public void 우선순위4_둘다불가시_None반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 4, false, false);
            Assert.AreEqual(EAppUpdateAction.None, r);
        }

        [Test]
        public void 우선순위2_Flexible불가시_None반환()
        {
            EAppUpdateAction r = AppUpdatePolicy.Decide(true, 2, true, false);
            Assert.AreEqual(EAppUpdateAction.None, r);
        }
    }
}
