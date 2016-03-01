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
	private enum RayCastGraphycAllower
	{
		CanUnset,
		NotSure,
		CantUnset
	}

	private class GraphicContainer
	{
		public GraphicContainer(Graphic graphic, RayCastGraphycAllower state)
		{
			Graphic = graphic;
			State = state;
		}
		public Graphic Graphic { get; set; }
		public RayCastGraphycAllower State { get; set; }
		public Selectable Selectable { get; set; }
	}

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
		m_selectableObjects.Clear();
		m_targetObjects.Clear();
		GameObject[] allgo = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject go in allgo)
		{
			var graphics = go.GetComponentsInChildren<Graphic>(true);
			foreach (var graphic in graphics)
			{
				var container = new GraphicContainer(graphic, RayCastGraphycAllower.CantUnset);
				m_targetObjects.Add(GetGameObjectPath(graphic.gameObject), container);
			}
			var selectables = go.GetComponentsInChildren<Selectable>(true);
			foreach (var selectable in selectables)
			{
				m_selectableObjects.Add(selectable);
			}
		}

		foreach (var graphicContainer in m_targetObjects)
		{
			var selectable = m_selectableObjects.FindAll(s => s.targetGraphic == graphicContainer.Value.Graphic);
			if (selectable.Count > 0)
			{
				graphicContainer.Value.State = RayCastGraphycAllower.CantUnset;
				graphicContainer.Value.Selectable = selectable[0];
			}
			// TODO: need some logic for detect RayCastGraphycAllower.NotSure. in cases like child in ScrollBar (check dropDown)
			else
			{
				graphicContainer.Value.State = RayCastGraphycAllower.CanUnset;
			}
		}

		m_sortedObjectsNames = m_targetObjects.Keys.ToList();
		m_sortedObjectsNames.Sort(MySorter);
	}

	private int MySorter(string x, string y)
	{
		var xContainer = m_targetObjects[x];
		var yContainer = m_targetObjects[y];
		int xState = (int)xContainer.State;
		int yState = (int)yContainer.State;
		int compare = xState.CompareTo(yState);
		if (compare == 0)
		{
			return x.CompareTo(y);
		}
		return compare;
	}

	private void OnGUI()
	{
		m_askPrefabChanges = EditorGUILayout.Toggle("AskPrefabChanges", m_askPrefabChanges);
		GameObject newSelected = null;
		var oldAligment = GUI.skin.button.alignment;
		var oldColor = GUI.color;
		GUI.skin.button.alignment = TextAnchor.MiddleLeft;
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); //, GUILayout.Width(600), GUILayout.Height(800)
		foreach (var objPath in m_sortedObjectsNames)
		{
			var container = m_targetObjects[objPath];
			if (!container.Graphic.raycastTarget)
			{
				continue;
			}
			switch (container.State)
			{
				case RayCastGraphycAllower.CantUnset:
					GUI.color = Color.red;
					break;
				case RayCastGraphycAllower.CanUnset:
					GUI.color = Color.green;
					break;
				case RayCastGraphycAllower.NotSure:
					GUI.color = Color.grey;
					break;
			}
			EditorGUILayout.BeginHorizontal();
			bool newRaycastTargetVal = EditorGUILayout.Toggle(container.Graphic.raycastTarget, m_maxToggleWidth);
			if (newRaycastTargetVal != container.Graphic.raycastTarget)
			{
				m_inChangingProcess = true;
				SetGraphicRaycastTarget(container.Graphic, newRaycastTargetVal);
				m_inChangingProcess = false;
			}
			if (GUILayout.Button(objPath))
			{
				newSelected = container.Graphic.gameObject;
			}
			if (container.Selectable)
			{
				if (GUILayout.Button("Select selectable"))
				{
					newSelected = container.Selectable.gameObject;
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		GUI.skin.button.alignment = oldAligment;
		GUI.color = oldColor;
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
		if (m_askPrefabChanges)
		{
			if (!EditorUtility.DisplayDialog("Apply prefab changes?", "Prefab found for " + graphic.gameObject, "Yes", "No"))
			{
				Debug.Log("Prefab found for " + graphic.gameObject, graphic.gameObject);
				return;
			}
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
	private Dictionary<string, GraphicContainer> m_targetObjects = new Dictionary<string, GraphicContainer>();
	private List<Selectable> m_selectableObjects = new List<Selectable>();
	private List<string> m_sortedObjectsNames = new List<string>();
	private GUILayoutOption m_maxToggleWidth = GUILayout.MaxWidth(20);

	private bool m_askPrefabChanges = true;
}
