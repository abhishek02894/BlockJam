using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class ShopView : BaseView
    {
        #region PUBLIC_VARS
        #endregion

        #region PRIVATE_VARS

        [SerializeField] private Canvas _canvas;
        //[SerializeField] private CurrencyTopbarComponents coinTopbar;
        //[SerializeField] private IapShopBundleDataSO iapShopBundleDataSO;
        //[SerializeField] private MainBundleShopView mainBundleShopView;
        //[SerializeField] private IapShopCoinBundleView[] iapShopBundleViews;
        //[SerializeField] private NoAdsBundleShopView miniNoAdsBundleView;
        //[SerializeField] private NoAdsBundleShopView mainNoAdsBundleView;
        //[SerializeField] private IapShopBundleView specialBundleView;
        //[SerializeField] private DailyDealsView _dailyDealsView;
        #endregion  

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);

            int coinAmount = DataManager.Instance.GetCurrency(CurrencyConstant.COINS).Value;

            SetView();
        }

        public void Show(int layer = 15)
        {
            _canvas.sortingOrder = layer;
            Show(null, false);
        }

        public void SetView()
        {
            //for (int i = 0; i < iapShopBundleViews.Length; i++)
            //{
            //    iapShopBundleViews[i].Init(iapShopBundleDataSO.iapShopBundleDatas[i]);
            //}
            //miniNoAdsBundleView.SetView(iapShopBundleDataSO.noAdsMiniBundleData);
            //mainNoAdsBundleView.SetView(iapShopBundleDataSO.noAdsMainBundleData);
            //specialBundleView.SetView(iapShopBundleDataSO.specialBundleData);
            //mainBundleShopView.ShowView(0);
            SetCurrencyTopbar();
            //_dailyDealsView.Init();
        }

        public void SetCurrencyTopbar()
        {
            //coinTopbar.SetCurrencyValue();
        }

        public override void OnViewHideDone()
        {
            base.OnViewHideDone();
        }

        public override void OnBackButtonPressed()
        {
            OnClose();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public void OnClose()
        {
            BottombarView bottombarView = MainSceneUIManager.Instance.GetView<BottombarView>();

            if (bottombarView.IsActive)
            {
                bottombarView.InitView();
            }
            Hide();
        }

        #endregion
    }
}
