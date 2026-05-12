#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Events;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace ChvjUnityInfra
{
    /// <summary>
    /// Google Play Games Services 매니저. Android only.
    /// 로그인, 클라우드 저장, 업적, 리더보드, 이벤트 지원.
    /// </summary>
    public class CHMGPGS : CHSingletonStatic<CHMGPGS>
    {
        private bool _initialized = false;

        private void Init()
        {
            if (_initialized) return;
            _initialized = true;

            PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder()
                .RequestIdToken()
                .RequestEmail()
                .EnableSavedGames()
                .Build());
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
        }

        public void Login(Action<bool, ILocalUser> onLoginSuccess = null)
        {
            Init();
            PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
            {
                onLoginSuccess?.Invoke(success == SignInStatus.Success, Social.localUser);
            });
        }

        public void Logout()
        {
            if (Social.localUser.authenticated)
            {
                PlayGamesPlatform.Instance.SignOut();
            }
        }

        public void SaveCloud(string fileName, string saveData, Action<bool> onCloudSaved = null)
        {
            if (!Social.localUser.authenticated) { onCloudSaved?.Invoke(false); return; }
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
                    else
                    {
                        onCloudSaved?.Invoke(false);
                    }
                });
        }

        public void LoadCloud(string fileName, Action<bool, string> onCloudLoaded = null)
        {
            if (!Social.localUser.authenticated) { onCloudLoaded?.Invoke(false, null); return; }
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
                    else
                    {
                        onCloudLoaded?.Invoke(false, null);
                    }
                });
        }

        public void DeleteCloud(string fileName, Action<bool> onCloudDeleted = null)
        {
            if (!Social.localUser.authenticated) { onCloudDeleted?.Invoke(false); return; }
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

        public void ShowAchievementUI() => Social.ShowAchievementsUI();

        public void UnlockAchievement(string gpgsId, Action<bool> onUnlocked = null) =>
            Social.ReportProgress(gpgsId, 100, success => onUnlocked?.Invoke(success));

        public void IncrementAchievement(string gpgsId, int steps, Action<bool> onUnlocked = null) =>
            PlayGamesPlatform.Instance.IncrementAchievement(gpgsId, steps, success => onUnlocked?.Invoke(success));

        public void ShowAllLeaderboardUI() => Social.ShowLeaderboardUI();

        public void ShowTargetLeaderboardUI(string gpgsId) =>
            ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(gpgsId);

        public void ReportLeaderboard(string gpgsId, long score, Action<bool> onReported = null) =>
            Social.ReportScore(score, gpgsId, success => onReported?.Invoke(success));

        public void LoadAllLeaderboardArray(string gpgsId, Action<IScore[]> onLoaded = null) =>
            Social.LoadScores(gpgsId, onLoaded);

        public void LoadCustomLeaderboardArray(string gpgsId, int rowCount, LeaderboardStart leaderboardStart,
            LeaderboardTimeSpan leaderboardTimeSpan, Action<bool, LeaderboardScoreData> onLoaded = null)
        {
            PlayGamesPlatform.Instance.LoadScores(gpgsId, leaderboardStart, rowCount, LeaderboardCollection.Public, leaderboardTimeSpan, data =>
            {
                onLoaded?.Invoke(data.Status == ResponseStatus.Success, data);
            });
        }

        public void LoadUsers(IScore[] scores, Action<IUserProfile[]> onUserProfiles)
        {
            List<string> userIds = new List<string>();
            foreach (IScore score in scores)
            {
                userIds.Add(score.userID);
            }

            Social.LoadUsers(userIds.ToArray(), (users) => onUserProfiles?.Invoke(users));
        }

        public void IncrementEvent(string gpgsId, uint steps)
        {
            PlayGamesPlatform.Instance.Events.IncrementEvent(gpgsId, steps);
        }

        public void LoadEvent(string gpgsId, Action<bool, IEvent> onEventLoaded = null)
        {
            PlayGamesPlatform.Instance.Events.FetchEvent(DataSource.ReadCacheOrNetwork, gpgsId, (status, iEvent) =>
            {
                onEventLoaded?.Invoke(status == ResponseStatus.Success, iEvent);
            });
        }

        public void LoadAllEvent(Action<bool, List<IEvent>> onEventsLoaded = null)
        {
            PlayGamesPlatform.Instance.Events.FetchAllEvents(DataSource.ReadCacheOrNetwork, (status, events) =>
            {
                onEventsLoaded?.Invoke(status == ResponseStatus.Success, events);
            });
        }
    }
}
#endif
