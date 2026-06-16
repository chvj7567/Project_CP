#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
            {
                for (int c = 0; c < size; ++c)
                {
                    GameObject go = new GameObject($"B{r}_{c}");
                    Block b = go.AddComponent<Block>();
                    b.row = r; b.col = c; b.index = r * size + c;
                    //# rectTransform 은 인스펙터 직렬화 필드 — EditMode 에서 null 이므로 직접 연결.
                    //# RequireComponent(RectTransform) 로 go.GetComponent 는 항상 non-null.
                    b.rectTransform = go.GetComponent<RectTransform>();
                    BlockStateField.SetValue(b, states[r, c]);
                    arr[r, c] = b;
                }
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
                    //# rectTransform 은 인스펙터 직렬화 필드 — EditMode 에서 null 이므로 직접 연결.
                    b.rectTransform = go.GetComponent<RectTransform>();
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
            {
                if (b != null)
                {
                    Object.DestroyImmediate(b.gameObject);
                }
            }
        }

        //# 실 GPBoard + GPMatchChecker 를 EditMode 에서 구성한다(렌더/UI 없이).
        //# GPBoard.Init 은 blockTypeCount 인자가 없다 — delayMs=0, blockTypeCount 는 matcher.Init 에만 전달.
        public static GPBoard CreateGPBoard(Block[,] arr, int size)
        {
            GPBoard board = new GPBoard();
            board.Init(arr, size, new Dictionary<EBlockState, Sprite>(), 0f, 0, default(CancellationToken));
            return board;
        }

        //# GPMatchChecker 를 EditMode 에서 구성한다.
        public static GPMatchChecker CreateGPMatchChecker(GPBoard board, int blockTypeCount)
        {
            GPMatchChecker matcher = new GPMatchChecker();
            matcher.Init(board, blockTypeCount);
            return matcher;
        }

        //# 골든에서 실 GPBombResolver.BombN 호출 전 1회 — SaveBombCollectionData 의
        //# CHMData.Instance.collectionLocalDataDic NRE 회피(빈 딕셔너리 주입). collectionLocalDataDic 은 public 필드.
        //# CHMData 는 CHSingletonStatic 이라 Instance 접근 시 인스턴스 생성된다.
        public static void EnsureCHMDataCollection()
        {
            if (CHMData.Instance.collectionLocalDataDic == null)
            {
                CHMData.Instance.collectionLocalDataDic = new Dictionary<string, Data.Collection>();
            }
        }
    }
}
#endif
