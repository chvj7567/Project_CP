namespace ChvjUnityInfra
{
    //# 부팅 시 업데이트 처리 액션
    public enum EAppUpdateAction
    {
        None,
        Immediate,
        Flexible,
    }

    //# 앱 재개(resume) 시 미완료 업데이트 재확인 결과 (spec §5.1)
    public enum EAppUpdateResume
    {
        None,
        //# Immediate가 진행 중이었음 → Immediate 재개 필요
        ImmediateInProgress,
        //# Flexible 다운로드가 이미 완료됨 → 재시작 안내 필요
        FlexibleDownloaded,
    }

    //# In-App Update 우선순위 → 처리 액션 판정. Google.Play 타입에 의존하지 않는 순수 로직.
    public static class AppUpdatePolicy
    {
        //# 이 값 이상 우선순위면 강제(Immediate), 미만이면 권장(Flexible)
        public const int ImmediateThreshold = 4;

        public static EAppUpdateAction Decide(bool updateAvailable, int priority, bool immediateAllowed, bool flexibleAllowed)
        {
            if (updateAvailable == false)
                return EAppUpdateAction.None;

            if (priority >= ImmediateThreshold && immediateAllowed)
                return EAppUpdateAction.Immediate;

            if (flexibleAllowed)
                return EAppUpdateAction.Flexible;

            return EAppUpdateAction.None;
        }
    }
}
