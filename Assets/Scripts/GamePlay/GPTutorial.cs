using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPTutorial
{
    GPBoard _board;
    RectTransform _guideFinger;
    RectTransform _guideHole;
    GameObject _guideBackground;
    Button _guideBackgroundBtn;
    List<RectTransform> _normalGuideHoleList;
    List<RectTransform> _bossGuideHoleList;
    CHTMPro _guideDesc;
    bool _guideEnd;

    public void Init(
        GPBoard board,
        RectTransform guideFinger,
        RectTransform guideHole,
        GameObject guideBackground,
        Button guideBackgroundBtn,
        List<RectTransform> normalGuideHoleList,
        List<RectTransform> bossGuideHoleList,
        CHTMPro guideDesc)
    {
        _board = board;
        _guideFinger = guideFinger;
        _guideHole = guideHole;
        _guideBackground = guideBackground;
        _guideBackgroundBtn = guideBackgroundBtn;
        _normalGuideHoleList = normalGuideHoleList;
        _bossGuideHoleList = bossGuideHoleList;
        _guideDesc = guideDesc;
    }

    public async Task StartGuide(ESelectStage selectStage, Data.Login loginData)
    {
        if (selectStage == ESelectStage.Normal &&
            loginData.guideIndex == (int)CHMMain.Json.GetConstValueInfo(EConstValue.NormalStageGuideMaxIndex))
        {
            Time.timeScale = 0;
            _guideBackground.SetActive(true);
            _guideBackground.transform.SetAsLastSibling();
            _guideBackgroundBtn.gameObject.SetActive(true);
            _guideBackgroundBtn.transform.SetAsLastSibling();

            loginData.guideIndex += await NormalStageGuideStart();

            _guideBackground.SetActive(false);
            _guideBackgroundBtn.gameObject.SetActive(false);
            Time.timeScale = 1;
            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }

        if (selectStage == ESelectStage.Boss &&
            loginData.guideIndex == (int)CHMMain.Json.GetConstValueInfo(EConstValue.BossStageGuideMaxIndex))
        {
            Time.timeScale = 0;
            _guideBackground.SetActive(true);
            _guideBackground.transform.SetAsLastSibling();
            _guideBackgroundBtn.gameObject.SetActive(true);
            _guideBackgroundBtn.transform.SetAsLastSibling();

            loginData.guideIndex += await BossStageGuideStart();

            _guideBackground.SetActive(false);
            _guideBackgroundBtn.gameObject.SetActive(false);
            Time.timeScale = 1;
            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }
    }

    public void StartTutorial(StageInfo stageInfo, List<StageBlockInfo> stageBlockInfoList, ESelectStage selectStage)
    {
        if (selectStage == ESelectStage.Hard || stageInfo.tutorialID <= 0) return;

        _guideEnd = false;
        _guideFinger.gameObject.SetActive(true);
        Time.timeScale = 0;
        _guideBackground.SetActive(true);
        _guideHole.gameObject.SetActive(true);
        _guideHole.SetAsLastSibling();
        _guideBackground.transform.SetAsLastSibling();
        _guideFinger.transform.SetAsLastSibling();

        var holeValue = GetTutorialStageImgSettingValue(stageBlockInfoList);
        _guideHole.sizeDelta = holeValue.Item1;
        _guideHole.anchoredPosition = holeValue.Item2;

        var tutorialInfo = CHMMain.Json.GetTutorialInfo(stageInfo.tutorialID);
        if (tutorialInfo != null)
            _guideDesc.SetStringID(tutorialInfo.descStringID);
    }

    public void HideGuide()
    {
        _guideBackground.SetActive(false);
        _guideHole.gameObject.SetActive(false);
        _guideFinger.gameObject.SetActive(false);
        _guideEnd = true;
    }

    public bool CheckTutorial()
    {
        for (int w = 0; w < _board.boardSize; w++)
            for (int h = 0; h < _board.boardSize; h++)
                if (_board.boardArr[w, h].tutorialBlock)
                    return true;
        return false;
    }

    public (Vector2, Vector2) TutorialBlockSetting(EBlockState blockState)
    {
        var arr = _board.boardArr;
        for (int w = 0; w < _board.boardSize; w++)
            for (int h = 0; h < _board.boardSize; h++)
                if (arr[w, h].GetBlockState() == blockState)
                {
                    arr[w, h].tutorialBlock = true;
                    return (arr[w, h].rectTransform.sizeDelta, arr[w, h].rectTransform.anchoredPosition);
                }
        return (Vector2.zero, Vector2.zero);
    }

    async Task<int> NormalStageGuideStart()
    {
        _guideBackground.SetActive(true);
        for (int i = 0; i < _normalGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 1 + (int)CHMMain.Json.GetConstValueInfo(EConstValue.NormalStageGuideMaxIndex));
            if (guideInfo == null) break;
            _normalGuideHoleList[i].gameObject.SetActive(true);
            _guideDesc.SetStringID(guideInfo.descStringID);

            var buttonTask = new TaskCompletionSource<bool>();
            var sub = _guideBackgroundBtn.OnClickAsObservable().Subscribe(_ => buttonTask.SetResult(true));
            await buttonTask.Task;
            _normalGuideHoleList[i].gameObject.SetActive(false);
            sub.Dispose();
        }
        return _normalGuideHoleList.Count;
    }

    async Task<int> BossStageGuideStart()
    {
        _guideBackground.SetActive(true);
        for (int i = 0; i < _bossGuideHoleList.Count; ++i)
        {
            var guideInfo = CHMMain.Json.GetGuideInfo(i + 1 + (int)CHMMain.Json.GetConstValueInfo(EConstValue.BossStageGuideMaxIndex));
            if (guideInfo == null) break;
            _bossGuideHoleList[i].gameObject.SetActive(true);
            _guideDesc.SetStringID(guideInfo.descStringID);

            var buttonTask = new TaskCompletionSource<bool>();
            var sub = _guideBackgroundBtn.OnClickAsObservable().Subscribe(_ => buttonTask.SetResult(true));
            await buttonTask.Task;
            _bossGuideHoleList[i].gameObject.SetActive(false);
            sub.Dispose();
        }
        return _bossGuideHoleList.Count;
    }

    (Vector2, Vector2) GetTutorialStageImgSettingValue(List<StageBlockInfo> stageBlockInfoList)
    {
        if (stageBlockInfoList == null) return (Vector2.zero, Vector2.zero);
        var tutorialBlocks = stageBlockInfoList.FindAll(_ => _.tutorialBlock);
        if (tutorialBlocks.Count <= 0) return (Vector2.zero, Vector2.zero);

        var arr = _board.boardArr;
        float sizeX = 0, sizeY = 0, posX = 0, posY = 0;

        if (tutorialBlocks.Count == 1)
        {
            var b = arr[tutorialBlocks[0].row, tutorialBlocks[0].col];
            sizeX = b.rectTransform.sizeDelta.x;
            sizeY = b.rectTransform.sizeDelta.y;
            posX = b.rectTransform.anchoredPosition.x;
            posY = b.rectTransform.anchoredPosition.y;
            _guideFinger.anchoredPosition = new Vector2(posX, posY);
        }
        else
        {
            var b1 = arr[tutorialBlocks[0].row, tutorialBlocks[0].col];
            var b2 = arr[tutorialBlocks[1].row, tutorialBlocks[1].col];
            if (b1.row == b2.row)
            {
                sizeX = b1.rectTransform.sizeDelta.x * 2;
                sizeY = b1.rectTransform.sizeDelta.y;
                posX = (b1.rectTransform.anchoredPosition.x + b2.rectTransform.anchoredPosition.x) / 2f;
                posY = b1.rectTransform.anchoredPosition.y;
            }
            else
            {
                sizeX = b1.rectTransform.sizeDelta.x;
                sizeY = b1.rectTransform.sizeDelta.y * 2;
                posX = b1.rectTransform.anchoredPosition.x;
                posY = (b1.rectTransform.anchoredPosition.y + b2.rectTransform.anchoredPosition.y) / 2f;
            }
            FingerMoveRepeat(b1.rectTransform.anchoredPosition, b2.rectTransform.anchoredPosition);
        }
        return (new Vector2(sizeX, sizeY), new Vector2(posX, posY));
    }

    async Task FingerMoveRepeat(Vector2 startPos, Vector2 endPos)
    {
        float moveTime = 2f;
        while (!_guideEnd)
        {
            _guideFinger.anchoredPosition = startPos;
            float elapsed = 0f;
            while (elapsed < moveTime)
            {
                _guideFinger.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / moveTime);
                elapsed += 0.025f;
                await Task.Yield();
            }
            await Task.Delay(1000);
        }
    }
}
