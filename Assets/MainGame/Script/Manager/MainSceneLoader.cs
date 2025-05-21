using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class MainSceneLoader : SerializedManager<MainSceneLoader>
    {
        #region PUBLIC_VARIABLES

        public List<ManagerInstanceLoader> managers;

        #endregion

        #region PRIVATE_VARIABLES

        public static SceneTransitionData SceneTransitionData => mySceneTransistionData;
        private static SceneTransitionData mySceneTransistionData;

        #endregion

        #region PROPERTIES
        public float LoadingProgress { get; private set; }

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            StartCoroutine(LoadManager());
        }

        #endregion

        #region PUBLIC_METHODS

        public static void SetSceneTransitionData(SceneTransitionData sceneTransitionData)
        {
            mySceneTransistionData = sceneTransitionData;
        }

        #endregion

        #region PRIVATE_METHODS

        private void OnMainSceneLoadingDone()
        {
            OnLoadingDone();
            MainSceneUIManager.Instance.GetView<MainView>().Show();
            MainSceneUIManager.Instance.GetView<BottombarView>().InitView();
            MainSceneUIManager.Instance.GetView<BottombarView>().Show();
            //AdManager.Instance.ShowBannerAd(true);
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region COROUTINES

        IEnumerator LoadManager()
        {
            LoadingProgress = 0f;
            yield return null;

            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].gameObject.SetActive(true);
                while (!managers[i].loaded)
                {
                    yield return 0;
                }
                LoadingProgress = ((float)(i + 1)) / managers.Count;
            }

            yield return null;

            LoadingProgress = 1f;
            yield return new WaitForSeconds(0.1f);
            OnMainSceneLoadingDone();
        }

        #endregion

        #region UI_CALLBACKS

        #endregion
    }

    public class SceneTransitionData
    {
        public GameLevelTriggerType gameLevelTriggerType;
        public SceneType loadingFromScene;
        public Level loadingFromLevel;

        public SceneTransitionData() { }
        public SceneTransitionData(SceneType sceneType)
        {
            loadingFromScene = sceneType;
        }
        public SceneTransitionData(SceneType sceneType, Level level)
        {
            loadingFromScene = sceneType;
            loadingFromLevel = level;
        }
    }

    public enum GameLevelTriggerType
    {
        NONE,
    }

    public enum GameLevelResultType
    {
        NONE,
        LEVEL_WIN,
        LEVEL_LOSE,
        LEVEL_ESCAPE
    }

    public enum SceneType
    {
        LOADING,
        MainScene,
    }
}