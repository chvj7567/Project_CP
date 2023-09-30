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
using static Defines;

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
    [SerializeField] Button plusBoomAllChance;
    [SerializeField] CHTMPro boomAllChanceText;
    [SerializeField] RectTransform bombEffectRectTransform;
    [SerializeField] ParticleSystem bombEffectPS;
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField, ReadOnly] int backgroundIndex = 0;

    [SerializeField] Spawner spawner;
    [SerializeField] public float delay;
    [SerializeField] CHInstantiateButton instBtn;

    [ReadOnly] Block[,] boardArr = new Block[MAX, MAX];
    [ReadOnly] public bool isDrag = false;
    [ReadOnly] public bool isLock = false;
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
    [SerializeField] CHTMPro moveCountText;
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
    [SerializeField, ReadOnly] ReactiveProperty<int> moveCount = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] public ReactiveProperty<int> killCount = new ReactiveProperty<int>();

    [SerializeField, ReadOnly] ReactiveProperty<int> power = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<float> attackDelay = new ReactiveProperty<float>();
    [SerializeField, ReadOnly] ReactiveProperty<float> attackSpeed = new ReactiveProperty<float>();
    [SerializeField, ReadOnly] ReactiveProperty<int> catPangLevel = new ReactiveProperty<int>();

    [SerializeField, ReadOnly] public ReactiveProperty<EGameResult> gameResult = new ReactiveProperty<EGameResult>();

    List<Sprite> blockSpriteList = new List<Sprite>();

    Infomation.StageInfo stageInfo;
    List<Infomation.StageBlockInfo> stageBlockInfoList = new List<Infomation.StageBlockInfo>();
    [SerializeField] int delayMillisecond;
    bool oneTimeAlarm = false;


    CancellationTokenSource tokenSource;

    async void Start()
    {
        tokenSource = new CancellationTokenSource();

        backgroundIndex = PlayerPrefs.GetInt(CHMMain.String.background);

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
                if (tokenSource != null && !tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }

                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMMain.UI.CloseUI(Defines.EUI.UIAlarm);
                CHMMain.Pool.Clear();
                PlayerPrefs.SetInt(CHMMain.String.background, backgroundIndex);

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

        gameResult.Value = EGameResult.None;

        gameResult.Subscribe(_ =>
        {
            if (_ == EGameResult.GameOver || _ == EGameResult.GameClear)
            {
                if (tokenSource != null && !tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
            }
        });

        power.Value = spawner.GetAttackCatList().First().attackPower;
        attackDelay.Value = spawner.GetAttackCatList().First().attackDelay;
        attackSpeed.Value = spawner.GetAttackCatList().First().attackSpeed;
        catPangLevel.Value = 1;

        boomAllBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (isLock == false && boomAllChance.Value > 0)
            {
                await BoomAll();
                boomAllChance.Value -= 1;
                AddBoomAllCount();
            }
        });

        /*boomAllChance.Subscribe(_ =>
        {
            boomAllChanceText.SetText(_);
        });

        plusBoomAllChance.OnClickAsObservable().Subscribe(_ =>
        {
            CHMAdmob.Instance.ShowRewardedAd();
            isLock = true;
            isAD.Value = true;
        });

        CHMAdmob.Instance.AcquireReward += () =>
        {
            boomAllChance.Value += 1;
            isAD.Value = false;
            isLock = false;
        };

        CHMAdmob.Instance.CloseAD += () =>
        {
            isAD.Value = false;
            isLock = false;
        };

        isAD.Subscribe(_ =>
        {
            adLoadingObj.SetActive(_);

            if (_ == true)
            {
                adLoadingObj.transform.SetAsLastSibling();
            }
        });*/

        boomAllChance.Value = 0;

        var stage = PlayerPrefs.GetInt(CHMMain.String.stage);
        if (stage <= 0)
        {
            stage = 1;
            PlayerPrefs.SetInt(CHMMain.String.stage, stage);
        }

        stageInfo = CHMMain.Json.GetStageInfo(stage);
        stageBlockInfoList = CHMMain.Json.GetStageBlockInfoList(stage);

        targetScoreText.SetText(stageInfo.targetScore);
        if (stageInfo.targetScore < 0)
        {
            targetScoreText.gameObject.SetActive(false);
        }

        if (stageInfo.moveCount > 0)
        {
            moveCount.Value = stageInfo.moveCount;
        }
        else
        {
            moveCount.Value = 9999;
        }

        boardSize = stageInfo.boardSize;

        moveCount.Subscribe(_ =>
        {
            moveCountText.SetText(_);
        });

        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        await CreateMap(stage);

        this.UpdateAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                if (gameResult.Value == EGameResult.GameClearWait)
                {
                    GameEnd(true);
                    return;
                }
                else if (gameResult.Value == EGameResult.GameOverWait)
                {
                    GameEnd(false);
                    return;
                }

                if (gameResult.Value == EGameResult.None)
                {
                    bool clear = true;

                    bool useTime = stageInfo.time > 0;
                    bool useTargetScore = stageInfo.targetScore > 0;
                    bool useMoveCount = stageInfo.moveCount > 0;

                    // 체력이 있는 블럭이 있거나 없어져야 되는 블럭이 있으면 미클리어
                    for (int i = 0; i < boardSize; ++i)
                    {
                        for (int j = 0; j < boardSize; ++j)
                        {
                            if (boardArr[i, j].GetHp() > 0 || boardArr[i, j].DisappearBlock() == true)
                            {
                                clear = false;
                                break;
                            }
                        }

                        if (clear == false)
                            break;
                    }

                    // 시간제한이 있고 시간이 다 된 경우 게임 종료 확인
                    if (useTime == true && timerImg.fillAmount >= 1)
                    {
                        // 목표 점수를 사용하는 경우 목표 점수 달성 확인
                        if (useTargetScore == true && totScore.Value < stageInfo.targetScore)
                        {
                            clear = false;
                        }

                        GameEnd(clear);
                    }
                    // 시간을 사용하고 시간이 다 되지 않은 경우 게임 종료 확인
                    else
                    {
                        // 목표 점수를 사용하는 경우 목표 점수 달성 확인
                        if (useTargetScore == true && totScore.Value < stageInfo.targetScore)
                        {
                            clear = false;
                        }

                        // 움직임 횟수를 사용하는 경우 횟수를 다 소진했을 때 게임 종료 확인
                        if (useMoveCount == true && moveCount.Value <= 0)
                        {
                            GameEnd(clear);
                        }
                        else
                        {
                            // 클리어 한 경우만 게임 종료
                            if (clear == true)
                            {
                                GameEnd(clear);
                            }
                        }
                    }
                }
            });

        //await AfterDrag(null, null);
    }

    void GameEnd(bool _clear)
    {
        if (isLock == true)
        {
            if (_clear == true)
            {
                gameResult.Value = EGameResult.GameClearWait;
            }
            else
            {
                gameResult.Value = EGameResult.GameOverWait;
            }

            return;
        }

        if (_clear == false)
        {
            gameResult.Value = EGameResult.GameOver;

            int gold = 0;
            if (GetClearState() == Defines.EClearState.Clear)
                gold = 0;
            else
                gold = totScore.Value / 3;

            CHMMain.UI.ShowUI(EUI.UIGameEnd, new UIGameEndArg
            {
                result = gameResult.Value,
                gold = gold
            });
        }
        else
        {
            gameResult.Value = EGameResult.GameClear;

            int gold = 0;
            if (GetClearState() == Defines.EClearState.Clear)
                gold = 0;
            else
                gold = totScore.Value;

            CHMMain.UI.ShowUI(EUI.UIGameEnd, new UIGameEndArg
            {
                result = gameResult.Value,
                gold = gold
            });

            SaveClearData();
        }
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

        if (isLock == false)
        {
            curTimer += Time.deltaTime;
            timerImg.fillAmount = curTimer / stageInfo.time;
        }

        if (isLock == true)
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

                await Task.Delay(3000, tokenSource.Token);
                oneTimeAlarm = false;
            }
        }
    }

    void OnDestroy()
    {
        if (tokenSource != null && !tokenSource.IsCancellationRequested)
        {
            tokenSource.Cancel();
        }
    }

    private void OnApplicationQuit()
    {
        CHMData.Instance.SaveData(CHMMain.String.catPang);
    }

    void CheckDissapearBlock()
    {
        int row = boardSize - 1;

        for (int i = 0; i < boardSize; ++i)
        {
            var block = boardArr[row, i];
            if (block.DisappearBlock() == true)
            {
                block.changeBlockState = Defines.EBlockState.BlueBomb;
            }
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

    void AddBoomAllCount()
    {
        if (CHMData.Instance.stageDataDic.TryGetValue(PlayerPrefs.GetInt(CHMMain.String.stage).ToString(), out var data))
        {
            data.boomAllCount += 1;
        }
    }

    void SaveClearData()
    {
        CHMData.Instance.GetStageData(PlayerPrefs.GetInt(CHMMain.String.stage).ToString()).clearState = Defines.EClearState.Clear;
    }

    Defines.EClearState GetClearState()
    {
        return CHMData.Instance.GetStageData(PlayerPrefs.GetInt(CHMMain.String.stage).ToString()).clearState;
    }

    void SaveBoomCollectionData(Block _block)
    {
        if (_block.IsBoomBlock() == false && _block.IsSpecialBlock() == false)
            return;

        if (GetClearState() == Defines.EClearState.Clear)
            return;

        
        var collectionData = CHMData.Instance.GetCollectionData(_block.GetBlockState().ToString());
        collectionData.value += 1;
    }

    public async Task AfterDrag(Block block1, Block block2, bool isBoom = false)
    {
        if (moveCount.Value == 0)
            return;

        isLock = true;

        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);

        if (block1 && block2 && block1.IsBlock() == true && block2.IsBlock() == true)
        {
            moveIndex1 = block1.index;
            moveIndex2 = block2.index;

            // 두 블럭 모두 스페셜 블럭일 경우
            if (block1.IsSpecialBlock() == true && block2.IsSpecialBlock() == true)
            {
                await BoomAll();
                return;
            }
            // 한 블럭만 스페셜 블럭 중 특별한 경우
            else if (block1.GetBlockState() == Defines.EBlockState.PinkBomb)
            {
                await Boom3(block1, block2.GetBlockState());
                return;
            }
            // 한 블럭만 스페셜 블럭 중 특별한 경우
            else if (block2.GetBlockState() == Defines.EBlockState.PinkBomb)
            {
                await Boom3(block2, block1.GetBlockState());
                return;
            }
            // 한 블럭만 스페셜 블럭일 경우
            else if (block1.IsSpecialBlock() == true)
            {
                await block1.Boom();
                return;
            }
            // 한 블럭만 스페셜 블럭일 경우
            else if (block2.IsSpecialBlock() == true)
            {
                await block2.Boom();
                return;
            }
            // 두 블럭 모두 폭탄 블럭일 경우
            else if (block1.IsBoomBlock() == true && block2.IsBoomBlock() == true)
            {
                bonusScore.Value += 30;
                block2.match = true;

                var random = (Defines.EBlockState)UnityEngine.Random.Range((int)Defines.EBlockState.CatPang, (int)Defines.EBlockState.BlueBomb + 1);
                block1.changeBlockState = random;
            }
            // 한 블럭만 폭탄 블럭일 경우
            else if (block1.IsBoomBlock() == true)
            {
                await block1.Boom();
                return;
            }
            // 한 블럭만 폭탄 블럭일 경우
            else if (block2.IsBoomBlock() == true)
            {
                await block2.Boom();
                return;
            }
        }

        bool back = false;

        isMatch = false;
        CheckMap();

        if (block1 != null && block2 != null)
        {
            if (isMatch == false)
            {
                ChangeBlock(block1, block2);
                block1.rectTransform.DOAnchorPos(block1.originPos, delay);
                block2.rectTransform.DOAnchorPos(block2.originPos, delay);

                await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
                back = true;
            }
        }

        do
        {
            await RemoveMatchBlock();
            await CreateBoomBlock();
            await DownBlock();

            totScore.Value += oneTimeScore.Value;
            totScore.Value += bonusScore.Value;
            curScore.Value += oneTimeScore.Value;
            curScore.Value += bonusScore.Value;
            oneTimeScore.Value = 0;
            bonusScore.Value = 0;

            CheckDissapearBlock();

            await UpdateMap();
            CheckMap();
        } while (isMatch == true);

        if ((block1 != null && block2 != null && back == false) || isBoom == true)
        {
            moveCount.Value -= 1;
        }

        totScore.Value += oneTimeScore.Value;
        totScore.Value += bonusScore.Value;
        curScore.Value += oneTimeScore.Value;
        curScore.Value += bonusScore.Value;
        oneTimeScore.Value = 0;
        bonusScore.Value = 0;

        if (addDefense == true)
        {
            if (totScore.Value > 0 && gameResult.Value == EGameResult.None && selectTog.isOn)
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

        isLock = false;
    }

    async Task CreateMap(int _stage)
    {
        foreach (var block in boardArr)
        {
            if (block == null) continue;
            float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
            block.originPos.x -= moveDis;
            block.SetOriginPos();

            block.rectTransform.DOScale(1f, delay);

            var stageBlockInfo = stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
            if (stageBlockInfo == null)
            {
                var random = (Defines.EBlockState)UnityEngine.Random.Range(0, stageInfo.blockTypeCount);
                random = block.CheckSelectCatShop(random);
                block.SetBlockState(Defines.ELog.CreateMap, 1, blockSpriteList[(int)random], random);
                block.SetHp(-1);
            }
            else
            {
                var blockState = block.CheckSelectCatShop(stageBlockInfo.blockState);
                block.SetBlockState(Defines.ELog.CreateMap, 2, blockSpriteList[(int)blockState], blockState);

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

        isMatch = false;
        CheckMap();

        do
        {
            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    var block = boardArr[i, j];

                    if (block == null)
                        continue;

                    if (block.squareMatch == true || block.IsMatch() == true)
                    {
                        var random = (Defines.EBlockState)UnityEngine.Random.Range(0, stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(Defines.ELog.CreateMap, 1, blockSpriteList[(int)random], random);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.match = false;
                        block.squareMatch = false;
                    }
                }
            }

            isMatch = false;
            CheckMap();

        } while (isMatch == true);

        Debug.Log("Create Map End");
        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
    }

    async Task UpdateMap()
    // 맵생성
    {
        try
        {
            bool reupdate = false;
            bool createDelay = false;
            do
            {
                foreach (var block in boardArr)
                {
                    if (block == null)
                        continue;

                    if (block.changeBlockState != Defines.EBlockState.None)
                    {
                        createDelay = true;
                        CreateNewBlock(block, Defines.ELog.UpdateMap, 1, block.changeBlockState);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.SetOriginPos();
                        block.changeBlockState = Defines.EBlockState.None;
                    }
                    else if (reupdate == true || block.IsMatch() == true)
                    {
                        if (block.IsFixdBlock() == true || block.DisappearBlock() == true)
                            continue;

                        var random = UnityEngine.Random.Range(0, stageInfo.blockTypeCount);

                        createDelay = true;
                        CreateNewBlock(block, Defines.ELog.UpdateMap, 2, (Defines.EBlockState)random);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.SetOriginPos();
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

                if (createDelay == true)
                {
                    await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
                }

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

                if (curBlock.IsBoomBlock() == true || curBlock.IsSpecialBlock() == true)
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
        if (_test == false)
        {
            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    boardArr[i, j].ResetCheckWallDamage();
                }
            }
        }

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
    // 스퀘어 Match 좌측 상단부터 확인
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

    async Task RemoveMatchBlock()
    // Match된 블럭 제거
    {
        bool removeDelay = false;
        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                if (block.IsSpecialBlock() == true || block.DisappearBlock() == true)
                    continue;

                // 없어져야 할 블럭
                if (block.IsMatch() == true && block.remove == false)
                {
                    // 주변에 hp가 있는 블럭은 데미지 줌
                    CheckArround(block.row, block.col);

                    oneTimeScore.Value += 1;

                    removeDelay = true;
                    block.remove = true;
                    block.rectTransform.DOScale(0f, delay);

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

                    // 아이템이 연달아 터지는 경우
                    if (block.IsBoomBlock() == true && block.boom == false)
                    {
                        bonusScore.Value += 20;
                        await block.Boom(false);

                        // Match 검사를 다시 해야함
                        i = -1;
                        break;
                    }
                }
            }
        }

        if (removeDelay)
        {
            CHMMain.Sound.Play(Defines.ESound.Cat);
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
        }
    }

    async Task CreateBoomBlock(bool boomBlock = true)
    // 폭탄 블럭 생성
    {
        bool createDelay = false;

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                int row = i;
                int col = j;

                // 스퀘어 매치 블럭 생성
                if (block.squareMatch == true)
                {
                    createDelay = true;
                    
                    CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 1, block.GetPangType());
                    block.ResetScore();
                    block.SetOriginPos();
                }

                // 십자가 매치 블럭 생성
                if (block.hScore >= 3 && block.vScore >= 3)
                {
                    createDelay = true;

                    if (block.hScore >= 4 || block.vScore >= 4)
                    {
                        createDelay = true;
                        CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 2, Defines.EBlockState.Arrow5);
                    }
                    else
                    {
                        createDelay = true;
                        CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 3, Defines.EBlockState.Arrow6);
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
            }
        }

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                // 가로 3개 초과 매치 시 특수 블럭 생성
                if (block.hScore > 3)
                {
                    createDelay = true;

                    bool checkMoveBlock = false;

                    int tempScore = block.hScore;
                    for (int idx = 0; idx < tempScore; ++idx)
                    {
                        int tempRow = i;
                        int tempCol = j + idx;
                        if (IsValidIndex(tempRow, tempCol) == false || boardArr[tempRow, tempCol] == null)
                            continue;

                        var tempBlock = boardArr[tempRow, tempCol];
                        if (tempBlock.index == moveIndex1 ||
                            tempBlock.index == moveIndex2)
                        {
                            checkMoveBlock = true;
                            if (tempScore >= 5)
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 4, Defines.EBlockState.Arrow1);
                            }
                            else
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 5, Defines.EBlockState.Arrow4);
                            }
                        }

                        boardArr[tempRow, tempCol].ResetScore();
                    }

                    if (checkMoveBlock == false)
                    {
                        if (tempScore >= 5)
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 6, Defines.EBlockState.Arrow1);
                        }
                        else
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 7, Defines.EBlockState.Arrow4);
                        }
                    }
                }
                // 세로 3개 초과 매치 시 특수 블럭 생성
                else if (block.vScore > 3)
                {
                    createDelay = true;

                    bool checkMoveBlock = false;

                    int tempScore = block.vScore;
                    for (int idx = 0; idx < tempScore; ++idx)
                    {
                        int tempRow = i + idx;
                        int tempCol = j;

                        if (IsValidIndex(tempRow, tempCol) == false || boardArr[tempRow, tempCol] == null)
                            continue;

                        var tempBlock = boardArr[tempRow, tempCol];
                        if (tempBlock.index == moveIndex1 ||
                            tempBlock.index == moveIndex2)
                        {
                            checkMoveBlock = true;
                            if (tempScore >= 5)
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 8, Defines.EBlockState.Arrow2);
                            }
                            else
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 9, Defines.EBlockState.Arrow3);
                            }
                        }

                        boardArr[tempRow, tempCol].ResetScore();
                    }

                    if (checkMoveBlock == false)
                    {
                        if (tempScore >= 5)
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 10, Defines.EBlockState.Arrow2);
                        }
                        else
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 11, Defines.EBlockState.Arrow3);
                        }
                    }
                }
            }
        }

        if (createDelay)
        {
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
        }
    }

    void CreateNewBlock(Block _block, Defines.ELog _log, int _key, Defines.EBlockState _blockState, bool isDelay = true)
    {
        _blockState = _block.CheckSelectCatShop(_blockState);
        _block.SetBlockState(_log, _key, blockSpriteList[(int)_blockState], _blockState);
        _block.match = false;
        _block.boom = false;
        _block.squareMatch = false;
        _block.remove = false;
        if (isDelay == true)
            _block.rectTransform.DOScale(1f, delay);
        else
            _block.rectTransform.localScale = Vector3.one;
    }

    void CreateRandomBlock(Block _block, Defines.ELog _log, int _key, int _maxIndex, bool isDelay = true)
    {
        var random = UnityEngine.Random.Range(0, _maxIndex);

        CreateNewBlock(_block, _log, _key, (Defines.EBlockState)random, isDelay);
    }

    void Check3Match(List<Block> blockList, Defines.EDirection direction, bool test = false)
    // 3Match 블럭 확인
    {
        Defines.EBlockState blockState = Defines.EBlockState.None;
        List<int> tempIndex = new List<int>();
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            if (blockList[i].IsFixdBlock() == true || blockList[i].IsBoomBlock() == true || blockList[i].IsSpecialBlock() == true)
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
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
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
            boardArr[row, col].IsFixdBlock() == true || boardArr[row, col].IsBoomBlock() == true ||
            boardArr[row, col].IsSpecialBlock() == true)
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

        if (boardArr[row, col].DisappearBlock() == false &&
            boardArr[row, col].IsFixdBlock() == false &&
            boardArr[row, col].match == false)
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
                boardArr[row, col].Damage(stageInfo.blockTypeCount);
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
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom1(Block block, bool ani = true)
    // 자기 주변 1칸 폭탄
    {
        bonusScore.Value += 10;
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

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
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

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
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

                    var blueHoleObj = CHMMain.Resource.Instantiate(bombEffectRectTransform.gameObject, transform.parent);
                    blueHoleObj.SetActive(true);
                    var rt = blueHoleObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = _specialBlock.rectTransform.anchoredPosition;
                    rt.DOAnchorPos(block.rectTransform.anchoredPosition, .05f);
                    blueHoleList.Add(blueHoleObj);

                    await Task.Delay(200, tokenSource.Token);
                }
            }
        }

        SaveBoomCollectionData(_specialBlock);

        if (_ani)
        {
            AfterDrag(null, null, true);
        }

        await Task.Delay(1000, tokenSource.Token);

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

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
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

        SaveBoomCollectionData(block);

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

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
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

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
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

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom9(Block block, bool ani = true)
    // 마름모 폭탄
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        changeMatchState(block.row - 2, block.col);
        changeMatchState(block.row - 1, block.col);
        changeMatchState(block.row - 1, block.col - 1);
        changeMatchState(block.row - 1, block.col + 1);
        changeMatchState(block.row, block.col - 2);
        changeMatchState(block.row, block.col - 1);
        changeMatchState(block.row, block.col + 1);
        changeMatchState(block.row, block.col + 2);
        changeMatchState(block.row + 1, block.col - 1);
        changeMatchState(block.row + 1, block.col + 1);
        changeMatchState(block.row + 1, block.col);
        changeMatchState(block.row + 2, block.col);

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom10(Block block, bool ani = true)
    // 자기 주변 1칸 띄운 사각형 폭탄
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        changeMatchState(block.row - 2, block.col - 2);
        changeMatchState(block.row - 2, block.col - 1);
        changeMatchState(block.row - 2, block.col);
        changeMatchState(block.row - 2, block.col + 1);
        changeMatchState(block.row - 2, block.col + 2);
        changeMatchState(block.row - 1, block.col - 2);
        changeMatchState(block.row - 1, block.col + 2);
        changeMatchState(block.row, block.col - 2);
        changeMatchState(block.row, block.col + 2);
        changeMatchState(block.row + 1, block.col - 2);
        changeMatchState(block.row + 1, block.col + 2);
        changeMatchState(block.row + 2, block.col - 2);
        changeMatchState(block.row + 2, block.col - 1);
        changeMatchState(block.row + 2, block.col);
        changeMatchState(block.row + 2, block.col + 1);
        changeMatchState(block.row + 2, block.col + 2);

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom11(Block block, bool ani = true)
    // 빠직 모양 폭탄
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        changeMatchState(block.row - 2, block.col - 1);
        changeMatchState(block.row - 1, block.col - 2);
        changeMatchState(block.row - 1, block.col - 1);

        changeMatchState(block.row - 2, block.col + 1);
        changeMatchState(block.row - 1, block.col + 2);
        changeMatchState(block.row - 1, block.col + 1);

        changeMatchState(block.row + 2, block.col - 1);
        changeMatchState(block.row + 1, block.col - 2);
        changeMatchState(block.row + 1, block.col - 1);

        changeMatchState(block.row + 2, block.col + 1);
        changeMatchState(block.row + 1, block.col + 2);
        changeMatchState(block.row + 1, block.col + 1);

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom12(Block block, bool ani = true)
    // Z 모양 폭탄
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        changeMatchState(block.row - 2, block.col - 2);
        changeMatchState(block.row - 2, block.col - 1);
        changeMatchState(block.row - 2, block.col);
        changeMatchState(block.row - 2, block.col + 1);
        changeMatchState(block.row - 2, block.col + 2);
        changeMatchState(block.row - 1, block.col + 1);
        changeMatchState(block.row + 1, block.col - 1);
        changeMatchState(block.row + 2, block.col - 2);
        changeMatchState(block.row + 2, block.col - 1);
        changeMatchState(block.row + 2, block.col);
        changeMatchState(block.row + 2, block.col + 1);
        changeMatchState(block.row + 2, block.col + 2);

        SaveBoomCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }
}