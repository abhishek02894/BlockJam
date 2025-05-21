using UnityEngine;
using UnityEngine.UI;

namespace Tag.Block
{
    public class MainSceneUIManager : UIManager<MainSceneUIManager>
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private Image bg;
        [SerializeField] private RectTransform bgParent;

        [SerializeField] private Image bgGameplay;
        [SerializeField] private RectTransform bgParentGameplay;

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            SetImageAspectRatio();
            //SetImageAspectRatioGameplay();
        }

        public void OnGameplayView()
        {
            SetActiveBG(true);
            //SoundHandler.Instance.PlayCoreBackgrondMusic();
        }

        public void OnMainView()
        {
            SetActiveBG(false);
            //SoundHandler.Instance.PlayMetaMusic();
            GetView<LevelMapView>().Show();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetActiveBG(bool isActive)
        {
            bgParentGameplay.gameObject.SetActive(isActive);
            bgParent.gameObject.SetActive(!isActive);
        }

        private void SetImageAspectRatio()
        {
            if (bg == null || bgParent == null)
                return;

            float width = bg.sprite.rect.width;
            float height = bg.sprite.rect.height;

            float aspectRatio = width / height;

            float parentWidth = bgParent.rect.width;
            float parentHeight = bgParent.rect.height;

            if (parentWidth / parentHeight > aspectRatio)
                bg.rectTransform.sizeDelta = new Vector2(parentWidth, parentWidth / aspectRatio);

            else
                bg.rectTransform.sizeDelta = new Vector2(parentHeight * aspectRatio, parentHeight);
        }

        private void SetImageAspectRatioGameplay()
        {
            if (bgGameplay == null || bgParentGameplay == null)
                return;

            float width = bgGameplay.sprite.rect.width;
            float height = bgGameplay.sprite.rect.height;

            float aspectRatio = width / height;

            float parentWidth = bgParentGameplay.rect.width;
            float parentHeight = bgParentGameplay.rect.height;

            if (parentWidth / parentHeight > aspectRatio)
                bgGameplay.rectTransform.sizeDelta = new Vector2(parentWidth, parentWidth / aspectRatio);

            else
                bgGameplay.rectTransform.sizeDelta = new Vector2(parentHeight * aspectRatio, parentHeight);
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
