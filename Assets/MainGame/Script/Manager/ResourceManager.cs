using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.Block
{
    public class ResourceManager : SerializedManager<ResourceManager>
    {
        #region PUBLIC_VARS
        [SerializeField] private Dictionary<int, LevelDataSO> levels = new Dictionary<int, LevelDataSO>();
        [SerializeField] private Dictionary<int, BaseItem> itemPrefabs = new Dictionary<int, BaseItem>();
        [SerializeField] private Dictionary<int, Material> materialsMapWithColor = new Dictionary<int, Material>();
        [SerializeField] private Dictionary<int, Material> ropematerialsMapWithColor = new Dictionary<int, Material>();
        [SerializeField] private Dictionary<int, Mesh> meshMappingWithExtraBlock = new Dictionary<int, Mesh>();
        [SerializeField] private Dictionary<int, BaseElement> elementMappingWithElement = new Dictionary<int, BaseElement>();
        [SerializeField] private BoosterDataSO boosterData;
        [SerializeField] private ItemSlot itemSlotPrefab;

        #endregion

        #region PRIVATE_VARS
        public BoosterDataSO BoosterData { get => boosterData; }

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public LevelDataSO GetLevelDataSO(int level)
        {
            if (levels.ContainsKey(level))
                return levels[level];
            return levels.Last().Value;
        }
        public BaseItem CreateItemFromData(ItemData itemData, Transform parent)
        {
            if (!itemPrefabs.ContainsKey(itemData.blockType))
            {
                Debug.LogError($"Item prefab with blockType {itemData.blockType} not found in GameManager!");
                return null;
            }
            BaseItem newItem = Instantiate(itemPrefabs[itemData.blockType], parent);
            return newItem;
        }
        public Material GetMaterial(int colorType)
        {
            return materialsMapWithColor[colorType];
        }
        public Material GetRopeMaterial(int colorType)
        {
            return ropematerialsMapWithColor[colorType];
        }
        public BaseElement GetElementPrefab(int elementId, Transform transform)
        {
            if (elementMappingWithElement.ContainsKey(elementId) && elementMappingWithElement[elementId] != null)
                return Instantiate(elementMappingWithElement[elementId], transform);
            return null;
        }

        public Mesh GetExtraItemMesh(int blocktype)
        {
            return meshMappingWithExtraBlock[blocktype];
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
