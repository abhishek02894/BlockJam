using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Tag.Block
{
    public class DeviceManager : Manager<DeviceManager>
    {
        #region private methods
        [SerializeField] private int tergetFPS = 60;
        [SerializeField] private List<string> deviceIds = new List<string>();
        [SerializeField] private bool isLogEnable;
        [SerializeField, ShowIf("isLogEnable")] private bool isSetDefaultData;
        [SerializeField, ShowIf("isSetDefaultData")] private string defaultGamedata;
        private bool isInit;

        public bool IsInit { get => isInit; }
        public List<string> DeviceIds { get => deviceIds; }

        public bool IsDefaultGamedataOneTimeLoaded
        {
            get { return PlayerPrefbsHelper.GetInt("IsDefaultGamedataOneTimeLoaded", 0) == 1; }
            set { PlayerPrefbsHelper.SetInt("IsDefaultGamedataOneTimeLoaded", (value) ? 1 : 0); }
        }
        #endregion

        #region unity callback

        public override void Awake()
        {
            isInit = false;
            base.Awake();
            if (IsPackageIdSame())
                OnLoadingDone();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
                Application.runInBackground = true;
#endif
            if (isLogEnable && isSetDefaultData && !IsDefaultGamedataOneTimeLoaded)
            {
                PlayerPrefbsHelper.SaveDataToPrefabsForDefaultGameData(defaultGamedata);
                IsDefaultGamedataOneTimeLoaded = true;
            }
            Application.targetFrameRate = tergetFPS;
            StartCoroutine(Wait());
        }
        #endregion

        #region public methods
        [Button]
        public string GetDeviceID()
        {
#if UNITY_EDITOR || UNITY_ANDROID
            return SystemInfo.deviceUniqueIdentifier;
#endif
            return "";
        }
        public bool IsDeveloper()
        {
            return DeviceIds.Contains(GetDeviceID());
        }
        public bool IsPackageIdSame()
        {
            return Application.identifier == "com.dragontales.mergegames";
        }
        public void SetDebugType()
        {
            if (IsDeveloper())
            {
                Debug.unityLogger.filterLogType = LogType.Log;
            }
            else
            {
                if (isLogEnable)
                    Debug.unityLogger.filterLogType = LogType.Log;
                else
                    Debug.unityLogger.filterLogType = LogType.Error;
            }
        }
        #endregion

        IEnumerator Wait()
        {
            while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsLoaded)
                yield return null;
            //deviceIds = FirebaseManager.Instance.FirebaseRemoteConfig.GetRemoteData(FireBaseRemoteConfigConstant.DEVLOPER_DEVICE_IDS, DeviceIds);
            isInit = true;
        }
    }
}