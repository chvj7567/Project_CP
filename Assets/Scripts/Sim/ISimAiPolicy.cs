using System;
using System.Collections.Generic;

namespace CatPang.Sim
{
    //# 한 수 = (Row,Col) 칸을 (Dr,Dc) 방향 인접칸과 swap.
    public struct SimMove
    {
        public int Row, Col, Dr, Dc;

        public SimMove(int r, int c, int dr, int dc)
        {
            Row = r;
            Col = c;
            Dr = dr;
            Dc = dc;
        }
    }

    //# AI 수 선택 인터페이스. 유효 수가 없으면 null 반환.
    public interface ISimAiPolicy
    {
        string Name { get; }
        SimMove? ChooseMove(SimBoard board, SimMatchChecker checker);
    }

    //# 유효 수 탐색/매치수 계산 헬퍼. swap 은 항상 복원하고 ResetAllMatch 로 상태 누수 방지.
    internal static class SimMoveFinder
    {
        //# 우/하만 검사(좌/상은 대칭 중복).
        private static readonly (int dr, int dc)[] Dirs = { (1, 0), (0, 1) };

        public static List<SimMove> FindValidMoves(SimBoard board, SimMatchChecker checker)
        {
            List<SimMove> moves = new List<SimMove>();
            int size = board.Size;
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    foreach ((int dr, int dc) d in Dirs)
                    {
                        int nr = r + d.dr;
                        int nc = c + d.dc;
                        if (board.IsValid(nr, nc) == false)
                            continue;

                        //# 실게임 규칙: 양쪽 칸 모두 드래그 가능해야 스왑. 고정블록(Wall 등) 스왑 금지.
                        //# (SimBoard.Swap 은 State 만 교환하므로, 고정블록을 스왑하면 Hp 가 칸에 남아 유령 동결블록 발생.)
                        if (board.Grid[r, c].CanNotDrag() || board.Grid[nr, nc].CanNotDrag())
                            continue;

                        board.Swap(r, c, nr, nc);
                        checker.CheckMap(board);
                        bool matched = checker.IsMatch;
                        board.Swap(r, c, nr, nc);
                        board.ResetAllMatch();
                        if (matched)
                        {
                            moves.Add(new SimMove(r, c, d.dr, d.dc));
                        }
                    }
                }
            }
            return moves;
        }

        public static int CountMatches(SimBoard board, SimMatchChecker checker, SimMove m)
        {
            board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
            checker.CheckMap(board);
            int n = 0;
            foreach (SimBlock b in board.Grid)
            {
                if (b.Match)
                {
                    ++n;
                }
            }
            board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
            board.ResetAllMatch();
            return n;
        }
    }

    //# 전략 A: 유효 수 중 랜덤. 시드 결정적(System.Random).
    public class RandomAiPolicy : ISimAiPolicy
    {
        public string Name => "Random";
        private readonly Random _rng;

        public RandomAiPolicy(int seed)
        {
            _rng = new Random(seed);
        }

        public SimMove? ChooseMove(SimBoard board, SimMatchChecker checker)
        {
            List<SimMove> moves = SimMoveFinder.FindValidMoves(board, checker);
            if (moves.Count == 0)
                return null;

            return moves[_rng.Next(moves.Count)];
        }
    }

    //# 전략 B: 매치 블록 수 최대. 동점은 시드 랜덤.
    public class GreedyAiPolicy : ISimAiPolicy
    {
        public string Name => "Greedy";
        private readonly Random _rng;

        public GreedyAiPolicy(int seed)
        {
            _rng = new Random(seed);
        }

        public SimMove? ChooseMove(SimBoard board, SimMatchChecker checker)
        {
            List<SimMove> moves = SimMoveFinder.FindValidMoves(board, checker);
            if (moves.Count == 0)
                return null;

            int best = -1;
            List<SimMove> bestMoves = new List<SimMove>();
            foreach (SimMove m in moves)
            {
                int cnt = SimMoveFinder.CountMatches(board, checker, m);
                if (cnt > best)
                {
                    best = cnt;
                    bestMoves.Clear();
                    bestMoves.Add(m);
                }
                else if (cnt == best)
                {
                    bestMoves.Add(m);
                }
            }
            return bestMoves[_rng.Next(bestMoves.Count)];
        }
    }
}
