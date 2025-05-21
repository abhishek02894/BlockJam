using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Block
{
    public class BaseElement : MonoBehaviour
    {
        #region PUBLIC_VARS
        protected BaseItem baseItem;
        protected BaseElementData baseElementData;
        #endregion

        #region PRIVATE_VARS
        public BaseElementData ElementData { get { return baseElementData; } }
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual void Init(BaseElementData baseElementData, BaseItem baseItem)
        {
            this.baseElementData = baseElementData;
            this.baseItem = baseItem;
        }
        public virtual List<MeshRenderer> GetMeshRender()
        {
            return new List<MeshRenderer>();
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
#if UNITY_EDITOR
        public virtual void InitForEditor(BaseElementData baseElementData, BaseItem baseItem)
        {
            this.baseItem = baseItem;
        }
#endif
    }
}
