using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tag.Block.Editor
{
    [HideLabel, TabGroup("item Creator"), HideReferenceObjectPicker]
    public class ItemCreatore
    {
        #region PUBLIC_VARS
        [BlockId, SerializeField, OnValueChanged("OnBlockIdChanged")] private int selectedBlockIndex = 0;
        // Reference to all available block items
        [HideInInspector] public Dictionary<int, BaseItem> blockItemPrefabs = new Dictionary<int, BaseItem>();
        // Reference to the LevelCreatore to access its level
        private LevelCreatore levelCreatore;
        
        // Item Data properties that can be set before placement
        [ReadOnly, FoldoutGroup("Item Data"), LabelText("Cell ID")]
        public int cellId = 0;
        
        [BlockColorId, FoldoutGroup("Item Data"), LabelText("Color Type"), OnValueChanged("OnItemPropertyChanged")] 
        public int colorType = 0;

        [FoldoutGroup("Item Data"), LabelText("Elements"), OnValueChanged("OnItemPropertyChanged")]
        public List<BaseElementData> elements = new List<BaseElementData>();

        [Space(15)]
        // Dictionary to store item data for placed items
        [SerializeField] private Dictionary<BaseItem, ItemData> placedItemsData = new Dictionary<BaseItem, ItemData>();

        [SerializeField] private List<BaseCell> OccupiedCells = new List<BaseCell>();

        // Remove Block Type from being editable in the inspector
        #endregion

        #region PRIVATE_VARS
        [ShowInInspector] private BaseItem currentSelectedItem;
        private bool isPlacingItem = false;
        [ShowInInspector] private BaseItem previewItem;
        private Vector3 cursorPosition;
        private Level currentLevel;
        private List<Vector3> lastCheckedPositions = new List<Vector3>();
        [ShowInInspector] private Texture2D selectedBlockPreview;
        #endregion

        #region UNITY_CALLBACKS
        public ItemCreatore(LevelCreatore levelCreatore = null)
        {
            // Initialize list first to prevent null reference
            blockItemPrefabs = new Dictionary<int, BaseItem>();
            this.levelCreatore = levelCreatore;

            // Load items - delay this call until after allocation
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    LoadBlockItems();
                    UpdateSelectedBlockPreview();
                }
            };
        }
        [FoldoutGroup("Item Data"), Button]
        private void UpdateAllItemOccupiedCells()
        {
            OccupiedCells.Clear();
            foreach (var item in placedItemsData)
            {
                UpdateOccupiedCells(item.Key);
            }
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void OnLevelChanged(Level level)
        {
            currentLevel = level;
            OnSceneGui();
        }

        public void OnSceneGui()
        {
            // First try to get level from the LevelCreatore
            if (levelCreatore != null && levelCreatore.level != null)
            {
                currentLevel = levelCreatore.level;
            }
            // As a fallback, check Selection
            else if (currentLevel == null || currentLevel != UnityEditor.Selection.activeGameObject?.GetComponent<Level>())
            {
                if (UnityEditor.Selection.activeGameObject?.GetComponent<Level>() != null)
                    currentLevel = UnityEditor.Selection.activeGameObject?.GetComponent<Level>();
            }

            Event current = Event.current;

            if (isPlacingItem && previewItem != null)
            {
                // Update preview position based on mouse position
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    BaseCell hitCell = hit.collider.GetComponent<BaseCell>();
                    if (OccupiedCells == null)
                        OccupiedCells = new List<BaseCell>();

                    if (hitCell != null)
                    {
                        cursorPosition = hitCell.transform.position;
                        previewItem.transform.position = new Vector3(cursorPosition.x, previewItem.transform.position.y, cursorPosition.z);

                        // Check placement validity and update preview visuals
                        bool canPlace = CanPlaceAtCurrentPosition();
                        DrawOccupiedCellsPreview();

                        SceneView.RepaintAll();
                    }
                }

                // Place item on mouse click
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    if (CanPlaceAtCurrentPosition())
                    {
                        PlaceItemAtCurrentPosition();
                        current.Use();
                    }
                    else
                    {
                        // Show a notification in the scene view that item can't be placed here
                        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Cannot place item here - position already occupied"));
                    }
                }

                // Cancel placement on right-click or escape
                if ((current.type == EventType.MouseDown && current.button == 1) ||
                    (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape))
                {
                    CancelPlacement();
                    current.Use();
                }
            }

            // Handle item selection for editing data
            if (current.type == EventType.MouseDown && current.button == 0 && current.control)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    BaseItem hitItem = hit.collider.GetComponentInParent<BaseItem>();
                    if (hitItem != null)
                    {
                        SelectItem(hitItem);
                        current.Use();
                    }
                }
            }

            // Handle item deletion with Delete key
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Delete && currentSelectedItem != null)
            {
                DeleteSelectedItem();
                current.Use();
            }
        }

        // Get all placed items data for saving to level
        public List<ItemData> GetAllItemsData()
        {
            List<ItemData> result = new List<ItemData>();
            foreach (var kvp in placedItemsData)
            {
                result.Add(kvp.Value);
            }
            return result;
        }

        [Button]
        public void AddItemDataToItem(List<BaseItem> baseItems)
        {
            ItemData itemData = null;
            for (int i = 0; i < baseItems.Count; i++)
            {
                itemData = new ItemData()
                {
                    blockType = baseItems[i].blocktype,
                    colorType = baseItems[i].ColorType,
                    //elements = baseItems[i].ItemData.elements,
                    //cellId = baseItems[i].ItemData.cellId,
                };
                AddItemData(baseItems[i], itemData);
            }
        }


        // Add item data to the dictionary (used when loading levels)
        public void AddItemData(BaseItem item, ItemData data)
        {
            if (item != null && data != null && !placedItemsData.ContainsKey(item))
            {
                placedItemsData[item] = data;
            }
        }

        // Clear all item data (used when loading a new level)
        public void ClearItemData()
        {
            placedItemsData.Clear();

            // Also reset current selection
            currentSelectedItem = null;

            // Reset item data properties
            colorType = 0;
            cellId = 0;
            elements.Clear();
        }

        // Delete the currently selected item
        public void DeleteSelectedItem()
        {
            if (currentSelectedItem == null)
                return;

            // Remove from occupied cells
            List<BaseCell> itemCells = currentSelectedItem.GetMyCells();
            if (itemCells != null && OccupiedCells != null)
            {
                foreach (var cell in itemCells)
                {
                    if (cell != null && OccupiedCells.Contains(cell))
                    {
                        OccupiedCells.Remove(cell);
                    }
                }
            }

            // Remove from placedItemsData
            if (placedItemsData.ContainsKey(currentSelectedItem))
            {
                placedItemsData.Remove(currentSelectedItem);
            }

            // Destroy the GameObject
            GameObject.DestroyImmediate(currentSelectedItem.gameObject);

            // Clear selection
            currentSelectedItem = null;

            // Notify user
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Item deleted"));
        }

        // Update the visual appearance of an item based on its data
        public void UpdateItemVisuals(BaseItem item, ItemData data)
        {
            if (item == null || data == null)
                return;
            colorType = data.colorType;
            cellId = data.cellId;

        }
        #endregion

        #region PRIVATE_FUNCTIONS
        [Button]
        public void DeselectCurrentSelectItem()
        {
            currentSelectedItem = null;
        }
        private void SelectItem(BaseItem item)
        {
            currentSelectedItem = item;
            UpdateItemCell(item);
            // Update UI fields with current data
            if (placedItemsData.TryGetValue(item, out ItemData data))
            {
                selectedBlockIndex = data.blockType;
                colorType = data.colorType;
                cellId = data.cellId;
                elements = new List<BaseElementData>(data.elements);
            }
            else
            {
                // If no data exists yet, create default data
                colorType = 0;
                cellId = GetCellIdForItem(item);
                elements = new List<BaseElementData>();
            }
            // Notify user
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Selected item for editing"));
        }

        private int GetCellIdForItem(BaseItem item)
        {
            var cells = item.GetMyCells();
            if (cells != null && cells.Count > 0 && cells[0] != null)
            {
                return cells[0].cellId;
            }

            return 0;
        }
        [FoldoutGroup("Item Data"), LabelText("Update Item"), Button]
        public void UpdateItemData()
        {
            if (currentSelectedItem == null)
                return;
                
            // Create updated item data
            ItemData data = new ItemData
            {
                blockType = selectedBlockIndex,
                colorType = colorType,
                cellId = cellId,
                elements = new List<BaseElementData>(elements)
            };
            
            // Apply to current selected item
            placedItemsData[currentSelectedItem] = data;
            currentSelectedItem.SetItemForEdiotr(data);
        }

        private void LoadBlockItems()
        {
            // Safety check
            if (blockItemPrefabs == null)
                blockItemPrefabs = new Dictionary<int, BaseItem>();
            else
                blockItemPrefabs.Clear();

            string[] blockGuids = AssetDatabase.FindAssets("Item-", new[] { "Assets/MainGame/Prefabs/Blocks" });

            foreach (string guid in blockGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BaseItem blockPrefab = AssetDatabase.LoadAssetAtPath<BaseItem>(path);

                if (blockPrefab != null)
                {
                    blockItemPrefabs.Add(blockPrefab.blocktype, blockPrefab);
                }
            }
            // Update preview after loading items
            UpdateSelectedBlockPreview();
        }

        private bool CanPlaceAtCurrentPosition()
        {
            if (currentLevel == null || currentLevel.Board == null || previewItem == null)
                return false;

            // Clear and update the preview's cells
            previewItem.ClearCell();

            // Update cells and track positions for visualization
            lastCheckedPositions.Clear();
            List<BaseCell> cellsToOccupy = new List<BaseCell>();

            foreach (var slotPos in previewItem.ItemSlot)
            {
                Vector3 worldPos = slotPos.transform.position;
                lastCheckedPositions.Add(worldPos); // Save for visualization

                BaseCell targetCell = currentLevel.Board.GetCellAtWorldPos(worldPos);

                // Check if cell exists at this position
                if (targetCell == null)
                    return false;

                // Check if cell is already occupied
                if (OccupiedCells.Contains(targetCell))
                    return false;

                // Add to potential cells
                cellsToOccupy.Add(targetCell);
                previewItem.AddCell(targetCell);
            }

            return cellsToOccupy.Count == previewItem.ItemSlot.Count;
        }

        private void DrawOccupiedCellsPreview()
        {
            if (lastCheckedPositions == null || lastCheckedPositions.Count == 0)
                return;
            bool canPlace = CanPlaceAtCurrentPosition();
            Color drawColor = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            Handles.color = drawColor;
            foreach (Vector3 pos in lastCheckedPositions)
            {
                Handles.DrawWireCube(pos, new Vector3(0.9f, 0.1f, 0.9f));
            }
        }

        private void PlaceItemAtCurrentPosition()
        {
            if (currentLevel == null || currentLevel.Board == null)
            {
                Debug.LogWarning("Cannot place item: No level or board selected");
                CancelPlacement();
                return;
            }

            if (!blockItemPrefabs.ContainsKey(selectedBlockIndex))
            {
                Debug.LogWarning("Invalid block selection");
                CancelPlacement();
                return;
            }

            // Final verification before placement
            if (!CanPlaceAtCurrentPosition())
            {
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Cannot place item here - position already occupied"));
                return;
            }

            BaseItem newItem = null;
            newItem = GameObject.Instantiate(blockItemPrefabs[selectedBlockIndex],
                                           new Vector3(cursorPosition.x, previewItem != null ? previewItem.transform.position.y : 0, cursorPosition.z),
                                           Quaternion.identity,
                                           currentLevel.ItemParent);
            if (newItem == null)
            {
                Debug.LogWarning("Failed to instantiate item");
                CancelPlacement();
                return;
            }

            if (newItem != null)
            {
                // Update cells and add them to the occupancy list
                List<BaseCell> newItemCells = new List<BaseCell>();
                foreach (var slotPos in newItem.ItemSlot)
                {
                    Vector3 worldPos = slotPos.transform.position;
                    BaseCell targetCell1 = currentLevel.Board.GetCellAtWorldPos(worldPos);
                    if (targetCell1 != null)
                    {
                        newItem.AddCell(targetCell1);
                        OccupiedCells.Add(targetCell1);
                        newItemCells.Add(targetCell1);
                    }
                }

                // Get cell ID from target cell or use the user-defined cellId
                BaseCell targetCell = currentLevel.Board.GetCellAtWorldPos(newItem.transform.position);
                int placedCellId = targetCell != null ? targetCell.cellId : cellId;

                // Create item data with all the properties set before placement
                ItemData itemData = new ItemData
                {
                    blockType = selectedBlockIndex,
                    colorType = colorType,
                    cellId = placedCellId,
                    elements = new List<BaseElementData>(elements)
                };

                // Apply item data to the placed item
                newItem.SetItemForEdiotr(itemData);
                placedItemsData[newItem] = itemData;
                ResetPreviewPosition();
            }
            else
            {
                Debug.LogWarning("Failed to place item: Item does not have BaseItem component");
                if (newItem != null)
                    GameObject.DestroyImmediate(newItem);
                CancelPlacement();
            }
        }
        private void UpdateItemCell(BaseItem baseItem)
        {
            Board board = currentLevel.Board;
            baseItem.ClearCell();
            foreach (var pos in baseItem.ItemSlot)
            {
                Vector3 worldPos = pos.transform.position;
                BaseCell targetCell = board.GetCellAtWorldPos(worldPos);
                if (targetCell != null)
                {
                    baseItem.AddCell(targetCell);
                }
            }
        }

        private void UpdateOccupiedCells(BaseItem baseItem)
        {
            Board board = currentLevel.Board;
            foreach (var pos in baseItem.ItemSlot)
            {
                Vector3 worldPos = pos.transform.position;
                BaseCell targetCell = board.GetCellAtWorldPos(worldPos);
                if (targetCell != null)
                {
                    OccupiedCells.Add(targetCell);
                }
            }
        }
        private void CancelPlacement()
        {
            if (previewItem != null)
            {
                if (previewItem.gameObject != null)
                    GameObject.DestroyImmediate(previewItem.gameObject);
                previewItem = null;
            }
            isPlacingItem = false;

            // Check if lastCheckedPositions is initialized
            if (lastCheckedPositions != null)
                lastCheckedPositions.Clear();
            else
                lastCheckedPositions = new List<Vector3>();
        }

        private void ResetPreviewPosition()
        {
            if (previewItem == null || currentLevel == null || currentLevel.Board == null)
            {
                CancelPlacement();
                return;
            }

            // Look for an unoccupied cell
            bool found = false;
            BaseCell[] cells = currentLevel.Board.GetComponentsInChildren<BaseCell>();

            if (cells == null || cells.Length == 0)
            {
                CancelPlacement();
                return;
            }

            foreach (BaseCell cell in cells)
            {
                if (cell == null || cell.transform == null)
                    continue;

                // Store original position
                Vector3 oldPosition = previewItem.transform.position;

                try
                {
                    // Move preview to this cell temporarily to check
                    previewItem.transform.position = new Vector3(
                        cell.transform.position.x,
                        previewItem.transform.position.y,
                        cell.transform.position.z
                    );

                    // Check if this would be a valid placement
                    if (CanPlaceAtCurrentPosition())
                    {
                        cursorPosition = cell.transform.position;
                        found = true;
                        break;
                    }

                    // Reset if not valid
                    previewItem.transform.position = oldPosition;
                }
                catch
                {
                    // If anything goes wrong, reset position and continue
                    if (previewItem != null && previewItem.transform != null)
                        previewItem.transform.position = oldPosition;
                }
            }

            if (!found)
            {
                // If all cells are occupied, cancel placement
                CancelPlacement();

                if (SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("No free cells available for placement"));
            }
        }

        [OnInspectorGUI]
        private void DisplayItemEditor()
        {
            // Safety check: make sure we have loaded items
            if (blockItemPrefabs == null || blockItemPrefabs.Count == 0)
            {
                LoadBlockItems();
            }

            GUILayout.BeginVertical();

            // Level information
            GUILayout.Label("Current Level:", EditorStyles.boldLabel);
            string levelInfo = currentLevel != null ? currentLevel.name : "No level selected";
            GUILayout.Label(levelInfo);

            GUILayout.Space(10);

            // Show count of items in dictionary
            GUILayout.Label($"Items in dictionary: {placedItemsData.Count}", EditorStyles.boldLabel);

            GUILayout.Space(10);

            // Display selected block preview
            GUILayout.Label("Selected Block:", EditorStyles.boldLabel);
            
            if (blockItemPrefabs.ContainsKey(selectedBlockIndex))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                // Display preview of the selected block
                if (selectedBlockPreview != null)
                {
                    GUILayout.Label(new GUIContent(selectedBlockPreview), GUILayout.Width(100), GUILayout.Height(100));
                }
                else
                {
                    GUILayout.Label("No preview available", GUILayout.Width(100), GUILayout.Height(100));
                }
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.Label(blockItemPrefabs[selectedBlockIndex].name, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                GUILayout.Label("No block selected", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Space(10);

            // Disable button if no level selected
            EditorGUI.BeginDisabledGroup(blockItemPrefabs.Count == 0 || currentLevel == null);
            if (GUILayout.Button("Place Selected Block"))
            {
                StartPlacingItem();
            }
            EditorGUI.EndDisabledGroup();

            // Show message if no level is selected
            if (currentLevel == null)
            {
                EditorGUILayout.HelpBox("Please select a level first", MessageType.Warning);
            }

            // Button to help users understand how to select items
            GUILayout.Space(5);
            GUILayout.Label("Tip: Ctrl+Click on a placed item to edit its data", EditorStyles.wordWrappedMiniLabel);

            GUILayout.EndVertical();
        }

        private void StartPlacingItem()
        {
            if (blockItemPrefabs == null || blockItemPrefabs.Count == 0 ||
                !blockItemPrefabs.ContainsKey(selectedBlockIndex))
                return;

            CancelPlacement(); // Clear any existing preview

            // Create preview item
            BaseItem previewObj = GameObject.Instantiate(blockItemPrefabs[selectedBlockIndex]);
            previewItem = previewObj.GetComponent<BaseItem>();

            if (previewItem != null)
            {
                isPlacingItem = true;
                UpdatePreviewItemData(); // Apply current property settings to preview
            }
            else
            {
                Debug.LogWarning("Failed to create preview: Selected prefab does not have BaseItem component");
                GameObject.DestroyImmediate(previewObj);
            }
        }

        private void OnItemPropertyChanged()
        {
            if (currentSelectedItem != null)
            {
                // Update the current selected item with new property values
                UpdateItemData();
            }
            
            if (isPlacingItem && previewItem != null)
            {
                // Update the preview item with new property values
                UpdatePreviewItemData();
            }
        }

        private void UpdatePreviewItemData()
        {
            if (previewItem == null) return;
            
            // Create item data with all current properties
            ItemData data = new ItemData
            {
                blockType = selectedBlockIndex,
                colorType = colorType,
                cellId = cellId,
                elements = new List<BaseElementData>(elements)
            };
            
            // Apply to preview
            previewItem.SetItemForEdiotr(data);
        }

        private void OnBlockIdChanged()
        {
            UpdateSelectedBlockPreview();
            if (isPlacingItem)
                UpdatePreviewItemData();
        }

        private void UpdateSelectedBlockPreview()
        {
            // Update the preview texture for the selected block
            if (blockItemPrefabs.ContainsKey(selectedBlockIndex))
            {
                selectedBlockPreview = AssetPreview.GetAssetPreview(blockItemPrefabs[selectedBlockIndex].gameObject);
                if (selectedBlockPreview == null)
                {
                    // Use custom 2D preview as fallback
                    selectedBlockPreview = Prefab2DPreviewUtility.RenderPrefab2DPreview(blockItemPrefabs[selectedBlockIndex].gameObject, 100, 100);
                }
            }
            else
            {
                selectedBlockPreview = null;
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
