using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# M3a Task5 — SimBombFactory 단위 테스트.
    //# 실게임 직접 골든 불가 사유: CreateNewBlock 이 Block.rectTransform.DOScale(DOTween/MonoBehaviour)
    //# 를 직접 호출하므로 헤드리스 EditMode 에서 NRE 발생 확정.
    //# Task2 SimMatchScoreGoldenTests 에서 입력 동등성(hScore/vScore/squareMatch) 보증 완료
    //# → 본 테스트는 생성 분기 로직만 단위 검증.
    public class SimBombFactoryGoldenTests
    {
        //# 가로 4매치 + 이동칸=(0,1) → (0,1) 이 Arrow1(arrowPangIndex=1).
        //# 손 추적: CheckMap 후 Grid[0,0~3].HScore=4. CreateLine(r=0,c=0,len=4):
        //# idx=1(tc=1) → Index==moveIdx1 → Arrow1 배치, placed=true. 나머지 칸 ResetScore.
        [Test]
        public void 생성_가로4매치_이동칸에_Arrow1()
        {
            SimBoard sb = new SimBoard(5);
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                }
            }
            SimMatchChecker sc = new SimMatchChecker();
            int moveIdx = sb.Grid[0, 1].Index;
            sc.SetMoveIndices(moveIdx, -1);
            sc.CheckMap(sb);

            SimBombFactory.CreateBombs(sb, 1, moveIdx, -1);

            Assert.AreEqual(EBlockState.Arrow1, sb.GetState(0, 1), "이동칸에 Arrow1 생성");
        }

        //# 2x2 사각매치 → CatPang 생성.
        //# 손 추적: moveIdx=Grid[0,0].Index=0 → a.SquareMatch=true(실 CheckSquare 이동칸 우선).
        //# Check3: (0,0)행 Cat1×2 → 3미만, 세로도 마찬가지 → 3매치 없음.
        //# 1루프: Grid[0,0].SquareMatch=true → CreateAt(CatPang). 보드 전체 CatPang=1.
        [Test]
        public void 생성_2x2사각매치_CatPang()
        {
            SimBoard sb = new SimBoard(4);
            //# (0,0)(0,1)(1,0)(1,1) Cat1 사각. 나머지는 교대 배치로 3매치 방지.
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
            };
            for (int r = 0; r < 4; ++r)
            {
                for (int c = 0; c < 4; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                }
            }
            SimMatchChecker sc = new SimMatchChecker();
            int moveIdx = sb.Grid[0, 0].Index;
            sc.SetMoveIndices(moveIdx, -1);
            sc.CheckMap(sb);
            SimBombFactory.CreateBombs(sb, 1, moveIdx, -1);

            int catpang = 0;
            for (int r = 0; r < 4; ++r)
            {
                for (int c = 0; c < 4; ++c)
                {
                    if (sb.GetState(r, c) == EBlockState.CatPang)
                    {
                        ++catpang;
                    }
                }
            }
            Assert.AreEqual(1, catpang, "사각매치 → CatPang 1개");
        }
    }
}
