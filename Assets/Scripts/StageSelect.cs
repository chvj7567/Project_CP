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
    List<TMP_Text> sizeList = new List<TMP_Text>();
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
        }
    }
}
