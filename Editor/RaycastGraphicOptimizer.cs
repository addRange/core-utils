//----------------------------------------------------------------------------------------------
// Created by Pavel [Ryfi] Sakson (26/02/2016)
//-----------------------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


class RaycastGraphicOptimizer : EditorWindow
{
    [MenuItem("Window/Raycast Graphic Optimizer")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(RaycastGraphicOptimizer), false, "Raycast Graphic Optimizer",true);
    }

    void OnGUI()
    {
        GameObject[] allgo = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        List<Image> allObjects = new List<Image>();
        allObjects.Clear();

        foreach (GameObject go in allgo)
        {
            var images = go.GetComponentsInChildren<Image>(true);
            allObjects.AddRange(images);
        }

        EditorGUILayout.Separator();

        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, true); //, GUILayout.Width(600), GUILayout.Height(800)

        foreach (Image image in allObjects)
        {
            if (image.raycastTarget == true && image.GetComponent<Button>() == null && image.GetComponent<Scrollbar>() == null && image.GetComponent<Dropdown>() == null)
            {
                Rect r = EditorGUILayout.BeginHorizontal("Button");
                if (GUI.Button(r, GUIContent.none))
                {
                    UnityEditor.Selection.activeObject = image;
                    image.raycastTarget = !image.raycastTarget;

                    var prefabRoot = PrefabUtility.FindPrefabRoot(image.gameObject);
                    if (prefabRoot != null)
                    {
                        var prefabParent = PrefabUtility.GetPrefabParent(prefabRoot);
                        PrefabUtility.ReplacePrefab(prefabRoot, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
                    }
                }

                EditorGUILayout.LabelField(GetGameObjectPath(image.gameObject));
                image.raycastTarget = EditorGUILayout.Toggle(image.raycastTarget);

                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    private Vector2 m_scrollPos = new Vector2(0, 0);
}
