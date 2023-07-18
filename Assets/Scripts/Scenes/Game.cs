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

    List<int> removeIndex = new List<int>();

    [ReadOnly]
    Block[,] boardArr = new Block[9, 9];

    [ReadOnly]
    public bool isDrag = false;

    async void Start()
    {
        CHMMain.UI.CreateEventSystemObject();
        instBtn.InstantiateButton(origin, margin, horizontalCount, verticalCount, parent, boardArr);
        MakeMap();

        for (int i = 0; i < 100; ++i)
        {
            await Task.Delay(1000);
            CheckMap();
            await Task.Delay(1000);
            DownBlock();
            await Task.Delay(1000);
            UpdateMap();

            removeIndex.Clear();
        }
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

                block.transform.localScale = Vector3.one;
                block.ResetScore();
                block.MoveOriginPos();
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
                block.type = (Defines.ESpriteType)random;
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

            CheckMatch(hBlockList, removeIndex, Defines.EDirection.Horizontal);
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

            CheckMatch(vBlockList, removeIndex, Defines.EDirection.Vertical);
        }

        foreach (var block in boardArr)
        {
            if (block.state == Defines.EState.Match)
            {
                var rectTransform = block.GetComponent<RectTransform>();
                var pos = rectTransform.anchoredPosition;
                var boardWidth = CHInstantiateButton.GetVerticalDistance() * verticalCount + margin * verticalCount;
                block.rectTransform.DOAnchorPosY(pos.y + boardWidth + 100f, 2f);
                block.transform.DOScale(.5f, .5f);
            }
        }
    }

    void CheckMatch(List<Block> blockList, List<int> removeIndex, Defines.EDirection direction)
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
            if (removeIndex.Contains(block.index) == false)
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

                    boardArr[targetRow, col] = moveBlock;
                    boardArr[row, col] = targetBlock;
                }
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
}
