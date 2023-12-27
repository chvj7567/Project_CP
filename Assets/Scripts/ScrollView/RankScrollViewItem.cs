using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class RankScrollViewItem : MonoBehaviour
{
    [SerializeField] CHTMPro userID;
    [SerializeField] CHTMPro stage;
    [SerializeField] CHTMPro stageRank;

    Infomation.RankInfo info;

    void Start()
    {

    }

    public void Init(int index, Infomation.RankInfo info)
    {
        userID.SetText(info.userID);
        stageRank.SetText(info.stageRank);
        stage.SetText(info.stage);
    }
}
