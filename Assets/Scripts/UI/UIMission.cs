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
                if (curTapIndex == 3)
                    ShowDailyTab();
            }).AddTo(this);
        }

        // 진입 시 자정 리셋 + 출석 처리
        DailyMissionService.CheckAndResetIfNeeded();
        DailyMissionService.MarkAttendance();

        normalTapBtn.OnClickAsObservable().Subscribe(_ => ShowNormalTab(1)).AddTo(this);
        specialTapBtn.OnClickAsObservable().Subscribe(_ => ShowNormalTab(2)).AddTo(this);

        if (dailyTapBtn != null)
            dailyTapBtn.OnClickAsObservable().Subscribe(_ => ShowDailyTab()).AddTo(this);

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
        curTapIndex = 3;

        bool available = CHMMain.Time != null && CHMMain.Time.IsAvailable;
        if (offlineLockObj != null) offlineLockObj.SetActive(!available);
        if (resetTimerText != null) resetTimerText.gameObject.SetActive(available);

        if (!available) return;

        DailyMissionService.CheckAndResetIfNeeded();
        scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(3));
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
        if (curTapIndex == 3 && resetTimerText != null)
            resetTimerText.SetText(DailyMissionService.GetResetCountdown());

        // 자정 넘김 자동 감지
        var todayKey = CHMMain.Time.GetUtcDateKey();
        if (string.IsNullOrEmpty(_lastDateKey)) { _lastDateKey = todayKey; return; }
        if (_lastDateKey != todayKey)
        {
            _lastDateKey = todayKey;
            DailyMissionService.CheckAndResetIfNeeded();
            if (curTapIndex == 3)
                scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
        }
    }
}
