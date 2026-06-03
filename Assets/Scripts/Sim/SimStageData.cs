using System;
using static Defines;

namespace CatPang.Sim
{
    //# 시뮬용 스테이지 1건. 보드는 BoardSize^2 의 EBlockState 배열(row-major).
    public class SimStageData
    {
        public int Stage;
        public int Group;
        public int BoardSize;
        public float Time;          //# >0 시간제한, <=0 무제한
        public int MoveCount;       //# >0 이동제한, <=0 무제한
        public int TargetScore;     //# >0 점수목표
        public int BlockTypeCount;
        public EBlockState[] InitialStates; //# 길이 BoardSize^2. None=레코드 없음(런타임 랜덤 채움)
        public int[] InitialHps; //# 길이 BoardSize^2. 각 칸 hp(StageBlock hp). 레코드 없으면 -1.

        public bool IsTimeMode => Time > 0;
        public bool IsMoveMode => MoveCount > 0;
    }

    //# Stage.json 실제 스키마: 루트가 StageInfo 배열. JsonUtility 는 루트 배열을 못 읽으므로 래핑 파싱(로더에서 처리).
    [Serializable] public class StageDto
    {
        public int group; public int stage; public int tutorialID;
        public int blockTypeCount; public int boardSize;
        public float time; public int targetScore; public int moveCount;
    }

    //# StageBlock.json 실제 스키마: 칸별 평면 레코드 배열.
    [Serializable] public class StageBlockDto
    {
        public int stage; public int blockState; public int hp;
        public int row; public int col; public bool tutorialBlock;
    }
}
