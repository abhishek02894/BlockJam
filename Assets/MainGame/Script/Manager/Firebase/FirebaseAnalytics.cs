//using Firebase.Analytics;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using FireAnalytics = Firebase.Analytics.FirebaseAnalytics;

//namespace Tag
//{
//    public class FirebaseAnalytics : MonoBehaviour
//    {
//        #region PUBLIC_VARS

//        public bool isInit = false;

//        #endregion

//        #region PRIVATE_VARS

//        #endregion

//        #region UNITY_CALLBACKS

//        #endregion

//        #region PUBLIC_FUNCTIONS

//        public void Init()
//        {
//            //Debug.Log("FirebaseAnalytics Init Successfully");

//            //DebugLog("Enabling data collection.");
//            FireAnalytics.SetAnalyticsCollectionEnabled(true);

//            //DebugLog("Set user properties.");
//            // Set the user's sign up method.
//            FireAnalytics.SetUserProperty(FireAnalytics.UserPropertySignUpMethod, "Guest");

//            // Set the user ID.
//            string userID = SystemInfo.deviceUniqueIdentifier;
//            FireAnalytics.SetUserId(userID);

//            // Set default session duration values.
//            //FirebaseAnalytics.SetMinimumSessionDuration(new TimeSpan(0, 0, 10));
//            FireAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 5, 0));
//            isInit = true;
//        }

//        public void LogEvent(string eventName)
//        {
//            if (!isInit)
//                return;
//            FireAnalytics.LogEvent(eventName);
//        }

//        public void LogEventWithParameters(string eventName, Dictionary<string, string> data)
//        {
//            if (!isInit)
//                return;
//            FireAnalytics.LogEvent(eventName, ConvertToArrayOfParameters(data));
//        }

//        public void LogEventWithParameters(string eventName, Parameter[] data)
//        {
//            if (!isInit)
//                return;
//            FireAnalytics.LogEvent(eventName, data);
//        }

//        #endregion

//        #region PRIVATE_FUNCTIONS

//        private Parameter[] ConvertToArrayOfParameters(Dictionary<string, string> data)
//        {
//            Parameter[] parameters = new Parameter[data.Count];
//            int count = 0;
//            foreach (var item in data)
//            {
//                parameters[count] = new Parameter(item.Key, item.Value);
//                count++;
//            }

//            return parameters;
//        }

//        #endregion

//        #region CO-ROUTINES

//        #endregion

//        #region EVENT_HANDLERS

//        #endregion

//        #region UI_CALLBACKS

//        #endregion
//    }
//}