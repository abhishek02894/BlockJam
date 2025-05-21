using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tag.Block;
using UnityEngine;

namespace Tag.Block
{
    public class ItemMovement : BaseItemMovement
    {
        [SerializeField] private LayerMask backgroundLayerMask;
        [SerializeField] private LayerMask cellLayerMask;
        protected RaycastHit hit;
        protected Vector3 pickOffset;
        private Vector3 itemPos = Vector3.zero;
        private Vector3 lastDragPosition = Vector3.negativeInfinity;
        private bool isMoved;
        [ShowInInspector, ReadOnly] protected BaseItem pickItem;
        [ShowInInspector, ReadOnly] protected BaseCell pickCell;
        [ShowInInspector, ReadOnly] protected List<BaseCell> possibleMoveCells = new List<BaseCell>();

        public BaseItem PickItem => pickItem;

        public override bool ItemPick(Vector3 pos)
        {
            CleanupDragState();
            if (GetRayHit(pos, out hit, itemLayerMask))
            {
                BaseItem item = hit.collider.GetComponent<BaseItem>();
                if (item != null && item.CanMoveItem())
                {
                    pickItem = item;
                    Debug.Log($"[ItemMovement] ItemPick: pickItem = {pickItem.gameObject.name}, spacing = {pickItem.Spacing}");
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("ShapePositions: ");
                    if (pickItem.ShapePositions != null && pickItem.ShapePositions.Count > 0) {
                        foreach (var sp in pickItem.ShapePositions) {
                            sb.Append(sp.ToString() + "; ");
                        }
                    } else {
                        sb.Append("Empty or Null");
                    }
                    Debug.Log(sb.ToString());
                    pickItem.OnItemPick();
                    int closestIndex = 0;
                    float minDist = float.MaxValue;
                    for (int i = 0; i < item.ShapePositions.Count; i++)
                    {
                        Vector3 cellWorldPos = item.transform.position + new Vector3(item.ShapePositions[i].x * item.Spacing, 0f, item.ShapePositions[i].z * item.Spacing);
                        float dist = Vector3.Distance(cellWorldPos, hit.point);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestIndex = i;
                        }
                    }
                    pickItem.SetPickedShapeIndex(closestIndex);
                    ItemMovementHelper.Instance.OnItemPick();
                    pickOffset = pickItem.transform.position - hit.point;
                    pickCell = LevelManager.Instance.CurrentLevel.Board.GetCellAtWorldPos(pickItem.transform.position + pickItem.GetPickedShapeOffset());
                    SetPossibleMoveCells();
                    HighlightPossibleMoves();
                    return true;
                }
            }
            CleanupDragState();
            return false;
        }

        public override Vector3 ItemDrag(Vector3 pos)
        {
            if (pickItem == null) return itemPos;

            if (GetRayHit(pos, out hit, backgroundLayerMask))
            {
                MoveItemOnBoard();
            }

            ItemMovementHelper.Instance.OnItemDrag();
            return itemPos;
        }

        private void MoveItemOnBoard()
        {
            if (pickItem == null) return; // Removed possibleMoveCells.Count check as we are not snapping

            // Calculate the desired position using the hit.point from the raycast (pointer's world position on the plane)
            // and the pickOffset (offset from item's origin to the point it was picked).
            // The 'hit' variable is a member of the class and is updated in ItemDrag before MoveItemOnBoard is called.
            Vector3 desired = hit.point + pickOffset;

            // --- Boundary Checks ---
            Board board = LevelManager.Instance.CurrentLevel.Board;
            float boardTransX = board.transform.position.x;
            float boardTransZ = board.transform.position.z;

            // Board boundaries: world coordinates of the centers of the outermost cells
            float boardEdgeMinX = boardTransX - (board.Columns - 1) / 2f * board.CellSpacing;
            float boardEdgeMaxX = boardTransX + (board.Columns - 1) / 2f * board.CellSpacing;
            float boardEdgeMinZ = boardTransZ - (board.Rows - 1) / 2f * board.CellSpacing;
            float boardEdgeMaxZ = boardTransZ + (board.Rows - 1) / 2f * board.CellSpacing;
            
            // Calculate item extents (min/max world coordinates of its constituent cell centers) at the 'desired' item position
            float itemCellCentersMinX = float.MaxValue;
            float itemCellCentersMaxX = float.MinValue;
            float itemCellCentersMinZ = float.MaxValue;
            float itemCellCentersMaxZ = float.MinValue;

            if (pickItem.ShapePositions.Count == 0) // Handle items with no shape positions (e.g. 1x1 item at origin)
            {
                 // For a 1x1 item, its "shape" is effectively at its origin.
                itemCellCentersMinX = desired.x;
                itemCellCentersMaxX = desired.x;
                itemCellCentersMinZ = desired.z;
                itemCellCentersMaxZ = desired.z;
            }
            else
            {
                foreach (var shapePos in pickItem.ShapePositions) // shapePos is local offset from item origin
                {
                    // World position of the center of this constituent cell of the item
                    Vector3 worldPosOfShapeCellCenter = desired + new Vector3(shapePos.x * pickItem.Spacing, 0, shapePos.z * pickItem.Spacing);
                    
                    itemCellCentersMinX = Mathf.Min(itemCellCentersMinX, worldPosOfShapeCellCenter.x);
                    itemCellCentersMaxX = Mathf.Max(itemCellCentersMaxX, worldPosOfShapeCellCenter.x);
                    itemCellCentersMinZ = Mathf.Min(itemCellCentersMinZ, worldPosOfShapeCellCenter.z);
                    itemCellCentersMaxZ = Mathf.Max(itemCellCentersMaxZ, worldPosOfShapeCellCenter.z);
                }
            }

            // Constrain the desired position based on cell centers
            float shiftX = 0f;
            float shiftZ = 0f;

            if (itemCellCentersMinX < boardEdgeMinX)
            {
                shiftX = boardEdgeMinX - itemCellCentersMinX;
            }
            else if (itemCellCentersMaxX > boardEdgeMaxX)
            {
                shiftX = boardEdgeMaxX - itemCellCentersMaxX;
            }

            if (itemCellCentersMinZ < boardEdgeMinZ)
            {
                shiftZ = boardEdgeMinZ - itemCellCentersMinZ;
            }
            else if (itemCellCentersMaxZ > boardEdgeMaxZ)
            {
                shiftZ = boardEdgeMaxZ - itemCellCentersMaxZ;
            }

            desired.x += shiftX;
            desired.z += shiftZ;
            // Note: Assuming the duplicated line "desired.x += shiftX; desired.z += shiftZ;" was an error in my previous diffs
            // and is NOT present in the actual file based on consistent tool failures to find it.
            // If it IS present, this diff will fail, and I'll need to re-add its removal.
            // For now, proceeding as if it's not there.
            // --- End Boundary Checks ---

            // --- Collision Detection & Conditional Movement ---
            bool canMoveToDesiredPosition = true;
            List<BaseCell> cellsItemWouldOccupy = new List<BaseCell>();

            // 1. Determine Target Cells
            if (pickItem.ShapePositions.Count == 0) // Handle 1x1 item (item's origin is its center)
            {
                BaseCell cell = board.GetCellAtWorldPos(desired); // 'desired' is item's origin
                // If the single cell for a 1x1 item is null (e.g. outside grid after boundary constraint - shouldn't happen often)
                if (cell == null) {
                    canMoveToDesiredPosition = false;
                } else {
                    cellsItemWouldOccupy.Add(cell);
                }
            }
            else
            {
                foreach (var shapePos in pickItem.ShapePositions) // shapePos is local offset from item's origin
                {
                    Vector3 worldPosOfShapePart = desired + new Vector3(shapePos.x * pickItem.Spacing, 0, shapePos.z * pickItem.Spacing);
                    BaseCell cell = board.GetCellAtWorldPos(worldPosOfShapePart);
                    if (cell == null) // If any part of the item maps to a null cell (e.g. outside grid)
                    {
                        canMoveToDesiredPosition = false;
                        break; 
                    }
                    cellsItemWouldOccupy.Add(cell);
                }
            }

            // 2. Check for Overlaps (only if all parts mapped to actual cells)
            if (canMoveToDesiredPosition) 
            {
                foreach (BaseCell targetCell in cellsItemWouldOccupy)
                {
                    // targetCell should not be null here due to checks above, but defensive check doesn't hurt.
                    if (targetCell == null) { // Should have been caught already
                        canMoveToDesiredPosition = false;
                        break;
                    }
                    if (targetCell.IsBlock) // Check if the cell itself is a blocker
                    {
                        canMoveToDesiredPosition = false;
                        break;
                    }
                    if (targetCell.Item != null && targetCell.Item != pickItem) // Cell is occupied by another item
                    {
                        canMoveToDesiredPosition = false;
                        break;
                    }
                }
            }
            // --- End Collision Detection ---

            // 3. Update Item Position Conditionally
            if (canMoveToDesiredPosition)
            {
                // Only call OnItemDrag and update lastDragPosition if the item has actually moved to a new position.
                if ((desired - pickItem.transform.position).sqrMagnitude > 0.0001f) 
                {
                    pickItem.OnItemDrag(desired); // Item's transform.position is updated by this
                    lastDragPosition = desired;   // Update internal tracking for drag logic
                    pickItem.SetLastValidPosition(board.GetCellAtWorldPos(desired).transform.position); // <<<< ADD THIS LINE
                    isMoved = true;             // Signifies that a move occurred in this frame.
                }
                itemPos = desired; // itemPos reflects the current valid position (or where it's trying to be if no significant movement).
            }
            else
            {
                // Collision detected or part of item is off-grid.
                // Do not call OnItemDrag with the new 'desired' position.
                // Item remains at its current transform.position, which is the 'lastDragPosition' from the previous valid frame.
                itemPos = pickItem.transform.position; // itemPos reflects the current actual (and valid) position.
                // isMoved is not set to true because no successful drag to a new 'desired' position occurred in this frame.
            }
            HighlightPossibleMoves(); // Remains commented out as per previous logic
        }
        public override void ItemPut(Vector3 pos)
        {
            if (pickItem == null)
            {
                CleanupDragState();
                return;
            }
            BaseCell putCell = null;
            if (GetRayHit(pos, out hit, cellLayerMask))
            {
                putCell = hit.collider.GetComponent<BaseCell>();
            }
            Vector3 finalPosition = pickItem.transform.position;
            bool canPlace = false;
            if (putCell != null)
            {
                Vector3 putCellPos = putCell.transform.position;
                Vector3 shapeOffset = pickItem.GetPickedShapeOffset(); // This is the raw local offset
                Vector3 offset = new Vector3(shapeOffset.x * pickItem.Spacing, 0f, shapeOffset.z * pickItem.Spacing); // This is the scaled offset
                finalPosition = new Vector3(putCellPos.x - offset.x, pickItem.transform.position.y, putCellPos.z - offset.z);

                // Logging as per corrected instructions
                Debug.Log($"[ItemMovement] ItemPut for {pickItem.gameObject.name}: putCell = {putCell.gameObject.name} at {putCell.transform.position}");
                Debug.Log($"[ItemMovement] Raw local shapeOffset from GetPickedShapeOffset() was = {shapeOffset}");
                Debug.Log($"[ItemMovement] pickItem.Spacing = {pickItem.Spacing}");
                Debug.Log($"[ItemMovement] The 'offset' variable (scaled for finalPosition calc) = {offset}");
                Debug.Log($"[ItemMovement] Calculated finalPosition = {finalPosition}");

                Board board = LevelManager.Instance.CurrentLevel.Board;
                canPlace = true;

                foreach (var posOffset in pickItem.ShapePositions)
                {
                    Vector3 worldPos = finalPosition + new Vector3(posOffset.x * pickItem.Spacing, 0f, posOffset.z * pickItem.Spacing);
                    BaseCell cell = board.GetCellAtWorldPos(worldPos);
                    // Removed !possibleMoveCells.Contains(putCell) and added cell.IsBlock check for robustness,
                    // ensuring placement considers only the current state of the target cells.
                    if (cell == null || (cell.Item != null && cell.Item != pickItem) || cell.IsBlock)
                    {
                        canPlace = false;
                        break;
                    }
                }
                if (canPlace)
                {
                    pickItem.transform.position = finalPosition;
                    pickItem.OnItemPut(putCell);
                    ItemMovementHelper.Instance.OnItemPut();
                }
                else
                    pickItem.ResetItemPosition();
            }
            else
            {
                pickItem.ResetItemPosition();
            }
            pickItem.UpdateOccupiedCells();
            LevelManager.Instance.CurrentLevel.CheckAllExitDoor();
            CleanupDragState();
        }

        public override void OnTouchCancel()
        {
            if (pickItem != null)
            {
                pickItem.ResetItemPosition();
            }
            CleanupDragState();
        }

        protected void HighlightPossibleMoves()
        {
            if (pickItem == null) return;
            Board board = LevelManager.Instance.CurrentLevel.Board;
            foreach (var cell in board.Cells)
            {
                if (possibleMoveCells.Contains(cell))
                {
                    cell.SpriteRenderer.color = Color.green;
                }
                else
                {
                    cell.SpriteRenderer.color = Color.white;
                }
            }
        }

        protected void CleanupDragState()
        {
            pickItem = null;
            pickCell = null;
            isMoved = false;
            pickOffset = Vector3.zero;
            itemPos = Vector3.zero;
            lastDragPosition = Vector3.negativeInfinity;
            possibleMoveCells.Clear();
            Level level = LevelManager.Instance.CurrentLevel;
            if (level == null)
                return;
            Board board = level.Board;
            foreach (var cell in board.Cells)
            {
                var renderer = cell.SpriteRenderer;
                if (renderer != null)
                    renderer.color = Color.white;
            }
        }

        public virtual void SetPossibleMoveCells()
        {
            possibleMoveCells.Clear();
            if (pickCell == null) return;
            HashSet<BaseCell> visited = new HashSet<BaseCell>();
            Vector3 shapeOffset = pickItem.GetPickedShapeOffset();
            foreach (var myCell in pickCell.NearByCell)
            {
                CheckPossibleMoveCell(myCell, pickItem, shapeOffset, visited);
            }
            HighlightPossibleMoves();
        }

        private void CheckPossibleMoveCell(BaseCell cell, BaseItem item, Vector3 shapeOffset, HashSet<BaseCell> visited)
        {
            if (cell == null || visited.Contains(cell)) return;
            visited.Add(cell);
            if (!IsPlaceItemInCell(cell, item, shapeOffset)) return;
            possibleMoveCells.Add(cell);
            foreach (var neighbor in cell.NearByCell)
            {
                CheckPossibleMoveCell(neighbor, item, shapeOffset, visited);
            }
        }

        [Button]
        public bool IsPlaceItemInCell(BaseCell cell, BaseItem baseItem, Vector3 shapeOffset)
        {
            Board board = LevelManager.Instance.CurrentLevel.Board;
            Vector3 offset = new Vector3(shapeOffset.x * baseItem.Spacing, 0f, shapeOffset.z * baseItem.Spacing);
            Vector3 basePos = cell.transform.position - offset;
            for (int i = 0; i < baseItem.ShapePositions.Count; i++)
            {
                Vector3 worldPos = basePos + new Vector3(baseItem.ShapePositions[i].x * baseItem.Spacing, 0f, baseItem.ShapePositions[i].z * baseItem.Spacing);
                BaseCell baseCell = board.GetCellAtWorldPos(worldPos);
                if (baseCell == null || (baseCell.HasItem && baseCell.Item != baseItem) || baseCell.IsBlock)
                {
                    return false;
                }
            }
            //Debug.LogError("cell " + cell.cellId + "__" + canPut);
            return true;
        }
    }
}
