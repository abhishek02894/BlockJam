using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.Block
{
    public class GameplayTopbarView : BaseView
    {
        #region PUBLIC_VARS
        public Text timerText;
        #endregion

        #region PRIVATE_VARS

        [SerializeField] UIAnimationHandler uIAnimationHandler;
        //[SerializeField] private List<CurrencyTopbarComponents> topbarComponents = new List<CurrencyTopbarComponents>();

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            MainSceneUIManager.Instance.OnGameplayView();
            TimeSpan timeSpan = new TimeSpan(0, 0, LevelManager.Instance.CurrentLevelDataSO.TimeInSecond);
            timerText.text = timeSpan.ToString();
            GameplayManager.Instance.RegisterOnTimerTick(SetTimer);
        }
        public override void Hide()
        {
            base.Hide();
            GameplayManager.Instance.DeregisterOnTimerTick(SetTimer);
        }
        public override void OnBackButtonPressed()
        {
            MainSceneUIManager.Instance.GetView<GameplaySettingView>().Show();
        }

        public void ShowAnimation()
        {
            uIAnimationHandler.ShowAnimation(() =>
            {

            });
        }

        public void HideAnimation()
        {
            uIAnimationHandler.HideAnimation(() =>
            {
            });
        }
        public void SetTimer(TimeSpan timeSpan)
        {
            timerText.text = timeSpan.FormateTimeSpan();
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public void OnSetting()
        {
            MainSceneUIManager.Instance.GetView<GameplaySettingView>().Show();
        }

        #endregion
    }
}
