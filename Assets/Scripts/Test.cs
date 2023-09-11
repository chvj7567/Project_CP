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
            CHMGPGS.Login((success, localUser) =>
            log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}");

        if (GUILayout.Button("Logout"))
            CHMGPGS.Logout();

        if (GUILayout.Button("SaveCloud"))
            CHMGPGS.SaveCloud("mysave", "want data", success => log = $"{success}");

        if (GUILayout.Button("LoadCloud"))
            CHMGPGS.LoadCloud("mysave", (success, data) => log = $"{success}, {data}");

        if (GUILayout.Button("DeleteCloud"))
            CHMGPGS.DeleteCloud("mysave", success => log = $"{success}");

        if (GUILayout.Button("ShowAchievementUI"))
            CHMGPGS.ShowAchievementUI();

        /*if (GUILayout.Button("UnlockAchievement_one"))
            CHMGPGS.UnlockAchievement(GPGSIds.achievement_one, success => log = $"{success}");

        if (GUILayout.Button("UnlockAchievement_two"))
            CHMGPGS.UnlockAchievement(GPGSIds.achievement_two, success => log = $"{success}");

        if (GUILayout.Button("IncrementAchievement_three"))
            CHMGPGS.IncrementAchievement(GPGSIds.achievement_three, 1, success => log = $"{success}");

        if (GUILayout.Button("ShowAllLeaderboardUI"))
            CHMGPGS.ShowAllLeaderboardUI();

        if (GUILayout.Button("ShowTargetLeaderboardUI_num"))
            CHMGPGS.ShowTargetLeaderboardUI(GPGSIds.leaderboard_num);

        if (GUILayout.Button("ReportLeaderboard_num"))
            CHMGPGS.ReportLeaderboard(GPGSIds.leaderboard_num, 1000, success => log = $"{success}");

        if (GUILayout.Button("LoadAllLeaderboardArray_num"))
            CHMGPGS.LoadAllLeaderboardArray(GPGSIds.leaderboard_num, scores =>
            {
                log = "";
                for (int i = 0; i < scores.Length; i++)
                    log += $"{i}, {scores[i].rank}, {scores[i].value}, {scores[i].userID}, {scores[i].date}\n";
            });

        if (GUILayout.Button("LoadCustomLeaderboardArray_num"))
            CHMGPGS.LoadCustomLeaderboardArray(GPGSIds.leaderboard_num, 10,
                GooglePlayGames.BasicApi.LeaderboardStart.PlayerCentered, GooglePlayGames.BasicApi.LeaderboardTimeSpan.Daily, (success, scoreData) =>
                {
                    log = $"{success}\n";
                    var scores = scoreData.Scores;
                    for (int i = 0; i < scores.Length; i++)
                        log += $"{i}, {scores[i].rank}, {scores[i].value}, {scores[i].userID}, {scores[i].date}\n";
                });

        if (GUILayout.Button("IncrementEvent_event"))
            CHMGPGS.IncrementEvent(GPGSIds.event_event, 1);

        if (GUILayout.Button("LoadEvent_event"))
            CHMGPGS.LoadEvent(GPGSIds.event_event, (success, iEvent) =>
            {
                log = $"{success}, {iEvent.Name}, {iEvent.CurrentCount}";
            });

        if (GUILayout.Button("LoadAllEvent"))
            CHMGPGS.LoadAllEvent((success, iEvents) =>
            {
                log = $"{success}\n";
                foreach (var iEvent in iEvents)
                    log += $"{iEvent.Name}, {iEvent.CurrentCount}\n";
            });*/

        GUILayout.Label(log);
    }
}