using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Tag.Block
{
    public class BaseCell : MonoBehaviour
    {
        #region PUBLIC_VARS
        public int cellId;
        #endregion

        #region PRIVATE_VARS
        [ShowInInspector, ReadOnly] protected BaseItem item;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BaseCell[] nearByCell = new BaseCell[0];

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        public BaseCell[] NearByCell => nearByCell;
        #endregion

        #region propertices
        public virtual bool HasItem => item != null;
        public BaseItem Item
        {
            get { return item; }
            set
            {
                item = value;
            }
        }
        public virtual bool IsBlock
        {
            get { return false; }
        }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual bool CanDragItem(BaseItem baseItem)
        {
            if ((HasItem && Item != baseItem) || IsBlock)
                return false;
            return true;
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
        [Button]
        public void SetNearByCells()
        {
            List<BaseCell> tmpCell = new List<BaseCell>();
            AddToNearByCell(tmpCell, Vector3.forward, 0.6f);
            AddToNearByCell(tmpCell, Vector3.back, 0.6f);
            AddToNearByCell(tmpCell, Vector3.left, 0.6f);
            AddToNearByCell(tmpCell, Vector3.right, 0.6f);
            //AddToNearByCell(tmpCell, new Vector3(1, 0, 1), 1f);
            //AddToNearByCell(tmpCell, new Vector3(-1, 0, 1), 1f);
            //AddToNearByCell(tmpCell, new Vector3(1, 0, -1), 1f);
            //AddToNearByCell(tmpCell, new Vector3(-1, 0, -1), 1f);
            nearByCell = tmpCell.ToArray();
        }

        private void AddToNearByCell(List<BaseCell> tmpCell, Vector3 direction, float distance)
        {
            RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, direction, distance);
            BaseCell cell = null;
            for (int i = 0; i < raycastHits.Length; i++)
            {
                cell = raycastHits[i].collider.gameObject.GetComponent<BaseCell>();
                if (cell != null && !tmpCell.Contains(cell))
                    tmpCell.Add(cell);
            }
        }
#endif
    }
}
