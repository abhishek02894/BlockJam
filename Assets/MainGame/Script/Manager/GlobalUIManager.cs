using UnityEngine;

namespace Tag.Block
{
    public class GlobalUIManager : UIManager<GlobalUIManager>
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        private void Start()
        {
            GetView<LoadingView>().Show();
            GetView<LoadingView>().SetLoadingBar(0.2f, true, 1f);
        }

        private void Update()
        {
            // On Android the back button is sent as Esc
            if (Input.GetKeyDown(KeyCode.Escape) /*&& !Tutorial.IsRunning*/)
            {
                HideLastOpenView(true);
            }
        }

        public void HideLastOpenView(bool isShowQuitView = false)
        {
            if (BaseView.backPressableViews.Count > 0)
            {
                BaseView lastOpenView = BaseView.backPressableViews[BaseView.backPressableViews.Count - 1];
                if (lastOpenView.CanPressBackButton())
                    lastOpenView.OnBackButtonPressed();
            }
            else if (isShowQuitView && CanOpenApplicationQuitView())
            {
                GetView<UserMessagePromptView>().Show("Quit Game?", "Are you sure you want to quit playing?", null, "", "Quit", null, QuitApplication);
            }
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private bool CanOpenApplicationQuitView()
        {
            if (GetView<InGameLoadingView>().IsActive)
                return false;

            return MainSceneLoader.Instance != null && MainSceneUIManager.Instance.GetView<MainView>().IsActive;
        }

        private void QuitApplication()
        {
            Debug.LogError("Quit");
            Application.Quit();
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
