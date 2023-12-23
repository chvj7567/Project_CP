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

    [Header("뒤로 가기")]
    [SerializeField] Button backBtn;

    [Header("타이머")]
    [SerializeField] Image timerImg;
    [SerializeField] CHTMPro timerText;
    [SerializeField, ReadOnly] float curTimer;

    [Header("골드")]
    [SerializeField] Image goldImg;

    [Header("블럭")]
    [SerializeField] Transform parent;
    [SerializeField] CHInstantiateButton instBtn;
    [SerializeField] GameObject origin;
    [SerializeField] float margin = 0f;

    [Header("배경")]
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField, ReadOnly] int backgroundIndex = 0;

    [Header("폭탄 이펙트")]
    [SerializeField] ParticleSystem bombEffectPS;
    [SerializeField] List<ParticleSystem> pangEffectList = new List<ParticleSystem>();

    [Header("게임 속도")]
    [SerializeField] public float delay;
    [SerializeField] int delayMillisecond;

    [Header("게임 상태")]
    [SerializeField, ReadOnly] Block[,] boardArr = new Block[MAX, MAX];
    [SerializeField, ReadOnly] public bool isDrag = false;
    [SerializeField, ReadOnly] public bool isLock = false;
    [SerializeField, ReadOnly] bool isMatch = false;
    [SerializeField, ReadOnly] bool oneTimeAlarm = false;
    [SerializeField, ReadOnly] int moveIndex1 = 0;
    [SerializeField, ReadOnly] int moveIndex2 = 0;
    [SerializeField, ReadOnly] int boardSize = 1;
    [SerializeField, ReadOnly] public ReactiveProperty<EGameState> gameResult = new ReactiveProperty<EGameState>();
    [SerializeField, ReadOnly] public bool gameEnd = false;
    [Space(20)]

    [SerializeField] CHTMPro targetScoreText;
    [SerializeField] CHTMPro moveCountText;
    [SerializeField] CHTMPro curScoreText;
    [SerializeField] CHTMPro bonusScoreText;
    [SerializeField, ReadOnly] ReactiveProperty<int> curScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> bonusScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] ReactiveProperty<int> moveCount = new ReactiveProperty<int>();

    [Header("유저 도와주기")]
    [SerializeField] bool autoPlay = false;
    [SerializeField, ReadOnly] int updateMapCount = 5;
    [SerializeField, ReadOnly] float teachTime;
    [SerializeField, ReadOnly] float dragTime;
    [SerializeField, ReadOnly] int canMatchRow = -1;
    [SerializeField, ReadOnly] int canMatchCol = -1;
    [SerializeField, ReadOnly] Defines.EDrag canMatchDrag = Defines.EDrag.None;

    [Header("보스 스테이지")]
    [SerializeField] GameObject normalBossObj;
    [SerializeField] GameObject angryBossObj;
    [SerializeField] GameObject cryBossObj;
    [SerializeField] Image bossHpImage;
    [SerializeField] CHTMPro bossHpText;
    [SerializeField] CHTMPro hpText;
    [SerializeField, ReadOnly] ReactiveProperty<int> hp = new ReactiveProperty<int>();

    [Header("각 스테이지 UI 오브젝트")]
    [SerializeField] GameObject onlyNormalStageObject;
    [SerializeField] GameObject onlyBossStageObject;

    [Header("폭탄 선택 바")]
    [SerializeField, ReadOnly] int arrowPangIndex = 1;
    [SerializeField] CHButton arrowPang1;
    [SerializeField] CHButton arrowPang2;
    [SerializeField] Image banView;

    [Header("가이드")]
    [SerializeField] RectTransform guideHole;
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button guideBackgroundBtn;
    [SerializeField] List<RectTransform> easyStageGuideHoleList = new List<RectTransform>();
    [SerializeField] List<RectTransform> bossStageGuideHoleList = new List<RectTransform>();
    [SerializeField] CHTMPro guideDesc;

    List<Sprite> _blockSpriteList = new List<Sprite>();
    Infomation.StageInfo _stageInfo;
    List<Infomation.StageBlockInfo> _stageBlockInfoList = new List<Infomation.StageBlockInfo>();
    Defines.ESelectStage _selectStage = Defines.ESelectStage.Normal;
    Data.Login _loginData;

    CancellationTokenSource tokenSource;

    int helpTime = 0;
    bool tutorialNextBlock = false;
    bool bossSkill = false;
    bool init = false;

    async void Start()
    {
        bonusScoreText.gameObject.SetActive(false);

        guideBackground.SetActive(false);
        guideHole.gameObject.SetActive(false);

        onlyNormalStageObject.SetActive(true);
        onlyBossStageObject.SetActive(false);

        for (int i = 0; i < easyStageGuideHoleList.Count; ++i)
        {
            easyStageGuideHoleList[i].gameObject.SetActive(false);
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

                    bool useTime = _stageInfo.time > 0;
                    bool useTargetScore = _stageInfo.targetScore > 0;
                    bool useMoveCount = _stageInfo.moveCount > 0;

                    // 체력이 있는 블럭이 있거나 없어져야 되는 블럭이 있으면 미클리어
                    for (int i = 0; i < boardSize; ++i)
                    {
                        for (int j = 0; j < boardSize; ++j)
                        {
                            if (boardArr[i, j].checkHp == false)
                                continue;

                            if (boardArr[i, j].GetHp() > 0 || boardArr[i, j].IsBottomTouchDisappearBlock() == true)
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
                        if (useTargetScore == true && curScore.Value < _stageInfo.targetScore)
                        {
                            clear = false;
                        }

                        GameEnd(clear);
                    }
                    // 시간을 사용하고 시간이 다 되지 않은 경우 게임 종료 확인
                    else
                    {
                        // 목표 점수를 사용하는 경우 목표 점수 달성 확인
                        if (useTargetScore == true && curScore.Value < _stageInfo.targetScore)
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
        if (isLock == false)
        {
            curTimer += Time.deltaTime;
            timerImg.fillAmount = curTimer / _stageInfo.time;

            if (curTimer >= helpTime)
            {
                if (_stageInfo.time >= helpTime)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.SetText(_stageInfo.time - helpTime);
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
            dragTime = Time.time;
        }
        else
        {
            

            // .5초 동안 드래그를 안하면 알려줌
            if (autoPlay && dragTime + .5f < Time.time)
            {
                var block = boardArr[canMatchRow, canMatchCol];
                block.Drag(canMatchDrag);
            }

            // 3초 동안 드래그를 안하면 알려줌
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
                }
                catch (TaskCanceledException) { }

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
    // 블럭 이미지 로드
    {
        for (EBlockState i = 0; i < EBlockState.Max; ++i)
        {
            TaskCompletionSource<Sprite> imageTask = new TaskCompletionSource<Sprite>();
            CHMMain.Resource.LoadSprite(i, (sprite) =>
            {
                if (sprite != null)
                    _blockSpriteList.Add(sprite);

                imageTask.SetResult(sprite);
            });

            await imageTask.Task;
        }
    }

    void InitData()
    // 데이터 초기화
    {
        if (init)
            return;

        init = true;

        tokenSource = new CancellationTokenSource();
        backgroundIndex = PlayerPrefs.GetInt(CHMMain.String.Background);
        _loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        _selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);

        ChangeBackgroundLoop();

        var stage = 0;
        switch (_selectStage)
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

        _stageInfo = CHMMain.Json.GetStageInfo(stage);
        _stageBlockInfoList = CHMMain.Json.GetStageBlockInfoList(stage);
        boardSize = _stageInfo.boardSize;

        switch (_selectStage)
        {
            case ESelectStage.Normal:
                {
                    // 튜토리얼은 Easy(일반)에서 진행함으로 삭제
                    _stageInfo.tutorialID = -1;
                    for (int i = 0; i < _stageBlockInfoList.Count; ++i)
                    {
                        _stageBlockInfoList[i].tutorialBlock = false;
                    }
                }
                break;
            case ESelectStage.Boss:
                break;
            case ESelectStage.Easy:
                {
                    // 난이도 조정
                    if (_stageInfo.time > 0)
                    {
                        _stageInfo.time *= 2;
                    }
                    else if (_stageInfo.targetScore > 0)
                    {
                        _stageInfo.targetScore /= 2;
                    }
                    else if (_stageInfo.moveCount > 0)
                    {
                        _stageInfo.moveCount *= 2;
                    }
                }
                break;
        }

        targetScoreText.SetText(_stageInfo.targetScore);
        if (_stageInfo.targetScore < 0)
        {
            targetScoreText.gameObject.SetActive(false);
        }

        if (_stageInfo.moveCount > 0)
        {
            moveCount.Value = _stageInfo.moveCount + _loginData.useMoveItemCount;
        }
        else
        {
            moveCount.Value = 9999;

            if (_loginData.useMoveItemCount > 0)
                _loginData.addMoveItemCount += _loginData.useMoveItemCount;
        }

        if (_stageInfo.time > 0)
        {
            _stageInfo.time += _loginData.useTimeItemCount * 10;
        }
        else
        {
            if (_loginData.useTimeItemCount > 0)
                _loginData.addTimeItemCount += _loginData.useTimeItemCount;
        }

        if (_selectStage == Defines.ESelectStage.Boss)
        {
            gameResult.Value = EGameState.BossStagePlay;
        }
        else
        {
            gameResult.Value = EGameState.EasyOrNormalStagePlay;
        }

        // 보스 스테이지일 경우만
        if (_selectStage == Defines.ESelectStage.Boss)
        {
            onlyBossStageObject.SetActive(true);
            onlyNormalStageObject.SetActive(false);

            normalBossObj.SetActive(true);
            angryBossObj.SetActive(false);
            cryBossObj.SetActive(false);

            hp.Subscribe(_ =>
            {
                if (_ >= 0)
                    hpText.SetText(hp);
            });

            hp.Value = _loginData.hp;

            // 1초뒤에 1초에 한 번씩 실행
            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                hp.Value -= 1;
            })
            .AddTo(this);

            curScore.Subscribe(_ =>
            {
                var fillAmountValue = (_stageInfo.targetScore - _) / (float)_stageInfo.targetScore;
                bossHpImage.DOFillAmount(fillAmountValue, .5f);
                var bossHp = Mathf.Max(0, _stageInfo.targetScore - _);
                bossHpText.SetText(bossHp);

                if (bossSkill == false && fillAmountValue <= .5f)
                {
                    bossSkill = true;

                    normalBossObj.SetActive(false);
                    angryBossObj.SetActive(true);
                    cryBossObj.SetActive(false);

                    CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        stringID = 78
                    });

                    // 10초에 한 번씩 실행
                    Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10))
                    .Subscribe(_ =>
                    {
                       BossSkill();
                    })
                    .AddTo(this);
                }
            });
        }
    }

    async Task StartGuide()
    // 가이드 시작
    {
        if (_selectStage == Defines.ESelectStage.Easy && _loginData.guideIndex == 5)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();

            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            var guideIndex = await EasyStageGuideStart();
            _loginData.guideIndex += guideIndex;

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }

        if (_selectStage == Defines.ESelectStage.Boss && _loginData.guideIndex == 11)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();

            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            var guideIndex = await BossStageGuideStart();
            _loginData.guideIndex += guideIndex;

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }
    }

    void StartTutorial()
    // 튜토리얼 시작
    {
        if (_selectStage != Defines.ESelectStage.Normal && _stageInfo.tutorialID > 0)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideHole.gameObject.SetActive(true);

            guideHole.SetAsLastSibling();
            guideBackground.transform.SetAsLastSibling();

            var holeValue = GetTutorialStageImgSettingValue(boardArr, _stageBlockInfoList);
            guideHole.sizeDelta = holeValue.Item1;
            guideHole.anchoredPosition = holeValue.Item2;

            var tutorialInfo = CHMMain.Json.GetTutorialInfo(_stageInfo.tutorialID);
            if (tutorialInfo != null)
            {
                guideDesc.SetStringID(tutorialInfo.descStringID);
            }
        }
    }

    async Task<int> EasyStageGuideStart()
    // 일반 스테이지 Game UI 가이드 시작
    {
        TaskCompletionSource<int> tutorialCompleteTask = new TaskCompletionSource<int>();

        guideBackground.SetActive(true);

        for (int i = 0; i < easyStageGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 6);
            if (guideInfo == null)
                break;

            easyStageGuideHoleList[i].gameObject.SetActive(true);
            guideDesc.SetStringID(guideInfo.descStringID);

            TaskCompletionSource<bool> buttonClicktask = new TaskCompletionSource<bool>();

            var btnComplete = guideBackgroundBtn.OnClickAsObservable().Subscribe(_ =>
            {
                buttonClicktask.SetResult(true);
            });

            await buttonClicktask.Task;

            easyStageGuideHoleList[i].gameObject.SetActive(false);

            btnComplete.Dispose();
        }

        tutorialCompleteTask.SetResult(easyStageGuideHoleList.Count);

        return await tutorialCompleteTask.Task;
    }

    async Task<int> BossStageGuideStart()
    // 보스 스테이지 Game UI 가이드 시작
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
    // 튜토리얼에서 밝게 보이는 부분 크기과 위치 지정
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
    // 게임 종료 시 실행
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

        if (clear == false)
        {
            gameResult.Value = EGameState.GameOver;
        }
        else
        {
            gameResult.Value = EGameState.GameClear;

            if (_selectStage == Defines.ESelectStage.Boss)
            {
                normalBossObj.SetActive(false);
                angryBossObj.SetActive(false);
                cryBossObj.SetActive(true);
            }
        }

        if (await CatPang(true))
        {
            gameResult.Value = EGameState.CatPang;

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 55,
                closeTime = 1
            });

            await Task.Delay(1000);
            await CatPang();

            // 게임결과 다시 체크하도록
            gameEnd = false;
            if (_selectStage == Defines.ESelectStage.Boss)
            {
                gameResult.Value = EGameState.BossStagePlay;
            }
            else
            {
                gameResult.Value = EGameState.EasyOrNormalStagePlay;
            }

            return;
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

    async Task<bool> CatPang(bool check = false)
    // 마지막에 터지는 캣팡!!!
    {
        isLock = true;

        for (int w = 0; w < boardSize; ++w)
        {
            for (int h = 0; h < boardSize; ++h)
            {
                if (boardArr[w, h].IsBombBlock() &&
                    boardArr[w, h].GetBlockState() != Defines.EBlockState.PinkBomb &&
                    boardArr[w, h].GetBlockState() != Defines.EBlockState.RainbowPang)
                {
                    if (check)
                        return true;

                    await boardArr[w, h].Bomb();

                    w = -1; break;
                }
            }
        }

        isLock = false;

        return false;
    }

    void SetDissapearBlock()
    // 사라져야할 블럭은 랜덤 생상 폭탄으로 변환
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
    // 배경 변환 반복
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
    // 다음 배경 인덱스 반환
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
    // 현재 스테이지를 클리어 상태로 저장
    {
        switch (_selectStage)
        {
            case ESelectStage.Normal:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).stage < PlayerPrefs.GetInt(CHMMain.String.Stage))
                    CHMData.Instance.GetLoginData(CHMMain.String.CatPang).stage = PlayerPrefs.GetInt(CHMMain.String.Stage);
                break;
            case ESelectStage.Boss:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage < PlayerPrefs.GetInt(CHMMain.String.BossStage))
                    CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage = PlayerPrefs.GetInt(CHMMain.String.BossStage);
                break;
            case ESelectStage.Easy:
                if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).easyStage < PlayerPrefs.GetInt(CHMMain.String.EasyStage))
                    CHMData.Instance.GetLoginData(CHMMain.String.CatPang).easyStage = PlayerPrefs.GetInt(CHMMain.String.EasyStage);
                break;
        }
    }

    Defines.EClearState GetClearState()
    // 현재 스테이지의 클리어 상태
    {
        switch (_selectStage)
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
    // 폭탄 콜렉션 데이터 저장
    {
        if (block.IsBombBlock() == false && block.IsSpecialBombBlock() == false)
            return;

        if (GetClearState() == Defines.EClearState.Clear)
            return;


        var collectionData = CHMData.Instance.GetCollectionData(block.GetBlockState().ToString());
        collectionData.value += 1;
    }

    public async Task AfterDrag(Block block1, Block block2, bool isBoom = false)
    // 블럭을 드래그하고 난 후 다음 드래그가 가능한 상태까지
    {
        // 블럭 생성기는 드래그 후 한 번만 동작해야 함.
        bool checkCreateBlock = false;

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

            // 두 블럭 모두 스페셜 블럭일 경우
            if (block1.IsSpecialBombBlock() == true && block2.IsSpecialBombBlock() == true)
            {
                block1.match = true;
                block2.match = true;
                await BoomAll();

                isLock = false;
                return;
            }
            // 한 블럭만 스페셜 블럭 중 특별한 경우
            else if (block1.GetBlockState() == Defines.EBlockState.PinkBomb)
            {
                await Boom3(block1, block2.GetBlockState());

                isLock = false;
                return;
            }
            // 한 블럭만 스페셜 블럭 중 특별한 경우
            else if (block2.GetBlockState() == Defines.EBlockState.PinkBomb)
            {
                await Boom3(block2, block1.GetBlockState());

                isLock = false;
                return;
            }
            // 한 블럭만 스페셜 블럭일 경우
            else if (block1.IsSpecialBombBlock() == true)
            {
                await block1.Bomb();

                isLock = false;
                return;
            }
            // 한 블럭만 스페셜 블럭일 경우
            else if (block2.IsSpecialBombBlock() == true)
            {
                await block2.Bomb();

                isLock = false;
                return;
            }
            // 두 블럭 모두 폭탄 블럭일 경우
            else if (block1.IsBombBlock() == true && block2.IsBombBlock() == true)
            {
                bonusScore.Value += 30;
                block2.match = true;

                var random = (Defines.EBlockState)UnityEngine.Random.Range((int)Defines.EBlockState.PinkBomb, (int)Defines.EBlockState.BlueBomb + 1);
                block1.changeBlockState = random;
            }
            // 한 블럭만 폭탄 블럭일 경우
            else if (block1.IsBombBlock() == true)
            {
                await block1.Bomb();

                isLock = false;
                return;
            }
            // 한 블럭만 폭탄 블럭일 경우
            else if (block2.IsBombBlock() == true)
            {
                await block2.Bomb();

                isLock = false;
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

            if (_selectStage == Defines.ESelectStage.Boss)
            {
                bonusScore.Value += _loginData.attack;
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

            if (checkCreateBlock == false)
            {
                checkCreateBlock = true;
                BlockCreatorBlock(Defines.EBlockState.WallCreator, Defines.EBlockState.Wall);
                BlockCreatorBlock(Defines.EBlockState.PotalCreator, Defines.EBlockState.Potal);
            }

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

                if (_selectStage != Defines.ESelectStage.Normal && _stageInfo.tutorialID > 0)
                {
                    var tutorialInfo = CHMMain.Json.GetTutorialInfo(_stageInfo.tutorialID);
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

        isLock = false;
    }

    public bool CheckTutorial()
    // 튜토리얼 블럭인지 확인
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
    // 튜토리얼 블럭으로 세팅
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
    // 스테이지 시작 시 맵 생성
    {
        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        foreach (var block in boardArr)
        {
            if (block == null) continue;
            float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
            block.originPos.x -= moveDis;
            block.SetOriginPos();
            block.rectTransform.DOScale(1f, delay);

            var stageBlockInfo = _stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
            if (stageBlockInfo == null)
            {
                var random = (Defines.EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                random = block.CheckSelectCatShop(random);
                block.SetBlockState(Defines.ELog.CreateMap, 1, _blockSpriteList[(int)random], random);
                block.checkHp = block.CheckHpBlock();
                block.SetHp(-1);
            }
            else
            {
                var blockState = block.CheckSelectCatShop(stageBlockInfo.blockState);
                block.SetBlockState(Defines.ELog.CreateMap, 2, _blockSpriteList[(int)blockState], blockState);
                block.checkHp = block.CheckHpBlock();
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

                    var stageBlockInfo = _stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
                    if (stageBlockInfo == null)
                    {
                        var random = (Defines.EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(Defines.ELog.CreateMap, 3, _blockSpriteList[(int)random], random);
                        block.SetHp(-1);
                    }
                    else
                    {
                        var blockState = block.CheckSelectCatShop(stageBlockInfo.blockState);
                        block.SetBlockState(Defines.ELog.CreateMap, 4, _blockSpriteList[(int)blockState], blockState);
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
                        var random = (Defines.EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(Defines.ELog.CreateMap, 5, _blockSpriteList[(int)random], random);
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
                canMatch = CanPlay();
                isMatch = false;
            }

        } while (isMatch == true || canMatch == false);

        Debug.Log("Create Map End");
        await Task.Delay((int)(delay * delayMillisecond), tokenSource.Token);
    }

    async Task UpdateMap()
    // 맵 업데이트
    {
        try
        {
            int count = 0;
            bool reUpdate = false;
            bool createDelay = false;

            int firstRow = 0;
            int firstCol = 0;
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
                        block.checkHp = false;
                    }
                    else if (reUpdate || block.IsMatch())
                    {
                        if (block.IsFixdBlock() || block.IsBottomTouchDisappearBlock())
                            continue;

                        if (reUpdate && block.GetBlockState() == Defines.EBlockState.RainbowPang)
                            continue;

                        firstRow = block.row;
                        firstCol = block.col;

                        var random = UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);

                        createDelay = true;
                        CreateNewBlock(block, Defines.ELog.UpdateMap, 2, (Defines.EBlockState)random);
                        block.SetHp(-1);
                        block.ResetScore();
                        block.SetOriginPos();
                    }
                }

                reUpdate = CanPlay() == false;

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

                if (count++ > updateMapCount)
                {
                    CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        stringID = 80
                    });

                    CreateNewBlock(boardArr[firstRow, firstCol], Defines.ELog.UpdateMap, 3, Defines.EBlockState.YellowBomb);
                    break;
                }
            } while (reUpdate);

            isMatch = false;
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancle Update Map");
        }
    }

    bool CanPlay()
    // 플레이가 가능한 맵인지 검사
    {
        isMatch = false;

        for (int i = 0; i < MAX; ++i)
        {
            for (int j = 0; j < MAX; ++j)
            {
                if (boardArr[i, j] == null)
                    continue;

                var curBlock = boardArr[i, j];
                if (curBlock.CanNotDragBlock() == true)
                    continue;

                var canDrag = CanDragBlock(curBlock);
                if (curBlock.GetBlockState() == Defines.EBlockState.PinkBomb && canDrag != Defines.EDrag.None)
                {
                    canMatchRow = curBlock.row;
                    canMatchCol = curBlock.col;
                    canMatchDrag = canDrag;
                    return true;
                }

                if (curBlock.IsBombBlock() == true || curBlock.IsSpecialBombBlock() == true)
                {
                    canMatchRow = curBlock.row;
                    canMatchCol = curBlock.col;
                    canMatchDrag = Defines.EDrag.Click;
                    return true;
                }

                var upBlock = IsValidIndex(i - 1, j) == true ? boardArr[i - 1, j] : null;
                var downBlock = IsValidIndex(i + 1, j) == true ? boardArr[i + 1, j] : null;
                var leftBlock = IsValidIndex(i, j - 1) == true ? boardArr[i, j - 1] : null;
                var rightBlock = IsValidIndex(i, j + 1) == true ? boardArr[i, j + 1] : null;

                if (upBlock != null &&
                    upBlock.CanNotDragBlock() == false)
                {
                    ChangeBlock(curBlock, upBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, upBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        canMatchDrag = Defines.EDrag.Up;
                        return true;
                    }
                }

                if (downBlock != null &&
                    downBlock.CanNotDragBlock() == false)
                {
                    ChangeBlock(curBlock, downBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, downBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        canMatchDrag = Defines.EDrag.Down;
                        return true;
                    }
                }

                if (leftBlock != null &&
                    leftBlock.CanNotDragBlock() == false)
                {
                    ChangeBlock(curBlock, leftBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, leftBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        canMatchDrag = Defines.EDrag.Left;
                        return true;
                    }
                }

                if (rightBlock != null &&
                    rightBlock.CanNotDragBlock() == false)
                {
                    ChangeBlock(curBlock, rightBlock);
                    CheckMap(true);
                    ChangeBlock(curBlock, rightBlock);
                    if (isMatch == true)
                    {
                        canMatchRow = curBlock.row;
                        canMatchCol = curBlock.col;
                        canMatchDrag = Defines.EDrag.Right;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void CheckMap(bool test = false)
    // 3Match 블럭 확인
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

                /*if (block.GetBlockState() == Defines.EBlockState.PinkBomb || block.IsBottomTouchDisappearBlock() == true)
                    continue;*/

                // 없어져야 할 블럭
                if (block.IsMatch() == true && block.remove == false)
                {
                    // 주변에 hp가 있는 블럭은 데미지 줌
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

                    // 아이템이 연달아 터지는 경우
                    if (block.IsBombBlock() == true && block.boom == false)
                    {
                        bonusScore.Value += 20;
                        await block.Bomb(false);

                        // Match 검사를 다시 해야함
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
    // 새 블럭을 만든다.
    {
        _blockState = _block.CheckSelectCatShop(_blockState);

        _block.SetBlockState(_log, _key, _blockSpriteList[(int)_blockState], _blockState);
        _block.checkHp = _block.CheckHpBlock();
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
    // 3Match 블럭 확인
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

            if (boardArr[row, col].IsMatch() || boardArr[row, col].IsFixdBlock())
                continue;

            Block moveBlock = boardArr[row, col];
            Block targetBlock = null;

            int wallRow = -1;
            for (int i = boardSize - 1; i > row; --i)
            {
                // 벽으로 사용하는 블럭인지 확인
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
    // 이펙트를 생성한다.
    {
        var copyObj = CHMMain.Resource.Instantiate(effect.gameObject, transform.parent);
        copyObj.SetActive(true);
        var rt = copyObj.GetComponent<RectTransform>();
        rt.anchoredPosition = movePos;

        return rt;
    }

    public void ChangeBlock(Block moveBlock, Block targetBlock)
    // 블럭의 위치 및 상태를 바꾼다.
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

        Debug.Log($"{moveBlock.row}/{moveBlock.col} <=> {targetBlock.row}/{targetBlock.col}");

        boardArr[moveBlock.row, moveBlock.col] = moveBlock;
        boardArr[targetBlock.row, targetBlock.col] = targetBlock;
    }

    bool IsNormalBlock(int row, int col)
    // 유효한 일반 블럭인지 확인
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null ||
            boardArr[row, col].IsFixdBlock() || boardArr[row, col].IsBombBlock() ||
            boardArr[row, col].IsSpecialBombBlock() || boardArr[row, col].IsBottomTouchDisappearBlock())
            return false;

        return true;
    }

    bool IsValidIndex(int row, int col)
    // 인덱스가 해당 스테이지에서 유효한지 확인
    {
        return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
    }

    bool ChangeMatchState(int row, int col)
    // 해당 블럭을 match 상태로 만든다.
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return false;

        DamageBlock(row, col);

        if (boardArr[row, col].GetBlockState() == Defines.EBlockState.RainbowPang &&
            boardArr[row, col].GetHp() > 0)
        {
            return false;
        }

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
    // 블럭이 매치되었다면 위, 아래, 양 옆 블럭에 데미지를 준다.
    {
        if (IsValidIndex(row, col) == false || boardArr[row, col] == null)
            return;

        DamageBlock(row - 1, col);
        DamageBlock(row, col + 1);
        DamageBlock(row, col - 1);
        DamageBlock(row + 1, col);
    }

    void DamageBlock(int row, int col)
    // hp가 있는 블럭이라면 hp - 1을 한다.
    {
        if (IsValidIndex(row, col) && boardArr[row, col] != null)
        {
            if (boardArr[row, col].GetBlockState() == Defines.EBlockState.RainbowPang)
            {
                boardArr[row, col].Damage(_stageInfo.blockTypeCount, false);
            }
            else
            {
                boardArr[row, col].Damage(_stageInfo.blockTypeCount);
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
                ChangeMatchState(i, j);
            }
        }

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }

    public async Task Bomb1(Block block, bool ani = true)
    // 자기 주변 1칸 폭탄
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

    public async Task Bomb2(Block block, bool ani = true)
    // 십자가 폭탄
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
    // 같은 블럭 폭탄
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

    public async Task Bomb4(Block block, bool ani = true)
    // 가로 줄 폭탄
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

    public async Task Bomb5(Block block, bool ani = true)
    // 세로 줄 폭탄
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

    public async Task Bomb6(Block block, bool ani = true)
    // X자 폭탄
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

    public async Task Bomb7(Block block, bool ani = true)
    // 좌하우상 대각선 줄 폭탄
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

    public async Task Bomb8(Block block, bool ani = true)
    // 좌상우하 대각선 줄 폭탄
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

    public async Task Bomb9(Block block, bool ani = true)
    // 마름모 폭탄
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

    public async Task Bomb10(Block block, bool ani = true)
    // 자기 주변 1칸 띄운 사각형 폭탄
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

    public async Task Bomb11(Block block, bool ani = true)
    // 빠직 모양 폭탄
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

    public async Task Bomb12(Block block, bool ani = true)
    // Z 모양 폭탄
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

                // -일 경우 계속 생성
                if (block.GetHp() == 0)
                    continue;

                bool change = false;
                var random = UnityEngine.Random.Range(0, 4);

                int tempW = w;
                int tempH = h;

                do
                {
                    switch (random)
                    {
                        case 0:
                            {
                                tempW -= 1;

                                var upBlock = IsValidIndex(tempW, tempH) == true ? boardArr[tempW, tempH] : null;
                                if (upBlock != null && upBlock.IsNormalBlock())
                                {
                                    block.Damage();
                                    change = true;
                                    upBlock.changeHp = 1;
                                    upBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                        case 1:
                            {
                                tempW += 1;

                                var downBlock = IsValidIndex(tempW, tempH) == true ? boardArr[tempW, tempH] : null;
                                if (downBlock != null && downBlock.IsNormalBlock())
                                {
                                    block.Damage();
                                    change = true;
                                    downBlock.changeHp = 1;
                                    downBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                        case 2:
                            {
                                tempH -= 1;

                                var leftBlock = IsValidIndex(tempW, tempH) == true ? boardArr[tempW, tempH] : null;
                                if (leftBlock != null && leftBlock.IsNormalBlock())
                                {
                                    block.Damage();
                                    change = true;
                                    leftBlock.changeHp = 1;
                                    leftBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                        case 3:
                            {
                                tempH += 1;

                                var rightBlock = IsValidIndex(tempW, tempH) == true ? boardArr[tempW, tempH] : null;
                                if (rightBlock != null && rightBlock.IsNormalBlock())
                                {
                                    block.Damage();
                                    change = true;
                                    rightBlock.changeHp = 1;
                                    rightBlock.changeBlockState = changeBlock;
                                }
                            }
                            break;
                    }

                    if (tempW < 0 || tempW > boardSize || tempH < 0 || tempH > boardSize)
                        break;

                } while (change == false);
            }
        }
    }

    void BossSkill()
    // 랜덤한 블럭 벽 또는 포탈로 변경
    {
        // 벽 or 포탈
        var block = UnityEngine.Random.Range(0, 2);
        // hp는 0부터 10까지
        var blockHp = UnityEngine.Random.Range(0, 10);
        if (blockHp == 0)
            blockHp = -1;

        int w = 0, h = 0;

        do
        {
            w = UnityEngine.Random.Range(0, boardSize);
            h = UnityEngine.Random.Range(0, boardSize);
        } while (boardArr[w, h].IsNormalBlock() == false);

        switch (block)
        {
            case 0:
                {
                    boardArr[w, h].changeBlockState = Defines.EBlockState.Wall;
                    boardArr[w, h].changeHp = blockHp;
                }
                break;
            case 1:
                {
                    boardArr[w, h].changeBlockState = Defines.EBlockState.Potal;
                    boardArr[w, h].changeHp = blockHp;
                }
                break;
        }
    }

    Defines.EDrag CanDragBlock(Block block)
    // 드래그가 가능한 블럭인지 확인
    {
        int w = block.row;
        int h = block.col;

        bool upCheck = IsValidIndex(w - 1, h) && boardArr[w - 1, h].CanNotDragBlock() == false;
        bool downCheck = IsValidIndex(w + 1, h) && boardArr[w + 1, h].CanNotDragBlock() == false;
        bool leftCheck = IsValidIndex(w, h - 1) && boardArr[w, h - 1].CanNotDragBlock() == false;
        bool rightCheck = IsValidIndex(w, h + 1) && boardArr[w, h + 1].CanNotDragBlock() == false;

        if (upCheck)
            return Defines.EDrag.Up;
        if (downCheck)
            return Defines.EDrag.Down;
        if (leftCheck)
            return Defines.EDrag.Left;
        if (rightCheck)
            return Defines.EDrag.Right;

        return Defines.EDrag.None;
    }

    public async Task RainbowPang(Block block, bool ani = true)
    // 무지개 팡은 각 특수 폭탄 1개 씩을 맵에 랜덤으로 뿌려줌
    {
        if (block.GetHp() > 0)
            return;

        bonusScore.Value += 10;
        block.match = true;
        block.boom = true;

        Defines.EBlockState blockState = Defines.EBlockState.PinkBomb;
        for (;blockState <= Defines.EBlockState.BlueBomb; ++blockState)
        {
            do
            {
                int w = UnityEngine.Random.Range(0, boardSize);
                int h = UnityEngine.Random.Range(0, boardSize);

                if (boardArr[w, h].IsNormalBlock() == false)
                    continue;

                if (boardArr[w, h].changeBlockState != EBlockState.None)
                    continue;

                boardArr[w, h].changeBlockState = blockState;
                break;

            } while (true);
        }

        if (ani)
        {
            await AfterDrag(null, null, true);
        }
    }
}