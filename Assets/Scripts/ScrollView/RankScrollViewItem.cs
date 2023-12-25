using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class RankScrollViewItem : MonoBehaviour
{
    [SerializeField] CHTMPro missionValueText;
    [SerializeField] List<GameObject> missionImgList = new List<GameObject>();
    [SerializeField] List<GameObject> rewardImgList = new List<GameObject>();
    [SerializeField] Button rewardBtn;
    [SerializeField] GameObject clearObj;

    Infomation.RankInfo info;
    Data.Mission missionData;
    Data.Collection collectionData;

    void Start()
    {

    }

    public void Init(int index, Infomation.RankInfo info)
    {

    }
}
