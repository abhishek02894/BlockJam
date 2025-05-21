using System;
using UnityEngine;

namespace Tag.Block
{
    public class LevelRestartView : BaseView
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
        }

        public override void OnBackButtonPressed()
        {
            OnClose();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void ForcefullyCloseAction()
        {
            GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(1f, () =>
            {
            });

            Hide();
            MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Hide();
            //MainSceneUIManager.Instance.GetView<GameplayBottomView>().Hide();
            //MainSceneUIManager.Instance.GetView<GameplayGoalView>().Hide();
            LevelManager.Instance.UnloadLevel();
            MainSceneUIManager.Instance.GetView<MainView>().Show();
            MainSceneUIManager.Instance.GetView<BottombarView>().Show();
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public void OnRetry()
        {
            //GameplayManager.Instance.OnRetry();
            //GameplayManager.Instance.OnCurrentLevelRestartOrNext();
            Hide();
            MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Hide();

            GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(0.8f, () =>
            {
                LevelManager.Instance.LevelRetry();
                MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Show();
            });
        }
        public void OnClose()
        {
            Hide();
        }

        #endregion
    }
}
