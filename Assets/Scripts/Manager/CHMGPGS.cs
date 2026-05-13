#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Events;
using UnityEngine.SocialPlatforms;

// 글로벌 CHMGPGS 어댑터: 게임 코드의 CHMGPGS.Instance.X 호출을 패키지 ChvjUnityInfra.CHMGPGS에 위임. (Android only)
public class CHMGPGS
{
    private static CHMGPGS _instance;
    public static CHMGPGS Instance => _instance ??= new CHMGPGS();

    public void Login(Action<bool, ILocalUser> onLoginSuccess = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.Login(onLoginSuccess);

    public void Logout() => ChvjUnityInfra.CHMGPGS.Instance.Logout();

    public void SaveCloud(string fileName, string saveData, Action<bool> onCloudSaved = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.SaveCloud(fileName, saveData, onCloudSaved);

    public void LoadCloud(string fileName, Action<bool, string> onCloudLoaded = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadCloud(fileName, onCloudLoaded);

    public void DeleteCloud(string fileName, Action<bool> onCloudDeleted = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.DeleteCloud(fileName, onCloudDeleted);

    public void ShowAchievementUI() => ChvjUnityInfra.CHMGPGS.Instance.ShowAchievementUI();

    public void UnlockAchievement(string gpgsId, Action<bool> onUnlocked = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.UnlockAchievement(gpgsId, onUnlocked);

    public void IncrementAchievement(string gpgsId, int steps, Action<bool> onUnlocked = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.IncrementAchievement(gpgsId, steps, onUnlocked);

    public void ShowAllLeaderboardUI() => ChvjUnityInfra.CHMGPGS.Instance.ShowAllLeaderboardUI();

    public void ShowTargetLeaderboardUI(string gpgsId) =>
        ChvjUnityInfra.CHMGPGS.Instance.ShowTargetLeaderboardUI(gpgsId);

    public void ReportLeaderboard(string gpgsId, long score, Action<bool> onReported = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(gpgsId, score, onReported);

    public void LoadAllLeaderboardArray(string gpgsId, Action<IScore[]> onLoaded = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadAllLeaderboardArray(gpgsId, onLoaded);

    public void LoadCustomLeaderboardArray(string gpgsId, int rowCount, LeaderboardStart leaderboardStart,
        LeaderboardTimeSpan leaderboardTimeSpan, Action<bool, LeaderboardScoreData> onLoaded = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadCustomLeaderboardArray(gpgsId, rowCount, leaderboardStart, leaderboardTimeSpan, onLoaded);

    public void LoadUsers(IScore[] scores, Action<IUserProfile[]> onUserProfiles) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadUsers(scores, onUserProfiles);

    public void IncrementEvent(string gpgsId, uint steps) =>
        ChvjUnityInfra.CHMGPGS.Instance.IncrementEvent(gpgsId, steps);

    public void LoadEvent(string gpgsId, Action<bool, IEvent> onEventLoaded = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadEvent(gpgsId, onEventLoaded);

    public void LoadAllEvent(Action<bool, List<IEvent>> onEventsLoaded = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadAllEvent(onEventsLoaded);
}
#endif
