using System;
using UnityEngine.UI;

namespace Tag.Block
{
    public class LevelWinView : BaseView
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        public Text levelText;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            SetView();
        }


        public override void OnBackButtonPressed()
        {
            OnClose();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetView()
        {
            levelText.text = "Level " + (DataManager.PlayerData.playerGameplayLevel - 1);
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public void OnNext()
        {
            Hide();
            GameplayManager.Instance.PlayNextLevel();
        }


        public void OnClose()
        {
            Hide();
            GameplayManager.Instance.OnLevelEnd();
        }

        #endregion
    }
}
