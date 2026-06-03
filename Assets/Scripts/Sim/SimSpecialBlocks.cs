using System;
using static Defines;

namespace CatPang.Sim
{
    //# M2 정적 특수블록 로직: CatBox 수집, Creator 변환, 상태전이 적용.
    //# 실게임: Block.CatInTheBox / GPBombResolver.BlockCreatorBlock / GPGameScene.UpdateMap.
    public static class SimSpecialBlocks
    {
        //# 실게임 Block.GetBaseCat: 스킨 고양이를 원본 Cat1~5 로 환산. M2 보드엔 스킨 없으므로 Cat1~7 그대로.
        private static EBlockState BaseCat(EBlockState s)
        {
            if (s >= EBlockState.Cat1 && s <= EBlockState.Cat7)
                return s;
            return EBlockState.None;
        }

        //# 실게임 Block.CheckInBoxBlock: 박스가 받는 고양이인지.
        private static bool BoxAccepts(EBlockState boxState, EBlockState upState)
        {
            EBlockState cat = BaseCat(upState);
            switch (boxState)
            {
                case EBlockState.CatBox1: return cat == EBlockState.Cat1 || cat == EBlockState.Cat6;
                case EBlockState.CatBox2: return cat == EBlockState.Cat2 || cat == EBlockState.Cat7;
                case EBlockState.CatBox3: return cat == EBlockState.Cat3;
                case EBlockState.CatBox4: return cat == EBlockState.Cat4;
                case EBlockState.CatBox5: return cat == EBlockState.Cat5;
                default: return false;
            }
        }

        //# 실게임 Block.CatInTheBox 662-682: 받는 고양이면 박스 hp 감소(hp>0일 때), 항상 true 반환.
        public static bool CatInTheBox(SimBlock box, EBlockState upState)
        {
            if (box.IsCatBox() == false)
                return false;
            if (BoxAccepts(box.State, upState) == false)
                return false;
            if (box.Hp <= 0)
                return true;
            box.Hp -= 1;
            return true;
        }

        //# 보드 전체 CatBox 스캔: 박스 위 칸이 받는 고양이면 위 블록 제거(Match=true). 수집 발생 여부 반환.
        public static bool CollectCatBoxes(SimBoard board)
        {
            bool any = false;
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    SimBlock box = board.Grid[r, c];
                    if (box.IsCatBox() == false)
                        continue;
                    if (board.IsValid(r - 1, c) == false)
                        continue;
                    SimBlock up = board.Grid[r - 1, c];
                    if (CatInTheBox(box, up.State))
                    {
                        if (up.IsNormal())
                        {
                            up.Match = true;
                            any = true;
                        }
                    }
                }
            }
            return any;
        }

        //# 실게임 GPBombResolver.BlockCreatorBlock 462-492: creator 가 랜덤 방향 인접 일반블록 1개를 변환 예약.
        public static void RunCreators(SimBoard board, Random rng)
        {
            int size = board.Size;
            (int dr, int dc)[] dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    SimBlock creator = board.Grid[r, c];
                    if (creator.IsCreator() == false)
                        continue;
                    if (creator.Hp == 0)
                        continue;

                    EBlockState target = creator.State == EBlockState.WallCreator
                        ? EBlockState.Wall
                        : EBlockState.Potal;

                    //# 실게임은 랜덤 시작 방향에서 인접 일반블록을 찾을 때까지 회전. 여기선 랜덤 시작 + 4방향 순회.
                    int start = rng.Next(0, 4);
                    for (int k = 0; k < 4; ++k)
                    {
                        (int dr, int dc) d = dirs[(start + k) % 4];
                        int nr = r + d.dr, nc = c + d.dc;
                        if (board.IsValid(nr, nc) == false)
                            continue;
                        SimBlock nb = board.Grid[nr, nc];
                        if (nb.IsNormal())
                        {
                            nb.ChangeBlockState = target;
                            nb.ChangeHp = 1;
                            creator.Damage(0, changeNormalBlock: false); //# creator 자신 데미지(전환 안 함)
                            break;
                        }
                    }
                }
            }
        }

        //# 실게임 GPGameScene.UpdateMap 의 changeBlockState 적용: 예약된 상태전이를 보드에 반영.
        public static void ApplyChanges(SimBoard board)
        {
            foreach (SimBlock b in board.Grid)
            {
                if (b.ChangeBlockState != EBlockState.None)
                {
                    b.State = b.ChangeBlockState;
                    b.Hp = b.ChangeHp;
                    b.ChangeBlockState = EBlockState.None;
                    b.ChangeHp = -1;
                    b.Match = false;
                }
            }
        }
    }
}
