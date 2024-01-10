using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class MissionScrollViewItem : MonoBehaviour
{
    [SerializeField] CHTMPro missionText;
    [SerializeField] CHTMPro missionValueText;
    [SerializeField] List<GameObject> missionImgList = new List<GameObject>();
    [SerializeField] List<GameObject> rewardImgList = new List<GameObject>();
    [SerializeField] Button rewardBtn;
    [SerializeField] GameObject clearObj;

    Infomation.MissionInfo _info;
    Data.Mission _missionData;
    Data.Collection _collectionData;

    void Start()
    {
        rewardBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var reward = _info.rewardCount;
            switch (_info.reward)
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

            if (_info.tapIndex == 1)
            {
                _missionData.repeatCount++;
                var clearValue = _info.clearValue + (_missionData.repeatCount * _info.addValue);
                SetBtnInteractable(clearValue);
                missionValueText.SetText(_collectionData.value - _missionData.startValue, clearValue);
            }
            else if (_info.tapIndex == 2)
            {
                var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                if (loginData.normalStage >= _info.clearValue &&
                    loginData.rewardStage < _info.clearValue)
                {
                    clearObj.SetActive(true);
                    loginData.rewardStage = _info.clearValue;
                    CHMData.Instance.SaveData(CHMMain.String.CatPang);
                }

                rewardBtn.interactable = false;
            }
        });
    }

    public void Init(int index, Infomation.MissionInfo info)
    {
        _info = info;

        if (_info.tapIndex == 1)
        {
            _collectionData = CHMData.Instance.GetCollectionData(_info.collectionType.ToString());
            _missionData = CHMData.Instance.GetMissionData(_info.missionID.ToString());

            missionText.SetStringID(13);

            clearObj.SetActive(false);

            SetMissionImage(_info.collectionType);
            SetRewardImage(_info.reward);

            if (_missionData.clearState == Defines.EClearState.Clear)
            {
                missionValueText.SetText(_info.clearValue, _info.clearValue);
                clearObj.SetActive(true);
                rewardBtn.interactable = false;
            }
            else
            {
                if (_missionData.clearState == Defines.EClearState.NotDoing)
                {
                    _missionData.startValue = _collectionData.value;
                    _missionData.clearState = Defines.EClearState.Doing;
                    rewardBtn.interactable = false;
                }

                var clearValue = _info.clearValue + (_missionData.repeatCount * _info.addValue);
                SetBtnInteractable(clearValue);
                missionValueText.SetStringID(20);
                missionValueText.SetText(_collectionData.value - _missionData.startValue, clearValue);
            }
        }
        else if (_info.tapIndex == 2)
        {
            missionText.SetStringID(123);
            missionValueText.SetStringID(27);
            missionValueText.SetText(_info.clearValue);
            SetMissionImage(_info.collectionType);
            SetRewardImage(_info.reward);

            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            if (loginData.normalStage >= _info.clearValue &&
                loginData.rewardStage < _info.clearValue)
            {
                rewardBtn.interactable = true;
            }
            else
            {
                if (loginData.rewardStage >= _info.clearValue)
                {
                    clearObj.SetActive(true);
                }

                rewardBtn.interactable = false;
            }
        }
    }

    void SetBtnInteractable(int clearValue)
    {
        if (_collectionData.value - _missionData.startValue < clearValue)
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
            case Defines.EBlockState.Fish:
                missionImgList[12].SetActive(true);
                break;
            case Defines.EBlockState.CatBox1:
                missionImgList[13].SetActive(true);
                break;
            case Defines.EBlockState.WallCreator:
                missionImgList[14].SetActive(true);
                break;
            case Defines.EBlockState.RainbowPang:
                missionImgList[15].SetActive(true);
                break;
            case Defines.EBlockState.Ball:
                missionImgList[16].SetActive(true);
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
