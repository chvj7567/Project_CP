using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

public class Game : MonoBehaviour
{
    [SerializeField] GameObject origin;
    [SerializeField] float margin = 0f;
    [SerializeField, Range(1, 9)] int horizontalCount = 1;
    [SerializeField, Range(1, 9)] int verticalCount = 1;
    [SerializeField] Transform parent;

    [SerializeField] CHInstantiateButton instBtn;

    [ReadOnly]
    Block[,] boardArr = new Block[9, 9];

    [ReadOnly]
    public bool isDrag = false;

    [ReadOnly]
    public bool isAni = false;

    [ReadOnly]
    bool isMatch = false;

    void Start()
    {
        CHMMain.UI.CreateEventSystemObject();
        instBtn.InstantiateButton(origin, margin, horizontalCount, verticalCount, parent, boardArr);

        UpdateMap(true);

        AfterDrag(null, null);
    }

    public async void AfterDrag(Block block1, Block block2, float moveTime = .5f)
    {
        isAni = true;

        bool first = false;
        await Task.Delay((int)(moveTime * 1000));

        do
        {
            isMatch = false;
            CheckMap();
            if (block1 != null && block2 != null)
            {
                if (first == false && isMatch == false)
                {
                    block1.rectTransform.DOAnchorPos(block2.originPos, .5f);
                    block2.rectTransform.DOAnchorPos(block1.originPos, .5f);
                    ChangeBlock(block1, block2);
                }
            }
            first = true;
            await RemoveMatchBlock();
            DownBlock();
            await Task.Delay(1000);
            UpdateMap();
            await Task.Delay(1000);
            CheckMap();

        } while (isMatch == true);

        isAni = false;
    }

    void UpdateMap(bool first = false)
    // 맵생성
    {
        foreach (var block in boardArr)
        {
            if (first == true || block.state == Defines.EState.Match)
            {
                var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);
                CHMMain.Resource.LoadSprite((Defines.ENormalBlockType)random, (sprite) =>
                {
                    block.SetNormalType((Defines.ENormalBlockType)random);
                    block.state = Defines.EState.Normal;
                    block.img.sprite = sprite;
                });

                if (first == false)
                {
                    block.ResetScore();
                    block.SetOriginPos();
                }
            }
        }
    }

    void CheckMap()
    // 3Match 블럭 제거
    {
        for (int i = 0; i < horizontalCount; ++i)
        {
            List<Block> hBlockList = new List<Block>();

            foreach (var block in boardArr)
            {
                if (block.row == i)
                {
                    hBlockList.Add(block);
                }
            }

            CheckMatch(hBlockList, Defines.EDirection.Horizontal);
        }

        for (int i = 0; i < verticalCount; ++i)
        {
            List<Block> vBlockList = new List<Block>();

            foreach (var block in boardArr)
            {
                if (block.col == i)
                {
                    vBlockList.Add(block);
                }
            }

            CheckMatch(vBlockList, Defines.EDirection.Vertical);
        }
    }

    async Task RemoveMatchBlock()
    {
        for (int i = 0; i < boardArr.GetLength(0); ++i)
        {
            for (int j = 0; j < boardArr.GetLength(1); ++j)
            {
                // 없어져야 할 블럭
                if (boardArr[i, j].state == Defines.EState.Match)
                {
                    int hScore = boardArr[i, j].horizontalScore;
                    int vScore = boardArr[i, j].verticalScore;
                    if (hScore >= 3 && vScore >= 3)
                    {
                        CHMMain.Resource.LoadSprite(Defines.ESpecailBlockType.Boom, (sprite) =>
                        {
                            boardArr[i, j].SetSpecailType(Defines.ESpecailBlockType.Boom);
                            boardArr[i, j].state = Defines.EState.Normal;
                            boardArr[i, j].img.sprite = sprite;
                            boardArr[i, j].ResetScore();
                            boardArr[i, j].SetOriginPos();
                        });
                    }
                    else
                    {
                        if (boardArr[i, j].GetSpecailType() == Defines.ESpecailBlockType.Boom)
                        {
                            Boom(boardArr[i, j]);
                            i = -1;
                            break;
                        }

                        boardArr[i, j].transform.DOScale(0f, 1f);
                    }
                }
            }
        }

        await Task.Delay(1000);
    }

    void CheckMatch(List<Block> blockList, Defines.EDirection direction)
    {
        Defines.ENormalBlockType matchType = Defines.ENormalBlockType.None;
        List<int> tempIndex = new List<int>();
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            if (matchType == Defines.ENormalBlockType.None)
            {
                matchType = blockList[i].GetNormalType();
                matchCount = 1;
                tempIndex.Add(blockList[i].index);
            }
            else if (matchType == blockList[i].GetNormalType())
            {
                ++matchCount;
                tempIndex.Add(blockList[i].index);

                if (matchCount >= 3)
                {
                    int temp = i;
                    for (int j = 0; j < matchCount; ++j)
                    {
                        var block = blockList[temp--];
                        block.SetScore(matchCount, direction);
                        block.state = Defines.EState.Match;
                        isMatch = true;
                    }
                }
            }
            else
            {
                matchType = blockList[i].GetNormalType();
                matchCount = 1;
                tempIndex.Clear();
                tempIndex.Add(blockList[i].index);
            }
        }
    }

    void DownBlock()
    {
        List<Block> order = new List<Block>();
        for (int i = 8; i >= 0; --i)
        {
            for (int j = 8; j >= 0; --j)
            {
                var temp = boardArr[i, j];
                if (temp != null)
                {
                    order.Add(temp);
                }
            }
        }

        foreach (var block in order)
        {
            int row = block.row;
            int col = block.col;
            if (boardArr[row, col].state != Defines.EState.Normal) continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;
            int targetRow = -1;
            for (int i = horizontalCount - 1; i > block.row; --i)
            {
                if (boardArr[i, col].state == Defines.EState.Match)
                {
                    targetBlock = boardArr[i, col];
                    targetRow = i;
                    break;
                }
            }

            if (targetBlock != null)
            {
                ChangeBlock(moveBlock, targetBlock);
            }
        }

        foreach (var block in boardArr)
        {
            if (block.state == Defines.EState.Normal)
            {
                block.MoveOriginPos();
            }
        }
    }

    public void ChangeBlock(Block moveBlock, Block targetBlock)
    {
        //Debug.Log($"Change {row}/{col} - {targetRow}/{col}");
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

    public void Boom(Block block)
    {
        Debug.Log($"{block.row} {block.col}");

        if (IsValidIndex(block.row - 1, block.col - 1, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row - 1, block.col - 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row - 1, block.col, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row - 1, block.col].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row - 1, block.col + 1, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row - 1, block.col + 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row, block.col - 1, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row, block.col - 1].state = Defines.EState.Match;
        }

        block.SetNormalType(Defines.ENormalBlockType.None);
        block.state = Defines.EState.Match;

        if (IsValidIndex(block.row, block.col + 1, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row, block.col + 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row + 1, block.col - 1, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row + 1, block.col - 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row + 1, block.col, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row + 1, block.col].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row + 1, block.col + 1, boardArr.GetLength(0), boardArr.GetLength(1)))
        {
            boardArr[block.row + 1, block.col + 1].state = Defines.EState.Match;
        }

        AfterDrag(null, null);
    }

    bool IsValidIndex(int row, int column, int rows, int columns)
    {
        return row >= 0 && row < rows && column >= 0 && column < columns;
    }
}
