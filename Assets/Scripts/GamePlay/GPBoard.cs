using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Defines;

public class GPBoard
{
    public Block[,] boardArr { get; private set; }
    public int boardSize { get; private set; }

    Dictionary<EBlockState, Sprite> _blockSpriteList;
    public float delay;
    public int delayMillisecond;
    CancellationToken _token;

    public void Init(Block[,] arr, int size, Dictionary<EBlockState, Sprite> sprites, float delay, int delayMs, CancellationToken token)
    {
        boardArr = arr;
        boardSize = size;
        _blockSpriteList = sprites;
        this.delay = delay;
        delayMillisecond = delayMs;
        _token = token;
    }

    public bool IsValidIndex(int row, int col)
    {
        return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
    }

    public bool IsNormalBlock(int row, int col)
    {
        if (!IsValidIndex(row, col) || boardArr[row, col] == null ||
            boardArr[row, col].IsFixdBlock() || boardArr[row, col].IsBombBlock() ||
            boardArr[row, col].IsSpecialBombBlock() || boardArr[row, col].IsFishBlock() ||
            boardArr[row, col].IsBallBlock())
            return false;
        return true;
    }

    public void ChangeBlock(Block moveBlock, Block targetBlock)
    {
        var tempPos = moveBlock.originPos;
        moveBlock.originPos = targetBlock.originPos;
        targetBlock.originPos = tempPos;

        var tempIndex = moveBlock.index;
        moveBlock.index = targetBlock.index;
        targetBlock.index = tempIndex;

        var tempRow = moveBlock.row;
        moveBlock.row = targetBlock.row;
        targetBlock.row = tempRow;

        var tempCol = moveBlock.col;
        moveBlock.col = targetBlock.col;
        targetBlock.col = tempCol;

        moveBlock.name = $"Block{moveBlock.row}/{moveBlock.col}";
        targetBlock.name = $"Block{targetBlock.row}/{targetBlock.col}";

        boardArr[moveBlock.row, moveBlock.col] = moveBlock;
        boardArr[targetBlock.row, targetBlock.col] = targetBlock;
    }

    public void CreateNewBlock(Block block, ELog log, int key, EBlockState blockState, bool isDelay = true)
    {
        blockState = block.CheckSelectCatShop(blockState);
        block.SetBlockState(log, key, _blockSpriteList[blockState], blockState);
        block.match = false;
        block.boom = false;
        block.squareMatch = false;
        block.remove = false;
        if (isDelay)
            block.rectTransform.DOScale(1f, delay);
        else
            block.rectTransform.localScale = Vector3.one;
    }

    public async Task DownBlock()
    {
        var order = new List<Block>();
        for (int i = boardSize - 1; i >= 0; --i)
            for (int j = boardSize - 1; j >= 0; --j)
                if (boardArr[i, j] != null)
                    order.Add(boardArr[i, j]);

        foreach (var block in order)
        {
            int row = block.row;
            int col = block.col;

            if (boardArr[row, col].IsMatch() || boardArr[row, col].IsFixdBlock())
                continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;

            int wallRow = -1;
            for (int i = boardSize - 1; i > row; --i)
                if (boardArr[i, col].IsWallBlock())
                    wallRow = i;

            int searchFrom = wallRow == -1 ? boardSize - 1 : wallRow;
            for (int i = searchFrom; i > row; --i)
            {
                if (boardArr[i, col].IsMatch())
                {
                    targetBlock = boardArr[i, col];
                    break;
                }
            }

            if (targetBlock != null)
                ChangeBlock(moveBlock, targetBlock);
        }

        bool downDelay = false;
        foreach (var block in boardArr)
        {
            if (block == null) continue;
            if (!block.IsFixdBlock())
            {
                downDelay = true;
                block.rectTransform.DOAnchorPos(block.originPos, delay);
            }
        }

        if (downDelay)
        {
            await Task.Delay((int)(delay * delayMillisecond), _token);
            await GPGameScene.WaitWhilePaused(_token);
        }
    }
}
