using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace Tag.Block
{
    public class AdManager : SerializedManager<AdManager>
    {
        #region PUBLIC_VARS
        [SerializeField] private BaseAd ads;
        #endregion

        #region Propertices
        public bool IsAdEnable => false;
        #endregion

        #region Overrided_Method
        #endregion

        #region PRIVATE_VARS
        #endregion

        #region UNITY_CALLBACKS
        public void Start()
        {
            if (IsAdEnable)
            {
                //InternetManager.Instance.CheckNetConnection(() => { Initialize(1); });
                Initialize();
            }
            else
                OnLoadingDone();
        }
        public override void OnDestroy()
        {
            ads.DestoryObject();
            StopAllCoroutines();
            base.OnDestroy();
        }

        #endregion

        #region PUBLIC_FUNCTIONS
        //private Coroutine testCO;
        public void ShowAD(Action<bool> successAction, string adName)
        {
            if (!IsAdEnable)
                successAction?.Invoke(false);
#if UNITY_EDITOR
            successAction?.Invoke(true);
            return;
#endif
            AdType adType = ads.GetHighestCMPAdType();
            Debug.LogError("adNameType " + adType.ToString());
            //testCO = StartCoroutine(TestCO());
            //StartCoroutine(TestCO2());
            if (adType == AdType.RewardedAd)
            {
                ShowRewardAd(successAction, adName);

            }
            if (adType == AdType.InterstitialAd)
            {
                ShowInterstitialAd(successAction, adName);
            }
        }

        private void ShowRewardAd(Action<bool> successAction, string adName)
        {
            if (ads.IsRewardedVideoAvailable())
            {
                ads.ShowRewardedVideo(successAction, adName);
                return;
            }
            successAction?.Invoke(false);
        }
        public void ShowInterstitialAd(Action<bool> onCloseAction, string adName)
        {
            if (ads.IsInterstitialAvailable())
            {
                ads.ShowInterstitial(onCloseAction, adName);
                return;
            }
            onCloseAction?.Invoke(true);
        }

        public void LoadInterstitialAd()
        {
            ads.LoadInterstitial();
        }

        public void RegisterOnAdLoad(AdType adType, Action action)
        {
            ads.RegisterOnAdLoad(adType, action);
        }

        public void DeregisterOnAdLoad(AdType adType, Action action)
        {
            ads.DeregisterOnAdLoad(adType, action);
        }

        public void RegisterOnAdWatched(AdType adType, Action action)
        {
            ads.RegisterOnAdWatched(adType, action);
        }

        public void DeregisterOnAdWatched(AdType adType, Action action)
        {
            ads.DeregisterOnAdWatched(adType, action);
        }
        public void RegisterOnAdWatchFailed(AdType adType, Action action)
        {
            ads.RegisterOnAdWatchFailed(adType, action);
        }

        public void DeregisterOnWatchFailed(AdType adType, Action action)
        {
            ads.DeregisterOnAdWatchFailed(adType, action);
        }

        public void StartTestSuit()
        {
            //MaxSdk.ShowMediationDebugger();
            //Debug.LogError("StartTestSuit");
        }

        public bool IsRewardVideoAvailable()
        {
            if (!IsAdEnable)
                return false;
#if UNITY_EDITOR
            return true;
#endif
            return ads.IsRewardedVideoAvailable();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void Initialize()
        {
            ads.Initialize(b =>
            {
                OnLoadingDone();
            });
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
   
}
