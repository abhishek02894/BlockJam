using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tag.Block;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;
using UnityEngine.UIElements;
using System.IO;
using System;
using log4net.Core;

namespace Tag.Block.Editor
{
    [HideLabel, TabGroup("Board Creator"), HideReferenceObjectPicker]
    public class LevelCreatore
    {
        public int row = 0;
        public int col = 0;
        public int cellSpacing = 1;
        public List<BaseCell> cells = new List<BaseCell>();
        public int levelId;
        public int levelTimeInSecond = 180;
        [OnValueChanged("LevelChange")] public Level level;

        private BaseCell undoCell;

        // Reference to ItemCreatore to access placed items data
        private ItemCreatore itemCreatore;

        private ExitDoorCreatore exitDoorCreatore;

        private BaseCell cell;
        private BaseCell cellAlternate; // Add alternate cell for chess pattern
        private bool isDeleteActive;
        private Stack<Vector3> undoPosition;
        private bool isUndoActive;

        public LevelCreatore()
        {
            row = 1;
            col = 1;
            GetCell();
            isDeleteActive = false;
            isUndoActive = false;
        }

        // Set reference to ItemCreatore
        public void setLevel()
        {
            if (level == null || level != UnityEditor.Selection.activeGameObject?.GetComponent<Level>())
            {
                if (UnityEditor.Selection.activeGameObject?.GetComponent<Level>() != null)
                    level = UnityEditor.Selection.activeGameObject?.GetComponent<Level>();
            }
            if (level == null)
                level = UnityEngine.Object.FindObjectOfType<Level>();
        }
        public void SetItemCreatore(ItemCreatore itemCreatore)
        {
            this.itemCreatore = itemCreatore;
            setLevel();
        }
        public void SetExitDoorCreatore(ExitDoorCreatore exitDoorCreatore)
        {
            this.exitDoorCreatore = exitDoorCreatore;
        }
        public void OnSceneGui()
        {
            Event current = Event.current;


            if (current.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                BaseCell cell = null;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    cell = hit.collider.GetComponent<BaseCell>();
                }
                if (cell == null)
                    return;
                if (isDeleteActive)
                {
                    RemoveCell(cell);
                }
            }
        }
        private void LevelChange()
        {
            if (level != null)
            {
                cells = new List<BaseCell>();
                cells = level.GetComponentsInChildren<BaseCell>().ToList();
            }
        }

        private void RemoveCell(BaseCell cell)
        {
            if (cell == null)
                return;
            Board board = cell.GetComponentInParent<Board>();
            List<BaseCell> cells = board.Cells.ToList();
            isUndoActive = true;
            if (undoPosition == null)
                undoPosition = new Stack<Vector3>();
            undoPosition.Push(cell.transform.position);
            cells.Remove(cell);
            board.Cells1 = cells;
            GameObject.DestroyImmediate(cell.gameObject);
        }
        private void OnUndoButtonClick()
        {
            if (!isUndoActive || undoPosition == null || undoPosition.Count <= 0)
                return;
            GetCell();

            // Determine which cell prefab to use based on position (to maintain the chess pattern)
            Vector3 vector3 = undoPosition.Peek();
            int rowIdx = Mathf.RoundToInt(-vector3.z / cellSpacing + (this.row - 1) / 2f);
            int colIdx = Mathf.RoundToInt(vector3.x / cellSpacing + (this.col - 1) / 2f);
            bool isWhiteCell = (rowIdx + colIdx) % 2 == 0;
            BaseCell cellToUse = isWhiteCell ? this.cell : this.cellAlternate;

            BaseCell c = (BaseCell)PrefabUtility.InstantiatePrefab(cellToUse, level.transform);
            vector3 = undoPosition.Pop();
            c.name = "Cell-" + vector3.x + "-" + vector3.z;
            c.transform.position = new Vector3(vector3.x, 0, vector3.z);
            AddDeletedCell(c);
            isUndoActive = false;
        }
        private void AddDeletedCell(BaseCell cell)
        {
            if (cell == null)
                return;
            Board area = cell.GetComponentInParent<Board>();
            List<BaseCell> cells = area.Cells.ToList();
            cells.Add(cell);
            area.Cells1 = cells;
        }
        private void SetTileColor(BaseCell cell, Sprite tileSprite)
        {
            if (cell == null)
                return;
            SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = tileSprite;
        }

        private void GetCell()
        {
            if (cell == null)
                cell = AssetDatabase.LoadAssetAtPath<BaseCell>("Assets/MainGame/Prefabs/Cell1.prefab");
            if (cellAlternate == null)
                cellAlternate = AssetDatabase.LoadAssetAtPath<BaseCell>("Assets/MainGame/Prefabs/Cell2.prefab"); // Add a second cell prefab for alternating pattern
        }

        [OnInspectorGUI]
        private void SelecteDeselectButton()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Level"))
                LoadLevel();
            if (GUILayout.Button("Create Level"))
                CreateTile();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("CLEAR CELL"))
                ClearCells();
            Color oldColor = GUI.color;
            if (isDeleteActive)
                GUI.color = Color.red;
            if (GUILayout.Button("DELETE"))
            {
                isDeleteActive = !isDeleteActive;
                if (undoPosition != null)
                    undoPosition.Clear();
            }
            GUI.color = oldColor;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
                SaveToPrefab();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void LoadLevel()
        {
            // Clear current level if any
            if (level != null)
            {
                GameObject.DestroyImmediate(level.gameObject);
                level = null;
            }
            cells?.Clear();

            // Build the prefab path
            string folderPath = $"Assets/MainGame/Prefabs/Levels/Level_{levelId}";
            string prefabPath = $"{folderPath}/Level_{levelId}.prefab";
            string dataAssetPath = $"{folderPath}/LevelData_{levelId}.asset";

            // First, load the LevelDataSO as it contains complete level information
            LevelDataSO levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(dataAssetPath);
            if (levelData == null)
            {
                Debug.LogWarning($"Level data not found at path: {dataAssetPath}");
                EditorUtility.DisplayDialog("Load Failed", $"Could not find level data at {dataAssetPath}.", "OK");
                return;
            }

            // Now check if the level prefab exists
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Level prefab not found at path: {prefabPath}");
                EditorUtility.DisplayDialog("Load Failed", $"Could not find level prefab at {prefabPath}.", "OK");
                return;
            }

            // Instantiate the prefab in the editor (this has cells and border doors only, no items)
            GameObject levelInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (levelInstance == null)
            {
                Debug.LogWarning($"Failed to instantiate level prefab: {prefabPath}");
                return;
            }

            // Get the Level component
            level = levelInstance.GetComponent<Level>();
            if (level == null)
            {
                Debug.LogWarning($"No Level component found on prefab: {prefabPath}");
                GameObject.DestroyImmediate(levelInstance);
                return;
            }

            // Update editor fields from loaded level
            if (level.Board != null)
            {
                row = level.Board.Rows;
                col = level.Board.Columns;
                cellSpacing = (int)level.Board.CellSpacing;
                cells = level.Board.Cells.ToList();

                // Make sure the Border parent exists
                Transform borderParent = null;
                for (int i = 0; i < level.Board.transform.childCount; i++)
                {
                    Transform child = level.Board.transform.GetChild(i);
                    if (child.name == "Border")
                    {
                        borderParent = child;
                        break;
                    }
                }

                // Ensure all doors have their cells set
                if (borderParent != null)
                {
                    foreach (Transform doorTransform in borderParent)
                    {
                        BaseExitDoor door = doorTransform.GetComponent<BaseExitDoor>();
                        if (door != null)
                        {
                            // Ensure door cells are set up
                            door.SetCells();
                            door.SetDoorForEdiotr(door.colorType);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Loaded level does not have a Board component: {prefabPath}");
            }

            // Now recreate all items from the item data
            if (levelData != null && itemCreatore != null && level != null)
            {
                // Clear any existing items in the ItemCreatore tracking
                itemCreatore.ClearItemData();

                // Make sure the item parent exists
                if (level.ItemParent == null)
                {
                    GameObject itemsParent = new GameObject("Items");
                    itemsParent.transform.SetParent(level.Board.transform);
                    itemsParent.transform.localPosition = new Vector3(0, 0.2f, 0);
                    level.SetitemParent(itemsParent);
                }

                // Clear any existing items in the scene
                if (level.ItemParent != null)
                {
                    for (int i = level.ItemParent.childCount - 1; i >= 0; i--)
                    {
                        GameObject.DestroyImmediate(level.ItemParent.GetChild(i).gameObject);
                    }
                }

                // Log what we're loading
                Debug.Log($"Loading level data with {levelData.ItemsData.Count} items");

                // Create items based on the data
                foreach (var itemData in levelData.ItemsData)
                {
                    // Find the cell where this item should be placed
                    BaseCell targetCell = level.Board.GetCellById(itemData.cellId);
                    if (targetCell != null)
                    {
                        // Make sure the block type is valid
                        if (itemCreatore.blockItemPrefabs.Count > 0)
                        {
                            BaseItem gameObject = null;
                            for (int i = 0; i < itemCreatore.blockItemPrefabs.Count; i++)
                            {
                                if (itemCreatore.blockItemPrefabs[i].blocktype == itemData.blockType)
                                    gameObject = itemCreatore.blockItemPrefabs[i];
                            }
                            if (gameObject != null)
                            {
                                BaseItem baseItem = (BaseItem)PrefabUtility.InstantiatePrefab(gameObject, level.ItemParent);
                                if (baseItem != null)
                                {
                                    // Update its visual appearance based on item data
                                    itemCreatore.UpdateItemVisuals(baseItem, itemData);
                                    baseItem.transform.position = targetCell.transform.position;
                                    baseItem.transform.localPosition = new Vector3(baseItem.transform.localPosition.x, 0, baseItem.transform.localPosition.z);
                                    // Add this item to the ItemCreatore's tracked items
                                    itemCreatore.AddItemData(baseItem, itemData);
                                    baseItem.SetItemForEdiotr(itemData);
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("No item prefabs available in the ItemCreatore");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Cell with ID {itemData.cellId} not found when loading item");
                    }
                }

                Debug.Log($"Successfully loaded level {levelId} with {levelData.ItemsData.Count} items");
            }
            else
            {
                Debug.LogWarning("Could not recreate items: Level data or ItemCreatore is null");
            }
        }


        private void CreateTile()
        {
            CreateArea();
            ClearCells();
            GetCell();
            if (cells == null)
                cells = new List<BaseCell>();
            float xOffset = (col - 1) / 2f;
            float yOffset = (row - 1) / 2f;
            int cellid = 0;
            // Instantiate new grid
            for (int row = 0; row < this.row; row++)
            {
                for (int col = 0; col < this.col; col++)
                {
                    // Alternate between cell prefabs to create a chess-board pattern
                    bool isWhiteCell = (row + col) % 2 == 0;
                    BaseCell cellToUse = isWhiteCell ? this.cell : this.cellAlternate;

                    // Use PrefabUtility.InstantiatePrefab to maintain prefab connection
                    BaseCell cell = (BaseCell)PrefabUtility.InstantiatePrefab(cellToUse, level.Board.cellParent);
                    float x = (col - xOffset) * cellSpacing;
                    float y = -(row - yOffset) * cellSpacing;
                    cell.transform.position = new Vector3(x, 0, y);
                    cell.gameObject.name = "Cell-" + cellid;
                    cell.cellId = cellid;
                    cellid++;
                    cells.Add(cell);
                }
            }
            level.Board.Cells1 = cells;
            level.Board.SetRowColSpacing(row, col, cellSpacing);
        }

        //private void CreateTile()
        //{
        //    if (level == null)
        //        CreateArea();
        //    ClearCells();
        //    GetCell();
        //    if (cells == null)
        //        cells = new List<BaseCell>();
        //    for (int i = 0; i < grid.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < grid.GetLength(1); j++)
        //        {
        //            if (grid[i, j])
        //            {
        //                BaseCell c = GameObject.Instantiate(cell, level.transform);
        //                c.name = "Cell-" + i + "-" + j;
        //                c.transform.position = new Vector3(-i, 0, j);
        //                cells.Add(c);
        //            }
        //        }
        //    }

        //    level.SetCellIdsAndNearByCell();
        //}

        private void CreateArea()
        {
            if (level != null)
            {
                // Destroy all children of the level GameObject
                for (int i = level.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject child = level.transform.GetChild(i).gameObject;
                    GameObject.DestroyImmediate(child);
                }
                GameObject.DestroyImmediate(level.gameObject);
            }

            GameObject gm = new GameObject("Level_" + levelId);
            gm.transform.position = Vector3.zero;
            level = gm.AddComponent<Level>();

            ThreadCollector ThreadCollectorPrefab = AssetDatabase.LoadAssetAtPath<ThreadCollector>("Assets/MainGame/Prefabs/ThreadCollector.prefab");
            ThreadCollector ThreadCollector = (ThreadCollector)PrefabUtility.InstantiatePrefab(ThreadCollectorPrefab, level.transform);
            ThreadCollector.transform.localPosition = new Vector3(0, 1.5f, 4.5f);
            level.SetThreadCollecter(ThreadCollector);

            GameObject board = new GameObject("Board");
            board.transform.SetParent(gm.transform);
            board.transform.localPosition = Vector3.zero;

            GameObject Items = new GameObject("Items");
            Items.transform.SetParent(board.transform);
            Items.transform.localPosition = new Vector3(0, 0.1f, 0);

            GameObject CellParent = new GameObject("CellParent");
            CellParent.transform.SetParent(board.transform);
            CellParent.transform.localPosition = Vector3.zero;

            //GameObject Border = new GameObject("Border");
            //Border.transform.SetParent(board.transform);
            //Border.transform.localPosition = Vector3.zero;


            level.SetBoard(board.AddComponent<Board>());
            level.Board.cellParent = CellParent.transform;
            level.SetitemParent(Items);
        }

        private void ClearCells()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] != null)
                    GameObject.DestroyImmediate(cells[i].gameObject);
            }

            cells.Clear();
        }

        private void SaveToPrefab()
        {
            if (level != null)
            {
                level.Board.SetNearByCells();
                string folderPath = "Assets/MainGame/Prefabs/Levels/Level_" + levelId;
                string prefabPath = folderPath + "/Level_" + levelId + ".prefab";

                // Check if the folder exists, if not, create it
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    UnityEditor.AssetDatabase.Refresh(); // Refresh the AssetDatabase so Unity recognizes the new folder
                }

                // Collect item data before removing items
                List<ItemData> allItemsData = new List<ItemData>();
                if (itemCreatore != null)
                {
                    allItemsData = itemCreatore.GetAllItemsData();

                    // Log the data for debugging
                    Debug.Log($"Collected {allItemsData.Count} items data for saving");
                    foreach (var item in allItemsData)
                    {
                        Debug.Log($"  Item - BlockType: {item.blockType}, ColorType: {item.colorType}, CellID: {item.cellId}");
                    }
                }
                if (exitDoorCreatore != null)
                {

                    level.ExitDoor = new List<BaseExitDoor>();
                    List<BaseExitDoor> baseExitDoors = exitDoorCreatore.BorderDoors;
                    for (int i = 0; i < baseExitDoors.Count; i++)
                    {
                        if (baseExitDoors[i] != null)
                            level.AddExitDoor(baseExitDoors[i]);
                    }
                }

                // Save all the exit doors
                // We don't need to remove them since they're part of the level structure
                // Each door has a BaseExitDoor component that stores its configuration
                Transform borderParent = null;
                for (int i = 0; i < level.Board.transform.childCount; i++)
                {
                    Transform child = level.Board.transform.GetChild(i);
                    if (child.name == "Border")
                    {
                        borderParent = child;
                        break;
                    }
                }

                // Make sure all doors have their cells set
                if (borderParent != null)
                {
                    foreach (Transform doorTransform in borderParent)
                    {
                        BaseExitDoor door = doorTransform.GetComponent<BaseExitDoor>();
                        if (door != null)
                        {
                            // Ensure door cells are set up
                            door.SetCells();
                        }
                    }
                }

                // Remove all items from the level before saving the prefab
                // This ensures the prefab only contains cells, not items
                if (level.ItemParent != null && level.ItemParent.childCount > 0)
                {
                    for (int i = level.ItemParent.childCount - 1; i >= 0; i--)
                    {
                        GameObject.DestroyImmediate(level.ItemParent.GetChild(i).gameObject);
                    }
                }

                // Save the level as a prefab (now containing only cells and doors, no items)
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(level.gameObject, prefabPath);

                // Now create a LevelDataSO to store additional data
                string dataAssetPath = folderPath + "/LevelData_" + levelId + ".asset";

                // Check if the asset already exists
                LevelDataSO levelDataSO = AssetDatabase.LoadAssetAtPath<LevelDataSO>(dataAssetPath);

                // If not, create a new one
                if (levelDataSO == null)
                {
                    levelDataSO = ScriptableObject.CreateInstance<LevelDataSO>();
                    AssetDatabase.CreateAsset(levelDataSO, dataAssetPath);
                }

                // Set the prefab level reference and item data
                if (savedPrefab != null)
                {
                    // Set the level reference in the data asset
                    Level levelComponent = savedPrefab.GetComponent<Level>();
                    levelDataSO.Level = levelComponent;
                    levelDataSO.TimeInSecond = levelTimeInSecond;
                    // Set the items data in the level data asset
                    levelDataSO.ItemsData = allItemsData;

                    // Set default time for the level (can be adjusted later)
                    levelDataSO.TimeInSecond = 60;

                    // Save changes to the asset
                    EditorUtility.SetDirty(levelDataSO);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("Level " + levelId + " data saved at: " + dataAssetPath);
                }
                else
                {
                    Debug.LogError("Failed to save level prefab at: " + prefabPath);
                }

                // Clean up the scene
                GameObject.DestroyImmediate(level.gameObject);
                level = null;
                EditorUtility.DisplayDialog("Level Saved", "Level " + (levelId - 1) + " and its data have been saved successfully!", "OK");
            }
        }
    }
}
