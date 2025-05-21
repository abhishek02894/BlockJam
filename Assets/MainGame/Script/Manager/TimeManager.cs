using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block {
    public class TimeManager : SerializedManager<TimeManager>
    {
        #region private veriables
        private Coroutine timeCoroutine;
        private List<Action<DateTime>> timerTickEvent = new List<Action<DateTime>>();
        #endregion

        #region propertices
        private string date;
        [ShowInInspector] private List<Action<bool, TimeSpan>> onTimerPause = new List<Action<bool, TimeSpan>>();
        private DateTime pauseTime;
        public static DateTime Now => DateTime.Now;
        #endregion

        #region unity callback

        public override void Awake()
        {
            base.Awake();
            StartCoroutine(InfiniteTimer());
            OnLoadingDone();
        }

        public override void OnDestroy()
        {
            if (timeCoroutine != null)
                StopCoroutine(timeCoroutine);
            base.OnDestroy();
        }

        public void RegisterOnTimerPause(Action<bool, TimeSpan> action)
        {
            if (!onTimerPause.Contains(action))
                onTimerPause.Add(action);
        }

        public void DeregisterOnTimerPause(Action<bool, TimeSpan> action)
        {
            if (onTimerPause.Contains(action))
                onTimerPause.Remove(action);
        }
        public void RegisterTimerTickEvent(Action<DateTime> action)
        {
            if (!timerTickEvent.Contains(action))
            {
                timerTickEvent.Add(action);
            }
        }

        public void DeRegisterTimerTickEvent(Action<DateTime> action)
        {
            if (timerTickEvent.Contains(action))
            {
                timerTickEvent.Remove(action);
            }
        }

        public void InvokeTimerTickEvent(DateTime time)
        {
            for (int i = 0; i < timerTickEvent.Count; i++)
            {
                timerTickEvent[i].Invoke(time);
            }
        }
        #endregion

        #region Private_method
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                pauseTime = Now;
            OnTimerPause(pauseStatus, Now - pauseTime);
        }
        private void OnTimerPause(bool pauseStatus, TimeSpan timeSpan)
        {
            for (int i = 0; i < onTimerPause.Count; i++)
            {
                onTimerPause[i]?.Invoke(pauseStatus, timeSpan);
            }
        }
        #endregion

        #region Coroutine
        IEnumerator InfiniteTimer()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);
            while (true)
            {
                yield return waitForSeconds;
                InvokeTimerTickEvent(Now);
            }
        }
        #endregion
    }
}