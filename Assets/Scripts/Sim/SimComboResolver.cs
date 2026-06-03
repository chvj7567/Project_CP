using static Defines;

namespace CatPang.Sim
{
    //# 실 GPGameScene.AfterDrag L787~805 조합 분기 포팅(순수 판정). 부수효과는 SimGame 이 수행.
    public static class SimComboResolver
    {
        public enum EComboResult
        {
            None,
            BoomAll,           //# 색폭탄 2개 → 전체 폭발
            Boom3,             //# PinkBomb + 임의 → 상대 색 전부 제거
            DetonateA,         //# a 단독 발동
            DetonateB,         //# b 단독 발동
            MergeToColorBomb,  //# 화살표/CatPang 2개 → 색폭탄 승급
        }

        //# a, b: 스왑된 두 블록. 결과만 반환. 순서는 실게임 그대로 (special 검사가 bomb 검사보다 먼저).
        //# PinkBomb 분기 주의: Pink + 색폭탄은 789 보다 787 이 먼저 → BoomAll.
        //# 비특수 폭탄 + 색폭탄: 797 보다 795 가 먼저 → DetonateB (병합 아님).
        public static EComboResult Resolve(SimBlock a, SimBlock b)
        {
            if (a == null)
                return EComboResult.None;
            if (b == null)
                return EComboResult.None;

            //# 1. 색폭탄 2개 (L787) — Pink 검사보다 먼저
            if (a.IsSpecialBomb() && b.IsSpecialBomb())
                return EComboResult.BoomAll;

            //# 2. 한쪽이 PinkBomb (L789~792) — 나머지 special 검사보다 먼저
            if (a.State == EBlockState.PinkBomb || b.State == EBlockState.PinkBomb)
                return EComboResult.Boom3;

            //# 3. 한쪽만 색폭탄 (L793~796)
            if (a.IsSpecialBomb())
                return EComboResult.DetonateA;
            if (b.IsSpecialBomb())
                return EComboResult.DetonateB;

            //# 4. 화살표/CatPang 2개 → 색폭탄 승급 (L797~803)
            if (a.IsBomb() && b.IsBomb())
                return EComboResult.MergeToColorBomb;

            //# 5. 한쪽만 폭탄 (L804~805)
            if (a.IsBomb())
                return EComboResult.DetonateA;
            if (b.IsBomb())
                return EComboResult.DetonateB;

            return EComboResult.None;
        }
    }
}
