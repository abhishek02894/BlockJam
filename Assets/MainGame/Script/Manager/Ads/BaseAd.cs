using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block {
    public class BaseAd : MonoBehaviour
    {
        #region private veriable

        private Action<bool> rewardedAdAction;
        private Action<bool> interstitialCloseAction;
        private Dictionary<AdType, List<Action>> onAdLoadEvent = new Dictionary<AdType, List<Action>>();
        private Dictionary<AdType, List<Action>> onAdWatchedEvent = new Dictionary<AdType, List<Action>>();
        private Dictionary<AdType, List<Action>> onAdWatchFailed = new Dictionary<AdType, List<Action>>();
        #endregion

        #region propertice

        private Dictionary<AdType, List<Action>> OnAdLoadEvent
        {
            get
            {
                if (onAdLoadEvent == null)
                    onAdLoadEvent = new Dictionary<AdType, List<Action>>();
                return onAdLoadEvent;
            }
        }
        private Dictionary<AdType, List<Action>> OnAdWatchedEvent
        {
            get
            {
                if (onAdWatchedEvent == null)
                    onAdWatchedEvent = new Dictionary<AdType, List<Action>>();
                return onAdWatchedEvent;
            }
        }

        private Dictionary<AdType, List<Action>> OnAdWatchFailedEvent
        {
            get
            {
                if (onAdWatchFailed == null)
                    onAdWatchFailed = new Dictionary<AdType, List<Action>>();
                return onAdWatchFailed;
            }
        }

        #endregion

        #region protected veriables

        protected Action<bool> onInitDone;

        #endregion

        #region virtual methods

        public virtual void Initialize(Action<bool> onInitDone)
        {
            this.onInitDone = onInitDone;
        }

        public virtual bool IsInitialize()
        {
            return true;
        }

        public virtual void LoadInterstitial()
        {
        }

        public virtual void ShowInterstitial(Action<bool> closeAction, string adName)
        {
            interstitialCloseAction = closeAction;
#if UNITY_IOS
            SoundManager.Instance.SetSountForAds(false);
#endif
        }

        public virtual void OnInterstitialLoadSuccess()
        {
            OnAdLoad(AdType.InterstitialAd);
        }

        public virtual void OnInterstitialLoadFail()
        {
        }
        public virtual void OnInterstitialClose()
        {
            CoroutineRunner.instance.Wait(0.1f, () =>
            {
                try
                {
                    if (interstitialCloseAction != null)
                        interstitialCloseAction?.Invoke(true);
                    OnAdWatched(AdType.InterstitialAd);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            });
#if UNITY_IOS
            SoundManager.Instance.SetSountForAds(true);
#endif
        }
        public virtual bool IsInterstitialAvailable()
        {
            return false;
        }

        public virtual void ShowRewardedVideo(Action<bool> rewardedAdSuccess, string adName)
        {
            rewardedAdAction = rewardedAdSuccess;
#if UNITY_IOS
            SoundManager.Instance.SetSountForAds(false);
#endif
        }

        public virtual void LoadRewardedVideo()
        {
        }

        public virtual void OnRewardedVideoLoadSuccess()
        {
            OnAdLoad(AdType.RewardedAd);
        }

        public virtual void OnRewardedVideoLoadFail()
        {

        }

        public virtual bool IsRewardedVideoAvailable()
        {
            return false;
        }

        public virtual void OnRewardAdShowSuccess()
        {
            CoroutineRunner.instance.Wait(0.1f, () =>
            {
                try
                {
                    if (rewardedAdAction != null)
                        rewardedAdAction?.Invoke(true);
                    OnAdWatched(AdType.RewardedAd);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            });
#if UNITY_IOS
            SoundManager.Instance.SetSountForAds(true);
#endif
        }

        public virtual void OnRewardAdShowFail()
        {
            try
            {
                if (rewardedAdAction != null)
                    rewardedAdAction?.Invoke(false);
                OnAdWatchFailed(AdType.RewardedAd);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
#if UNITY_IOS
            SoundManager.Instance.SetSountForAds(true);
#endif
        }

        public virtual void LoadBanner()
        {
        }

        public virtual void OnBannerLoadSuccess()
        {

        }

        public virtual void OnBannerLoadFail()
        {
        }

        public virtual bool IsBannerAvailable()
        {
            return false;
        }
        public virtual AdType GetHighestCMPAdType()
        {
            return AdType.RewardedAd;
        }
        public virtual void DestoryObject()
        {

        }
        #endregion

        #region public methods

        public void RegisterOnAdLoad(AdType adType, Action action)
        {
            if (!OnAdLoadEvent.ContainsKey(adType))
                OnAdLoadEvent.Add(adType, new List<Action>());
            if (!OnAdLoadEvent[adType].Contains(action))
                OnAdLoadEvent[adType].Add(action);
        }

        public void DeregisterOnAdLoad(AdType adType, Action action)
        {
            if (!OnAdLoadEvent.ContainsKey(adType))
                return;
            if (OnAdLoadEvent[adType].Contains(action))
                OnAdLoadEvent[adType].Remove(action);
        }
        private void OnAdLoad(AdType adType)
        {
            if (!OnAdLoadEvent.ContainsKey(adType))
                return;
            try
            {
                for (int i = 0; i < OnAdLoadEvent[adType].Count; i++)
                {
                    OnAdLoadEvent[adType][i]?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void RegisterOnAdWatched(AdType adType, Action action)
        {
            if (!OnAdWatchedEvent.ContainsKey(adType))
                OnAdWatchedEvent.Add(adType, new List<Action>());
            if (!OnAdWatchedEvent[adType].Contains(action))
                OnAdWatchedEvent[adType].Add(action);
        }

        public void DeregisterOnAdWatched(AdType adType, Action action)
        {
            if (!OnAdWatchedEvent.ContainsKey(adType))
                return;
            if (OnAdWatchedEvent[adType].Contains(action))
                OnAdWatchedEvent[adType].Remove(action);
        }

        private void OnAdWatched(AdType adType)
        {
            if (!OnAdWatchedEvent.ContainsKey(adType))
                return;
            try
            {
                for (int i = 0; i < OnAdWatchedEvent[adType].Count; i++)
                {
                    OnAdWatchedEvent[adType][i]?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void RegisterOnAdWatchFailed(AdType adType, Action action)
        {
            if (!OnAdWatchFailedEvent.ContainsKey(adType))
                OnAdWatchFailedEvent.Add(adType, new List<Action>());
            if (!OnAdWatchFailedEvent[adType].Contains(action))
                OnAdWatchFailedEvent[adType].Add(action);
        }

        public void DeregisterOnAdWatchFailed(AdType adType, Action action)
        {
            if (!OnAdWatchFailedEvent.ContainsKey(adType))
                return;
            if (OnAdWatchFailedEvent[adType].Contains(action))
                OnAdWatchFailedEvent[adType].Remove(action);
        }

        private void OnAdWatchFailed(AdType adType)
        {
            if (!OnAdWatchFailedEvent.ContainsKey(adType))
                return;
            try
            {
                for (int i = 0; i < OnAdWatchFailedEvent[adType].Count; i++)
                {
                    OnAdWatchFailedEvent[adType][i]?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        #endregion
    }

    public enum AdType
    {
        RewardedAd,
        InterstitialAd,
        BannerAd
    }
}