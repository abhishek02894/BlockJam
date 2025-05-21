using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptTemplates : Editor
{
    #region Script Templates
    private const string MONOBEHAVIOUR_TEMPLATE = @"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class #SCRIPTNAME# : MonoBehaviour
{
    #region Variables
    
    #endregion

    #region Unity Methods
    private void Awake()
    {
        
    }
    
    private void Start()
    {
        
    }
    
    private void Update()
    {
        
    }
    #endregion

    #region Public Methods
    
    #endregion

    #region Private Methods
    
    #endregion
}";

    private const string SCRIPTABLE_OBJECT_TEMPLATE = @"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ""#SCRIPTNAME#"", menuName = ""ScriptableObjects/#SCRIPTNAME#"")]
public class #SCRIPTNAME# : ScriptableObject
{
    #region Variables
    
    #endregion

    #region Public Methods
    
    #endregion

    #region Private Methods
    
    #endregion
}";
    #endregion

    #region Menu Items
    [MenuItem("Assets/Create/C# Script with Regions", false, 80)]
    public static void CreateScriptWithRegions()
    {
        string path = GetSelectedPathOrFallback();
        string fileName = "NewMonoBehaviour.cs";
        string fullPath = Path.Combine(path, fileName);
        
        // Ensure filename is unique
        fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
        fileName = Path.GetFileName(fullPath);
        
        // Replace placeholder with actual script name
        string scriptName = Path.GetFileNameWithoutExtension(fileName);
        string content = MONOBEHAVIOUR_TEMPLATE.Replace("#SCRIPTNAME#", scriptName);
        
        // Write file
        File.WriteAllText(fullPath, content);
        AssetDatabase.Refresh();
        
        // Select the newly created file
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        Selection.activeObject = asset;
    }

    [MenuItem("Assets/Create/ScriptableObject with Regions", false, 81)]
    public static void CreateScriptableObjectWithRegions()
    {
        string path = GetSelectedPathOrFallback();
        string fileName = "NewScriptableObject.cs";
        string fullPath = Path.Combine(path, fileName);
        
        // Ensure filename is unique
        fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
        fileName = Path.GetFileName(fullPath);
        
        // Replace placeholder with actual script name
        string scriptName = Path.GetFileNameWithoutExtension(fileName);
        string content = SCRIPTABLE_OBJECT_TEMPLATE.Replace("#SCRIPTNAME#", scriptName);
        
        // Write file
        File.WriteAllText(fullPath, content);
        AssetDatabase.Refresh();
        
        // Select the newly created file
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        Selection.activeObject = asset;
    }
    #endregion

    #region Utility Methods
    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(path))
            {
                return path;
            }
            else if (File.Exists(path))
            {
                return Path.GetDirectoryName(path);
            }
        }
        
        return path;
    }
    #endregion
} 