using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class Game : MonoBehaviour
{
    const int MAX = 9;

    [SerializeField] Image timerImg;
    [SerializeField] CHTMPro timerText;
    [SerializeField, ReadOnly] float curTimer;
    [SerializeField] Image goldImg;
    [SerializeField] Image viewImg1;
    [SerializeField] Image viewImg2;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject origin;
    [SerializeField] float margin = 0f;
    [SerializeField, Range(1, MAX)] int boardSize = 1;
    [SerializeField] Transform parent;
    [SerializeField] ParticleSystem bombEffectPS;
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField] List<ParticleSystem> pangEffectList = new List<ParticleSystem>();
    [SerializeField, ReadOnly] int backgroundIndex = 0;

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

    [SerializeField] CHTMPro targetScoreText;
    [SerializeField] CHTMPro moveCountText;
    [SerializeField] CHTMPro curScoreText;
    [SerializeField] CHTMPro bonusScoreText;

    [SerializeField] GameObject onlyNormalStageObject;
    [SerializeField] GameObject onlyBossStageObject;
    [SerializeField] Image bossHpImage;
    [SerializeField] CHTMPro bossHpText;
    [SerializeField] CHTMPro hpText;
    [SerializeField] int selectScore;
    [SerializeField] int selectCurScore;

    [SerializeField, ReadOnly] ReactiveProperty<int> hp = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> curScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> bonusScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> moveCount = new ReactiveProperty<int>();

    [SerializeField] CHButton arrowPang1;
    [SerializeField] CHButton arrowPang2;
    [SerializeField] Image banView;
    [SerializeField, ReadOnly] public ReactiveProperty<EGameState> gameResult = new ReactiveProperty<EGameState>();
    [SerializeField, ReadOnly] int arrowPangIndex = 1;

    
    [SerializeField] RectTransform guideHole;
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button guideBackgroundBtn;
    [SerializeField] List<RectTransform> normalStageGuideHoleList = new List<RectTransform>();
    [SerializeField] List<RectTransform> bossStageGuideHoleList = new List<RectTransform>();
    [SerializeField] CHTMPro guideDesc;

    [SerializeField] int delayMillisecond;

    public bool tutorialNextBlock = false;
    List<Sprite> blockSpriteList = new List<Sprite>();
    public Infomation.StageInfo stageInfo;
    public List<Infomation.StageBlockInfo> stageBlockInfoList = new List<Infomation.StageBlockInfo>();

    bool oneTimeAlarm = false;
    public bool gameEnd = false;
    
    int helpTime = 0;

    Defines.ESelectStage selectStage = Defines.ESelectStage.Normal;
    Data.Login loginData;

    CancellationTokenSource tokenSource;

    bool init = false;

    async void Start()
    {
        bonusScoreText.gameObject.SetActive(false);

        guideBackground.SetActive(false);
        guideHole.gameObject.SetActive(false);

        onlyNormalStageObject.SetActive(true);
        onlyBossStageObject.SetActive(false);

        for (int i = 0; i < normalStageGuideHoleList.Count; ++i)
        {
            normalStageGuideHoleList[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < bossStageGuideHoleList.Count; ++i)
        {
            bossStageGuideHoleList[i].gameObject.SetActive(false);
        }

        guideBackgroundBtn.gameObject.SetActive(false);

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
                PlayerPrefs.SetInt(CHMMain.String.Background, backgroundIndex);

                SceneManager.LoadScene(1);
            });
        }

        if (arrowPang1 && arrowPang2)
        {
            arrowPang1.button.OnClickAsObservable().Subscribe(_ =>
            {
                arrowPangIndex = 1;
                banView.rectTransform.DOAnchorPosX(arrowPang2.rectTransform.anchoredPosition.x, .5f);
            });

            arrowPang2.button.OnClickAsObservable().Subscribe(_ =>
            {
                arrowPangIndex = 2;
                banView.rectTransform.DOAnchorPosX(arrowPang1.rectTransform.anchoredPosition.x, .5f);
            });
        }

        curScore.Subscribe(_ =>
        {
            curScoreText.SetText(_);
        });

        moveCount.Subscribe(_ =>
        {
            moveCountText.SetText(_);
        });

        gameResult.Subscribe(_ =>
        {
            if (_ == EGameState.GameOver || _ == EGameState.GameClear)
            {
                if (tokenSource != null && !tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
            }
        });

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (gameResult.Value == EGameState.GameClearWait)
                {
                    GameEnd(true);
                    return;
                }
                else if (gameResult.Value == EGameState.GameOverWait)
                {
                    GameEnd(false);
                    return;
                }

                if (gameResult.Value == EGameState.EasyOrNormalStagePlay)
                {
                    bool clear = true;

                    bool useTime = stageInfo.time > 0;
                    bool useTargetScore = stageInfo.targetScore > 0;
                    bool useMoveCount = stageInfo.moveCount > 0;

                    // ü���� �ִ� ���� �ְų� �������� �Ǵ� ���� ������ ��Ŭ����
                    for (int i = 0; i < boardSize; ++i)
                    {
                        for (int j = 0; j < boardSize; ++j)
                        {
                            if (boardArr[i, j].GetHp() > 0 || boardArr[i, j].IsBottomTouchDisappearBlock() == true)
                            {
                                clear = false;
                                break;
                            }
                        }

                        if (clear == false)
                            break;
                    }

                    // �ð������� �ְ� �ð��� �� �� ��� ���� ���� Ȯ��
                    if (useTime == true && timerImg.fillAmount >= 1)
                    {
                        // ��ǥ ������ ����ϴ� ��� ��ǥ ���� �޼� Ȯ��
                        if (useTargetScore == true && curScore.Value < stageInfo.targetScore)
                        {
                            clear = false;
                        }

                        GameEnd(clear);
                    }
                    // �ð��� ����ϰ� �ð��� �� ���� ���� ��� ���� ���� Ȯ��
                    else
                    {
                        // ��ǥ ������ ����ϴ� ��� ��ǥ ���� �޼� Ȯ��
                        if (useTargetScore == true && curScore.Value < stageInfo.targetScore)
                        {
                            clear = false;
                        }

                        // ������ Ƚ���� ����ϴ� ��� Ƚ���� �� �������� �� ���� ���� Ȯ��
                        if (useMoveCount == true && moveCount.Value <= 0)
                        {
                            GameEnd(clear);
                        }
                        else
                        {
                            // Ŭ���� �� ��츸 ���� ����
                            if (clear == true)
                            {
                                GameEnd(clear);
                            }
                        }
                    }
                }
                else if (gameResult.Value == EGameState.BossStagePlay)
                {
                    if (hp.Value <= 0)
                    {
                        if (bossHpImage.fillAmount <= 0)
                        {
                            GameEnd(true);
                            return;
                        }

                        GameEnd(false);
                        return;
                    }

                    if (bossHpImage.fillAmount <= 0)
                    {
                        GameEnd(true);
                        return;
                    }
                }
            });

        await LoadImage();
        InitData();
        await CreateMap();
        await StartGuide();
        StartTutorial();
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

            if (curTimer >= helpTime)
            {
                if (stageInfo.time >= helpTime)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.SetText(stageInfo.time - helpTime);
                    ++helpTime;
                }
                else
                {
                    timerText.gameObject.SetActive(false);
                }
            }
        }

        if (isLock == true)
        {
            teachTime = Time.time;
        }
        else
        {
            // 3�� ���� �巡�׸� ���ϸ� �˷���
            if (teachTime + 3 < Time.time && oneTimeAlarm == false && canMatchRow >= 0 && canMatchCol >= 0)
            {
                oneTimeAlarm = true;

                try
                {
                    var block = boardArr[canMatchRow, canMatchCol];
                    block.transform.DOScale(1.5f, 0.25f).OnComplete(() =>
                    {
                        block.transform.DOScale(1f, 0.25f);
                    });
                    await Task.Delay(3000, tokenSource.Token);
                } catch (TaskCanceledException) {}
                
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
        CHMData.Instance.SaveData(CHMMain.String.CatPang);
    }

    async Task LoadImage()
    // �� �̹��� �ε�
    {
        for (EBlockState i = 0; i < EBlockState.Max; ++i)
        {
            TaskCompletionSource<Sprite> imageTask = new TaskCompletionSource<Sprite>();
            CHMMain.Resource.LoadSprite(i, (sprite) =>
            {
                if (sprite != null)
                    blockSpriteList.Add(sprite);

                imageTask.SetResult(sprite);
            });

            await imageTask.Task;
        }
    }

    void InitData()
    // ������ �ʱ�ȭ
    {
        if (init)
            return;

        init = true;

        tokenSource = new CancellationTokenSource();
        backgroundIndex = PlayerPrefs.GetInt(CHMMain.String.Background);
        loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);

        ChangeBackgroundLoop();

        var stage = 0;
        switch (selectStage)
        {
            case ESelectStage.Normal:
                {
                    stage = PlayerPrefs.GetInt(CHMMain.String.Stage);
                }
                break;
            case ESelectStage.Boss:
                {
                    stage = PlayerPrefs.GetInt(CHMMain.String.BossStage);
                }
                break;
            case ESelectStage.Easy:
                {
                    stage = PlayerPrefs.GetInt(CHMMain.String.EasyStage);
                }
                break;
        }

        stageInfo = CHMMain.Json.GetStageInfo(stage);
        stageBlockInfoList = CHMMain.Json.GetStageBlockInfoList(stage);
        boardSize = stageInfo.boardSize;

        switch (selectStage)
        {
            case ESelectStage.Normal:
                {
                    // Ʃ�丮���� Easy(�Ϲ�)���� ���������� ����
                    stageInfo.tutorialID = -1;
                    for (int i = 0; i < stageBlockInfoList.Count; ++i)
                    {
                        stageBlockInfoList[i].tutorialBlock = false;
                    }
                }
                break;
            case ESelectStage.Boss:
                break;
            case ESelectStage.Easy:
                {
                    // ���̵� ����
                    if (stageInfo.time > 0)
                    {
                        stageInfo.time *= 2;
                    }
                    else if (stageInfo.targetScore > 0)
                    {
                        stageInfo.targetScore /= 2;
                    }
                    else if (stageInfo.moveCount > 0)
                    {
                        stageInfo.moveCount *= 2;
                    }
                }
                break;
        }

        targetScoreText.SetText(stageInfo.targetScore);
        if (stageInfo.targetScore < 0)
        {
            targetScoreText.gameObject.SetActive(false);
        }

        if (stageInfo.moveCount > 0)
        {
            moveCount.Value = stageInfo.moveCount + loginData.useMoveItemCount;
        }
        else
        {
            moveCount.Value = 9999;

            if (loginData.useMoveItemCount > 0)
                loginData.addMoveItemCount += loginData.useMoveItemCount;
        }

        if (stageInfo.time > 0)
        {
            stageInfo.time += loginData.useTimeItemCount * 10;
        }
        else
        {
            if (loginData.useTimeItemCount > 0)
                loginData.addTimeItemCount += loginData.useTimeItemCount;
        }

        if (selectStage == Defines.ESelectStage.Boss)
        {
            gameResult.Value = EGameState.BossStagePlay;
        }
        else
        {
            gameResult.Value = EGameState.EasyOrNormalStagePlay;
        }

        // ���� ���������� ��츸
        if (selectStage == Defines.ESelectStage.Boss)
        {
            onlyBossStageObject.SetActive(true);
            onlyNormalStageObject.SetActive(false);

            hp.Subscribe(_ =>
            {
                if (_ >= 0)
                    hpText.SetText(hp);
            });

            hp.Value = loginData.hp;

            // 1�ʵڿ� 1�ʿ� �� ���� ����
            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                hp.Value -= 1;
            })
            .AddTo(this);

            curScore.Subscribe(_ =>
            {
                var fillAmountValue = (stageInfo.targetScore - _) / (float)stageInfo.targetScore;
                bossHpImage.DOFillAmount(fillAmountValue, .5f);
                bossHpText.SetText(Mathf.Max(0, stageInfo.targetScore - _));
            });
        }
    }

    async Task StartGuide()
    // ���̵� ����
    {
        if (selectStage == Defines.ESelectStage.Easy && loginData.guideIndex == 5)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();

            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            var guideIndex = await EasyStageGuideStart();
            loginData.guideIndex += guideIndex;

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }

        if (selectStage == Defines.ESelectStage.Boss && loginData.guideIndex == 11)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();

            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            var guideIndex = await BossStageGuideStart();
            loginData.guideIndex += guideIndex;

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }
    }

    void StartTutorial()
    // Ʃ�丮�� ����
    {
        if (selectStage != Defines.ESelectStage.Normal && stageInfo.tutorialID > 0)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideHole.gameObject.SetActive(true);

            guideHole.SetAsLastSibling();
            guideBackground.transform.SetAsLastSibling();

            var holeValue = GetTutorialStageImgSettingValue(boardArr, stageBlockInfoList);
            guideHole.sizeDelta = holeValue.Item1;
            guideHole.anchoredPosition = holeValue.Item2;

            var tutorialInfo = CHMMain.Json.GetTutorialInfo(stageInfo.tutorialID);
            if (tutorialInfo != null)
            {
                guideDesc.SetStringID(tutorialInfo.descStringID);
            }
        }
    }

    async Task<int> EasyStageGuideStart()
    // �Ϲ� �������� Game UI ���̵� ����
    {
        TaskCompletionSource<int> tutorialCompleteTask = new TaskCompletionSource<int>();

        guideBackground.SetActive(true);

        for (int i = 0; i < normalStageGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 6);
            if (guideInfo == null)
                break;

            normalStageGuideHoleList[i].gameObject.SetActive(true);
            guideDesc.SetStringID(guideInfo.descStringID);

            TaskCompletionSource<bool> buttonClicktask = new TaskCompletionSource<bool>();

            var btnComplete = guideBackgroundBtn.OnClickAsObservable().Subscribe(_ =>
            {
                buttonClicktask.SetResult(true);
            });

            await buttonClicktask.Task;

            normalStageGuideHoleList[i].gameObject.SetActive(false);

            btnComplete.Dispose();
        }

        tutorialCompleteTask.SetResult(normalStageGuideHoleList.Count);

        return await tutorialCompleteTask.Task;
    }

    async Task<int> BossStageGuideStart()
    // ���� �������� Game UI ���̵� ����
    {
        TaskCompletionSource<int> tutorialCompleteTask = new TaskCompletionSource<int>();

        guideBackground.SetActive(true);

        for (int i = 0; i < bossStageGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 12);
            if (guideInfo == null)
                break;

            bossStageGuideHoleList[i].gameObject.SetActive(true);
            guideDesc.SetStringID(guideInfo.descStringID);

            TaskCompletionSource<bool> buttonClicktask = new TaskCompletionSource<bool>();

            var btnComplete = guideBackgroundBtn.OnClickAsObservable().Subscribe(_ =>
            {
                buttonClicktask.SetResult(true);
            });

            await buttonClicktask.Task;

            bossStageGuideHoleList[i].gameObject.SetActive(false);

            btnComplete.Dispose();
        }

        tutorialCompleteTask.SetResult(bossStageGuideHoleList.Count);

        return await tutorialCompleteTask.Task;
    }

    (Vector2, Vector2) GetTutorialStageImgSettingValue(Block[,] blockArr, List<StageBlockInfo> stageBlockInfoList)
    // Ʃ�丮�󿡼� ��� ���̴� �κ� ũ��� ��ġ ����
    {
        if (null == stageBlockInfoList || null == blockArr)
            return (Vector2.zero, Vector2.zero);

        var tutorialBlockList = stageBlockInfoList.FindAll(_ => _.tutorialBlock);
        if (tutorialBlockList.Count <= 0)
            return (Vector2.zero, Vector2.zero);

        float sizeX = 0f, sizeY = 0f;
        float posX = 0f, posY = 0f;

        if (tutorialBlockList.Count == 1)
        {
            var tutorialBlock = blockArr[tutorialBlockList[0].row, tutorialBlockList[0].col];
            return (tutorialBlock.rectTransform.sizeDelta, tutorialBlock.rectTransform.anchoredPosition);
        }
        else if (tutorialBlockList.Count == 2)
        {
            var tutorialBlock1 = blockArr[tutorialBlockList[0].row, tutorialBlockList[0].col];
            var tutorialBlock2 = blockArr[tutorialBlockList[1].row, tutorialBlockList[1].col];

            if (tutorialBlock1.row == tutorialBlock2.row)
            {
                sizeX = tutorialBlock1.rectTransform.sizeDelta.x * 2;
                sizeY = tutorialBlock1.rectTransform.sizeDelta.y;
                posX = (tutorialBlock1.rectTransform.anchoredPosition.x + tutorialBlock2.rectTransform.anchoredPosition.x) / 2f;
                posY = tutorialBlock1.rectTransform.anchoredPosition.y;
            }
            else
            {
                sizeX = tutorialBlock1.rectTransform.sizeDelta.x;
                sizeY = tutorialBlock1.rectTransform.sizeDelta.y * 2;
                posX = tutorialBlock1.rectTransform.anchoredPosition.x;
                posY = (tutorialBlock1.rectTransform.anchoredPosition.y + tutorialBlock2.rectTransform.anchoredPosition.y) / 2f;
            }
        }

        return (new Vector2(sizeX, sizeY), new Vector2(posX, posY));
    }

    async void GameEnd(bool clear)
    // ���� ���� �� ����
    {
        if (isLock)
        {
            if (clear)
            {
                gameResult.Value = EGameState.GameClearWait;
            }
            else
            {
                gameResult.Value = EGameState.GameOverWait;
            }

            return;
        }

        if (gameEnd == false)
        {
            gameEnd = true;
        }
        else
        {
            return;
        }

        if (CheckCatPang())
        {
            gameResult.Value = EGameState.CatPang;

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 55,
                closeTime = 1
            });

            await Task.Delay(1000);
            await CatPang();

            // ���Ӱ�� �ٽ� üũ�ϵ���
            gameEnd = false;
            if (selectStage == Defines.ESelectStage.Boss)
            {
                gameResult.Value = EGameState.BossStagePlay;
            }
            else
            {
                gameResult.Value = EGameState.EasyOrNormalStagePlay;
            }

            return;
        }

        if (clear == false)
        {
            gameResult.Value = EGameState.GameOver;
        }
        else
        {
            gameResult.Value = EGameState.GameClear;
        }

        CHMMain.UI.ShowUI(EUI.UIGameEnd, new UIGameEndArg
        {
            clearState = GetClearState(),
            result = gameResult.Value,
            gold = curScore.Value
        });

        if (clear)
        {
            SaveClearData();
        }
    }

    bool CheckCatPang()
    // �������� ������ Ĺ���� �������� Ȯ��
    {
        for (int w = 0; w < boardSize; ++w)
        {
            for (int h = 0; h < boardSize; ++h)
            {
                if (boardArr[w, h].IsBombBlock())
                {
                    return true;
                }
            }
        }

        return false;
    }

    async Task CatPang()
    // �������� ������ Ĺ��!!!
    {
        isLock = true;

        for (int w = 0; w < boardSize; ++w)
        {
            for (int h = 0; h < boardSize; ++h)
            {
                if (boardArr[w, h].IsBombBlock())
                {
                    await boardArr[w, h].Boom();

                    w = -1; break;
                }
            }
        }

        isLock = false;
    }

    void SetDissapearBlock()
    // ��������� ���� ���� ���� ��ź���� ��ȯ
    {
        int row = boardSize - 1;

        for (int i = 0; i < boardSize; ++i)
        {
            var block = boardArr[row, i];
            if (block.IsBottomTouchDisappearBlock() == true)
            {
                var random = UnityEngine.Random.Range((int)Defines.EBlockState.PinkBomb, (int)Defines.EBlockState.BlueBomb + 1);
                block.changeBlockState = (Defines.EBlockState)random;

                block.tutorialBlock = false;
            }
        }
    }

    async Task ChangeBackgroundLoop()
    // ��� ��ȯ �ݺ�
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
    // ���� ��� �ε��� ��ȯ
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

    void SaveClearData()
    // ���� ���������� Ŭ���� ���·� ����
    {
        switch (selectStage)
        {
            case ESelectStage.Normal:
                CHMData.Instance.GetLoginData(CHMMain.String.CatPang).stage = PlayerPrefs.GetInt(CHMMain.String.Stage);
                break;
            case ESelectStage.Boss:
                CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage = PlayerPrefs.GetInt(CHMMain.String.BossStage);
                break;
            case ESelectStage.Easy:
                CHMData.Instance.GetLoginData(CHMMain.String.CatPang).easyStage = PlayerPrefs.GetInt(CHMMain.String.EasyStage);
                break;
        }
    }

    Defines.EClearState GetClearState()
    // ���� ���������� Ŭ���� ����
    {
        switch (selectStage)
        {
            case ESelectStage.Normal:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).stage >= PlayerPrefs.GetInt(CHMMain.String.Stage))
                {
                    return Defines.EClearState.Clear;
                }
                break;
            case ESelectStage.Boss:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage >= PlayerPrefs.GetInt(CHMMain.String.BossStage))
                {
                    return Defines.EClearState.Clear;
                }
                break;
            case ESelectStage.Easy:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).easyStage >= PlayerPrefs.GetInt(CHMMain.String.EasyStage))
                {
                    return Defines.EClearState.Clear;
                }
                break;
        }

        return Defines.EClearState.Doing;
    }

    void SaveBombCollectionData(Block block)
    // ��ź �ݷ��� ������ ����
    {
        if (block.IsBombBlock() == false && block.IsSpecialBombBlock() == false)
            return;

        if (GetClearState() == Defines.EClearState.Clear)
            return;

        
        var collectionData = CHMData.Instance.GetCollectionData(block.GetBlockState().ToString());
        collectionData.value += 1;
    }

    public async Task AfterDrag(Block block1, Block block2, bool isBoom = false)
    // ���� �巡���ϰ� �� �� ���� �巡�װ� ������ ���±���
    {
        if (moveCount.Value == 0 && gameResult.Value != EGameState.CatPang)
            return;

        Time.timeScale = 1;
        guideBackground.SetActive(false);
        guideHole.gameObject.SetActive(false);

        isLock = true;

        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);

        if (block1 && block2 && block1.IsBlock() == true && block2.IsBlock() == true)
        {
            if (block1.tutorialBlock)
                block1.tutorialBlock = false;
            if (block2.tutorialBlock)
                block2.tutorialBlock = false;

            moveIndex1 = block1.index;
            moveIndex2 = block2.index;

            // �� �� ��� ����� ���� ���
            if (block1.IsSpecialBombBlock() == true && block2.IsSpecialBombBlock() == true)
            {
                block1.match = true;
                block2.match = true;
                await BoomAll();
                return;
            }
            // �� ���� ����� �� �� Ư���� ���
            else if (block1.GetBlockState() == Defines.EBlockState.PinkBomb)
            {
                await Boom3(block1, block2.GetBlockState());
                return;
            }
            // �� ���� ����� �� �� Ư���� ���
            else if (block2.GetBlockState() == Defines.EBlockState.PinkBomb)
            {
                await Boom3(block2, block1.GetBlockState());
                return;
            }
            // �� ���� ����� ���� ���
            else if (block1.IsSpecialBombBlock() == true)
            {
                await block1.Boom();
                return;
            }
            // �� ���� ����� ���� ���
            else if (block2.IsSpecialBombBlock() == true)
            {
                await block2.Boom();
                return;
            }
            // �� �� ��� ��ź ���� ���
            else if (block1.IsBombBlock() == true && block2.IsBombBlock() == true)
            {
                bonusScore.Value += 30;
                block2.match = true;

                var random = (Defines.EBlockState)UnityEngine.Random.Range((int)Defines.EBlockState.PinkBomb, (int)Defines.EBlockState.BlueBomb + 1);
                block1.changeBlockState = random;
            }
            // �� ���� ��ź ���� ���
            else if (block1.IsBombBlock() == true)
            {
                await block1.Boom();
                return;
            }
            // �� ���� ��ź ���� ���
            else if (block2.IsBombBlock() == true)
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
            await CreateBombBlock();
            await DownBlock();

            curScore.Value += bonusScore.Value;
            if (bonusScore.Value > 0)
            {
                bonusScoreText.gameObject.SetActive(true);
                bonusScoreText.SetText(bonusScore.Value);
                await Task.Delay((int)(delay * delayMillisecond));
                bonusScoreText.gameObject.SetActive(false);
            }
            bonusScore.Value = 0;

            SetDissapearBlock();
            await UpdateMap();
            CheckMap();
            
        } while (isMatch || await CatInTheBox());

        if ((block1 != null && block2 != null && back == false) || isBoom == true && gameResult.Value != EGameState.CatPang)
        {
            moveCount.Value -= 1;
        }

        curScore.Value += bonusScore.Value;
        if (bonusScore.Value > 0)
        {
            bonusScoreText.gameObject.SetActive(true);
            bonusScoreText.SetText(bonusScore.Value);
            await Task.Delay((int)(delay * delayMillisecond));
            bonusScoreText.gameObject.SetActive(false);
        }
        bonusScore.Value = 0;

        do
        {
            if (tutorialNextBlock == false)
            {
                tutorialNextBlock = true;

                if (selectStage != Defines.ESelectStage.Normal && stageInfo.tutorialID > 0)
                {
                    var tutorialInfo = CHMMain.Json.GetTutorialInfo(stageInfo.tutorialID);
                    if (tutorialInfo == null || tutorialInfo.connectNextBlock == Defines.EBlockState.None)
                        break;

                    guideBackground.SetActive(true);
                    guideHole.gameObject.SetActive(true);

                    var settingValue = TutorialBlockSetting(boardArr, tutorialInfo.connectNextBlock);
                    guideHole.sizeDelta = settingValue.Item1;
                    guideHole.anchoredPosition = settingValue.Item2;

                    guideDesc.SetStringID(tutorialInfo.descNextBlockStringID);
                }
            }
        } while (false);

        BlockCreatorBlock(Defines.EBlockState.WallCreator, Defines.EBlockState.Wall);
        await UpdateMap();

        isLock = false;
    }

    public bool CheckTutorial()
    // Ʃ�丮�� ������ Ȯ��
    {
        for (int w = 0; w < boardSize; w++)
        {
            for (int h = 0; h < boardSize; h++)
            {
                if (boardArr[w, h].tutorialBlock)
                {
                    return true;
                }
            }
        }

        return false;
    }

    (Vector2, Vector2) TutorialBlockSetting(Block[,] blockArr, Defines.EBlockState blockState)
    // Ʃ�丮�� ������ ����
    {
        for (int w = 0; w < boardSize; w++)
        {
            for (int h = 0; h < boardSize; h++)
            {
                if (blockArr[w, h].GetBlockState() == blockState)
                {
                    blockArr[w, h].tutorialBlock = true;
                    return (blockArr[w, h].rectTransform.sizeDelta, blockArr[w, h].rectTransform.anchoredPosition);
                }
            }
        }
            
        return (Vector2.zero, Vector2.zero);
    }

    async Task CreateMap()
    // �������� ���� �� �� ����
    {
        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

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
                block.tutorialBlock = stageBlockInfo.tutorialBlock;

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

        bool canMatch = true;

        do
        {
            if (canMatch == false)
            {
                foreach (var block in boardArr)
                {
                    if (block == null) continue;

                    var stageBlockInfo = stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
                    if (stageBlockInfo == null)
                    {
                        var random = (Defines.EBlockState)UnityEngine.Random.Range(0, stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(Defines.ELog.CreateMap, 3, blockSpriteList[(int)random], random);
                        block.SetHp(-1);
                    }
                    else
                    {
                        var blockState = block.CheckSelectCatShop(stageBlockInfo.blockState);
                        block.SetBlockState(Defines.ELog.CreateMap, 4, blockSpriteList[(int)blockState], blockState);
                        block.tutorialBlock = stageBlockInfo.tutorialBlock;

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
            }

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
                        block.SetBlockState(Defines.ELog.CreateMap, 5, blockSpriteList[(int)random], random);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.match = false;
                        block.squareMatch = false;
                    }
                }
            }

            isMatch = false;
            CheckMap();

            if (isMatch == false)
            {
                canMatch = CanMatch();
                isMatch = false;
            }

        } while (isMatch == true || canMatch == false);

        Debug.Log("Create Map End");
        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
    }

    async Task UpdateMap()
    // �� ������Ʈ
    {
        try
        {
            bool reUpdate = false;
            bool matchUpdate = false;
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
                        block.SetHp(block.changeHp);
                        block.ResetScore();
                        block.SetOriginPos();
                        block.changeBlockState = Defines.EBlockState.None;
                    }
                    else if (reUpdate == true || block.IsMatch() == true)
                    {
                        if (block.IsFixdBlock() == true || block.IsBottomTouchDisappearBlock() == true)
                            continue;

                        var random = UnityEngine.Random.Range(0, stageInfo.blockTypeCount);

                        createDelay = true;
                        CreateNewBlock(block, Defines.ELog.UpdateMap, 2, (Defines.EBlockState)random);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.SetOriginPos();
                    }
                }

                reUpdate = CanMatch() == false;

                if (reUpdate)
                {
                    CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        stringID = 56
                    });
                }

                if (createDelay)
                {
                    await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
                }

                //matchUpdate = await CatInTheBox();

            } while (reUpdate || matchUpdate);

            isMatch = false;
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancle Update Map");
        }
    }

    bool CanMatch()
    // Match�� ������ ������ �˻�
    {
        isMatch = false;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                if (boardArr[i, j] == null)
                    continue;

                var curBlock = boardArr[i, j];
                if (curBlock.IsNotDragBlock() == true)
                    continue;

                if (curBlock.IsBombBlock() == true || curBlock.IsSpecialBombBlock() == true)
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
                    upBlock.IsNotDragBlock() == false)
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
                    downBlock.IsNotDragBlock() == false)
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
                    leftBlock.IsNotDragBlock() == false)
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
                    rightBlock.IsNotDragBlock() == false)
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

    void CheckMap(bool test = false)
    // 3Match �� Ȯ��
    {
        if (test == false)
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
                CheckSquareMatch(i, j, test);
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

            Check3Match(hBlockList, Defines.EDirection.Horizontal, test);
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

            Check3Match(vBlockList, Defines.EDirection.Vertical, test);
        }
    }

    bool CheckSquareMatch(int row, int col, bool _test = false)
    // ������ Match ���� ��ܺ��� Ȯ��
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

        // ���⼭���ʹ� ������ ��ġ Ȯ��
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
    // Match�� �� ����
    {
        bool removeDelay = false;
        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null)
                    continue;

                /*if (block.GetBlockState() == Defines.EBlockState.PinkBomb || block.IsBottomTouchDisappearBlock() == true)
                    continue;*/

                // �������� �� ��
                if (block.IsMatch() == true && block.remove == false)
                {
                    // �ֺ��� hp�� �ִ� ���� ������ ��
                    CheckArround(block.row, block.col);

                    curScore.Value += 1;

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
                                rect.DOAnchorPos(goldImg.rectTransform.anchoredPosition, UnityEngine.Random.Range(.2f, 1f)).OnComplete(() =>
                                {
                                    CHMMain.Resource.Destroy(gold);
                                });
                            });
                        }
                    }

                    // �������� ���޾� ������ ���
                    if (block.IsBombBlock() == true && block.boom == false)
                    {
                        bonusScore.Value += 20;
                        await block.Boom(false);

                        // Match �˻縦 �ٽ� �ؾ���
                        i = -1;
                        break;
                    }
                }
            }
        }

        if (removeDelay)
        {
            CHMMain.Sound.Play(Defines.ESound.Ppauk);
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
        }
    }

    async Task CreateBombBlock(bool boomBlock = true)
    // ��ź �� ����
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

                // ������ ��ġ �� ����
                if (block.squareMatch == true)
                {
                    createDelay = true;
                    
                    CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 1, block.GetPangType());
                    block.ResetScore();
                    block.SetOriginPos();
                }

                // ���ڰ� ��ġ �� ����
                if (block.hScore >= 3 && block.vScore >= 3)
                {
                    createDelay = true;

                    if (arrowPangIndex == 1)
                    {
                        CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 2, Defines.EBlockState.Arrow5);
                    }
                    else if (arrowPangIndex == 2)
                    {
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

                // ���� 3�� �ʰ� ��ġ �� Ư�� �� ����
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

                            if (arrowPangIndex == 1)
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 4, Defines.EBlockState.Arrow1);
                            }
                            else if (arrowPangIndex == 2)
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 5, Defines.EBlockState.Arrow4);
                            }
                        }

                        boardArr[tempRow, tempCol].ResetScore();
                    }

                    if (checkMoveBlock == false)
                    {
                        if (arrowPangIndex == 1)
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 6, Defines.EBlockState.Arrow1);
                        }
                        else if (arrowPangIndex == 2)
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 7, Defines.EBlockState.Arrow4);
                        }
                    }
                }
                // ���� 3�� �ʰ� ��ġ �� Ư�� �� ����
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

                            if (arrowPangIndex == 1)
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 9, Defines.EBlockState.Arrow3);
                            }
                            else if (arrowPangIndex == 2)
                            {
                                CreateNewBlock(tempBlock, Defines.ELog.CreateBoomBlock, 8, Defines.EBlockState.Arrow2);
                            }
                        }

                        boardArr[tempRow, tempCol].ResetScore();
                    }

                    if (checkMoveBlock == false)
                    {
                        if (arrowPangIndex == 1)
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 11, Defines.EBlockState.Arrow3);
                        }
                        else if (arrowPangIndex == 2)
                        {
                            CreateNewBlock(block, Defines.ELog.CreateBoomBlock, 10, Defines.EBlockState.Arrow2);
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
    // �� ���� �����.
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

    void Check3Match(List<Block> blockList, Defines.EDirection direction, bool test = false)
    // 3Match �� Ȯ��
    {
        Defines.EBlockState blockState = Defines.EBlockState.None;
        List<int> tempIndex = new List<int>();
        int matchCount = 0;

        for (int i = 0; i < blockList.Count; ++i)
        {
            if (blockList[i].IsFixdBlock() || blockList[i].IsBombBlock() || blockList[i].IsBottomTouchDisappearBlock() || blockList[i].IsNormalBlock() == false)
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
    // �� �Ʒ��� �̵�
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

            if (boardArr[row, col].IsMatch() || boardArr[row, col].IsFixdBlock())
                continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;

            int wallRow = -1;
            for (int i = boardSize - 1; i > row; --i)
            {
                // ������ ����ϴ� ������ Ȯ��
                if (boardArr[i, col].IsWallBlock())
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

    RectTransform CreateEffect(ParticleSystem effect, Vector2 movePos)
    // ����Ʈ�� �����Ѵ�.
    {
        var copyObj = CHMMain.Resource.Instantiate(effect.gameObject, transform.parent);
        copyObj.SetActive(true);
        var rt = copyObj.GetComponent<RectTransform>();
        rt.anchoredPosition = movePos;

        return rt;
    }

    public void ChangeBlock(Block moveBlock, Block targetBlock)
    // ���� ��ġ �� ���¸� �ٲ۴�.
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
    // ��ȿ�� �Ϲ� ������ Ȯ��
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null ||
            boardArr[row, col].IsFixdBlock() || boardArr[row, col].IsBombBlock() ||
            boardArr[row, col].IsSpecialBombBlock() || boardArr[row, col].IsBottomTouchDisappearBlock())
            return false;

        return true;
    }

    bool IsValidIndex(int row, int col)
    // �ε����� �ִ� ���� ������� Ȯ��
    {
        return row >= 0 && row < MAX && col >= 0 && col < MAX;
    }

    bool ChangeMatchState(int row, int col)
    // �ش� ���� match ���·� �����.
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return false;

        DamageBlock(row, col);

        if (boardArr[row, col].IsBottomTouchDisappearBlock() == false &&
            boardArr[row, col].IsFixdBlock() == false &&
            boardArr[row, col].GetBlockState() != Defines.EBlockState.PinkBomb)
        {
            boardArr[row, col].match = true;
            return true;
        }

        return false;
    }

    void CheckArround(int row, int col)
    // ���� ��ġ�Ǿ��ٸ� ��, �Ʒ�, �� �� ���� �������� �ش�.
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return;

        DamageBlock(row - 1, col);
        DamageBlock(row, col + 1);
        DamageBlock(row, col - 1);
        DamageBlock(row + 1, col);
    }

    void DamageBlock(int row, int col)
    // hp�� �ִ� ���̶�� hp - 1�� �Ѵ�.
    {
        if (IsValidIndex(row, col) && boardArr[row, col] != null)
        {
            if (boardArr[row, col].IsFixdBlock())
            {
                boardArr[row, col].Damage(stageInfo.blockTypeCount);
            }
        }
    }

    public async Task BoomAll(bool ani = true)
    // �� ��ü ��ź
    {
        bonusScore.Value += 5;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                ChangeMatchState(i, j);
            }
        }

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom1(Block block, bool ani = true)
    // �ڱ� �ֺ� 1ĭ ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Ching);

        int random = UnityEngine.Random.Range((int)Defines.EPangEffect.Blue, (int)Defines.EPangEffect.Green + 1);
        CreateEffect(pangEffectList[random], block.rectTransform.anchoredPosition);

        ChangeMatchState(block.row - 1, block.col - 1);
        ChangeMatchState(block.row - 1, block.col);
        ChangeMatchState(block.row - 1, block.col + 1);
        ChangeMatchState(block.row, block.col - 1);
        ChangeMatchState(block.row, block.col + 1);
        ChangeMatchState(block.row + 1, block.col - 1);
        ChangeMatchState(block.row + 1, block.col);
        ChangeMatchState(block.row + 1, block.col + 1);

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom2(Block block, bool ani = true)
    // ���ڰ� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line], block.rectTransform.anchoredPosition);

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(i, block.col);
        }

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(block.row, i);
        }

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom3(Block _specialBlock, Defines.EBlockState _blockState, bool _ani = true)
    // ���� �� ��ź
    {
        bonusScore.Value += 10;
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

                    var rt = CreateEffect(bombEffectPS, _specialBlock.rectTransform.anchoredPosition);
                    rt.DOAnchorPos(block.rectTransform.anchoredPosition, .05f);
                    blueHoleList.Add(rt.gameObject);

                    await Task.Delay(200, tokenSource.Token);
                }
            }
        }

        SaveBombCollectionData(_specialBlock);

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
    // ���� �� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        var rt = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 0));

        var rt2 = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, 180));

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(block.row, i);
        }

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom5(Block block, bool ani = true)
    // ���� �� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        var rt = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 90));

        var rt2 = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, 270));

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(i, block.col);
        }

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null);
        }
    }

    public async Task Boom6(Block block, bool ani = true)
    // X�� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        var rt = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Center_Hit], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 45));

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(block.row - i, block.col - i);
            ChangeMatchState(block.row - i, block.col + i);
            ChangeMatchState(block.row + i, block.col - i);
            ChangeMatchState(block.row + i, block.col + i);
        }

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom7(Block block, bool ani = true)
    // ���Ͽ�� �밢�� �� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        var rt = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, 45));

        var rt2 = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, 225));

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(block.row - i, block.col + i);
            ChangeMatchState(block.row + i, block.col - i);
        }

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom8(Block block, bool ani = true)
    // �»���� �밢�� �� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        var rt = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt.Rotate(new Vector3(0, 0, -45));

        var rt2 = CreateEffect(pangEffectList[(int)Defines.EPangEffect.Move_Line2], block.rectTransform.anchoredPosition);
        rt2.Rotate(new Vector3(0, 0, -225));

        for (int i = 0; i < MAX; ++i)
        {
            ChangeMatchState(block.row - i, block.col - i);
            ChangeMatchState(block.row + i, block.col + i);
        }

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom9(Block block, bool ani = true)
    // ������ ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        int random = UnityEngine.Random.Range((int)Defines.EPangEffect.Blue, (int)Defines.EPangEffect.Green + 1);
        CreateEffect(pangEffectList[random], block.rectTransform.anchoredPosition);

        ChangeMatchState(block.row - 2, block.col);
        ChangeMatchState(block.row - 1, block.col);
        ChangeMatchState(block.row - 1, block.col - 1);
        ChangeMatchState(block.row - 1, block.col + 1);
        ChangeMatchState(block.row, block.col - 2);
        ChangeMatchState(block.row, block.col - 1);
        ChangeMatchState(block.row, block.col + 1);
        ChangeMatchState(block.row, block.col + 2);
        ChangeMatchState(block.row + 1, block.col - 1);
        ChangeMatchState(block.row + 1, block.col + 1);
        ChangeMatchState(block.row + 1, block.col);
        ChangeMatchState(block.row + 2, block.col);

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom10(Block block, bool ani = true)
    // �ڱ� �ֺ� 1ĭ ��� �簢�� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        int random = UnityEngine.Random.Range((int)Defines.EPangEffect.Blue, (int)Defines.EPangEffect.Green + 1);
        CreateEffect(pangEffectList[random], block.rectTransform.anchoredPosition);

        ChangeMatchState(block.row - 2, block.col - 2);
        ChangeMatchState(block.row - 2, block.col - 1);
        ChangeMatchState(block.row - 2, block.col);
        ChangeMatchState(block.row - 2, block.col + 1);
        ChangeMatchState(block.row - 2, block.col + 2);
        ChangeMatchState(block.row - 1, block.col - 2);
        ChangeMatchState(block.row - 1, block.col + 2);
        ChangeMatchState(block.row, block.col - 2);
        ChangeMatchState(block.row, block.col + 2);
        ChangeMatchState(block.row + 1, block.col - 2);
        ChangeMatchState(block.row + 1, block.col + 2);
        ChangeMatchState(block.row + 2, block.col - 2);
        ChangeMatchState(block.row + 2, block.col - 1);
        ChangeMatchState(block.row + 2, block.col);
        ChangeMatchState(block.row + 2, block.col + 1);
        ChangeMatchState(block.row + 2, block.col + 2);

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom11(Block block, bool ani = true)
    // ���� ��� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        int random = UnityEngine.Random.Range((int)Defines.EPangEffect.Blue, (int)Defines.EPangEffect.Green + 1);
        CreateEffect(pangEffectList[random], block.rectTransform.anchoredPosition);

        ChangeMatchState(block.row - 2, block.col - 1);
        ChangeMatchState(block.row - 1, block.col - 2);
        ChangeMatchState(block.row - 1, block.col - 1);

        ChangeMatchState(block.row - 2, block.col + 1);
        ChangeMatchState(block.row - 1, block.col + 2);
        ChangeMatchState(block.row - 1, block.col + 1);

        ChangeMatchState(block.row + 2, block.col - 1);
        ChangeMatchState(block.row + 1, block.col - 2);
        ChangeMatchState(block.row + 1, block.col - 1);

        ChangeMatchState(block.row + 2, block.col + 1);
        ChangeMatchState(block.row + 1, block.col + 2);
        ChangeMatchState(block.row + 1, block.col + 1);

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Boom12(Block block, bool ani = true)
    // Z ��� ��ź
    {
        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        CHMMain.Sound.Play(ESound.Pising);

        int random = UnityEngine.Random.Range((int)Defines.EPangEffect.Blue, (int)Defines.EPangEffect.Green + 1);
        CreateEffect(pangEffectList[random], block.rectTransform.anchoredPosition);

        ChangeMatchState(block.row - 2, block.col - 2);
        ChangeMatchState(block.row - 2, block.col - 1);
        ChangeMatchState(block.row - 2, block.col);
        ChangeMatchState(block.row - 2, block.col + 1);
        ChangeMatchState(block.row - 2, block.col + 2);
        ChangeMatchState(block.row - 1, block.col + 1);
        ChangeMatchState(block.row + 1, block.col - 1);
        ChangeMatchState(block.row + 2, block.col - 2);
        ChangeMatchState(block.row + 2, block.col - 1);
        ChangeMatchState(block.row + 2, block.col);
        ChangeMatchState(block.row + 2, block.col + 1);
        ChangeMatchState(block.row + 2, block.col + 2);

        SaveBombCollectionData(block);

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task<bool> CatInTheBox()
    {
        bool inDelay = false;

        for (int w = 0; w < boardSize; w++)
        {
            for (int h = 0; h < boardSize; h++)
            {
                var block = boardArr[w, h];
                if (block == null)
                    continue;

                if (block.IsBoxBlock() == false)
                    continue;

                var upBlock = IsValidIndex(w - 1, h) == true ? boardArr[w - 1, h] : null;
                if (upBlock == null)
                    continue;

                if (block.CatInTheBox(upBlock.GetBlockState()))
                {
                    inDelay = true;

                    Debug.Log($"CatInTheBox {upBlock.GetBlockState()}:{upBlock.row}/{upBlock.col}");

                    upBlock.remove = true;
                    upBlock.match = true;
                    upBlock.rectTransform.DOAnchorPosY(block.rectTransform.anchoredPosition.y, delay);
                    upBlock.rectTransform.DOScale(0f, delay);
                }
            }
        }

        if (inDelay)
        {
            await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
            return true;
        }

        return false;
    }

    public void BlockCreatorBlock(Defines.EBlockState creatorBlock, Defines.EBlockState changeBlock)
    {
        for (int w = 0; w < boardSize; w++)
        {
            for (int h = 0; h < boardSize; h++)
            {
                var block = boardArr[w, h];
                if (block == null)
                    continue;

                if (block.GetBlockState() != creatorBlock)
                    continue;

                if (block.GetHp() <= 0)
                    continue;

                block.Damage();

                bool change = false;

                do
                {
                    var random = UnityEngine.Random.Range(0, 4);
                    switch (random)
                    {
                        case 0:
                            {
                                var upBlock = IsValidIndex(w - 1, h) == true ? boardArr[w - 1, h] : null;
                                if (upBlock != null)
                                {
                                    change = true;
                                    upBlock.changeHp = 1;
                                    upBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                        case 1:
                            {
                                var downBlock = IsValidIndex(w + 1, h) == true ? boardArr[w + 1, h] : null;
                                if (downBlock != null)
                                {
                                    change = true;
                                    downBlock.changeHp = 1;
                                    downBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                        case 2:
                            {
                                var leftBlock = IsValidIndex(w, h - 1) == true ? boardArr[w, h - 1] : null;
                                if (leftBlock != null)
                                {
                                    change = true;
                                    leftBlock.changeHp = 1;
                                    leftBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                        case 3:
                            {
                                var rightBlock = IsValidIndex(w, h + 1) == true ? boardArr[w, h + 1] : null;
                                if (rightBlock != null)
                                {
                                    change = true;
                                    rightBlock.changeHp = 1;
                                    rightBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                    }
                } while (change == false);
            }
        }
    }
}