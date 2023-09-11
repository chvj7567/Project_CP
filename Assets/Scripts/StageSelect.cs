using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    [SerializeField]
    List<Button> btnList = new List<Button>();
    [SerializeField]
    List<TMP_Text> textList = new List<TMP_Text>();
    [SerializeField]
    List<GameObject> clearObjList = new List<GameObject>();

    public void Init()
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            int index = i;
            btnList[index].OnClickAsObservable().Subscribe(_ =>
            {
                PlayerPrefs.SetInt("stage", int.Parse(textList[index].text));
                SceneManager.LoadScene(1);
            });

            SetClearObj(index);
        }
    }

    public bool SetPage(int _page)
    {
        var stageList = CHMMain.Json.GetStageInfoList(_page);
        if (stageList == null || stageList.Count == 0)
            return false;

        var stageCount = stageList.Count;

        for (int i = 0; i < textList.Count; ++i)
        {
            if (i >= stageCount)
            {
                btnList[i].gameObject.SetActive(false);
                clearObjList[i].gameObject.SetActive(false);
                continue;
            }

            btnList[i].gameObject.SetActive(true);
            textList[i].text = stageList[i].stage.ToString();

            SetClearObj(i);
        }

        return true;
    }

    void SetClearObj(int _index)
    {
        if (CHMMain.Data.stageDataDic.TryGetValue(textList[_index].text, out var data))
        {
            if (data.clear)
            {
                clearObjList[_index].SetActive(true);
            }
            else
            {
                clearObjList[_index].SetActive(false);
            }
        }
        else
        {
            clearObjList[_index].SetActive(false);
        }
    }
}
