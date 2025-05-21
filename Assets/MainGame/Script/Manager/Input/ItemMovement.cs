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
            if (pickItem == null || possibleMoveCells.Count == 0) return;

            // Find the nearest possibleMoveCell to the hit.point
            BaseCell nearestCell = null;
            float minDist = float.MaxValue;
            foreach (var cell in possibleMoveCells)
            {
                float dist = Vector3.Distance(cell.transform.position, hit.point);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestCell = cell;
                }
            }

            if (nearestCell == null) return;

            // Calculate the correct position for the item based on the picked shape offset
            Vector3 shapeOffset = pickItem.GetPickedShapeOffset();
            Vector3 offset = new Vector3(shapeOffset.x * pickItem.Spacing, 0f, shapeOffset.z * pickItem.Spacing);
            Vector3 desired = new Vector3(nearestCell.transform.position.x - offset.x, pickItem.transform.position.y, nearestCell.transform.position.z - offset.z);
            //Vector3 desired = hit.point - offset;

            //if ((desired - lastDragPosition).sqrMagnitude < 0.001f)
            //    return;

            lastDragPosition = desired;
            pickItem.OnItemDrag(desired);
            pickItem.SetLastValidPosition(desired);
            isMoved = true;
            itemPos = nearestCell.transform.position;
            HighlightPossibleMoves();
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
                Vector3 shapeOffset = pickItem.GetPickedShapeOffset();
                Vector3 offset = new Vector3(shapeOffset.x * pickItem.Spacing, 0f, shapeOffset.z * pickItem.Spacing);
                finalPosition = new Vector3(putCellPos.x - offset.x, pickItem.transform.position.y, putCellPos.z - offset.z);

                Board board = LevelManager.Instance.CurrentLevel.Board;
                canPlace = true;

                foreach (var posOffset in pickItem.ShapePositions)
                {
                    Vector3 worldPos = finalPosition + new Vector3(posOffset.x * pickItem.Spacing, 0f, posOffset.z * pickItem.Spacing);
                    BaseCell cell = board.GetCellAtWorldPos(worldPos);
                    if (cell == null || (cell.Item != null && cell.Item != pickItem) || !possibleMoveCells.Contains(putCell))
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
