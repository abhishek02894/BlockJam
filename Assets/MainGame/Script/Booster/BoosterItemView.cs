using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.Block
{
    public class BoosterItemView : MonoBehaviour
    {
        #region PUBLIC_VARS
        public int BoosterId { get => boosterId; }

        #endregion

        #region PRIVATE_VARS

        [SerializeField] GameObject coinGO;
        [SerializeField] GameObject lockGO;
        [SerializeField] GameObject unLockGO;
        [SerializeField] GameObject boosterCountGO;
        [SerializeField] private BoosterDataSO boosterDataSO;
        [SerializeField, CurrencyId] int boosterId;
        [SerializeField] private BoosterCurrency boosterCurrency;
        [SerializeField] Text boosterCount;
        [SerializeField] Text lockLevel;
        [SerializeField] BaseBoosterUseConditions boosterUseConditions;
        [SerializeField] Text coinAmountText;
        private BoosterData boosterData;
        [SerializeField] private float boosteCoolDownTime = 0.2f;
        private static bool inCoolDown = false;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void SetView()
        {
            boosterData = boosterDataSO.GetBoosterData(boosterId);
            if (boosterData == null)
                return;

            if (boosterData.unlockLevel <= DataManager.PlayerData.playerGameplayLevel)
            {
                SetUnlockGO(false);
                boosterCount.text = $"{boosterCurrency.Value}";
            }
            else
            {
                SetUnlockGO(true);
                lockLevel.text = "Lv." + boosterData.unlockLevel;
            }
            coinGO.SetActive(boosterCurrency.Value <= 0);
            boosterCountGO.SetActive(boosterCurrency.Value >= 1);
            coinAmountText.text = boosterData.purchaseCoinAmount.ToString();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetUnlockGO(bool isUnlock)
        {
            lockGO.SetActive(!isUnlock);
            unLockGO.SetActive(isUnlock);
        }

        public void OnUse()
        {
            //boosterCurrency.Add(-1);
            SetView();
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public void OnClik()
        {
            if (inCoolDown)
                return;

            if (boosterUseConditions.CanUseBooster())
            {
                BoosterManager.Instance.ActvieBooster(boosterId, OnUse);
                inCoolDown = true;
                CoroutineRunner.instance.Wait(boosteCoolDownTime, () => { inCoolDown = false; });
            }
        }

        public void OnCoinClick()
        {
            if (DataManager.Instance.GetCurrency(CurrencyConstant.COINS).Value >= boosterData.purchaseCoinAmount)
            {
                DataManager.Instance.GetCurrency(CurrencyConstant.COINS).Add(-boosterData.purchaseCoinAmount);
                DataManager.Instance.GetCurrency(boosterId).Add(1);
                SetView();
                OnClik();
                return;
            }
            MainSceneUIManager.Instance.GetView<ShopView>().Show();
        }

        #endregion
    }
}
