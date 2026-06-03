using System.Reflection;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# 실게임 Block 을 렌더/UI 없이 EditMode 에서 생성해 GPMatchChecker 골든 캡처에 쓴다.
    //# Block.SetBlockState 는 img/hpText 등 UI 를 건드려 NRE → private blockState 를 리플렉션으로 직접 세팅.
    public static class RealBlockFactory
    {
        private static readonly FieldInfo BlockStateField =
            typeof(Block).GetField("blockState", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo HpField =
            typeof(Block).GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Block[,] CreateBoard(EBlockState[,] states)
        {
            int size = states.GetLength(0);
            Block[,] arr = new Block[size, size];
            for (int r = 0; r < size; ++r)
                for (int c = 0; c < size; ++c)
                {
                    GameObject go = new GameObject($"B{r}_{c}");
                    Block b = go.AddComponent<Block>();
                    b.row = r; b.col = c; b.index = r * size + c;
                    BlockStateField.SetValue(b, states[r, c]);
                    arr[r, c] = b;
                }
            return arr;
        }

        //# state + hp 를 함께 세팅. hps[r,c] = -1 이면 hp 미설정(일반블록).
        public static Block[,] CreateBoard(EBlockState[,] states, int[,] hps)
        {
            int size = states.GetLength(0);
            Block[,] arr = new Block[size, size];
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    GameObject go = new GameObject($"B{r}_{c}");
                    Block b = go.AddComponent<Block>();
                    b.row = r; b.col = c; b.index = r * size + c;
                    BlockStateField.SetValue(b, states[r, c]);
                    HpField.SetValue(b, hps[r, c]);
                    arr[r, c] = b;
                }
            }
            return arr;
        }

        public static void Destroy(Block[,] arr)
        {
            foreach (Block b in arr)
                if (b != null) Object.DestroyImmediate(b.gameObject);
        }
    }
}
