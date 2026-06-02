using System.Linq;
using UnityEngine;
using static Defines;

namespace CatPang.Sim
{
    //# Stage.json + StageBlock.json(둘 다 루트가 배열) 을 합쳐 SimStageData 로.
    //# JsonUtility 는 최상위 배열을 직접 파싱 못 하므로 "{\"items\":<json>}" 로 감싸 파싱한다.
    public static class SimStageLoader
    {
        [System.Serializable] private class StageWrap { public StageDto[] items; }
        [System.Serializable] private class BlockWrap { public StageBlockDto[] items; }

        public static SimStageData Load(string stageJson, string stageBlockJson, int stage, bool normalMode)
        {
            StageDto[] stages = JsonUtility.FromJson<StageWrap>("{\"items\":" + stageJson + "}").items;
            StageBlockDto[] blocks = JsonUtility.FromJson<BlockWrap>("{\"items\":" + stageBlockJson + "}").items;

            StageDto meta = stages.First(d => d.stage == stage);
            int size = meta.boardSize;

            float time = meta.time;
            int moveCount = meta.moveCount;
            int targetScore = meta.targetScore;
            if (normalMode)
            {
                //# Infomation.StageInfo.ApplyNormalModifiers 와 동일:
                //# time = -1; if (targetScore > 0) targetScore /= 2; else if (moveCount > 0) moveCount *= 2;
                time = -1;
                if (targetScore > 0)
                {
                    targetScore /= 2;
                }
                else if (moveCount > 0)
                {
                    moveCount *= 2;
                }
            }

            EBlockState[] states = new EBlockState[size * size];
            for (int i = 0; i < states.Length; ++i)
            {
                states[i] = EBlockState.None;
            }
            foreach (StageBlockDto b in blocks.Where(b => b.stage == stage))
            {
                if (b.row >= 0 && b.row < size && b.col >= 0 && b.col < size)
                {
                    states[b.row * size + b.col] = (EBlockState)b.blockState;
                }
            }

            return new SimStageData
            {
                Stage = meta.stage,
                Group = meta.group,
                BoardSize = size,
                Time = time,
                MoveCount = moveCount,
                TargetScore = targetScore,
                BlockTypeCount = meta.blockTypeCount,
                InitialStates = states,
            };
        }
    }
}
