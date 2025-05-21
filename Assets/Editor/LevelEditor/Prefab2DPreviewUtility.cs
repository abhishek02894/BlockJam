using UnityEngine;
using UnityEditor;

public static class Prefab2DPreviewUtility
{
    public static Texture2D RenderPrefab2DPreview(GameObject prefab, int width = 128, int height = 128)
    {
        // Create a temporary camera
        GameObject cameraGO = new GameObject("PreviewCamera");
        Camera cam = cameraGO.AddComponent<Camera>();
        cam.backgroundColor = Color.clear;
        cam.clearFlags = CameraClearFlags.Color;
        cam.orthographic = true;
        cam.orthographicSize = 1.5f;

        // Instantiate the prefab at the origin
        GameObject instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.hideFlags = HideFlags.HideAndDontSave;
        instance.transform.rotation = Quaternion.identity;

        // Calculate bounds to center the object
        var renderers = instance.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(Vector3.zero, Vector3.one);
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        // Move the instance to the center
        instance.transform.position = -bounds.center;

        // Set camera position for top-down (Y axis) view
        Vector3 camPos = new Vector3(0, bounds.extents.magnitude + 2f, 0); // Top-down
        cam.transform.position = camPos;
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Set up render texture
        RenderTexture rt = new RenderTexture(width, height, 16);
        cam.targetTexture = rt;
        cam.Render();

        // Read the texture
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Cleanup
        RenderTexture.active = null;
        cam.targetTexture = null;
        GameObject.DestroyImmediate(rt);
        GameObject.DestroyImmediate(cameraGO);
        GameObject.DestroyImmediate(instance);

        return tex;
    }
} 