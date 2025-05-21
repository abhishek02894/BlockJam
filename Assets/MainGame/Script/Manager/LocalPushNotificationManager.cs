using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Android;

namespace Tag.Block
{
    public class LocalPushNotificationManager : SerializedManager<LocalPushNotificationManager>
    {
        #region PRIVATE_VARS

        //[SerializeField] private string channelId;
        //public List<BaseLocalNotification> localNotifications = new List<BaseLocalNotification>();
        //private AndroidNotificationChannel notificationChannel;

        #endregion

        #region PUBLIC_VARS

        #endregion

        #region Propertices

        //private bool IsLocalNotificationEnable => FirebaseManager.Instance.FirebaseRemoteConfig.IsFeatureEnable(FireBaseFeatureConfigConstant.LOCAL_PUSH_NOTIFICATION_ENABLE);
        //private bool IsNotificationRequestPermissionPopUpEnable => FirebaseManager.Instance.FirebaseRemoteConfig.IsFeatureEnable(FireBaseFeatureConfigConstant.IsNotificationRequestPermissionPopUpEnable);

        #endregion

        #region Overrided_Method
        #endregion

        #region UNITY_CALLBACKS

        //public void Start()
        //{
        //if (IsNotificationRequestPermissionPopUpEnable)
        //    CheckAndRequestPermission();
        //if (IsLocalNotificationEnable)
        //{
        //    AndroidNotificationIntentData androidNotificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        //    if (androidNotificationIntentData != null)
        //    {
        //        AnalyticsEventManager.Instance.LogEvent_LocalNotification(androidNotificationIntentData.Notification.Title.GetEventTitle());
        //    }
        //}
        //ClearAllNotificationData();
        //}

        //public void OnApplicationPause(bool pause)
        //{
        //if (IsLocalNotificationEnable)
        //    ScheduleNotification(pause);
        //}

        #endregion

        #region PUBLIC_FUNCTIONS

        public void ScheduleNotification(bool pause)
        {
            //if (!GameplayUIManager.Instance || !IsLocalNotificationEnable)
            //    return;

            //if (pause)
            //{
            //    CreateChannel();
            //    for (int i = 0; i < localNotifications.Count; i++)
            //    {
            //        localNotifications[i].ScheduleNotification(channelId);
            //    }
            //}
            //else
            //{
            //    ClearAllNotificationData();
            //}
        }

        //public void ClearAllNotificationData()
        //{
        //    AndroidNotificationCenter.CancelAllNotifications();
        //    AndroidNotificationCenter.DeleteNotificationChannel(channelId);
        //}

        #endregion

        #region PRIVATE_FUNCTIONS

        //private void CreateChannel()
        //{
        //    notificationChannel = GetNotificationChannelSetting();
        //    AndroidNotificationCenter.RegisterNotificationChannel(notificationChannel);
        //}

        //private AndroidNotificationChannel GetNotificationChannelSetting()
        //{
        //    AndroidNotificationChannel notificationChannel = new AndroidNotificationChannel()
        //    {
        //        Id = channelId,
        //        Name = channelId + "channel",
        //        Description = "Home Town notification",
        //        EnableVibration = true,
        //        LockScreenVisibility = LockScreenVisibility.Public,
        //        Importance = Importance.High,
        //    };
        //    return notificationChannel;
        //}
        //void CheckAndRequestPermission()
        //{
        //    if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        //    {
        //        Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        //    }
        //}
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
        //  [Button]
        //  public void SendPushNotification(string playfabId, string Subject, string Message)
        //  {
        //      SendPushNotificationRequest request = new SendPushNotificationRequest
        //      {
        //          Message = Message,
        //          Recipient = playfabId,
        //          Subject = Subject,
        //          TargetPlatforms = new List<PushNotificationPlatform>
        //{
        //  PushNotificationPlatform.GoogleCloudMessaging // Use the appropriate platform for your needs (e.g., GoogleCloudMessaging for Android)
        //      }
        //      };
        //      PlayFabServerAPI.SendPushNotification(request, OnSucess, error =>
        //      {
        //          Debug.LogError("SendPushNotificationRequest failed : " + error.ErrorMessage);
        //      });
        //  }

        //  private void OnSucess(SendPushNotificationResult result)
        //  {
        //      Debug.LogError("SendPushNotificationRequest Success");

        //  }
    }
}