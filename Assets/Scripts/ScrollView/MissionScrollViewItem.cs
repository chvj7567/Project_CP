using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class MissionScrollViewItem : MonoBehaviour
{
    [SerializeField] CHTMPro missionValueText;
    [SerializeField] List<GameObject> missionImgList = new List<GameObject>();
    [SerializeField] Button rewardBtn;
    [SerializeField] GameObject clearObj;

    Infomation.MissionInfo info;
    Data.Mission missionData;
    Data.Collection collectionData;

    void Start()
    {
        rewardBtn.OnClickAsObservable().Subscribe(_ =>
        {
            missionData.repeatCount++;
            var clearValue = info.clearValue + (missionData.repeatCount * info.addValue);

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg { alarmText = $"{missionData.repeatCount}Reward" });

            SetBtnInteractable(clearValue);
            missionValueText.SetText(collectionData.value - missionData.startValue, clearValue);

            CHMData.Instance.SaveData(CHMMain.String.catPang);
        });
    }

    public void Init(int _index, Infomation.MissionInfo _info)
    {
        info = _info;

        if (CHMData.Instance.collectionDataDic.TryGetValue(info.collectionType.ToString(), out collectionData) == false)
            return;

        if (CHMData.Instance.missionDataDic.TryGetValue(info.missionID.ToString(), out missionData) == false)
        {
            missionData = CHMData.Instance.CreateMissionData(info.missionID.ToString());
        }

        clearObj.SetActive(false);

        SetImage(info.collectionType);

        if (missionData.clearState == Defines.EClearState.Clear)
        {
            missionValueText.SetText(info.clearValue, info.clearValue);
            clearObj.SetActive(true);
            rewardBtn.interactable = false;
        }
        else
        {
            if (missionData.clearState == Defines.EClearState.NotDoing)
            {
                missionData.startValue = collectionData.value;
                missionData.clearState = Defines.EClearState.Doing;
                rewardBtn.interactable = false;
            }

            var clearValue = info.clearValue + (missionData.repeatCount * info.addValue);
            SetBtnInteractable(clearValue);
            missionValueText.SetText(collectionData.value - missionData.startValue, clearValue);
        }
    }

    void SetBtnInteractable(int _clearValue)
    {
        if (collectionData.value - missionData.startValue < _clearValue)
        {
            rewardBtn.interactable = false;
        }
        else
        {
            rewardBtn.interactable = true;
        }
    }

    void SetImage(Defines.EBlockState _blockState)
    {
        if (missionImgList.Count < 8)
            return;

        for (int i = 0; i < missionImgList.Count; ++i)
        {
            missionImgList[i].SetActive(false);
        }

        switch (_blockState)
        {
            case Defines.EBlockState.Arrow1:
                missionImgList[0].SetActive(true);
                break;
            case Defines.EBlockState.Arrow2:
                missionImgList[1].SetActive(true);
                break;
            case Defines.EBlockState.Arrow3:
                missionImgList[2].SetActive(true);
                break;
            case Defines.EBlockState.Arrow4:
                missionImgList[3].SetActive(true);
                break;
            case Defines.EBlockState.Arrow5:
                missionImgList[4].SetActive(true);
                break;
            case Defines.EBlockState.Arrow6:
                missionImgList[5].SetActive(true);
                break;
            case Defines.EBlockState.CatPang:
                missionImgList[6].SetActive(true);
                break;
            case Defines.EBlockState.PinkBomb:
                missionImgList[7].SetActive(true);
                break;
        }
    }
}
