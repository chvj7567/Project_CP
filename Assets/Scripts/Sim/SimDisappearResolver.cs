using static Defines;

namespace CatPang.Sim
{
    //# 실 GPGameScene.SetDissapearBlock(L691~725) 포팅: 맨아래줄 Fish/Ball 변환 예약.
    //# Fish → 색폭탄(PinkBomb..BlueBomb, 시드 RNG). Ball → Potal(hp5) + 좌우 일반블록 Potal 확산(hp4↓).
    //# 예약만(ChangeBlockState/ChangeHp), 적용은 SimSpecialBlocks.ApplyChanges.
    public static class SimDisappearResolver
    {
        private const int PortalBlockHp = 5;     //# 실 GPGameScene.PortalBlockHp(L37)
        private const int BallPortalStartHp = 4; //# 실 GPGameScene.BallPortalStartHp(L39)

        public static void Resolve(SimBoard board, System.Random rng)
        {
            int row = board.Size - 1;
            for (int i = 0; i < board.Size; ++i)
            {
                SimBlock block = board.Grid[row, i];
                if (block.IsFish())
                {
                    //# 실 L700: Random(PinkBomb, BlueBomb+1). PinkBomb=19, BlueBomb=23 — 양 끝 포함 5종.
                    block.ChangeBlockState = (EBlockState)rng.Next((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
                }
                else if (block.IsBall())
                {
                    block.ChangeBlockState = EBlockState.Potal;
                    block.ChangeHp = PortalBlockHp;

                    //# 우측 확산(실 L708~715) — ballHp<=0 가드가 인접칸 검사 전(L712)에 있음.
                    int ballHp = BallPortalStartHp;
                    for (int k = i + 1; k < board.Size; ++k)
                    {
                        SimBlock cb = board.Grid[row, k];
                        if (ballHp <= 0)
                            break;
                        if (cb.IsNormal() || cb.Match)
                        {
                            cb.ChangeBlockState = EBlockState.Potal;
                            cb.ChangeHp = ballHp;
                            ballHp -= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //# 좌측 확산(실 L716~722) — 실게임에 ballHp<=0 가드 없음(비대칭). hp 0/음수 Potal 가능. 충실 포팅.
                    ballHp = BallPortalStartHp;
                    for (int k = i - 1; k >= 0; --k)
                    {
                        SimBlock cb = board.Grid[row, k];
                        if (cb.IsNormal() || cb.Match)
                        {
                            cb.ChangeBlockState = EBlockState.Potal;
                            cb.ChangeHp = ballHp;
                            ballHp -= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
