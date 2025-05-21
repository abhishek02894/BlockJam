using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class BaseElementData
    {
        #region PUBLIC_VARS
        [ElementTypeId] public int elementType;
        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual void Init(BaseItem baseItem)
        {

        }
        public virtual void ExitItem(Vector3 ItemPosition)
        {

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
    public class ElementType
    {
        public const int FixDirection = 1;
        public const int ExtraItem = 2;
    }
}
