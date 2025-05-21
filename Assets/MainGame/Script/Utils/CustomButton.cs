using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tag.Block
{
    [AddComponentMenu("UI/CustomButton", 30)]
    public class CustomButton : Button
    {
        #region static veriables

        private static bool isButtonClick;

        #endregion

        #region overrided methods

        [ContextMenu("Add Animation Componant")]
        public void AddButtonClickAnimationAndSound()
        {
            if (gameObject.GetComponent<ButtonClickAnimation>() == null)
                gameObject.AddComponent<ButtonClickAnimation>();
            //if (gameObject.GetComponent<ButtonClickSound>() == null)
            //    gameObject.AddComponent<ButtonClickSound>();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (isButtonClick)
                return;
            isButtonClick = true;
            base.OnPointerClick(eventData);

            CoroutineRunner.instance.Wait(0.2f, () =>
            {
                isButtonClick = false;
            });
        }

        #endregion

        #region coroutine

        #endregion
    }
}