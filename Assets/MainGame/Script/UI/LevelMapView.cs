using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Tag.Block
{
    public class LevelMapView : BaseView
    {
        [SerializeField] private Text currentLevelButtonText;
        [SerializeField] private GameObject firstLevelButton;
        [SerializeField] private List<LevelButton> levelButtons = new List<LevelButton>();
        [SerializeField] private InputField levelText;

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            InitializeLevelMap();
        }

        private void InitializeLevelMap()
        {
            int levelNumber = DataManager.PlayerData.playerGameplayLevel;
            currentLevelButtonText.text = "Level " + levelNumber;
            firstLevelButton.SetActive(levelNumber > 1);
            foreach (var level in levelButtons)
            {
                level.Setup(levelNumber);
                levelNumber++;
            }
        }
        public void LevelButtonClick()
        {
            Hide();
            MainSceneUIManager.Instance.GetView<BottombarView>().Hide();
            MainSceneUIManager.Instance.GetView<MainView>().Hide();

            if (!string.IsNullOrEmpty(levelText.text))
            {
                GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(0.8f, () =>
                {
                    DataManager.Instance.SetLevel_Editor(int.Parse(levelText.text));
                    LevelManager.Instance.LoadLevel(int.Parse(levelText.text));
                    MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Show();
                });
            }
            else
            {
                GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(0.8f, () =>
                {
                    LevelManager.Instance.LoadLevel();
                    MainSceneUIManager.Instance.GetView<GameplayTopbarView>().Show();
                });
            }




        }
    }
}