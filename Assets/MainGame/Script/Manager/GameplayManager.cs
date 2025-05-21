using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class GameplayManager : SerializedManager<GameplayManager>
    {
        #region PUBLIC_VARS
        [SerializeField] protected Dictionary<int, int> boosterUseData = new Dictionary<int, int>();
        public Transform RopeEndPosition;
        private Coroutine levelTimer;
        private List<Action<TimeSpan>> timerTickAction = new List<Action<TimeSpan>>();
        private bool isFrezzTimer;
        private TimeSpan remainingTime;
        public TimeSpan RemainingTime => remainingTime;

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public void OnBoosterUse(int boosterId)
        {
            if (boosterUseData.ContainsKey(boosterId))
            {
                boosterUseData[boosterId] += 1;
            }
            else
            {
                boosterUseData.Add(boosterId, 1);
            }
            LevelManager.Instance.CurrentLevel.CheckForLevelWin();
        }
        public virtual void SaveAllDataOfLevel()
        {
            //if (isGameComplate)
            //    return;
            //LevelProgressData levelProgressData = DataManager.LevelProgressData;
            //LevelManager.Instance.LoadedLevel.SaveCellData(levelProgressData);
            //ItemStackSpawnerManager.Instance.SaveData(levelProgressData);
            //GameplayGoalHandler.Instance.SaveGoalData(levelProgressData);
            //levelProgressData.currentReviveCountCoin = reviveCountCoin;
            //levelProgressData.currentReviveCountAd = reviveCountAd;
            //levelProgressData.adTileWatchCount = adTileWatchCountAd;
            //levelProgressData.boosterUseData = boosterUseData;
            //DataManager.Instance.SaveLevelProgressData(levelProgressData);
        }
        public void OnLevelStart()
        {

        }
        public void OnLevelEnd()
        {
            MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Hide();
            MainSceneUIManager.Instance.GetView<GameplayBottomView>().Hide();
            GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(0.8f, () =>
            {
                LevelManager.Instance.UnloadLevel();
                MainSceneUIManager.Instance.GetView<MainView>().Show();
                MainSceneUIManager.Instance.GetView<BottombarView>().Show();
            });
            StopTimer();
        }

        public void OnLevelWin()
        {
            LevelManager.Instance.LevelWin();
            StopTimer();
            CoroutineRunner.instance.Wait(0.12f, () =>
            {
                MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Hide();
                MainSceneUIManager.Instance.GetView<GameplayBottomView>().Hide();
                MainSceneUIManager.Instance.GetView<LevelWinView>().Show();
            });
        }
        public void PlayNextLevel()
        {
            MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Hide();
            MainSceneUIManager.Instance.GetView<GameplayBottomView>().Hide();
            GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(0.8f, () =>
            {
                LevelManager.Instance.OnNextLevel();
                MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Show();
                MainSceneUIManager.Instance.GetView<GameplayBottomView>().Show();

            });
        }
        public void OnLevelFailed()
        {
            LevelManager.Instance.LevelFailed();
            StopTimer();
        }
        public void RegisterOnTimerTick(Action<TimeSpan> action)
        {
            if (!timerTickAction.Contains(action))
                timerTickAction.Add(action);
        }
        public void DeregisterOnTimerTick(Action<TimeSpan> action)
        {
            if (timerTickAction.Contains(action))
                timerTickAction.Remove(action);
        }
        public void StartTimer()
        {
            if (levelTimer != null)
                return;
            levelTimer = StartCoroutine(LevelTimerCO(LevelManager.Instance.CurrentLevelDataSO.TimeInSecond));
        }
        public void FrezzTimer(bool isTrue)
        {
            isFrezzTimer = isTrue;
        }
        public void StopTimer()
        {
            if (levelTimer != null)
                StopCoroutine(levelTimer);
            levelTimer = null;
        }
        private IEnumerator LevelTimerCO(int timeInSeconds)
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);
            remainingTime = TimeSpan.FromSeconds(timeInSeconds);

            while (remainingTime.TotalSeconds > 0)
            {
                yield return waitForSeconds;

                if (!isFrezzTimer)
                {
                    remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));

                    // Notify all registered actions
                    for (int i = 0; i < timerTickAction.Count; i++)
                    {
                        timerTickAction[i]?.Invoke(remainingTime);
                    }

                    // If time is up, fail the level
                    if (remainingTime.TotalSeconds <= 0)
                    {
                        OnLevelFailed();
                        break;
                    }
                }
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}
