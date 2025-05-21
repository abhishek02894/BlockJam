using UnityEngine;

namespace Tag.Block
{
    public class BaseBoosterUseConditions : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField, CurrencyId] internal int boosterId;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual bool CanUseBooster()
        {
            int boosterCount = DataManager.Instance.GetCurrency(boosterId).Value;
            if (boosterCount <= 0)
                return false;
            return true;
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
}
