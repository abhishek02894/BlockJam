﻿/// Credit Senshi  
/// Sourced from - http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/ (uGUITools link)

using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityEngine.UI.Extensions
{
    public static class uGUITools
    {
        [MenuItem("uGUI/Anchors to Corners #%[")]
        static void AnchorsToCorners()
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;
                RectTransform pt = Selection.activeTransform.parent as RectTransform;

                if (t == null || pt == null) return;

                Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
                Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);

                t.anchorMin = newAnchorsMin;
                t.anchorMax = newAnchorsMax;
                t.offsetMin = t.offsetMax = new Vector2(0, 0);
            }
        }

        [MenuItem("uGUI/Corners to Anchors #%]")]
        static void CornersToAnchors()
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;

                if (t == null) return;

                t.offsetMin = t.offsetMax = new Vector2(0, 0);
            }
        }

        [MenuItem("uGUI/Mirror Horizontally Around Anchors #%;")]
        static void MirrorHorizontallyAnchors()
        {
            MirrorHorizontally(false);
        }

        [MenuItem("uGUI/Mirror Horizontally Around Parent Center #%:")]
        static void MirrorHorizontallyParent()
        {
            MirrorHorizontally(true);
        }

        [MenuItem("Main Game/Clear Player Prefs")]
        static void ClearData()
        {
            PlayerPrefs.DeleteAll();
        }

        static void MirrorHorizontally(bool mirrorAnchors)
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;
                RectTransform pt = Selection.activeTransform.parent as RectTransform;

                if (t == null || pt == null) return;

                if (mirrorAnchors)
                {
                    Vector2 oldAnchorMin = t.anchorMin;
                    t.anchorMin = new Vector2(1 - t.anchorMax.x, t.anchorMin.y);
                    t.anchorMax = new Vector2(1 - oldAnchorMin.x, t.anchorMax.y);
                }

                Vector2 oldOffsetMin = t.offsetMin;
                t.offsetMin = new Vector2(-t.offsetMax.x, t.offsetMin.y);
                t.offsetMax = new Vector2(-oldOffsetMin.x, t.offsetMax.y);

                t.localScale = new Vector3(-t.localScale.x, t.localScale.y, t.localScale.z);
            }
        }

        [MenuItem("uGUI/Mirror Vertically Around Anchors #%'")]
        static void MirrorVerticallyAnchors()
        {
            MirrorVertically(false);
        }

        [MenuItem("uGUI/Mirror Vertically Around Parent Center #%\"")]
        static void MirrorVerticallyParent()
        {
            MirrorVertically(true);
        }

        [MenuItem("Scene/Load Loading Scene %F1")]
        static void LoadIntroScene()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/MainGame/Scenes/Loading.unity");
        }

        [MenuItem("Scene/Load MainScene Scene %F2")]
        static void LoadGameplayScene()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/MainGame/Scenes/MainScene.unity");
        }

        [MenuItem("Scene/Load LevelEditor Scene %F3")]
        static void LoadLevelEditorScene()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/MainGame/Scenes/LevelEditor.unity");
        }

        //[MenuItem("DataSheet/Area Data Sheet")]
        //static void AreaDataSheet()
        //{
        //    OpenSheet("", "Area Data Sheet");
        //}

        static void OpenSheet(string url, string sheetName)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                Debug.Log("<color=red>" + sheetName + "</color>" + " could not open");
            }
        }
        static void MirrorVertically(bool mirrorAnchors)
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;
                RectTransform pt = Selection.activeTransform.parent as RectTransform;

                if (t == null || pt == null) return;

                if (mirrorAnchors)
                {
                    Vector2 oldAnchorMin = t.anchorMin;
                    t.anchorMin = new Vector2(t.anchorMin.x, 1 - t.anchorMax.y);
                    t.anchorMax = new Vector2(t.anchorMax.x, 1 - oldAnchorMin.y);
                }

                Vector2 oldOffsetMin = t.offsetMin;
                t.offsetMin = new Vector2(t.offsetMin.x, -t.offsetMax.y);
                t.offsetMax = new Vector2(t.offsetMax.x, -oldOffsetMin.y);

                t.localScale = new Vector3(t.localScale.x, -t.localScale.y, t.localScale.z);
            }
        }
    }
}
