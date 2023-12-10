using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class MissionScrollViewItem : MonoBehaviour
{
    [SerializeField] CHTMPro missionValueText;
    [SerializeField] List<GameObject> missionImgList = new List<GameObject>();
    [SerializeField] List<GameObject> rewardImgList = new List<GameObject>();
    [SerializeField] Button rewardBtn;
    [SerializeField] GameObject clearObj;

    Infomation.MissionInfo info;
    Data.Mission missionData;
    Data.Collection collectionData;

    void Start()
    {
        rewardBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var reward = info.rewardCount;
            switch (info.reward)
            {
                case Defines.EReward.Gold:
                    {
                        CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value += reward;
                        CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            stringID = 57,
                            intValue = reward
                        });
                    }
                    break;
                case Defines.EReward.AddTime:
                    {
                        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                        if (loginData == null)
                            return;

                        loginData.addTimeItemCount += reward;
                        CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            stringID = 58,
                            intValue = reward
                        });
                    }
                    break;
                case Defines.EReward.AddMove:
                    {
                        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                        if (loginData == null)
                            return;

                        loginData.addMoveItemCount += reward;
                        CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            stringID = 59,
                            intValue = reward
                        });
                    }
                    break;
            }

            missionData.repeatCount++;
            var clearValue = info.clearValue + (missionData.repeatCount * info.addValue);
            SetBtnInteractable(clearValue);
            missionValueText.SetText(collectionData.value - missionData.startValue, clearValue);
        });
    }

    public void Init(int _index, Infomation.MissionInfo _info)
    {
        info = _info;

        collectionData = CHMData.Instance.GetCollectionData(info.collectionType.ToString());
        missionData = CHMData.Instance.GetMissionData(info.missionID.ToString());

        clearObj.SetActive(false);

        SetMissionImage(info.collectionType);
        SetRewardImage(info.reward);

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

    void SetMissionImage(Defines.EBlockState blockState)
    {
        if (missionImgList == null)
            return;

        for (int i = 0; i < missionImgList.Count; ++i)
        {
            missionImgList[i].SetActive(false);
        }

        switch (blockState)
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
            case Defines.EBlockState.YellowBomb:
                missionImgList[8].SetActive(true);
                break;
            case Defines.EBlockState.OrangeBomb:
                missionImgList[9].SetActive(true);
                break;
            case Defines.EBlockState.GreenBomb:
                missionImgList[10].SetActive(true);
                break;
            case Defines.EBlockState.BlueBomb:
                missionImgList[11].SetActive(true);
                break;
        }
    }

    void SetRewardImage(Defines.EReward reward)
    {
        if (rewardImgList == null)
            return;

        for (int i = 0; i < rewardImgList.Count; ++i)
        {
            if ((int)reward == i)
            {
                rewardImgList[i].SetActive(true);
            }
            else
            {
                rewardImgList[i].SetActive(false);
            }
        }
    }
}
