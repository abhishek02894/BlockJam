using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class GameplayBottomView : BaseView
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] UIAnimationHandler uIAnimationHandler;
        [SerializeField] private BoosterItemView[] boosterItemViews;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            SetView();
        }

        public void ShowAnimation()
        {
            if (uIAnimationHandler != null)
                uIAnimationHandler.ShowAnimation(() => { });
        }

        public void HideAnimation()
        {
            if (uIAnimationHandler != null)
                uIAnimationHandler.HideAnimation(() => { });
        }

        public void CheakUndoBoosterCondition()
        {
        }

        public void SetView()
        {
            for (int i = 0; i < boosterItemViews.Length; i++)
            {
                boosterItemViews[i].SetView();
            }
        }

        public Vector3 GetBoosterPos(int boosterId)
        {
            for (int i = 0; i < boosterItemViews.Length; i++)
            {
                if (boosterItemViews[i].BoosterId == boosterId)
                {
                    return boosterItemViews[i].transform.position;
                }
            }
            return Vector3.zero;
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
