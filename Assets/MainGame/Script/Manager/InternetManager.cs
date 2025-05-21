using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Tag.Block {
    public class InternetManager : Manager<InternetManager>
    {
        #region private veriabels

        [SerializeField] private string netCheckURL;
        [SerializeField] private float internetCheckingInterval;

        private WaitForSeconds waitForSeconds;

        #endregion

        #region unitycallback

        private void Start()
        {
            waitForSeconds = new WaitForSeconds(internetCheckingInterval);
#if UNITY_EDITOR
            OnLoadingDone();
            return;
#endif
            CheckNetConnection(OnNetConnectionResponse);
        }

        #endregion

        #region private methods

        private void OnNetConnectionResponse(bool isNetAvailable)
        {
            if (isNetAvailable)
                OnLoadingDone();
            else
            {
                Debug.LogError("No Internet available");
                //GlobalUIManager.Instance.GetView<NoInternetView>().Show(() => { CheckNetConnection(OnNetConnectionResponse); });
            }
        }

        private bool IsReachableToNetwork()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }

        #endregion

        #region public methods

        public void CheckNetConnection(Action<bool> onCheckingDone)
        {
            StartCoroutine(CheckInternetConnection(onCheckingDone));
        }

        public void CheckNetConnection(Action onInternetRestore)
        {
            if (IsReachableToNetwork())
            {
                onInternetRestore?.Invoke();
                return;
            }

            CheckNetConnection(b =>
            {
                if (b)
                    onInternetRestore?.Invoke();
                else
                {
                    Debug.LogError("No Internet available");
                    //GlobalUIManager.Instance.GetView<NoInternetView>().Show(() => { CheckNetConnection(OnNetConnectionResponse); });
                }
            });
        }

        public void StartInternetCheckingInRepeat()
        {
            StartCoroutine(CheckInternetInRepeat());
        }

        #endregion

        #region Coroutine

        IEnumerator CheckInternetConnection(Action<bool> action)
        {
            bool result;
            using (var request = UnityWebRequest.Head(netCheckURL))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                result = !request.isNetworkError && !request.isHttpError && request.responseCode == 200;
            }
            action(result);
        }

        IEnumerator CheckInternetInRepeat()
        {
            yield return waitForSeconds;
            if (IsReachableToNetwork())
            {
                StartInternetCheckingInRepeat();
            }
            else
            {
                CheckNetConnection(StartInternetCheckingInRepeat);
            }
        }
        #endregion
    }
}