using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class FixDirectionElementData : BaseElementData
    {
        #region PUBLIC_VARS
        [SerializeField] private bool isVertical;

        public bool IsVertical => isVertical;
        #endregion

        #region PRIVATE_VARS
        public FixDirectionElementData()
        {
            elementType = ElementType.FixDirection;
        }
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

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
