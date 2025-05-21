using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Tag.Block
{
    public class DataManager : SerializedManager<DataManager>
    {
        #region private veriables

        [SerializeField] private PlayerPersistantDefaultDataHandler _playerPersistantDefaultDataHandler;

        private string FirstSessionStartTime
        {
            get { return PlayerPrefs.GetString(FirstSessionStartTime_PrefsKey, CustomTime.GetCurrentTime().GetPlayerPrefsSaveString()); }
            set { PlayerPrefs.SetString(FirstSessionStartTime_PrefsKey, value); }
        }

        private string FirstSessionStartTime_PrefsKey = "FirstSessioStartTimePrefsData";

        private string InstallTime
        {
            get { return PlayerPrefs.GetString(InstallTime_PrefsKey, Utility.GetUnixTimestamp().ToString()); }
            set { PlayerPrefs.SetString(InstallTime_PrefsKey, value); }
        }

        private int CurrentBundleVersionCode
        {
            get { return PlayerPrefs.GetInt("CurrentBundleVersionCodeKey", 0); }
            set { PlayerPrefs.SetInt("CurrentBundleVersionCodeKey", value); }
        }

        private string InstallTime_PrefsKey = "InstallTimePrefsData";

        #endregion

        #region public static

        #endregion

        #region propertices

        public bool isFirstSession;
        public DateTime FirstSessionStartDateTime
        {
            get
            {
                CustomTime.TryParseDateTime(FirstSessionStartTime, out DateTime firstSessionDT);
                return firstSessionDT;
            }
        }

        public long InstallUnixTime
        {
            get
            {
                if (long.TryParse(InstallTime, out long installTime))
                    return installTime;

                return Utility.GetUnixTimestamp();
            }
        }

        public static MainPlayerProgressData PlayerData
        {
            get
            {
                return PlayerPersistantData.GetMainPlayerProgressData();
            }

            set
            {
                PlayerPersistantData.SetMainPlayerProgressData(value);
            }
        }

        #endregion

        #region Unity_callback

        public override void Awake()
        {
            base.Awake();
            PlayerPrefbsHelper.SaveData = true;

            isFirstSession = PlayerPersistantData.GetMainPlayerProgressData() == null;
            if (isFirstSession)
            {
                FirstSessionStartTime = CustomTime.GetCurrentTime().Date.GetPlayerPrefsSaveString();
                InstallTime = Utility.GetUnixTimestamp().ToString();
            }

            _playerPersistantDefaultDataHandler.CheckForDefaultDataAssignment();
            OnLoadingDone();

            if (CurrentBundleVersionCode != VersionCodeFetcher.GetBundleVersionCode())
            {
                Debug.LogError("Reset Level Data Due To New Build Chanages");
            }
            CurrentBundleVersionCode = VersionCodeFetcher.GetBundleVersionCode();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnCurrencyUnload();
        }

        public Currency GetCurrency(int currencyID)
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            if (currencyDict.ContainsKey(currencyID))
                return currencyDict[currencyID];
            return null;
        }

        public void SaveData(MainPlayerProgressData playerData)
        {
            PlayerData = playerData;
        }

        [Button]
        public void IncreaseLevel()
        {
            var playerData = PlayerData;
            playerData.playerGameplayLevel++;
            SaveData(playerData);
        }
        #endregion

        #region private Methods

        private void OnCurrencyUnload()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            foreach (var item in currencyDict)
            {
                item.Value.OnDestroy();
            }
        }

        #endregion

        #region public methods
        [Button]
        public void SetLevel_Editor(int level)
        {
            var playerData = PlayerData;
            playerData.playerGameplayLevel = level;
            SaveData(playerData);
        }
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
        [Button]
        public void Editor_PrintPlayerPersistantData()
        {
            var allPlayerData = PlayerPersistantData.GetPlayerPrefsData();
            foreach (var keyValuePair in allPlayerData)
            {
                Debug.Log(string.Format("{0} - {1}", keyValuePair.Key, keyValuePair.Value));
            }
        }

        
#endif
        #endregion

    }

}