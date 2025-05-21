using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class FrezzTimeBooster : BaseBooster
    {
        #region PUBLIC_VARS
        [SerializeField] private int frezzTimeInSecond;
        private Coroutine coroutine;
        private TimeSpan remainingTime;
        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public override void OnActive(Action onUse)
        {
            ActiveConfirmationView();
            OnUse();
            LevelManager.Instance.RegisterOnLevelFailed(OnLevelFailed);
            LevelManager.Instance.RegisterOnLevelWin(OnLevelWin);
            base.OnActive(onUse);
        }

        

        public override void OnUse()
        {
            Debug.LogError("Use Frezz Booster");
            GameplayManager.Instance.SaveAllDataOfLevel();
            GameplayManager.Instance.FrezzTimer(true);
            StartTimer();
            DeActiveConfirmationView();
            base.OnUse();
        }

        public override void OnUnUse()
        {
            base.OnUnUse();
            GameplayManager.Instance.FrezzTimer(false);
            LevelManager.Instance.DeregisterOnLevelFailed(OnLevelFailed);
            LevelManager.Instance.DeregisterOnLevelWin(OnLevelWin);
            DeActiveConfirmationView();
        }
        private void OnLevelFailed(Level level)
        {
            StopTimer();
        }

        private void OnLevelWin(Level level)
        {
            StopTimer();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void StartTimer()
        {
            StopTimer();
            coroutine = StartCoroutine(StartFrezzTimer());
        }
        private void StopTimer()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }
        #endregion

        #region CO-ROUTINES
        IEnumerator StartFrezzTimer()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);
            remainingTime = TimeSpan.FromSeconds(frezzTimeInSecond);

            while (remainingTime.TotalSeconds > 0)
            {
                yield return waitForSeconds;
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            }
            OnUnUse();
            coroutine = null;
        }
        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}
