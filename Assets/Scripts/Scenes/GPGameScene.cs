using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPGameScene : MonoBehaviour
{
    private const int MAX = 9;

    [Header("뒤로 가기")]
    [SerializeField] private Button backBtn;

    [Header("타이머")]
    [SerializeField] private Image timerImg;
    [SerializeField] private CHText timerText;
    [SerializeField, ReadOnly] private float curTimer;

    [Header("점수 골드 및 이미지")]
    [SerializeField] private Image goldImg;
    [SerializeField] private RectTransform goldImgTarget;
    [SerializeField] private List<Image> catFootImgList = new List<Image>();

    [Header("보드")]
    [SerializeField] private Transform parent;
    [SerializeField] private CHInstantiateButton instBtn;
    [SerializeField] private GameObject origin;
    [SerializeField] private float margin = 0f;

    [Header("폭탄 이펙트")]
    [SerializeField] private ParticleSystem bombEffectPS;
    [SerializeField] private List<ParticleSystem> pangEffectList = new List<ParticleSystem>();

    [Header("게임 속도")]
    [SerializeField] public float delay;
    [SerializeField] private int delayMillisecond;

    [Header("게임 상태")]
    [SerializeField, ReadOnly] private Block[,] boardArr = new Block[MAX, MAX];
    [SerializeField, ReadOnly] public bool isDrag = false;
    [SerializeField, ReadOnly] public bool isLock = false;
    [SerializeField, ReadOnly] private bool isMatch = false;
    [SerializeField, ReadOnly] private bool oneTimeAlarm = false;
    [SerializeField, ReadOnly] private int moveIndex1 = 0;
    [SerializeField, ReadOnly] private int moveIndex2 = 0;
    [SerializeField, ReadOnly] private int boardSize = 1;
    [SerializeField, ReadOnly] public ReactiveProperty<EGameState> gameResult = new ReactiveProperty<EGameState>();
    [SerializeField, ReadOnly] public bool gameEnd = false;

    [SerializeField] private CHText targetScoreText;
    [SerializeField] private CHText moveCountText;
    [SerializeField] private CHText curScoreText;
    [SerializeField] private CHText bonusScoreText;
    [SerializeField, ReadOnly] private ReactiveProperty<int> curScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] private ReactiveProperty<int> bonusScore = new ReactiveProperty<int>();
    [SerializeField, ReadOnly] private ReactiveProperty<int> moveCount = new ReactiveProperty<int>();

    [Header("자동 플레이")]
    [SerializeField] private bool autoPlay = false;
    [SerializeField, ReadOnly] private int updateMapCount = 5;
    [SerializeField, ReadOnly] private float teachTime;
    [SerializeField, ReadOnly] private float dragTime;

    [Header("보스 스테이지")]
    [SerializeField] private GameObject normalBossObj;
    [SerializeField] private GameObject angryBossObj;
    [SerializeField] private GameObject cryBossObj;
    [SerializeField] private Image bossHpImage;
    [SerializeField] private CHText bossHpText;
    [SerializeField] private CHText hpText;

    [Header("스테이지별 UI 오브젝트")]
    [SerializeField] private GameObject onlyNormalStageObject;
    [SerializeField] private GameObject onlyBossStageObject;

    [Header("폭탄 선택 UI")]
    [SerializeField, ReadOnly] private int arrowPangIndex = 1;
    [SerializeField] private CHButton arrowPang1;
    [SerializeField] private CHButton arrowPang2;
    [SerializeField] private Image banView;

    [Header("가이드")]
    [SerializeField] private bool guideEnd = false;
    [SerializeField] private RectTransform guideFinger;
    [SerializeField] private RectTransform guideHole;
    [SerializeField] private GameObject guideBackground;
    [SerializeField] private Button guideBackgroundBtn;
    [SerializeField] private List<RectTransform> normalStageGuideHoleList = new List<RectTransform>();
    [SerializeField] private List<RectTransform> bossStageGuideHoleList = new List<RectTransform>();
    [SerializeField] private CHText guideDesc;

    private Dictionary<EBlockState, Sprite> _blockSpriteList = new Dictionary<EBlockState, Sprite>();
    private StageInfo _stageInfo;
    private List<StageBlockInfo> _stageBlockInfoList = new List<StageBlockInfo>();
    private ESelectStage _selectStage = ESelectStage.Hard;
    private Data.Login _loginData;

    private CancellationTokenSource _tokenSource;
    private float _gameTime = 0;
    private int _helpTime = 0;
    private bool _tutorialNextBlock = false;
    private bool _init = false;

    private GPBoard _board;
    private GPMatchChecker _matcher;
    private GPBombResolver _bombResolver;
    private GPTutorial _tutorial;
    private GPBossController _boss;

    private async void Start()
    {
        InitUI();
        BindUI();

        await LoadImage();
        InitData();
        await CreateMap();
        await _tutorial.StartGuide(_selectStage, _loginData);
        _tutorial.StartTutorial(_stageInfo, _stageBlockInfoList, _selectStage);
    }

    private async void Update()
    {
        if (_init == false)
            return;

        if (gameResult.Value == EGameState.GameClearWait) { GameEnd(true); return; }
        if (gameResult.Value == EGameState.GameOverWait) { GameEnd(false); return; }

        if (gameResult.Value == EGameState.NormalOrHardStagePlay)
        {
            bool clear = true;
            bool useTime = _stageInfo.time > 0;
            bool useTargetScore = _stageInfo.targetScore > 0;
            bool useMoveCount = _stageInfo.moveCount > 0;

            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    if (boardArr[i, j].GetBlockState() == EBlockState.RainbowPang) continue;
                    if (!boardArr[i, j].checkHp) continue;
                    if (boardArr[i, j].GetHp() > 0 || boardArr[i, j].IsFishBlock() || boardArr[i, j].IsBallBlock())
                    { clear = false; break; }
                }
                if (!clear) break;
            }

            if (useTime && timerImg.fillAmount >= 1)
            {
                if (useTargetScore && curScore.Value < _stageInfo.targetScore) clear = false;
                GameEnd(clear);
            }
            else
            {
                if (useTargetScore && curScore.Value < _stageInfo.targetScore) clear = false;
                if (useMoveCount && moveCount.Value <= 0) GameEnd(clear);
                else if (clear) GameEnd(clear);
            }
        }
        else if (gameResult.Value == EGameState.BossStagePlay)
        {
            if (_boss != null && _boss.hp.Value <= 0) { GameEnd(bossHpImage.fillAmount <= 0); return; }
            if (bossHpImage.fillAmount <= 0) { GameEnd(true); return; }
        }

        _gameTime += Time.deltaTime;

        if (!isLock)
        {
            curTimer += Time.deltaTime;
            timerImg.fillAmount = curTimer / _stageInfo.time;

            if (curTimer >= _helpTime)
            {
                if (_stageInfo.time >= _helpTime)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.SetText(_stageInfo.time - _helpTime);
                    ++_helpTime;
                }
                else timerText.gameObject.SetActive(false);
            }
        }

        if (isLock) { teachTime = _gameTime; dragTime = _gameTime; }
        else
        {
            if (autoPlay && dragTime + .5f < _gameTime)
                boardArr[_matcher.canMatchRow, _matcher.canMatchCol].Drag(_matcher.canMatchDrag);

            if (teachTime + 3 < _gameTime && !oneTimeAlarm && _matcher.canMatchRow >= 0 && _matcher.canMatchCol >= 0)
            {
                oneTimeAlarm = true;
                try
                {
                    var block = boardArr[_matcher.canMatchRow, _matcher.canMatchCol];
                    block.transform.DOScale(1.5f, 0.25f).OnComplete(() => block.transform.DOScale(1f, 0.25f));
                    await Task.Delay(3000, _tokenSource.Token);
                }
                catch (TaskCanceledException) { }
                oneTimeAlarm = false;
            }
        }
    }

    private void OnDestroy()
    {
        _tokenSource?.Cancel();
    }

    private void OnApplicationQuit()
    {
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    private void InitUI()
    {
        bonusScoreText.gameObject.SetActive(false);
        guideFinger.gameObject.SetActive(false);
        guideBackground.SetActive(false);
        guideHole.gameObject.SetActive(false);
        onlyNormalStageObject.SetActive(true);
        onlyBossStageObject.SetActive(false);
        guideBackgroundBtn.gameObject.SetActive(false);

        foreach (var h in normalStageGuideHoleList)
        {
            h.gameObject.SetActive(false);
        }

        foreach (var h in bossStageGuideHoleList)
        {
            h.gameObject.SetActive(false);
        }
    }

    private void BindUI()
    {
        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                _tokenSource?.Cancel();
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMUI.Instance.CloseUI(EUI.UIAlarm);
                CHMPool.Instance.Clear();
                LBLobbyScene.fromGame = true;
                SceneManager.LoadScene(1);
            });
        }

        if (arrowPang1 && arrowPang2)
        {
            arrowPang1.button.OnClickAsObservable().Subscribe(_ =>
            {
                arrowPangIndex = 1;
                _bombResolver?.SetArrowPangIndex(1);
                banView.rectTransform.DOAnchorPosX(arrowPang2.rectTransform.anchoredPosition.x, .5f);
            });
            arrowPang2.button.OnClickAsObservable().Subscribe(_ =>
            {
                arrowPangIndex = 2;
                _bombResolver?.SetArrowPangIndex(2);
                banView.rectTransform.DOAnchorPosX(arrowPang1.rectTransform.anchoredPosition.x, .5f);
            });
        }

        curScore.Subscribe(_ => curScoreText.SetText(_));
        moveCount.Subscribe(_ => moveCountText.SetText(_));
    }

    private async Task LoadImage()
    {
        var tasks = new List<Task>();
        for (EBlockState i = 0; i < EBlockState.Max; ++i)
        {
            if ((int)i >= 7 && (int)i <= 9) continue;
            var blockState = i;
            var tcs = new TaskCompletionSource<Sprite>();
            CHMResource.Instance.LoadSprite(blockState, sprite =>
            {
                if (sprite != null) _blockSpriteList[blockState] = sprite;
                tcs.SetResult(sprite);
            });
            tasks.Add(tcs.Task);
        }
        await Task.WhenAll(tasks);
    }

    private void InitData()
    {
        if (_init) return;
        _init = true;

        _tokenSource = new CancellationTokenSource();
        _loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        _selectStage = (ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage);

        int stage = 0;
        switch (_selectStage)
        {
            case ESelectStage.Hard:   stage = PlayerPrefs.GetInt(CHMString.Instance.HardStage); break;
            case ESelectStage.Boss:   stage = PlayerPrefs.GetInt(CHMString.Instance.BossStage); break;
            case ESelectStage.Normal: stage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage); break;
        }

        _stageInfo = CHMJson.Instance.GetStageInfo(stage);
        _stageBlockInfoList = CHMJson.Instance.GetStageBlockInfoList(stage);
        boardSize = _stageInfo.boardSize;

        switch (_selectStage)
        {
            case ESelectStage.Hard:
                _stageInfo.tutorialID = -1;
                foreach (var b in _stageBlockInfoList) b.tutorialBlock = false;
                break;
            case ESelectStage.Normal:
                _stageInfo.time = -1;
                if (_stageInfo.targetScore > 0) _stageInfo.targetScore /= 2;
                else if (_stageInfo.moveCount > 0) _stageInfo.moveCount *= 2;
                break;
        }

        targetScoreText.SetText(_stageInfo.targetScore);
        if (_stageInfo.targetScore < 0) targetScoreText.gameObject.SetActive(false);

        var addMoveItemValue = (int)CHMJson.Instance.GetConstValueInfo(EConstValue.AddMoveItemValue);
        var addTimeItemValue = (int)CHMJson.Instance.GetConstValueInfo(EConstValue.AddTimeItemValue);

        if (_stageInfo.moveCount > 0) moveCount.Value = _stageInfo.moveCount + _loginData.useMoveItemCount * addMoveItemValue;
        else
        {
            moveCount.Value = 99;
            if (_loginData.useMoveItemCount > 0) _loginData.addMoveItemCount += _loginData.useMoveItemCount;
        }

        if (_stageInfo.time > 0) _stageInfo.time += _loginData.useTimeItemCount * addTimeItemValue;
        else if (_loginData.useTimeItemCount > 0) _loginData.addTimeItemCount += _loginData.useTimeItemCount;

        gameResult.Value = _selectStage == ESelectStage.Boss ? EGameState.BossStagePlay : EGameState.NormalOrHardStagePlay;

        // GP 클래스 초기화
        _board = new GPBoard();
        _board.Init(boardArr, boardSize, _blockSpriteList, delay, delayMillisecond, _tokenSource.Token);

        _matcher = new GPMatchChecker();
        _matcher.Init(_board, _stageInfo.blockTypeCount);

        _bombResolver = new GPBombResolver();
        _bombResolver.Init(
            _board, _matcher,
            () => AfterDrag(null, null, true),
            score => bonusScore.Value += score,
            CreateEffect,
            sound => CHMSound.Instance.Play(sound),
            pangEffectList, bombEffectPS,
            _tokenSource.Token);
        _bombResolver.SetArrowPangIndex(arrowPangIndex);

        _tutorial = new GPTutorial();
        _tutorial.Init(_board, guideFinger, guideHole, guideBackground, guideBackgroundBtn,
            normalStageGuideHoleList, bossStageGuideHoleList, guideDesc);

        if (_selectStage == ESelectStage.Boss)
        {
            onlyBossStageObject.SetActive(true);
            onlyNormalStageObject.SetActive(false);
            normalBossObj.SetActive(true);
            angryBossObj.SetActive(false);
            cryBossObj.SetActive(false);

            _boss = new GPBossController();
            _boss.Init(_board, _stageInfo, _loginData, bossHpImage, bossHpText, hpText,
                normalBossObj, angryBossObj, cryBossObj, curScore, this);
        }
    }

    private async Task CreateMap()
    {
        instBtn.InstantiateButton(origin, margin, boardSize, boardSize, parent, boardArr);

        foreach (var block in boardArr)
        {
            if (block == null) continue;
            float moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2;
            block.originPos.x -= moveDis;
            block.SetOriginPos();
            block.rectTransform.DOScale(1f, delay);

            var info = _stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
            if (info == null)
            {
                var random = (EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                random = block.CheckSelectCatShop(random);
                block.SetBlockState(ELog.CreateMap, 1, _blockSpriteList[random], random);
                block.checkHp = block.CheckHpBlock();
                block.SetHp(-1);
            }
            else
            {
                var blockState = block.CheckSelectCatShop(info.blockState);
                block.SetBlockState(ELog.CreateMap, 2, _blockSpriteList[blockState], blockState);
                block.checkHp = block.CheckHpBlock();
                block.tutorialBlock = info.tutorialBlock;
                block.SetHp(block.IsNormalBlock() ? -1 : info.hp);
            }
        }

        isMatch = false;
        _matcher.CheckMap();

        bool canMatch = true;
        do
        {
            if (!canMatch)
            {
                foreach (var block in boardArr)
                {
                    if (block == null) continue;
                    var info = _stageBlockInfoList.Find(_ => _.row == block.row && _.col == block.col);
                    if (info == null)
                    {
                        var random = (EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(ELog.CreateMap, 3, _blockSpriteList[random], random);
                        block.SetHp(-1);
                    }
                    else
                    {
                        var blockState = block.CheckSelectCatShop(info.blockState);
                        block.SetBlockState(ELog.CreateMap, 4, _blockSpriteList[blockState], blockState);
                        block.tutorialBlock = info.tutorialBlock;
                        block.SetHp(block.IsNormalBlock() ? -1 : info.hp);
                    }
                }
            }

            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    var block = boardArr[i, j];
                    if (block == null) continue;
                    if (block.squareMatch || block.IsMatch())
                    {
                        var random = (EBlockState)UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        random = block.CheckSelectCatShop(random);
                        block.SetBlockState(ELog.CreateMap, 5, _blockSpriteList[random], random);
                        block.SetHp(-1); block.ResetScore(); block.match = false; block.squareMatch = false;
                    }
                }
            }

            isMatch = false;
            _matcher.CheckMap();
            if (!_matcher.isMatch) canMatch = _matcher.CanPlay();
            _matcher.isMatch = false;

        } while (_matcher.isMatch || !canMatch);

        Debug.Log("Create Map End");
        await Task.Delay((int)(delay * delayMillisecond), _tokenSource.Token);
    }

    private async Task UpdateMap()
    {
        try
        {
            int count = 0;
            bool reUpdate = false;
            bool createDelay = false;
            int firstRow = 0, firstCol = 0;

            do
            {
                foreach (var block in boardArr)
                {
                    if (block == null) continue;
                    if (block.changeBlockState != EBlockState.None)
                    {
                        createDelay = true;
                        _board.CreateNewBlock(block, ELog.UpdateMap, 1, block.changeBlockState);
                        block.SetHp(block.changeHp); block.ResetScore(); block.SetOriginPos();
                        block.changeBlockState = EBlockState.None;
                    }
                    else if (reUpdate || block.IsMatch())
                    {
                        if (block.IsFixdBlock() || block.IsFishBlock()) continue;
                        if (reUpdate && (block.GetBlockState() == EBlockState.RainbowPang || block.IsBallBlock())) continue;
                        firstRow = block.row; firstCol = block.col;
                        var random = UnityEngine.Random.Range(0, _stageInfo.blockTypeCount);
                        createDelay = true;
                        _board.CreateNewBlock(block, ELog.UpdateMap, 2, (EBlockState)random);
                        block.SetHp(-1); block.ResetScore(); block.SetOriginPos();
                    }
                }

                reUpdate = !_matcher.CanPlay();
                if (reUpdate) CHMUI.Instance.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 56 });
                if (createDelay) await Task.Delay((int)(delay * delayMillisecond), _tokenSource.Token);

                if (count++ > updateMapCount)
                {
                    CHMUI.Instance.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 80 });
                    _board.CreateNewBlock(boardArr[firstRow, firstCol], ELog.UpdateMap, 3, EBlockState.YellowBomb);
                    break;
                }
            } while (reUpdate);

            _matcher.isMatch = false;
        }
        catch (TaskCanceledException) { Debug.Log("Cancel Update Map"); }
    }

    private async Task RemoveMatchBlock()
    {
        bool removeDelay = false;
        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block == null) continue;
                if (block.IsMatch() && !block.remove)
                {
                    _matcher.CheckArround(block.row, block.col);
                    curScore.Value += 1;
                    removeDelay = true;
                    block.remove = true;
                    block.rectTransform.DOScale(0f, delay);

                    Image img = _selectStage == ESelectStage.Boss
                        ? catFootImgList[UnityEngine.Random.Range(0, catFootImgList.Count)]
                        : goldImg;

                    var gold = CHMResource.Instance.Instantiate(img.gameObject, transform.parent);
                    if (gold != null)
                    {
                        var rect = gold.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            // 도착지는 씬에 배치된 goldImgTarget을 사용 (goldImg는 프리팹 템플릿이라 .position이 의미 없음).
                            rect.position = block.rectTransform.position;
                            var destPos = goldImgTarget != null ? goldImgTarget.position : img.rectTransform.position;
                            rect.DOMove(destPos, UnityEngine.Random.Range(.4f, .8f)).OnComplete(() =>
                                CHMResource.Instance.Destroy(gold));
                        }
                    }

                    if (block.IsBombBlock() && !block.boom)
                    {
                        bonusScore.Value += 20;
                        await block.Bomb(false);
                        i = -1; break;
                    }
                }
            }
        }

        if (removeDelay)
        {
            CHMSound.Instance.Play(ESound.Ppauk);
            await Task.Delay((int)(delay * delayMillisecond), _tokenSource.Token);
        }
    }

    private void SetDissapearBlock()
    {
        int row = boardSize - 1;
        for (int i = 0; i < boardSize; ++i)
        {
            var block = boardArr[row, i];
            if (block.IsFishBlock())
            {
                block.tutorialBlock = false;
                block.changeBlockState = (EBlockState)UnityEngine.Random.Range((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
            }
            else if (block.IsBallBlock())
            {
                block.tutorialBlock = false;
                block.changeBlockState = EBlockState.Potal;
                block.changeHp = 5;

                int ballHp = 4;
                for (int k = i + 1; k < boardSize; ++k)
                {
                    var cb = boardArr[row, k];
                    if (ballHp <= 0) break;
                    if (cb.IsNormalBlock() || cb.remove) { cb.changeBlockState = EBlockState.Potal; cb.changeHp = ballHp--; cb.checkHp = true; }
                    else break;
                }
                ballHp = 4;
                for (int k = i - 1; k >= 0; --k)
                {
                    var cb = boardArr[row, k];
                    if (cb.IsNormalBlock() || cb.remove) { cb.changeBlockState = EBlockState.Potal; cb.changeHp = ballHp--; cb.checkHp = true; }
                    else break;
                }
            }
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
                if (block == null || !block.IsBoxBlock()) continue;
                var upBlock = _board.IsValidIndex(w - 1, h) ? boardArr[w - 1, h] : null;
                if (upBlock == null) continue;
                if (block.CatInTheBox(upBlock.GetBlockState()))
                {
                    inDelay = true;
                    upBlock.remove = true; upBlock.match = true;
                    upBlock.rectTransform.DOAnchorPosY(block.rectTransform.anchoredPosition.y, delay);
                    upBlock.rectTransform.DOScale(0f, delay);
                }
            }
        }
        if (inDelay) { await Task.Delay((int)(delay * delayMillisecond), _tokenSource.Token); return true; }
        return false;
    }

    private RectTransform CreateEffect(ParticleSystem effect, Vector2 movePos)
    {
        var copyObj = CHMResource.Instance.Instantiate(effect.gameObject, transform.parent);
        copyObj.SetActive(true);
        var rt = copyObj.GetComponent<RectTransform>();
        rt.anchoredPosition = movePos;
        return rt;
    }

    public async Task AfterDrag(Block block1, Block block2, bool isBoom = false)
    {
        bool checkCreateBlock = false;

        if (moveCount.Value == 0 && gameResult.Value != EGameState.CatPang) return;

        Time.timeScale = 1;
        _tutorial.HideGuide();
        isLock = true;

        await Task.Delay((int)(delay * delayMillisecond), _tokenSource.Token);

        if (block1 && block2 && block1.IsBlock() && block2.IsBlock())
        {
            if (block1.tutorialBlock) block1.tutorialBlock = false;
            if (block2.tutorialBlock) block2.tutorialBlock = false;

            moveIndex1 = block1.index;
            moveIndex2 = block2.index;
            _matcher.SetMoveIndices(moveIndex1, moveIndex2);

            if (block1.IsSpecialBombBlock() && block2.IsSpecialBombBlock())
            { block1.match = true; block2.match = true; await _bombResolver.BoomAll(); isLock = false; return; }
            else if (block1.GetBlockState() == EBlockState.PinkBomb)
            { await _bombResolver.Boom3(block1, block2.GetBlockState()); isLock = false; return; }
            else if (block2.GetBlockState() == EBlockState.PinkBomb)
            { await _bombResolver.Boom3(block2, block1.GetBlockState()); isLock = false; return; }
            else if (block1.IsSpecialBombBlock())
            { await block1.Bomb(); isLock = false; return; }
            else if (block2.IsSpecialBombBlock())
            { await block2.Bomb(); isLock = false; return; }
            else if (block1.IsBombBlock() && block2.IsBombBlock())
            {
                moveCount.Value -= 1;
                bonusScore.Value += 30;
                block2.match = true;
                block1.changeBlockState = (EBlockState)UnityEngine.Random.Range((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
            }
            else if (block1.IsBombBlock()) { await block1.Bomb(); isLock = false; return; }
            else if (block2.IsBombBlock()) { await block2.Bomb(); isLock = false; return; }
        }

        bool back = false;
        _matcher.isMatch = false;
        _matcher.CheckMap();

        if (block1 != null && block2 != null && !_matcher.isMatch)
        {
            _board.ChangeBlock(block1, block2);
            block1.rectTransform.DOAnchorPos(block1.originPos, delay);
            block2.rectTransform.DOAnchorPos(block2.originPos, delay);
            await Task.Delay((int)(delay * delayMillisecond), _tokenSource.Token);
            back = true;
        }

        do
        {
            await RemoveMatchBlock();
            await _bombResolver.CreateBombBlock(moveIndex1, moveIndex2);
            await _board.DownBlock();

            if (_selectStage == ESelectStage.Boss) bonusScore.Value += _loginData.attack;

            curScore.Value += bonusScore.Value;
            if (bonusScore.Value > 0)
            {
                bonusScoreText.gameObject.SetActive(true);
                bonusScoreText.SetText(bonusScore.Value);
                await Task.Delay((int)(delay * delayMillisecond));
                bonusScoreText.gameObject.SetActive(false);
            }
            bonusScore.Value = 0;

            if (!checkCreateBlock)
            {
                checkCreateBlock = true;
                _bombResolver.BlockCreatorBlock(EBlockState.WallCreator, EBlockState.Wall);
                _bombResolver.BlockCreatorBlock(EBlockState.PotalCreator, EBlockState.Potal);
            }

            SetDissapearBlock();
            await UpdateMap();
            _matcher.isMatch = false;
            _matcher.CheckMap();

        } while (_matcher.isMatch || await CatInTheBox());

        bool validDrag = block1 != null && block2 != null && !back;
        if (validDrag || (isBoom && gameResult.Value != EGameState.CatPang))
            moveCount.Value -= 1;

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
            if (!_tutorialNextBlock)
            {
                _tutorialNextBlock = true;
                if (_selectStage != ESelectStage.Hard && _stageInfo.tutorialID > 0)
                {
                    var tutInfo = CHMJson.Instance.GetTutorialInfo(_stageInfo.tutorialID);
                    if (tutInfo == null || tutInfo.connectNextBlock == EBlockState.None) break;
                    guideBackground.SetActive(true);
                    guideHole.gameObject.SetActive(true);
                    var sv = _tutorial.TutorialBlockSetting(tutInfo.connectNextBlock);
                    guideHole.sizeDelta = sv.Item1;
                    guideHole.anchoredPosition = sv.Item2;
                    guideFinger.gameObject.SetActive(true);
                    guideFinger.anchoredPosition = sv.Item2;
                    guideDesc.SetStringID(tutInfo.descNextBlockStringID);
                }
            }
        } while (false);

        isLock = false;
    }

    private async void GameEnd(bool clear)
    {
        if (isLock) { gameResult.Value = clear ? EGameState.GameClearWait : EGameState.GameOverWait; return; }
        if (gameEnd) return;
        gameEnd = true;

        gameResult.Value = clear ? EGameState.GameClear : EGameState.GameOver;

        if (clear && _selectStage == ESelectStage.Boss) _boss.OnClear();

        if (await _bombResolver.CatPang(true))
        {
            gameResult.Value = EGameState.CatPang;
            CHMUI.Instance.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 55, closeTime = 1 });
            await Task.Delay(1000);
            await _bombResolver.CatPang();
            gameEnd = false;
            gameResult.Value = _selectStage == ESelectStage.Boss ? EGameState.BossStagePlay : EGameState.NormalOrHardStagePlay;
            return;
        }

        CHMUI.Instance.ShowUI(EUI.UIGameEnd, new UIGameEndArg
        {
            clearState = GetClearState(),
            result = gameResult.Value,
            gold = curScore.Value
        });

        if (clear) SaveClearData();
    }

    private void SaveClearData()
    {
        switch (_selectStage)
        {
            case ESelectStage.Hard:
                if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).hardStage < PlayerPrefs.GetInt(CHMString.Instance.HardStage))
                {
                    CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).hardStage = PlayerPrefs.GetInt(CHMString.Instance.HardStage);
#if UNITY_ANDROID && !UNITY_EDITOR
                    ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_hard_stage_rank, PlayerPrefs.GetInt(CHMString.Instance.HardStage));
#endif
                }
                break;
            case ESelectStage.Boss:
                if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).bossStage < PlayerPrefs.GetInt(CHMString.Instance.BossStage))
                {
                    CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).bossStage = PlayerPrefs.GetInt(CHMString.Instance.BossStage);
#if UNITY_ANDROID && !UNITY_EDITOR
                    ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_boss_stage_rank, PlayerPrefs.GetInt(CHMString.Instance.BossStage));
#endif
                }
                break;
            case ESelectStage.Normal:
                if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage < PlayerPrefs.GetInt(CHMString.Instance.NormalStage))
                {
                    CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage);
#if UNITY_ANDROID && !UNITY_EDITOR
                    ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_normal_stage_rank, PlayerPrefs.GetInt(CHMString.Instance.NormalStage));
#endif
                }
                break;
        }
    }

    private EClearState GetClearState()
    {
        switch (_selectStage)
        {
            case ESelectStage.Hard:   if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).hardStage >= PlayerPrefs.GetInt(CHMString.Instance.HardStage)) return EClearState.Clear; break;
            case ESelectStage.Boss:   if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).bossStage >= PlayerPrefs.GetInt(CHMString.Instance.BossStage)) return EClearState.Clear; break;
            case ESelectStage.Normal: if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage >= PlayerPrefs.GetInt(CHMString.Instance.NormalStage)) return EClearState.Clear; break;
        }
        return EClearState.Doing;
    }

    // Block.cs 에서 호출하는 public 파사드
    public void ChangeBlock(Block a, Block b) => _board.ChangeBlock(a, b);
    public bool CheckTutorial() => _tutorial.CheckTutorial();
    public async Task Bomb1(Block b, bool ani = true) => await _bombResolver.Bomb1(b, ani);
    public async Task Bomb2(Block b, bool ani = true) => await _bombResolver.Bomb2(b, ani);
    public async Task Boom3(Block b, EBlockState s, bool ani = true) => await _bombResolver.Boom3(b, s, ani);
    public async Task Bomb4(Block b, bool ani = true) => await _bombResolver.Bomb4(b, ani);
    public async Task Bomb5(Block b, bool ani = true) => await _bombResolver.Bomb5(b, ani);
    public async Task Bomb6(Block b, bool ani = true) => await _bombResolver.Bomb6(b, ani);
    public async Task Bomb7(Block b, bool ani = true) => await _bombResolver.Bomb7(b, ani);
    public async Task Bomb8(Block b, bool ani = true) => await _bombResolver.Bomb8(b, ani);
    public async Task Bomb9(Block b, bool ani = true) => await _bombResolver.Bomb9(b, ani);
    public async Task Bomb10(Block b, bool ani = true) => await _bombResolver.Bomb10(b, ani);
    public async Task Bomb11(Block b, bool ani = true) => await _bombResolver.Bomb11(b, ani);
    public async Task Bomb12(Block b, bool ani = true) => await _bombResolver.Bomb12(b, ani);
    public async Task RainbowPang(Block b, bool ani = true) => await _bombResolver.RainbowPang(b, ani);
    public async Task BoomAll(bool ani = true) => await _bombResolver.BoomAll(ani);
}
