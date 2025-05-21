using System;
using System.Collections;
using System.Collections.Generic;
using Tag.Block;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Tag.Block.Editor
{
    [HideLabel, TabGroup("Border Creatore"), HideReferenceObjectPicker]
    public class BorderCreatore
    {
        #region PUBLIC_VARS
        [SerializeField] private GameObject topBorderPrefab;
        
        [SerializeField] private GameObject bottomBorderPrefab;
        [SerializeField] private GameObject leftBorderPrefab;
        [SerializeField] private GameObject rightBorderPrefab;
        [SerializeField] private GameObject cornerBorderPrefab;
        [SerializeField, ReadOnly] private Transform borderParent;

        [FoldoutGroup("Border Settings")]
        [SerializeField] private float borderHeight = 0.1f;

        [FoldoutGroup("Border Settings")]
        [SerializeField] private bool autoDetectTurns = true;

        [FoldoutGroup("Border Settings")]
        [SerializeField] private Color borderColor = new Color(0.5f, 0.25f, 0.1f);

        private List<GameObject> borderObjects = new List<GameObject>();
        private LevelCreatore levelCreatore;
        #endregion

        #region CONSTRUCTORS
        public BorderCreatore(LevelCreatore levelCreatore = null)
        {
            this.levelCreatore = levelCreatore;

            // Load default border prefabs
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    LoadDefaultBorderPrefabs();
                }
            };
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void OnSceneGui()
        {
            if (levelCreatore == null)
                return;
            Level selectedLevel = levelCreatore.level;
            if (selectedLevel == null && Selection.activeGameObject != null)
            {
                selectedLevel = Selection.activeGameObject.GetComponentInParent<Level>();
            }
        }

        [Button("Set Border")]
        public void SetBorder()
        {
            if (levelCreatore == null || levelCreatore.level == null || levelCreatore.level.Board == null)
            {
                Debug.LogError("No level or board available");
                return;
            }

            ClearExistingBorders();
            FindOrCreateBorderParent();
            CreateBorders();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void LoadDefaultBorderPrefabs()
        {
            topBorderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MainGame/Prefabs/Borders/TopBorder.prefab");
            bottomBorderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MainGame/Prefabs/Borders/BottomBorder.prefab");
            leftBorderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MainGame/Prefabs/Borders/LeftBorder.prefab");
            rightBorderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MainGame/Prefabs/Borders/RightBorder.prefab");
            cornerBorderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MainGame/Prefabs/Borders/CornerBorder.prefab");

        }

        private void FindOrCreateBorderParent()
        {
            // First try to find existing Border transform
            for (int i = 0; i < levelCreatore.level.Board.transform.childCount; i++)
            {
                Transform child = levelCreatore.level.Board.transform.GetChild(i);
                if (child.name == "Borders")
                {
                    borderParent = child;
                    return;
                }
            }

            // If no border parent found, create one
            GameObject borderGameObject = new GameObject("Borders");
            borderGameObject.transform.SetParent(levelCreatore.level.Board.transform);
            borderGameObject.transform.localPosition = Vector3.zero;
            borderParent = borderGameObject.transform;
        }

        private void ClearExistingBorders()
        {
            if (borderParent != null)
            {
                // Destroy all existing border objects
                for (int i = borderParent.childCount - 1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(borderParent.GetChild(i).gameObject);
                }
            }

            borderObjects.Clear();
        }

        private void CreateBorders()
        {
            if (topBorderPrefab == null || bottomBorderPrefab == null || leftBorderPrefab == null || rightBorderPrefab == null || cornerBorderPrefab == null)
            {
                Debug.LogError("Border prefabs not assigned");
                return;
            }

            List<BaseCell> cells = levelCreatore.level.Board.Cells;
            if (cells == null || cells.Count == 0)
            {
                Debug.LogError("No cells found in the board");
                return;
            }

            // Find the board boundaries
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (BaseCell cell in cells)
            {
                if (cell.transform.position.x < minX) minX = cell.transform.position.x;
                if (cell.transform.position.x > maxX) maxX = cell.transform.position.x;
                if (cell.transform.position.z < minZ) minZ = cell.transform.position.z;
                if (cell.transform.position.z > maxZ) maxZ = cell.transform.position.z;
            }

            float cellSpacing = levelCreatore.level.Board.CellSpacing;
            float halfSpacing = cellSpacing / 2f;

            // Determine which cells are occupied to create a grid representation
            bool[,] occupiedCells = GetOccupiedCellsGrid(cells, minX, minZ, maxX, maxZ, cellSpacing);

            // Create a 2D grid of border elements
            CreateBordersFromGrid(occupiedCells, minX, minZ, cellSpacing);
        }

        private bool[,] GetOccupiedCellsGrid(List<BaseCell> cells, float minX, float minZ, float maxX, float maxZ, float cellSpacing)
        {
            // Calculate grid dimensions
            int cols = Mathf.RoundToInt((maxX - minX) / cellSpacing) + 3; // +3 for margins
            int rows = Mathf.RoundToInt((maxZ - minZ) / cellSpacing) + 3;

            bool[,] grid = new bool[rows, cols];

            // Mark cells as occupied in the grid
            foreach (BaseCell cell in cells)
            {
                int col = Mathf.RoundToInt((cell.transform.position.x - minX) / cellSpacing) + 1;
                int row = Mathf.RoundToInt((cell.transform.position.z - minZ) / cellSpacing) + 1;

                if (row >= 0 && row < rows && col >= 0 && col < cols)
                {
                    grid[row, col] = true;
                }
            }

            return grid;
        }

        private void CreateBordersFromGrid(bool[,] grid, float startX, float startZ, float cellSpacing)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // Create grid of border segments using edge detection
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Check if this is a cell we need to add border to
                    if (!grid[row, col]) continue;

                    // Get cell center position
                    float x = startX + (col - 1) * cellSpacing;
                    float z = startZ + (row - 1) * cellSpacing;

                    // Check surrounding cells to determine border edges
                    bool hasTop = row > 0 && grid[row - 1, col];
                    bool hasBottom = row < rows - 1 && grid[row + 1, col];
                    bool hasLeft = col > 0 && grid[row, col - 1];
                    bool hasRight = col < cols - 1 && grid[row, col + 1];

                    // Create borders at the edges
                    if (!hasTop)
                    {
                        // Top border (- Z)
                        CreateTopBorder(x, z - cellSpacing / 2);
                    }

                    if (!hasBottom)
                    {
                        // Bottom border (+ Z)
                        CreateBottomBorder(x, z + cellSpacing / 2);
                    }

                    if (!hasLeft)
                    {
                        // Left border (- X)
                        CreateLeftBorder(x - cellSpacing / 2, z);
                    }

                    if (!hasRight)
                    {
                        // Right border (+ X)
                        CreateRightBorder(x + cellSpacing / 2, z);
                    }

                    // Handle corners
                    // Check if we need to add corner pieces
                    bool hasTopLeft = row > 0 && col > 0 && grid[row - 1, col - 1];
                    bool hasTopRight = row > 0 && col < cols - 1 && grid[row - 1, col + 1];
                    bool hasBottomLeft = row < rows - 1 && col > 0 && grid[row + 1, col - 1];
                    bool hasBottomRight = row < rows - 1 && col < cols - 1 && grid[row + 1, col + 1];

                    // Top left corner
                    if (!hasTop && !hasLeft && !hasTopLeft)
                    {
                        CreateCornerPiece(x - cellSpacing / 2, z - cellSpacing / 2, Direction.Forward, Direction.Right);
                    }

                    // Top right corner
                    if (!hasTop && !hasRight && !hasTopRight)
                    {
                        CreateCornerPiece(x + cellSpacing / 2, z - cellSpacing / 2, Direction.Forward, Direction.Left);
                    }

                    // Bottom left corner
                    if (!hasBottom && !hasLeft && !hasBottomLeft)
                    {
                        CreateCornerPiece(x - cellSpacing / 2, z + cellSpacing / 2, Direction.Back, Direction.Right);
                    }

                    // Bottom right corner
                    if (!hasBottom && !hasRight && !hasBottomRight)
                    {
                        CreateCornerPiece(x + cellSpacing / 2, z + cellSpacing / 2, Direction.Back, Direction.Left);
                    }
                }
            }
        }

        private void CreateTopBorder(float x, float z)
        {
            if (topBorderPrefab == null) return;

            Vector3 position = new Vector3(x, borderHeight, z);
            GameObject borderObj = (GameObject)PrefabUtility.InstantiatePrefab(topBorderPrefab, borderParent);
            borderObj.transform.position = position;
            borderObj.transform.rotation = Quaternion.identity;
            borderObj.name = "Border_Top";
            borderObjects.Add(borderObj);
        }

        private void CreateBottomBorder(float x, float z)
        {
            if (bottomBorderPrefab == null) return;

            Vector3 position = new Vector3(x, borderHeight, z);
            GameObject borderObj = (GameObject)PrefabUtility.InstantiatePrefab(bottomBorderPrefab, borderParent);
            borderObj.transform.position = position;
            borderObj.transform.rotation = Quaternion.identity;
            borderObj.name = "Border_Bottom";
            borderObjects.Add(borderObj);
        }

        private void CreateLeftBorder(float x, float z)
        {
            if (leftBorderPrefab == null) return;

            Vector3 position = new Vector3(x, borderHeight, z);
            GameObject borderObj = (GameObject)PrefabUtility.InstantiatePrefab(leftBorderPrefab, borderParent);
            borderObj.transform.position = position;
            borderObj.transform.rotation = Quaternion.identity;
            borderObj.name = "Border_Left";
            borderObjects.Add(borderObj);
        }

        private void CreateRightBorder(float x, float z)
        {
            if (rightBorderPrefab == null) return;

            Vector3 position = new Vector3(x, borderHeight, z);
            GameObject borderObj = (GameObject)PrefabUtility.InstantiatePrefab(rightBorderPrefab, borderParent);
            borderObj.transform.position = position;
            borderObj.transform.rotation = Quaternion.identity;
            borderObj.name = "Border_Right";
            borderObjects.Add(borderObj);
        }

        private void CreateCornerPiece(float x, float z, Direction dir1, Direction dir2)
        {
            if (cornerBorderPrefab == null) return;

            Vector3 position = new Vector3(x, borderHeight, z);
            GameObject cornerObj = (GameObject)PrefabUtility.InstantiatePrefab(cornerBorderPrefab, borderParent);
            cornerObj.transform.position = position;
            cornerObj.transform.rotation = Quaternion.identity;
            cornerObj.name = $"Corner_{dir1}_{dir2}";

            // Determine the rotation based on which corner this is
            float rotation = 0f;
            if (dir1 == Direction.Forward && dir2 == Direction.Right) rotation = 0f;
            else if (dir1 == Direction.Forward && dir2 == Direction.Left) rotation = 270;//
            else if (dir1 == Direction.Back && dir2 == Direction.Left) rotation = 180f;
            else if (dir1 == Direction.Back && dir2 == Direction.Right) rotation = 90;//

            cornerObj.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

            borderObjects.Add(cornerObj);
        }
        #endregion
    }
}