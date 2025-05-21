using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tag.Block
{
    public static class PlayerPersistantData
    {
        #region PUBLIC_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        private static PersistantVariable<MainPlayerProgressData> _mainPlayerProgressData = new PersistantVariable<MainPlayerProgressData>(PlayerPrefsKeys.Main_Player_Progress_Data_Key, null);
        private static PersistantVariable<PlayerProfileData> _playerProfileData = new PersistantVariable<PlayerProfileData>(PlayerPrefsKeys.Player_Profile_Data_Key, null);
        private static Dictionary<int, Currency> _currencyDict = new Dictionary<int, Currency>();

        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public static MainPlayerProgressData GetMainPlayerProgressData()
        {
            return _mainPlayerProgressData.Value;
        }

        public static void SetMainPlayerProgressData(MainPlayerProgressData mainPlayerProgressData)
        {
            _mainPlayerProgressData.Value = mainPlayerProgressData;
        }

        public static PlayerProfileData GetPlayerProfileData()
        {
            return _playerProfileData.Value;
        }

        public static void SetPlayerProfileData(PlayerProfileData playerProfileData)
        {
            _playerProfileData.Value = playerProfileData;
        }

        public static Dictionary<int, Currency> GetCurrancyDictionary()
        {
            return _currencyDict;
        }

        public static void SetCurrancyDictionary(Dictionary<int, Currency> currencyDict)
        {
            _currencyDict = currencyDict;
        }

        public static Dictionary<string, string> GetPlayerPersistantCurrancyData()
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            foreach (var pair in _currencyDict)
            {
                dataDictionary.Add(pair.Value.key, pair.Value.Value.ToString());
            }
            return dataDictionary;
        }

        public static void SetPlayerPersistantCurrancyData(Dictionary<string, string> currancyData)
        {
            foreach (var pair in currancyData)
            {
                foreach (var values in _currencyDict.Values)
                {
                    if (values.key == pair.Key && int.TryParse(pair.Value, out int currancyVal))
                    {
                        values.SetValue(currancyVal);
                        break;
                    }
                }
            }
        }

        public static Dictionary<string, string> GetPlayerPrefsData()
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            dataDictionary.Add(PlayerPrefsKeys.Currancy_Data_Key, SerializeUtility.SerializeObject(GetPlayerPersistantCurrancyData()));
            return dataDictionary;
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region COROUTINES

        #endregion

        #region UI_CALLBACKS

        #endregion
    }

    #region MAIN_PLAYER_PROGRESS_DATA

    public class MainPlayerProgressData
    {
        [JsonProperty("pglev")] public int playerGameplayLevel;
        [JsonProperty("pws")] public int winStreak;
        [JsonProperty("pls")] public int loseStreak;
        [JsonProperty("naet")] public DateTime noAdsEndTime = DateTime.MinValue;

        public void AddNoAdsPurchase(int minutes)
        {
            if (noAdsEndTime >= DateTime.Now)
            {
                noAdsEndTime = noAdsEndTime.AddMinutes(minutes);
            }
            else
            {
                noAdsEndTime = DateTime.Now;
                noAdsEndTime = noAdsEndTime.AddMinutes(minutes);
            }
            CoroutineRunner.instance.Wait(0.1f, () =>
            {
                //AdManager.Instance.HideBannerAd();
                //if (RemoveAdsHandler.Instance != null)
                //    RemoveAdsHandler.Instance.OnRemoveAdsPurchase();
            });
        }

        public bool IsNoAdsActive()
        {
            return noAdsEndTime.Subtract(DateTime.Now).TotalSeconds > 0;
        }
    }

    public class CurrencyMappingData
    {
        [JsonProperty("cid"), CurrencyId] public int currencyID;
        [JsonProperty("cur")] public Currency currency;
    }
    #endregion

    public class PlayerProfileData
    {
        [JsonProperty("pn")] public string playerName;
        [JsonProperty("ai")] public int avtarId = 0;
        [JsonProperty("fi")] public int frameId = 0;
    }

    public class PlayerPrefsKeys
    {
        public const string Currancy_Data_Key = "CurrancyPlayerData";
        public const string Main_Player_Progress_Data_Key = "MainPlayerProgressData";
        public const string Tutorial_Player_Data_Key = "TutorialPlayerData";
        public const string Level_Progress_Data_Key = "LevelProgressData";
        public const string GameStats_Player_Data_Key = "GameStatsPlayerData";
        public const string Player_Profile_Data_Key = "PlayerProfileData";
    }
}