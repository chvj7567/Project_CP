using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Jobs;

public class Game : MonoBehaviour
{
    const int MAX = 9;

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

    [SerializeField] Spawner spawner;
    [SerializeField] public float delay;

    [ReadOnly] Block[,] boardArr = new Block[MAX, MAX];
    [ReadOnly] public bool isDrag = false;
    [ReadOnly] public bool isAni = false;
    [ReadOnly] bool isMatch = false;

    [SerializeField, ReadOnly] float time;
    [SerializeField, ReadOnly] int canMatchRow = -1;
    [SerializeField, ReadOnly] int canMatchCol = -1;

    [SerializeField, ReadOnly] int moveIndex1 = 0;
    [SerializeField, ReadOnly] int moveIndex2 = 0;

    [SerializeField] int maxPower = 99999;
    [SerializeField] float minDelay = .1f;
    [SerializeField] float maxSpeed = 30f;
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

    List<Sprite> normalBlockSpriteList = new List<Sprite>();
    List<Sprite> specialBlockSpriteList = new List<Sprite>();
    public List<Sprite> wallBlockSpriteList = new List<Sprite>();

    bool oneTimeAlarm = false;

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

        for (int i = 0; i < (int)Defines.EWallBlockType.Max; ++i)
        {
            CHMMain.Resource.LoadSprite((Defines.EWallBlockType)i, (sprite) =>
            {
                if (sprite != null)
                    wallBlockSpriteList.Add(sprite);
            });
        }

        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMMain.Pool.Clear();
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
            if(_ == true)
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

        boardSize = PlayerPrefs.GetInt("size");

        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        var stage = PlayerPrefs.GetInt("stage");
        if (stage <= 0)
        {
            stage = 1;
        }

        CreateMap(stage);

        await AfterDrag(null, null);

        CHMMain.Sound.Play(Defines.ESound.Bgm);
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

        if (isAni == true)
        {
            time = Time.time;
        }
        else
        {
            // 5초 동안 드래그를 안하면 알려줌
            if (time + 5 < Time.time && oneTimeAlarm == false && canMatchRow >= 0 && canMatchCol >= 0)
            {
                oneTimeAlarm = true;

                boardArr[canMatchRow, canMatchCol].transform.DOScale(1.5f, 0.25f).OnComplete(() =>
                {
                    boardArr[canMatchRow, canMatchCol].transform.DOScale(1f, 0.25f);
                });

                await Task.Delay(3000);
                oneTimeAlarm = false;
            }
        }
    }

    public async Task AfterDrag(Block block1, Block block2)
    {
        isAni = true;
        if (block1 && block2)
        {
            moveIndex1 = block1.index;
            moveIndex2 = block2.index;

            if ((block1.GetSpecailType() == Defines.ESpecailBlockType.CatPang4 &&
                block2.GetSpecailType() == Defines.ESpecailBlockType.CatPang5) ||
                (block1.GetSpecailType() == Defines.ESpecailBlockType.CatPang5 &&
                block2.GetSpecailType() == Defines.ESpecailBlockType.CatPang4))
            {
                await Boom2(block1);
                return;
            }

            if (block1.GetSpecailType() != Defines.ESpecailBlockType.None &&
                block2.GetNormalType() != Defines.ENormalBlockType.None)
            {
                await Boom3(block1, block2.GetNormalType());
                return;
            }

            if (block2.GetSpecailType() != Defines.ESpecailBlockType.None &&
                block1.GetNormalType() != Defines.ENormalBlockType.None)
            {
                await Boom3(block2, block1.GetNormalType());
                return;
            }
        }

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

                    await Task.Delay((int)(delay * 1000));
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
        curScore.Value += oneTimeScore.Value;
        curScore.Value += bonusScore.Value;
        oneTimeScore.Value = 0;
        bonusScore.Value = 0;

        await Task.Delay((int)(delay * 1000));

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
                    catPangImgList = specialBlockSpriteList
                });
            }
        }

        isAni = false;
    }

    void CreateMap(int _stage)
    {
        switch (_stage)
        {
            case 1:
                {
                    foreach (var block in boardArr)
                    {
                        if (block == null) continue;

                        float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
                        block.originPos.x -= moveDis;
                        block.SetOriginPos();
                        block.rectTransform.DOScale(1f, delay);

                        var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);

                        block.SetNormalType((Defines.ENormalBlockType)random);
                        block.state = Defines.EState.Normal;
                        block.img.sprite = normalBlockSpriteList[random];
                    }
                }
                break;
            case 2:
                {
                    foreach (var block in boardArr)
                    {
                        if (block == null) continue;

                        float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
                        block.originPos.x -= moveDis;
                        block.SetOriginPos();
                        block.rectTransform.DOScale(1f, delay);


                        if (block.row != 4)
                        {
                            var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);

                            block.SetNormalType((Defines.ENormalBlockType)random);
                            block.state = Defines.EState.Normal;
                            block.img.sprite = normalBlockSpriteList[random];
                        }
                        else
                        {
                            block.SetNormalType(Defines.ENormalBlockType.None);
                            block.state = Defines.EState.Potal;
                            block.img.sprite = wallBlockSpriteList[(int)Defines.EWallBlockType.Wall3];
                            block.SetWallHp(3);
                        }
                    }
                }
                break;
            case 3:
                {
                    foreach (var block in boardArr)
                    {
                        if (block == null) continue;

                        float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
                        block.originPos.x -= moveDis;
                        block.SetOriginPos();
                        block.rectTransform.DOScale(1f, delay);


                        if (block.row != 4 || block.col % 2 == 0)
                        {
                            var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);

                            block.SetNormalType((Defines.ENormalBlockType)random);
                            block.state = Defines.EState.Normal;
                            block.img.sprite = normalBlockSpriteList[random];
                        }
                        else
                        {
                            block.SetNormalType(Defines.ENormalBlockType.None);
                            block.state = Defines.EState.Wall;
                            block.img.sprite = wallBlockSpriteList[(int)Defines.EWallBlockType.Wall0];
                            block.SetWallHp(-1);
                        }
                    }
                }
                break;
        }
    }

    async Task UpdateMap()
    // 맵생성
    {
        bool reupdate = false;

        do
        {
            foreach (var block in boardArr)
            {
                if (block == null) continue;

                if (reupdate == true || block.state == Defines.EState.Match)
                {
                    var random = Random.Range(0, (int)Defines.ENormalBlockType.Max);

                    block.SetNormalType((Defines.ENormalBlockType)random);
                    block.state = Defines.EState.Normal;
                    block.img.sprite = normalBlockSpriteList[random];

                    block.ResetScore();
                    block.SetOriginPos();
                    block.rectTransform.DOScale(1f, delay);
                }
            }

            await Task.Delay((int)(delay * 1000));

            reupdate = CanMatch() == false;

        } while (reupdate == true);

        isMatch = false;
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
                if (curBlock.GetSpecailType() != Defines.ESpecailBlockType.None)
                {
                    canMatchRow = curBlock.row;
                    canMatchCol = curBlock.col;
                    return true;
                }

                var upBlock = IsValidIndex(i - 1, j) == true ? boardArr[i - 1, j] : null;
                var downBlock = IsValidIndex(i + 1, j) == true ? boardArr[i + 1, j] : null;
                var leftBlock = IsValidIndex(i, j - 1) == true ? boardArr[i, j - 1] : null;
                var rightBlock = IsValidIndex(i, j + 1) == true ? boardArr[i, j + 1] : null;

                if (upBlock != null)
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

                if (downBlock != null)
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

                if (leftBlock != null)
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

                if (rightBlock != null)
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

            CheckMatch(hBlockList, Defines.EDirection.Horizontal, _test);
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

            CheckMatch(vBlockList, Defines.EDirection.Vertical, _test);
        }
    }

    async Task RemoveMatchBlock()
    // Match된 블럭 제거, 특수 블럭 생성
    {
        bool removeDelay = false;
        bool createBoomDelay = false;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                // 없어져야 할 블럭
                if (block.state == Defines.EState.Match)
                {
                    CheckArround(block.row, block.col);
                    var gold = CHMMain.Resource.Instantiate(goldImg.gameObject, transform.parent);
                    if (gold != null)
                    {
                        var rect = gold.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = block.rectTransform.anchoredPosition;
                            rect.DOAnchorPosY(rect.anchoredPosition.y + Random.Range(30f, 50f), .5f).OnComplete(() =>
                            {
                                CHMMain.Sound.Play(Defines.ESound.Gold);
                                rect.DOAnchorPos(goldImg.rectTransform.anchoredPosition, Random.Range(.2f, 1f)).OnComplete(() =>
                                {
                                    CHMMain.Resource.Destroy(gold);
                                });
                            });
                        }
                    }

                    oneTimeScore.Value += 1;

                    // 아이템이 연달아 터지는 경우
                    bool isSpecailType = false;

                    switch (block.GetSpecailType())
                    {
                        case Defines.ESpecailBlockType.CatPang1:
                            {
                                isSpecailType = true;
                                bonusScore.Value += 20;
                                await Boom1(block, false);
                                i = -1;
                            }
                            break;
                        case Defines.ESpecailBlockType.CatPang2:
                            {
                                isSpecailType = true;
                                bonusScore.Value += 20;
                                await Boom2(block, false);
                                i = -1;
                            }
                            break;
                        case Defines.ESpecailBlockType.CatPang3:
                            {
                                // 드래그 해야 터지는 폭탄
                            }
                            break;
                        case Defines.ESpecailBlockType.CatPang4:
                            {
                                isSpecailType = true;
                                bonusScore.Value += 20;
                                await Boom4(block, false);
                                i = -1;
                            }
                            break;
                        case Defines.ESpecailBlockType.CatPang5:
                            {
                                isSpecailType = true;
                                bonusScore.Value += 20;
                                await Boom5(block, false);
                                i = -1;
                            }
                            break;
                        case Defines.ESpecailBlockType.CatPang6:
                            {
                                isSpecailType = true;
                                bonusScore.Value += 20;
                                await Boom6(block, false);
                                i = -1;
                            }
                            break;
                    }

                    if (isSpecailType == true)
                        break;

                    removeDelay = true;

                    int row = i;
                    int col = j;

                    block.rectTransform.DOScale(0f, delay).OnComplete(() =>
                    {
                        // 블럭 제거 후 + 매치 특수 블럭 생성
                        if (block.hScore >= 3 && block.vScore >= 3)
                        {
                            createBoomDelay = true;
                            
                            if (block.hScore >= 4 || block.vScore >= 4)
                            {
                                block.SetSpecailType(Defines.ESpecailBlockType.CatPang2);
                                block.state = Defines.EState.Normal;
                                block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang2];
                                block.ResetScore();
                                block.SetOriginPos();
                                block.rectTransform.DOScale(1f, delay);
                            }
                            else
                            {
                                block.SetSpecailType(Defines.ESpecailBlockType.CatPang6);
                                block.state = Defines.EState.Normal;
                                block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang6];
                                block.ResetScore();
                                block.SetOriginPos();
                                block.rectTransform.DOScale(1f, delay);
                            }

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

                block.ResetCheckWallDamage();

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
                            if (IsValidIndex(tempRow, tempCol) == false || boardArr[tempRow, tempCol] == null)
                                continue;

                            if (boardArr[tempRow, tempCol].index == moveIndex1 ||
                                boardArr[tempRow, tempCol].index == moveIndex2)
                            {
                                checkMoveBlock = true;
                                if (tempScore >= 5)
                                {
                                    boardArr[tempRow, tempCol].SetSpecailType(Defines.ESpecailBlockType.CatPang4);
                                    boardArr[tempRow, tempCol].state = Defines.EState.Normal;
                                    boardArr[tempRow, tempCol].img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang4];
                                    boardArr[tempRow, tempCol].rectTransform.DOScale(1f, delay);
                                }
                                else
                                {
                                    boardArr[tempRow, tempCol].SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                                    boardArr[tempRow, tempCol].state = Defines.EState.Normal;
                                    boardArr[tempRow, tempCol].img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                                    boardArr[tempRow, tempCol].rectTransform.DOScale(1f, delay);
                                }
                            }

                            boardArr[tempRow, tempCol].ResetScore();
                        }

                        if (checkMoveBlock == false)
                        {
                            if (tempScore >= 5)
                            {
                                block.SetSpecailType(Defines.ESpecailBlockType.CatPang4);
                                block.state = Defines.EState.Normal;
                                block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang4];
                                block.rectTransform.DOScale(1f, delay);
                            }
                            else
                            {
                                block.SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                                block.state = Defines.EState.Normal;
                                block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                                block.rectTransform.DOScale(1f, delay);
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
                                    boardArr[tempRow, tempCol].SetSpecailType(Defines.ESpecailBlockType.CatPang5);
                                    boardArr[tempRow, tempCol].state = Defines.EState.Normal;
                                    boardArr[tempRow, tempCol].img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang5];
                                    boardArr[tempRow, tempCol].rectTransform.DOScale(1f, delay);
                                }
                                else
                                {
                                    boardArr[tempRow, tempCol].SetSpecailType(Defines.ESpecailBlockType.CatPang1);
                                    boardArr[tempRow, tempCol].state = Defines.EState.Normal;
                                    boardArr[tempRow, tempCol].img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang1];
                                    boardArr[tempRow, tempCol].rectTransform.DOScale(1f, delay);
                                }
                            }

                            boardArr[tempRow, tempCol].ResetScore();
                        }

                        if (checkMoveBlock == false)
                        {
                            if (tempScore >= 5)
                            {
                                block.SetSpecailType(Defines.ESpecailBlockType.CatPang5);
                                block.state = Defines.EState.Normal;
                                block.img.sprite = specialBlockSpriteList[(int)Defines.ESpecailBlockType.CatPang5];
                                block.rectTransform.DOScale(1f, delay);
                            }
                            else
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
        }

        if (createBoomDelay)
        {
            await Task.Delay((int)(delay * 1000));
        }
    }

    void CheckMatch(List<Block> blockList, Defines.EDirection direction, bool test = false)
    {
        Defines.ENormalBlockType matchType = Defines.ENormalBlockType.None;
        List<int> tempIndex = new List<int>();
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            if (blockList[i].state == Defines.EState.Potal)
            {
                matchType = Defines.ENormalBlockType.None;
                matchCount = 0;
                continue;
            }

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
                    if (test == false)
                    {
                        int temp = i;
                        for (int j = 0; j < matchCount; ++j)
                        {
                            var block = blockList[temp--];
                            block.SetScore(matchCount, direction);
                            block.state = Defines.EState.Match;
                            
                        }
                    }

                    isMatch = true;
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
    // 3Match 블럭 확인

    async Task DownBlock()
    // 블럭 아래로 이동
    {
        List<Block> order = new List<Block>();
        for (int i = boardSize - 1; i >= 0; --i)
        {
            for (int j = boardSize - 1; j >= 0; --j)
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
            if (boardArr[row, col].state != Defines.EState.Normal)
                continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;

            int wallRow = -1;
            for (int i = boardSize - 1; i > row; --i)
            {
                if (boardArr[i, col].state == Defines.EState.Wall)
                {
                    wallRow = i;
                }
            }

            if (wallRow == -1)
            {
                for (int i = boardSize - 1; i > row; --i)
                {
                    if (boardArr[i, col].state == Defines.EState.Match)
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
                    if (boardArr[i, col].state == Defines.EState.Match)
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

    bool changeMatchState(int row, int col)
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return false;

        if (boardArr[row, col].state != Defines.EState.Potal && boardArr[row, col].state != Defines.EState.Wall)
        {
            boardArr[row, col].state = Defines.EState.Match;
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
            if (boardArr[row, col].state == Defines.EState.Potal)
            {
                boardArr[row, col].DamagePotal();
            }
            else if (boardArr[row, col].state == Defines.EState.Wall)
            {
                boardArr[row, col].DamageWall();
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
        block.SetNormalType(Defines.ENormalBlockType.Max);
        block.state = Defines.EState.Match;

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
        block.SetNormalType(Defines.ENormalBlockType.Max);
        block.state = Defines.EState.Match;

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

    public async Task Boom3(Block specialBlock, Defines.ENormalBlockType normalBlockType, bool ani = true)
    // 같은 블럭 폭탄
    {
        bonusScore.Value += 5;
        specialBlock.SetNormalType(Defines.ENormalBlockType.Max);
        specialBlock.state = Defines.EState.Match;

        List<GameObject> blueHoleList = new List<GameObject>();

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                if (block.GetNormalType() == normalBlockType)
                {
                    block.state = Defines.EState.Match;

                    var blueHoleObj = CHMMain.Resource.Instantiate(darkBall.gameObject, transform.parent);
                    blueHoleObj.SetActive(true);
                    var rt = blueHoleObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = specialBlock.rectTransform.anchoredPosition;
                    rt.DOAnchorPos(block.rectTransform.anchoredPosition, .2f);
                    blueHoleList.Add(blueHoleObj);

                    await Task.Delay(200);
                }
            }
        }

        if (ani)
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
        block.SetNormalType(Defines.ENormalBlockType.Max);

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
        block.SetNormalType(Defines.ENormalBlockType.Max);

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
        block.SetNormalType(Defines.ENormalBlockType.None);

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
        block.SetNormalType(Defines.ENormalBlockType.Max);

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
        block.SetNormalType(Defines.ENormalBlockType.Max);

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
