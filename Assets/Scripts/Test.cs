#if UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    string log;


    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 3);


        if (GUILayout.Button("ClearLog"))
            log = "";

        if (GUILayout.Button("Login"))
            ChvjUnityInfra.CHMGPGS.Instance.Login((success, localUser) =>
            log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}");

        if (GUILayout.Button("Logout"))
            ChvjUnityInfra.CHMGPGS.Instance.Logout();

        if (GUILayout.Button("SaveCloud"))
            ChvjUnityInfra.CHMGPGS.Instance.SaveCloud("mysave", "want data", success => log = $"{success}");

        if (GUILayout.Button("LoadCloud"))
            ChvjUnityInfra.CHMGPGS.Instance.LoadCloud("mysave", (success, data) => log = $"{success}, {data}");

        if (GUILayout.Button("DeleteCloud"))
            ChvjUnityInfra.CHMGPGS.Instance.DeleteCloud("mysave", success => log = $"{success}");

        /*if (GUILayout.Button("ShowAchievementUI"))
            ChvjUnityInfra.CHMGPGS.Instance.ShowAchievementUI();

        if (GUILayout.Button("UnlockAchievement_one"))
            ChvjUnityInfra.CHMGPGS.Instance.UnlockAchievement(GPGSIds.achievement_one, success => log = $"{success}");

        if (GUILayout.Button("UnlockAchievement_two"))
            ChvjUnityInfra.CHMGPGS.Instance.UnlockAchievement(GPGSIds.achievement_two, success => log = $"{success}");

        if (GUILayout.Button("IncrementAchievement_three"))
            ChvjUnityInfra.CHMGPGS.Instance.IncrementAchievement(GPGSIds.achievement_three, 1, success => log = $"{success}");

        if (GUILayout.Button("ShowAllLeaderboardUI"))
            ChvjUnityInfra.CHMGPGS.Instance.ShowAllLeaderboardUI();

        if (GUILayout.Button("ShowTargetLeaderboardUI_num"))
            ChvjUnityInfra.CHMGPGS.Instance.ShowTargetLeaderboardUI(GPGSIds.leaderboard_num);

        if (GUILayout.Button("ReportLeaderboard_num"))
            ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_num, 1000, success => log = $"{success}");

        if (GUILayout.Button("LoadAllLeaderboardArray_num"))
            ChvjUnityInfra.CHMGPGS.Instance.LoadAllLeaderboardArray(GPGSIds.leaderboard_num, scores =>
            {
                log = "";
                for (int i = 0; i < scores.Length; i++)
                    log += $"{i}, {scores[i].rank}, {scores[i].value}, {scores[i].userID}, {scores[i].date}\n";
            });

        if (GUILayout.Button("LoadCustomLeaderboardArray_num"))
            ChvjUnityInfra.CHMGPGS.Instance.LoadCustomLeaderboardArray(GPGSIds.leaderboard_num, 10,
                GooglePlayGames.BasicApi.LeaderboardStart.PlayerCentered, GooglePlayGames.BasicApi.LeaderboardTimeSpan.Daily, (success, scoreData) =>
                {
                    log = $"{success}\n";
                    var scores = scoreData.Scores;
                    for (int i = 0; i < scores.Length; i++)
                        log += $"{i}, {scores[i].rank}, {scores[i].value}, {scores[i].userID}, {scores[i].date}\n";
                });

        if (GUILayout.Button("IncrementEvent_event"))
            ChvjUnityInfra.CHMGPGS.Instance.IncrementEvent(GPGSIds.event_event, 1);

        if (GUILayout.Button("LoadEvent_event"))
            ChvjUnityInfra.CHMGPGS.Instance.LoadEvent(GPGSIds.event_event, (success, iEvent) =>
            {
                log = $"{success}, {iEvent.Name}, {iEvent.CurrentCount}";
            });

        if (GUILayout.Button("LoadAllEvent"))
            ChvjUnityInfra.CHMGPGS.Instance.LoadAllEvent((success, iEvents) =>
            {
                log = $"{success}\n";
                foreach (var iEvent in iEvents)
                    log += $"{iEvent.Name}, {iEvent.CurrentCount}\n";
            });*/

        GUILayout.Label(log);
    }
}
#endif