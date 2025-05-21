using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Tag.Block
{
    public abstract class BaseBooster : SerializedMonoBehaviour
    {
        #region PUBLIC_VARS
        public bool IsActive { get => isActive; }

        #endregion

        #region PRIVATE_VARS

        [SerializeField, CurrencyId] internal int boosteID;
        internal Action onUse;
        internal bool isActive;

        [Space(10)]
        [SerializeField] private bool isConfirmationActive;
        [ShowIf("isConfirmationActive"), SerializeField] private float cameraTweenDuration = 0.2f;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void OnActive(Action onUse)
        {
            this.onUse = onUse;
            isActive = true;
        }

        public virtual void OnUse()
        {
            GameplayManager.Instance.OnBoosterUse(boosteID);
            if (onUse != null)
                onUse.Invoke();
            isActive = false;
            BoosterManager.Instance.DeActvieBooster();
            Vibrator.Vibrate(Vibrator.hugeIntensity);
        }

        public virtual void OnUnUse()
        {
            isActive = false;
            BoosterManager.Instance.DeActvieBooster();
        }

        public virtual void ActiveConfirmationView()
        {
            HideAllGameplayViews();
            MainSceneUIManager.Instance.GetView<BoosterActiveInfoView>().ShowView(ResourceManager.Instance.BoosterData.GetBoosterData(boosteID), OnUnUse);
        }

        public virtual void DeActiveConfirmationView()
        {
            MainSceneUIManager.Instance.GetView<BoosterActiveInfoView>().Hide();
            ShowAllGameplayViews();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void HideAllGameplayViews()
        {
            MainSceneUIManager.Instance.GetView<GameplayTopbarView>().HideAnimation();
            MainSceneUIManager.Instance.GetView<GameplayBottomView>().HideAnimation();
        }

        private void ShowAllGameplayViews()
        {
            MainSceneUIManager.Instance.GetView<GameplayTopbarView>().ShowAnimation();
            MainSceneUIManager.Instance.GetView<GameplayBottomView>().ShowAnimation();
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
