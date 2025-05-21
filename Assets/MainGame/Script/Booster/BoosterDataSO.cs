using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    [CreateAssetMenu(fileName = "BoosterData", menuName = Constant.GAME_NAME + "/BoosterData")]
    public class BoosterDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARS

        public List<BoosterData> boosterDatas = new List<BoosterData>();

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public BoosterData GetBoosterData(int id)
        {
            return boosterDatas.Find(x => x.boosterID == id);
        }

        public BoosterData CanShowBoosterUnlock(int level)
        {
            for (int i = 0; i < boosterDatas.Count; i++)
            {
                if (boosterDatas[i].CanShow(level))
                    return boosterDatas[i];
            }
            return null;
        }

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
    public class BoosterData
    {
        [CurrencyId] public int boosterID;
        public string boosterName;
        public string boosterDes;
        public string boosterPrefskey;
        public int unlockLevel;
        public int defaultBoosterCount;
        public Sprite boosterSprite;
        public int purchaseCoinAmount;

        public bool CanShow(int level)
        {
            return PlayerPrefs.GetInt(boosterPrefskey) == 0 && unlockLevel == level;
        }

        public void SetAsShow()
        {
            PlayerPrefs.SetInt(boosterPrefskey, 1);
        }
    }
}
