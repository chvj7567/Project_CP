using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;
using UniRx;

public class MissionScrollViewItem : MonoBehaviour
{
    [SerializeField] CHText missionText;
    [SerializeField] CHText missionValueText;
    [SerializeField] List<GameObject> missionImgList = new List<GameObject>();
    [SerializeField] List<GameObject> rewardImgList = new List<GameObject>();
    [SerializeField] CHText rewardCountText;   // 보상 개수 (x10 형식)
    [SerializeField] Button rewardBtn;
    [SerializeField] CHText rewardBtnText;   // 보상 버튼 라벨 (광고 미션: 보기/받기 전환)
    [SerializeField] GameObject clearObj;

    Infomation.MissionInfo _info;
    Data.Mission _missionData;
    Data.Collection _collectionData;

    void Start()
    {
        rewardBtn.OnClickAsObservable().Subscribe(_ =>
        {
            // 광고 시청 미션 — 목표 미달이면 클릭 시 리워드 광고 재생
            if (IsAdWatchMission()
                && DailyMissionService.GetDailyProgress(_info) < _info.clearValue)
            {
                CHMAdmob.Instance.ShowRewardedAd();
                return;
            }

            var reward = _info.rewardCount;
            switch (_info.reward)
            {
                case Defines.EReward.Gold:
                    {
                        CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value += reward;
                        CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            stringID = 57,
                            intValue = reward
                        });
                    }
                    break;
                case Defines.EReward.AddTime:
                    {
                        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
                        if (loginData == null)
                            return;

                        loginData.addTimeItemCount += reward;
                        CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            stringID = 58,
                            intValue = reward
                        });
                    }
                    break;
                case Defines.EReward.AddMove:
                    {
                        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
                        if (loginData == null)
                            return;

                        loginData.addMoveItemCount += reward;
                        CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                        {
                            stringID = 59,
                            intValue = reward
                        });
                    }
                    break;
            }

            if (_info.tapIndex == UIMission.MissionTabNormal)
            {
                _missionData.repeatCount++;
                var clearValue = _info.clearValue + (_missionData.repeatCount * _info.addValue);
                SetBtnInteractable(clearValue);
                missionValueText.SetText(_collectionData.value - _missionData.startValue, clearValue);
            }
            else if (_info.tapIndex == UIMission.MissionTabSpecial)
            {
                var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
                if (loginData.normalStage >= _info.clearValue &&
                    loginData.rewardStage < _info.clearValue)
                {
                    clearObj.SetActive(true);
                    loginData.rewardStage = _info.clearValue;
                    CHMData.Instance.SaveData(CHMString.Instance.CatPang);
                }

                rewardBtn.interactable = false;
            }
            else if (_info.tapIndex == UIMission.MissionTabDaily)
            {
                // 일일 미션 — 1회 수령으로 완료 처리. repeatCount 미사용.
                _missionData.clearState = Defines.EClearState.Clear;
                clearObj.SetActive(true);
                rewardBtn.interactable = false;
                CHMData.Instance.SaveData(CHMString.Instance.CatPang);
            }
        });
    }

    public void Init(int index, Infomation.MissionInfo info)
    {
        _info = info;

        // 미션별 설명 (Mission.json의 descStringID)
        missionText.SetStringID(_info.descStringID);

        // 보상 버튼 라벨 기본값 "받기" (광고 미션 미시청 시 아래에서 "보기"로 변경)
        if (rewardBtnText != null)
            rewardBtnText.SetStringID(173);

        // 보상 개수 표시 (x10 형식)
        if (rewardCountText != null)
            rewardCountText.SetText("x", _info.rewardCount);

        if (_info.tapIndex == UIMission.MissionTabNormal)
        {
            _collectionData = CHMData.Instance.GetCollectionData(_info.collectionType.ToString());
            _missionData = CHMData.Instance.GetMissionData(_info.missionID.ToString());

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
        else if (_info.tapIndex == UIMission.MissionTabSpecial)
        {
            // 풀링 재사용 시 이전 미션의 클리어 표시가 남지 않도록 초기화
            clearObj.SetActive(false);

            missionValueText.SetStringID(27);
            missionValueText.SetText(_info.clearValue);
            SetMissionImage(_info.collectionType);
            SetRewardImage(_info.reward);

            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
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
        else if (_info.tapIndex == UIMission.MissionTabDaily)
        {
            // 일일 미션 — 진행도는 DailyMissionService가 카운터/스냅샷 차분으로 계산
            _missionData = CHMData.Instance.GetMissionData(_info.missionID.ToString());

            clearObj.SetActive(false);

            SetMissionImage(_info.collectionType);
            SetRewardImage(_info.reward);

            int current = DailyMissionService.GetDailyProgress(_info);
            int target = _info.clearValue;

            if (_missionData.clearState == Defines.EClearState.Clear)
            {
                missionValueText.SetText(target, target);
                clearObj.SetActive(true);
                rewardBtn.interactable = false;
            }
            else
            {
                missionValueText.SetStringID(20);
                missionValueText.SetText(Mathf.Min(current, target), target);
                // 광고 미션은 미달 시에도 버튼 활성화 (클릭 시 리워드 광고 재생)
                rewardBtn.interactable = IsAdWatchMission() || current >= target;

                // 광고 미션 미시청 — 버튼 라벨 "보기"
                if (IsAdWatchMission() && current < target && rewardBtnText != null)
                    rewardBtnText.SetStringID(172);
            }
        }
    }

    // 광고 시청 일일 미션 여부
    bool IsAdWatchMission()
    {
        return _info != null
            && _info.tapIndex == UIMission.MissionTabDaily
            && _info.dailyCounter == Defines.EDailyCounter.AdWatch;
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
