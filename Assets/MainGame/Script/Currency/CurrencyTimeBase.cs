using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    [CreateAssetMenu(fileName = "TimeBaseCurrency", menuName = Constant.GAME_NAME + "/Currency/Time base Currency")]
    public class CurrencyTimeBase : Currency
    {
        #region public veriables

        public int unitTimeUpdate;

        #endregion

        #region private veriables

        private Coroutine coroutine;
        private List<Action<bool, bool>> onTimerStartOrStop = new List<Action<bool, bool>>();
        private List<Action<TimeSpan>> onTimerTick = new List<Action<TimeSpan>>();
        private List<Action<int>> onCurrencyUpdateByTimer = new List<Action<int>>();
        protected int time;
        private bool canStartTimer;

        private List<Action<bool, bool>> onInfiniteTimerStartOrStop = new List<Action<bool, bool>>();
        private List<Action<TimeSpan>> onInfiniteEnergyTimerTick = new List<Action<TimeSpan>>();
        private Coroutine infiniteEnergyCoroutine;
        private bool isInfiniteEnergyActive;
        private DateTime infiniteEnergyEndTime;

        #endregion

        #region propertices

        public virtual int UnitTimeUpdate
        {
            get { return unitTimeUpdate; }
        }

        private DateTime lastUpdateTime;

        public DateTime LastUpdateTime
        {
            get
            {
                string time = PlayerPrefbsHelper.GetString(key + "_Time", "");
                lastUpdateTime = (string.IsNullOrEmpty(time)) ? TimeManager.Now : JsonConvert.DeserializeObject<DateTime>(time);
                return lastUpdateTime;
            }
            private set
            {
                lastUpdateTime = value;
                PlayerPrefbsHelper.SetString(key + "_Time", JsonConvert.SerializeObject(lastUpdateTime));
            }
        }

        public DateTime LastUpdateTimeSaved
        {
            get
            {
                string time = PlayerPrefbsHelper.GetSavedString(key + "_Time", "");
                return (string.IsNullOrEmpty(time)) ? DateTime.Now : DateTime.Parse(time);
            }
        }

        public bool IsInfiniteEnergyActive { get => isInfiniteEnergyActive; }

        #endregion

        #region virtual methods

        public override void Add(int value, Action successAction = null, Vector3 position = default(Vector3))
        {
            if (isInfiniteEnergyActive && DateTime.Now < infiniteEnergyEndTime)
            {
                //base.Add(value, successAction, position);
                return; // Don't subtract energy if infinite energy is active
            }

            if (Value + value > defaultValue)
            {
                return;
            }

            if (value >= 0)
            {
                base.Add(value, successAction, position);
                return;
            }

            value = Mathf.Abs(value);
            if (Value >= value)
            {
                Debug.LogError("3----------");
                base.Add(-value, successAction, position);
                successAction?.Invoke();
            }
        }

        public override void Init()
        {
            base.Init();

            isInfiniteEnergyActive = PlayerPrefbsHelper.GetInt("IsInfiniteEnergyActive", 0) == 1;
            if (IsInfiniteEnergyActive)
            {
                string savedTime = PlayerPrefbsHelper.GetString("InfiniteEnergyEndTime", "");
                if (!string.IsNullOrEmpty(savedTime))
                {
                    infiniteEnergyEndTime = JsonConvert.DeserializeObject<DateTime>(savedTime);
                    if (TimeManager.Now > infiniteEnergyEndTime)
                    {
                        isInfiniteEnergyActive = false;
                        PlayerPrefbsHelper.SetInt("IsInfiniteEnergyActive", 0);
                    }
                    else
                    {
                        StartInfiniteEnergyTimer();
                    }
                }
            }

            AddRemainEnergy();
            RegisterOnCurrencyChangeEvent(CheckForTimer);
            canStartTimer = true;
            //TimeManager.Instance.RegisterOnTimerPause(OnGameResume);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            StopTimer();
            StopInfiniteEnergyTimer();
            RemoveOnCurrencyChangeEvent(CheckForTimer);
            //TimeManager.Instance.DeregisterOnTimerPause(OnGameResume);
        }

        public override void RemoveAllCallback()
        {
            base.RemoveAllCallback();
            onTimerStartOrStop.Clear();
            onTimerTick.Clear();
            onCurrencyUpdateByTimer.Clear();
            onInfiniteEnergyTimerTick.Clear();
            onInfiniteTimerStartOrStop.Clear();
        }

        #endregion

        #region private Methods

        private void OnGameResume(bool isPause, TimeSpan pauseTimeSpan)
        {
            if (Value >= defaultValue)
            {
                return;
            }
            if (!isPause)
            {
                if (pauseTimeSpan.TotalSeconds < time)
                {
                    StopTimer();
                    time = (time - (int)pauseTimeSpan.TotalSeconds);
                    StartTimer();
                    return;
                }

                int additionalValue = 1;
                long remainTime = (int)pauseTimeSpan.TotalSeconds;
                remainTime -= time;
                additionalValue += ((int)remainTime / UnitTimeUpdate);
                remainTime -= ((additionalValue - 1) * UnitTimeUpdate);
                StopTimer();
                if ((Value + additionalValue) >= defaultValue)
                {
                    int updateValues = defaultValue - Value;
                    Value += updateValues;
                    OnCurrencyUpdateByTimer(updateValues);
                    LastUpdateTime = TimeManager.Now;
                }
                else
                {
                    Value += additionalValue;
                    OnCurrencyUpdateByTimer(additionalValue);
                    time = UnitTimeUpdate - (int)remainTime;
                    LastUpdateTime = TimeManager.Now.AddSeconds(-time);
                    if (Value < defaultValue)
                        StartTimer();
                }
            }
        }

        private void AddRemainEnergy()
        {
            if (Value >= defaultValue)
                return;
            TimeSpan timeSpan = TimeManager.Now - LastUpdateTime;
            int energy = Mathf.FloorToInt((float)(timeSpan.TotalSeconds / UnitTimeUpdate));
            int remainTime = (int)(timeSpan.TotalSeconds - (energy * UnitTimeUpdate));
            if (energy > 0)
            {
                if ((Value + energy) < defaultValue)
                    Add(energy);
                else
                    SetValue(defaultValue);
                LastUpdateTime = TimeManager.Now.AddSeconds(-remainTime);
            }

            if (Value >= defaultValue)
                return;
            time = (remainTime > 0) ? (UnitTimeUpdate - remainTime) : UnitTimeUpdate;
            StartTimer();
        }

        private void CheckForTimer(int value)
        {
            if (coroutine != null || !canStartTimer)
            {
                if (defaultValue <= Value && coroutine != null)
                {
                    OnTimerStart(false, false);
                    StopTimer();
                    coroutine = null;
                }

                return;
            }

            LastUpdateTime = TimeManager.Now;
            if (defaultValue > Value)
            {
                time = UnitTimeUpdate;
                StartTimer();
                return;
            }

            OnTimerStart(false, false);
        }

        private void OnTimerStart(bool value, bool isUpdated)
        {
            for (int i = 0; i < onTimerStartOrStop.Count; i++)
            {
                onTimerStartOrStop[i]?.Invoke(value, isUpdated);
            }
        }

        private void OnInfiniteTimerStart(bool value, bool isUpdated)
        {
            for (int i = 0; i < onInfiniteTimerStartOrStop.Count; i++)
            {
                onInfiniteTimerStartOrStop[i]?.Invoke(value, isUpdated);
            }
        }

        private void OnTimerTick(TimeSpan timeSpan)
        {
            for (int i = 0; i < onTimerTick.Count; i++)
            {
                onTimerTick[i]?.Invoke(timeSpan);
            }
        }

        private void OnCurrencyUpdateByTimer(int value)
        {
            for (int i = 0; i < onTimerTick.Count; i++)
            {
                onCurrencyUpdateByTimer[i]?.Invoke(value);
            }
        }

        public void StartTimer()
        {
            StopTimer();
            coroutine = CoroutineRunner.instance.CoroutineStart(Timer());
        }

        private void StopTimer()
        {
            if (coroutine != null)
                CoroutineRunner.instance.CoroutineStop(coroutine);
            coroutine = null;
        }

        [Button]
        public void ActivateInfiniteEnergy(int second)
        {
            isInfiniteEnergyActive = true;
            if (infiniteEnergyEndTime >= TimeManager.Now)
            {
                infiniteEnergyEndTime = infiniteEnergyEndTime.AddSeconds(second);
            }
            else
            {
                infiniteEnergyEndTime = TimeManager.Now.AddSeconds(second);
            }
            PlayerPrefbsHelper.SetString("InfiniteEnergyEndTime", JsonConvert.SerializeObject(infiniteEnergyEndTime));
            PlayerPrefbsHelper.SetInt("IsInfiniteEnergyActive", 1);
            SetValue(defaultValue); // Set energy to max

            // Start infinite energy timer
            StartInfiniteEnergyTimer();
        }

        private void StartInfiniteEnergyTimer()
        {
            StopInfiniteEnergyTimer();
            OnInfiniteTimerStart(true, false);
            infiniteEnergyCoroutine = CoroutineRunner.instance.CoroutineStart(InfiniteEnergyTimer());
        }

        private void StopInfiniteEnergyTimer()
        {
            if (infiniteEnergyCoroutine != null)
            {
                CoroutineRunner.instance.CoroutineStop(infiniteEnergyCoroutine);
                infiniteEnergyCoroutine = null;
            }
        }

        public void RegisterInfiniteEnergyTimerTick(Action<TimeSpan> action)
        {
            if (!onInfiniteEnergyTimerTick.Contains(action))
            {
                onInfiniteEnergyTimerTick.Add(action);
            }
        }

        public void RemoveInfiniteEnergyTimerTick(Action<TimeSpan> action)
        {
            if (onInfiniteEnergyTimerTick.Contains(action))
            {
                onInfiniteEnergyTimerTick.Remove(action);
            }
        }

        private void OnInfiniteEnergyTimerTick(TimeSpan remainingTime)
        {
            foreach (var action in onInfiniteEnergyTimerTick)
            {
                action?.Invoke(remainingTime);
            }
        }

        private IEnumerator InfiniteEnergyTimer()
        {
            WaitForSeconds oneSecond = new WaitForSeconds(1);

            while (TimeManager.Now < infiniteEnergyEndTime)
            {
                TimeSpan remainingTime = infiniteEnergyEndTime - DateTime.Now;
                OnInfiniteEnergyTimerTick(remainingTime);
                yield return oneSecond;
            }

            // Deactivate infinite energy when timer ends
            isInfiniteEnergyActive = false;
            PlayerPrefs.SetInt("IsInfiniteEnergyActive", 0);
            infiniteEnergyCoroutine = null;
            OnInfiniteTimerStart(false, false);
        }

        #endregion

        #region public methods

        public void SetValue(int value, DateTime lastUpdateTime)
        {
            SetValue(value);
            LastUpdateTime = lastUpdateTime;
        }

        public void PauseTimer()
        {
            if (Value >= defaultValue)
                return;
            canStartTimer = false;
            StopTimer();
            coroutine = null;
            OnTimerStart(false, false);
        }

        public void ResumeTimer()
        {
            if (Value >= defaultValue)
                return;
            canStartTimer = true;
            StartTimer();
        }

        public bool IsFull()
        {
            return Value >= defaultValue;
        }

        public void RegisterTimerTick(Action<TimeSpan> action)
        {
            if (!onTimerTick.Contains(action))
            {
                onTimerTick.Add(action);
            }
        }

        public void RemoveTimerTick(Action<TimeSpan> action)
        {
            if (onTimerTick.Contains(action))
            {
                onTimerTick.Remove(action);
            }
        }

        public void RegisterTimerStartOrStop(Action<bool, bool> action)
        {
            if (!onTimerStartOrStop.Contains(action))
            {
                onTimerStartOrStop.Add(action);
            }
        }

        public void RemoveTimerStartOrStop(Action<bool, bool> action)
        {
            if (onTimerStartOrStop.Contains(action))
            {
                onTimerStartOrStop.Remove(action);
            }
        }

        public void RegisterInfiniteTimerStartOrStop(Action<bool, bool> action)
        {
            if (!onInfiniteTimerStartOrStop.Contains(action))
            {
                onInfiniteTimerStartOrStop.Add(action);
            }
        }

        public void RemoveInfiniteTimerStartOrStop(Action<bool, bool> action)
        {
            if (onInfiniteTimerStartOrStop.Contains(action))
            {
                onInfiniteTimerStartOrStop.Remove(action);
            }
        }

        public void RegisterOnCurrencyUpdateByTimer(Action<int> action)
        {
            if (!onCurrencyUpdateByTimer.Contains(action))
            {
                onCurrencyUpdateByTimer.Add(action);
            }
        }

        public void RemoveOnCurrencyUpdateByTimer(Action<int> action)
        {
            if (onCurrencyUpdateByTimer.Contains(action))
            {
                onCurrencyUpdateByTimer.Remove(action);
            }
        }

        #endregion

        #region Coroutine

        IEnumerator Timer()
        {
            OnTimerStart(true, false);
            TimeSpan i = new TimeSpan(0, 0, time);
            TimeSpan second = new TimeSpan(0, 0, 1);
            WaitForSeconds one = new WaitForSeconds(1);
            while (i.TotalSeconds > 0)
            {
                i = i.Subtract(second);
                OnTimerTick(i);
                time--;
                yield return one;
            }
            coroutine = null;
            Value++;
            OnCurrencyUpdateByTimer(1);
        }

        #endregion
    }

    public class TimeBaseCurrencyDataConfig
    {
        public int value;
        public DateTime lasteUpdateTime;
    }
}