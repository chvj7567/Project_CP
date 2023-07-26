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
    [SerializeField] Image viewImg;
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

    public float delay;

    async void Start()
    {
        CHMMain.UI.CreateEventSystemObject();
        instBtn.InstantiateButton(origin, margin, horizontalCount, verticalCount, parent, boardArr);

        await UpdateMap(true);

        AfterDrag(null, null);
    }

    private void Update()
    {
        if (isAni == true)
        {
            viewImg.color = Color.red;
        }
        else
        {
            viewImg.color = Color.green;
        }
    }

    public async void AfterDrag(Block block1, Block block2)
    {
        isAni = true;

        bool first = false;
        await Task.Delay((int)(delay * 1000));

        do
        {
            isMatch = false;
            CheckMap();
            if (block1 != null && block2 != null)
            {
                if (first == false && isMatch == false)
                {
                    block1.rectTransform.DOAnchorPos(block2.originPos, delay);
                    block2.rectTransform.DOAnchorPos(block1.originPos, delay);
                    ChangeBlock(block1, block2);
                }
            }
            first = true;
            await RemoveMatchBlock();
            await DownBlock();
            await UpdateMap();
            CheckMap();

        } while (isMatch == true);

        isAni = false;
    }

    async Task UpdateMap(bool first = false)
    // 맵생성
    {
        foreach (var block in boardArr)
        {
            if (first == true || block.state == Defines.EState.Match)
            {
                if (block.horizontalScore >= 4 || block.verticalScore >= 4)
                {
                    CHMMain.Resource.LoadSprite(Defines.ESpecailBlockType.Boom, (sprite) =>
                    {
                        block.SetSpecailType(Defines.ESpecailBlockType.Boom);
                        block.state = Defines.EState.Normal;
                        block.img.sprite = sprite;
                        block.ResetScore();
                        block.SetOriginPos();
                    });
                }
                else
                {
                    var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);
                    CHMMain.Resource.LoadSprite((Defines.ENormalBlockType)random, (sprite) =>
                    {
                        block.SetNormalType((Defines.ENormalBlockType)random);
                        block.state = Defines.EState.Normal;
                        block.img.sprite = sprite;
                    });
                }

                if (first == false)
                {
                    block.ResetScore();
                    block.SetOriginPos();
                }
            }
        }

        await Task.Delay((int)(delay * 1000));
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
        bool removeDelay = false;

        for (int i = 0; i < boardArr.GetLength(0); ++i)
        {
            for (int j = 0; j < boardArr.GetLength(1); ++j)
            {
                // 없어져야 할 블럭
                if (boardArr[i, j].state == Defines.EState.Match)
                {
                    var block = boardArr[i, j];

                    if (boardArr[i, j].GetSpecailType() == Defines.ESpecailBlockType.Boom)
                    {
                        Boom(boardArr[i, j], false);
                        i = -1;
                        break;
                    }

                    removeDelay = true;
                    boardArr[i, j].rectTransform.DOScale(0f, delay).OnComplete(() =>
                    {
                        if (block.horizontalScore >= 4 || block.verticalScore >= 4)
                        {
                            CHMMain.Resource.LoadSprite(Defines.ESpecailBlockType.Boom, (sprite) =>
                            {
                                block.SetSpecailType(Defines.ESpecailBlockType.Boom);
                                block.state = Defines.EState.Normal;
                                block.img.sprite = sprite;
                                block.ResetScore();
                                block.SetOriginPos();
                            });
                        }
                    });
                }
            }
        }

        if (removeDelay)
        {
            await Task.Delay((int)(delay * 2000));
        }
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

    async Task DownBlock()
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

        bool downDelay = false;

        foreach (var block in boardArr)
        {
            if (block.state == Defines.EState.Normal)
            {
                downDelay = true;
                block.rectTransform.DOAnchorPos(block.originPos, delay);
            }
        }

        if (downDelay)
        {
            await Task.Delay((int)(delay * 1000));
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

    public void Boom(Block block, bool ani = true)
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

        if (ani)
        {
            AfterDrag(null, null);
        }
    }

    bool IsValidIndex(int row, int column, int rows, int columns)
    {
        return row >= 0 && row < rows && column >= 0 && column < columns;
    }
}
