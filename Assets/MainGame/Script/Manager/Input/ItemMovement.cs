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

            // Pass the original screen position 'pos' to MoveItemOnBoard
            MoveItemOnBoard(pos);
            // The 'hit' member is no longer primarily used by MoveItemOnBoard for cell detection.
            // GetRayHit for backgroundLayerMask might still be useful for other things, 
            // or can be removed if MoveItemOnBoard is the only consumer of its specific hit.point.
            // For now, let's leave the GetRayHit call for background, as ItemDrag might still use hit.point for other reasons
            // or it might be used by derived classes.
            // However, MoveItemOnBoard will now do its own raycasting for cells.

            ItemMovementHelper.Instance.OnItemDrag();
            return itemPos;
        }

        private void MoveItemOnBoard(Vector3 screenPos) // Changed signature
        {
            if (pickItem == null || possibleMoveCells.Count == 0)
            {
                // If no item or no possible moves, do nothing or reset isMoved for this frame.
                // isMoved = false; // This depends on how isMoved is intended to be used overall.
                return;
            }

            BaseCell cellUnderPointer = null;
            RaycastHit cellHit; // Use a local RaycastHit for this specific raycast
            if (GetRayHit(screenPos, out cellHit, cellLayerMask))
            {
                cellUnderPointer = cellHit.collider.GetComponent<BaseCell>();
            }

            if (cellUnderPointer != null && possibleMoveCells.Contains(cellUnderPointer))
            {
                // This cellUnderPointer is a valid target.
                Vector3 shapeOffset = pickItem.GetPickedShapeOffset(); // Raw local offset
                Vector3 scaledOffset = new Vector3(shapeOffset.x * pickItem.Spacing, 0f, shapeOffset.z * pickItem.Spacing);
                Vector3 targetPositionForItemOrigin = cellUnderPointer.transform.position - scaledOffset;
                targetPositionForItemOrigin.y = pickItem.transform.position.y; // Maintain Y position

                if ((targetPositionForItemOrigin - pickItem.transform.position).sqrMagnitude > 0.0001f)
                {
                    pickItem.OnItemDrag(targetPositionForItemOrigin);
                    pickItem.SetLastValidPosition(targetPositionForItemOrigin); // Update last valid position
                    itemPos = targetPositionForItemOrigin;
                    isMoved = true;
                }
                // If not significantly different, item effectively hasn't moved to a new cell.
                // itemPos might already be targetPositionForItemOrigin from a previous frame.
                // isMoved remains as it was (potentially true from a previous frame, or false if newly picked).
            }
            else
            {
                // No valid cell under pointer or cell is not in possibleMoveCells. Item should not move.
                // isMoved = false; // Again, depends on overall isMoved semantics.
                // itemPos should remain the current valid snapped position of the item.
                // If the item was at a valid cell, itemPos should be that cell's derived item origin.
                // If it hasn't moved yet since pick, itemPos might be its initial pickup position.
                // For now, let itemPos remain pickItem.transform.position, which is the last valid snapped position.
                itemPos = pickItem.transform.position;
            }
        }
        public override void ItemPut(Vector3 pos)
        {
            if (pickItem == null)
            {
                CleanupDragState();
                return;
            }

            BaseCell putCell = null;
            // Raycast to find the cell under the cursor (using cellLayerMask)
            // Use a new RaycastHit variable for this specific raycast.
            if (GetRayHit(pos, out RaycastHit hitInfoForPut, cellLayerMask)) 
            {
                putCell = hitInfoForPut.collider.GetComponent<BaseCell>();
            }

            if (putCell != null && possibleMoveCells.Contains(putCell))
            {
                // Cell is valid and among the possible move cells. Proceed with placement.
                Vector3 putCellPos = putCell.transform.position;
                Vector3 shapeOffset = pickItem.GetPickedShapeOffset(); // Raw local offset
                Vector3 scaledOffset = new Vector3(shapeOffset.x * pickItem.Spacing, 0f, shapeOffset.z * pickItem.Spacing);
                Vector3 finalPosition = new Vector3(putCellPos.x - scaledOffset.x, pickItem.transform.position.y, putCellPos.z - scaledOffset.z);
                finalPosition.y = pickItem.transform.position.y; // Ensure Y is maintained

                // Existing Logging (keep this block for debugging)
                Debug.Log($"[ItemMovement] ItemPut for {pickItem.gameObject.name}: putCell = {putCell.gameObject.name} at {putCell.transform.position}");
                Debug.Log($"[ItemMovement] Dropped on a 'possibleMoveCell'.");
                Debug.Log($"[ItemMovement] Raw local shapeOffset from GetPickedShapeOffset() was = {shapeOffset}");
                Debug.Log($"[ItemMovement] pickItem.Spacing = {pickItem.Spacing}");
                Debug.Log($"[ItemMovement] The 'offset' variable (scaled for finalPosition calc) = {scaledOffset}"); // Use scaledOffset here
                Debug.Log($"[ItemMovement] Calculated finalPosition = {finalPosition}");

                pickItem.transform.position = finalPosition;
                pickItem.OnItemPut(putCell); // Call the BaseItem's OnItemPut
                ItemMovementHelper.Instance.OnItemPut(); // Notify helper
            }
            else
            {
                // Dropped on an invalid cell or not on a cell at all.
                Debug.Log($"[ItemMovement] ItemPut for {pickItem.gameObject.name}: Dropped on an invalid location (not in possibleMoveCells or off-board). Resetting item.");
                pickItem.ResetItemPosition(); // Resets to lastValidPosition (last valid snapped cell)
            }

            pickItem.UpdateOccupiedCells();
            LevelManager.Instance.CurrentLevel.CheckAllExitDoor(); // This seems like game-specific logic, keep it.
            CleanupDragState(); // Clears highlights, pickItem, etc.
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
            // lastDragPosition = Vector3.negativeInfinity; // lastDragPosition is no longer used with this drag logic
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
            if (pickItem == null) return; // Check pickItem instead of pickCell for robustness

            Board board = LevelManager.Instance.CurrentLevel.Board;
            Vector3 shapeOffset = pickItem.GetPickedShapeOffset(); // The local offset of the picked part

            foreach (BaseCell boardCell in board.Cells)
            {
                // Check if the pickItem can be placed with its 'picked part' (shapeOffset) aligned with this boardCell
                if (IsPlaceItemInCell(boardCell, pickItem, shapeOffset))
                {
                    possibleMoveCells.Add(boardCell);
                }
            }
            HighlightPossibleMoves();
        }

        // private void CheckPossibleMoveCell(BaseCell cell, BaseItem item, Vector3 shapeOffset, HashSet<BaseCell> visited)
        // {
        //     if (cell == null || visited.Contains(cell)) return;
        //     visited.Add(cell);
        //     if (!IsPlaceItemInCell(cell, item, shapeOffset)) return;
        //     possibleMoveCells.Add(cell);
        //     foreach (var neighbor in cell.NearByCell)
        //     {
        //         CheckPossibleMoveCell(neighbor, item, shapeOffset, visited);
        //     }
        // }

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
