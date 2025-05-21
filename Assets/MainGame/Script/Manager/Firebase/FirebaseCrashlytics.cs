//using Firebase.Crashlytics;
//using System;
//using UnityEngine;

//namespace Tag
//{
//    public class FirebaseCrashlytics : MonoBehaviour
//    {
//        public bool isInit = false;

//        public void Init()
//        {
//            //Debug.Log("FirebaseCrashlytics Init Successfully");
//            isInit = true;
//            string userID = SystemInfo.deviceUniqueIdentifier;
//            Crashlytics.SetUserId(userID);
//        }


//        // End our session when the program exits.
//        void OnDestroy()
//        {
//        }

//        // Causes an error that will crash the app at the platform level (Android or iOS)
//        public void ThrowUncaughtException()
//        {
//            DebugLog("Causing a platform crash.");
//            throw new InvalidOperationException("Uncaught exception created from UI.");
//        }

//        // Log a caught exception.
//        public void LogCaughtException()
//        {
//            DebugLog("Catching an logging an exception.");
//            try
//            {
//                throw new InvalidOperationException("This exception should be caught");
//            }
//            catch (Exception ex)
//            {
//                Crashlytics.LogException(ex);
//            }
//        }

//        public void LogCaughtException(Exception ex)
//        {
//            Crashlytics.LogException(ex);
//        }

//        // Write to the Crashlytics session log
//        public void WriteCustomLog(String s)
//        {
//            DebugLog("Logging message to Crashlytics session: " + s);
//            Crashlytics.Log(s);
//        }

//        // Output text to the debug log text field, as well as the console.
//        public void DebugLog(string s)
//        {
//            // print(s);
//        }
//    }
//}