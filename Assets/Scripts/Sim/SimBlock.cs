using System;
using static Defines;

namespace CatPang.Sim
{
    //# 순수 데이터 블록. 실게임 Block 의 매치/낙하/HP 관련 필드만 미러. Unity 비종속.
    public class SimBlock
    {
        public EBlockState State;
        public int Row;
        public int Col;
        public bool Match;
        public bool SquareMatch;

        //# M2: HP/상태전이. Hp<=0 또는 -1 은 HP 없는 일반블록.
        public int Hp = -1;
        public EBlockState ChangeBlockState = EBlockState.None; //# 다음 상태전이 예약
        public int ChangeHp = -1;
        private bool _checkDamage; //# 턴당 1회 데미지 가드

        //# 일반 고양이 블록 여부. M1: Cat1~7.
        public bool IsNormal()
        {
            return State >= EBlockState.Cat1 && State <= EBlockState.Cat7;
        }

        public bool IsWall()
        {
            return State == EBlockState.Wall;
        }

        public bool IsCatBox()
        {
            return State >= EBlockState.CatBox1 && State <= EBlockState.CatBox5;
        }

        public bool IsCreator()
        {
            return State == EBlockState.WallCreator || State == EBlockState.PotalCreator;
        }

        //# 낙하를 막는 블록(실게임 Block.IsWallBlock): Wall/CatBox/Creator. Potal 은 안 막음.
        public bool IsWallLike()
        {
            return IsWall() || IsCatBox() || IsCreator();
        }

        //# 드래그(스왑) 불가 블록 — 실게임 Block.CanNotDragBlock 의 M2 부분집합.
        //# Wall/Potal/CatBox/Creator. (M3 의 Fish/RainbowPang 은 M2 보드에 없음.)
        //# 실게임은 스왑 시 양쪽 칸 모두 !CanNotDrag 일 때만 허용 — 고정블록은 절대 이동하지 않는다.
        public bool CanNotDrag()
        {
            return IsWall() || State == EBlockState.Potal || IsCatBox() || IsCreator();
        }

        //# 매치로 제거 가능한 블록인가(인접 데미지 대상 아님, 직접 매치 대상). 일반블록만.
        //# 목표블록 판정(실게임 CheckHpBlock): Creator 만 false.
        public bool CheckHp()
        {
            return IsCreator() == false;
        }

        //# 실게임 Block.Damage 포팅: 턴당 1회 + hp>=1 + 박스 아님. hp 0 이면 일반블록 전환 예약.
        public void Damage(int blockTypeCount, bool changeNormalBlock = true)
        {
            if (_checkDamage == false && Hp >= 1 && IsCatBox() == false)
            {
                _checkDamage = true;
                Hp -= 1;
                if (Hp <= 0 && changeNormalBlock)
                {
                    //# 시드 RNG 는 호출측에서 못 넘기므로 결정성은 호출 맥락(SimMatchChecker)이 책임.
                    //# 여기선 System.Random 정적 시드 없이 — 대신 SimMatchChecker 가 rng 주입 버전을 쓴다.
                    ChangeBlockState = (EBlockState)_damageRng.Next(0, blockTypeCount);
                }
            }
        }

        //# Damage 의 일반블록 전환 RNG. SetDamageRng 로 시드 주입(결정성).
        private static System.Random _damageRng = new System.Random(0);
        public static void SetDamageRng(System.Random rng) { _damageRng = rng; }

        public void ResetCheckDamage()
        {
            _checkDamage = false;
        }

        public void ResetMatch()
        {
            Match = false;
            SquareMatch = false;
        }
    }
}
