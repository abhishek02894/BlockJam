using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Tag.Block;

namespace Tag.Block.Editor
{
    public class BorderPrefab
    {
        public static void CreateBorderPrefabs()
        {
            // Create prefabs folder if it doesn't exist
            string prefabFolderPath = "Assets/MainGame/Prefabs/Borders";
            if (!Directory.Exists(prefabFolderPath))
            {
                Directory.CreateDirectory(prefabFolderPath);
                AssetDatabase.Refresh();
            }

            // Create the straight border prefab
            GameObject straightBorder = CreateStraightBorderObject();
            SavePrefab(straightBorder, prefabFolderPath + "/StraightBorder.prefab");

            // Create the corner border prefab
            GameObject cornerBorder = CreateCornerBorderObject();
            SavePrefab(cornerBorder, prefabFolderPath + "/CornerBorder.prefab");

            AssetDatabase.Refresh();
            Debug.Log("Border prefabs created successfully!");
        }

        private static GameObject CreateStraightBorderObject()
        {
            // Create a basic cube for straight border segments
            GameObject straightBorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            straightBorder.name = "StraightBorder";
            
            // Set scale to make it look like a border segment
            straightBorder.transform.localScale = new Vector3(1f, 0.2f, 1f);
            
            // Create and assign material with brown color
            Material borderMaterial = new Material(Shader.Find("Standard"));
            borderMaterial.color = new Color(0.5f, 0.25f, 0.1f); // Brown color
            straightBorder.GetComponent<MeshRenderer>().sharedMaterial = borderMaterial;
            
            return straightBorder;
        }

        private static GameObject CreateCornerBorderObject()
        {
            // Create parent object for the corner
            GameObject cornerBorder = new GameObject("CornerBorder");
            
            // Create two cubes rotated to form a corner
            GameObject horizontal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horizontal.name = "Horizontal";
            horizontal.transform.SetParent(cornerBorder.transform);
            horizontal.transform.localScale = new Vector3(1f, 0.2f, 1f);
            horizontal.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            
            GameObject vertical = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vertical.name = "Vertical";
            vertical.transform.SetParent(cornerBorder.transform);
            vertical.transform.localScale = new Vector3(1f, 0.2f, 1f);
            vertical.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            
            // Create corner cube to fill in the gap
            GameObject cornerCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cornerCube.name = "Corner";
            cornerCube.transform.SetParent(cornerBorder.transform);
            cornerCube.transform.localScale = new Vector3(1f, 0.2f, 1f);
            cornerCube.transform.localPosition = Vector3.zero;
            
            // Create and assign material with brown color
            Material borderMaterial = new Material(Shader.Find("Standard"));
            borderMaterial.color = new Color(0.5f, 0.25f, 0.1f); // Brown color
            
            horizontal.GetComponent<MeshRenderer>().sharedMaterial = borderMaterial;
            vertical.GetComponent<MeshRenderer>().sharedMaterial = borderMaterial;
            cornerCube.GetComponent<MeshRenderer>().sharedMaterial = borderMaterial;
            
            return cornerBorder;
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            // Delete existing prefab if it exists
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            // Create the prefab
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            GameObject.DestroyImmediate(obj);
        }
    }
} 