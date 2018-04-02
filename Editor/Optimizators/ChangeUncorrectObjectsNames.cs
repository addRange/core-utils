//----------------------------------------------------------------------------------------------
// Created by Tanya Gaiduk (2017.03.23)
//-----------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public class ChangeUncorrectObjectsName : EditorWindow
{
	private enum Mode
	{
		OnScene,
		InResources,
		InAnimKeys,
	}

	[MenuItem("Window/Utils/Change Uncorrect Names")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(ChangeUncorrectObjectsName), false, "Change Uncorrect Names", true);
	}

	private void ChangeNamesForGameObjects(GameObject[] rootObjects) //Change names for GameObjects
	{
		foreach (GameObject rootObjectChangeName in rootObjects)
		{
			Transform[] objectsForChangeName = rootObjectChangeName.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < objectsForChangeName.Length; ++i)
			{
				string s = GetCorrectName(objectsForChangeName[i].gameObject);
				if (objectsForChangeName[i].name != s)
				{
					string path = GetGameObjectPath(objectsForChangeName[i].gameObject);
					if (m_targetObjects.ContainsKey(path))
					{
						Debug.LogError("Not supported MultiNames by one hierachy! " + objectsForChangeName[i].gameObject,
											objectsForChangeName[i].gameObject);
						continue;
					}

					m_targetObjects.Add(path, objectsForChangeName[i].gameObject);
				}
			}
		}
	}

	private void ChangeNamesForObjects(Object[] rootObjects) //Change names for Objects
	{
		for (int i = 0; i < rootObjects.Length; i++)
		{
			Object obj = rootObjects[i];
			string s = GetCorrectName(obj);
			if (obj.name != s)
			{
				string path = AssetDatabase.GetAssetPath(obj);
				if (m_targetObjects.ContainsKey(path))
				{
					Debug.LogError("WTF? " + path, obj);
					continue;
				}

				m_targetObjects.Add(path, obj);
			}
		}
	}

	private void ChangeNamesForAnimationKeys(AnimationClip anim) //Change names for AnimationsClip
	{
		var path = AssetDatabase.GetAssetPath(anim);
		anim = AssetDatabase.LoadMainAssetAtPath(path) as AnimationClip;
		EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(anim);
		for (int i = 0; i < bindings.Length; i++)
		{
			var curve = AnimationUtility.GetEditorCurve(anim, bindings[i]);
			AnimationUtility.SetEditorCurve(anim, bindings[i], null);
			string correctPath = GetCorrectName(bindings[i].path, false);
			bindings[i].path = correctPath;
			AnimationUtility.SetEditorCurve(anim, bindings[i], curve);
		}

		AssetDatabase.SaveAssets();
	}

	private void FindNamesForGameObjectsOnScene() //Find GameObject on Scene
	{
		List<GameObject> scenesRoots = new List<GameObject>();
		for (int i = 0; i < SceneManager.sceneCount; ++i)
		{
			var openedScene = SceneManager.GetSceneAt(i);
			GameObject[] rootObjectsOnScene = openedScene.GetRootGameObjects();
			scenesRoots.AddRange(rootObjectsOnScene);
		}

		m_targetObjects.Clear();
		ChangeNamesForGameObjects(scenesRoots.ToArray());
	}

	private void FindNamesForResources() //Find GameObjects and Objects in Resources
	{
		var objectGuids = AssetDatabase.FindAssets("t:Object");
		List<Object> rootObjects = new List<Object>();
		string assetPath1 = "Assets/Resources/";
		string assetPath2 = "Assets/SceneResources/";
		for (int i = 0; i < objectGuids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(objectGuids[i]);
			if (assetPath.Contains(assetPath1) || assetPath.Contains(assetPath2))
			{
				Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
				rootObjects.Add(obj);
			}
		}

		List<GameObject> listOfGameObjects = new List<GameObject>(); //selecting GameObjects from list Objects
		List<Object> listOfOtherObjects = new List<Object>();
		foreach (var obj in rootObjects)
		{
			if (obj is GameObject)
			{
				listOfGameObjects.Add(obj as GameObject);
			}
			else
			{
				listOfOtherObjects.Add(obj);
			}
		}

		m_targetObjects.Clear();
		ChangeNamesForGameObjects(listOfGameObjects.ToArray());
		ChangeNamesForObjects(listOfOtherObjects.ToArray());
	}

	private void FindNamesForAnimationKeys() //Find keys in Animations 
	{
		m_targetObjects.Clear();
		var objectGuids = AssetDatabase.FindAssets("t:AnimationClip");
		for (int i = 0; i < objectGuids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(objectGuids[i]);
			AnimationClip objAnim = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
			var path = AssetDatabase.GetAssetPath(objAnim);
			if (Path.GetExtension(path) == ".FBX")
			{
				continue;
			}

			foreach (var binding in AnimationUtility.GetCurveBindings(objAnim))
			{
				if (binding.path != GetCorrectName(binding.path, true))
				{
					m_targetObjects.Add(path, objAnim);
					break;
				}
			}
		}
	}

	private void OnGUI()
	{
		m_mode = (Mode) EditorGUILayout.EnumPopup(m_mode);

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Find All Uncorrect Names")) //Find all names
		{
			if (m_mode == Mode.OnScene) //Find GameObject on Scene
			{
				FindNamesForGameObjectsOnScene();
			}

			if (m_mode == Mode.InResources) //Find GameObjects and Objects in Resources
			{
				FindNamesForResources();
			}

			if (m_mode == Mode.InAnimKeys) //Find keys in Animations 
			{
				FindNamesForAnimationKeys();
			}
			m_sortedObjectsNames = m_targetObjects.Keys.ToList();
			m_sortedObjectsNames.Sort();
		}

		if (GUILayout.Button("Change All Names")) // Change all names
		{
			if (m_mode == Mode.OnScene) // Change GameObject on Scene
			{
				RenameAllObjects();
			}
			if (m_mode == Mode.InResources) //Change GameObjects and Objects in Resources
			{
				RenameAllObjects();
			}
			if (m_mode == Mode.InAnimKeys) //Change keys in Animations 
			{
				//RenameAllObjects();
				foreach (var objPath in m_sortedObjectsNames)
				{
					ChangeNamesForAnimationKeys(m_targetObjects[objPath] as AnimationClip);
				}
			}

			m_sortedObjectsNames.Clear();
			m_targetObjects.Clear();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		m_askPrefabChanges = EditorGUILayout.Toggle("AskPrefabChanges", m_askPrefabChanges);
		m_applyToPrefab = EditorGUILayout.Toggle("ApplyToPrefab", m_applyToPrefab);
		GUILayout.EndHorizontal();

		Object newSelected = null;
		var oldAligment = GUI.skin.button.alignment;
		var oldColor = GUI.color;
		GUI.skin.button.alignment = TextAnchor.MiddleLeft;
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); //, GUILayout.Width(600), GUILayout.Height(800)

		for (int i = 0; i < m_sortedObjectsNames.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(m_sortedObjectsNames[i]))
			{
				newSelected = m_targetObjects[m_sortedObjectsNames[i]];
				//foreach (var binding in AnimationUtility.GetCurveBindings(newSelected as AnimationClip))
				//{
				//	var allPath = string.Concat(AssetDatabase.GetAssetPath(newSelected), "/", binding.path);
				//	if (allPath != GetCorrectName(allPath, false))
				//	{
				//		Debug.Log(allPath + "/" + binding.propertyName);
				//	}
				//}
			}

			if (GUILayout.Button("Fix name"))
			{
				var obj = m_targetObjects[m_sortedObjectsNames[i]];
				if (m_mode == Mode.InAnimKeys)
				{
					Debug.Log("Changed keys in anim " + obj, obj);
					ChangeNamesForAnimationKeys(obj as AnimationClip);
				}
				else
				{
					SetCorrectName(obj);
					Debug.Log("Changed name for object " + obj, obj);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
				m_targetObjects.Remove(m_sortedObjectsNames[i]);
				m_sortedObjectsNames.RemoveAt(i);
				--i;
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

	private void RenameAllObjects()
	{
		m_prefabsForApply.Clear();
		foreach (var objPath in m_sortedObjectsNames)
		{
			SetCorrectName(m_targetObjects[objPath], true);
		}

		ApplyCollectedPrefabs(m_prefabsForApply);
	}

	private void ApplyCollectedPrefabs(Dictionary<GameObject, Object> prefabsForApply)
	{
		foreach (var pairPrefabObjRoot in prefabsForApply)
		{
			var prefabRoot = pairPrefabObjRoot.Key;
			var prefabParent = pairPrefabObjRoot.Value;
			PrefabUtility.ReplacePrefab(prefabRoot, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
			Debug.Log("Prefab applied " + prefabParent, prefabParent);
		}
	}

	private void SetCorrectName(Object forObject, bool onlyCollectPrefabs = false)
	{
		forObject.name = GetCorrectName(forObject);
		if (AssetDatabase.IsMainAsset(forObject))
		{
			string path = AssetDatabase.GetAssetOrScenePath(forObject);
			string newName = GetCorrectName(path);
			var error = AssetDatabase.RenameAsset(path, newName);
			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogError(error);
			}
		}
		UnityEditor.EditorUtility.SetDirty(forObject);
		var forGameObject = forObject as GameObject;
		if (forGameObject == null)
		{
			return;
		}

		UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(forGameObject.scene);
		if (!m_applyToPrefab)
		{
			return;
		}

		var prefabRoot = PrefabUtility.FindPrefabRoot(forGameObject);
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
			if (!EditorUtility.DisplayDialog("Apply prefab changes?", "Prefab found for " + forGameObject, "Yes", "No"))
			{
				Debug.Log("Prefab found for " + forGameObject, forGameObject);
				return;
			}
		}

		if (onlyCollectPrefabs)
		{
			if (!m_prefabsForApply.ContainsKey(prefabRoot))
			{
				m_prefabsForApply.Add(prefabRoot, prefabParent);
			}
		}
		else
		{
			PrefabUtility.ReplacePrefab(prefabRoot, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
			Debug.Log("Prefab applied " + prefabParent, prefabParent);
		}
	}

	private string GetCorrectName(Object forObject) //Function about get new correct names
	{
		var s = forObject.name;
		return GetCorrectName(s, true);
	}

	private string GetCorrectName(string path, bool onlyName = true,
											bool withDebug = false) //Function about get new correct names
	{
		char[] separators = {'_', ' '};
		string partName = null;
		if (onlyName)
		{
			partName = Path.GetFileNameWithoutExtension(path);
		}
		else
		{
			partName = path;
		}
		if (withDebug)
		{
			Debug.Log("partName=" + partName);
		}
		string[] partSlash = partName.Split('/');
		for (int j = 0; j < partSlash.Length; j++)
		{
			string slash = partSlash[j];
			if (withDebug)
			{
				Debug.Log(j + ") " + slash + " =>");
			}
			string[] partOfString = slash.Split(separators);
			for (int i = 0; i < partOfString.Length; ++i)
			{
				string part = partOfString[i];
				if ((part == part.ToUpper()) || (part == part.ToLower()))
				{
					if (part.Length == 0)
					{
						continue;
					}

					char[] chars = part.ToLower().ToCharArray();
					chars[0] = char.ToUpper(chars[0]);
					var newPart = new string(chars);
					if (withDebug)
					{
						Debug.Log(j + "." + i + ") " + part + " => " + newPart);
					}
					partOfString[i] = newPart;
				}
				else
				{
					if (withDebug)
					{
						Debug.Log(j + "." + i + ") " + part + "; Not change");
					}
				}
			}

			slash = string.Join(string.Empty, partOfString);
			if (withDebug)
			{
				Debug.Log(j + ") " + slash + " <=");
			}
			partSlash[j] = slash;
		}

		partName = string.Join("/", partSlash);

		return partName;
	}

	private string GetGameObjectPath(GameObject obj) // Return path of objects
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
	private Vector2 m_scrollPos = Vector2.zero;

	private Dictionary<string, Object> m_targetObjects = new Dictionary<string, Object>();
	private List<string> m_sortedObjectsNames = new List<string>();
	private Dictionary<GameObject, Object> m_prefabsForApply = new Dictionary<GameObject, Object>();

	private bool m_askPrefabChanges = true;
	private bool m_applyToPrefab = true;
	private Mode m_mode = Mode.OnScene;
}