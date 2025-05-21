using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Tag.Block
{
    public class FirebaseRemoteConfig : SerializedMonoBehaviour
    {
        #region public veriables

        public bool isInit = false;
        public bool useDevelopmentConfig = false;
        public bool isDebugString = false;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Dictionary<string, string> defaultRemoteData = new Dictionary<string, string>();
        [SerializeField] private Dictionary<string, bool> featureConfig = new Dictionary<string, bool>();
        //private FireRemoteConfig firebaseRemoteConfig;

        #endregion

        #region propertice
        //public FireRemoteConfig FirebaseRemoteConfigs => firebaseRemoteConfig;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_METHODS

        public void Init()
        {
            //firebaseRemoteConfig = FireRemoteConfig.GetInstance(FirebaseManager.Instance.App);
            //SetDefaultSetting();
            isInit = true;

        }

        public bool IsFeatureEnable(string featureKey)
        {
            if (featureConfig.ContainsKey(featureKey))
                return featureConfig[featureKey];
            return false;
        }

        [Button]
        public T GetRemoteData<T>(string key, T defaultValue)
        {
            //key = GetRemoteConfigKey(key);
            //try
            //{
            //    if (firebaseRemoteConfig.Keys.Contains(key))
            //    {
            //        if (isDebugString)
            //            Debug.Log("<color=green>Firebase Key: " + key + "</color> _JSON: " + firebaseRemoteConfig.GetValue(key).StringValue);
            //        return JsonConvert.DeserializeObject<T>(firebaseRemoteConfig.GetValue(key).StringValue);
            //    }
            //    if (isDebugString && defaultRemoteData.ContainsKey(key))
            //        Debug.Log("<color=red> Try: Firebase Key: " + key + " </color> _JSON:  : " + defaultRemoteData[key]);
            if (defaultRemoteData.ContainsKey(key))
                return JsonConvert.DeserializeObject<T>(defaultRemoteData[key]);
            //    if (isDebugString)
            //        Debug.Log("<color=red> Try: Firebase Key: " + key + " </color> _JSON:  : " + JsonConvert.SerializeObject(defaultValue));
            return defaultValue;
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError(e.Message);
            //    if (isDebugString)
            //        Debug.Log("<color=red> Catch: Firebase Key: " + key + " </color> _JSON:  : " + JsonConvert.SerializeObject(defaultValue));
            //    return defaultValue;
            //}
        }
        #endregion

        #region private methods

        private void SetDefaultSetting()
        {
            //Dictionary<string, object> defaultData = new Dictionary<string, object>();

            //foreach (var data in defaultRemoteData)
            //{
            //    if (!string.IsNullOrEmpty(data.Key) && !string.IsNullOrEmpty(data.Value))
            //        defaultData.Add(GetRemoteConfigKey(data.Key), data.Value);
            //}
            //firebaseRemoteConfig.SetDefaultsAsync(defaultData).ContinueWithOnMainThread(FetchSetting);
        }

        private void FetchSetting(Task task)
        {
            //if (!task.IsFaulted && task.IsCompleted && !task.IsCanceled)
            //{
            //    Task fetchTask = firebaseRemoteConfig.FetchAsync(TimeSpan.Zero);
            //    fetchTask.ContinueWithOnMainThread(ActiveSetting);
            //    return;
            //}

            //isInit = true;
        }

        private void ActiveSetting(Task task)
        {
            //if (!task.IsFaulted && task.IsCompleted && !task.IsCanceled)
            //{
            //    firebaseRemoteConfig.ActivateAsync();
            //    FillSetting();
            //    return;
            //}

            //isInit = true;
        }

        private void FillSetting()
        {
            SetFeatures();
            //remoteConfigFetchDataConfig = GetRemoteData(FireBaseRemoteConfigConstant.RemoteConfigFirebaseGAConfig, remoteConfigFetchDataConfig);
            isInit = true;
        }

        private void SetFeatures()
        {
            //Dictionary<string, bool> backup = featureConfig;
            //try
            //{
            //    if (firebaseRemoteConfig.Keys.Contains(GetRemoteConfigKey(FireBaseRemoteConfigConstant.FEATURE)))
            //    {
            //        featureConfig = JsonConvert.DeserializeObject<Dictionary<string, bool>>(firebaseRemoteConfig.GetValue(GetRemoteConfigKey(FireBaseRemoteConfigConstant.FEATURE)).StringValue);
            //    }
            //}
            //catch (Exception e)
            //{
            //    featureConfig = backup;
            //}
        }

        private string GetRemoteConfigKey(string key)
        {
            if (useDevelopmentConfig)
            {
                return "Dev_" + key;
            }
#if UNITY_IOS
            return key + "_IOS";
#endif
            return key;
        }

        #endregion

#if UNITY_EDITOR

        #region MyRegion
        private List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }
        [Button]
        public string GetFeatureConfigJson()
        {
            return JsonConvert.SerializeObject(featureConfig);
        }
        [Button]
        public void SetDefaultConfigJson(string str)
        {
            defaultRemoteData = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        }
        #endregion

#endif
    }

    public class FireBaseFeatureConfigConstant
    {
        public const string ADS_ENABLE = "AdsEnable";
        public const string IsReporterEnable = "IsReporterEnable";

    }

    public class FireBaseRemoteConfigConstant
    {
        public const string FEATURE = "Feature";
        public const string RATE_US_FEEDBACK = "RateUsFeedback";

    }
}
