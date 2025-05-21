using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Tag.Block
{
    public class FixDirectionItemMovement : ItemMovement
    {
        #region PUBLIC_VARS
        [ReadOnly,ShowInInspector] private bool isVerticalDirection = true;
        #endregion

        #region PRIVATE_VARS
        private Vector3 originalPosition;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public override bool ItemPick(Vector3 pos)
        {
            CleanupDragState();
            if (GetRayHit(pos, out hit, itemLayerMask))
            {
                BaseItem item = hit.collider.GetComponent<BaseItem>();
                if (item != null && item.CanMoveItem() && item.IsContainElement(ElementType.FixDirection))
                {
                    pickItem = item;
                    pickItem.OnItemPick();
                    if (item.GetElement<FixDirectionElementData>(ElementType.FixDirection) != null)
                    {
                        isVerticalDirection = item.GetElement<FixDirectionElementData>(ElementType.FixDirection).IsVertical;
                    }
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
                    originalPosition = pickItem.transform.position;
                    pickCell = LevelManager.Instance.CurrentLevel.Board.GetCellAtWorldPos(pickItem.transform.position + pickItem.GetPickedShapeOffset());
                    SetPossibleMoveCells();
                    HighlightPossibleMoves();
                    return true;
                }
            }
            CleanupDragState();
            return false;
        }

        public override void SetPossibleMoveCells()
        {
            possibleMoveCells.Clear();
            if (pickCell == null || pickItem == null) return;

            Board board = LevelManager.Instance.CurrentLevel.Board;
            Vector3 shapeOffset = pickItem.GetPickedShapeOffset();

            // Get the direction from the item
            bool isVertical = IsVerticalDirection();

            // Add cells in the fixed direction
            FindCellsInFixedDirection(pickCell, shapeOffset, isVertical);

            HighlightPossibleMoves();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private bool IsVerticalDirection()
        {
            return isVerticalDirection;
        }

        private void FindCellsInFixedDirection(BaseCell startCell, Vector3 shapeOffset, bool isVertical)
        {
            HashSet<BaseCell> visited = new HashSet<BaseCell>();
            Queue<BaseCell> cellQueue = new Queue<BaseCell>();

            cellQueue.Enqueue(startCell);
            visited.Add(startCell);

            while (cellQueue.Count > 0)
            {
                BaseCell currentCell = cellQueue.Dequeue();

                foreach (BaseCell neighborCell in currentCell.NearByCell)
                {
                    if (visited.Contains(neighborCell)) continue;

                    // Check if neighbor is in the allowed direction
                    bool isAllowedDirection = IsInAllowedDirection(currentCell, neighborCell, isVertical);

                    if (isAllowedDirection && IsPlaceItemInCell(neighborCell, pickItem, shapeOffset))
                    {
                        // Skip the original position of the item
                        Vector3 offset = new Vector3(shapeOffset.x * pickItem.Spacing, 0f, shapeOffset.z * pickItem.Spacing);
                        Vector3 potentialPosition = new Vector3(neighborCell.transform.position.x - offset.x, pickItem.transform.position.y, neighborCell.transform.position.z - offset.z);

                        // If this position is not the original position, add it
                        if (Vector3.Distance(potentialPosition, originalPosition) > 0.01f)
                        {
                            possibleMoveCells.Add(neighborCell);
                            cellQueue.Enqueue(neighborCell);
                        }

                        visited.Add(neighborCell);
                    }
                }
            }
            possibleMoveCells.Add(startCell);
        }

        private bool IsInAllowedDirection(BaseCell current, BaseCell neighbor, bool isVertical)
        {
            Vector3 direction = neighbor.transform.position - current.transform.position;

            if (isVertical)
            {
                // Allow only up/down movement (Z axis in your coordinate system)
                return Mathf.Approximately(direction.x, 0f) && !Mathf.Approximately(direction.z, 0f);
            }
            else
            {
                // Allow only left/right movement (X axis in your coordinate system)
                return !Mathf.Approximately(direction.x, 0f) && Mathf.Approximately(direction.z, 0f);
            }
        }
        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}
