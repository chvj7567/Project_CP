using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    const int MAX = 9;

    [SerializeField] Image viewImg1;
    [SerializeField] Image viewImg2;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject origin;
    [SerializeField] float margin = 0f;
    [SerializeField, Range(1, MAX)] int boardSize = 1;
    [SerializeField] Transform parent;

    [SerializeField] CHInstantiateButton instBtn;

    [ReadOnly] Block[,] boardArr = new Block[MAX, MAX];
    [ReadOnly] public bool isDrag = false;
    [ReadOnly] public bool isAni = false;
    [ReadOnly] bool isMatch = false;
    [ReadOnly] public float delay;

    [SerializeField, ReadOnly] int moveIndex1 = 0;
    [SerializeField, ReadOnly] int moveIndex2 = 0;

    [SerializeField] CHTMPro totScoreText;
    [SerializeField] CHTMPro bonusScoreText;

    [SerializeField, ReadOnly] ReactiveProperty<int> totScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> oneTimeScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> bonusScore = new ReactiveProperty<int>();

    [SerializeField, ReadOnly] List<int> towerPoint = new List<int>();

    List<Sprite> normalBlockSpriteList = new List<Sprite>();
    List<Sprite> specialBlockSpriteList = new List<Sprite>();

    async void Start()
    {
        for (int i = 0; i < (int)Defines.ENormalBlockType.Max; ++i)
        {
            CHMMain.Resource.LoadSprite((Defines.ENormalBlockType)i, (sprite) =>
            {
                if (sprite != null)
                    normalBlockSpriteList.Add(sprite);
            });
        }

        for (int i = 0; i < (int)Defines.ESpecailBlockType.Max; ++i)
        {
            CHMMain.Resource.LoadSprite((Defines.ESpecailBlockType)i, (sprite) =>
            {
                if (sprite != null)
                    specialBlockSpriteList.Add(sprite);
            });
        }

        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                CHInstantiateButton.ResetBlockDict();
                SceneManager.LoadScene(0);
            });
        }

        totScore.Subscribe(_ =>
        {
            totScoreText.SetText(_);
        });

        bonusScore.Subscribe(_ =>
        {
            bonusScoreText.SetText(_);
        });

        boardSize = PlayerPrefs.GetInt("size");

        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        await UpdateMap(true);

        AfterDrag(null, null);
    }

    private void Update()
    {
        if (isAni == true)
        {
            viewImg1.color = Color.red;
        }
        else
        {
            viewImg1.color = Color.green;
        }

        if (isDrag == true)
        {
            viewImg2.color = Color.red;
        }
        else
        {
            viewImg2.color = Color.green;
        }
    }

    public async void AfterDrag(Block block1, Block block2)
    {
        isAni = true;
        if (block1) moveIndex1 = block1.index;
        if (block2) moveIndex2 = block2.index;

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

        towerPoint.Add(oneTimeScore.Value);
        totScore.Value += oneTimeScore.Value;
        totScore.Value += bonusScore.Value;
        oneTimeScore.Value = 0;
        bonusScore.Value = 0;
        isAni = false;
    }

    async Task UpdateMap(bool first = false)
    // 맵생성
    {
        foreach (var block in boardArr)
        {
            if (block == null) continue;

            if (first)
            {
                float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
                block.originPos.x -= moveDis;
                block.SetOriginPos();
                block.rectTransform.DOScale(1f, delay);
            }

            if (first == true || block.state == Defines.EState.Match)
            {
                var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);

                block.SetNormalType((Defines.ENormalBlockType)random);
                block.state = Defines.EState.Normal;
                block.img.sprite = normalBlockSpriteList[random];

                if (first == false)
                {
                    block.ResetScore();
                    block.SetOriginPos();
                    block.rectTransform.DOScale(1f, delay);
                }
            }
        }

        await Task.Delay((int)(delay * 1000));
    }

    void CheckMap()
    // 3Match 블럭 제거
    {
        for (int i = 0; i < boardSize; ++i)
        {
            List<Block> hBlockList = new List<Block>();

            foreach (var block in boardArr)
            {
                if (block == null) continue;

                if (block.row == i)
                {
                    hBlockList.Add(block);
                }
            }

            CheckMatch(hBlockList, Defines.EDirection.Horizontal);
        }

        for (int i = 0; i < boardSize; ++i)
        {
            List<Block> vBlockList = new List<Block>();

            foreach (var block in boardArr)
            {
                if (block == null) continue;

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
        bool createBoomDelay = false;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                var block = boardArr[i, j];
                if (block == null) continue;

                // 없어져야 할 블럭
                if (block.state == Defines.EState.Match)
                {
                    oneTimeScore.Value += 1;

                    // 아이템이 연달아 터지는 경우
                    if (block.GetSpecailType() == Defines.ESpecailBlockType.CatPang1)
                    {
                        bonusScore.Value += 10;
                        Boom2(block, false);
                        i = -1;
                        break;
                    }

                    removeDelay = true;

                    int row = i;
                    int col = j;
                    block.rectTransform.DOScale(0f, delay).OnComplete(() =>
                    {
                        // 블럭 제거 후 + 매치 특수 블럭 생성
                        if (block.hScore >= 3 && block.vScore >= 3)
                        {
                            createBoomDelay = true;
                            block.SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                            block.state = Defines.EState.Normal;
                            block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                            block.ResetScore();
                            block.SetOriginPos();
                            block.rectTransform.DOScale(1f, delay);

                            for (int idx = 1; idx < MAX; ++idx)
                            {
                                int tempRow = row + idx;
                                int tempCol = col;
                                if (IsValidIndex(tempRow, tempCol) && boardArr[tempRow, tempCol] != null)
                                {
                                    if (boardArr[tempRow, tempCol].hScore > 0
                                    || boardArr[tempRow, tempCol].vScore > 0)
                                    {
                                        boardArr[tempRow, tempCol].ResetScore();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            for (int idx = 1; idx < MAX; ++idx)
                            {
                                int tempRow = row - idx;
                                int tempCol = col;
                                if (IsValidIndex(tempRow, tempCol) && boardArr[tempRow, tempCol] != null)
                                {
                                    if (boardArr[tempRow, tempCol].hScore > 0
                                    || boardArr[tempRow, tempCol].vScore > 0)
                                    {
                                        boardArr[tempRow, tempCol].ResetScore();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            for (int idx = 1; idx < MAX; ++idx)
                            {
                                int tempRow = row;
                                int tempCol = col + idx;
                                if (IsValidIndex(tempRow, tempCol) && boardArr[tempRow, tempCol] != null)
                                {
                                    if (boardArr[tempRow, tempCol].hScore > 0
                                    || boardArr[tempRow, tempCol].vScore > 0)
                                    {
                                        boardArr[tempRow, tempCol].ResetScore();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            for (int idx = 1; idx < MAX; ++idx)
                            {
                                int tempRow = row;
                                int tempCol = col - idx;
                                if (IsValidIndex(tempRow, tempCol) && boardArr[tempRow, tempCol] != null)
                                {
                                    if (boardArr[tempRow, tempCol].hScore > 0
                                    || boardArr[tempRow, tempCol].vScore > 0)
                                    {
                                        boardArr[tempRow, tempCol].ResetScore();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        if (removeDelay)
        {
            await Task.Delay((int)(delay * 1000));
        }

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                var block = boardArr[i, j];
                if (block == null) continue;

                if (block.state == Defines.EState.Match)
                {
                    // 가로 3개 초과 매치 시 특수 블럭 생성
                    if (block.hScore > 3)
                    {
                        createBoomDelay = true;

                        bool checkMoveBlock = false;

                        int tempScore = block.hScore;
                        for (int idx = 0; idx < tempScore; ++idx)
                        {
                            int tempRow = i;
                            int tempCol = j + idx;
                            if (boardArr[tempRow, tempCol].index == moveIndex1 ||
                                boardArr[tempRow, tempCol].index == moveIndex2)
                            {
                                checkMoveBlock = true;
                                boardArr[tempRow, tempCol].SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                                boardArr[tempRow, tempCol].state = Defines.EState.Normal;
                                boardArr[tempRow, tempCol].img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                                boardArr[tempRow, tempCol].rectTransform.DOScale(1f, delay);
                            }

                            boardArr[tempRow, tempCol].ResetScore();
                        }

                        if (checkMoveBlock == false)
                        {
                            block.SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                            block.state = Defines.EState.Normal;
                            block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                            block.rectTransform.DOScale(1f, delay);
                        }
                    }
                    // 세로 3개 초과 매치 시 특수 블럭 생성
                    else if (block.vScore > 3)
                    {
                        createBoomDelay = true;

                        bool checkMoveBlock = false;

                        int tempScore = block.vScore;
                        for (int idx = 0; idx < tempScore; ++idx)
                        {
                            int tempRow = i + idx;
                            int tempCol = j;

                            if (boardArr[tempRow, tempCol].index == moveIndex1 ||
                                boardArr[tempRow, tempCol].index == moveIndex2)
                            {
                                checkMoveBlock = true;
                                boardArr[tempRow, tempCol].SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                                boardArr[tempRow, tempCol].state = Defines.EState.Normal;
                                boardArr[tempRow, tempCol].img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                                boardArr[tempRow, tempCol].rectTransform.DOScale(1f, delay);
                            }

                            boardArr[tempRow, tempCol].ResetScore();
                        }

                        if (checkMoveBlock == false)
                        {
                            block.SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                            block.state = Defines.EState.Normal;
                            block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                            block.rectTransform.DOScale(1f, delay);
                        }
                    }
                }
            }
        }

        if (createBoomDelay)
        {
            await Task.Delay((int)(delay * 1000));
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
            for (int i = boardSize - 1; i > block.row; --i)
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
            if (block == null) continue;

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

    bool IsValidIndex(int row, int column)
    {
        return row >= 0 && row < MAX && column >= 0 && column < MAX;
    }

    public void BoomAll(Block block, bool ani = true)
    {
        bonusScore.Value += 5;
        block.SetNormalType(Defines.ENormalBlockType.None);
        block.state = Defines.EState.Match;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                if (IsValidIndex(i, j))
                {
                    if (boardArr[i, j] != null)
                    {
                        boardArr[i, j].state = Defines.EState.Match;
                    }
                }
            }
        }

        if (ani)
        {
            AfterDrag(null, null);
        }
    }

    public void Boom1(Block block, bool ani = true)
    {
        bonusScore.Value += 5;
        block.SetNormalType(Defines.ENormalBlockType.None);
        block.state = Defines.EState.Match;

        if (IsValidIndex(block.row - 1, block.col - 1))
        {
            if (boardArr[block.row - 1, block.col - 1] != null)
                boardArr[block.row - 1, block.col - 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row - 1, block.col))
        {
            if (boardArr[block.row - 1, block.col] != null)
                boardArr[block.row - 1, block.col].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row - 1, block.col + 1))
        {
            if (boardArr[block.row - 1, block.col + 1] != null)
                boardArr[block.row - 1, block.col + 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row, block.col - 1))
        {
            if (boardArr[block.row, block.col - 1] != null)
                boardArr[block.row, block.col - 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row, block.col + 1))
        {
            if (boardArr[block.row, block.col + 1] != null)
                boardArr[block.row, block.col + 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row + 1, block.col - 1))
        {
            if (boardArr[block.row + 1, block.col - 1] != null)
                boardArr[block.row + 1, block.col - 1].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row + 1, block.col))
        {
            if (boardArr[block.row + 1, block.col] != null)
                boardArr[block.row + 1, block.col].state = Defines.EState.Match;
        }

        if (IsValidIndex(block.row + 1, block.col + 1))
        {
            if (boardArr[block.row + 1, block.col + 1] != null)
                boardArr[block.row + 1, block.col + 1].state = Defines.EState.Match;
        }

        if (ani)
        {
            AfterDrag(null, null);
        }
    }

    public void Boom2(Block block, bool ani = true)
    {
        bonusScore.Value += 5;
        block.SetNormalType(Defines.ENormalBlockType.None);
        block.state = Defines.EState.Match;

        for (int i = 0; i < MAX; ++i)
        {
            if (IsValidIndex(i, block.col))
            {
                if (boardArr[i, block.col] != null)
                    boardArr[i, block.col].state = Defines.EState.Match;
            }
        }

        for (int i = 0; i < MAX; ++i)
        {
            if (IsValidIndex(block.row, i))
            {
                if (boardArr[block.row, i] != null)
                    boardArr[block.row, i].state = Defines.EState.Match;
            }
        }

        if (ani)
        {
            AfterDrag(null, null);
        }
    }
}
