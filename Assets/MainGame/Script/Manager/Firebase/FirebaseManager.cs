using System;
using System.Collections;
using UnityEngine;

namespace Tag.Block
{
    public class FirebaseManager : Manager<FirebaseManager>
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        //[SerializeField] private FirebaseCrashlytics crashlytics;
        [SerializeField] private FirebaseRemoteConfig remoteConfig;
        //[SerializeField] private FirebaseAnalytics firebaseAnalytics;
        //[SerializeField] private FirebaseCloudMessaging firebaseCloudMessaging;
        //[SerializeField] private FirebaseApp app;
        private bool isInit = false;
        //private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        #endregion

        #region Propertices

        //public FirebaseAnalytics FirebaseAnalytics
        //{
        //    get => firebaseAnalytics;
        //}

        //public FirebaseApp App
        //{
        //    get => app;
        //}

        public FirebaseRemoteConfig FirebaseRemoteConfig
        {
            get => remoteConfig;
        }
        //public FirebaseCloudMessaging FirebaseCloudMessaging
        //{
        //    get => firebaseCloudMessaging;
        //}
        #endregion

        #region UNITY_CALLBACKS

        private void Start()
        {
            InternetManager.Instance.CheckNetConnection(OnInternetCheckFinish);
        }

        #endregion

        #region PRIVATE_METHODS

        private void OnInternetCheckFinish()
        {
            StartCoroutine(InitWaitCo());
            try
            {
                //ContinueWithOnMainThread for fixing 'Input Dispatch Timed out ANR'
                //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                //{
                //    dependencyStatus = task.Result;
                //    if (dependencyStatus == DependencyStatus.Available)
                //    {
                //        app = Firebase.FirebaseApp.DefaultInstance;
                        isInit = true;
                //    }
                //    else
                //    {
                //        Debug.Log("Could not resolve all Firebase dependencies: " + dependencyStatus);
                //    }
                //});
            }
            catch (Exception e)
            {
                Debug.Log(e + "Async task failed");
            }
        }


        private void Init()
        {
            //FirebaseApp.LogLevel = LogLevel.Error;
            StartCoroutine(LoadAllFirebaseComponent());
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS

        #endregion

        #region COROUTINES

        private IEnumerator InitWaitCo()
        {
            while (!isInit)
            {
                yield return null;
            }
            Init();
        }

        private IEnumerator LoadAllFirebaseComponent()
        {
            remoteConfig.Init();
            while (!remoteConfig.isInit)
                yield return null;
            //crashlytics.Init();
            //while (!crashlytics.isInit)
            //    yield return null;
            //firebaseAnalytics.Init();
            //while (!firebaseAnalytics.isInit)
            //    yield return null;
            //if (remoteConfig.IsFeatureEnable(FireBaseFeatureConfigConstant.PUSH_NOTIFICATION_ENABLE))
            //{
            //    firebaseCloudMessaging.Init();
            //    while (!firebaseCloudMessaging.isInit)
            //        yield return null;
            //}
            OnLoadingDone();
        }

        #endregion
    }
}