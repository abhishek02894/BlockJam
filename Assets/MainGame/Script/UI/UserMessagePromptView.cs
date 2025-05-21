using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.Block
{
    public class UserMessagePromptView : BaseView
    {
        #region PUBLIC_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        [SerializeField] private Text headerText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text promptTextWithImage;
        [SerializeField] private Button negativeButton;
        [SerializeField] private Button positiveButton;
        [SerializeField] private Text negativeButtonText;
        [SerializeField] protected Text positiveButtonText;
        [SerializeField] private Image messgeImage;
        [SerializeField] private GameObject imageContent;
        [SerializeField] private GameObject textContent;

        private Action actionToCallOnNegativeButtonClick;
        private Action actionToCallOnPositiveButtonClick;

        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(string headerText, string descriptionText, Sprite messageSprite ,string positiveButtonText = "Okay", string negativeButtonText = "Cancel", Action positiveButtonAction = null, Action negativeButtonAction = null)
        {
            this.headerText.text = headerText;
            this.promptText.text = descriptionText;
            this.promptTextWithImage.text = descriptionText;

            this.positiveButtonText.text = positiveButtonText;
            this.negativeButtonText.text = negativeButtonText;

            positiveButton.gameObject.SetActive(!string.IsNullOrEmpty(positiveButtonText));
            negativeButton.gameObject.SetActive(!string.IsNullOrEmpty(negativeButtonText));

            actionToCallOnPositiveButtonClick = positiveButtonAction;
            actionToCallOnNegativeButtonClick = negativeButtonAction;

            imageContent.gameObject.SetActive(messageSprite != null);
            textContent.gameObject.SetActive(messageSprite == null);

            messgeImage.sprite = messageSprite;

            Show();

            LayoutRebuilder.ForceRebuildLayoutImmediate(messgeImage.rectTransform);
        }

        public override void OnBackButtonPressed()
        {
            Hide();
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region COROUTINES

        #endregion

        #region UI_CALLBACKS

        public void OnButtonClick_PositiveButton()
        {
            Hide();
            actionToCallOnPositiveButtonClick?.Invoke();
        }

        public void OnButtonClick_NegativeButton()
        {
            Hide();
            actionToCallOnNegativeButtonClick?.Invoke();
        }

        #endregion
    }
}