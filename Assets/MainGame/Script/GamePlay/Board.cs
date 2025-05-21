using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.Block
{
    public class Board : MonoBehaviour
    {
        #region PUBLIC_VARS
        [SerializeField] private List<BaseCell> cells = new List<BaseCell>();
        [SerializeField, Min(1)] private int rows = 5;
        [SerializeField, Min(1)] private int columns = 5;
        [SerializeField] private float cellSpacing = 1f;

        public int Rows => rows;
        public int Columns => columns;
        public float CellSpacing => cellSpacing;

        public List<BaseCell> Cells => cells;

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        //[Button]
        //private void SetCell()
        //{
        //    // Clear old cells
        //    foreach (var cell in Cells)
        //    {
        //        if (cell != null)
        //            DestroyImmediate(cell.gameObject);
        //    }
        //    Cells.Clear();

        //    float xOffset = (columns - 1) / 2f;
        //    float yOffset = (rows - 1) / 2f;
        //    int cellid = 0;
        //    // Instantiate new grid
        //    for (int row = 0; row < rows; row++)
        //    {
        //        for (int col = 0; col < columns; col++)
        //        {
        //            BaseCell prefabToUse = ((row + col) % 2 == 0) ? cellPrefab : alternativeCellPrefab;
        //            BaseCell cell = Instantiate(prefabToUse, cellParent);
        //            float x = (col - xOffset) * cellSpacing;
        //            float y = -(row - yOffset) * cellSpacing;
        //            cell.transform.position = new Vector3(x, 0, y);
        //            cell.gameObject.name = "Cell-" + cellid;
        //            cell.cellId = cellid;
        //            cellid++;
        //            Cells.Add(cell);
        //        }
        //    }
        //}

        public BaseCell GetCellAtWorldPos(Vector3 worldPos)
        {
            BaseCell nearest = null;
            float minDist = float.MaxValue;
            foreach (var cell in Cells)
            {
                if (!cell.IsBlock)
                {
                    float dist = Vector3.Distance(cell.transform.position, worldPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = cell;
                    }
                }
            }
            if (minDist <= cellSpacing / 2f)
                return nearest;
            return null;
        }
        public BaseCell GetCellAtWorldPos(List<BaseCell> cells, Vector3 worldPos)
        {
            BaseCell nearest = null;
            float minDist = float.MaxValue;
            foreach (var cell in cells)
            {
                if (!cell.IsBlock)
                {
                    float dist = Vector3.Distance(cell.transform.position, worldPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = cell;
                    }
                }
            }
            if (minDist <= cellSpacing / 2f)
                return nearest;
            return null;
        }
        public BaseCell GetCellById(int id)
        {
            return Cells.Find(x => x.cellId == id);
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
        public List<BaseCell> Cells1 { get => cells; set => cells = value; }

        public Transform cellParent;
        public void SetNearByCells()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].SetNearByCells();
            }
        }
        public void SetRowColSpacing(int row, int col, float spacing)
        {
            this.cellSpacing = spacing;
            this.rows = row;
            this.columns = col;
        }
#endif
    }
}
