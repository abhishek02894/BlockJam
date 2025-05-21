#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tag.Block;
using System.Linq;

public class ItemPrefabManager : EditorWindow
{
    [MenuItem("Tools/Block Game/Item Prefab Manager")]
    public static void ShowWindow()
    {
        GetWindow<ItemPrefabManager>("Item Prefab Manager");
    }

    private GameplayManager gameManagerInstance;
    private Object itemSlotPrefab;
    private List<BaseItem> itemPrefabs = new List<BaseItem>();
    private Dictionary<int, BaseItem> blockTypeToPrefab = new Dictionary<int, BaseItem>();
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        // Look for any existing GameManager in the scene
        gameManagerInstance = FindObjectOfType<GameplayManager>();
        FindItemPrefabs();
    }

    private void FindItemPrefabs()
    {
        itemPrefabs.Clear();
        blockTypeToPrefab.Clear();

        // Search for all item prefabs in the project
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/MainGame/Prefabs/Blocks" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                BaseItem item = prefab.GetComponent<BaseItem>();
                if (item != null)
                {
                    itemPrefabs.Add(item);
                    
                    // Extract the block type
                    int blockType = item.BlockType;
                    if (!blockTypeToPrefab.ContainsKey(blockType))
                    {
                        blockTypeToPrefab[blockType] = item;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate block type {blockType} found in prefabs: {blockTypeToPrefab[blockType].name} and {item.name}");
                    }
                }
            }
        }

        // Look for ItemSlot prefab
        string[] itemSlotGuids = AssetDatabase.FindAssets("ItemBlock t:Prefab");
        foreach (string guid in itemSlotGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && prefab.GetComponent<ItemSlot>() != null)
            {
                itemSlotPrefab = prefab;
                break;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Prefab Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        gameManagerInstance = EditorGUILayout.ObjectField("Game Manager", gameManagerInstance, typeof(GameplayManager), true) as GameplayManager;
        
        if (GUILayout.Button("Find Item Prefabs"))
        {
            FindItemPrefabs();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Slot Prefab");
        itemSlotPrefab = EditorGUILayout.ObjectField("Item Slot Prefab", itemSlotPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Found Item Prefabs", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var entry in blockTypeToPrefab)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Block Type {entry.Key}:", GUILayout.Width(100));
            EditorGUILayout.ObjectField(entry.Value, typeof(BaseItem), false);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Game Manager"))
        {
            if (gameManagerInstance == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a GameManager instance first.", "OK");
                return;
            }

            SetupGameManager();
        }
    }

    private void SetupGameManager()
    {
        SerializedObject serializedManager = new SerializedObject(gameManagerInstance);
        
        // Set up the item prefabs dictionary
        SerializedProperty itemPrefabsProp = serializedManager.FindProperty("itemPrefabs");
        itemPrefabsProp.ClearArray();
        
        int index = 0;
        foreach (var entry in blockTypeToPrefab)
        {
            itemPrefabsProp.InsertArrayElementAtIndex(index);
            SerializedProperty keyProp = itemPrefabsProp.GetArrayElementAtIndex(index).FindPropertyRelative("key");
            SerializedProperty valueProp = itemPrefabsProp.GetArrayElementAtIndex(index).FindPropertyRelative("value");
            
            keyProp.intValue = entry.Key;
            valueProp.objectReferenceValue = entry.Value;
            
            index++;
        }
        
        // Set up the ItemSlot prefab
        SerializedProperty itemSlotPrefabProp = serializedManager.FindProperty("itemSlotPrefab");
        if (itemSlotPrefab != null)
        {
            itemSlotPrefabProp.objectReferenceValue = (itemSlotPrefab as GameObject)?.GetComponent<ItemSlot>();
        }
        
        serializedManager.ApplyModifiedProperties();
        
        EditorUtility.DisplayDialog("Success", "GameManager has been set up with the item prefabs.", "OK");
    }
}
#endif 