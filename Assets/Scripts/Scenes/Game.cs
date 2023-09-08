using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    const int MAX = 9;

    [SerializeField] bool addDefense;
    [SerializeField] Image timerImg;
    [SerializeField, ReadOnly] float curTimer;
    [SerializeField] Image goldImg;
    [SerializeField] Image viewImg1;
    [SerializeField] Image viewImg2;
    [SerializeField] Button backBtn;
    [SerializeField] Toggle selectTog;
    [SerializeField] GameObject origin;
    [SerializeField] float margin = 0f;
    [SerializeField, Range(1, MAX)] int boardSize = 1;
    [SerializeField] Transform parent;
    [SerializeField] ReactiveProperty<int> boomAllChance = new ReactiveProperty<int>();
    [SerializeField] Button boomAllBtn;
    [SerializeField] CHTMPro boomAllChanceText;
    [SerializeField] RectTransform darkBall;
    [SerializeField] CHInstantiateButton instBtn;
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField, ReadOnly] int backgroundIndex = 0;

    [SerializeField] Spawner spawner;
    [SerializeField] public float delay;

    [ReadOnly] Block[,] boardArr = new Block[MAX, MAX];
    [ReadOnly] public bool isDrag = false;
    [ReadOnly] public bool isAni = false;
    [ReadOnly] bool isMatch = false;

    [SerializeField, ReadOnly] float teachTime;
    [SerializeField, ReadOnly] int canMatchRow = -1;
    [SerializeField, ReadOnly] int canMatchCol = -1;

    [SerializeField, ReadOnly] int moveIndex1 = 0;
    [SerializeField, ReadOnly] int moveIndex2 = 0;

    [SerializeField] int maxPower = 99999;
    [SerializeField] float minDelay = .1f;
    [SerializeField] float maxSpeed = 30f;
    [SerializeField] CHTMPro targetScoreText;
    [SerializeField] CHTMPro curScoreText;
    [SerializeField] CHTMPro oneTimeScoreText;
    [SerializeField] CHTMPro killCountText;
    [SerializeField] CHTMPro powerText;
    [SerializeField] CHTMPro delayText;
    [SerializeField] CHTMPro speedText;
    [SerializeField] int selectScore;
    [SerializeField] int selectCurScore;
    [SerializeField, ReadOnly] ReactiveProperty<int> curScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> totScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> oneTimeScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> bonusScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] public ReactiveProperty<int> killCount = new ReactiveProperty<int>();

    [SerializeField, ReadOnly] ReactiveProperty<int> power = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<float> attackDelay = new ReactiveProperty<float>();
    [SerializeField, ReadOnly] ReactiveProperty<float> attackSpeed = new ReactiveProperty<float>();
    [SerializeField, ReadOnly] ReactiveProperty<int> catPangLevel = new ReactiveProperty<int>();

    [SerializeField] GameObject gameOverObj;
    [SerializeField] TMP_Text gameOverText;
    [SerializeField, ReadOnly] public ReactiveProperty<bool> gameOver = new ReactiveProperty<bool>();

    [SerializeField, ReadOnly] List<int> towerPoint = new List<int>();

    List<Sprite> blockSpriteList = new List<Sprite>();

    Infomation.StageInfo stageInfo;
    List<Infomation.StageBlockInfo> stageBlockInfoList = new List<Infomation.StageBlockInfo>();

    bool oneTimeAlarm = false;

    CancellationTokenSource tokenSource;

    async void Start()
    {
        tokenSource = new CancellationTokenSource();

        backgroundIndex = PlayerPrefs.GetInt("background");

        ChangeBackgroundLoop();

        for (int i = 0; i < (int)Defines.EBlockState.Max; ++i)
        {
            CHMMain.Resource.LoadSprite((Defines.EBlockState)i, (sprite) =>
            {
                if (sprite != null)
                    blockSpriteList.Add(sprite);
            });
        }

        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMMain.UI.CloseUI(Defines.EUI.UIAlarm);
                CHMMain.Pool.Clear();
                PlayerPrefs.SetInt("background", backgroundIndex);
                SceneManager.LoadScene(0);
            });
        }

        curScore.Subscribe(_ =>
        {
            curScoreText.SetText(_);
        });

        oneTimeScore.Subscribe(_ =>
        {
            oneTimeScoreText.SetText(_);
        });

        killCount.Subscribe(_ =>
        {
            killCountText.SetText(_);
        });

        power.Subscribe(_ =>
        {
            powerText.SetText(_);
        });

        attackDelay.Subscribe(_ =>
        {
            var a = Mathf.Round(_ * 10) / 10;
            delayText.SetText(a);
        });

        attackSpeed.Subscribe(_ =>
        {
            var a = Mathf.Round(_ * 10) / 10;
            speedText.SetText(a);
        });

        gameOverObj.SetActive(false);

        gameOver.Subscribe(_ =>
        {
            if (_ == true)
            {
                Time.timeScale = 0;
                CHMMain.UI.CloseUI(Defines.EUI.UIChoice);
                gameOverObj.transform.SetAsLastSibling();
                gameOverObj.SetActive(true);
                gameOverText.DOText("GameOver", 5f);
            }
        });

        power.Value = spawner.GetAttackCatList().First().attackPower;
        attackDelay.Value = spawner.GetAttackCatList().First().attackDelay;
        attackSpeed.Value = spawner.GetAttackCatList().First().attackSpeed;
        catPangLevel.Value = 1;

        boomAllBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (isAni == false && boomAllChance.Value > 0)
            {
                await BoomAll();
                boomAllChance.Value -= 1;
            }
        });

        boomAllChance.Subscribe(_ =>
        {
            boomAllChanceText.SetText(_);
        });

        var stage = PlayerPrefs.GetInt("stage");
        if (stage <= 0)
        {
            stage = 1;
        }

        stageInfo = CHMMain.Json.GetStageInfo(stage);
        stageBlockInfoList = CHMMain.Json.GetStageBlockInfoList(stage);

        targetScoreText.SetText(stageInfo.targetScore);
        if (stageInfo.targetScore < 0)
        {
            targetScoreText.gameObject.SetActive(false);
        }

        boardSize = stageInfo.boardSize;
        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        await CreateMap(stage);
        CHMMain.Sound.Play(Defines.ESound.Bgm);

        this.UpdateAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                if (gameOver.Value == false)
                {
                    if (timerImg.fillAmount >= 1)
                    {
                        if (stageInfo.targetScore > 0)
                        {
                            if (totScore.Value < stageInfo.targetScore)
                            {
                                gameOverText.text = "Game Over";
                            }
                            else
                            {
                                gameOverText.text = "Game Clear";
                                SaveData();
                            }
                        }
                        else
                        {
                            bool clear = true;

                            for (int i = 0; i < boardSize; ++i)
                            {
                                for (int j = 0; j < boardSize; ++j)
                                {
                                    if (boardArr[i, j].GetHp() > 0)
                                    {
                                        clear = false;
                                    }
                                }
                            }

                            if (clear == false)
                            {
                                gameOverText.text = "Game Over";
                            }
                            else
                            {
                                gameOverText.text = "Game Clear";
                                SaveData();
                            }
                        }

                        gameOver.Value = true;
                    }
                    else
                    {
                        if (stageInfo.targetScore > 0)
                        {
                            if (totScore.Value >= stageInfo.targetScore)
                            {
                                gameOverText.text = "Game Clear";
                                gameOver.Value = true;
                                SaveData();
                            }
                        }
                        else
                        {
                            bool clear = true;

                            for (int i = 0; i < boardSize; ++i)
                            {
                                for (int j = 0; j < boardSize; ++j)
                                {
                                    if (boardArr[i, j].GetHp() > 0)
                                    {
                                        clear = false;
                                    }
                                }
                            }

                            if (clear)
                            {
                                gameOverText.text = "Game Clear";
                                gameOver.Value = true;
                                SaveData();
                            }
                        }
                    }
                }
            });

        await AfterDrag(null, null);
    }

    private async void Update()
    {
        /*if (isAni == true)
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
        }*/

        curTimer += Time.deltaTime;
        timerImg.fillAmount = curTimer / stageInfo.time;

        /*if (isAni == true)
        {
            teachTime = Time.time;
        }
        else
        {
            // 5초 동안 드래그를 안하면 알려줌
            if (teachTime + 5 < Time.time && oneTimeAlarm == false && canMatchRow >= 0 && canMatchCol >= 0)
            {
                oneTimeAlarm = true;

                var block = boardArr[canMatchRow, canMatchCol];
                block.transform.DOScale(1.5f, 0.25f).OnComplete(() =>
                {
                    block.transform.DOScale(1f, 0.25f);
                });

                await Task.Delay(3000);
                oneTimeAlarm = false;
            }
        }*/
    }

    void OnDestroy()
    {
        if (tokenSource != null && !tokenSource.IsCancellationRequested)
        {
            tokenSource.Cancel();
        }
    }

    async Task ChangeBackgroundLoop()
    {
        for (int i = 0; i < backgroundList.Count; ++i)
        {
            if (i != backgroundIndex)
            {
                Color color = backgroundList[i].color;
                color.a = 0f;
                backgroundList[i].color = color;
            }
            else
            {
                Color color = backgroundList[i].color;
                color.a = 1f;
                backgroundList[i].color = color;
            }
        }

        await Task.Delay(5000, tokenSource.Token);

        try
        {
            while (true)
            {
                backgroundIndex = ChangeBackground();

                await Task.Delay(10000, tokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancle Change Background");
        }
    }

    int ChangeBackground()
    {
        if (backgroundIndex >= backgroundList.Count)
            return 0;

        int nextIndex = backgroundIndex + 1;
        if (nextIndex >= backgroundList.Count)
        {
            nextIndex = 0;
        }

        backgroundList[backgroundIndex].DOFade(0f, 5f);
        backgroundList[nextIndex].DOFade(1f, 5f);

        return nextIndex;
    }

    void SaveData()
    {
        if (CHMMain.Data.stageDataDic.TryGetValue(PlayerPrefs.GetInt("stage").ToString(), out var data))
        {
            data.clear = true;
        }

        CHMMain.Data.SaveJson();
    }

    public async Task AfterDrag(Block block1, Block block2)
    {
        isAni = true;
        bool first = false;

        if (block1 && block2 && block1.IsBlock() == true && block2.IsBlock() == true)
        {
            moveIndex1 = block1.index;
            moveIndex2 = block2.index;

            if (block1.IsSpecialBlock() == true)
            {
                if (block1.GetBlockState() == Defines.EBlockState.Bomb)
                {
                    await Boom3(block1, block2.GetBlockState());
                    return;
                }
            }
            else if (block2.IsSpecialBlock() == true)
            {
                if (block2.GetBlockState() == Defines.EBlockState.Bomb)
                {
                    await Boom3(block2, block1.GetBlockState());
                    return;
                }
            }
            else if (block1.IsBoomBlock() == true && block2.IsBoomBlock() == true)
            {
                first = true;
                bonusScore.Value += 30;
                block2.match = true;
                block1.changeBlockState = Defines.EBlockState.Bomb;
            }
            else if (block1.IsBoomBlock() == true)
            {
                await block1.Boom();
                return;
            }
            else if (block2.IsBoomBlock() == true)
            {
                await block2.Boom();
                return;
            }
        }

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

                    await Task.Delay((int)(delay * 1000));
                }
            }

            first = true;

            await RemoveMatchBlockAndCreateBoomBlock();
            await DownBlock();
            await UpdateMap();
            CheckMap();
        } while (isMatch == true);

        towerPoint.Add(oneTimeScore.Value);
        totScore.Value += oneTimeScore.Value;
        totScore.Value += bonusScore.Value;
        curScore.Value += oneTimeScore.Value;
        curScore.Value += bonusScore.Value;
        oneTimeScore.Value = 0;
        bonusScore.Value = 0;

        await Task.Delay((int)(delay * 1000));

        if (addDefense == true)
        {
            if (totScore.Value > 0 && gameOver.Value == false && selectTog.isOn)
            {
                if (totScore.Value >= selectCurScore)
                {
                    var temp = totScore.Value / selectScore + 1;
                    selectCurScore = selectScore * temp;

                    CHMMain.UI.ShowUI(Defines.EUI.UIChoice, new UIChoiceArg
                    {
                        curScore = curScore,
                        power = power,
                        delay = attackDelay,
                        speed = attackSpeed,
                        catPangLevel = catPangLevel,
                        attackCatList = spawner.GetAttackCatList(),
                        maxPower = maxPower,
                        minDealy = minDelay,
                        maxSpeed = maxSpeed,
                        catPangImgList = blockSpriteList,
                    });
                }
            }
        }

        isAni = false;
    }

    async Task CreateMap(int _stage)
    {
        foreach (var block in boardArr)
        {
            if (block == null) continue;
            float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
            block.originPos.x -= moveDis;
            block.SetOriginPos();

            block.changeUpScale = true;
            block.rectTransform.DOScale(1f, delay).OnComplete(() => { block.changeUpScale = false; });

            var stageBlockInfo = stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
            if (stageBlockInfo == null)
            {
                var random = UnityEngine.Random.Range(0, stageInfo.blockTypeCount);

                block.SetBlockState(Defines.ELog.CreateMap, 1, (Defines.EBlockState)random);
                block.img.sprite = blockSpriteList[random];
                block.SetHp(-1);
            }
            else
            {
                block.SetBlockState(Defines.ELog.CreateMap, 2, stageBlockInfo.blockState);
                block.img.sprite = blockSpriteList[stageBlockInfo.index];

                if (block.IsNormalBlock() == true)
                {
                    block.SetHp(-1);
                }
                else
                {
                    block.SetHp(stageBlockInfo.hp);
                }
            }
        }

        await Task.Delay((int)(delay * 1000));
    }

    async Task UpdateMap()
    // 맵생성
    {
        try
        {
            bool reupdate = false;

            do
            {
                foreach (var block in boardArr)
                {
                    if (block == null ||
                        (block.IsFixdBlock() == true && block.IsMatch() == false))
                        continue;

                    if (reupdate == true || block.IsMatch() == true)
                    {
                        var random = UnityEngine.Random.Range(0, stageInfo.blockTypeCount);

                        CreateBlock(block, Defines.ELog.UpdateMap, 1, (Defines.EBlockState)random);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.SetOriginPos();
                    }
                    else
                    {
                        if (block.changeBlockState != Defines.EBlockState.None)
                        {
                            CreateBlock(block, Defines.ELog.UpdateMap, 2, block.changeBlockState);
                            block.SetHp(-1);
                            block.ResetScore();
                            block.SetOriginPos();
                            block.changeBlockState = Defines.EBlockState.None;
                        }
                    }
                }

                reupdate = CanMatch() == false;

                if (reupdate == true)
                {
                    CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        alarmText = $"Remake Map"
                    });
                }

                await Task.Delay((int)(delay * 1000), tokenSource.Token);

            } while (reupdate == true);

            isMatch = false;
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancle Update Map");
        }
    }

    bool CanMatch()
    // Match가 가능한 맵인지 검사
    {
        isMatch = false;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                if (boardArr[i, j] == null)
                    continue;

                var curBlock = boardArr[i, j];
                if (curBlock.IsFixdBlock() == true)
                    continue;

                if (curBlock.IsBlock() == true)
                {
                    canMatchRow = curBlock.row;
                    canMatchCol = curBlock.col;
                    return true;
                }

                var upBlock = IsValidIndex(i - 1, j) == true ? boardArr[i - 1, j] : null;
                var downBlock = IsValidIndex(i + 1, j) == true ? boardArr[i + 1, j] : null;
                var leftBlock = IsValidIndex(i, j - 1) == true ? boardArr[i, j - 1] : null;
                var rightBlock = IsValidIndex(i, j + 1) == true ? boardArr[i, j + 1] : null;

                if (upBlock != null &&
                    upBlock.IsFixdBlock() == false)
                {
                    ChangeBlock(curBlock, upBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, upBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        return true;
                    }
                }

                if (downBlock != null &&
                    downBlock.IsFixdBlock() == false)
                {
                    ChangeBlock(curBlock, downBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, downBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        return true;
                    }
                }

                if (leftBlock != null &&
                    leftBlock.IsFixdBlock() == false)
                {
                    ChangeBlock(curBlock, leftBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, leftBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        return true;
                    }
                }

                if (rightBlock != null &&
                    rightBlock.IsFixdBlock() == false)
                {
                    ChangeBlock(curBlock, rightBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, rightBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void CheckMap(bool _test = false)
    // 3Match 블럭 확인
    {
        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                CheckSquareMatch(i, j, _test);
            }
        }

        for (int i = 0; i < boardSize; ++i)
        {
            List<Block> hBlockList = new List<Block>();
            foreach (var block in boardArr)
            {
                if (block == null)
                    continue;

                if (block.row == i)
                {
                    hBlockList.Add(block);
                }
            }

            Check3Match(hBlockList, Defines.EDirection.Horizontal, _test);
        }

        for (int i = 0; i < boardSize; ++i)
        {
            List<Block> vBlockList = new List<Block>();

            foreach (var block in boardArr)
            {
                if (block == null)
                    continue;

                if (block.col == i)
                {
                    vBlockList.Add(block);
                }
            }

            Check3Match(vBlockList, Defines.EDirection.Vertical, _test);
        }
    }

    bool CheckSquareMatch(int row, int col, bool _test = false)
    // 스퀘어 Match 확인
    // 좌측 상단부터 확인
    {
        if (IsNormalBlock(row, col) == false || IsNormalBlock(row + 1, col) == false ||
            IsNormalBlock(row, col + 1) == false || IsNormalBlock(row + 1, col + 1) == false)
            return false;

        if (boardArr[row, col].IsMatch() == true ||
            boardArr[row + 1, col].IsMatch() == true ||
            boardArr[row, col + 1].IsMatch() == true ||
            boardArr[row + 1, col + 1].IsMatch() == true)
            return false;

        Defines.EBlockState normalBlockType = boardArr[row, col].GetBlockState();

        if (normalBlockType != boardArr[row + 1, col].GetBlockState())
            return false;

        if (normalBlockType != boardArr[row, col + 1].GetBlockState())
            return false;

        if (normalBlockType != boardArr[row + 1, col + 1].GetBlockState())
            return false;

        // 여기서부터는 스퀘어 매치 확정
        isMatch = true;

        if (_test == false)
        {
            boardArr[row, col].match = true;
            boardArr[row + 1, col].match = true;
            boardArr[row, col + 1].match = true;
            boardArr[row + 1, col + 1].match = true;

            if (moveIndex1 == boardArr[row, col].index ||
                moveIndex2 == boardArr[row, col].index)
            {
                boardArr[row, col].squareMatch = true;
            }
            else if (moveIndex1 == boardArr[row + 1, col].index ||
                moveIndex2 == boardArr[row + 1, col].index)
            {
                boardArr[row + 1, col].squareMatch = true;
            }
            else if (moveIndex1 == boardArr[row, col + 1].index ||
                moveIndex2 == boardArr[row, col + 1].index)
            {
                boardArr[row, col + 1].squareMatch = true;
            }
            else if (moveIndex1 == boardArr[row + 1, col + 1].index ||
                moveIndex2 == boardArr[row + 1, col + 1].index)
            {
                boardArr[row + 1, col + 1].squareMatch = true;
            }
            else
            {
                boardArr[row, col].squareMatch = true;
            }
        }

        return true;
    }

    async Task RemoveMatchBlockAndCreateBoomBlock()
    // Match된 블럭 제거, 폭탄 블럭 생성
    {
        bool removeDelay = false;
        bool createBoomDelay = false;

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                // 없어져야 할 블럭
                if (block.IsMatch() == true)
                {
                    CheckArround(block.row, block.col);
                    var gold = CHMMain.Resource.Instantiate(goldImg.gameObject, transform.parent);
                    if (gold != null)
                    {
                        var rect = gold.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = block.rectTransform.anchoredPosition;
                            rect.DOAnchorPosY(rect.anchoredPosition.y + UnityEngine.Random.Range(30f, 50f), .5f).OnComplete(() =>
                            {
                                CHMMain.Sound.Play(Defines.ESound.Gold);
                                rect.DOAnchorPos(goldImg.rectTransform.anchoredPosition, UnityEngine.Random.Range(.2f, 1f)).OnComplete(() =>
                                {
                                    CHMMain.Resource.Destroy(gold);
                                });
                            });
                        }
                    }

                    oneTimeScore.Value += 1;

                    // 아이템이 연달아 터지는 경우
                    if (block.IsBoomBlock() == true && block.boom == false)
                    {
                        bonusScore.Value += 20;
                        await block.Boom(false);

                        // Match 검사를 다시 해야함
                        i = -1;
                        break;
                    }

                    removeDelay = true;

                    int row = i;
                    int col = j;

                    if (block.changeUpScale == true)
                    {
                        Debug.Log($"Not Remove Up Sclae {row}/{col}");
                    }
                    else if (block.changeDownScale == true)
                    {
                        Debug.Log($"Not Remove Down Sclae {row}/{col}");
                    }
                    else
                    {
                        block.changeDownScale = true;
                        block.rectTransform.DOScale(0f, delay).OnComplete(() =>
                        {
                            block.changeDownScale = false;
                            // 스퀘어 매치 블럭 생성
                            if (block.squareMatch == true)
                            {
                                var pangType = block.GetPangType();
                                CreateBlock(block, Defines.ELog.RemoveMatchBlock, 1, pangType);
                                block.ResetScore();
                                block.SetOriginPos();
                                block.squareMatch = false;
                            }

                            // 십자가 매치 블럭 생성
                            if (block.hScore >= 3 && block.vScore >= 3)
                            {
                                createBoomDelay = true;

                                if (block.hScore >= 4 || block.vScore >= 4)
                                {
                                    CreateBlock(block, Defines.ELog.RemoveMatchBlock, 2, Defines.EBlockState.Arrow5);
                                }
                                else
                                {
                                    CreateBlock(block, Defines.ELog.RemoveMatchBlock, 3, Defines.EBlockState.Arrow6);
                                }

                                block.ResetScore();
                                block.SetOriginPos();

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
        }

        if (removeDelay)
        {
            CHMMain.Sound.Play(Defines.ESound.Cat);
            await Task.Delay((int)(delay * 1000));
        }

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null) continue;

                block.ResetCheckWallDamage();

                if (block.IsMatch() == true)
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
                            if (IsValidIndex(tempRow, tempCol) == false || boardArr[tempRow, tempCol] == null)
                                continue;

                            if (boardArr[tempRow, tempCol].index == moveIndex1 ||
                                boardArr[tempRow, tempCol].index == moveIndex2)
                            {
                                checkMoveBlock = true;
                                if (tempScore >= 5)
                                {
                                    CreateBlock(block, Defines.ELog.RemoveMatchBlock, 4, Defines.EBlockState.Arrow1);
                                }
                                else
                                {
                                    CreateBlock(block, Defines.ELog.RemoveMatchBlock, 5, Defines.EBlockState.Arrow4);
                                }
                            }

                            boardArr[tempRow, tempCol].ResetScore();
                        }

                        if (checkMoveBlock == false)
                        {
                            if (tempScore >= 5)
                            {
                                CreateBlock(block, Defines.ELog.RemoveMatchBlock, 6, Defines.EBlockState.Arrow2);
                            }
                            else
                            {
                                CreateBlock(block, Defines.ELog.RemoveMatchBlock, 7, Defines.EBlockState.Arrow3);
                            }
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

                            if (IsValidIndex(tempRow, tempCol) == false || boardArr[tempRow, tempCol] == null)
                                continue;

                            if (boardArr[tempRow, tempCol].index == moveIndex1 ||
                                boardArr[tempRow, tempCol].index == moveIndex2)
                            {
                                checkMoveBlock = true;
                                if (tempScore >= 5)
                                {
                                    CreateBlock(block, Defines.ELog.RemoveMatchBlock, 8, Defines.EBlockState.CatPang5);
                                }
                                else
                                {
                                    CreateBlock(block, Defines.ELog.RemoveMatchBlock, 9, Defines.EBlockState.CatPang1);
                                }
                            }

                            boardArr[tempRow, tempCol].ResetScore();
                        }

                        if (checkMoveBlock == false)
                        {
                            if (tempScore >= 5)
                            {
                                CreateBlock(block, Defines.ELog.RemoveMatchBlock, 10, Defines.EBlockState.CatPang5);
                            }
                            else
                            {
                                CreateBlock(block, Defines.ELog.RemoveMatchBlock, 11, Defines.EBlockState.CatPang1);
                            }
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

    void CreateBlock(Block _block, Defines.ELog _log, int _key, Defines.EBlockState _boomBlock)
    {
        _block.SetBlockState(_log, _key, _boomBlock);
        _block.img.sprite = blockSpriteList[(int)_boomBlock];
        _block.match = false;
        _block.boom = false;
        _block.squareMatch = false;
        if (_block.changeUpScale == true)
        {
            Debug.Log($"Not Create Up Scale {_block.row}/{_block.col}");
        }
        else if (_block.changeDownScale == true)
        {
            Debug.Log($"Not Create Down Scale {_block.row}/{_block.col}");
        }
        else
        {
            _block.changeUpScale = true;
            _block.rectTransform.DOScale(1f, delay).OnComplete(() => { _block.changeUpScale = false; });
        }
    }

    void Check3Match(List<Block> blockList, Defines.EDirection direction, bool test = false)
    // 3Match 블럭 확인
    {
        Defines.EBlockState blockState = Defines.EBlockState.None;
        List<int> tempIndex = new List<int>();
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            if (blockList[i].IsFixdBlock() == true)
            {
                blockState = Defines.EBlockState.None;
                matchCount = 0;
                continue;
            }

            if (blockState == Defines.EBlockState.None)
            {
                blockState = blockList[i].GetBlockState();
                matchCount = 1;
                tempIndex.Add(blockList[i].index);
            }
            else if (blockState == blockList[i].GetBlockState())
            {
                ++matchCount;
                tempIndex.Add(blockList[i].index);

                if (matchCount >= 3)
                {
                    if (test == false)
                    {
                        int temp = i;
                        for (int j = 0; j < matchCount; ++j)
                        {
                            var block = blockList[temp--];
                            block.SetScore(matchCount, direction);
                            block.match = true;
                        }
                    }

                    isMatch = true;
                }
            }
            else
            {
                blockState = blockList[i].GetBlockState();
                matchCount = 1;
                tempIndex.Clear();
                tempIndex.Add(blockList[i].index);
            }
        }
    }

    async Task DownBlock()
    // 블럭 아래로 이동
    {
        List<Block> order = new List<Block>();
        for (int i = boardSize - 1; i >= 0; --i)
        {
            for (int j = boardSize - 1; j >= 0; --j)
            {
                var block = boardArr[i, j];
                if (block != null)
                {
                    order.Add(block);
                }
            }
        }

        foreach (var block in order)
        {
            int row = block.row;
            int col = block.col;

            if (boardArr[row, col].IsMatch() == true || boardArr[row, col].IsFixdBlock() == true)
                continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;

            int wallRow = -1;
            for (int i = boardSize - 1; i > row; --i)
            {
                if (boardArr[i, col].GetBlockState() == Defines.EBlockState.Wall)
                {
                    wallRow = i;
                }
            }

            if (wallRow == -1)
            {
                for (int i = boardSize - 1; i > row; --i)
                {
                    if (boardArr[i, col].IsMatch() == true)
                    {
                        targetBlock = boardArr[i, col];
                        break;
                    }
                }
            }
            else
            {
                for (int i = wallRow; i > row; --i)
                {
                    if (boardArr[i, col].IsMatch() == true)
                    {
                        targetBlock = boardArr[i, col];
                        break;
                    }
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

            if (block.IsFixdBlock() == false)
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

    bool IsNormalBlock(int row, int col)
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null ||
            boardArr[row, col].IsFixdBlock() == true || boardArr[row, col].IsBlock() == false)
            return false;

        return true;
    }

    bool IsValidIndex(int row, int col)
    {
        return row >= 0 && row < MAX && col >= 0 && col < MAX;
    }

    bool changeMatchState(int row, int col)
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return false;

        if (boardArr[row, col].IsFixdBlock() == false && boardArr[row, col].match == false)
        {
            boardArr[row, col].match = true;
            return true;
        }

        return false;
    }

    void CheckArround(int row, int col)
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return;

        DamageArround(row - 1, col);
        DamageArround(row, col + 1);
        DamageArround(row, col - 1);
        DamageArround(row + 1, col);
    }

    void DamageArround(int row, int col)
    {
        if (IsValidIndex(row, col) && boardArr[row, col] != null)
        {
            if (boardArr[row, col].IsFixdBlock() == true)
            {
                boardArr[row, col].Damage();
            }
        }
    }

    public async Task BoomAll(bool ani = true)
    // 맵 전체 폭탄
    {
        bonusScore.Value += 5;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                changeMatchState(i, j);
            }
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom1(Block block, bool ani = true)
    // 자기 주변 1칸 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        changeMatchState(block.row - 1, block.col - 1);
        changeMatchState(block.row - 1, block.col);
        changeMatchState(block.row - 1, block.col + 1);
        changeMatchState(block.row, block.col - 1);
        changeMatchState(block.row, block.col + 1);
        changeMatchState(block.row + 1, block.col - 1);
        changeMatchState(block.row + 1, block.col);
        changeMatchState(block.row + 1, block.col + 1);

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom2(Block block, bool ani = true)
    // 십자가 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(i, block.col);
        }

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(block.row, i);
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom3(Block _specialBlock, Defines.EBlockState _blockState, bool _ani = true)
    // 같은 블럭 폭탄
    {
        bonusScore.Value += 5;
        _specialBlock.match = true;
        _specialBlock.boom = true;

        List<GameObject> blueHoleList = new List<GameObject>();

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                if (block.GetBlockState() == _blockState)
                {
                    block.match = true;

                    var blueHoleObj = CHMMain.Resource.Instantiate(darkBall.gameObject, transform.parent);
                    blueHoleObj.SetActive(true);
                    var rt = blueHoleObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = _specialBlock.rectTransform.anchoredPosition;
                    rt.DOAnchorPos(block.rectTransform.anchoredPosition, .2f);
                    blueHoleList.Add(blueHoleObj);

                    await Task.Delay(200);
                }
            }
        }

        if (_ani)
        {
            AfterDrag(null, null);
        }

        await Task.Delay(1000);

        for (int i = 0; i < blueHoleList.Count; ++i)
        {
            CHMMain.Resource.Destroy(blueHoleList[i]);
        }
    }

    public async Task Boom4(Block block, bool ani = true)
    // 가로 줄 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(block.row, i);
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom5(Block block, bool ani = true)
    // 세로 줄 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(i, block.col);
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom6(Block block, bool ani = true)
    // X자 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(block.row - i, block.col - i);
            changeMatchState(block.row - i, block.col + i);
            changeMatchState(block.row + i, block.col - i);
            changeMatchState(block.row + i, block.col + i);
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom7(Block block, bool ani = true)
    // 좌하우상 대각선 줄 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(block.row - i, block.col + i);
            changeMatchState(block.row + i, block.col - i);
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom8(Block block, bool ani = true)
    // 좌상우하 대각선 줄 폭탄
    {
        bonusScore.Value += 5;
        block.match = true;
        block.boom = true;

        for (int i = 0; i < MAX; ++i)
        {
            changeMatchState(block.row - i, block.col - i);
            changeMatchState(block.row + i, block.col + i);
        }

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }
}