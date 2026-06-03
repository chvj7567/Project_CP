using System;
using static Defines;

namespace CatPang.Sim
{
    //# 순수 데이터 블록. 실게임 Block 의 매치/낙하/HP 관련 필드만 미러. Unity 비종속.
    public class SimBlock
    {
        //# M3a: 이 블록이 발동될 때 SimBombResolver 가 분기할 폭탄 종류.
        //# CLAUDE.md "폭탄 발동 효과" 표 + 실 Block.Bomb() switch 기준.
        public enum EBombKind
        {
            None,
            Bomb1,   //# CatPang — 3×3
            Bomb2,   //# Arrow5  — 십자(+)
            Bomb4,   //# Arrow1  — 가로 한 줄
            Bomb5,   //# Arrow3  — 세로 한 줄
            Bomb6,   //# Arrow6  — 대각 X
            Bomb7,   //# Arrow2  — 대각 /
            Bomb8,   //# Arrow4  — 대각 \
            Bomb9,   //# YellowBomb — 마름모
            Bomb10,  //# OrangeBomb — 5×5 테두리
            Bomb11,  //# BlueBomb   — 5×5 모서리
            Bomb12,  //# GreenBomb  — 5×5 변형
            Rainbow, //# RainbowPang
        }
        public EBlockState State;
        public int Row;
        public int Col;
        public int Index;   //# M3a: 실 Block.index 미러(squareMatch 위치 판정). SimBoard 생성자가 r*size+c 로 세팅.
        public bool Match;
        public bool SquareMatch;

        //# M3a: 매치 런렝스. 실 Block.hScore/vScore 미러. SimBombResolver.CreateBombBlock 이 읽는다.
        public int HScore;
        public int VScore;

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

        //# 드래그(스왑) 불가 블록 — 실게임 Block.CanNotDragBlock 미러.
        //# Wall/Potal/Fish/CatBox/Creator/RainbowPang. M3a: Fish 미포함(보드에 없음), RainbowPang 추가.
        public bool CanNotDrag()
        {
            return IsWall()
                || State == EBlockState.Potal
                || IsCatBox()
                || IsCreator()
                || State == EBlockState.RainbowPang;
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

        //# 실 Block.SetScore(score, direction): 가로/세로 매치 길이 기록.
        public void SetScore(int count, bool horizontal)
        {
            if (horizontal)
            {
                HScore = count;
            }
            else
            {
                VScore = count;
            }
        }

        //# 실 Block.ResetScore(): hScore/vScore 만 리셋. squareMatch 는 ResetMatch 가 담당(실게임과 동일).
        public void ResetScore()
        {
            HScore = 0;
            VScore = 0;
        }

        public void ResetMatch()
        {
            Match = false;
            SquareMatch = false;
            HScore = 0;
            VScore = 0;
        }

        //# M3a: 화살표 폭탄 여부. Arrow1~6(int 10~15) 연속 범위.
        public bool IsArrow()
        {
            return State >= EBlockState.Arrow1 && State <= EBlockState.Arrow6;
        }

        //# M3a: 고양이 방울 여부.
        public bool IsCatPang()
        {
            return State == EBlockState.CatPang;
        }

        //# M3a: 5색 특수폭탄 여부. 실 Block.IsSpecialBombBlock() 전체 집합 미러.
        public bool IsSpecialBomb()
        {
            return State == EBlockState.PinkBomb
                || State == EBlockState.GreenBomb
                || State == EBlockState.OrangeBomb
                || State == EBlockState.BlueBomb
                || State == EBlockState.YellowBomb;
        }

        //# M3a: 폭탄 계열 여부. 실 Block.IsBombBlock() 전체 집합 미러.
        //# CatPang + Arrow1~6 + 5색폭탄(IsSpecialBomb) + RainbowPang.
        public bool IsBomb()
        {
            return IsCatPang()
                || IsArrow()
                || IsSpecialBomb()
                || State == EBlockState.RainbowPang;
        }

        //# M3a: 이 블록의 발동 폭탄 종류. 실 Block.Bomb() switch 와 1:1 대응.
        //# PinkBomb 은 단독 발동 없음(실 L461-463) → None.
        public EBombKind BombKind()
        {
            switch (State)
            {
                case EBlockState.CatPang:    return EBombKind.Bomb1;
                case EBlockState.Arrow1:     return EBombKind.Bomb4;
                case EBlockState.Arrow2:     return EBombKind.Bomb7;
                case EBlockState.Arrow3:     return EBombKind.Bomb5;
                case EBlockState.Arrow4:     return EBombKind.Bomb8;
                case EBlockState.Arrow5:     return EBombKind.Bomb2;
                case EBlockState.Arrow6:     return EBombKind.Bomb6;
                case EBlockState.YellowBomb: return EBombKind.Bomb9;
                case EBlockState.OrangeBomb: return EBombKind.Bomb10;
                case EBlockState.BlueBomb:   return EBombKind.Bomb11;
                case EBlockState.GreenBomb:  return EBombKind.Bomb12;
                case EBlockState.RainbowPang: return EBombKind.Rainbow;
                default:                     return EBombKind.None;
            }
        }
    }
}
