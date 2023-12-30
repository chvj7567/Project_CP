using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class RankScrollViewItem : MonoBehaviour
{
    [SerializeField] UIRank uiRank;
    [SerializeField] List<Image> imageList = new List<Image>();
    [SerializeField] CHTMPro userID;
    [SerializeField] CHTMPro stage;
    [SerializeField] CHTMPro stageRank;

    Infomation.RankInfo info;

    public void Init(int index, Infomation.RankInfo info)
    {
        this.info = info;

        userID.SetText(info.userID);

        foreach (var img in imageList)
        {
            img.gameObject.SetActive(false);
        }

        if (index >= imageList.Count)
        {
            var r = UnityEngine.Random.Range(0f, 1f);
            var g = UnityEngine.Random.Range(0f, 1f);
            var b = UnityEngine.Random.Range(0f, 1f);

            imageList[imageList.Count - 1].gameObject.SetActive(true);
            imageList[imageList.Count - 1].color = new Color(r, g, b);
        }
        else
        {

            imageList[index].gameObject.SetActive(true);
        }

        stageRank.SetText(info.stageRank);
        
        if (uiRank.curTap == Defines.ESelectStage.Boss && info.stage > 0)
        {
            stage.SetText(info.stage - CHStatic.BossStageStartValue);
        }
        else
        {
            stage.SetText(info.stage);
        }
    }
}
