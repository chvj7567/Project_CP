using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;

public class UIMissionArg : CHUIArg
{

}

public class UIMission : UIBase
{
    UIMissionArg arg;

    [SerializeField] MissionScrollView scrollView;
    [SerializeField] Button normalTapBtn;
    [SerializeField] Button specialTapBtn;
    [SerializeField] Button dailyTapBtn;        // 일일 탭 (신규)
    [SerializeField] CHText curTapText;
    [SerializeField] CHText resetTimerText;     // 리셋까지 남은 시간 표시 (HH:MM:SS)
    [SerializeField] GameObject offlineLockObj; // NTP 미수신 시 표시할 잠금 오버레이

    [SerializeField, ReadOnly] int curTapIndex;

    // 미션 탭 인덱스 — 일반 / 특별 / 일일 (MissionScrollViewItem과 공유)
    public const int MissionTabNormal = 1;
    public const int MissionTabSpecial = 2;
    public const int MissionTabDaily = 3;

    string _lastDateKey = "";

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIMissionArg;
    }

    private void Start()
    {
        // 일일 탭 잠금 초기 상태
        if (offlineLockObj != null) offlineLockObj.SetActive(false);
        UpdateDailyLockState();

        // NTP가 나중에라도 동기화되면 잠금 해제 + 일일 탭이면 내용 갱신
        if (CHMMain.Time != null && CHMMain.Time.OnAvailable != null)
        {
            CHMMain.Time.OnAvailable.Subscribe(_ =>
            {
                UpdateDailyLockState();
                if (curTapIndex == MissionTabDaily)
                    ShowDailyTab();
            }).AddTo(this);
        }

        // 진입 시 자정 리셋 + 출석 처리
        DailyMissionService.CheckAndResetIfNeeded();
        DailyMissionService.MarkAttendance();

        normalTapBtn.OnClickAsObservable().Subscribe(_ => ShowNormalTab(MissionTabNormal)).AddTo(this);
        specialTapBtn.OnClickAsObservable().Subscribe(_ => ShowNormalTab(MissionTabSpecial)).AddTo(this);

        if (dailyTapBtn != null)
            dailyTapBtn.OnClickAsObservable().Subscribe(_ => ShowDailyTab()).AddTo(this);

        // 리워드 광고 시청 완료 시 일일 탭이면 진행도 즉시 갱신
        CHMAdmob.Instance.AcquireReward += OnRewardAcquired;

        curTapText.SetStringID(121);

        // 기본 탭 = 일일 탭
        ShowDailyTab();
    }

    // 일반/특별 탭(1, 2). 리셋 타이머는 일일 탭 전용이라 숨김
    void ShowNormalTab(int index)
    {
        curTapIndex = index;
        if (offlineLockObj != null) offlineLockObj.SetActive(false);
        if (resetTimerText != null) resetTimerText.gameObject.SetActive(false);
        scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(index));
    }

    // 일일 탭(3). NTP 수신 시에만 내용·리셋 타이머 표시, 미수신 시 잠금 오버레이
    void ShowDailyTab()
    {
        curTapIndex = MissionTabDaily;

        bool available = CHMMain.Time != null && CHMMain.Time.IsAvailable;
        if (offlineLockObj != null) offlineLockObj.SetActive(!available);
        if (resetTimerText != null) resetTimerText.gameObject.SetActive(available);

        if (!available) return;

        DailyMissionService.CheckAndResetIfNeeded();
        scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(MissionTabDaily));
    }

    // 리워드 광고 시청 완료 콜백 — 일일 탭이면 진행도 즉시 갱신
    void OnRewardAcquired()
    {
        if (curTapIndex == MissionTabDaily)
            ShowDailyTab();
    }

    private void OnDestroy()
    {
        CHMAdmob.Instance.AcquireReward -= OnRewardAcquired;
    }

    void UpdateDailyLockState()
    {
        if (dailyTapBtn == null) return;
        dailyTapBtn.interactable = CHMMain.Time != null && CHMMain.Time.IsAvailable;
    }

    private void Update()
    {
        if (CHMMain.Time == null || !CHMMain.Time.IsAvailable) return;

        // 카운트다운 텍스트 갱신
        if (curTapIndex == MissionTabDaily && resetTimerText != null)
            resetTimerText.SetText(DailyMissionService.GetResetCountdown());

        // 자정 넘김 자동 감지
        var todayKey = CHMMain.Time.GetUtcDateKey();
        if (string.IsNullOrEmpty(_lastDateKey)) { _lastDateKey = todayKey; return; }
        if (_lastDateKey != todayKey)
        {
            _lastDateKey = todayKey;
            DailyMissionService.CheckAndResetIfNeeded();
            if (curTapIndex == MissionTabDaily)
                scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
        }
    }
}
