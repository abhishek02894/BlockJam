using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tag.Block;
using System.IO;
using System.Reflection;

namespace Tag.Block.Editor
{
    /// <summary>
    /// Utility class for fixing door configurations at runtime
    /// </summary>
    public static class DoorSetup
    {
        public static void ResizeAllDoors(int cellWidth)
        {
            // Find all BaseExitDoor components in the scene
            BaseExitDoor[] doors = GameObject.FindObjectsOfType<BaseExitDoor>();
            int count = 0;

            foreach (BaseExitDoor door in doors)
            {
                if (door != null)
                {
                    Direction direction = GetDoorDirection(door);
                    ResizeDoor(door, direction, cellWidth);
                    count++;
                }
            }

            Debug.Log($"Resized {count} doors to {cellWidth} cells width.");
        }

        public static void ResizeDoor(BaseExitDoor door, Direction direction, int cellWidth)
        {
            if (door == null)
                return;

            // Get cell spacing - default is 1.0
            float cellSpacing = 1.0f;

            // Try to find the Level component to get the actual cell spacing
            Level level = door.transform.parent?.GetComponentInParent<Level>();
            if (level != null && level.Board != null)
            {
                cellSpacing = level.Board.CellSpacing;
            }

            // Calculate door dimensions
            float width = cellWidth * cellSpacing;
            float thickness = 0.2f;
            float height = 0.2f;

            // Update scale based on direction
            switch (direction)
            {
                case Direction.Left:
                case Direction.Right:
                    door.transform.localScale = new Vector3(thickness, height, width);
                    break;

                case Direction.Forward: // Top
                case Direction.Back: // Bottom
                    door.transform.localScale = new Vector3(width, height, thickness);
                    break;
            }

            // Update raycast points for the door
            UpdateRaycastPoints(door, direction, cellWidth, cellSpacing);

            // Update cells
            door.SetCells();
        }

        private static void UpdateRaycastPoints(BaseExitDoor door, Direction direction, int cellWidth, float cellSpacing)
        {
            if (door == null)
                return;

            // Get the raycast points via reflection
            var raycastPointField = typeof(BaseExitDoor).GetField("raycastPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            if (raycastPointField == null)
                return;

            List<GameObject> raycastPoints = raycastPointField.GetValue(door) as List<GameObject>;
            if (raycastPoints == null)
            {
                raycastPoints = new List<GameObject>();
                raycastPointField.SetValue(door, raycastPoints);
            }

            // Clear existing raycast points
            foreach (var point in raycastPoints)
            {
                if (point != null)
                    GameObject.DestroyImmediate(point);
            }

            raycastPoints.Clear();

            // Create exactly as many raycast points as the door width (in cells)
            int pointCount = cellWidth; // Exactly as many points as the door width
            float width = cellWidth * cellSpacing;

            for (int i = 0; i < pointCount; i++)
            {
                GameObject raycastPoint = new GameObject($"RaycastPoint_{i}");
                raycastPoint.transform.SetParent(door.transform);
                raycastPoints.Add(raycastPoint);

                // Calculate position to be at the exact center of each cell
                // For a door with 2 cells, we want positions at -0.5 and +0.5 (assuming cellSpacing=1)
                float cellCenter = (i * cellSpacing) + (cellSpacing * 0.5f);
                float startOffset = (width * 0.5f) - (cellSpacing * 0.5f);
                float offset = cellCenter - startOffset;

                switch (direction)
                {
                    case Direction.Left:
                        raycastPoint.transform.localPosition = new Vector3(0.6f, 0, offset);
                        break;
                    case Direction.Right:
                        raycastPoint.transform.localPosition = new Vector3(-0.6f, 0, offset);
                        break;
                    case Direction.Forward: // Top
                        raycastPoint.transform.localPosition = new Vector3(offset, 0, -0.6f);
                        break;
                    case Direction.Back: // Bottom
                        raycastPoint.transform.localPosition = new Vector3(offset, 0, 0.6f);
                        break;
                }
            }
        }

        public static Direction GetDoorDirection(BaseExitDoor door)
        {
            // Determine door direction by checking its position relative to the level board
            Transform parent = door.transform.parent;
            if (parent != null && parent.name == "Border")
            {
                Level level = parent.GetComponentInParent<Level>();
                if (level != null && level.Board != null)
                {
                    BaseCell[] cells = level.Board.GetComponentsInChildren<BaseCell>();
                    if (cells.Length > 0)
                    {
                        // Find board bounds
                        float minX = float.MaxValue, maxX = float.MinValue;
                        float minZ = float.MaxValue, maxZ = float.MinValue;

                        foreach (BaseCell cell in cells)
                        {
                            Vector3 pos = cell.transform.position;
                            minX = Mathf.Min(minX, pos.x);
                            maxX = Mathf.Max(maxX, pos.x);
                            minZ = Mathf.Min(minZ, pos.z);
                            maxZ = Mathf.Max(maxZ, pos.z);
                        }

                        // Add a small margin
                        float margin = 0.5f;
                        Vector3 doorPos = door.transform.position;

                        // Check which boundary this door is closest to
                        if (Mathf.Abs(doorPos.x - minX) < margin)
                            return Direction.Left;
                        else if (Mathf.Abs(doorPos.x - maxX) < margin)
                            return Direction.Right;
                        else if (Mathf.Abs(doorPos.z - maxZ) < margin)
                            return Direction.Forward;
                        else if (Mathf.Abs(doorPos.z - minZ) < margin)
                            return Direction.Back;
                    }
                }
            }

            // Try to determine from name if position doesn't work
            string name = door.gameObject.name.ToLower();
            if (name.Contains("left"))
                return Direction.Left;
            else if (name.Contains("right"))
                return Direction.Right;
            else if (name.Contains("top"))
                return Direction.Forward;
            else if (name.Contains("bottom"))
                return Direction.Back;

            // Default to right if we can't determine
            return Direction.Right;
        }
    }
}