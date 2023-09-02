using System.Collections;
using System.Collections.Generic;
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
    void Start()
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            int index = i;
            btnList[index].OnClickAsObservable().Subscribe(_ =>
            {
                PlayerPrefs.SetInt("stage", int.Parse(textList[index].text));
                SceneManager.LoadScene(1);
            });

            if (CHMMain.Data.stageDataDic.TryGetValue(textList[index].text, out var data))
            {
                if (data.clear)
                {
                    clearObjList[int.Parse(textList[index].text) - 1].SetActive(true);
                }
                else
                {
                    clearObjList[int.Parse(textList[index].text) - 1].SetActive(false);
                }
            }
            else
            {
                clearObjList[int.Parse(textList[index].text) - 1].SetActive(false);
            }
        }
    }
}
