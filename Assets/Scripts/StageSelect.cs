using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    [SerializeField]
    List<CHButton> btnList = new List<CHButton>();

    public void Init()
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            int index = i;
            btnList[index].button.OnClickAsObservable().Subscribe(_ =>
            {
                PlayerPrefs.SetInt("stage", int.Parse(btnList[index].text.text));
                SceneManager.LoadScene(1);
            });

            SetClearObj(index);
            SetLockObj(index);
        }
    }

    public bool SetPage(int _page)
    {
        var stageList = CHMMain.Json.GetStageInfoList(_page);
        if (stageList == null || stageList.Count == 0)
            return false;

        var stageCount = stageList.Count;

        for (int i = 0; i < btnList.Count; ++i)
        {
            if (i >= stageCount)
            {
                btnList[i].gameObject.SetActive(false);
                continue;
            }

            btnList[i].gameObject.SetActive(true);
            btnList[i].text.text = stageList[i].stage.ToString();

            SetClearObj(i);
            SetLockObj(i);
        }

        return true;
    }

    void SetClearObj(int _index)
    {
        if (int.TryParse(btnList[_index].text.text, out int stage) == false)
            return;

        if (CHMData.Instance.stageDataDic.TryGetValue(stage.ToString(), out var data))
        {
            if (data.clear)
            {
                btnList[_index].clearObj.SetActive(true);
            }
            else
            {
                btnList[_index].clearObj.SetActive(false);
            }
        }
        else
        {
            btnList[_index].clearObj.SetActive(false);
        }
    }

    async void SetLockObj(int _index)
    {
        if (int.TryParse(btnList[_index].text.text, out int stage) == false)
            return;

        btnList[_index].button.interactable = false;

        if (stage == 1)
        {
            btnList[_index].button.interactable = true;
            btnList[_index].lockObj.SetActive(false);
            btnList[_index].unlockObj.SetActive(false);
            return;
        }

        if (CHMData.Instance.stageDataDic.TryGetValue((stage - 1).ToString(), out var data))
        {
            if (data.clear)
            {
                if (CHMData.Instance.stageDataDic.TryGetValue(stage.ToString(), out var data2) == false)
                {
                    return;
                }

                if (PlayerPrefs.GetInt("stage") == stage - 1 && data2.clear == false)
                {
                    btnList[_index].lockObj.SetActive(true);
                    await Task.Delay(1000);

                    if (btnList[_index] == null)
                        return;

                    btnList[_index].lockObj.SetActive(false);
                    btnList[_index].unlockObj.SetActive(true);
                    var rectTransform = btnList[_index].unlockObj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 30f, 1f).OnComplete(() =>
                        {
                            btnList[_index].button.interactable = true;
                            btnList[_index].unlockObj.SetActive(false);
                        });
                    }
                }
                else
                {
                    btnList[_index].button.interactable = true;
                    btnList[_index].lockObj.SetActive(false);
                }
            }
            else
            {
                btnList[_index].lockObj.SetActive(true);
            }
        }
        else
        {
            btnList[_index].lockObj.SetActive(true);
        }
    }
}
