//----------------------------------------------------------------------------------------------
// Created by Pavel [Ryfi] Sakson (26/02/2016)
//-----------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class RaycastGraphicOptimizer : EditorWindow
{
	[MenuItem("Window/Raycast Graphic Optimizer")]

	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(RaycastGraphicOptimizer), false, "Raycast Graphic Optimizer", true);
	}

	private void OnEnable()
	{
		UpdateObjectsForOptimize();
		EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
	}

	private void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
	}

	private void OnHierarchyWindowChanged()
	{
		if (m_inChangingProcess)
		{
			return;
		}
		UpdateObjectsForOptimize();
	}

	private void UpdateObjectsForOptimize()
	{
		m_targetObjects.Clear();
		GameObject[] allgo = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject go in allgo)
		{
			var graphics = go.GetComponentsInChildren<Graphic>(true);
			foreach (var graphic in graphics)
			{
				m_targetObjects.Add(GetGameObjectPath(graphic.gameObject), graphic);
			}
		}
		m_sortedObjectsNames = m_targetObjects.Keys.ToList();
		m_sortedObjectsNames.Sort();
	}

	private void OnGUI()
	{
		GameObject newSelected = null;
		var oldAligment = GUI.skin.button.alignment;
		GUI.skin.button.alignment = TextAnchor.MiddleLeft;
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); //, GUILayout.Width(600), GUILayout.Height(800)
		foreach (var objPath in m_sortedObjectsNames)
		{
			var graphic = m_targetObjects[objPath];
			if (!graphic.raycastTarget)
			{
				continue;
			}
			EditorGUILayout.BeginHorizontal();

			bool newRaycastTargetVal = EditorGUILayout.Toggle(graphic.raycastTarget, m_maxToggleWidth);
			if (newRaycastTargetVal != graphic.raycastTarget)
			{
				m_inChangingProcess = true;
				SetGraphicRaycastTarget(graphic, newRaycastTargetVal);
				m_inChangingProcess = false;
			}
			if (GUILayout.Button(objPath))
			{
				newSelected = graphic.gameObject;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		GUI.skin.button.alignment = oldAligment;
		if (newSelected != null)
		{
			Selection.activeObject = newSelected;
		}
	}

	private void SetGraphicRaycastTarget(Graphic graphic, bool raycastTarget)
	{
		graphic.raycastTarget = raycastTarget;
		var prefabRoot = PrefabUtility.FindPrefabRoot(graphic.gameObject);
		if (prefabRoot == null)
		{
			return;
		}
		var prefabParent = PrefabUtility.GetPrefabParent(prefabRoot);
		if (prefabParent == null)
		{
			return;
		}
		if (!EditorUtility.DisplayDialog("Apply prefab changes?", "Prefab found for " + graphic.gameObject, "Yes", "No"))
		{
			Debug.Log("Prefab found for " + graphic.gameObject, graphic.gameObject);
			return;
		}
		PrefabUtility.ReplacePrefab(prefabRoot, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
		Debug.Log("Prefab applyed " + prefabParent, prefabParent);
	}

	private string GetGameObjectPath(GameObject obj)
	{
		string path = obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return path;
	}

	// Optimization
	private bool m_inChangingProcess = false;

	private Vector2 m_scrollPos = Vector2.zero;
	private Dictionary<string, Graphic> m_targetObjects = new Dictionary<string, Graphic>();
	private List<string> m_sortedObjectsNames = new List<string>();
	private GUILayoutOption m_maxToggleWidth = GUILayout.MaxWidth(20);
}
