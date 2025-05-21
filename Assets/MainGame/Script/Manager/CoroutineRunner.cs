using System;
using System.Collections;
using UnityEngine;

namespace Tag.Block {
    public class CoroutineRunner : MonoBehaviour
    {
        #region properties

        private static CoroutineRunner _coroutineRunner;

        public static CoroutineRunner instance
        {
            get
            {
                if (_coroutineRunner == null)
                {
                    GameObject obj = new GameObject("CoroutineRunner");
                    obj.AddComponent<CoroutineRunner>();
                }
                return _coroutineRunner;
            }
        }

        #endregion

        #region unity callback

        private void Awake()
        {
            _coroutineRunner = this;
        }
        private void OnDestroy()
        {
            AllCoroutiesStop();
            if (_coroutineRunner != null)
                _coroutineRunner = null;
        }


        #endregion

        #region public static methods

        //Pass End time Here
        public Coroutine WaitAndInvoke(DateTime endTime, Action action)
        {
            return StartCoroutine(WaitForEndAndInvoke(endTime, action));
        }

        public Coroutine Wait(float wait, Action action)
        {
            return StartCoroutine(WaitTime(wait, action));
        }

        public Coroutine InvokeRepeat(float wait, Action action)
        {
            return StartCoroutine(RepeatInvoke(action, wait));
        }

        public Coroutine CoroutineStart(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        public void CoroutineStop(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        public void AllCoroutiesStop()
        {
            StopAllCoroutines();
        }
        #endregion

        #region Coroutine

        IEnumerator WaitTime(float wait, Action action)
        {
            yield return new WaitForSeconds(wait);
            action?.Invoke();
        }

        IEnumerator RepeatInvoke(Action action, float wait)
        {
            WaitForSeconds w = new WaitForSeconds(wait);
            while (true)
            {
                yield return w;
                action?.Invoke();
            }
        }

        IEnumerator WaitForEndAndInvoke(DateTime endTime, Action action)
        {
            DateTime startTime = endTime;
            WaitForSeconds seconds = new WaitForSeconds(1);
            TimeSpan timeSpan = startTime - TimeManager.Now;
            while (timeSpan.TotalSeconds > 0)
            {
                timeSpan = startTime - TimeManager.Now;
                yield return seconds;
            }
            action?.Invoke();
        }
        #endregion
    }
}