using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    [CreateAssetMenu(fileName = "LevelData", menuName = Constant.GAME_NAME + "/LevelData")]
    public class LevelDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARS
        [SerializeField] private Level level;
        [SerializeField] private int timeInSecond;
        [SerializeField] private List<ItemData> itemsData = new List<ItemData>();
        #endregion

        #region PROPERTIES
        // Public properties with getters and setters
        public Level Level { get { return level; } set { level = value; } }
        public List<ItemData> ItemsData { get { return itemsData; } set { itemsData = value; } }
        public int TimeInSecond { get { return timeInSecond; } set { timeInSecond = value; } }
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
    public class ItemData
    {
        [BlockId] public int blockType;
        [BlockColorId] public int colorType;
        public int cellId;
        public List<BaseElementData> elements = new List<BaseElementData>();
    }
}
