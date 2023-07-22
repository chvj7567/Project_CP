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

    async void Start()
    {
        CHMMain.UI.CreateEventSystemObject();
        instBtn.InstantiateButton(origin, margin, horizontalCount, verticalCount, parent, boardArr);

        MakeMap();

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
            RemoveMatchBlock();
            await Task.Delay(500);
            DownBlock();
            await Task.Delay(500);
            UpdateMap();
            await Task.Delay(500);

        } while (isMatch == true);

        isAni = false;
    }

    void UpdateMap()
    // 매치되서 없어진 블럭들 재생성
    {
        foreach (var block in boardArr)
        {
            var img = block.GetComponent<Image>();

            if (block.state == Defines.EState.Match)
            {
                var random = Random.Range(0, (int)Defines.ESpriteType.Max);
                CHMMain.Resource.LoadSprite((Defines.ESpriteType)random, (sprite) =>
                {
                    block.type = (Defines.ESpriteType)random;
                    block.state = Defines.EState.Normal;
                    img.sprite = sprite;
                });

                block.ResetScore();
                block.SetOriginPos();
            }
        }
    }

    void MakeMap()
    // 처음 맵 만들 때
    {
        foreach (var block in boardArr)
        {
            var img = block.GetComponent<Image>();

            var random = Random.Range(0, (int)Defines.ESpriteType.Max);
            CHMMain.Resource.LoadSprite((Defines.ESpriteType)random, (sprite) =>
            {
                var spriteType = (Defines.ESpriteType)random;
                block.type = spriteType;
                img.sprite = sprite;
            });
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

    void RemoveMatchBlock()
    {
        foreach (var block in boardArr)
        {
            if (block.state == Defines.EState.Match)
            {
                var rectTransform = block.GetComponent<RectTransform>();
                block.transform.DOScale(0f, .5f);
            }
        }
    }

    void CheckMatch(List<Block> blockList, Defines.EDirection direction)
    {
        Defines.ESpriteType matchType = Defines.ESpriteType.None;
        List<int> tempIndex = new List<int>();
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            if (matchType == Defines.ESpriteType.None)
            {
                matchType = blockList[i].type;
                matchCount = 1;
                tempIndex.Add(blockList[i].index);
            }
            else if (matchType == blockList[i].type)
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
                matchType = blockList[i].type;
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
}
