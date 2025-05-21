using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaskObject : MonoBehaviour
{
    // Change to MeshRenderer array
    public List<MeshRenderer> renderersToMask = new List<MeshRenderer>();

    public void AddObjectToMask(List<MeshRenderer> meshRenderer)
    {
        for (int i = 0; i < meshRenderer.Count; i++)
        {
            if (!renderersToMask.Contains(meshRenderer[i]))
            {
                renderersToMask.Add(meshRenderer[i]);
            }
        }
        SetRenderQueue();
    }

    [ContextMenu("Set Render Queue")]
    public void SetRenderQueue()
    {
        for (int i = 0; i < renderersToMask.Count; i++)
        {
            // Check for null entries
            if (renderersToMask[i] == null)
            {
                //Debug.LogError($"MeshRenderer at index {i} is not assigned!", this.gameObject);
                continue;
            }

            try
            {
                // Check if the renderer has a material
                if (renderersToMask[i].sharedMaterial == null)
                {
                    //Debug.LogError($"MeshRenderer '{renderersToMask[i].name}' has no material assigned!", renderersToMask[i].gameObject);
                    continue;
                }

                // Create a separate material instance
                Material materialInstance = new Material(renderersToMask[i].sharedMaterial);
                materialInstance.renderQueue = 3002;

                // Assign the material instance
                renderersToMask[i].material = materialInstance;

                //Debug.Log($"Successfully set render queue for '{renderersToMask[i].name}'");
            }
            catch (System.Exception ex)
            {
                //Debug.LogError($"Error processing '{renderersToMask[i].name}': {ex.Message}", renderersToMask[i].gameObject);
            }
        }
    }

}

