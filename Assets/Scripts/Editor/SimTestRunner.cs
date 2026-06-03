using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEditor;

namespace CatPang.Sim.EditorTools
{
    //# MCP(editor_invoke_method)에서 EditMode 테스트를 동기 실행하기 위한 헬퍼.
    //# Unity Test Runner 의 TestRunnerApi 는 결과가 비동기 콜백이라 MCP 단발 호출과 안 맞는다.
    //# 여기서는 NUnit [Test] 메서드를 리플렉션으로 직접 호출해 통과/실패를 문자열로 리턴한다.
    //# (SetUp/TearDown/예외만 처리하는 경량 러너 — 복잡한 NUnit 기능은 미지원. 시뮬 테스트는 순수 단위라 충분.)
    public static class SimTestRunner
    {
        //# 지정 네임스페이스 접두어로 시작하는 모든 테스트 클래스의 [Test] 를 실행.
        //# 반환: "PASS n / FAIL m" 요약 + 실패 상세.
        public static string RunByNamespace(string namespacePrefix)
        {
            StringBuilder sb = new StringBuilder();
            int pass = 0;
            int fail = 0;

            //# 테스트 클래스가 어느 어셈블리에 컴파일됐는지 불확실하므로 로드된 전 어셈블리를 검색한다.
            System.Collections.Generic.List<Type> typeList = new System.Collections.Generic.List<Type>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] candidates;
                try
                {
                    candidates = a.GetTypes();
                }
                catch
                {
                    //# 일부 어셈블리는 GetTypes 에서 로드 예외 — 건너뛴다.
                    continue;
                }

                foreach (Type t in candidates)
                {
                    if (t.IsClass && t.Namespace != null && t.Namespace.StartsWith(namespacePrefix))
                    {
                        typeList.Add(t);
                    }
                }
            }

            Type[] types = typeList.ToArray();
            if (types.Length == 0)
            {
                return $"ERROR: no test class found under namespace '{namespacePrefix}'";
            }

            foreach (Type type in types)
            {
                MethodInfo[] tests = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                    .ToArray();

                if (tests.Length == 0)
                {
                    continue;
                }

                MethodInfo setUp = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttributes(typeof(SetUpAttribute), false).Length > 0);
                MethodInfo tearDown = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttributes(typeof(TearDownAttribute), false).Length > 0);

                foreach (MethodInfo test in tests)
                {
                    object instance = Activator.CreateInstance(type);
                    try
                    {
                        if (setUp != null)
                        {
                            setUp.Invoke(instance, null);
                        }

                        test.Invoke(instance, null);

                        if (tearDown != null)
                        {
                            tearDown.Invoke(instance, null);
                        }

                        pass += 1;
                    }
                    catch (Exception ex)
                    {
                        fail += 1;
                        //# 리플렉션 호출 예외는 InnerException 에 실제 Assert 실패가 담긴다.
                        Exception real = ex.InnerException ?? ex;
                        //# NRE 등 디버깅 위해 스택트레이스 첫 줄(발생 위치)도 첨부.
                        string firstFrame = "";
                        if (real.StackTrace != null)
                        {
                            string[] frames = real.StackTrace.Split('\n');
                            if (frames.Length > 0)
                            {
                                firstFrame = " @ " + frames[0].Trim();
                            }
                        }
                        sb.Append($"\nFAIL {type.Name}.{test.Name}: {real.GetType().Name} {real.Message}{firstFrame}");
                    }
                }
            }

            string summary = $"PASS {pass} / FAIL {fail}";
            return summary + sb.ToString();
        }

        //# CatPang.Sim.Tests 네임스페이스 전체 실행 (MCP 호출 진입점).
        public static string RunSimTests()
        {
            return RunByNamespace("CatPang.Sim.Tests");
        }
    }
}
