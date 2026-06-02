using static Defines;

namespace CatPang.Sim
{
    //# 순수 데이터 블록. 실게임 Block 의 매치/낙하 관련 필드만 미러. Unity 비종속.
    public class SimBlock
    {
        public EBlockState State;
        public int Row;
        public int Col;
        public bool Match;
        public bool SquareMatch;

        //# 일반 고양이 블록 여부. M1 은 Cat1~7 만 사용(스킨/특수블록 비대상).
        public bool IsNormal()
        {
            return State >= EBlockState.Cat1 && State <= EBlockState.Cat7;
        }

        //# 매치 판정 플래그 초기화.
        public void ResetMatch()
        {
            Match = false;
            SquareMatch = false;
        }
    }
}
