using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tag.Block
{
    public abstract class BaseLocalNotification
    {
        #region PRIVATE_VARS
        private bool IsEnable
        {
            get => false;//GamePlaySettingManager.Instance.IsNotificationOn && FirebaseManager.Instance.FirebaseRemoteConfig.IsFeatureEnable(FireBaseFeatureConfigConstant.LOCAL_PUSH_NOTIFICATION_ENABLE);
        }
        #endregion

        #region PUBLIC_VARS
        public NotificationData notificationData = new NotificationData();
        #endregion

        #region Propertices
        #endregion

        #region Virtual_Method
        //public virtual AndroidNotification GetAndroidNotification()
        //{
        //    return new AndroidNotification()
        //    {
        //        Title = notificationData.title,
        //        Text = notificationData.text,
        //        SmallIcon = "small_icon",
        //        LargeIcon = "large_icon",
        //        ShowTimestamp = false,
        //    };
        //}

        public virtual bool CanSendNotification()
        {
            return IsEnable;
        }
        public abstract double GetFireTime();
        public abstract void ScheduleNotification(string channelId);
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
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
[System.Serializable]
public class NotificationData
{
    public string title;
    public string text;
}