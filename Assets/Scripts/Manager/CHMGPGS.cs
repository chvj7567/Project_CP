using GooglePlayGames.BasicApi.Events;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class CHMGPGS
{
    [SerializeField]
    static TMP_Text googleLog;

    [SerializeField]
    static TMP_Text saveLog;

    static void Init()
    {
        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .RequestEmail()
            .Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }


    static public void Login(Action<bool, UnityEngine.SocialPlatforms.ILocalUser> onLoginSuccess = null)
    {
        Init();
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
        {
            onLoginSuccess?.Invoke(success == SignInStatus.Success, Social.localUser);
        });
    }

    static public void Logout()
    {
        if (Social.localUser.authenticated)

        {
            PlayGamesPlatform.Instance.SignOut();
        }
    }

    static public void SaveCloud(string fileName, string saveData, Action<bool> onCloudSaved = null)
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    var update = new SavedGameMetadataUpdate.Builder().Build();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(saveData);
                    PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, bytes, (status2, game2) =>
                    {
                        onCloudSaved?.Invoke(status2 == SavedGameRequestStatus.Success);
                    });
                }
            });
    }

    static public void LoadCloud(string fileName, Action<bool, string> onCloudLoaded = null)
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (status2, loadedData) =>
                    {
                        if (status2 == SavedGameRequestStatus.Success)
                        {
                            string data = System.Text.Encoding.UTF8.GetString(loadedData);
                            onCloudLoaded?.Invoke(true, data);
                        }
                        else
                        {
                            onCloudLoaded?.Invoke(false, null);
                        }
                    });
                }
            });
    }

    static public void DeleteCloud(string fileName, Action<bool> onCloudDeleted = null)
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    PlayGamesPlatform.Instance.SavedGame.Delete(game);
                    onCloudDeleted?.Invoke(true);
                }
                else
                {
                    onCloudDeleted?.Invoke(false);
                }
            });
    }


    static public void ShowAchievementUI() =>
        Social.ShowAchievementsUI();

    static public void UnlockAchievement(string gpgsId, Action<bool> onUnlocked = null) =>
        Social.ReportProgress(gpgsId, 100, success => onUnlocked?.Invoke(success));

    static public void IncrementAchievement(string gpgsId, int steps, Action<bool> onUnlocked = null) =>
        PlayGamesPlatform.Instance.IncrementAchievement(gpgsId, steps, success => onUnlocked?.Invoke(success));


    static public void ShowAllLeaderboardUI() =>
        Social.ShowLeaderboardUI();

    static public void ShowTargetLeaderboardUI(string gpgsId) =>
        ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(gpgsId);

    static public void ReportLeaderboard(string gpgsId, long score, Action<bool> onReported = null) =>
        Social.ReportScore(score, gpgsId, success => onReported?.Invoke(success));

    static public void LoadAllLeaderboardArray(string gpgsId, Action<UnityEngine.SocialPlatforms.IScore[]> onloaded = null) =>
        Social.LoadScores(gpgsId, onloaded);

    static public void LoadCustomLeaderboardArray(string gpgsId, int rowCount, LeaderboardStart leaderboardStart,
        LeaderboardTimeSpan leaderboardTimeSpan, Action<bool, LeaderboardScoreData> onloaded = null)
    {
        PlayGamesPlatform.Instance.LoadScores(gpgsId, leaderboardStart, rowCount, LeaderboardCollection.Public, leaderboardTimeSpan, data =>
        {
            onloaded?.Invoke(data.Status == ResponseStatus.Success, data);
        });
    }


    static public void IncrementEvent(string gpgsId, uint steps)
    {
        PlayGamesPlatform.Instance.Events.IncrementEvent(gpgsId, steps);
    }

    static public void LoadEvent(string gpgsId, Action<bool, IEvent> onEventLoaded = null)
    {
        PlayGamesPlatform.Instance.Events.FetchEvent(DataSource.ReadCacheOrNetwork, gpgsId, (status, iEvent) =>
        {
            onEventLoaded?.Invoke(status == ResponseStatus.Success, iEvent);
        });
    }

    static public void LoadAllEvent(Action<bool, List<IEvent>> onEventsLoaded = null)
    {
        PlayGamesPlatform.Instance.Events.FetchAllEvents(DataSource.ReadCacheOrNetwork, (status, events) =>
        {
            onEventsLoaded?.Invoke(status == ResponseStatus.Success, events);
        });
    }
}
