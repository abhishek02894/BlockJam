using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tag.Block;
using System.IO;

namespace Tag.Block.Editor
{
    /// <summary>
    /// Utility class to create door prefabs for the level editor
    /// </summary>
    public static class DoorPrefab
    {
        public static void CreateDoorPrefabs()
        {
            string prefabFolder = "Assets/MainGame/Prefabs/Doors";

            // Create the folder if it doesn't exist
            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
                AssetDatabase.Refresh();
            }

            // Create each direction door prefab
            CreateDoorPrefab(prefabFolder, "LeftDoor", Direction.Left);
            CreateDoorPrefab(prefabFolder, "RightDoor", Direction.Right);
            CreateDoorPrefab(prefabFolder, "TopDoor", Direction.Forward);
            CreateDoorPrefab(prefabFolder, "BottomDoor", Direction.Back);

            AssetDatabase.Refresh();
        }

        private static void CreateDoorPrefab(string folder, string name, Direction direction)
        {
            string prefabPath = $"{folder}/{name}.prefab";

            // Check if the prefab already exists
            if (File.Exists(prefabPath))
            {
                Debug.Log($"Door prefab {name} already exists at {prefabPath}");
                return;
            }

            // Create the door GameObject
            GameObject doorObject = new GameObject(name);

            // Add required components - using 3D mesh components
            MeshFilter meshFilter = doorObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = doorObject.AddComponent<MeshRenderer>();
            BoxCollider collider = doorObject.AddComponent<BoxCollider>();
            BaseExitDoor exitDoor = doorObject.AddComponent<BaseExitDoor>();

            // Set the cube mesh
            meshFilter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            // Configure door dimensions based on direction
            float thickness = 0.2f;  // Thickness of the door
            float height = 0.2f;   // Height of the door

            // Set all door rotations to 0,0,0 as requested
            doorObject.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Set scale based on direction
            switch (direction)
            {
                case Direction.Left:
                    doorObject.transform.localScale = new Vector3(thickness, height, 1.0f);
                    doorObject.transform.position = new Vector3(-0.6f, 0, 0);
                    doorObject.GetComponent<BaseExitDoor>().SetRayCastDirection(Direction.Right);
                    break;

                case Direction.Right:
                    doorObject.transform.localScale = new Vector3(thickness, height, 1.0f);
                    doorObject.transform.position = new Vector3(0.6f, 0, 0);
                    doorObject.GetComponent<BaseExitDoor>().SetRayCastDirection(Direction.Left);
                    break;

                case Direction.Forward: // Top
                    doorObject.transform.localScale = new Vector3(1.0f, height, thickness);
                    doorObject.transform.position = new Vector3(0, 0, 0.6f);
                    doorObject.GetComponent<BaseExitDoor>().SetRayCastDirection(Direction.Back);
                    break;

                case Direction.Back: // Bottom
                    doorObject.transform.localScale = new Vector3(1.0f, height, thickness);
                    doorObject.transform.position = new Vector3(0, 0, -0.6f);
                    doorObject.GetComponent<BaseExitDoor>().SetRayCastDirection(Direction.Forward);
                    break;
            }

            // Create material for the door with a distinguishable color
            Material doorMaterial = new Material(Shader.Find("Standard"));
            doorMaterial.color = Color.red; // Default red color for visibility

            // Set the material
            renderer.sharedMaterial = doorMaterial;

            // Create and save the material as an asset
            string materialPath = $"{folder}/{name}Material.mat";
            AssetDatabase.CreateAsset(doorMaterial, materialPath);

            // Set up the exit door component
            exitDoor.colorType = 0; // Default color type

            // Create raycast points
            CreateRaycastPoints(doorObject, direction);

            // Save the prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(doorObject, prefabPath);

            // Cleanup the scene object
            Object.DestroyImmediate(doorObject);

            Debug.Log($"Created door prefab: {prefabPath}");
        }

        private static void CreateRaycastPoints(GameObject doorObject, Direction direction)
        {
            List<GameObject> raycastPoints = new List<GameObject>();

            // Create a single raycast point
            GameObject raycastPoint = new GameObject("RaycastPoint_0");
            raycastPoint.transform.SetParent(doorObject.transform);
            raycastPoints.Add(raycastPoint);

            // Position the raycast point based on direction
            switch (direction)
            {
                case Direction.Left:
                    raycastPoint.transform.localPosition = new Vector3(0.6f, 0, 0);
                    break;
                case Direction.Right:
                    raycastPoint.transform.localPosition = new Vector3(-0.6f, 0, 0);
                    break;
                case Direction.Forward: // Top
                    raycastPoint.transform.localPosition = new Vector3(0, 0, -0.6f);
                    break;
                case Direction.Back: // Bottom
                    raycastPoint.transform.localPosition = new Vector3(0, 0, 0.6f);
                    break;
            }

            // Assign raycast points to the BaseExitDoor component
            BaseExitDoor exitDoor = doorObject.GetComponent<BaseExitDoor>();
            if (exitDoor != null)
            {
                exitDoor.SetDoorValue(raycastPoints);
            }
        }
    }
}