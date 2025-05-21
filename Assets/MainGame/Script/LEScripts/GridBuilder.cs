using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag
{
    public class GridBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelDataReader levelDataReader;

        [Header("Grid Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;

        [Header("Prefabs")]
        [SerializeField] private GameObject defaultBlockPrefab;
        [SerializeField] private GameObject emptyBlockPrefab;
        [SerializeField] private Transform gridParent;

        // Dictionary to map cell codes to prefabs (can be expanded)
        [System.Serializable]
        public class BlockMapping
        {
            public string cellCode;
            public GameObject prefab;
            public Color color = Color.white;
        }

        [SerializeField] private List<BlockMapping> blockMappings = new List<BlockMapping>();
        private Dictionary<string, BlockMapping> mappingsLookup = new Dictionary<string, BlockMapping>();

        private GameObject[,] spawnedBlocks;
        [Button]
        private void Start()
        {
            InitializeMappings();
            BuildGrid();
        }

        private void InitializeMappings()
        {
            mappingsLookup.Clear();
            foreach (var mapping in blockMappings)
            {
                if (!string.IsNullOrEmpty(mapping.cellCode))
                {
                    mappingsLookup[mapping.cellCode] = mapping;
                }
            }
        }

        [ContextMenu("Rebuild Grid")]
        public void BuildGrid()
        {
            ClearGrid();

            // Get the level grid from LevelDataReader
            string[,] levelGrid = levelDataReader.GetTransposedLevelGrid(currentLevel);
            if (levelGrid == null)
            {
                Debug.LogError($"Failed to get grid for level {currentLevel}");
                return;
            }

            int rows = levelGrid.GetLength(0);
            int cols = levelGrid.GetLength(1);

            spawnedBlocks = new GameObject[rows, cols];

            // Create the parent object if it doesn't exist
            if (gridParent == null)
            {
                GameObject parent = new GameObject($"Level_{currentLevel}_Grid");
                gridParent = parent.transform;
                gridParent.position = transform.position;
            }

            // Spawn blocks for each cell
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    string cellValue = levelGrid[r, c];
                    Vector3 position = new Vector3(
                        c * cellSize + gridOffset.x,
                        0,
                        r * cellSize + gridOffset.y
                    );

                    GameObject blockPrefab = GetPrefabForCell(cellValue);
                    if (blockPrefab != null)
                    {
                        // Instantiate the block
                        GameObject block = Instantiate(blockPrefab, position, Quaternion.identity, gridParent);
                        block.name = $"Block_{r}_{c}_{cellValue}";

                        // Store reference to the spawned block
                        spawnedBlocks[r, c] = block;

                        // Apply color if available
                        ApplyBlockColor(block, cellValue);

                        // Set custom data from the cellValue if needed
                        SetBlockData(block, cellValue);
                    }
                }
            }

            Debug.Log($"Built grid for level {currentLevel} with dimensions {rows}x{cols}");
        }

        private void ClearGrid()
        {
            // Destroy existing blocks
            if (gridParent != null)
            {
                if (Application.isPlaying)
                {
                    foreach (Transform child in gridParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
                else
                {
                    while (gridParent.childCount > 0)
                    {
                        DestroyImmediate(gridParent.GetChild(0).gameObject);
                    }
                }
            }

            spawnedBlocks = null;
        }

        private GameObject GetPrefabForCell(string cellValue)
        {
            if (string.IsNullOrWhiteSpace(cellValue))
            {
                return emptyBlockPrefab;
            }

            // First check if we have an exact mapping
            if (mappingsLookup.TryGetValue(cellValue, out BlockMapping mapping))
            {
                return mapping.prefab;
            }

            // Try to parse the cell value format (e.g., "1/ng/org")
            string[] parts = cellValue.Split('/');

            // Check for partial matches in the code
            foreach (var entry in mappingsLookup)
            {
                if (cellValue.Contains(entry.Key))
                {
                    return entry.Value.prefab;
                }
            }

            // Default to the default block
            return defaultBlockPrefab;
        }

        private void ApplyBlockColor(GameObject block, string cellValue)
        {
            // Try to extract color from the cell value (e.g., "1/ng/org")
            string colorCode = "";
            string[] parts = cellValue.Split('/');

            if (parts.Length >= 3)
            {
                colorCode = parts[2];
            }

            // Check if we have a mapping for this color
            foreach (var mapping in blockMappings)
            {
                if (mapping.cellCode == colorCode || cellValue.Contains(mapping.cellCode))
                {
                    // Find renderer and set color
                    Renderer renderer = block.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = mapping.color;
                    }
                    break;
                }
            }
        }

        private void SetBlockData(GameObject block, string cellValue)
        {
            // Parse the cellValue and set any additional component data
            // This depends on your specific block component implementation

            // Example:
            // BlockComponent blockComponent = block.GetComponent<BlockComponent>();
            // if (blockComponent != null)
            // {
            //     string[] parts = cellValue.Split('/');
            //     if (parts.Length > 0 && int.TryParse(parts[0], out int value))
            //     {
            //         blockComponent.SetValue(value);
            //     }
            // }
        }

        public void SetLevel(int levelNumber)
        {
            currentLevel = levelNumber;
            BuildGrid();
        }
    }
}