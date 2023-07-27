using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectScene : MonoBehaviour
{
    [SerializeField]
    List<Button> btnList = new List<Button>();
    [SerializeField]
    List<TMP_Text> textList = new List<TMP_Text>();

    void Start()
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            int index = i;
            btnList[index].OnClickAsObservable().Subscribe(_ =>
            {
                PlayerPrefs.SetInt("size", int.Parse(textList[index].text));
                SceneManager.LoadScene(2);
            });
        }
    }
}
