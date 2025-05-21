using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class ExtraItemElementData : BaseElementData
    {
        #region PUBLIC_VARS
        [BlockColorId, SerializeField] private int extraColorType;
        #endregion

        #region PRIVATE_VARS
        [ShowInInspector, ReadOnly] private BaseItem parentItem;
        #endregion

        #region UNITY_CALLBACKS
        public int ExtraColorType => extraColorType;
        #endregion

        #region PUBLIC_FUNCTIONS
        public ExtraItemElementData()
        {
            this.elementType = ElementType.ExtraItem;
        }
        public override void Init(BaseItem baseItem)
        {
            elementType = ElementType.ExtraItem;
            base.Init(baseItem);
            parentItem = baseItem;
        }
        public void SpawnExtraItem(Vector3 spawnPosition)
        {
            LevelManager.Instance.SpawnExtraItem(parentItem.BlockType, extraColorType, spawnPosition);
        }

        public override void ExitItem(Vector3 itemPosition)
        {
            base.ExitItem(itemPosition);
            SpawnExtraItem(itemPosition);
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
