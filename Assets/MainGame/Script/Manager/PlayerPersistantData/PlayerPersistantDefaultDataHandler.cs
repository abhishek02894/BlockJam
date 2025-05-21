using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class PlayerPersistantDefaultDataHandler : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private MainPlayerProgressData _defaultMainPlayerProgressData;
        [SerializeField] private PlayerProfileData _defaultPlayerProfileData;
        [SerializeField] private List<CurrencyMappingData> _currencyMappingDatas = new List<CurrencyMappingData>();
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS

        public void CheckForDefaultDataAssignment()
        {
            var mainProgressData = PlayerPersistantData.GetMainPlayerProgressData();
            if (mainProgressData == null)
                PlayerPersistantData.SetMainPlayerProgressData(_defaultMainPlayerProgressData);

            var playerProfileData = PlayerPersistantData.GetPlayerProfileData();
            if (playerProfileData == null)
                PlayerPersistantData.SetPlayerProfileData(_defaultPlayerProfileData);

            MapCurrency();
            CurrencyInit();
        }

        #endregion

        #region PRIVATE_METHODS

        private void MapCurrency()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            for (int i = 0; i < _currencyMappingDatas.Count; i++)
            {
                currencyDict.Add(_currencyMappingDatas[i].currencyID, _currencyMappingDatas[i].currency);
            }
            PlayerPersistantData.SetCurrancyDictionary(currencyDict);
        }

        private void CurrencyInit()
        {
            var currencyDict = PlayerPersistantData.GetCurrancyDictionary();
            foreach (var item in currencyDict)
            {
                item.Value.Init();
            }
        }

        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}