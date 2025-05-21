using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Tag.Block
{
    public class BottomBarButton : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private GameObject selectedGO;
        [SerializeField] private GameObject deSelectedGO;
        [SerializeField] private UIAnimationHandler selectedIcon;
        [SerializeField] private UIAnimationHandler tabText;
        [SerializeField] private Button clickButton;
        [SerializeField] private float animationDuration = 0.3f;
        private RectTransform rectTransform;
        private LayoutGroup parentLayout;

        #endregion

        #region UNITY_CALLBACKS

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentLayout = GetComponentInParent<LayoutGroup>();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void OnSelect()
        {
            rectTransform
                .DOSizeDelta(new Vector2(310, 388), animationDuration)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() => {
                    if (parentLayout != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayout.GetComponent<RectTransform>());
                    }
                });
            
            selectedGO.SetActive(true);
            deSelectedGO.SetActive(false);
            selectedIcon.SetInAnimationData();
            selectedIcon.gameObject.SetActive(true);
            selectedIcon.ShowAnimation();

            tabText.SetInAnimationData();
            tabText.gameObject.SetActive(true);
            tabText.ShowAnimation();

            clickButton.interactable = false;
        }

        public void OnDeselect()
        {
            rectTransform
                .DOSizeDelta(new Vector2(194, 388), animationDuration)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() => {
                    if (parentLayout != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayout.GetComponent<RectTransform>());
                    }
                });
            
            selectedIcon.HideAnimation();
            tabText.gameObject.SetActive(false);
            selectedGO.SetActive(false);
            deSelectedGO.SetActive(true);
            clickButton.interactable = true;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public void OnClick()
        {
            MainSceneUIManager.Instance.GetView<BottombarView>().OnBottomBarButtonClick(this);
        }

        #endregion
    }
}
