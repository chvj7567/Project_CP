using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using static Defines;

public class StageSelect : MonoBehaviour
{
    [SerializeField]
    List<CHButton> btnList = new List<CHButton>();

    List<IDisposable> disposeList = new List<IDisposable>();

    public void Init(Defines.ESelectStage select)
    {
        Color color = Color.white;
        switch (select)
        {
            case ESelectStage.Hard:
                color = Color.cyan;
                break;
            case ESelectStage.Boss:
                color = Color.red;
                break;
            case ESelectStage.Normal:
                color = Color.white;
                break;
        }

        for (int i = 0; i < disposeList.Count; i++)
        {
            disposeList[i].Dispose();
        }

        disposeList.Clear();

        for (int i = 0; i < btnList.Count; i++)
        {
            int index = i;
            var btnDispose = btnList[index].button.OnClickAsObservable().Subscribe(_ =>
            {
                var selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);
                switch (selectStage)
                {
                    case ESelectStage.Hard:
                        {
                            PlayerPrefs.SetInt(CHMMain.String.HardStage, int.Parse(btnList[index].text.text));

                            CHMMain.UI.ShowUI(Defines.EUI.UIGameStart, new UIGameStartArg
                            {
                                stage = PlayerPrefs.GetInt(CHMMain.String.HardStage)
                            });
                        }
                        break;
                    case ESelectStage.Boss:
                        {
                            PlayerPrefs.SetInt(CHMMain.String.BossStage, int.Parse(btnList[index].text.text) + CHMData.Instance.BossStageStartValue);

                            CHMMain.UI.ShowUI(Defines.EUI.UIGameStart, new UIGameStartArg
                            {
                                stage = PlayerPrefs.GetInt(CHMMain.String.BossStage)
                            });
                        }
                        break;
                    case ESelectStage.Normal:
                        {
                            PlayerPrefs.SetInt(CHMMain.String.NormalStage, int.Parse(btnList[index].text.text));

                            CHMMain.UI.ShowUI(Defines.EUI.UIGameStart, new UIGameStartArg
                            {
                                stage = PlayerPrefs.GetInt(CHMMain.String.NormalStage)
                            });
                        }
                        break;
                }
            });

            disposeList.Add(btnDispose);

            btnList[index].image.color = color;

            SetClearObj(index);
            SetLockObj(index);
        }
    }

    public bool SetPage(int page, Defines.ESelectStage select)
    {
        Debug.Log($"SetPage : {select.ToString()} {page} ");

        if (select == Defines.ESelectStage.Boss)
        {
            page += CHMData.Instance.BossStageStartValue;
        }

        var stageList = CHMMain.Json.GetStageInfoList(page);
        if (stageList == null || stageList.Count == 0)
        {
            Debug.Log($"SetPage Failed : {page}");
            return false;
        }

        var stageCount = stageList.Count;

        for (int i = 0; i < btnList.Count; ++i)
        {
            if (i >= stageCount)
            {
                btnList[i].gameObject.SetActive(false);
                continue;
            }

            btnList[i].gameObject.SetActive(true);

            if (PlayerPrefs.GetInt(CHMMain.String.SelectStage) == (int)Defines.ESelectStage.Boss)
            {
                btnList[i].text.text = (stageList[i].stage - CHMData.Instance.BossStageStartValue).ToString();
            }
            else
            {
                btnList[i].text.text = stageList[i].stage.ToString();
            }

            SetClearObj(i);
            SetLockObj(i);
        }

        return true;
    }

    void SetClearObj(int index)
    {
        if (int.TryParse(btnList[index].text.text, out int stage) == false)
            return;

        int clearStage = GetClearStage();
        if (clearStage >= stage)
        {
            btnList[index].clearObj.SetActive(true);
        }
        else
        {
            btnList[index].clearObj.SetActive(false);
        }
    }

    async void SetLockObj(int index)
    {
        if (int.TryParse(btnList[index].text.text, out int stage) == false)
            return;

        btnList[index].button.interactable = false;
        
        if (stage == 1)
        {
            btnList[index].button.interactable = true;
            btnList[index].lockObj.SetActive(false);
            btnList[index].unlockObj.SetActive(false);
            return;
        }

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        var beforeStage = stage - 1;

        var lastPlayStage = GetLastPlayStage();
        int clearStage = GetClearStage();

        // 이전 스테이지를 클리어했다면
        if (clearStage >= beforeStage)
        {
            // 전 스테이지를 마지막으로 클리어하고 현 스테이지를 클리어하지 않은 상태라면
            if (lastPlayStage == beforeStage && clearStage < stage)
            {
                btnList[index].lockObj.SetActive(true);
                await Task.Delay(1000);

                if (btnList[index] == null)
                {
                    Debug.Log($"Task Null");
                    return;
                }

                btnList[index].lockObj.SetActive(false);
                btnList[index].unlockObj.SetActive(true);
                var rectTransform = btnList[index].unlockObj.GetComponent<RectTransform>();
                rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 30f, 1f).OnComplete(() =>
                {
                    btnList[index].button.interactable = true;
                    btnList[index].unlockObj.SetActive(false);
                });
            }
            else
            {
                btnList[index].button.interactable = true;
                btnList[index].lockObj.SetActive(false);
                btnList[index].unlockObj.SetActive(false);
            }
        }
        else
        {
            btnList[index].lockObj.SetActive(true);
            btnList[index].unlockObj.SetActive(false);
        }
    }

    public int GetLastPlayStage()
    {
        var lastPlayStage = 0;
        var selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);

        switch (selectStage)
        {
            case Defines.ESelectStage.Hard:
                lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.HardStage);
                break;
            case Defines.ESelectStage.Boss:
                lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.BossStage) - CHMData.Instance.BossStageStartValue;
                break;
            case Defines.ESelectStage.Normal:
                lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.NormalStage);
                break;
        }

        return lastPlayStage;
    }

    public int GetClearStage()
    {
        int clearStage = 0;
        var selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);

        switch (selectStage)
        {
            case Defines.ESelectStage.Hard:
                clearStage = CHMData.Instance.GetLoginData(CHMMain.String.CatPang).hardStage;
                break;
            case Defines.ESelectStage.Boss:
                clearStage = CHMData.Instance.GetLoginData(CHMMain.String.CatPang).bossStage - CHMData.Instance.BossStageStartValue;
                break;
            case Defines.ESelectStage.Normal:
                clearStage = CHMData.Instance.GetLoginData(CHMMain.String.CatPang).normalStage;
                break;
        }

        return clearStage;
    }
}
