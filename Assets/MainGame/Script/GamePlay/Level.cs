using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

namespace Tag.Block
{

    public class Level : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private Transform itemParent;
        [SerializeField] private Board board;
        [SerializeField] private List<BaseExitDoor> exitDoor;
        [SerializeField] private ThreadCollector threadCollector;
        [ShowInInspector, ReadOnly] private List<BaseItem> instantiatedItems = new List<BaseItem>();
        #endregion

        #region PRIVATE_VARS
        public Board Board { get { return board; } }
        public ThreadCollector ThreadCollector => threadCollector;
        public Transform ItemParent { get { return itemParent; } }

        public int totalBlockCell;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public void Init(List<ItemData> itemsDatas)
        {
            for (int i = 0; i < instantiatedItems.Count; i++)
            {
                if (instantiatedItems[i] != null)
                {
                    Destroy(instantiatedItems[i].gameObject);
                }
            }
            instantiatedItems.Clear();
            foreach (var itemData in itemsDatas)
            {
                BaseItem item = ResourceManager.Instance.CreateItemFromData(itemData, itemParent);
                if (item != null)
                {
                    item.Init(itemData);
                    instantiatedItems.Add(item);
                    totalBlockCell += item.ItemSlotCount;
                }
            }
        }

        // Add a new item to the level (used for dynamically spawned items)
        public void AddItem(BaseItem item)
        {
            if (item != null && !instantiatedItems.Contains(item))
            {
                instantiatedItems.Add(item);
            }
        }

        public void ResetLevel()
        {
            for (int i = 0; i < instantiatedItems.Count; i++)
            {
                if (instantiatedItems[i] != null)
                {
                    instantiatedItems[i].ResetLevel();
                }
            }
        }

        [Button]
        public void CheckAllExitDoor()
        {
            for (int i = 0; i < exitDoor.Count; i++)
            {
                exitDoor[i].CheckBlock();
            }
        }

        public void OnItemExited()
        {
            // Remove null or inactive items from the list
            instantiatedItems.RemoveAll(item => item == null || !item.gameObject.activeSelf);

            // Check if all items have exited
            CheckForLevelWin();
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        public void CheckForLevelWin()
        {
            // Count active items
            int activeItems = instantiatedItems.Count(item => item != null && item.gameObject.activeSelf);

            if (activeItems == 0)
            {
                Debug.Log("Level Completed!");
                GameplayManager.Instance.OnLevelWin();
            }
        }
        public bool CanUseHammerBooster()
        {
            for (int i = 0; i < instantiatedItems.Count; i++)
            {
                if (instantiatedItems[i].CanUseBooster())
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
#if UNITY_EDITOR
        public void SetThreadCollecter(ThreadCollector threadCollector)
        {
            this.threadCollector = threadCollector;
        }
        public void SetBoard(Board board)
        {
            this.board = board;
        }
        public void SetNearByCells()
        {
            board.SetNearByCells();
        }
        public void SetitemParent(GameObject itemParent)
        {
            this.itemParent = itemParent.transform;
        }
        public List<BaseExitDoor> ExitDoor { get => exitDoor; set => exitDoor = value; }

        public void AddExitDoor(BaseExitDoor baseExitDoor)
        {
            if (!ExitDoor.Contains(baseExitDoor))
                ExitDoor.Add(baseExitDoor);
        }
#endif
    }
}
