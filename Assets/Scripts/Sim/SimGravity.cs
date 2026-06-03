using System;
using static Defines;

namespace CatPang.Sim
{
    //# 낙하 + 리필. M1: Wall 없음 → 각 열에서 비매치 블록을 바닥으로 모으고 빈칸을 위에 랜덤 리필.
    //# RNG 는 시드 주입(System.Random)으로 결정적 — UnityEngine.Random 미사용.
    //# 리필 범위 0..blockTypeCount-1 = Cat1(0)..  (GPGameScene.UpdateMap 의 Random(0, blockTypeCount) 와 동일).
    public class SimGravity
    {
        private readonly Random _rng;

        public SimGravity(int seed)
        {
            _rng = new Random(seed);
        }

        public void Apply(SimBoard board, int blockTypeCount)
        {
            int size = board.Size;
            for (int c = 0; c < size; ++c)
            {
                //# Wall 류(낙하 차단)를 경계로 열을 세그먼트로 나눠 각 세그먼트 안에서만 압축+리필.
                int segmentBottom = size - 1;
                for (int r = size - 1; r >= 0; --r)
                {
                    if (board.Grid[r, c].IsWallLike())
                    {
                        //# Wall 위 세그먼트 [r+1 .. segmentBottom] 압축+리필
                        CompactSegment(board, c, r + 1, segmentBottom, blockTypeCount);
                        //# Wall 위쪽이 다음 세그먼트 바닥
                        segmentBottom = r - 1;
                    }
                }
                CompactSegment(board, c, 0, segmentBottom, blockTypeCount);
            }

            board.ResetAllMatch();
        }

        //# 한 세그먼트 [top..bottom] 에서 비매치 블록을 bottom 으로 압축, 남은 위 칸 랜덤 리필.
        //# State 이동 시 Hp 도 함께 이동(블록이 떨어지면 HP 동행). 리필 칸은 Hp=-1.
        private void CompactSegment(SimBoard board, int c, int top, int bottom, int blockTypeCount)
        {
            if (bottom < top)
                return;

            int writeRow = bottom;
            for (int r = bottom; r >= top; --r)
            {
                if (board.Grid[r, c].Match == false)
                {
                    //# source 칸의 전환예약(ChangeBlockState/ChangeHp)까지 target 으로 함께 이동.
                    //# State/Hp 만 옮기면 전환대기 블록이 이동 시 예약을 잃어 hp<=0 Wall 동결 등 유령블록 발생.
                    //# writeRow==r 이면 자기복사라 무해.
                    SimBlock src = board.Grid[r, c];
                    SimBlock dst = board.Grid[writeRow, c];
                    dst.State = src.State;
                    dst.Hp = src.Hp;
                    dst.ChangeBlockState = src.ChangeBlockState;
                    dst.ChangeHp = src.ChangeHp;
                    --writeRow;
                }
            }

            for (int r = writeRow; r >= top; --r)
            {
                //# 리필 칸은 깨끗한 일반블록 — 이전 점유 블록의 전환예약 잔존을 반드시 비운다.
                //# (안 비우면 다음 ApplyChanges 가 신규 일반블록을 잔존 예약대로 Wall 등으로 둔갑시킴.)
                SimBlock cell = board.Grid[r, c];
                cell.State = (EBlockState)_rng.Next(0, blockTypeCount);
                cell.Hp = -1;
                cell.ChangeBlockState = EBlockState.None;
                cell.ChangeHp = -1;
            }
        }
    }
}
