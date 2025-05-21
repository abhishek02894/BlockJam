using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tag.Block.Editor
{
    [HideLabel, TabGroup("Exit Door Creatore"), HideReferenceObjectPicker]
    public class ExitDoorCreatore
    {
        #region PUBLIC_VARS
        [SerializeField] private List<BaseExitDoor> borderDoors = new List<BaseExitDoor>();

        // References to prefabs for different door types
        [HideInInspector] public BaseExitDoor leftDoorPrefab;
        [HideInInspector] public BaseExitDoor rightDoorPrefab;
        [HideInInspector] public BaseExitDoor topDoorPrefab;
        [HideInInspector] public BaseExitDoor bottomDoorPrefab;
        [HideInInspector] public ExitDoorSlot exitDoorPartPrefab; // Reference to ExitDoorPart prefab

        // Door settings
        [BlockColorId, FoldoutGroup("Door Settings"), OnValueChanged("UpdateBorderData")] public int selectedColorType = 0;

        [ReadOnly, FoldoutGroup("Door Settings")]
        public Direction doorDirection = Direction.Right;

        // Add door size control
        [HideInInspector, FoldoutGroup("Door Settings"), Range(1, 10), OnValueChanged("UpdateDoorSize")]
        public int doorSizeCells = 1;

        // Door scale multiplier
        [HideInInspector, FoldoutGroup("Door Settings"), Range(0.5f, 2.0f), OnValueChanged("UpdateDoorSize")]
        public float doorScaleMultiplier = 1.0f;

        private LevelCreatore levelCreatore;

        public List<BaseExitDoor> BorderDoors => borderDoors;

        #endregion

        #region PRIVATE_VARS
        private Level currentLevel;
        private Transform borderParent;
        private bool isPlacingDoor = false;
        private BaseExitDoor previewDoor;
        private Vector3 cursorPosition;
        private BaseExitDoor selectedDoorPrefab;
        private BaseExitDoor selectedDoor; // Currently selected door for deletion
        #endregion

        #region CONSTRUCTORS
        public ExitDoorCreatore(LevelCreatore levelCreatore = null)
        {
            this.levelCreatore = levelCreatore;

            // Load default door prefabs
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    LoadDefaultDoorPrefabs();
                }
            };
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void OnLevelChanged(Level newLevel)
        {
            if (newLevel != null && (currentLevel == null || currentLevel != newLevel))
            {
                currentLevel = newLevel;
                borderParent = null; // Reset so FindOrCreateBorderParent will be called
                FindOrCreateBorderParent();

                // Refresh the doors list
                RefreshDoorsList();

                // Update existing doors with raycast points
                UpdateExistingDoorsWithRaycastPoints();
            }
        }

        public void OnSceneGui()
        {
            // Get current level reference - check both the levelCreatore and Selection
            if (currentLevel == null || currentLevel != levelCreatore?.level)
            {
                // First try to get from levelCreatore
                if (levelCreatore != null && levelCreatore.level != null)
                {
                    currentLevel = levelCreatore.level;
                }
                // If still null, try to get from Selection
                else if (Selection.activeGameObject != null)
                {
                    Level selectedLevel = Selection.activeGameObject.GetComponent<Level>();
                    if (selectedLevel == null)
                    {
                        selectedLevel = Selection.activeGameObject.GetComponentInParent<Level>();
                    }
                    if (selectedLevel != null)
                    {
                        currentLevel = selectedLevel;
                    }
                }
            }

            // Find border parent if level exists but border parent is null
            if (currentLevel != null && currentLevel.Board != null && borderParent == null)
            {
                FindOrCreateBorderParent();
            }

            Event current = Event.current;

            if (isPlacingDoor && previewDoor != null)
            {
                // Update preview position based on mouse position
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("cell")))
                {
                    BaseCell hitCell = hit.collider.GetComponent<BaseCell>();
                    if (hitCell != null)
                    {
                        Vector3 targetPosition = CalculateDoorPosition(hitCell);
                        previewDoor.transform.position = targetPosition;

                        // Draw a visual indicator for placement
                        SceneView.RepaintAll();
                    }
                }

                // Place door on mouse click
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    PlaceDoorAtCurrentPosition();
                    current.Use();
                }

                // Cancel placement on right-click or escape
                if ((current.type == EventType.MouseDown && current.button == 1) ||
                    (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape))
                {
                    CancelPlacement();
                    current.Use();
                }
            }

            // Handle door selection for editing properties - now works with just a click (not Ctrl+click)
            if (current.type == EventType.MouseDown && current.button == 0 && !isPlacingDoor)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    BaseExitDoor hitDoor = hit.collider.GetComponentInParent<BaseExitDoor>();
                    if (hitDoor != null)
                    {
                        SelectDoor(hitDoor);
                        current.Use();
                    }
                }
            }

            // Draw handles for selected door
            if (selectedDoor != null)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireCube(selectedDoor.transform.position, selectedDoor.transform.localScale * 1.05f);

                // Handle door deletion with Delete key
                if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Delete)
                {
                    DeleteSelectedDoor();
                    current.Use();
                }
            }
        }

        // Add a new method to find or create the border parent
        private void FindOrCreateBorderParent()
        {
            // First try to find existing Border transform
            for (int i = 0; i < currentLevel.Board.transform.childCount; i++)
            {
                Transform child = currentLevel.Board.transform.GetChild(i);
                if (child.name == "Door")
                {
                    borderParent = child;
                    RefreshDoorsList(); // Refresh the list when border parent is found
                    return;
                }
            }

            // If no border parent found, create one
            GameObject borderGameObject = new GameObject("Door");
            borderGameObject.transform.SetParent(currentLevel.Board.transform);
            borderGameObject.transform.localPosition = Vector3.zero;
            borderParent = borderGameObject.transform;
            RefreshDoorsList(); // Refresh the list with newly created border parent
        }

        public void StartDoorPlacement(Direction direction)
        {
            doorDirection = direction;
            CancelPlacement(); // Clear any existing preview

            // Choose the right prefab based on direction
            switch (direction)
            {
                case Direction.Left:
                    selectedDoorPrefab = leftDoorPrefab;
                    break;
                case Direction.Right:
                    selectedDoorPrefab = rightDoorPrefab;
                    break;
                case Direction.Forward:
                    selectedDoorPrefab = topDoorPrefab;
                    break;
                case Direction.Back:
                    selectedDoorPrefab = bottomDoorPrefab;
                    break;
            }

            if (selectedDoorPrefab != null)
            {
                previewDoor = (BaseExitDoor)PrefabUtility.InstantiatePrefab(selectedDoorPrefab);
                previewDoor.transform.position = previewDoor.transform.position;
                previewDoor.transform.rotation = Quaternion.Euler(0, 0, 0);
                previewDoor.transform.localScale = Vector3.one; // Always (1,1,1)

                Transform partParent = previewDoor.transform.Find("DoorPartParent");
                float cellSpacing = currentLevel?.Board?.CellSpacing ?? 1.0f;
                previewDoor.ClearSlot();
                for (int i = 0; i < doorSizeCells; i++)
                {
                    ExitDoorSlot part = (ExitDoorSlot)PrefabUtility.InstantiatePrefab(exitDoorPartPrefab, partParent);
                    previewDoor.AddSlot(part);
                    part.name = $"ExitDoorPart_{i}";
                    Vector3 pos = Vector3.zero;
                    switch (doorDirection)
                    {
                        case Direction.Left:
                        case Direction.Right:
                            pos = new Vector3(0, 0, -((doorSizeCells - 1) * cellSpacing / 2) + (i * cellSpacing));
                            break;
                        case Direction.Forward:
                        case Direction.Back:
                            pos = new Vector3(0, 0, (-((doorSizeCells - 1) * cellSpacing / 2) + (i * cellSpacing)));
                            break;
                    }
                    part.transform.localPosition = pos;
                }
                previewDoor.SetDoorForEdiotr(selectedColorType);
                isPlacingDoor = true;
            }
            else
            {
                Debug.LogWarning("Failed to create preview: Selected door prefab is missing");
            }
        }
        public void UpdateBorderData()
        {
            if (selectedDoor == null)
                return;

            selectedDoor.SetDoorForEdiotr(selectedColorType);
        }
        public void RefreshDoorsList()
        {
            if (currentLevel == null || currentLevel.Board == null || borderParent == null)
            {
                Debug.LogWarning("Cannot refresh doors list: Level or border parent is missing");
                return;
            }

            // Clear the current list
            borderDoors.Clear();

            // Find all doors in the border parent
            BaseExitDoor[] doors = borderParent.GetComponentsInChildren<BaseExitDoor>();
            if (doors != null && doors.Length > 0)
            {
                borderDoors.AddRange(doors);
                Debug.Log($"Door list refreshed. Found {borderDoors.Count} doors.");
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void LoadDefaultDoorPrefabs()
        {
            // Check if we need to create the door prefabs first
            bool needsCreation = false;

            // Check if any of the prefabs don't exist
            if (!AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/LeftDoor.prefab") ||
                !AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/RightDoor.prefab") ||
                !AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/TopDoor.prefab") ||
                !AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/BottomDoor.prefab"))
            {
                needsCreation = true;
            }

            // Create the door prefabs if needed
            if (needsCreation)
            {
                DoorPrefab.CreateDoorPrefabs();
            }

            // Try to load door prefabs from the project
            leftDoorPrefab = AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/LeftDoor.prefab");
            rightDoorPrefab = AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/RightDoor.prefab");
            topDoorPrefab = AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/TopDoor.prefab");
            bottomDoorPrefab = AssetDatabase.LoadAssetAtPath<BaseExitDoor>("Assets/MainGame/Prefabs/Doors/BottomDoor.prefab");
            exitDoorPartPrefab = AssetDatabase.LoadAssetAtPath<ExitDoorSlot>("Assets/MainGame/Prefabs/Doors/ExitDoorPArt.prefab");

            // If any prefab is still missing, show an error
            if (leftDoorPrefab == null || rightDoorPrefab == null || topDoorPrefab == null || bottomDoorPrefab == null || exitDoorPartPrefab == null)
            {
                Debug.LogError("Failed to load or create door prefabs. Some doors might not work properly.");
            }
        }

        private Vector3 CalculateDoorPosition(BaseCell cell)
        {
            // Get the cell spacing and board dimensions
            float cellSpacing = 1.0f;
            int rows = 5;
            int columns = 5;

            if (currentLevel != null && currentLevel.Board != null)
            {
                cellSpacing = currentLevel.Board.CellSpacing;
                rows = currentLevel.Board.Rows;
                columns = currentLevel.Board.Columns;
            }

            // Calculate position based on cell position and door direction
            Vector3 position = cell.transform.position;

            // Determine if the cell is on the edge of the board
            bool isLeftEdge = false;
            bool isRightEdge = false;
            bool isTopEdge = false;
            bool isBottomEdge = false;

            // Find all cells to check if this is an edge cell
            BaseCell[] allCells = currentLevel.Board.GetComponentsInChildren<BaseCell>();

            // Get the min/max coordinates to determine board boundaries
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (BaseCell boardCell in allCells)
            {
                if (boardCell.transform.position.x < minX) minX = boardCell.transform.position.x;
                if (boardCell.transform.position.x > maxX) maxX = boardCell.transform.position.x;
                if (boardCell.transform.position.z < minZ) minZ = boardCell.transform.position.z;
                if (boardCell.transform.position.z > maxZ) maxZ = boardCell.transform.position.z;
            }

            // Check if this cell is on an edge
            isLeftEdge = Mathf.Approximately(cell.transform.position.x, minX);
            isRightEdge = Mathf.Approximately(cell.transform.position.x, maxX);
            isBottomEdge = Mathf.Approximately(cell.transform.position.z, minZ);
            isTopEdge = Mathf.Approximately(cell.transform.position.z, maxZ);

            // Calculate the offset based on cell spacing
            float offset = cellSpacing / 2.0f + 0.1f; // Half cell spacing plus a small gap

            // Base position of the cell
            Vector3 doorPosition = cell.transform.position;
            doorPosition.y = 0.1f; // Slightly above the board

            // Apply manual sizing instead of automatic cell detection
            if (previewDoor != null)
            {
                // (Removed: scale logic for previewDoor)
            }

            // Adjust position based on direction and if the cell is on the edge
            switch (doorDirection)
            {
                case Direction.Left:
                    if (isLeftEdge)
                    {
                        doorPosition.x = minX - offset;

                        // Add offset adjustment for even door sizes
                        if (doorSizeCells % 2 == 0)
                        {
                            doorPosition.z += cellSpacing / 2.0f;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Left doors should be placed on the left edge of the board");
                        doorPosition.x = cell.transform.position.x - offset;
                    }
                    break;

                case Direction.Right:
                    if (isRightEdge)
                    {
                        doorPosition.x = maxX + offset;

                        // Add offset adjustment for even door sizes
                        if (doorSizeCells % 2 == 0)
                        {
                            doorPosition.z += cellSpacing / 2.0f;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Right doors should be placed on the right edge of the board");
                        doorPosition.x = cell.transform.position.x + offset;
                    }
                    break;

                case Direction.Forward: // Top
                    if (isTopEdge)
                    {
                        doorPosition.z = maxZ + offset;

                        // Add offset adjustment for even door sizes
                        if (doorSizeCells % 2 == 0)
                        {
                            doorPosition.x += cellSpacing / 2.0f;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Top doors should be placed on the top edge of the board");
                        doorPosition.z = cell.transform.position.z + offset;
                    }
                    break;

                case Direction.Back: // Bottom
                    if (isBottomEdge)
                    {
                        doorPosition.z = minZ - offset;

                        // Add offset adjustment for even door sizes
                        if (doorSizeCells % 2 == 0)
                        {
                            doorPosition.x += cellSpacing / 2.0f;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Bottom doors should be placed on the bottom edge of the board");
                        doorPosition.z = cell.transform.position.z - offset;
                    }
                    break;
            }

            return doorPosition;
        }

        private void PlaceDoorAtCurrentPosition()
        {
            if (currentLevel == null || borderParent == null)
            {
                Debug.LogWarning("Cannot place door: No level or border parent selected");
                CancelPlacement();
                return;
            }

            BaseExitDoor exitDoor = (BaseExitDoor)PrefabUtility.InstantiatePrefab(selectedDoorPrefab, borderParent);
            exitDoor.transform.position = previewDoor.transform.position;
            exitDoor.transform.rotation = Quaternion.Euler(0, 0, 0);
            exitDoor.transform.localScale = Vector3.one; // Always (1,1,1)

            Transform partParent = exitDoor.transform.Find("DoorPartParent");
            float cellSpacing = currentLevel?.Board?.CellSpacing ?? 1.0f;
            exitDoor.ClearSlot();
            for (int i = 0; i < doorSizeCells; i++)
            {
                ExitDoorSlot part = (ExitDoorSlot)PrefabUtility.InstantiatePrefab(exitDoorPartPrefab, partParent);
                exitDoor.AddSlot(part);
                part.name = $"ExitDoorPart_{i}";
                Vector3 pos = Vector3.zero;
                switch (doorDirection)
                {
                    case Direction.Left:
                    case Direction.Right:
                        pos = new Vector3(0, 0, -((doorSizeCells - 1) * cellSpacing / 2) + (i * cellSpacing));
                        break;
                    case Direction.Forward:
                    case Direction.Back:
                        pos = new Vector3(0, 0, (-((doorSizeCells - 1) * cellSpacing / 2) + (i * cellSpacing)));
                        break;
                }
                part.transform.localPosition = pos;
            }

            // Set door properties
            if (exitDoor != null)
            {
                exitDoor.colorType = selectedColorType;
                exitDoor.SetCells();

                // Make sure to add to the border doors list
                if (!borderDoors.Contains(exitDoor))
                {
                    borderDoors.Add(exitDoor);
                    Debug.Log($"Door added to list. Total doors: {borderDoors.Count}");
                }

                // Add raycast points based on door size
                AddRaycastPoints(exitDoor);
            }
            exitDoor.SetDoorForEdiotr(selectedColorType);
            // Reset the preview
            CancelPlacement();
        }

        private void CancelPlacement()
        {
            if (previewDoor != null)
            {
                GameObject.DestroyImmediate(previewDoor.gameObject);
                previewDoor = null;
            }
            isPlacingDoor = false;
        }

        private void SelectDoor(BaseExitDoor door)
        {
            if (door != null)
            {
                // Update selected properties based on the door
                selectedColorType = door.colorType;

                // Determine door direction based on position and board boundaries
                doorDirection = DoorSetup.GetDoorDirection(door);

                // Get DoorPartParent and count ExitDoorPart children
                Transform partParent = door.transform.Find("DoorPartParent");
                if (partParent != null)
                {
                    doorSizeCells = partParent.childCount;
                    if (doorSizeCells > 0)
                    {
                        doorScaleMultiplier = partParent.GetChild(0).localScale.y;
                    }
                    else
                    {
                        doorScaleMultiplier = 1.0f;
                    }
                }
                else
                {
                    doorSizeCells = 1;
                    doorScaleMultiplier = 1.0f;
                }

                // Make sure doorSizeCells is at least 1
                doorSizeCells = Mathf.Max(1, doorSizeCells);
                doorScaleMultiplier = Mathf.Max(0.5f, doorScaleMultiplier);

                // Store the selected door for deletion
                selectedDoor = door;

                // Select the door in Unity's hierarchy
                Selection.activeGameObject = door.gameObject;

                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Selected door - Press Delete to remove"));
            }
        }

        private void DeleteSelectedDoor()
        {
            if (selectedDoor == null)
                return;

            // Remove from the list of border doors
            if (borderDoors.Contains(selectedDoor))
            {
                borderDoors.Remove(selectedDoor);
                Debug.Log($"Door removed from list. Remaining doors: {borderDoors.Count}");
            }

            // Destroy the door GameObject
            GameObject.DestroyImmediate(selectedDoor.gameObject);

            // Clear the selection
            selectedDoor = null;
            Selection.activeGameObject = null;

            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Door deleted"));
        }

        private void UpdateDoorActiveSide()
        {
            // This method would handle updating the door direction if it's already placed
            if (Selection.activeGameObject != null)
            {
                BaseExitDoor door = Selection.activeGameObject.GetComponent<BaseExitDoor>();
                if (door != null)
                {
                    // Recalculate cells for the door with new direction
                    door.SetCells();
                }
            }
        }

        [OnInspectorGUI]
        private void DisplayBorderEditor()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Border Exit Doors Editor", EditorStyles.boldLabel);

            // Level information
            GUILayout.Label("Current Level:", EditorStyles.boldLabel);
            string levelInfo = currentLevel != null ? currentLevel.name : "No level selected";
            GUILayout.Label(levelInfo);

            // Add refresh button to help with debugging
            if (GUILayout.Button("Refresh Level Reference"))
            {
                // Force update level reference from levelCreatore
                if (levelCreatore != null && levelCreatore.level != null)
                {
                    currentLevel = levelCreatore.level;
                    FindOrCreateBorderParent();
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Level reference refreshed"));
                }
                else
                {
                    // Try to find level from selection
                    Level selectedLevel = null;
                    if (Selection.activeGameObject != null)
                    {
                        selectedLevel = Selection.activeGameObject.GetComponent<Level>();
                        if (selectedLevel == null)
                        {
                            selectedLevel = Selection.activeGameObject.GetComponentInParent<Level>();
                        }
                    }

                    if (selectedLevel != null)
                    {
                        currentLevel = selectedLevel;
                        FindOrCreateBorderParent();
                        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Level reference refreshed from selection"));
                    }
                    else
                    {
                        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("No level found in the scene"));
                    }
                }
            }

            if (currentLevel == null)
            {
                EditorGUILayout.HelpBox("No level is active. Please create or load a level first.", MessageType.Warning);
            }
            else if (currentLevel.Board == null)
            {
                EditorGUILayout.HelpBox("Level has no Board component. Please fix the level structure.", MessageType.Error);
            }

            GUILayout.Space(10);

            // Door size controls
            GUILayout.Label("Door Size Settings:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            doorSizeCells = EditorGUILayout.IntSlider("Door Size (Cells)", doorSizeCells, 1, 10);
            doorScaleMultiplier = EditorGUILayout.Slider("Height Scale", doorScaleMultiplier, 0.5f, 2.0f);
            GUILayout.Space(10);

            GUILayout.Label("Place Door:", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(currentLevel == null || currentLevel.Board == null);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Left", GUILayout.Height(30)))
            {
                StartDoorPlacement(Direction.Left);
            }

            if (GUILayout.Button("Right", GUILayout.Height(30)))
            {
                StartDoorPlacement(Direction.Right);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Top", GUILayout.Height(30)))
            {
                StartDoorPlacement(Direction.Forward);
            }

            if (GUILayout.Button("Bottom", GUILayout.Height(30)))
            {
                StartDoorPlacement(Direction.Back);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Add a button for door deletion
            if (selectedDoor != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("DeSelected Door", GUILayout.Height(30), GUILayout.Width(200)))
                {
                    selectedDoor = null;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // List of all doors with delete buttons
                if (borderDoors.Count > 0)
                {
                    GUILayout.Label("All Doors:", EditorStyles.boldLabel);

                    for (int i = 0; i < borderDoors.Count; i++)
                    {
                        BaseExitDoor door = borderDoors[i];
                        if (door != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"Door {i + 1}: {door.name}");

                            // Select button
                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                            {
                                SelectDoor(door);
                            }

                            // Delete button
                            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                            {
                                // Store the door temporarily
                                BaseExitDoor doorToDelete = door;

                                // Select it first (to update UI)
                                SelectDoor(doorToDelete);

                                // Delete it
                                DeleteSelectedDoor();

                                // Break out of the loop since we've modified the collection
                                break;
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            GUILayout.Space(10);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
            GUILayout.Label("Tip: Click on a door to select it, then press Delete to remove it", EditorStyles.wordWrappedMiniLabel);

            GUILayout.EndVertical();
        }

        // New method to add raycast points
        private void AddRaycastPoints(BaseExitDoor exitDoor)
        {
            if (exitDoor == null || currentLevel == null || currentLevel.Board == null)
                return;

            float cellSpacing = currentLevel.Board.CellSpacing;

            // Clear any existing raycast points first
            for (int i = exitDoor.transform.childCount - 1; i >= 0; i--)
            {
                if (exitDoor.transform.GetChild(i).name.StartsWith("RaycastPoint"))
                {
                    GameObject.DestroyImmediate(exitDoor.transform.GetChild(i).gameObject);
                }
            }
            exitDoor.RaycastPoint.Clear();

            GameObject raycastPointParent = new GameObject($"RaycastPointParent");
            raycastPointParent.transform.SetParent(exitDoor.transform);
            raycastPointParent.transform.localPosition = Vector3.zero;
            raycastPointParent.transform.rotation = Quaternion.identity;
            raycastPointParent.transform.localScale = Vector3.one;
            // Create new raycast points based on door size and direction
            for (int i = 0; i < doorSizeCells; i++)
            {
                GameObject raycastPoint = new GameObject($"RaycastPoint_{i}");
                raycastPoint.transform.SetParent(raycastPointParent.transform);

                // Position raycast point based on door direction
                Vector3 pointPosition = Vector3.zero;
                switch (doorDirection)
                {
                    case Direction.Left:
                    case Direction.Right:
                        // For left/right doors, points should be distributed along Z axis
                        pointPosition = new Vector3(
                            0,
                            0,
                            -((doorSizeCells - 1) * cellSpacing / 2) + (i * cellSpacing)
                        );
                        break;
                    case Direction.Forward:
                    case Direction.Back:
                        // For top/bottom doors, points should be distributed along X axis
                        pointPosition = new Vector3(
                            -((doorSizeCells - 1) * cellSpacing / 2) + (i * cellSpacing),
                            0,
                            0
                        );
                        break;
                }

                raycastPoint.transform.localPosition = new Vector3(pointPosition.x, -0.12f, pointPosition.z);
                exitDoor.RaycastPoint.Add(raycastPoint);
                exitDoor.SetCells();
                // Add a small sphere to visualize the raycast point (will only be visible in editor)
                //if (Application.isEditor)
                //{
                //    GameObject visualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //    visualizer.name = "Visualizer";
                //    visualizer.transform.SetParent(raycastPoint.transform);
                //    visualizer.transform.localPosition = Vector3.zero;
                //    visualizer.transform.localScale = Vector3.one * 0.2f;

                //    // Make visualizer only visible in editor
                //    visualizer.GetComponent<MeshRenderer>().material.color = Color.red;
                //    visualizer.SetActive(false);
                //}
            }

            Debug.Log($"Added {doorSizeCells} raycast points to door {exitDoor.name}");
        }

        // Update existing doors with raycast points when loading
        public void UpdateExistingDoorsWithRaycastPoints()
        {
            if (borderDoors == null || borderDoors.Count == 0)
                return;

            foreach (BaseExitDoor door in borderDoors)
            {
                if (door != null)
                {
                    // Determine door direction
                    doorDirection = DoorSetup.GetDoorDirection(door);

                    // Calculate door size
                    float cellSpacing = currentLevel?.Board?.CellSpacing ?? 1.0f;
                    switch (doorDirection)
                    {
                        case Direction.Left:
                        case Direction.Right:
                            doorSizeCells = Mathf.RoundToInt(door.transform.localScale.z / cellSpacing);
                            break;
                        case Direction.Forward:
                        case Direction.Back:
                            doorSizeCells = Mathf.RoundToInt(door.transform.localScale.x / cellSpacing);
                            break;
                    }

                    doorSizeCells = Mathf.Max(1, doorSizeCells);

                    // Add raycast points
                    AddRaycastPoints(door);
                }
            }
        }
        #endregion
    }
}